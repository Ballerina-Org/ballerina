open System
open Ballerina.Collections.Map
open Ballerina.FileDBAPIPlayground.Factory
open Ballerina.DSL.Next.Types
open Ballerina.DSL.Next.Terms

[<EntryPoint>]
let main args =
  let domain_name = "DOMAIN_NAME"

  createAndRunAPI
    (Map.add
      (ResolvedIdentifier.Create(domain_name, "IsAuthorized"))
      (TypeValue.CreatePrimitive PrimitiveType.Bool, Kind.Star))
    id
    (fun _ ->
      let values =
        [ ResolvedIdentifier.Create(domain_name, "IsAuthorized"), Value.Primitive(PrimitiveValue.Bool true) ]
        |> Map.ofList

      ExprEvalContext.Updaters.Values(Map.merge (fun _ -> id) values))
    args
