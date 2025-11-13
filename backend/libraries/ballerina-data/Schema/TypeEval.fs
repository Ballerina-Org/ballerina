namespace Ballerina.Data

open Ballerina.Data.Schema.Model

module TypeEval =

  open Ballerina.Collections.Sum
  open Ballerina.State.WithError
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Types.Patterns
  open Ballerina.DSL.Next.Types.TypeChecker.Eval
  open Ballerina.DSL.Next.Types.TypeChecker.Model
  open Ballerina.DSL.Next.Types.TypeChecker.Patterns
  open Ballerina.Errors
  open Ballerina.DSL.Next.Terms.Model
  open Ballerina.DSL.Next.Terms.TypeEval
  open Ballerina.LocalizedErrors
  open Ballerina.StdLib.OrderPreservingMap
  open Ballerina.Cat.Collections.OrderedMap

  let inline private (>>=) f g = fun x -> state.Bind(f x, g)

  type EntityDescriptor<'T, 'Id, 'ValueExt when 'Id: comparison> with
    static member Updaters =
      {| Type = fun u (c: EntityDescriptor<'T, 'Id, 'ValueExt>) -> { c with Type = c.Type |> u }
         Methods = fun u (c: EntityDescriptor<'T, 'Id, 'ValueExt>) -> { c with Methods = c.Methods |> u } |}

  type Schema<'T, 'Id, 'ValueExt when 'Id: comparison> with
    static member CreateTypeContext
      (schema: Schema<TypeExpr, Identifier, 'ValueExt>)
      : State<OrderedMap<Identifier, TypeValue * Kind>, TypeExprEvalContext, TypeExprEvalState, Errors> =
      schema.Types
      |> OrderedMap.map (fun identifier typeExpr ->
        state {

          let! tv, kind = TypeExpr.Eval None Location.Unknown typeExpr
          do! TypeExprEvalState.bindType (identifier |> TypeCheckScope.Empty.Resolve) (tv, kind)
          return tv, kind
        })
      |> state.AllMapOrdered

    static member SchemaEval
      : Schema<TypeExpr, Identifier, 'ValueExt>
          -> State<Schema<TypeValue, ResolvedIdentifier, 'ValueExt>, TypeExprEvalContext, TypeExprEvalState, Errors> =
      fun schema ->
        state {
          let! types = Schema.CreateTypeContext schema

          let! entities =
            schema.Entities
            |> Map.map (fun _ v ->
              state {
                let! typeVal, typeValK = v.Type |> TypeExpr.Eval None Location.Unknown

                do!
                  typeValK
                  |> Kind.AsStar
                  |> Sum.mapRight (Errors.FromErrors Location.Unknown)
                  |> state.OfSum
                  |> state.Ignore

                let! updaters =
                  v.Updaters
                  |> Seq.map (fun u ->
                    state {
                      let! condition = u.Condition |> Expr.TypeEval Location.Unknown
                      let! expr = u.Expr |> Expr.TypeEval Location.Unknown

                      return
                        { Path = u.Path
                          Condition = condition
                          Expr = expr }
                    })
                  |> state.All

                let! predicates =
                  v.Predicates
                  |> Map.map (fun _ -> Expr.TypeEval Location.Unknown)
                  |> state.AllMap

                return
                  { Type = typeVal
                    Methods = v.Methods
                    Updaters = updaters
                    Predicates = predicates }
              })
            |> state.AllMap

          return
            { Types = types |> OrderedMap.map (fun _k -> fst)
              Entities = entities
              Lookups = schema.Lookups }
        }
