namespace TryBallerina.Interactive

open System
open System.Linq
open System.Threading.Tasks

type CodeAnnotation =
  | Raw of string
  | CheatLiteral of name: string * code: string
  | Result

/// <summary>
/// Takes the code as strings and invokes any line before proceeding into another
/// It can cheat the F# language like making literals on demand
/// </summary>
type CheatInteractiveBuilder(debug: Boolean) =
    
    let printfn v = if debug then printfn v
    
    member _.Return(code: CodeAnnotation) : Task<CodeAnnotation> = Task.FromResult code      

    member _.Bind(value: CodeAnnotation, binder: CodeAnnotation -> Task<CodeAnnotation>) : Task<CodeAnnotation> =
        task {
            match value with
            | Result -> return Result
            | Raw code ->
                printfn $"Executing raw code: {code}"
                let! x = Languages.fsharp code
                printfn $"""Result of execution: {x.Events.ToList() |> Seq.fold (fun acc ev -> acc + ev.GetType().Name + ", ") ""}"""
                return! binder (Raw code)

            | CheatLiteral (name, code) ->
                printfn $"Executing cheat raw code: {code}"
                let! x = Languages.fsharp $"""let {name} = {code}"""
                printfn $"""Result of execution: {x.Events.ToList() |> Seq.fold (fun acc ev -> acc + ev.GetType().Name + ", ") ""}"""

                let! valueStr = Languages.FSharp.requestStringValue name
                match valueStr with
                | None -> 
                    printfn $"Failed to get value for {name}"
                    return Result
                | Some valueStr ->
                
                    let code = $"""[<Literal>]{Environment.NewLine}let {name} = {valueStr.trippleQuote()}"""
                    printfn $"Executing cheat lit code: {code}"
                    let! x = Languages.fsharp code
                    
                    printfn $"""Result of execution: {x.Events.ToList() |> Seq.fold (fun acc ev -> acc + ev.GetType().Name + ", ") ""}"""
                    return! binder (CheatLiteral(name, code))
        }
        
    member _.ReturnFrom(t: Task<string>) = t