namespace IDEApi.Controllers

open System.Linq
open Microsoft.AspNetCore.Mvc
open FSharp.Data
open System.Text.Json.Nodes
open Ballerina.DSL.Expr.Types
open Ballerina.Collections.Option
open IDEApi
open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Logging

open Ballerina.IDE
open Ballerina.Collections.Sum

    
[<ApiController>]
[<Route("[controller]")>]
type EnumController (_logger : ILogger<EnumController>) =
  inherit ControllerBase()

  [<HttpGet>]
  member this.get(specName: string) =
    task {
      let! spec = Storage.Entity.get specName
      let op =
        sum {
          let! spec = spec |> Sum.fromOption (fun () -> $"{specName} is not found in storage")
          let! _mergedJson, parsedForms = Parser.parse spec
          let enums = parsedForms.Value.Apis.Enums |> Map.keys |> _.ToArray()
          
          return 
            enums
            |> Array.choose ( fun e ->
              option {
                let typeContext = parsedForms.Value.Types
                let! key = parsedForms.Value.Apis.Enums |> Map.tryFindKey (fun k _ -> k.ToLowerInvariant() = e.ToLowerInvariant().Replace("fields",""))
                let enumApi = parsedForms.Value.Apis.Enums[key]
                let typeBinding = typeContext[enumApi.TypeId.TypeName]
                return key, RandomSeeder.traverse typeContext (JsonValue.Null) enumApi.UnderlyingEnum.TypeName typeBinding.Type
              }
             )
            |> Array.map ( fun (name, json) -> JsonValue.Record [| name, json |])
            |> List.ofArray |> RandomSeeder.mergeJsonList
        }
       
      match op with
      | Left result -> return this.Content(result.ToString(), """application/json""") :> IActionResult
      | Right error -> return this.StatusCode(500, $"Internal Server Error:{error}") :> IActionResult
    }
