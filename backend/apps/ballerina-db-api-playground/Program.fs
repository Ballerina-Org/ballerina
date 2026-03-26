open System
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.Hosting
open Ballerina.API.MemoryDB.API
open Microsoft.Extensions.DependencyInjection
open System.Text.Json.Serialization
open Ballerina.API.MemoryDB.MemoryDBAPIFactory
open Ballerina.API.MemoryDB.Model
open Ballerina.Collections.Sum
open System.CommandLine
open Ballerina.API.MemoryDB.API
open System.IO
open Ballerina.Errors
open Ballerina
open Ballerina.LocalizedErrors
open Ballerina.FileDBAPIPlayground.Factory
open Ballerina.DSL.Next.Types
open Ballerina.DSL.Next.Terms

[<EntryPoint>]
let main args =
  let r = new Random()

  createAndRunAPI
    (Map.add
      (ResolvedIdentifier.Create("InjectedPermissionsContext", "CurrentUserCanCreate"))
      ((TypeValue.CreateBool(), Kind.Star), Value.Primitive(PrimitiveValue.Unit)))
    id
    (ExprEvalContext.Updaters.Values(
      Map.add
        (ResolvedIdentifier.Create("InjectedPermissionsContext", "CurrentUserCanCreate"))
        (Value.Primitive(PrimitiveValue.Bool(r.NextDouble() > 0.5)))
    ))
    args
