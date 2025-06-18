namespace IDEApi.Controllers

open System.Linq

open IDEApi
open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Logging

open Ballerina.IDE
open Ballerina.Collections.Sum

type EntityResponse = { Value: string  }
    
[<ApiController>]
[<Route("[controller]")>]
type EntityController (_logger : ILogger<EntityController>) =
  inherit ControllerBase()

  [<HttpGet>]
  member _.Get() =  
    let op =
      sum {
        let! spec = Storage.lockedSpec () |> Sum.fromOption (fun () -> "Error: missing a locked spec") //TODO:use Errors.Singleton
        let! _mergedJson, parsedForms = Parser.parse spec
        return { Value = parsedForms.Value.Types.Keys.ToArray()[0] }
      }
      
    {
      Value =
        match op with
        | Left result -> result.Value
        | Right error -> error
    }