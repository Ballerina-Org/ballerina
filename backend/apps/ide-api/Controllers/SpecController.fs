namespace IDEApi.Controllers

open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Logging

open Ballerina.IDE

type SpecRequest = { specBody: string }
type SpecValidationResult = { specName: string; isValid: bool }

[<ApiController>]
[<Route("[controller]")>]
type SpecController (_logger : ILogger<SpecController>) =
    inherit ControllerBase()

    [<HttpPost("validate")>]
    member _.Validate([<FromBody>] req: SpecRequest) =
        let res = Validator.parseAndValidate req.specBody
        { isValid = res.IsLeft; specName = "" }
        |> ActionResult<SpecValidationResult> 