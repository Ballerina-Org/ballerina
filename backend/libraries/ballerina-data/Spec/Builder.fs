namespace Ballerina.Data.Spec

module Builder =
  open Ballerina.Errors
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Types.Eval
  open Ballerina.State.WithError
  open Ballerina.Data.Spec.Model

  let typeContextFromSpecBody (spec: SpecFormat) : State<unit list, TypeExprEvalContext, TypeExprEvalState, Errors> =
    spec.TypesV2
    |> List.map (fun (name, expr) ->
      state {
        let! ctx = state.GetState()
        let! tv, kind = TypeExpr.Eval expr

        let nextCtx =
          { ctx with
              Bindings = ctx.Bindings.Add(Identifier.LocalScope name, (tv, kind))
              Symbols =
                ctx.Symbols.Add(
                  Identifier.LocalScope name,
                  { Name = Identifier.LocalScope name
                    Guid = System.Guid.NewGuid() }
                ) }

        do! state.SetState(fun _ -> nextCtx)
        return ()
      })
    |> state.All
