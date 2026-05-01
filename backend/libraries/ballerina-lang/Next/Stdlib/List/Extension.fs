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
  open Ballerina.DSL.Next.Types.TypeChecker
  open Ballerina.Lenses
  open Ballerina.DSL.Next.Extensions
  open Ballerina.DSL.Next.StdLib.List.Model
  open Ballerina.DSL.Next.Serialization.PocoObjects
  open Ballerina.DSL.Next.Serialization
  open Ballerina.DSL.Next.Serialization.ValueSerializer
  open Ballerina.DSL.Next.Serialization.ValueDeserializer
  open Ballerina.DSL.Next.StdLib
  open Ballerina.Data.Delta.Serialization.DeltaDTO
  open Ballerina.Data.Delta.Serialization
  open Ballerina.Data.Delta.Serialization.DeltaSerializer
  open Ballerina.Data.Delta.Serialization.DeltaDeserializer

  let ListExtension<'runtimeContext, 'ext, 'extDTO, 'deltaExt, 'deltaExtDTO
    when 'ext: comparison
    and 'extDTO: not null
    and 'extDTO: not struct
    and 'deltaExtDTO: not null
    and 'deltaExtDTO: not struct>
    (valueLens: PartialLens<'ext, ListValues<'ext>>)
    // (consLens: PartialLens<'ext, ListConstructors<'ext>>)
    (operationLens: PartialLens<'ext, ListOperations<'ext>>)
    (valueDTOLens: PartialLens<'extDTO, ListValueDTO<'extDTO>>)
    (deltaLens: PartialLens<'deltaExt, ListDeltaExt<'ext, 'deltaExt>>)
    (deltaDTOLens:
      PartialLens<'deltaExtDTO, ListDeltaExtDTO<'extDTO, 'deltaExtDTO>>)
    (typeSymbol: Option<TypeSymbol>)
    : TypeExtension<
        'runtimeContext,
        'ext,
        'extDTO,
        'deltaExt,
        'deltaExtDTO,
        Unit,
        ListValues<'ext>,
        ListOperations<'ext>
       > *
      TypeSymbol *
      (TypeValue<'ext> -> TypeValue<'ext>)
    =
    let listId = Identifier.LocalScope "List"

    let listSymbolId =
      typeSymbol |> Option.defaultWith (fun () -> listId |> TypeSymbol.Create)

    let aVar, aKind = TypeVar.Create("a"), Kind.Star
    let listId = listId |> TypeCheckScope.Empty.Resolve

    let listOf (argName: string) =
      TypeExpr.Apply(
        TypeExpr.Lookup(Identifier.LocalScope "List"),
        TypeExpr.Lookup(Identifier.LocalScope argName)
      )

    let make_listType (inner: TypeValue<'ext>) =
      TypeValue.Imported
        { Id = listId
          Sym = listSymbolId
          Parameters = []
          Arguments = [ inner ] }

    // TypeValue.CreateImported(
    //   { Id = listId
    //     Sym = listSymbolId
    //     Parameters = []
    //     Arguments = [ TypeValue.Lookup(Identifier.LocalScope argName) ]
    //
    //      }
    // )

    let listFoldId =
      Identifier.FullyQualified([ "List" ], "fold")
      |> TypeCheckScope.Empty.Resolve

    let listLengthId =
      Identifier.FullyQualified([ "List" ], "length")
      |> TypeCheckScope.Empty.Resolve

    let listFilterId =
      Identifier.FullyQualified([ "List" ], "filter")
      |> TypeCheckScope.Empty.Resolve

    let listMapId =
      Identifier.FullyQualified([ "List" ], "map")
      |> TypeCheckScope.Empty.Resolve

    let listOrderById =
      Identifier.FullyQualified([ "List" ], "orderBy")
      |> TypeCheckScope.Empty.Resolve

    let listAppendId =
      Identifier.FullyQualified([ "List" ], "append")
      |> TypeCheckScope.Empty.Resolve

    let listConsId =
      Identifier.FullyQualified([ "List" ], "Cons")
      |> TypeCheckScope.Empty.Resolve

    let listNilId =
      Identifier.FullyQualified([ "List" ], "Nil")
      |> TypeCheckScope.Empty.Resolve

    let listDecomposeId =
      Identifier.FullyQualified([ "List" ], "decompose")
      |> TypeCheckScope.Empty.Resolve

    let listAnyId =
      Identifier.FullyQualified([ "List" ], "any")
      |> TypeCheckScope.Empty.Resolve

    let getValueAsList
      (v: Value<TypeValue<'ext>, 'ext>)
      : Sum<List<Value<TypeValue<'ext>, 'ext>>, Errors<Unit>> =
      sum {
        let! v, _ = v |> Value.AsExt

        let! v =
          valueLens.Get v
          |> sum.OfOption(
            (fun () -> "cannot get list value") |> Errors<Unit>.Singleton()
          )

        let! v = v |> ListValues.AsList
        v
      }

    let _toValueFromList
      (v: List<Value<TypeValue<'ext>, 'ext>>)
      : Value<TypeValue<'ext>, 'ext> =
      (ListValues.List v |> valueLens.Set, None) |> Ext

    let lengthOperation
      : ResolvedIdentifier *
        TypeOperationExtension<
          'runtimeContext,
          'ext,
          Unit,
          ListValues<'ext>,
          ListOperations<'ext>
         > =
      listLengthId,
      { Type =
          TypeValue.CreateLambda(
            TypeParameter.Create("a", aKind),
            TypeExpr.Arrow(
              TypeExpr.Apply(
                TypeExpr.Lookup(Identifier.LocalScope "List"),
                TypeExpr.Lookup(Identifier.LocalScope "a")
              ),
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
                |> sum.OfOption(
                  (fun () -> "cannot get option value") |> Errors.Singleton loc0
                )
                |> reader.OfSum

              let! v =
                v
                |> ListValues.AsList
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              return Value.Primitive(PrimitiveValue.Int32(v.Length))
            } //: 'extOperations * Value<TypeValue<'ext>, 'ext> -> ExprEvaluator<'ext, 'extValues> }
      }

    let foldOperation
      : ResolvedIdentifier *
        TypeOperationExtension<
          'runtimeContext,
          'ext,
          Unit,
          ListValues<'ext>,
          ListOperations<'ext>
         > =
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
                  (ListOperations.List_Fold({| f = Some v; acc = None |})
                   |> operationLens.Set,
                   Some listFoldId)
                  |> Ext
              | Some f -> // the closure has the function - second step in the application
                match acc with
                | None -> // the closure has the function but not the accumulator - second step in the application
                  return
                    (ListOperations.List_Fold({| f = Some f; acc = Some v |})
                     |> operationLens.Set,
                     Some listFoldId)
                    |> Ext
                | Some acc -> // the closure has the function and the accumulator - third step in the application
                  let! v, _ =
                    v
                    |> Value.AsExt
                    |> sum.MapError(Errors.MapContext(replaceWith loc0))
                    |> reader.OfSum

                  let! v =
                    valueLens.Get v
                    |> sum.OfOption(
                      (fun () -> "cannot get option value")
                      |> Errors.Singleton loc0
                    )
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

    let filterOperation
      : ResolvedIdentifier *
        TypeOperationExtension<
          'runtimeContext,
          'ext,
          Unit,
          ListValues<'ext>,
          ListOperations<'ext>
         > =
      listFilterId,
      { Type =
          TypeValue.CreateLambda(
            TypeParameter.Create("a", aKind),
            TypeExpr.Arrow(
              TypeExpr.Arrow(
                TypeExpr.Lookup(Identifier.LocalScope "a"),
                TypeExpr.Primitive PrimitiveType.Bool
              ),
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
                  (ListOperations.List_Filter({| f = Some v |})
                   |> operationLens.Set,
                   Some listFilterId)
                  |> Ext
              | Some predicate -> // the closure has the function - second step in the application
                let! v, _ =
                  v
                  |> Value.AsExt
                  |> sum.MapError(Errors.MapContext(replaceWith loc0))
                  |> reader.OfSum

                let! v =
                  valueLens.Get v
                  |> sum.OfOption(
                    (fun () -> "cannot get option value")
                    |> Errors.Singleton loc0
                  )
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
                  (v'
                   |> List.filter snd
                   |> List.map fst
                   |> ListValues.List
                   |> valueLens.Set,
                   Some listFilterId)
                  |> Ext
            } //: 'runtimeContext * 'extOperations * Value<TypeValue<'ext>, 'ext> -> ExprEvaluator<'runtimeContext, 'ext, 'extValues> }
      }

    let anyOperation
      : ResolvedIdentifier *
        TypeOperationExtension<
          'runtimeContext,
          'ext,
          Unit,
          ListValues<'ext>,
          ListOperations<'ext>
         > =
      listAnyId,
      { Type =
          TypeValue.CreateLambda(
            TypeParameter.Create("a", aKind),
            TypeExpr.Lambda(
              TypeParameter.Create("b", Kind.Star),
              TypeExpr.Arrow(
                TypeExpr.Arrow(
                  TypeExpr.Lookup(Identifier.LocalScope "a"),
                  TypeExpr.Sum
                    [ TypeExpr.Primitive PrimitiveType.Unit
                      TypeExpr.Lookup(Identifier.LocalScope "b") ]
                ),
                TypeExpr.Arrow(
                  TypeExpr.Apply(
                    TypeExpr.Lookup(Identifier.LocalScope "List"),
                    TypeExpr.Lookup(Identifier.LocalScope "a")
                  ),
                  TypeExpr.Sum
                    [ TypeExpr.Primitive PrimitiveType.Unit
                      TypeExpr.Lookup(Identifier.LocalScope "b") ]
                )
              )
            )
          )
        Kind = Kind.Arrow(Kind.Star, Kind.Arrow(Kind.Star, Kind.Star))
        Operation = List_Any {| f = None |}
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | List_Any v -> Some(List_Any v)
            | _ -> None)
        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              let! op =
                op
                |> ListOperations.AsAny
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              match op with
              | None -> // first step: store the predicate function
                return
                  (ListOperations.List_Any({| f = Some v |})
                   |> operationLens.Set,
                   Some listAnyId)
                  |> Ext
              | Some predicate -> // second step: iterate list, short-circuit on first 2Of2
                let! v, _ =
                  v
                  |> Value.AsExt
                  |> sum.MapError(Errors.MapContext(replaceWith loc0))
                  |> reader.OfSum

                let! v =
                  valueLens.Get v
                  |> sum.OfOption(
                    (fun () -> "cannot get list value")
                    |> Errors.Singleton loc0
                  )
                  |> reader.OfSum

                let! v =
                  v
                  |> ListValues.AsList
                  |> sum.MapError(Errors.MapContext(replaceWith loc0))
                  |> reader.OfSum

                // Apply predicate to each element and collect (item, tag) pairs
                let! results =
                  v
                  |> List.map (fun item ->
                    reader {
                      let! res = Expr.EvalApply loc0 [] (predicate, item)

                      let! tag, _value =
                        res
                        |> Value.AsSum
                        |> sum.MapError(Errors.MapContext(replaceWith loc0))
                        |> reader.OfSum

                      return tag, res
                    })
                  |> reader.All

                // Find the first 2Of2 result
                let selector2Of2 = { Case = 2; Count = 2 }
                match results |> List.tryFind (fun (tag, _) -> tag = selector2Of2) with
                | Some (_, found) -> return found
                | None ->
                  let selector1Of2 = { Case = 1; Count = 2 }
                  return Value.Sum(selector1Of2, Value.Primitive PrimitiveValue.Unit)
            } }

    let mapOperation
      : ResolvedIdentifier *
        TypeOperationExtension<
          'runtimeContext,
          'ext,
          Unit,
          ListValues<'ext>,
          ListOperations<'ext>
         > =
      listMapId,
      { Type =
          TypeValue.CreateLambda(
            TypeParameter.Create("a", aKind),
            TypeExpr.Lambda(
              TypeParameter.Create("b", Kind.Star),
              TypeExpr.Arrow(
                TypeExpr.Arrow(
                  TypeExpr.Lookup(Identifier.LocalScope "a"),
                  TypeExpr.Lookup(Identifier.LocalScope "b")
                ),
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
                  (ListOperations.List_Map({| f = Some v |})
                   |> operationLens.Set,
                   Some listMapId)
                  |> Ext
              | Some f -> // the closure has the function - second step in the application
                let! v =
                  getValueAsList v
                  |> sum.MapError(Errors.MapContext(replaceWith loc0))
                  |> reader.OfSum

                let! v' =
                  v
                  |> List.map (fun v -> Expr.EvalApply loc0 [] (f, v))
                  |> reader.All

                return (ListValues.List v' |> valueLens.Set, None) |> Ext
            } //: 'runtimeContext * 'extOperations * Value<TypeValue<'ext>, 'ext> -> ExprEvaluator<'runtimeContext, 'ext, 'extValues> }
      }

    let appendOperation
      : ResolvedIdentifier *
        TypeOperationExtension<
          'runtimeContext,
          'ext,
          Unit,
          ListValues<'ext>,
          ListOperations<'ext>
         > =
      listAppendId,
      { Type =
          TypeValue.CreateLambda(
            TypeParameter.Create("a", aKind),
            TypeExpr.Arrow(
              TypeExpr.Apply(
                TypeExpr.Lookup(Identifier.LocalScope "List"),
                TypeExpr.Lookup(Identifier.LocalScope "a")
              ),
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
                  (ListOperations.List_Append({| l = Some v |})
                   |> operationLens.Set,
                   Some listAppendId)
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

    let consOperation
      : ResolvedIdentifier *
        TypeOperationExtension<
          'runtimeContext,
          'ext,
          Unit,
          ListValues<'ext>,
          ListOperations<'ext>
         > =
      listConsId,
      { Type =
          TypeValue.CreateLambda(
            TypeParameter.Create("a", aKind),
            TypeExpr.Arrow(
              TypeExpr.Tuple(
                [ TypeExpr.Lookup(Identifier.LocalScope "a"); listOf "a" ]
              ),
              listOf "a"
            )
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
                  |> sum.OfOption(
                    (fun () -> $"Error: expected list") |> Errors.Singleton loc0
                  )
                  |> reader.OfSum

                let! tail =
                  tail
                  |> ListValues.AsList
                  |> sum.MapError(Errors.MapContext(replaceWith loc0))
                  |> reader.OfSum

                return
                  (ListValues.List(head :: tail) |> valueLens.Set, None) |> Ext
              | _ ->
                return!
                  (fun () -> "Error: expected pair")
                  |> Errors.Singleton loc0
                  |> reader.Throw
            } //: 'runtimeContext * 'extOperations * Value<TypeValue<'ext>, 'ext> -> ExprEvaluator<'runtimeContext, 'ext, 'extValues> }
      }

    let nilOperation
      : ResolvedIdentifier *
        TypeOperationExtension<
          'runtimeContext,
          'ext,
          Unit,
          ListValues<'ext>,
          ListOperations<'ext>
         > =
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

    let decomposeOperation
      : ResolvedIdentifier *
        TypeOperationExtension<
          'runtimeContext,
          'ext,
          Unit,
          ListValues<'ext>,
          ListOperations<'ext>
         > =
      listDecomposeId,
      { Type =
          TypeValue.CreateLambda(
            TypeParameter.Create("a", aKind),
            TypeExpr.Arrow(
              listOf "a",
              TypeExpr.Sum(
                [ TypeExpr.Primitive PrimitiveType.Unit
                  TypeExpr.Tuple(
                    [ TypeExpr.Lookup(Identifier.LocalScope "a"); listOf "a" ]
                  ) ]
              )
            )

          )
        Kind = Kind.Arrow(Kind.Star, Kind.Star)
        Operation = List_Decompose
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | List_Decompose -> Some List_Decompose
            | _ -> None)
        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              do!
                op
                |> ListOperations.AsDecompose
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! v =
                v
                |> getValueAsList
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              match v with
              | head :: tail ->
                let tailAsValue =
                  (ListValues.List tail |> valueLens.Set, None) |> Ext

                return
                  Value.Sum(
                    { Case = 2; Count = 2 },
                    Value.Tuple([ head; tailAsValue ])
                  )
              | [] ->
                return
                  Value.Sum(
                    { Case = 1; Count = 2 },
                    Value.Primitive PrimitiveValue.Unit
                  )
            } }


    //'ext :: ValueExt Choice1of6 ListValue ...
    let listToDTO
      (value: 'ext)
      (applicableId: Option<ResolvedIdentifierDTO>)
      : Reader<
          ValueDTO<'extDTO>,
          SerializationContext<'ext, 'extDTO>,
          Ballerina.Errors.Errors<unit>
         >
      =
      reader {
        let! List listValue =
          value
          |> valueLens.Get
          |> sum.OfOption(
            Ballerina.Errors.Errors.Singleton () (fun _ ->
              "Expected list value in listToDTO.")
          )
          |> reader.OfSum

        if listValue.Length = 0 then
          return
            [||]
            |> valueDTOLens.Set
            |> fun ext -> new ValueDTO<'extDTO>(applicableId, ext)
        else
          let! listElementsDTO = listValue |> List.map valueToDTO |> reader.All

          return
            listElementsDTO
            |> List.toArray
            |> valueDTOLens.Set
            |> fun ext -> new ValueDTO<'extDTO>(applicableId, ext)
      }

    let DTOToList
      (valueDTO: 'extDTO)
      (applicableId: Option<ResolvedIdentifier>)
      : Reader<
          Value<TypeValue<'ext>, 'ext>,
          SerializationContext<'ext, 'extDTO>,
          Ballerina.Errors.Errors<unit>
         >
      =
      reader {
        let! listValueDTO =
          valueDTO
          |> valueDTOLens.Get
          |> sum.OfOption(
            Ballerina.Errors.Errors.Singleton () (fun _ ->
              "Expected list value DTO in DTOToList.")
          )
          |> reader.OfSum



        match listValueDTO.Length with
        | 0 -> return Ext(ListValues.List [] |> valueLens.Set, applicableId)
        | _ when isNull listValueDTO |> not && listValueDTO.Length > 0 ->
          let! listElements =
            listValueDTO |> Array.map valueFromDTO |> reader.All

          return
            Ext(ListValues.List listElements |> valueLens.Set, applicableId)
        | _ ->
          return!
            reader.Throw(
              Ballerina.Errors.Errors.Singleton () (fun _ ->
                "Non empty list DTO was given but the data is null or empty.")
            )
      }

    let listDeltaToDTO
      (delta: 'deltaExt)
      : Reader<
          DeltaDTO<'extDTO, 'deltaExtDTO>,
          DeltaSerializationContext<'ext, 'extDTO, 'deltaExt, 'deltaExtDTO>,
          Errors<unit>
         >
      =
      reader {
        let! listDelta =
          delta
          |> deltaLens.Get
          |> sum.OfOption(
            Errors.Singleton () (fun _ ->
              "Expected list delta extension in listDeltaToDTO.")
          )
          |> reader.OfSum

        match listDelta with
        | UpdateElement(i, delta) ->
          let! deltaDTO = deltaToDTO delta

          return
            ListDeltaExtDTO.CreateUpdate i deltaDTO
            |> deltaDTOLens.Set
            |> fun ext -> new DeltaDTO<'extDTO, 'deltaExtDTO>(ext)
        | AppendElement v ->
          let! valueDTO =
            valueToDTO v
            |> reader.MapContext(fun context -> context.SerializationContext)

          return
            ListDeltaExtDTO.CreateAppend valueDTO
            |> deltaDTOLens.Set
            |> fun ext -> new DeltaDTO<'extDTO, 'deltaExtDTO>(ext)
        | RemoveElement index ->
          return
            ListDeltaExtDTO.CreateRemove index
            |> deltaDTOLens.Set
            |> fun ext -> new DeltaDTO<'extDTO, 'deltaExtDTO>(ext)
        | InsertElement(index, v) ->
          let! valueDTO =
            valueToDTO v
            |> reader.MapContext(fun context -> context.SerializationContext)

          return
            ListDeltaExtDTO.CreateInsert index valueDTO
            |> deltaDTOLens.Set
            |> fun ext -> new DeltaDTO<'extDTO, 'deltaExtDTO>(ext)
        | DuplicateElement index ->
          return
            ListDeltaExtDTO.CreateDuplicate index
            |> deltaDTOLens.Set
            |> fun ext -> new DeltaDTO<'extDTO, 'deltaExtDTO>(ext)
        | SetAllElements value ->
          let! valueDTO =
            valueToDTO value
            |> reader.MapContext(fun context -> context.SerializationContext)

          return
            ListDeltaExtDTO.CreateSetAllElements valueDTO
            |> deltaDTOLens.Set
            |> fun ext -> new DeltaDTO<'extDTO, 'deltaExtDTO>(ext)
        | RemoveAllElements ->
          return
            ListDeltaExtDTO.CreateRemoveAllElements
            |> deltaDTOLens.Set
            |> fun ext -> new DeltaDTO<'extDTO, 'deltaExtDTO>(ext)
        | MoveElement(fromIndex, toIndex) ->
          return
            ListDeltaExtDTO.CreateMoveElement fromIndex toIndex
            |> deltaDTOLens.Set
            |> fun ext -> new DeltaDTO<'extDTO, 'deltaExtDTO>(ext)
      }

    let listDeltaFromDTO
      (deltaDTO: 'deltaExtDTO)
      : Reader<
          Ballerina.Data.Delta.Model.Delta<'ext, 'deltaExt>,
          DeltaSerializationContext<'ext, 'extDTO, 'deltaExt, 'deltaExtDTO>,
          Errors<unit>
         >
      =
      reader {
        let! listDeltaDTO =
          deltaDTO
          |> deltaDTOLens.Get
          |> sum.OfOption(
            Errors.Singleton () (fun _ ->
              "Expected list delta DTO extension in listDeltaToDTO.")
          )
          |> reader.OfSum

        if listDeltaDTO.UpdateElement |> isNull |> not then
          let! index, value =
            assertSingleElementDictionary
              listDeltaDTO.UpdateElement
              "update element delta"

          let! delta = deltaFromDTO value

          return
            UpdateElement(index, delta)
            |> deltaLens.Set
            |> Data.Delta.Model.Delta.Ext
        elif listDeltaDTO.AppendElement |> isNull |> not then
          let! value =
            valueFromDTO listDeltaDTO.AppendElement
            |> reader.MapContext(fun context -> context.SerializationContext)

          return
            AppendElement value |> deltaLens.Set |> Data.Delta.Model.Delta.Ext
        elif listDeltaDTO.RemoveElement.HasValue then
          return
            RemoveElement listDeltaDTO.RemoveElement.Value
            |> deltaLens.Set
            |> Data.Delta.Model.Delta.Ext
        elif isNull listDeltaDTO.InsertElement |> not then
          let! index, value =
            assertSingleElementDictionary
              listDeltaDTO.InsertElement
              "insert element delta"

          let! value =
            valueFromDTO value
            |> reader.MapContext(fun context -> context.SerializationContext)

          return
            InsertElement(index, value)
            |> deltaLens.Set
            |> Data.Delta.Model.Delta.Ext
        elif listDeltaDTO.DuplicateElement.HasValue then
          return
            DuplicateElement listDeltaDTO.DuplicateElement.Value
            |> deltaLens.Set
            |> Data.Delta.Model.Delta.Ext
        elif isNull listDeltaDTO.SetAllElements |> not then
          let! value =
            valueFromDTO listDeltaDTO.SetAllElements
            |> reader.MapContext(fun context -> context.SerializationContext)

          return
            SetAllElements value |> deltaLens.Set |> Data.Delta.Model.Delta.Ext
        elif listDeltaDTO.RemoveAllElements.HasValue then
          return
            RemoveAllElements |> deltaLens.Set |> Data.Delta.Model.Delta.Ext
        elif isNull listDeltaDTO.MoveElement |> not then
          return
            MoveElement(
              listDeltaDTO.MoveElement.From,
              listDeltaDTO.MoveElement.To
            )
            |> deltaLens.Set
            |> Data.Delta.Model.Delta.Ext
        else
          return!
            reader.Throw(
              Errors.Singleton () (fun _ -> "Malformed list delta DTO.")
            )
      }

    let orderByOperation
      : ResolvedIdentifier *
        TypeOperationExtension<
          'runtimeContext,
          'ext,
          Unit,
          ListValues<'ext>,
          ListOperations<'ext>
         > =
      listOrderById,
      { Type =
          TypeValue.CreateLambda(
            TypeParameter.Create("a", aKind),
            TypeExpr.Lambda(
              TypeParameter.Create("b", Kind.Star),
              TypeExpr.Arrow(
                TypeExpr.Arrow(
                  TypeExpr.Lookup(Identifier.LocalScope "a"),
                  TypeExpr.Lookup(Identifier.LocalScope "b")
                ),
                TypeExpr.Arrow(listOf "a", listOf "a")
              )
            )
          )
        Kind = Kind.Arrow(Kind.Star, Kind.Arrow(Kind.Star, Kind.Star))
        Operation = List_OrderBy {| f = None |}
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | List_OrderBy v -> Some(List_OrderBy v)
            | _ -> None)
        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              let! op =
                op
                |> ListOperations.AsOrderBy
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return
                  (ListOperations.List_OrderBy({| f = Some v |})
                   |> operationLens.Set,
                   Some listOrderById)
                  |> Ext
              | Some f -> // the closure has the function - second step in the application
                let! v =
                  getValueAsList v
                  |> sum.MapError(Errors.MapContext(replaceWith loc0))
                  |> reader.OfSum

                let! v' =
                  v
                  |> List.map (fun v ->
                    reader {
                      let! sortKey = Expr.EvalApply loc0 [] (f, v)
                      return sortKey, v
                    })
                  |> reader.All

                let v' = v' |> List.sortBy fst |> List.map snd

                return (ListValues.List v' |> valueLens.Set, None) |> Ext
            } //: 'extOperations * Value<TypeValue<'ext>, 'ext> -> ExprEvaluator<'ext, 'extValues> }
      }

    let isListInstanceOf: IsExtInstanceOf<'ext> =
      fun (f: IsValueInstanceOf<'ext>) v t ->
        reader {
          let! l =
            v
            |> valueLens.Get
            |> sum.OfOption(
              Errors.Singleton () (fun _ ->
                "Expected list value in isListInstanceOf.")
            )
            |> reader.OfSum

          match l, t with
          | ListValues.List l, TypeValue.Imported i when i.Id = listId ->
            let! arg_t =
              i.Arguments
              |> Seq.tryHead
              |> sum.OfOption(
                Errors.Singleton () (fun _ ->
                  "Expected type argument in isListInstanceOf.")
              )
              |> reader.OfSum

            return!
              l
              |> Seq.map (fun v -> f (v, arg_t))
              |> reader.All
              |> reader.Ignore
          | _ ->
            return!
              Errors.Singleton () (fun _ ->
                "Expected list value in isListInstanceOf.")
              |> reader.Throw
        }

    let listExtension =
      { TypeName = listId, listSymbolId
        TypeVars = [ (aVar, aKind) ]
        Cases = Map.empty
        Operations =
          [ lengthOperation
            foldOperation
            filterOperation
            anyOperation
            mapOperation
            orderByOperation
            appendOperation
            consOperation
            nilOperation
            decomposeOperation ]
          |> Map.ofList
        // Deconstruct =
        //   fun (v: ListValues<'ext>) ->
        //     match v with
        //     | ListValues.List(v :: vs) ->
        //       Value<TypeValue<'ext>, 'ext>.Tuple([ v; (vs |> ListValues.List |> valueLens.Set, None) |> Ext ])
        //     | _ -> Value<TypeValue<'ext>, 'ext>.Primitive PrimitiveValue.Unit
        Serialization =
          Some
            { SerializationContext =
                { ToDTO = listToDTO
                  FromDTO = DTOToList }
              ToDTO = listDeltaToDTO
              FromDTO = listDeltaFromDTO }
        ExtTypeChecker = isListInstanceOf |> Some }

    listExtension, listSymbolId, make_listType
