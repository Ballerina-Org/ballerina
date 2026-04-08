namespace Ballerina.DSL.Next.Types.TypeChecker

module SchemaPathValidation =
  open Ballerina
  open Ballerina.Collections.Sum
  open Ballerina.State.WithError
  open Ballerina.Errors
  open Ballerina.Collections.Map
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Types.Patterns
  open Ballerina.Cat.Collections.OrderedMap
  open Ballerina.DSL.Next.Types.TypeChecker.Model
  open Ballerina.DSL.Next.Types.TypeChecker.LiftOtherSteps
  open Ballerina.DSL.Next.Unification
  open Ballerina.DSL.Next.Terms.Patterns

  let validatePath<'ve when 've: comparison>
    (evalTypeExpr: TypeExpr<'ve> -> TypeExprEvalResult<'ve>)
    (context: TypeCheckContext<'ve>)
    (loc0: Location)
    (source: TypeValue<'ve>)
    (target: TypeValue<'ve>)
    (path: SchemaPathSegmentExpr list)
    =
    let (!) = evalTypeExpr

    let ofSum (p: Sum<'a, Errors<Unit>>) =
      p |> Sum.mapRight (Errors.MapContext(replaceWith loc0)) |> state.OfSum

    let rec loop source path =
      state {
        match path with
        | [] -> do! TypeValue.Unify(loc0, source, target) |> Expr.liftUnification
        | (_, segment) :: rest ->
          match segment with
          | SchemaPathTypeDecompositionExpr.Field fieldName ->
            let! sourceRecord = source |> TypeValue.AsRecord |> ofSum

            let! (_, (fieldType, _)) =
              sourceRecord
              |> OrderedMap.toSeq
              |> Seq.tryFind (fun (k, _) -> k.Name.LocalName = fieldName.LocalName)
              |> Sum.fromOption (fun () ->
                (fun () -> $"Error: cannot find field {fieldName} in record type {source}")
                |> Errors.Singleton loc0)
              |> state.OfSum

            return! loop fieldType rest
          | SchemaPathTypeDecompositionExpr.UnionCase caseName ->
            let! _, sourceCase = source |> TypeValue.AsUnion |> ofSum

            let! (_, caseType) =
              sourceCase
              |> OrderedMap.toSeq
              |> Seq.tryFind (fun (k, _) -> k.Name.LocalName = caseName.LocalName)
              |> Sum.fromOption (fun () ->
                (fun () -> $"Error: cannot find case {caseName} in union type {source}")
                |> Errors.Singleton loc0)
              |> state.OfSum

            return! loop caseType rest
          | SchemaPathTypeDecompositionExpr.SumCase caseName ->
            let! sourceCase = source |> TypeValue.AsSum |> ofSum

            if
              caseName.Case < 1
              || caseName.Case > sourceCase.Length
              || caseName.Count <> sourceCase.Length
            then
              return!
                (fun () -> $"Error: sum case {caseName} is out of bounds for sum type {source}")
                |> Errors.Singleton loc0
                |> state.Throw
            else
              let! caseType =
                sourceCase
                |> Seq.tryItem (caseName.Case - 1)
                |> Sum.fromOption (fun () ->
                  (fun () -> $"Error: cannot find sum case {caseName} in sum type {source}")
                  |> Errors.Singleton loc0)
                |> state.OfSum

              return! loop caseType rest
          | SchemaPathTypeDecompositionExpr.Item item ->
            let! sourceCase = source |> TypeValue.AsTuple |> ofSum

            if item.Index < 1 || item.Index > sourceCase.Length then
              return!
                (fun () -> $"Error: tuple index {item} is out of bounds for tuple type {source}")
                |> Errors.Singleton loc0
                |> state.Throw
            else
              let! caseType =
                sourceCase
                |> Seq.tryItem (item.Index - 1)
                |> Sum.fromOption (fun () ->
                  (fun () -> $"Error: cannot find tuple index {item} in tuple type {source}")
                  |> Errors.Singleton loc0)
                |> state.OfSum

              return! loop caseType rest
          | SchemaPathTypeDecompositionExpr.Iterator it ->
            let! container, _ = it.Container |> TypeExpr.Lookup |> (!)
            let! t_arg, _ = it.TypeDef |> TypeExpr.Lookup |> (!)

            let! t_map, _ =
              context.Values
              |> Map.tryFindWithError
                (it.Mapper |> TypeCheckScope.Empty.Resolve)
                "mapper function"
                (fun () -> it.Mapper.LocalName)
                loc0
              |> state.OfSum

            let! t_map, _ =
              !TypeExpr.Apply(TypeExpr.Apply(TypeExpr.FromTypeValue t_map, TypeExpr.FromTypeValue t_arg),
                              TypeExpr.FromTypeValue t_arg)

            let! expected, _ = !(TypeExpr.Apply(TypeExpr.FromTypeValue container, TypeExpr.FromTypeValue t_arg))

            let! expected, _ =
              !(TypeExpr.Arrow(
                TypeExpr.Arrow(TypeExpr.FromTypeValue t_arg, TypeExpr.FromTypeValue t_arg),
                TypeExpr.Arrow(TypeExpr.FromTypeValue source, TypeExpr.FromTypeValue expected)
              ))

            do! TypeValue.Unify(loc0, t_map, expected) |> Expr.liftUnification

            return! loop t_arg rest
      }

    loop source path
