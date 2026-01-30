namespace Ballerina.DSL.Next.Types.TypeChecker


[<AutoOpen>]
module KindEval =
  open System
  open Ballerina.Fun
  open Ballerina
  open Ballerina.Collections.Sum
  open Ballerina.StdLib.Object
  open Ballerina.State.WithError
  open Ballerina.Reader.WithError
  open Ballerina.Errors
  open Ballerina.LocalizedErrors
  open Ballerina.Errors
  open Ballerina.Collections.Map
  open System
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Types.Patterns
  open Ballerina.StdLib.OrderPreservingMap
  open Ballerina.Cat.Collections.OrderedMap
  open Ballerina.Collections.NonEmptyList
  open Ballerina.DSL.Next.Types.TypeChecker.Model
  open Ballerina.DSL.Next.Types.TypeChecker.Patterns

  type TypeExpr<'valueExt> with
    static member KindEval<'ve when 've: comparison>() : TypeExprKindEval<'ve> =
      fun _n loc0 t ->
        state {
          let (!) = TypeValue.KindEval<'ve> () None loc0
          let (!!) = TypeExpr.KindEval<'ve> () None loc0
          let error e = Errors.Singleton loc0 e

          match t with
          | TypeExpr.FromTypeValue tv -> return! !tv
          | TypeExpr.Lookup id ->
            let! ctx = state.GetContext()

            return!
              ctx
              |> Map.tryFindWithError id.LocalName "variables" (fun () -> "kind") loc0
              |> state.OfSum
          | TypeExpr.Lambda(param, body) ->
            let! bodyKind = !!body |> state.MapContext(Map.add param.Name param.Kind)
            return Kind.Arrow(param.Kind, bodyKind)
          | TypeExpr.Apply(func, arg) ->
            let! funcKind = !!func
            let! argKind = !!arg

            match funcKind with
            | Kind.Arrow(paramKind, returnKind) when paramKind = argKind -> return returnKind
            | _ -> return! (fun () -> $"Error: kind mismatch") |> error |> state.Throw
          | _ -> return Kind.Star
        }

  and TypeValue<'valueExt> with
    static member KindEval<'ve when 've: comparison>() : TypeValueKindEval<'ve> =
      fun _n loc0 t ->
        state {

          // let (!) = TypeValue.KindEval<'ve> () None loc0
          let (!!) = TypeExpr.KindEval<'ve> () None loc0

          // let error e = Errors.Singleton loc0 e

          // let ofSum (p: Sum<'a, Errors<Unit>>) =
          //   p |> Sum.mapRight (Errors.MapContext(replaceWith loc0)) |> state.OfSum

          match t with
          | TypeValue.Lookup id ->
            let! ctx = state.GetContext()

            return!
              ctx
              |> Map.tryFindWithError id.LocalName "variables" (fun () -> "kind") loc0
              |> state.OfSum
          | TypeValue.Lambda { value = (param, body) } ->
            let! bodyKind = !!body |> state.MapContext(Map.add param.Name param.Kind)
            return Kind.Arrow(param.Kind, bodyKind)
          // | TypeValue.Apply { value = (var, arg) } ->
          //   let! funcKind = !(TypeValue.Lookup(var.Name |> Identifier.LocalScope))
          //   let! argKind = !arg

          //   match funcKind with
          //   | Kind.Arrow(paramKind, returnKind) when paramKind = argKind -> return returnKind
          //   | _ -> return! $"Error: kind mismatch" |> error |> state.Throw
          | TypeValue.Imported i ->
            let paramKinds = i.Parameters |> List.map (fun p -> p.Kind)
            return List.foldBack (fun k acc -> Kind.Arrow(k, acc)) paramKinds Kind.Star

          | _ -> return Kind.Star
        }
