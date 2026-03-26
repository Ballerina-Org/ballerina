open System
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.Hosting
open Ballerina.API.MemoryDB.API
open Microsoft.Extensions.DependencyInjection
open System.Text.Json.Serialization
open Ballerina.API.MemoryDB.MemoryDBAPIFactory
open Ballerina.API.MemoryDB.Model
open Ballerina.Collections.Map
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
  // TODO: move this to the BISE repo
  // TODO: define a translation utility that turns Sum5 -> Session, then call the regular permission functions
  // TODO: return proper errors from the command line, with position in the original file
  let domain_name = "ORGANIZATION"

  createAndRunAPI
    (Map.add
      (ResolvedIdentifier.Create(domain_name, "CurrentUser"))
      (TypeValue.CreateSum
        [ TypeValue.CreatePrimitive PrimitiveType.Unit
          TypeValue.CreatePrimitive PrimitiveType.Guid ],
       Kind.Star)
     >> Map.add
       (ResolvedIdentifier.Create(domain_name, "CurrentOwner"))
       (TypeValue.CreateSum
         [ TypeValue.CreatePrimitive PrimitiveType.Unit
           TypeValue.CreatePrimitive PrimitiveType.Guid ],
        Kind.Star)
     >> Map.add
       (ResolvedIdentifier.Create(domain_name, "CurrentManager"))
       (TypeValue.CreateSum
         [ TypeValue.CreatePrimitive PrimitiveType.Unit
           TypeValue.CreatePrimitive PrimitiveType.Guid ],
        Kind.Star)
     >> Map.add
       (ResolvedIdentifier.Create(domain_name, "CurrentApiToken"))
       (TypeValue.CreateSum
         [ TypeValue.CreatePrimitive PrimitiveType.Unit
           TypeValue.CreatePrimitive PrimitiveType.Guid ],
        Kind.Star))
    id
    (fun httpContext ->
      let values =
        [ ResolvedIdentifier.Create(domain_name, "CurrentUser"),
          Value.Sum({ Case = 2; Count = 2 }, Value.Primitive(PrimitiveValue.Unit))
          ResolvedIdentifier.Create(domain_name, "CurrentOwner"),
          Value.Sum({ Case = 2; Count = 2 }, Value.Primitive(PrimitiveValue.Unit))
          ResolvedIdentifier.Create(domain_name, "CurrentManager"),
          Value.Sum({ Case = 2; Count = 2 }, Value.Primitive(PrimitiveValue.Unit))
          ResolvedIdentifier.Create(domain_name, "CurrentApiToken"),
          Value.Sum({ Case = 2; Count = 2 }, Value.Primitive(PrimitiveValue.Unit)) ]
        |> Map.ofList

      let values =
        httpContext.Request.Headers.["X-User-Id"]
        |> Guid.TryParse
        |> function
          | true, guid ->
            values
            |> Map.add
              (ResolvedIdentifier.Create(domain_name, "CurrentUser"))
              (Value.Sum({ Case = 1; Count = 2 }, Value.Primitive(PrimitiveValue.Guid(guid))))
          | _ ->
            httpContext.Request.Headers.["X-Owner-Id"]
            |> Guid.TryParse
            |> function
              | true, guid ->
                values
                |> Map.add
                  (ResolvedIdentifier.Create(domain_name, "CurrentOwner"))
                  (Value.Sum({ Case = 1; Count = 2 }, Value.Primitive(PrimitiveValue.Guid(guid))))
              | _ ->
                httpContext.Request.Headers.["X-Manager-Id"]
                |> Guid.TryParse
                |> function
                  | true, guid ->
                    values
                    |> Map.add
                      (ResolvedIdentifier.Create(domain_name, "CurrentManager"))
                      (Value.Sum({ Case = 1; Count = 2 }, Value.Primitive(PrimitiveValue.Guid(guid))))
                  | _ ->
                    httpContext.Request.Headers.["X-Api-Token"]
                    |> Guid.TryParse
                    |> function
                      | true, guid ->
                        values
                        |> Map.add
                          (ResolvedIdentifier.Create(domain_name, "CurrentApiToken"))
                          (Value.Sum({ Case = 1; Count = 2 }, Value.Primitive(PrimitiveValue.Guid(guid))))
                      | _ -> values


      ExprEvalContext.Updaters.Values(Map.merge (fun _ -> id) values))
    args
