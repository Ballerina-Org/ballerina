namespace Ballerina.DSL.Next

module Unification =
  open Ballerina.Collections.Sum
  open Ballerina.Collections.NonEmptyList
  open Ballerina.State.WithError
  open Ballerina.Reader.WithError
  open Ballerina.Errors
  open System
  open Ballerina.Fun
  open Ballerina.StdLib.Object
  open Ballerina.DSL.Next.Model
  open Ballerina.DSL.Next.EquivalenceClasses

  type UnificationContext = TypeExprEvalContext

  type UnificationState = EquivalenceClasses<TypeVar, TypeValue>

  type TypeValue with
    static member Unify(left: TypeValue, right: TypeValue) : State<Unit, UnificationContext, UnificationState, Errors> =
      let bind (var: TypeVar, value: TypeValue) =
        state {
          let! ctx = state.GetContext()

          return!
            EquivalenceClasses.Bind(var, value)
            |> State.mapContext<UnificationContext> (fun (_ctx: UnificationContext) ->
              { equalize =
                  fun (left, right) ->
                    state {
                      // let! s = state.GetState()
                      // do Console.WriteLine($"Equalizing {left} and {right} with state {s.ToFSharpString}")
                      // do Console.ReadLine() |> ignore

                      do! TypeValue.Unify(left, right) |> state.MapContext(fun _ -> ctx)
                    }
                asVar =
                  fun value ->
                    match value with
                    | TypeValue.Var v -> Sum.Left v
                    | _ -> $"Cannot convert {value} to a variable" |> Errors.Singleton |> Sum.Right
                toValue = fun (v: TypeVar) -> TypeValue.Var v })

        }

      // do Console.WriteLine($"Unifying {left} and {right}")
      // do Console.ReadLine() |> ignore

      state {
        match left, right with
        | Primitive p1, Primitive p2 when p1 = p2 -> return ()
        | Lookup l1, Lookup l2 when l1 = l2 -> return ()
        | Lookup l, t2
        | t2, Lookup l ->
          let! t1 = UnificationContext.tryFindType l.Name |> state.OfReader
          return! TypeValue.Unify(t1, t2)
        | Var v, t
        | t, Var v ->
          // let! ctx = state.GetContext()
          // let! s = state.GetState()
          // do Console.WriteLine($"Binding variable {v} to type {t}")
          // do Console.WriteLine($"Context = {ctx.ToFSharpString} and state = {s.ToFSharpString}")
          // do Console.ReadLine() |> ignore
          return! bind (v, t)
        | Lambda(p1, t1), Lambda(p2, t2) ->
          if p1.Kind <> p2.Kind then
            return!
              $"Cannot unify type parameters: {p1} and {p2}"
              |> Errors.Singleton
              |> state.Throw
          else
            let! ctx = state.GetContext()
            let! s = state.GetState()

            let! ctx, ctx1, ctx2 =
              state {

                if p1.Kind = Kind.Star then
                  let v1 = p1.Name |> TypeVar.Create
                  let v2 = p2.Name |> TypeVar.Create
                  do! bind (v1, v2 |> TypeValue.Var)
                  let v1 = TypeValue.Var v1
                  let v2 = TypeValue.Var v2

                  ctx
                  |> UnificationContext.Updaters.Bindings(Map.add p1.Name v1 >> Map.add p2.Name v2),
                  ctx |> UnificationContext.Updaters.Bindings(Map.add p1.Name v1),
                  ctx |> UnificationContext.Updaters.Bindings(Map.add p2.Name v2)
                else
                  let s1 = TypeSymbol.Create p1.Name
                  let s2 = TypeSymbol.Create p2.Name

                  ctx
                  |> UnificationContext.Updaters.Symbols(Map.add p1.Name s1 >> Map.add p2.Name s2),
                  ctx |> UnificationContext.Updaters.Symbols(Map.add p1.Name s1),
                  ctx |> UnificationContext.Updaters.Symbols(Map.add p2.Name s2)
              }

            let! v1 =
              t1
              |> TypeExpr.AsValue
                (UnificationContext.tryFindType >> ReaderWithError.Run ctx1)
                (UnificationContext.tryFindSymbol >> ReaderWithError.Run ctx1)
              |> state.OfSum

            let! v2 =
              t2
              |> TypeExpr.AsValue
                (UnificationContext.tryFindType >> ReaderWithError.Run ctx2)
                (UnificationContext.tryFindSymbol >> ReaderWithError.Run ctx2)
              |> state.OfSum

            // do Console.WriteLine($"Unifying lambda types: {v1} and {v2}")
            // do Console.ReadLine() |> ignore

            do! TypeValue.Unify(v1, v2) |> state.MapContext(replaceWith ctx)
            do! state.SetState(replaceWith s)
        | Arrow(l1, r1), Arrow(l2, r2)
        | Map(l1, r1), Map(l2, r2) ->
          do! TypeValue.Unify(l1, l2)
          do! TypeValue.Unify(r1, r2)
        | List(e1), List(e2)
        | Set(e1), Set(e2) -> do! TypeValue.Unify(e1, e2)
        | Tuple(e1), Tuple(e2)
        | Sum(e1), Sum(e2) when List.length e1 = List.length e2 ->
          for (v1, v2) in List.zip e1 e2 do
            do! TypeValue.Unify(v1, v2)
        | Record(e1), Record(e2)
        | Union(e1), Union(e2) when Map.count e1 = Map.count e2 ->
          for (k1, v1) in e1 |> Map.toSeq do
            let! v2 = e2 |> Map.tryFindWithError k1 "record" k1.Name |> state.OfSum
            do! TypeValue.Unify(v1, v2)
        | _ -> return! $"Cannot unify types: {left} and {right}" |> Errors.Singleton |> state.Throw
      }
