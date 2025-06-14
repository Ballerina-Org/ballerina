namespace IDEApi.Controllers

open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Logging

open Ballerina.IDE
open Ballerina.Collections.Sum

open System

type SpecRequest = { SpecBody: string }
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
        |> Validator.parseAndValidate 
        |> createResponseBody
        |> ActionResult<SpecValidationResult> 