namespace TryBallerina.Interactive

module Dependencies =
    let private nugets = 
        [
            "FSharp.Data"
            "FSharp.Data.LiteralProviders"
            "FSharp.Management.Fork"
            // ▽ make sure to use the same versions as are in the project dependencies
            "Microsoft.DotNet.Interactive, 1.0.0-beta.25177.1"
            "Microsoft.DotNet.Interactive.FSharp, 1.0.0-beta.25177.1"
            "BallerinaCore"
            "Bogus"
        ]

    let private usings =
        [
            "FSharp.Data"
            "FSharp.Data.LiteralProviders"
            "Microsoft.DotNet.Interactive.Commands"
            "Microsoft.DotNet.Interactive"
            "Microsoft.DotNet.Interactive.Formatting"
            "Microsoft.DotNet.Interactive.FSharp"
            "TryBallerina.Interactive"
            "TryBallerina.Interactive.BuildersRunner"
            "TryBallerina.Interactive.FileStore"
            "FSharp.Management"
            
            "Ballerina.DSL.Expr"
            "Ballerina.DSL.Expr.Model"
            "Ballerina.Collections.Sum"
            "Ballerina.DSL.FormEngine.Model"
            "Ballerina.DSL.Expr.Types.Model"
            "Ballerina"
            "Ballerina.IDE"
            "Bogus"
        ]

    let packages =
        ("", nugets) 
        ||> List.fold (fun acc item -> $"""{acc}#r "nuget:{item}"{System.Environment.NewLine}""") 
    let ``open`` =
        ("", usings)
        ||> List.fold (fun acc item -> $"""{acc}open {item}{System.Environment.NewLine}""") 
    
    let load () =
      task {
          let! _ = Languages.fsharp packages
          let! _ = Languages.fsharp ``open``
          let! _ = Languages.fsharp """let faker = Faker "en" """
          return ()
      }