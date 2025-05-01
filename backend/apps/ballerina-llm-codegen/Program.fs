module BallerinaLlmCodegen =
  open Ballerina.Codegen.Python.Generator.Main
  open Ballerina.Collections.Sum    
  open Ballerina.Core.StringBuilder 
  open Ballerina.Errors             
  open Ballerina.DSL.Expr.Types.Model

  [<EntryPoint>]
  let main argv =
    let pythonResult =
      Generator.ToPython()

    match pythonResult with
    | Left sb   -> printfn "Success! got: %A" sb
    | Right err -> printfn "Failed with error: %A" err

    0
