namespace Ballerina.Data

open Ballerina.Data.Schema.Model

module TypeEval =

  open Ballerina.State.WithError
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Types.Patterns
  open Ballerina.DSL.Next.Types.Eval
  open Ballerina.Errors
  open Ballerina.DSL.Next.Terms.Model
  open Ballerina.DSL.Next.Terms.TypeEval

  let inline private (>>=) f g = fun x -> state.Bind(f x, g)

  type EntityDescriptor<'T> with
    static member Updaters =
      {| Type = fun u (c: EntityDescriptor<'T>) -> { c with Type = c.Type |> u }
         Methods = fun u (c: EntityDescriptor<'T>) -> { c with Methods = c.Methods |> u } |}

  type Schema<'T> with
    static member SchemaEval
      : Schema<TypeExpr> -> State<Schema<TypeValue>, TypeExprEvalContext, TypeExprEvalState, Errors> =
      fun schema ->
        state {
          let! entities =
            schema.Entities
            |> Map.map (fun _ v ->
              state {
                let! typeVal, typeValK = v.Type |> TypeExpr.Eval
                do! typeValK |> Kind.AsStar |> state.OfSum |> state.Ignore

                let! updaters =
                  v.Updaters
                  |> Seq.map (fun u ->
                    state {
                      let! condition = u.Condition |> Expr.TypeEval
                      let! expr = u.Expr |> Expr.TypeEval

                      return
                        { Path = u.Path
                          Condition = condition
                          Expr = expr }
                    })
                  |> state.All

                let! predicates = v.Predicates |> Map.map (fun _ -> Expr.TypeEval) |> state.AllMap

                return
                  { Type = typeVal
                    Methods = v.Methods
                    Updaters = updaters
                    Predicates = predicates }
              })
            |> state.AllMap

          return
            { Entities = entities
              Lookups = schema.Lookups }
        }
