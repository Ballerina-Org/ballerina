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
  open Ballerina.DSL.Next.Types.TypeChecker

  let inline private (>>=) f g = fun x -> state.Bind(f x, g)

  type EntityDescriptor<'T, 'Id, 'ValueExt when 'Id: comparison> with
    static member Updaters =
      {| Type = fun u (c: EntityDescriptor<'T, 'Id, 'ValueExt>) -> { c with Type = c.Type |> u }
         Methods = fun u (c: EntityDescriptor<'T, 'Id, 'ValueExt>) -> { c with Methods = c.Methods |> u } |}

  type Schema<'T, 'Id, 've when 'Id: comparison> with
    static member CreateTypeContext<'ValueExt when 'ValueExt: comparison>
      (tc: TypeChecker<Expr<TypeExpr<'ValueExt>, Identifier, 'ValueExt>, 'ValueExt>)
      (schema: Schema<TypeExpr<'ValueExt>, Identifier, 'ValueExt>)
      : State<
          OrderedMap<Identifier, TypeValue<'ValueExt> * Kind>,
          TypeCheckContext<'ValueExt>,
          TypeCheckState<'ValueExt>,
          Errors
         >
      =
      schema.Types
      |> OrderedMap.map (fun identifier typeExpr ->
        state {
          let eval = TypeExpr.Eval()
          let! tv, kind = eval tc None Location.Unknown typeExpr
          do! TypeCheckState.bindType (identifier |> TypeCheckScope.Empty.Resolve) (tv, kind)
          return tv, kind
        })
      |> state.AllMapOrdered

    static member SchemaEval
      ()
      : Schema<TypeExpr<'ValueExt>, Identifier, 'ValueExt>
          -> State<
            Schema<TypeValue<'ValueExt>, ResolvedIdentifier, 'ValueExt>,
            TypeCheckContext<'ValueExt>,
            TypeCheckState<'ValueExt>,
            Errors
           >
      =
      fun schema ->
        state {
          let tc = Expr.TypeCheck()
          let! types = Schema.CreateTypeContext<'ValueExt> tc schema
          let eval = TypeExpr.Eval()

          let! entities =
            schema.Entities
            |> Map.map (fun _ v ->
              state {
                let! typeVal, typeValK = v.Type |> eval tc None Location.Unknown

                do!
                  typeValK
                  |> Kind.AsStar
                  |> Sum.mapRight (Errors.FromErrors Location.Unknown)
                  |> state.OfSum
                  |> state.Ignore

                let eval = Expr.TypeEval()

                let! updaters =
                  v.Updaters
                  |> Seq.map (fun u ->
                    state {
                      let! condition = u.Condition |> eval tc Location.Unknown
                      let! expr = u.Expr |> eval tc Location.Unknown

                      return
                        { Path = u.Path
                          Condition = condition
                          Expr = expr }
                    })
                  |> state.All

                let! predicates = v.Predicates |> Map.map (fun _ -> eval tc Location.Unknown) |> state.AllMap

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
