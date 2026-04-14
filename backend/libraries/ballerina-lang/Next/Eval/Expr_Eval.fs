namespace Ballerina.DSL.Next.Terms

[<AutoOpen>]
module Eval =
  open Ballerina
  open Ballerina.Collections.Sum
  open Ballerina.Collections.Map
  open Ballerina.Coroutines.Model
  open Ballerina.Reader.WithError
  open Ballerina.Errors
  open Ballerina.LocalizedErrors
  open Ballerina.Errors
  open Ballerina.StdLib.String
  open Ballerina.StdLib.Object
  open System
  open Ballerina.DSL.Next.Terms.Patterns
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Types.Patterns
  open Ballerina.DSL.Next.Types.TypeChecker.Model
  open Ballerina
  open Ballerina.Cat.Collections.OrderedMap
  open Ballerina.Collections.NonEmptyList

  type ExprEvalContextSymbols =
    { Types: Map<ResolvedIdentifier, TypeSymbol>
      RecordFields: Map<ResolvedIdentifier, TypeSymbol>
      UnionCases: Map<ResolvedIdentifier, TypeSymbol> }

    static member Empty =
      { Types = Map.empty
        RecordFields = Map.empty
        UnionCases = Map.empty }

    static member FromTypeChecker(ctx: TypeExprEvalSymbols) =
      { Types = ctx.Types
        RecordFields = ctx.RecordFields
        UnionCases = ctx.UnionCases }

    static member Append
      (s1: ExprEvalContextSymbols)
      (s2: ExprEvalContextSymbols)
      =
      { Types = Map.fold (fun acc k v -> Map.add k v acc) s1.Types s2.Types
        RecordFields =
          Map.fold
            (fun acc k v -> Map.add k v acc)
            s1.RecordFields
            s2.RecordFields
        UnionCases =
          Map.fold (fun acc k v -> Map.add k v acc) s1.UnionCases s2.UnionCases }

  type ExprEvalContextScope<'valueExtension> =
    { Values:
        Map<
          ResolvedIdentifier,
          Value<TypeValue<'valueExtension>, 'valueExtension>
         >
      Symbols: ExprEvalContextSymbols }

  type ExprEvalContext<'runtimeContext, 'valueExtension> =
    { Scope: ExprEvalContextScope<'valueExtension>
      ExtensionOps: ValueExtensionOps<'runtimeContext, 'valueExtension>
      RuntimeContext: 'runtimeContext
      RootLevelEval: bool }

  and ApplicableExtEvalResult<'runtimeContext, 'valueExtension> =
    (Location
      -> List<RunnableExpr<'valueExtension>>
      -> 'valueExtension
      -> Value<TypeValue<'valueExtension>, 'valueExtension>
      -> ExprEvaluator<
        'runtimeContext,
        'valueExtension,
        Value<TypeValue<'valueExtension>, 'valueExtension>
       >)

  and ExtEvalResult<'runtimeContext, 'valueExtension> =
    | Result of Value<TypeValue<'valueExtension>, 'valueExtension>
    | Async of
      Coroutine<
        ExtEvalResult<'runtimeContext, 'valueExtension>,
        Unit,
        Unit,
        Unit,
        Errors<Location>
       >
    | Applicable of
      (Value<TypeValue<'valueExtension>, 'valueExtension>
        -> ExprEvaluator<
          'runtimeContext,
          'valueExtension,
          Value<TypeValue<'valueExtension>, 'valueExtension>
         >)
    | TypeApplicable of
      (TypeValue<'valueExtension>
        -> ExprEvaluator<
          'runtimeContext,
          'valueExtension,
          Value<TypeValue<'valueExtension>, 'valueExtension>
         >)
    | Matchable of
      (Map<ResolvedIdentifier, RunnableCaseHandler<'valueExtension>>
        -> ExprEvaluator<
          'runtimeContext,
          'valueExtension,
          Value<TypeValue<'valueExtension>, 'valueExtension>
         >)

  and ExtensionEvaluator<'runtimeContext, 'valueExtension> =
    Location
      -> List<RunnableExpr<'valueExtension>>
      -> 'valueExtension
      -> ExprEvaluator<
        'runtimeContext,
        'valueExtension,
        ExtEvalResult<'runtimeContext, 'valueExtension>
       >

  and ValueExtensionOps<'runtimeContext, 'valueExtension> =
    { Eval: ExtensionEvaluator<'runtimeContext, 'valueExtension>
      Applicables:
        Map<
          ResolvedIdentifier,
          ApplicableExtEvalResult<'runtimeContext, 'valueExtension>
         > }

  and ExprEvaluator<'runtimeContext, 'valueExtension, 'res> =
    Reader<
      'res,
      ExprEvalContext<'runtimeContext, 'valueExtension>,
      Errors<Location>
     >

  type ExprEvalContext<'runtimeContext, 'valueExtension> with
    static member Empty
      : 'runtimeContext -> ExprEvalContext<'runtimeContext, 'valueExtension> =
      fun runtimeContext ->
        { RootLevelEval = true
          RuntimeContext = runtimeContext
          Scope =
            { Values = Map.empty
              Symbols = ExprEvalContextSymbols.Empty }
          ExtensionOps =
            { Eval =
                fun loc0 _ _ ->
                  (fun () -> $"Error: cannot evaluate empty extension")
                  |> Errors.Singleton loc0
                  |> reader.Throw
              Applicables = Map.empty } }

    static member WithTypeCheckingSymbols<'valueExtension>
      (ctx: ExprEvalContext<'runtimeContext, 'valueExtension>)
      (symbols: TypeExprEvalSymbols)
      : ExprEvalContext<'runtimeContext, 'valueExtension> =
      { ctx with
          Scope =
            { ctx.Scope with
                Symbols =
                  ExprEvalContextSymbols.Append
                    ctx.Scope.Symbols
                    (ExprEvalContextSymbols.FromTypeChecker symbols) } }

    static member Getters =
      {| Values =
          fun (c: ExprEvalContext<'runtimeContext, 'valueExtension>) ->
            c.Scope.Values
         ExtensionOps =
          fun (c: ExprEvalContext<'runtimeContext, 'valueExtension>) ->
            c.ExtensionOps
         Symbols =
          fun (c: ExprEvalContext<'runtimeContext, 'valueExtension>) ->
            c.Scope.Symbols |}

    static member Updaters =
      {| Values =
          fun u (c: ExprEvalContext<'runtimeContext, 'valueExtension>) ->
            { c with
                Scope =
                  { c.Scope with
                      Values = u (c.Scope.Values) } }
         ExtensionOps =
          fun u (c: ExprEvalContext<'runtimeContext, 'valueExtension>) ->
            { c with
                ExtensionOps = u (c.ExtensionOps) }
         Symbols =
          fun u (c: ExprEvalContext<'runtimeContext, 'valueExtension>) ->
            { c with
                Scope =
                  { c.Scope with
                      Symbols = u (c.Scope.Symbols) } }
         RootLevelEval =
          fun u (c: ExprEvalContext<'runtimeContext, 'valueExtension>) ->
            { c with
                RootLevelEval = u (c.RootLevelEval) } |}

  type Expr<'T, 'Id, 'valueExt when 'Id: comparison> with
    static member EvalApply (loc0: Location) (rest: List<_>) (fV, argV) =
      reader {
        let! fVVar, fvBody, closure, _scope =
          fV
          |> Value.AsLambda
          |> sum.MapError(Errors.MapContext(replaceWith loc0))
          |> reader.OfSum

        return!
          reader {
            let closure =
              closure
              |> Map.add
                (fVVar.Name
                 |> Identifier.LocalScope
                 |> TypeCheckScope.Empty.Resolve)
                argV

            let! res =
              NonEmptyList.OfList(fvBody, rest)
              |> Expr.Eval
              |> reader.MapContext(
                ExprEvalContext.Updaters.Values(Map.merge (fun _ -> id) closure)
              )
              |> reader.Catch

            match res with
            | Left res -> return res
            | Right err ->
              // do Console.WriteLine($"Warning: error during function application {fV} {argV} ({fvBody})")
              // do Console.ReadLine() |> ignore
              // do closure |> Map.iter (fun k v -> Console.WriteLine($"  {k} = {v}"))
              // do Console.ReadLine() |> ignore
              return! err |> reader.Throw
          }
          |> reader.MapError(
            Errors<Location>.MapPriority(replaceWith ErrorPriority.High)
          )
      }

    // NOTE: expressions are concatenated in the order of the input (the returned value is of the type of the last expression)
    static member Eval<'runtimeContext, 'valueExtension>
      (NonEmptyList(e, rest): NonEmptyList<RunnableExpr<'valueExtension>>)
      : ExprEvaluator<
          'runtimeContext,
          'valueExtension,
          Value<TypeValue<'valueExtension>, 'valueExtension>
         >
      =
      let (!) = NonEmptyList.One >> Expr.Eval<'runtimeContext, 'valueExtension>

      let (!!) =
        fun e ->
          Expr.Eval<'runtimeContext, 'valueExtension>(NonEmptyList(e, rest))

      let loc0 = e.Location

      reader {
        match e.Expr with
        | RunnableExprRec.Primitive PrimitiveValue.Unit ->

          match rest with
          | [] -> return Value.Primitive PrimitiveValue.Unit
          | p :: rest -> return! Expr.Eval(NonEmptyList.OfList(p, rest))
        | RunnableExprRec.Primitive v -> return Value.Primitive v
        | RunnableExprRec.If { RunnableExprIf.Cond = cond
                               RunnableExprIf.Then = thenBody
                               RunnableExprIf.Else = elseBody } ->
          let! condV = !cond

          match condV with
          | Value.Primitive(PrimitiveValue.Bool true) -> return! !!thenBody
          | Value.Primitive(PrimitiveValue.Bool false) -> return! !!elseBody
          | v ->
            return!
              (fun () -> $"expected boolean in if condition, got {v}")
              |> Errors.Singleton loc0
              |> reader.Throw
        | RunnableExprRec.Let { RunnableExprLet.Var = var
                                RunnableExprLet.Type = _varType
                                RunnableExprLet.Val = valueExpr
                                RunnableExprLet.Rest = body } ->
          let! value = !valueExpr

          return!
            !!body
            |> reader.MapContext(
              ExprEvalContext.Updaters.Values(
                Map.add
                  (var.Name |> Identifier.LocalScope |> e.Scope.Resolve)
                  value
              )
            )
        | RunnableExprRec.Do { Val = e1; Rest = e2 } ->
          let! _ = !e1
          return! !!e2
        | RunnableExprRec.Lookup({ Id = id }) ->
          let! ctx = reader.GetContext()

          let! res =
            ctx.Scope.Values
            |> Map.tryFindWithError
              id
              "variables"
              (fun () -> id.AsFSharpString)
              loc0
            |> reader.OfSum
            |> reader.Catch

          match res with
          | Left v -> return v
          | Right err -> return! err |> reader.Throw
        | RunnableExprRec.RecordCons { Fields = fields } ->
          // let! ctx = reader.GetContext()

          let! fields =
            fields
            |> List.map (fun (id, field) ->
              reader {
                let! v = !field

                return id, v
              })
            |> reader.All
            |> reader.Map Map.ofList

          return Value.Record(fields)
        | RunnableExprRec.RecordWith({ Record = record; Fields = fields }) ->
          let! recordV = !record

          let! recordV =
            recordV
            |> Value.AsRecord
            |> sum.MapError(Errors.MapContext(replaceWith loc0))
            |> reader.OfSum

          // let! ctx = reader.GetContext()

          let! fields =
            fields
            |> List.map (fun (id, field) ->
              reader {
                let! v = !field
                return id, v
              })
            |> reader.All
            |> reader.Map Map.ofList

          let fields = Map.fold (fun acc k v -> Map.add k v acc) recordV fields

          return Value.Record(fields)
        | RunnableExprRec.RecordDes({ RunnableExprRecordDes.Expr = recordExpr
                                      RunnableExprRecordDes.Field = fieldId }) ->
          let! recordV = !recordExpr

          let! recordV =
            recordV
            |> Value.AsRecord
            |> sum.MapError(Errors.MapContext(replaceWith loc0))
            |> reader.OfSum

          // let! ctx = reader.GetContext()

          // let! fieldId =
          //   ctx.Symbols.RecordFields
          //   |> Map.tryFindWithError fieldId "record field id" (fieldId.ToFSharpString) loc0
          //   |> reader.OfSum

          return!
            recordV
            |> Map.tryFindWithError
              fieldId
              "record field"
              (fun () -> fieldId.AsFSharpString)
              loc0
            |> reader.OfSum

        | RunnableExprRec.TupleCons { Items = fields } ->
          let! fields = fields |> List.map (!) |> reader.All

          return Value.Tuple(fields)
        | RunnableExprRec.TupleDes { RunnableExprTupleDes.Tuple = recordExpr
                                     RunnableExprTupleDes.Item = fieldId } ->
          let! recordV = !recordExpr

          let! recordV =
            recordV
            |> Value.AsTuple
            |> sum.MapError(Errors.MapContext(replaceWith loc0))
            |> reader.OfSum

          return!
            recordV
            |> List.tryItem (fieldId.Index - 1)
            |> sum.OfOption(
              (fun () ->
                $"Error: tuple index {fieldId.Index} out of bounds, size {List.length recordV}")
              |> Errors.Singleton loc0
            )
            |> reader.OfSum
        | RunnableExprRec.SumCons({ Selector = selector }) ->
          let t_unit = TypeValue.CreateUnit()
          let k_star = Kind.Star

          return
            Value.Lambda(
              Var.Create "x",
              RunnableExpr.Apply(
                RunnableExpr.SumCons(selector, t_unit, k_star),
                RunnableExpr.Lookup(
                  "x" |> Identifier.LocalScope |> e.Scope.Resolve,
                  t_unit,
                  k_star
                ),
                t_unit,
                k_star
              ),
              Map.empty,
              e.Scope
            )
        | RunnableExprRec.Apply({ RunnableExprApply.F = { Expr = RunnableExprRec.SumCons selector }
                                  RunnableExprApply.Arg = valueE }) ->
          let! valueV = !valueE
          return Value.Sum(selector.Selector, valueV)
        | RunnableExprRec.Apply({ RunnableExprApply.F = { RunnableExpr.Expr = RunnableExprRec.UnionDes({ RunnableExprUnionDes.Handlers = cases; RunnableExprUnionDes.Fallback = fallback }) }
                                  RunnableExprApply.Arg = unionE }) ->
          let! unionV = !unionE

          return!
            reader.Any2
              (reader {
                let! unionVCase, unionV =
                  unionV
                  |> Value.AsUnion
                  |> sum.MapError(Errors.MapContext(replaceWith loc0))
                  |> reader.OfSum

                return!
                  reader {

                    let! caseHandler =
                      cases
                      |> Map.tryFindWithError
                        unionVCase
                        "union case"
                        (fun () -> unionVCase.AsFSharpString)
                        loc0
                      |> reader.OfSum
                      |> reader.Catch
                      |> reader.Map(Sum.toOption)

                    match caseHandler with
                    | Some caseHandler ->
                      let caseVar, caseBody = caseHandler

                      let add_var =
                        match caseVar with
                        | None -> id
                        | Some caseVar ->
                          Map.add
                            (caseVar.Name
                             |> Identifier.LocalScope
                             |> e.Scope.Resolve)
                            unionV

                      return!
                        !!caseBody
                        |> reader.MapContext(
                          ExprEvalContext.Updaters.Values add_var
                        )
                    | None ->
                      match fallback with
                      | Some fallback -> return! !fallback
                      | None ->
                        return!
                          (fun () ->
                            $"Error: cannot find case handler for union case. Cases = {cases.Keys.AsFSharpString}. Case = {unionVCase.AsFSharpString}.")
                          |> Errors.Singleton loc0
                          |> reader.Throw
                  }
                  |> reader.MapError(
                    Errors.MapPriority(replaceWith ErrorPriority.High)
                  )
              })
              (reader {
                let! unionV, _ =
                  unionV
                  |> Value.AsExt
                  |> sum.MapError(Errors.MapContext(replaceWith loc0))
                  |> reader.OfSum

                return!
                  reader {
                    let! ctx = reader.GetContext()
                    let unionV = ctx.ExtensionOps.Eval loc0 rest unionV

                    match! unionV with
                    | Matchable f -> return! f cases
                    | _ ->
                      return!
                        (fun () ->
                          "Expected an applicable or matchable extension function")
                        |> Errors.Singleton loc0
                        |> reader.Throw
                  }
                  |> reader.MapError(
                    Errors.MapPriority(replaceWith ErrorPriority.High)
                  )
              })
            |> reader.MapError(Errors<Location>.FilterHighestPriorityOnly)

        | RunnableExprRec.Apply({ RunnableExprApply.F = { Expr = RunnableExprRec.SumDes cases }
                                  RunnableExprApply.Arg = sumE }) ->
          let! sumV = !sumE

          let! sumVCase, sumV =
            sumV
            |> Value.AsSum
            |> sum.MapError(Errors.MapContext(replaceWith loc0))
            |> reader.OfSum

          let! caseHandler =
            cases.Handlers
            |> Map.tryFindWithError
              sumVCase
              "sum case"
              (fun () -> sumVCase.AsFSharpString)
              loc0
            |> reader.OfSum

          let caseVar, caseBody = caseHandler

          let add_var =
            match caseVar with
            | None -> id
            | Some caseVar ->
              Map.add
                (caseVar.Name |> Identifier.LocalScope |> e.Scope.Resolve)
                sumV

          return!
            !!caseBody
            |> reader.MapContext(ExprEvalContext.Updaters.Values(add_var))

        | RunnableExprRec.FromValue({ RunnableExprFromValue.Value = v
                                      RunnableExprFromValue.ValueType = _
                                      RunnableExprFromValue.ValueKind = _ }) -> return v

        | RunnableExprRec.Apply({ F = f; Arg = argE }) ->
          let! fV = !f
          let! argV = !argE

          return!
            reader.Any(
              reader { return! Expr.EvalApply loc0 rest (fV, argV) },
              [ reader {
                  let! fUnionCons =
                    fV
                    |> Value.AsUnionCons
                    |> sum.MapError(Errors.MapContext(replaceWith loc0))
                    |> reader.OfSum

                  return Value.UnionCase(fUnionCons, argV)
                }
                reader {
                  let! fRecord =
                    fV
                    |> Value.AsRecord
                    |> sum.MapError(Errors.MapContext(replaceWith loc0))
                    |> reader.OfSum

                  return!
                    reader {
                      let! fRecordCons =
                        fRecord
                        |> Map.tryFindWithError
                          ("cons" |> Identifier.LocalScope |> e.Scope.Resolve)
                          "record field"
                          (fun () -> "cons")
                          loc0
                        |> reader.OfSum

                      let! fUnionCons =
                        fRecordCons
                        |> Value.AsUnionCons
                        |> sum.MapError(Errors.MapContext(replaceWith loc0))
                        |> reader.OfSum

                      return Value.UnionCase(fUnionCons, argV)
                    }
                    |> reader.MapError(
                      Errors.MapPriority(replaceWith ErrorPriority.High)
                    )
                }
                reader {
                  let! fieldId =
                    fV
                    |> Value.AsRecordDes
                    |> sum.MapError(Errors.MapContext(replaceWith loc0))
                    |> reader.OfSum

                  return!
                    reader {
                      // let fieldId = fieldId.Name
                      let! recordV =
                        argV
                        |> Value.AsRecord
                        |> sum.MapError(Errors.MapContext(replaceWith loc0))
                        |> reader.OfSum

                      return!
                        recordV
                        |> Map.tryFindWithError
                          fieldId
                          "record field"
                          (fun () -> fieldId.ToString())
                          loc0
                        |> reader.OfSum
                        |> reader.MapError(
                          Errors.Map(String.append $" in record {argV}")
                        )
                    }
                    |> reader.MapError(
                      Errors.MapPriority(replaceWith ErrorPriority.High)
                    )
                }
                reader {
                  let! fExt, app_id =
                    fV
                    |> Value.AsExt
                    |> sum.MapError(Errors.MapContext(replaceWith loc0))
                    |> reader.OfSum

                  return!
                    reader {
                      let! ctx = reader.GetContext()

                      match app_id with
                      | Some app_id ->

                        let f =
                          ctx.ExtensionOps.Applicables |> Map.tryFind app_id

                        match f with
                        | Some f -> return! f loc0 rest fExt argV
                        | None ->
                          let! fExt = ctx.ExtensionOps.Eval loc0 rest fExt

                          match fExt with
                          | Applicable f -> return! f argV
                          | _ ->
                            return!
                              (fun () -> $"Cannot apply {fExt}")
                              |> Errors.Singleton loc0
                              |> reader.Throw
                      | None ->
                        let! fExt = ctx.ExtensionOps.Eval loc0 rest fExt

                        match fExt with
                        | Applicable f -> return! f argV
                        | _ ->
                          return!
                            (fun () -> $"Cannot apply {fExt}")
                            |> Errors.Singleton loc0
                            |> reader.Throw

                    }
                    |> reader.MapError(
                      Errors.MapPriority(replaceWith ErrorPriority.High)
                    )
                } ]
            )
            |> reader.MapError(Errors<Location>.FilterHighestPriorityOnly)

        | RunnableExprRec.Lambda { RunnableExprLambda.Param = var
                                   RunnableExprLambda.ParamType = _
                                   RunnableExprLambda.Body = body } ->
          let! context = reader.GetContext()
          return Value.Lambda(var, body, context.Scope.Values, e.Scope)
        | RunnableExprRec.TypeLambda({ Param = _tp; Body = body }) ->
          return! !body
        | RunnableExprRec.TypeLet({ RunnableExprTypeLet.Name = typeName
                                    RunnableExprTypeLet.TypeDef = typeDefinition
                                    RunnableExprTypeLet.Body = body }) ->

          let scope =
            e.Scope |> TypeCheckScope.Updaters.Type(replaceWith (Some typeName))

          let bind_component
            (n, v)
            : Updater<
                Map<
                  ResolvedIdentifier,
                  Value<TypeValue<'valueExtension>, 'valueExtension>
                 >
               >
            =
            Map.add n v

          let! definition_as_union =
            typeDefinition
            |> TypeValue.AsUnion
            |> sum.MapError(Errors.MapContext(replaceWith loc0))
            |> reader.OfSum
            |> reader.Catch
            |> reader.Map(Sum.toOption)
          // |> reader.Map(Option.map WithTypeExprSourceMapping.Getters.Value)

          let! bind_definition_cases =
            definition_as_union
            |> Option.map (fun (_, definition_cases) ->
              definition_cases
              |> OrderedMap.toSeq
              |> Seq.map (fun (k, _) ->
                reader {
                  return
                    bind_component (
                      k.Name |> scope.Resolve,
                      Value.UnionCons(k.Name |> scope.Resolve)
                    )
                    >> bind_component (
                      k.Name.LocalName
                      |> Identifier.LocalScope
                      |> TypeCheckScope.Empty.Resolve,
                      Value.UnionCons(k.Name |> scope.Resolve)
                    )
                })
              |> reader.All)
            |> reader.RunOption
            |> reader.Map(
              Option.map (List.fold (>>) id) >> Option.defaultValue id
            )

          let! definition_fields =
            typeDefinition
            |> TypeValue.AsRecord
            |> sum.MapError(Errors.MapContext(replaceWith loc0))
            |> reader.OfSum
            |> reader.Catch
            |> reader.Map(Sum.toOption)

          let! bind_definition_fields =
            definition_fields
            |> Option.map (fun definition_fields ->
              definition_fields
              |> OrderedMap.toSeq
              |> Seq.map (fun (k, _) ->
                reader {
                  return
                    bind_component (
                      k.Name |> scope.Resolve,
                      Value.RecordDes(k.Name |> scope.Resolve)
                    )
                    >> bind_component (
                      k.Name.LocalName
                      |> Identifier.LocalScope
                      |> TypeCheckScope.Empty.Resolve,
                      Value.RecordDes(k.Name |> scope.Resolve)
                    )
                })
              |> reader.All)
            |> reader.RunOption
            |> reader.Map(
              Option.map (List.fold (>>) id) >> Option.defaultValue id
            )

          return!
            !!body
            |> reader.MapContext(
              ExprEvalContext.Updaters.Values(
                bind_definition_cases >> bind_definition_fields
              )
            )
        | RunnableExprRec.TypeApply({ RunnableExprTypeApply.Func = typeLambda
                                      RunnableExprTypeApply.TypeArg = typeArg }) ->
          return!
            reader.Any(
              // Note: ordering matters here, the most specific branch needs to be evaluated first
              reader {
                let! typeLambda = !typeLambda

                let! ext, _ =
                  typeLambda
                  |> Value.AsExt
                  |> sum.MapError(Errors.MapContext(replaceWith loc0))
                  |> reader.OfSum

                let! ctx = reader.GetContext()
                let! ext = ctx.ExtensionOps.Eval loc0 rest ext

                match ext with
                | TypeApplicable f -> return! f typeArg
                | _ ->
                  return!
                    (fun () ->
                      $"Expected extension to be type applicable, got {ext}")
                    |> Errors.Singleton loc0
                    |> reader.Throw
              },
              // this says we do not care about the type info
              [ !!typeLambda ]
            )

        | RunnableExprRec.EntitiesDes({ Expr = s }) ->
          let! s_v = !s

          let! s_v =
            s_v
            |> Value.AsRecord
            |> sum.MapError(Errors.MapContext(replaceWith loc0))
            |> reader.OfSum

          let! entities_v =
            s_v
            |> Map.tryFindWithError
              ("Entities"
               |> Identifier.LocalScope
               |> TypeCheckScope.Empty.Resolve)
              "entities schema field"
              (fun () -> "Entities")
              loc0
            |> reader.OfSum

          return entities_v
        | RunnableExprRec.RelationsDes({ Expr = s }) ->
          let! s_v = !s

          let! s_v =
            s_v
            |> Value.AsRecord
            |> sum.MapError(Errors.MapContext(replaceWith loc0))
            |> reader.OfSum

          let! relations_v =
            s_v
            |> Map.tryFindWithError
              ("Relations"
               |> Identifier.LocalScope
               |> TypeCheckScope.Empty.Resolve)
              "relations schema field"
              (fun () -> "Relations")
              loc0
            |> reader.OfSum

          return relations_v
        | RunnableExprRec.EntityDes({ Expr = s; EntityName = entityName }) ->
          let! s_v = !s

          let! s_v =
            s_v
            |> Value.AsRecord
            |> sum.MapError(Errors.MapContext(replaceWith loc0))
            |> reader.OfSum

          let! entity_v =
            s_v
            |> Map.tryFindWithError
              (entityName.Name
               |> Identifier.LocalScope
               |> TypeCheckScope.Empty.Resolve)
              "entity schema field"
              (fun () -> entityName.Name)
              loc0
            |> reader.OfSum

          return entity_v
        | RunnableExprRec.RelationDes({ RunnableExprRelationDes.Expr = s
                                        RunnableExprRelationDes.RelationName = relationName }) ->
          let! s_v = !s

          let! s_v =
            s_v
            |> Value.AsRecord
            |> sum.MapError(Errors.MapContext(replaceWith loc0))
            |> reader.OfSum

          let! relation_v =
            s_v
            |> Map.tryFindWithError
              (relationName.Name
               |> Identifier.LocalScope
               |> TypeCheckScope.Empty.Resolve)
              "relation schema field"
              (fun () -> relationName.Name)
              loc0
            |> reader.OfSum

          return relation_v
        | RunnableExprRec.RelationLookupDes({ RunnableExprRelationLookupDes.Expr = record_v
                                              RunnableExprRelationLookupDes.RelationName = _relation_name
                                              RunnableExprRelationLookupDes.Direction = direction }) ->
          let! record_v = !record_v

          let! record_v =
            record_v
            |> Value.AsRecord
            |> sum.MapError(Errors.MapContext(replaceWith loc0))
            |> reader.OfSum

          let! relation_v =
            record_v
            |> Map.tryFindWithError
              ((match direction with
                | FromTo -> "From"
                | _ -> "To")
               |> Identifier.LocalScope
               |> TypeCheckScope.Empty.Resolve)
              "relation schema field From/To"
              (fun () -> direction.AsFSharpString)
              loc0
            |> reader.OfSum

          return relation_v
        | RunnableExprRec.Query(RunnableExprQuery.UnionQueries(q1, q2)) ->
          let! v1 = !(RunnableExpr.Query(q1, e.Type, e.Kind))
          let! v2 = !(RunnableExpr.Query(q2, e.Type, e.Kind))

          let! v1 =
            v1
            |> Value.AsQuery
            |> sum.MapError(Errors.MapContext(replaceWith loc0))
            |> reader.OfSum

          let! v2 =
            v2
            |> Value.AsQuery
            |> sum.MapError(Errors.MapContext(replaceWith loc0))
            |> reader.OfSum

          return
            Value.Query(
              ValueQuery.ValueUnionQueries(v1, v2, v1.DeserializeFrom)
            )
        | RunnableExprRec.Query q ->

          let rec replace_closure_lookups
            closure
            (e: RunnableExprQueryExpr<_>)
            =
            reader {
              let (!) = replace_closure_lookups closure

              match e.Expr with
              | RunnableExprQueryExprRec.QueryConstant _
              | RunnableExprQueryExprRec.QueryIntrinsic(_, _) -> return e
              | RunnableExprQueryExprRec.QueryTupleCons items ->
                let! items = items |> List.map (!) |> reader.All

                return
                  { e with
                      Expr = RunnableExprQueryExprRec.QueryTupleCons items }
              | RunnableExprQueryExprRec.QueryRecordDes(expr, field, isJson) ->
                let! expr = !expr

                return
                  { e with
                      Expr =
                        RunnableExprQueryExprRec.QueryRecordDes(
                          expr,
                          field,
                          isJson
                        ) }
              | RunnableExprQueryExprRec.QueryTupleDes(expr, item, isJson) ->
                let! expr = !expr

                return
                  { e with
                      Expr =
                        RunnableExprQueryExprRec.QueryTupleDes(
                          expr,
                          item,
                          isJson
                        ) }
              | RunnableExprQueryExprRec.QueryConditional(cond,
                                                             ``then``,
                                                             ``else``) ->
                let! cond = !cond
                let! ``then`` = !``then``
                let! ``else`` = !``else``

                return
                  { e with
                      Expr =
                        RunnableExprQueryExprRec.QueryConditional(
                          cond,
                          ``then``,
                          ``else``
                        ) }
              | RunnableExprQueryExprRec.QueryUnionDes(expr, handlers) ->
                let! expr = !expr

                let! handlers =
                  handlers
                  |> Map.map (fun _k handler ->
                    reader {
                      let! handlerBody = !handler.Body

                      return { handler with Body = handlerBody }
                    })
                  |> reader.AllMap

                return
                  { e with
                      Expr =
                        RunnableExprQueryExprRec.QueryUnionDes(
                          expr,
                          handlers
                        ) }
              | RunnableExprQueryExprRec.QuerySumDes(expr, handlers) ->
                let! expr = !expr

                let! handlers =
                  handlers
                  |> Map.map (fun _k handler ->
                    reader {
                      let! handlerBody = !handler.Body

                      return { handler with Body = handlerBody }
                    })
                  |> reader.AllMap

                return
                  { e with
                      Expr =
                        RunnableExprQueryExprRec.QuerySumDes(expr, handlers) }
              | RunnableExprQueryExprRec.QueryApply(func, arg) ->
                let! func = !func
                let! arg = !arg

                return
                  { e with
                      Expr = RunnableExprQueryExprRec.QueryApply(func, arg) }
              | RunnableExprQueryExprRec.QueryLookup(l) ->
                match closure |> Map.tryFind l with
                | None -> return e
                | Some(v, t) ->
                  return
                    { e with
                        Expr =
                          RunnableExprQueryExprRec.QueryClosureValue(v, t) }
              | RunnableExprQueryExprRec.QueryClosureValue(_, _) -> return e
              | RunnableExprQueryExprRec.QueryCastTo(e, t) ->
                let! e = !e

                return
                  { e with
                      Expr = RunnableExprQueryExprRec.QueryCastTo(e, t) }
              | RunnableExprQueryExprRec.QueryCount(q) ->
                let! q = q |> replace_closure_lookups_query

                return
                  { e with
                      Expr = RunnableExprQueryExprRec.QueryCountEvaluated q }
              | RunnableExprQueryExprRec.QueryExists(q) ->
                let! q = q |> replace_closure_lookups_query

                return
                  { e with
                      Expr = RunnableExprQueryExprRec.QueryExistsEvaluated q }
              | RunnableExprQueryExprRec.QueryArray(q) ->
                let! q = q |> replace_closure_lookups_query

                return
                  { e with
                      Expr = RunnableExprQueryExprRec.QueryArrayEvaluated q }
              | RunnableExprQueryExprRec.QueryCountEvaluated(_) -> return e
              | RunnableExprQueryExprRec.QueryExistsEvaluated(_) -> return e
              | RunnableExprQueryExprRec.QueryArrayEvaluated(_) -> return e
            }

          and replace_closure_lookups_query
            (q: RunnableExprQuery<_>) // : Reader<ValueQuery<_, _>, _, _>
            =
            reader {
              match q with
              | RunnableExprQuery.UnionQueries(q1, q2) ->
                let! q1 = replace_closure_lookups_query q1
                let! q2 = replace_closure_lookups_query q2
                return ValueQuery.ValueUnionQueries(q1, q2, q1.DeserializeFrom)
              | RunnableExprQuery.SimpleQuery q ->
                let! iterators =
                  q.Iterators
                  |> NonEmptyList.map (fun iterator ->
                    reader {
                      let! sourceV = !iterator.Source

                      let varType = iterator.VarType

                      return
                        { ValueQueryIterator.Location = iterator.Location
                          Var = iterator.Var
                          Source = sourceV
                          VarType = varType }
                    })
                  |> reader.AllNonEmpty

                let! ctx = reader.GetContext()

                let closure =
                  ctx.Scope.Values
                  |> Map.filter (fun k _ -> q.Closure |> Map.containsKey k)

                let closure = closure |> Map.map (fun k v -> v, q.Closure.[k])

                let! joins =
                  q.Joins
                  |> Option.map (
                    NonEmptyList.map (fun join ->
                      reader {
                        let! leftV =
                          join.Left |> replace_closure_lookups closure

                        let! rightV =
                          join.Right |> replace_closure_lookups closure

                        return
                          { join with
                              Left = leftV
                              Right = rightV }
                      })
                    >> reader.AllNonEmpty
                  )
                  |> reader.RunOption

                let! where =
                  q.Where
                  |> Option.map (replace_closure_lookups closure)
                  |> reader.RunOption

                let! select = replace_closure_lookups closure q.Select

                let! orderBy =
                  q.OrderBy
                  |> Option.map (fun (v, dir) ->
                    reader {
                      let! v = replace_closure_lookups closure v
                      return v, dir
                    })
                  |> reader.RunOption

                let! distinct =
                  q.Distinct
                  |> Option.map (replace_closure_lookups closure)
                  |> reader.RunOption

                return
                  ValueQuery.ValueQuerySimple
                    { Iterators = iterators
                      Joins = joins
                      Where = where
                      Select = select
                      OrderBy = orderBy
                      Distinct = distinct
                      DeserializeFrom = q.DeserializeFrom }
            }

          let! q' = replace_closure_lookups_query q
          return Value.Query q'
        | _ ->
          return!
            (fun () -> $"Cannot eval expression {e}")
            |> Errors.Singleton loc0
            |> reader.Throw
      }
