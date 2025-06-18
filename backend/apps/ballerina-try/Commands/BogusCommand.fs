namespace TryBallerina.Interactive

open System.Threading.Tasks

open Microsoft.DotNet.Interactive
open Microsoft.DotNet.Interactive.Commands
open Microsoft.DotNet.Interactive.Directives

type BogusCommand() = 

  inherit KernelDirectiveCommand()
    member val Lang: string = null with get, set

    static member attachTo (kernel: Kernel) =
      let handler (command: BogusCommand) (_context: KernelInvocationContext): Task =
       let fsharp = kernel.FindKernelByName "fsharp"
       task {
         let! _ = fsharp.SubmitCodeAsync $"""let faker = Faker "{command.Lang.ToLowerInvariant()}" """
         return ()
       }
      let cmd = KernelActionDirective("#!faker")
        
      cmd.Parameters.Add(KernelDirectiveParameter("--lang", "culture in which Bogus will created fakers", Required = true))

      kernel.AddDirective<BogusCommand>(cmd, handler)

