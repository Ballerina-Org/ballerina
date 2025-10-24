namespace Ballerina.DSL.Next.Terms

[<AutoOpen>]
module Eval =
  open Ballerina.Collections.Sum
  open Ballerina.Collections.Map
  open Ballerina.Coroutines.Model
  open Ballerina.Reader.WithError
  open Ballerina.LocalizedErrors
  open Ballerina.StdLib.String
  open Ballerina.StdLib.Object
  open System
  open Ballerina.DSL.Next.Unification
  open Ballerina.DSL.Next.Terms.Model
  open Ballerina.DSL.Next.Terms.Patterns
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Types.Patterns
  open Ballerina.DSL.Next.Types.TypeChecker.Model
  open Ballerina.DSL.Next.Types.TypeChecker.Expr
  open Ballerina.DSL.Next.Types.TypeChecker.Eval
  open Ballerina.DSL.Next.Types.Model
  open Ballerina
  open Ballerina.StdLib.OrderPreservingMap
  open Ballerina.Cat.Collections.OrderedMap
  open Ballerina.DSL.Next.Types.TypeChecker.Expr

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

    static member Append (s1: ExprEvalContextSymbols) (s2: ExprEvalContextSymbols) =
      { Types = Map.fold (fun acc k v -> Map.add k v acc) s1.Types s2.Types
        RecordFields = Map.fold (fun acc k v -> Map.add k v acc) s1.RecordFields s2.RecordFields
        UnionCases = Map.fold (fun acc k v -> Map.add k v acc) s1.UnionCases s2.UnionCases }

  type ExprEvalContext<'valueExtension> =
    { Values: Map<ResolvedIdentifier, Value<TypeValue, 'valueExtension>>
      ExtensionOps: ValueExtensionOps<'valueExtension>
      Symbols: ExprEvalContextSymbols }

  and ExtEvalResult<'valueExtension> =
    | Result of Value<TypeValue, 'valueExtension>
    | Async of Coroutine<ExtEvalResult<'valueExtension>, Unit, Unit, Unit, Errors>
    | Applicable of
      (Value<TypeValue, 'valueExtension> -> ExprEvaluator<'valueExtension, Value<TypeValue, 'valueExtension>>)
    | TypeApplicable of (TypeValue -> ExprEvaluator<'valueExtension, Value<TypeValue, 'valueExtension>>)
    | Matchable of
      (Map<ResolvedIdentifier, CaseHandler<TypeValue, ResolvedIdentifier>>
        -> ExprEvaluator<'valueExtension, Value<TypeValue, 'valueExtension>>)

  and ExtensionEvaluator<'valueExtension> =
    Location -> 'valueExtension -> ExprEvaluator<'valueExtension, ExtEvalResult<'valueExtension>>

  and ValueExtensionOps<'valueExtension> =
    { Eval: ExtensionEvaluator<'valueExtension> }

  and ExprEvaluator<'valueExtension, 'res> = Reader<'res, ExprEvalContext<'valueExtension>, Errors>

  type ExprEvalContext<'valueExtension> with
    static member Empty: ExprEvalContext<'valueExtension> =
      { Values = Map.empty
        ExtensionOps =
          { Eval =
              fun loc0 _ ->
                (loc0, $"Error: cannot evaluate empty extension")
                |> Errors.Singleton
                |> reader.Throw }
        Symbols = ExprEvalContextSymbols.Empty }

    static member Getters =
      {| Values = fun (c: ExprEvalContext<'valueExtension>) -> c.Values
         ExtensionOps = fun (c: ExprEvalContext<'valueExtension>) -> c.ExtensionOps
         Symbols = fun (c: ExprEvalContext<'valueExtension>) -> c.Symbols |}

    static member Updaters =
      {| Values = fun u (c: ExprEvalContext<'valueExtension>) -> { c with Values = u (c.Values) }
         ExtensionOps =
          fun u (c: ExprEvalContext<'valueExtension>) ->
            { c with
                ExtensionOps = u (c.ExtensionOps) }
         Symbols = fun u (c: ExprEvalContext<'valueExtension>) -> { c with Symbols = u (c.Symbols) } |}

  type Expr<'T, 'Id when 'Id: comparison> with
    static member EvalApply (loc0: Location) (fV, argV) =
      reader {
        let! fVVar, fvBody, closure, _scope =
          fV |> Value.AsLambda |> sum.MapError(Errors.FromErrors loc0) |> reader.OfSum

        return!
          reader {
            let closure =
              closure
              |> Map.add (fVVar.Name |> Identifier.LocalScope |> TypeCheckScope.Empty.Resolve) argV

            let! res =
              fvBody
              |> Expr.Eval
              |> reader.MapContext(ExprEvalContext.Updaters.Values(Map.merge (fun _ -> id) closure))
              |> reader.Catch

            match res with
            | Left res -> return res
            | Right err ->
              do Console.WriteLine($"Warning: error during function application {fV} {argV} ({fvBody})")
              do Console.ReadLine() |> ignore
              do closure |> Map.iter (fun k v -> Console.WriteLine($"  {k} = {v}"))
              do Console.ReadLine() |> ignore
              return! err |> reader.Throw
          }
          |> reader.MapError(Errors.SetPriority ErrorPriority.High)
      }

    static member Eval<'valueExtension>
      (e: Expr<TypeValue, ResolvedIdentifier>)
      : ExprEvaluator<'valueExtension, Value<TypeValue, 'valueExtension>> =
      let (!) = Expr.Eval<'valueExtension>
      let loc0 = e.Location

      reader {
        match e.Expr with
        | ExprRec.Primitive v -> return Value.Primitive v
        | ExprRec.If({ Cond = cond
                       Then = thenBody
                       Else = elseBody }) ->
          let! condV = cond |> Expr.Eval

          match condV with
          | Value.Primitive(PrimitiveValue.Bool true) -> return! thenBody |> Expr.Eval
          | Value.Primitive(PrimitiveValue.Bool false) -> return! elseBody |> Expr.Eval
          | v ->
            return!
              (loc0, $"expected boolean in if condition, got {v}")
              |> Errors.Singleton
              |> reader.Throw
        | ExprRec.Let({ Var = var
                        Type = _varType
                        Val = valueExpr
                        Rest = body }) ->
          let! value = valueExpr |> Expr.Eval

          return!
            !body
            |> reader.MapContext(
              ExprEvalContext.Updaters.Values(Map.add (var.Name |> Identifier.LocalScope |> e.Scope.Resolve) value)
            )
        | ExprRec.Lookup({ Id = id }) ->
          let! ctx = reader.GetContext()

          let! res =
            ctx.Values
            |> Map.tryFindWithError id "variables" (id.ToFSharpString) loc0
            |> reader.OfSum
            |> reader.Catch

          match res with
          | Left v -> return v
          | Right err ->
            do Console.WriteLine($"Warning: variable {id} not found in context.")
            do Console.ReadLine() |> ignore
            do ctx.Values |> Map.iter (fun k v -> Console.WriteLine($"  {k} = {v}"))
            do Console.ReadLine() |> ignore
            return! err |> reader.Throw
        | ExprRec.RecordCons { Fields = fields } ->
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
        | ExprRec.RecordWith({ Record = record; Fields = fields }) ->
          let! recordV = !record

          let! recordV =
            recordV
            |> Value.AsRecord
            |> sum.MapError(Errors.FromErrors loc0)
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
        | ExprRec.RecordDes({ ExprRecordDes.Expr = recordExpr
                              Field = fieldId }) ->
          let! recordV = !recordExpr

          let! recordV =
            recordV
            |> Value.AsRecord
            |> sum.MapError(Errors.FromErrors loc0)
            |> reader.OfSum

          // let! ctx = reader.GetContext()

          // let! fieldId =
          //   ctx.Symbols.RecordFields
          //   |> Map.tryFindWithError fieldId "record field id" (fieldId.ToFSharpString) loc0
          //   |> reader.OfSum

          return!
            recordV
            |> Map.tryFindWithError fieldId "record field" (fieldId.ToFSharpString) loc0
            |> reader.OfSum

        | ExprRec.TupleCons { Items = fields } ->
          let! fields = fields |> List.map (!) |> reader.All

          return Value.Tuple(fields)
        | ExprRec.TupleDes({ ExprTupleDes.Tuple = recordExpr
                             Item = fieldId }) ->
          let! recordV = !recordExpr
          let! recordV = recordV |> Value.AsTuple |> sum.MapError(Errors.FromErrors loc0) |> reader.OfSum

          return!
            recordV
            |> List.tryItem (fieldId.Index - 1)
            |> sum.OfOption(
              (loc0, $"Error: tuple index {fieldId.Index} out of bounds, size {List.length recordV}")
              |> Errors.Singleton
            )
            |> reader.OfSum
        | ExprRec.SumCons({ Selector = selector }) ->
          return
            Value.Lambda(
              Var.Create "x",
              Expr.Apply(Expr.SumCons(selector), Expr.Lookup("x" |> Identifier.LocalScope |> e.Scope.Resolve)),
              Map.empty,
              e.Scope
            )
        | ExprRec.Apply({ F = { Expr = ExprRec.SumCons selector }
                          Arg = valueE }) ->
          let! valueV = !valueE
          return Value.Sum(selector.Selector, valueV)
        | ExprRec.Apply({ F = { Expr = ExprRec.UnionDes({ Handlers = cases
                                                          Fallback = fallback }) }
                          Arg = unionE }) ->
          let! unionV = !unionE

          return!
            reader.Any2
              (reader {
                let! unionVCase, unionV =
                  unionV |> Value.AsUnion |> sum.MapError(Errors.FromErrors loc0) |> reader.OfSum

                return!
                  reader {

                    let! caseHandler =
                      cases
                      |> Map.tryFindWithError unionVCase "union case" (unionVCase.ToFSharpString) loc0
                      |> reader.OfSum
                      |> reader.Catch
                      |> reader.Map(Sum.toOption)

                    match caseHandler with
                    | Some caseHandler ->
                      let caseVar, caseBody = caseHandler

                      return!
                        !caseBody
                        |> reader.MapContext(
                          ExprEvalContext.Updaters.Values(
                            Map.add (caseVar.Name |> Identifier.LocalScope |> e.Scope.Resolve) unionV
                          )
                        )
                    | None ->
                      match fallback with
                      | Some fallback -> return! !fallback
                      | None ->
                        return!
                          (loc0, $"Error: cannot find case handler for union case {unionVCase.Name}")
                          |> Errors.Singleton
                          |> reader.Throw
                  }
                  |> reader.MapError(Errors.SetPriority ErrorPriority.High)
              })
              (reader {
                let! unionV = unionV |> Value.AsExt |> sum.MapError(Errors.FromErrors loc0) |> reader.OfSum

                return!
                  reader {
                    let! ctx = reader.GetContext()
                    let unionV = ctx.ExtensionOps.Eval loc0 unionV

                    match! unionV with
                    | Matchable f -> return! f cases
                    | _ ->
                      return!
                        (loc0, "Expected an applicable or matchable extension function")
                        |> Errors.Singleton
                        |> reader.Throw
                  }
                  |> reader.MapError(Errors.SetPriority ErrorPriority.High)
              })
            |> reader.MapError(Errors.FilterHighestPriorityOnly)

        | ExprRec.Apply({ F = { Expr = ExprRec.SumDes cases }
                          Arg = sumE }) ->
          let! sumV = !sumE
          let! sumVCase, sumV = sumV |> Value.AsSum |> sum.MapError(Errors.FromErrors loc0) |> reader.OfSum

          let! caseHandler =
            cases.Handlers
            |> Map.tryFindWithError sumVCase "sum case" (sumVCase.ToFSharpString) loc0
            |> reader.OfSum

          let caseVar, caseBody = caseHandler

          return!
            !caseBody
            |> reader.MapContext(
              ExprEvalContext.Updaters.Values(Map.add (caseVar.Name |> Identifier.LocalScope |> e.Scope.Resolve) sumV)
            )

        | ExprRec.Apply({ F = f; Arg = argE }) ->
          let! fV = !f
          let! argV = !argE

          return!
            reader.Any(
              reader { return! Expr.EvalApply loc0 (fV, argV) },
              [ (reader {
                  let! fUnionCons = fV |> Value.AsUnionCons |> sum.MapError(Errors.FromErrors loc0) |> reader.OfSum
                  return Value.UnionCase(fUnionCons, argV)
                })
                (reader {
                  let! fRecord = fV |> Value.AsRecord |> sum.MapError(Errors.FromErrors loc0) |> reader.OfSum

                  return!
                    reader {
                      let! fRecordCons =
                        fRecord
                        |> Map.tryFindWithError
                          ("cons" |> Identifier.LocalScope |> e.Scope.Resolve)
                          "record field"
                          "cons"
                          loc0
                        |> reader.OfSum

                      let! fUnionCons =
                        fRecordCons
                        |> Value.AsUnionCons
                        |> sum.MapError(Errors.FromErrors loc0)
                        |> reader.OfSum

                      return Value.UnionCase(fUnionCons, argV)
                    }
                    |> reader.MapError(Errors.SetPriority ErrorPriority.High)
                })
                (reader {
                  let! fieldId = fV |> Value.AsRecordDes |> sum.MapError(Errors.FromErrors loc0) |> reader.OfSum

                  return!
                    reader {
                      // let fieldId = fieldId.Name
                      let! recordV = argV |> Value.AsRecord |> sum.MapError(Errors.FromErrors loc0) |> reader.OfSum

                      return!
                        recordV
                        |> Map.tryFindWithError fieldId "record field" (fieldId.ToString()) loc0
                        |> reader.OfSum
                        |> reader.MapError(Errors.Map(String.append $" in record {argV}"))
                    }
                    |> reader.MapError(Errors.SetPriority ErrorPriority.High)
                })
                (reader {
                  let! fExt = fV |> Value.AsExt |> sum.MapError(Errors.FromErrors loc0) |> reader.OfSum

                  return!
                    reader {
                      let! ctx = reader.GetContext()
                      let! fExt = ctx.ExtensionOps.Eval loc0 fExt

                      match fExt with
                      | Applicable f -> return! f argV
                      | _ ->
                        return!
                          (loc0, "Expected an applicable or matchable extension function")
                          |> Errors.Singleton
                          |> reader.Throw
                    }
                    |> reader.MapError(Errors.SetPriority ErrorPriority.High)
                }) ]
            )
            |> reader.MapError(Errors.FilterHighestPriorityOnly)


        | ExprRec.Lambda({ Param = var
                           ParamType = _
                           Body = body }) ->
          let! context = reader.GetContext()
          return Value.Lambda(var, body, context.Values, e.Scope)
        | ExprRec.TypeLambda({ Param = _tp; Body = body }) -> return! !body
        | ExprRec.TypeLet({ ExprTypeLet.Name = typeName
                            TypeDef = typeDefinition
                            Body = body }) ->

          let scope = e.Scope |> TypeCheckScope.Updaters.Type(replaceWith (Some typeName))

          let bind_component (n, v) : Updater<Map<ResolvedIdentifier, Value<TypeValue, 'valueExtension>>> = Map.add n v

          let! definition_cases =
            typeDefinition
            |> TypeValue.AsUnion
            |> sum.MapError(Errors.FromErrors loc0)
            |> reader.OfSum
            |> reader.Catch
            |> reader.Map(Sum.toOption)
            |> reader.Map(Option.map WithTypeExprSourceMapping.Getters.Value)

          let! bind_definition_cases =
            definition_cases
            |> Option.map (fun definition_cases ->
              definition_cases
              |> OrderedMap.toSeq
              |> Seq.map (fun (k, _) ->
                reader {
                  let lens =
                    Value.Record(
                      [ "cons" |> Identifier.LocalScope |> TypeCheckScope.Empty.Resolve,
                        Value.UnionCons(k.Name |> scope.Resolve)
                        "map" |> Identifier.LocalScope |> TypeCheckScope.Empty.Resolve,
                        Value.Lambda(
                          Var.Create "f",
                          Expr.Lambda(
                            Var.Create "x",
                            None,
                            Expr.Apply(
                              Expr.UnionDes(
                                (k.Name |> scope.Resolve,
                                 (Var.Create "_",
                                  Expr.Apply(
                                    Expr.Lookup(k.Name |> scope.Resolve),
                                    Expr.Apply(
                                      Expr.Lookup("f" |> Identifier.LocalScope |> TypeCheckScope.Empty.Resolve),
                                      Expr.Lookup("_" |> Identifier.LocalScope |> TypeCheckScope.Empty.Resolve)
                                    )
                                  )))
                                :: [ for caseId, _ in definition_cases |> OrderedMap.toSeq do
                                       if caseId <> k then
                                         yield
                                           caseId.Name |> scope.Resolve,
                                           (Var.Create "_",
                                            Expr.Apply(
                                              Expr.Lookup(caseId.Name |> e.Scope.Resolve),
                                              Expr.Lookup(
                                                "_" |> Identifier.LocalScope |> TypeCheckScope.Empty.Resolve
                                              )
                                            )) ]
                                |> Map.ofList,
                                None
                              ),
                              Expr.Lookup("x" |> Identifier.LocalScope |> TypeCheckScope.Empty.Resolve)
                            )
                          ),
                          Map.empty,
                          TypeCheckScope.Empty
                        ) ]
                      |> Map.ofList
                    )

                  return
                    bind_component (k.Name |> scope.Resolve, lens)
                    >> bind_component (
                      k.Name.LocalName |> Identifier.LocalScope |> TypeCheckScope.Empty.Resolve,
                      lens
                    )
                })
              |> reader.All)
            |> reader.RunOption
            |> reader.Map(Option.map (List.fold (>>) id) >> Option.defaultValue id)

          let! definition_fields =
            typeDefinition
            |> TypeValue.AsRecord
            |> sum.MapError(Errors.FromErrors loc0)
            |> reader.OfSum
            |> reader.Catch
            |> reader.Map(Sum.toOption)
            |> reader.Map(Option.map WithTypeExprSourceMapping.Getters.Value)

          let! bind_definition_fields =
            definition_fields
            |> Option.map (fun definition_fields ->
              definition_fields
              |> OrderedMap.toSeq
              |> Seq.map (fun (k, _) ->
                reader {
                  let lens =
                    Value.Record(
                      [ "get" |> Identifier.LocalScope |> TypeCheckScope.Empty.Resolve,
                        Value.RecordDes(k.Name |> scope.Resolve)
                        "map" |> Identifier.LocalScope |> TypeCheckScope.Empty.Resolve,
                        Value.Lambda(
                          Var.Create "f",
                          Expr.Lambda(
                            Var.Create "x",
                            None,
                            Expr.RecordWith(
                              Expr.Lookup("x" |> Identifier.LocalScope |> TypeCheckScope.Empty.Resolve),
                              [ k.Name |> scope.Resolve,
                                Expr.Apply(
                                  Expr.Lookup("f" |> Identifier.LocalScope |> TypeCheckScope.Empty.Resolve),
                                  Expr.RecordDes(
                                    Expr.Lookup("x" |> Identifier.LocalScope |> TypeCheckScope.Empty.Resolve),
                                    k.Name |> scope.Resolve
                                  )
                                ) ]
                            )
                          ),
                          Map.empty,
                          TypeCheckScope.Empty
                        ) ]
                      |> Map.ofList
                    )

                  return
                    bind_component (k.Name |> scope.Resolve, lens)
                    >> bind_component (
                      k.Name.LocalName |> Identifier.LocalScope |> TypeCheckScope.Empty.Resolve,
                      lens
                    )
                })
              |> reader.All)
            |> reader.RunOption
            |> reader.Map(Option.map (List.fold (>>) id) >> Option.defaultValue id)

          return!
            !body
            |> reader.MapContext(ExprEvalContext.Updaters.Values(bind_definition_cases >> bind_definition_fields))
        | ExprRec.TypeApply({ ExprTypeApply.Func = typeLambda
                              TypeArg = typeArg }) ->
          return!
            reader.Any(
              // Note: ordering matters here, the most specific branch needs to be evaluated first
              reader {
                let! typeLambda = !typeLambda

                let! ext =
                  typeLambda
                  |> Value.AsExt
                  |> sum.MapError(Errors.FromErrors loc0)
                  |> reader.OfSum

                let! ctx = reader.GetContext()
                let! ext = ctx.ExtensionOps.Eval loc0 ext

                match ext with
                | TypeApplicable f -> return! f typeArg
                | _ ->
                  return!
                    (loc0, $"Expected extension to be type applicable, got {ext}")
                    |> Errors.Singleton
                    |> reader.Throw
              },
              // this says we do not care about the type info
              [ !typeLambda ]
            )
        | _ -> return! (loc0, $"Cannot eval expression {e}") |> Errors.Singleton |> reader.Throw
      }
