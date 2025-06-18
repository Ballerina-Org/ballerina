namespace TryBallerina.Interactive

open System.IO
open System.Runtime.CompilerServices
    
type UtilsExtensions() =
     
    [<Extension>]
    static member trippleQuote(raw: string) =
        $"\"\"\"{raw}\"\"\""
        
module Languages =

  open Microsoft.DotNet.Interactive
  open Microsoft.DotNet.Interactive.Commands
  open Microsoft.DotNet.Interactive.Events
  
  open System.Linq
  
  let fsharp code = 
      Kernel.Root.SendAsync(SubmitCode(code ,"fsharp"))
      
  let custom kernel code = 
      Kernel.Root.SendAsync(SubmitCode(code ,kernel))
      
  let csharp code = 
      Kernel.Root.SendAsync(SubmitCode(code ,"csharp"))
  
  let fsharpFromCSharp code =
      let code = $"""
#!fsharp
{code}
"""
      Kernel.Root.SendAsync(SubmitCode(code ,"csharp"))

  let python code =
      Kernel.Root.SendAsync(SubmitCode(code ,"pythonkernel"))
  
  // defering code is necessary when we execute code not from the notebook cell, but from the library that is used in the notebook
  // it is some limitation of the current implementation of the .NET Interactive
      
  let fsharpDefer code = 
      SubmitCode(code, "fsharp")
      |> Kernel.Current.DeferCommand    

  let pythonDefer code =
      SubmitCode(code ,"pythonkernel")|> Kernel.Current.DeferCommand
      
  module FSharp =
    let requestStringValue (name: string) =
      //TODO: enwrap messages with a Sum<_,_> for better error handling
      task {
          try
              let! requestedValue = 
                  RequestValue(name, "text/plain", "fsharp")
                  |> Kernel.Root.SendAsync
                  
              if requestedValue.Events.ToList() |> Seq.exists (fun t -> t.GetType().Name = "ValueProduced") then
                  let event = 
                      requestedValue.Events.First(fun x -> x.GetType() = typeof<ValueProduced>) 
                  match event with
                  | null -> return None
                  | _evt ->
                      let valueProduced = event :?> ValueProduced
                      return Some valueProduced.FormattedValue.Value
              else 
                  return None
          with
          | _ex -> return None
      }
      
[<AutoOpen>]
module LanguagesAutoOpen =
  let makeStrong (path: string) =
    let name = Path.GetFileNameWithoutExtension path |> _.Replace("-","_").Replace("'","").Replace(".","_").Replace(" ","")
    let code = $"""
let {name} = JsonProvider<{path.trippleQuote()}, InferTypesFromValues = false>.GetSample(){System.Environment.NewLine}
let {name}_content = File.ReadAllText({path.trippleQuote()})"""
    //printfn $"{code}"
    code  |> Languages.fsharpDefer
