namespace IDEApi.Controllers

open System.Linq
open Microsoft.AspNetCore.Mvc
open FSharp.Data
open Ballerina.DSL.Expr.Types
open Ballerina.Collections.Option
open IDEApi
open Microsoft.Extensions.Logging

open Ballerina.IDE
open Ballerina.Collections.Sum

type EntityResponse = { Value: string  }
    
[<ApiController>]
[<Route("[controller]")>]
type EntityController (_logger : ILogger<EntityController>) =
  inherit ControllerBase()

  [<HttpPost("seed")>]
  member this.Seed([<FromBody>] req: SpecRequest) =  
    let op =
      sum {
        
        let spec = req.SpecBody
        let! _mergedJson, parsedForms = Parser.parse spec
        let entityNames = parsedForms.Value.Apis.Entities |> Map.keys |> _.ToArray()

        return
          entityNames
          |> Array.choose ( fun entity ->
            option {
              let typeContext = parsedForms.Value.Types
              let! key = parsedForms.Value.Types |> Map.tryFindKey (fun k _ -> k.ToLowerInvariant() = entity.ToLowerInvariant())
              let typeBinding = typeContext[key]
              return entity, RandomSeeder.traverse typeContext (JsonValue.String entity) entity typeBinding.Type
            }

           )
          |> Array.map ( fun (name, json) -> JsonValue.Record [| name, json |])
          |> List.ofArray |> RandomSeeder.mergeJsonList
      }
       
    match op with
    | Left result -> this.Content(result.ToString(), """application/json""") :> IActionResult
    | Right error -> this.StatusCode(500, $"Internal Server Error:{error}") :> IActionResult
    
  [<HttpGet>]
  member this.get(specName: string) =
    task {
      let! spec = Storage.Entity.get specName
      let op =
        sum {
          let! spec = spec |> Sum.fromOption (fun () -> $"{specName} is not found in storage")
          let! _mergedJson, parsedForms = Parser.parse spec
          let entityNames = parsedForms.Value.Apis.Entities |> Map.keys |> _.ToArray()
          
          let test =
            entityNames
            |> Array.choose ( fun entity ->
              option {
                let typeContext = parsedForms.Value.Types
                let! key = parsedForms.Value.Types |> Map.tryFindKey (fun k _ -> k.ToLowerInvariant() = entity.ToLowerInvariant())
                let typeBinding = parsedForms.Value.Types[key]
                return entity, RandomSeeder.traverse typeContext (JsonValue.String entity) entity typeBinding.Type
              }

             )
            |> Array.map ( fun (name, json) -> JsonValue.Record [| name, json |])
          let t = test |> List.ofArray |> RandomSeeder.mergeJsonList
          return t
        }
       
      match op with
      | Left result -> return this.Content(result.ToString(), """application/json""") :> IActionResult
      | Right error -> return this.StatusCode(500, $"Internal Server Error:{error}") :> IActionResult
    }
