namespace Ballerina.DSL.Next.StdLib.List

[<AutoOpen>]
module Extension =
  open Ballerina.Collections.Sum
  open Ballerina.Reader.WithError
  open Ballerina.Errors
  open Ballerina.DSL.Next.Terms
  open Ballerina.DSL.Next.Terms.Patterns
  open Ballerina.DSL.Next.Types
  open Ballerina.DSL.Next.Types.Patterns
  open Ballerina.Lenses
  open Ballerina.DSL.Next.Extensions
  open Ballerina.DSL.Next.StdLib.List

  let ListExtension<'ext>
    (valueLens: PartialLens<'ext, ListValues<'ext>>)
    // (consLens: PartialLens<'ext, ListConstructors<'ext>>)
    (operationLens: PartialLens<'ext, ListOperations<'ext>>)
    : TypeExtension<'ext, Unit, ListValues<'ext>, ListOperations<'ext>> =
    let listId = Identifier.LocalScope "List"
    let listSymbolId = listId |> TypeSymbol.Create
    let aVar, aKind = TypeVar.Create("a"), Kind.Star
    let listFilterId = Identifier.FullyQualified([ "List" ], "filter")
    let listMapId = Identifier.FullyQualified([ "List" ], "map")
    let listConsId = Identifier.FullyQualified([ "List" ], "Cons")
    let listNilId = Identifier.FullyQualified([ "List" ], "Nil")

    let filterOperation: Identifier * TypeOperationExtension<'ext, Unit, ListValues<'ext>, ListOperations<'ext>> =
      listFilterId,
      { Type =
          TypeValue.Lambda(
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
          fun (op, v) ->
            reader {
              let! op = op |> ListOperations.AsFilter |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return ListOperations.List_Filter({| f = Some v |}) |> operationLens.Set
              | Some predicate -> // the closure has the function - second step in the application
                let! v = v |> Value.AsExt |> reader.OfSum

                let! v =
                  valueLens.Get v
                  |> sum.OfOption("cannot get option value" |> Errors.Singleton)
                  |> reader.OfSum

                let! v = v |> ListValues.AsList |> reader.OfSum

                let! v' =
                  v
                  |> List.map (fun v ->
                    reader {
                      let! res = Expr.EvalApply(predicate, v)
                      let! res = res |> Value.AsPrimitive |> reader.OfSum
                      let! res = res |> PrimitiveValue.AsBool |> reader.OfSum
                      return v, res
                    })
                  |> reader.All

                return v' |> List.filter snd |> List.map fst |> ListValues.List |> valueLens.Set
            } //: 'extOperations * Value<TypeValue, 'ext> -> ExprEvaluator<'ext, 'extValues> }
      }

    let mapOperation: Identifier * TypeOperationExtension<'ext, Unit, ListValues<'ext>, ListOperations<'ext>> =
      listMapId,
      { Type =
          TypeValue.Lambda(
            TypeParameter.Create("a", aKind),
            TypeExpr.Lambda(
              TypeParameter.Create("b", Kind.Star),
              TypeExpr.Arrow(
                TypeExpr.Arrow(TypeExpr.Lookup(Identifier.LocalScope "a"), TypeExpr.Lookup(Identifier.LocalScope "b")),
                TypeExpr.Arrow(
                  TypeExpr.Apply(
                    TypeExpr.Lookup(Identifier.LocalScope "List"),
                    TypeExpr.Lookup(Identifier.LocalScope "a")
                  ),
                  TypeExpr.Apply(
                    TypeExpr.Lookup(Identifier.LocalScope "List"),
                    TypeExpr.Lookup(Identifier.LocalScope "b")
                  )
                )
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
          fun (op, v) ->
            reader {
              let! op = op |> ListOperations.AsMap |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return ListOperations.List_Map({| f = Some v |}) |> operationLens.Set
              | Some f -> // the closure has the function - second step in the application
                let! v = v |> Value.AsExt |> reader.OfSum

                let! v =
                  valueLens.Get v
                  |> sum.OfOption("cannot get option value" |> Errors.Singleton)
                  |> reader.OfSum

                let! v = v |> ListValues.AsList |> reader.OfSum

                let! v' = v |> List.map (fun v -> Expr.EvalApply(f, v)) |> reader.All

                return ListValues.List v' |> valueLens.Set
            } //: 'extOperations * Value<TypeValue, 'ext> -> ExprEvaluator<'ext, 'extValues> }
      }

    let consOperation: Identifier * TypeOperationExtension<'ext, Unit, ListValues<'ext>, ListOperations<'ext>> =
      listConsId,
      { Type =
          TypeValue.Lambda(
            TypeParameter.Create("a", aKind),
            TypeExpr.Arrow(
              TypeExpr.Tuple(
                [ TypeExpr.Lookup(Identifier.LocalScope "a")
                  TypeExpr.Apply(
                    TypeExpr.Lookup(Identifier.LocalScope "List"),
                    TypeExpr.Lookup(Identifier.LocalScope "a")
                  ) ]
              ),
              TypeExpr.Apply(TypeExpr.Lookup(Identifier.LocalScope "List"), TypeExpr.Lookup(Identifier.LocalScope "a"))
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
          fun (op, v) ->
            reader {
              do! op |> ListOperations.AsCons |> reader.OfSum

              let! items = v |> Value.AsTuple |> reader.OfSum

              match items with
              | [ head; tail ] ->
                let! tail = tail |> Value.AsExt |> reader.OfSum

                let! tail =
                  tail
                  |> valueLens.Get
                  |> sum.OfOption($"Error: expected list" |> Errors.Singleton)
                  |> reader.OfSum

                let! tail = tail |> ListValues.AsList |> reader.OfSum

                return ListValues.List(head :: tail) |> valueLens.Set
              | _ -> return! "Error: expected pair" |> Errors.Singleton |> reader.Throw
            } //: 'extOperations * Value<TypeValue, 'ext> -> ExprEvaluator<'ext, 'extValues> }
      }

    let nilOperation: Identifier * TypeOperationExtension<'ext, Unit, ListValues<'ext>, ListOperations<'ext>> =
      listNilId,
      { Type =
          TypeValue.Lambda(
            TypeParameter.Create("a", aKind),
            TypeExpr.Arrow(
              TypeExpr.Primitive(PrimitiveType.Unit),
              TypeExpr.Apply(TypeExpr.Lookup(Identifier.LocalScope "List"), TypeExpr.Lookup(Identifier.LocalScope "a"))
            )
          )
        Kind = Kind.Arrow(Kind.Star, Kind.Star)
        Operation = List_Nil
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | List_Nil -> Some List_Nil
            | _ -> None)
        Apply =
          fun (op, _) ->
            reader {
              do! op |> ListOperations.AsNil |> reader.OfSum

              return ListValues.List [] |> valueLens.Set
            } //: 'extOperations * Value<TypeValue, 'ext> -> ExprEvaluator<'ext, 'extValues> }
      }

    { TypeName = listId, listSymbolId
      TypeVars = [ (aVar, aKind) ]
      WrapTypeVars = fun t -> TypeValue.Lambda(TypeParameter.Create(aVar.Name, aKind), t)
      Cases = Map.empty
      Operations = [ filterOperation; mapOperation; consOperation; nilOperation ] |> Map.ofList
      Deconstruct =
        fun (v: ListValues<'ext>) ->
          match v with
          | ListValues.List(v :: vs) ->
            Value<TypeValue, 'ext>.Tuple([ v; vs |> ListValues.List |> valueLens.Set |> Ext ])
          | _ -> Value<TypeValue, 'ext>.Primitive PrimitiveValue.Unit }
