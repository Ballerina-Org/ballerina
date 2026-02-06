namespace Ballerina.DSL.Next.StdLib.List

[<AutoOpen>]
module Extension =
  open Ballerina
  open Ballerina.Collections.Sum
  open Ballerina.Reader.WithError
  open Ballerina.LocalizedErrors
  open Ballerina.Errors
  open Ballerina.DSL.Next.Terms
  open Ballerina.DSL.Next.Terms.Patterns
  open Ballerina.DSL.Next.Types
  open Ballerina.DSL.Next.Types.Patterns
  open Ballerina.Lenses
  open Ballerina.DSL.Next.Extensions
  open Ballerina.DSL.Next.StdLib.List.Model


  let ListExtension<'ext>
    (valueLens: PartialLens<'ext, ListValues<'ext>>)
    // (consLens: PartialLens<'ext, ListConstructors<'ext>>)
    (operationLens: PartialLens<'ext, ListOperations<'ext>>)
    : TypeExtension<'ext, Unit, ListValues<'ext>, ListOperations<'ext>> =
    let listId = Identifier.LocalScope "List"
    let listSymbolId = listId |> TypeSymbol.Create
    let aVar, aKind = TypeVar.Create("a"), Kind.Star
    let listId = listId |> TypeCheckScope.Empty.Resolve

    let listOf (argName: string) =
      TypeExpr.Apply(TypeExpr.Lookup(Identifier.LocalScope "List"), TypeExpr.Lookup(Identifier.LocalScope argName))
    // TypeValue.CreateImported(
    //   { Id = listId
    //     Sym = listSymbolId
    //     Parameters = []
    //     Arguments = [ TypeValue.Lookup(Identifier.LocalScope argName) ]
    //     UnionLike = None
    //     RecordLike = None }
    // )

    let listFoldId =
      Identifier.FullyQualified([ "List" ], "fold") |> TypeCheckScope.Empty.Resolve

    let listLengthId =
      Identifier.FullyQualified([ "List" ], "length") |> TypeCheckScope.Empty.Resolve

    let listFilterId =
      Identifier.FullyQualified([ "List" ], "filter") |> TypeCheckScope.Empty.Resolve

    let listMapId =
      Identifier.FullyQualified([ "List" ], "map") |> TypeCheckScope.Empty.Resolve

    let listAppendId =
      Identifier.FullyQualified([ "List" ], "append") |> TypeCheckScope.Empty.Resolve

    let listConsId =
      Identifier.FullyQualified([ "List" ], "Cons") |> TypeCheckScope.Empty.Resolve

    let listNilId =
      Identifier.FullyQualified([ "List" ], "Nil") |> TypeCheckScope.Empty.Resolve

    let getValueAsList (v: Value<TypeValue<'ext>, 'ext>) : Sum<List<Value<TypeValue<'ext>, 'ext>>, Errors<Unit>> =
      sum {
        let! v, _ = v |> Value.AsExt

        let! v =
          valueLens.Get v
          |> sum.OfOption((fun () -> "cannot get list value") |> Errors<Unit>.Singleton())

        let! v = v |> ListValues.AsList
        v
      }

    let _toValueFromList (v: List<Value<TypeValue<'ext>, 'ext>>) : Value<TypeValue<'ext>, 'ext> =
      (ListValues.List v |> valueLens.Set, None) |> Ext

    let lengthOperation: ResolvedIdentifier * TypeOperationExtension<'ext, Unit, ListValues<'ext>, ListOperations<'ext>> =
      listLengthId,
      { Type =
          TypeValue.CreateLambda(
            TypeParameter.Create("a", aKind),
            TypeExpr.Arrow(
              TypeExpr.Apply(TypeExpr.Lookup(Identifier.LocalScope "List"), TypeExpr.Lookup(Identifier.LocalScope "a")),
              TypeExpr.Primitive(PrimitiveType.Int32)
            )
          )
        Kind = Kind.Arrow(Kind.Star, Kind.Star)
        Operation = List_Length
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | List_Length -> Some(List_Length)
            | _ -> None)
        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              do!
                op
                |> ListOperations.AsLength
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! v, _ =
                v
                |> Value.AsExt
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! v =
                valueLens.Get v
                |> sum.OfOption((fun () -> "cannot get option value") |> Errors.Singleton loc0)
                |> reader.OfSum

              let! v =
                v
                |> ListValues.AsList
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              return Value.Primitive(PrimitiveValue.Int32(v.Length))
            } //: 'extOperations * Value<TypeValue<'ext>, 'ext> -> ExprEvaluator<'ext, 'extValues> }
      }

    let foldOperation: ResolvedIdentifier * TypeOperationExtension<'ext, Unit, ListValues<'ext>, ListOperations<'ext>> =
      listFoldId,
      { Type =
          TypeValue.CreateLambda(
            TypeParameter.Create("a", aKind),
            TypeExpr.Lambda(
              TypeParameter.Create("acc", aKind),
              TypeExpr.Arrow(
                TypeExpr.Arrow(
                  TypeExpr.Lookup(Identifier.LocalScope "acc"),
                  TypeExpr.Arrow(
                    TypeExpr.Lookup(Identifier.LocalScope "a"),
                    TypeExpr.Lookup(Identifier.LocalScope "acc")
                  )
                ),
                TypeExpr.Arrow(
                  TypeExpr.Lookup(Identifier.LocalScope "acc"),
                  TypeExpr.Arrow(
                    TypeExpr.Apply(
                      TypeExpr.Lookup(Identifier.LocalScope "List"),
                      TypeExpr.Lookup(Identifier.LocalScope "a")
                    ),
                    TypeExpr.Lookup(Identifier.LocalScope "acc")
                  )
                )
              )
            )
          )
        Kind = Kind.Arrow(Kind.Star, Kind.Arrow(Kind.Star, Kind.Star))
        Operation = List_Fold {| f = None; acc = None |}
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | List_Fold v -> Some(List_Fold v)
            | _ -> None)
        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              let! f, acc =
                op
                |> ListOperations.AsFold
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              match f with
              | None -> // the closure is empty - first step in the application
                return
                  (ListOperations.List_Fold({| f = Some v; acc = None |}) |> operationLens.Set, Some listFoldId)
                  |> Ext
              | Some f -> // the closure has the function - second step in the application
                match acc with
                | None -> // the closure has the function but not the accumulator - second step in the application
                  return
                    (ListOperations.List_Fold({| f = Some f; acc = Some v |}) |> operationLens.Set, Some listFoldId)
                    |> Ext
                | Some acc -> // the closure has the function and the accumulator - third step in the application
                  let! v, _ =
                    v
                    |> Value.AsExt
                    |> sum.MapError(Errors.MapContext(replaceWith loc0))
                    |> reader.OfSum

                  let! v =
                    valueLens.Get v
                    |> sum.OfOption((fun () -> "cannot get option value") |> Errors.Singleton loc0)
                    |> reader.OfSum

                  let! l =
                    v
                    |> ListValues.AsList
                    |> sum.MapError(Errors.MapContext(replaceWith loc0))
                    |> reader.OfSum

                  let! l =
                    l
                    |> List.fold
                      (fun acc v ->
                        reader {
                          let! acc = acc
                          let! f1 = Expr.EvalApply loc0 [] (f, acc)
                          return! Expr.EvalApply loc0 [] (f1, v)
                        })
                      (reader { return acc })

                  return l
            } //: 'extOperations * Value<TypeValue<'ext>, 'ext> -> ExprEvaluator<'ext, 'extValues> }
      }

    let filterOperation: ResolvedIdentifier * TypeOperationExtension<'ext, Unit, ListValues<'ext>, ListOperations<'ext>> =
      listFilterId,
      { Type =
          TypeValue.CreateLambda(
            TypeParameter.Create("a", aKind),
            TypeExpr.Arrow(
              TypeExpr.Arrow(TypeExpr.Lookup(Identifier.LocalScope "a"), TypeExpr.Primitive PrimitiveType.Bool),
              TypeExpr.Arrow(
                TypeExpr.Apply(
                  TypeExpr.Lookup(Identifier.LocalScope "List"),
                  TypeExpr.Lookup(Identifier.LocalScope "a")
                ),
                TypeExpr.Apply(
                  TypeExpr.Lookup(Identifier.LocalScope "List"),
                  TypeExpr.Lookup(Identifier.LocalScope "a")
                )
              )
            )
          )
        Kind = Kind.Arrow(Kind.Star, Kind.Star)
        Operation = List_Filter {| f = None |}
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | List_Filter v -> Some(List_Filter v)
            | _ -> None)
        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              let! op =
                op
                |> ListOperations.AsFilter
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return
                  (ListOperations.List_Filter({| f = Some v |}) |> operationLens.Set, Some listFilterId)
                  |> Ext
              | Some predicate -> // the closure has the function - second step in the application
                let! v, _ =
                  v
                  |> Value.AsExt
                  |> sum.MapError(Errors.MapContext(replaceWith loc0))
                  |> reader.OfSum

                let! v =
                  valueLens.Get v
                  |> sum.OfOption((fun () -> "cannot get option value") |> Errors.Singleton loc0)
                  |> reader.OfSum

                let! v =
                  v
                  |> ListValues.AsList
                  |> sum.MapError(Errors.MapContext(replaceWith loc0))
                  |> reader.OfSum

                let! v' =
                  v
                  |> List.map (fun v ->
                    reader {
                      let! res = Expr.EvalApply loc0 [] (predicate, v)

                      let! res =
                        res
                        |> Value.AsPrimitive
                        |> sum.MapError(Errors.MapContext(replaceWith loc0))
                        |> reader.OfSum

                      let! res =
                        res
                        |> PrimitiveValue.AsBool
                        |> sum.MapError(Errors.MapContext(replaceWith loc0))
                        |> reader.OfSum

                      return v, res
                    })
                  |> reader.All

                return
                  (v' |> List.filter snd |> List.map fst |> ListValues.List |> valueLens.Set, Some listFilterId)
                  |> Ext
            } //: 'extOperations * Value<TypeValue<'ext>, 'ext> -> ExprEvaluator<'ext, 'extValues> }
      }

    let mapOperation: ResolvedIdentifier * TypeOperationExtension<'ext, Unit, ListValues<'ext>, ListOperations<'ext>> =
      listMapId,
      { Type =
          TypeValue.CreateLambda(
            TypeParameter.Create("a", aKind),
            TypeExpr.Lambda(
              TypeParameter.Create("b", Kind.Star),
              TypeExpr.Arrow(
                TypeExpr.Arrow(TypeExpr.Lookup(Identifier.LocalScope "a"), TypeExpr.Lookup(Identifier.LocalScope "b")),
                TypeExpr.Arrow(listOf "a", listOf "b")
              )
            )
          )
        Kind = Kind.Arrow(Kind.Star, Kind.Arrow(Kind.Star, Kind.Star))
        Operation = List_Map {| f = None |}
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | List_Map v -> Some(List_Map v)
            | _ -> None)
        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              let! op =
                op
                |> ListOperations.AsMap
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return
                  (ListOperations.List_Map({| f = Some v |}) |> operationLens.Set, Some listMapId)
                  |> Ext
              | Some f -> // the closure has the function - second step in the application
                let! v =
                  getValueAsList v
                  |> sum.MapError(Errors.MapContext(replaceWith loc0))
                  |> reader.OfSum

                let! v' = v |> List.map (fun v -> Expr.EvalApply loc0 [] (f, v)) |> reader.All

                return (ListValues.List v' |> valueLens.Set, None) |> Ext
            } //: 'extOperations * Value<TypeValue<'ext>, 'ext> -> ExprEvaluator<'ext, 'extValues> }
      }

    let appendOperation: ResolvedIdentifier * TypeOperationExtension<'ext, Unit, ListValues<'ext>, ListOperations<'ext>> =
      listAppendId,
      { Type =
          TypeValue.CreateLambda(
            TypeParameter.Create("a", aKind),
            TypeExpr.Arrow(
              TypeExpr.Apply(TypeExpr.Lookup(Identifier.LocalScope "List"), TypeExpr.Lookup(Identifier.LocalScope "a")),
              TypeExpr.Arrow(
                TypeExpr.Apply(
                  TypeExpr.Lookup(Identifier.LocalScope "List"),
                  TypeExpr.Lookup(Identifier.LocalScope "a")
                ),
                TypeExpr.Apply(
                  TypeExpr.Lookup(Identifier.LocalScope "List"),
                  TypeExpr.Lookup(Identifier.LocalScope "a")
                )
              )
            )
          )
        Kind = Kind.Arrow(Kind.Star, Kind.Star)
        Operation = List_Append {| l = None |}
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | List_Append v -> Some(List_Append v)
            | _ -> None)
        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              let! op =
                op
                |> ListOperations.AsAppend
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return
                  (ListOperations.List_Append({| l = Some v |}) |> operationLens.Set, Some listAppendId)
                  |> Ext
              | Some l -> // the closure has the first list - second step in the application
                let! l =
                  l
                  |> getValueAsList
                  |> sum.MapError(Errors.MapContext(replaceWith loc0))
                  |> reader.OfSum

                let! v =
                  v
                  |> getValueAsList
                  |> sum.MapError(Errors.MapContext(replaceWith loc0))
                  |> reader.OfSum

                let v' = List.append l v

                return (ListValues.List v' |> valueLens.Set, None) |> Ext
            } //: 'extOperations * Value<TypeValue<'ext>, 'ext> -> ExprEvaluator<'ext, 'extValues> }
      }

    let consOperation: ResolvedIdentifier * TypeOperationExtension<'ext, Unit, ListValues<'ext>, ListOperations<'ext>> =
      listConsId,
      { Type =
          TypeValue.CreateLambda(
            TypeParameter.Create("a", aKind),
            TypeExpr.Arrow(TypeExpr.Tuple([ TypeExpr.Lookup(Identifier.LocalScope "a"); listOf "a" ]), listOf "a")
          )
        Kind = Kind.Arrow(Kind.Star, Kind.Star)
        Operation = List_Cons
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | List_Cons -> Some List_Cons
            | _ -> None)
        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              do!
                op
                |> ListOperations.AsCons
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! items =
                v
                |> Value.AsTuple
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              match items with
              | [ head; tail ] ->
                let! tail, _ =
                  tail
                  |> Value.AsExt
                  |> sum.MapError(Errors.MapContext(replaceWith loc0))
                  |> reader.OfSum

                let! tail =
                  tail
                  |> valueLens.Get
                  |> sum.OfOption((fun () -> $"Error: expected list") |> Errors.Singleton loc0)
                  |> reader.OfSum

                let! tail =
                  tail
                  |> ListValues.AsList
                  |> sum.MapError(Errors.MapContext(replaceWith loc0))
                  |> reader.OfSum

                return (ListValues.List(head :: tail) |> valueLens.Set, None) |> Ext
              | _ -> return! (fun () -> "Error: expected pair") |> Errors.Singleton loc0 |> reader.Throw
            } //: 'extOperations * Value<TypeValue<'ext>, 'ext> -> ExprEvaluator<'ext, 'extValues> }
      }

    let nilOperation: ResolvedIdentifier * TypeOperationExtension<'ext, Unit, ListValues<'ext>, ListOperations<'ext>> =
      listNilId,
      { Type =
          TypeValue.CreateLambda(
            TypeParameter.Create("a", aKind),
            TypeExpr.Arrow(TypeExpr.Primitive PrimitiveType.Unit, listOf "a")
          )
        Kind = Kind.Arrow(Kind.Star, Kind.Star)
        Operation = List_Nil
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | List_Nil -> Some List_Nil
            | _ -> None)
        Apply =
          fun loc0 _rest (op, _) ->
            reader {
              do!
                op
                |> ListOperations.AsNil
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              return (ListValues.List [] |> valueLens.Set, None) |> Ext
            } //: 'extOperations * Value<TypeValue<'ext>, 'ext> -> ExprEvaluator<'ext, 'extValues> }
      }

    { TypeName = listId, listSymbolId
      TypeVars = [ (aVar, aKind) ]
      Cases = Map.empty
      Operations =
        [ lengthOperation
          foldOperation
          filterOperation
          mapOperation
          appendOperation
          consOperation
          nilOperation ]
        |> Map.ofList
    // Deconstruct =
    //   fun (v: ListValues<'ext>) ->
    //     match v with
    //     | ListValues.List(v :: vs) ->
    //       Value<TypeValue<'ext>, 'ext>.Tuple([ v; (vs |> ListValues.List |> valueLens.Set, None) |> Ext ])
    //     | _ -> Value<TypeValue<'ext>, 'ext>.Primitive PrimitiveValue.Unit
    }
