namespace IDEApi.Controllers

open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Logging

open Ballerina.IDE
open Ballerina.Collections.Sum
open IDEApi

open System
open System.Threading.Tasks

type SpecRequest = { SpecBody: string }
type SaveSpecRequest = { Name: string; SpecBody: string }
type SpecValidationResult = { IsValid: bool; Errors: string }

[<ApiController>]
[<Route("[controller]")>]
type SpecController (_logger : ILogger<SpecController>) =
  inherit ControllerBase()

  let createResponseBody (sum: Sum<unit, _>) =
    {
      IsValid = sum.IsLeft
      Errors = 
        match sum with
        | Left _ -> String.Empty
        | Right msg -> msg
    }

  [<HttpPost("validate")>]
  member _.Validate([<FromBody>] req: SpecRequest) =
    req.SpecBody
    |> Parser.validate 
    |> createResponseBody
    |> ActionResult<SpecValidationResult>
      
  // [<HttpPost("lock")>]
  // member _.Lock([<FromBody>] req: SpecRequest) : Task<IActionResult> =
  //   ActionResult<unit>(Storage.lockSpec req.SpecBody)
  //   
    
  [<HttpPost("save")>]
  member _.Save([<FromBody>] req: SaveSpecRequest) =
    task {
      do! Storage.Entity.save req.SpecBody req.Name
      return ActionResult<bool>(true)
    }