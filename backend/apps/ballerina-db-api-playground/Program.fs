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
    (Map.add (ResolvedIdentifier.Create("InjectedBackgroundContext", "MaxValue")) ((TypeValue.CreateInt32(), Kind.Star))
     >> Map.add (ResolvedIdentifier.Create("InjectedBackgroundContext", "Step")) ((TypeValue.CreateInt32(), Kind.Star)))
    (fun _ ->
      let values =
        [ ResolvedIdentifier.Create(domain_name, "IsAuthorized"), Value.Primitive(PrimitiveValue.Bool true) ]
        |> Map.ofList

      ExprEvalContext.Updaters.Values(Map.merge (fun _ -> id) values))
    (let values =
      [ ResolvedIdentifier.Create("InjectedBackgroundContext", "MaxValue"), Value.Primitive(PrimitiveValue.Int32 10)
        ResolvedIdentifier.Create("InjectedBackgroundContext", "Step"), Value.Primitive(PrimitiveValue.Int32 2) ]
      |> Map.ofList

     ExprEvalContext.Updaters.Values(Map.merge (fun _ -> id) values))
    args
