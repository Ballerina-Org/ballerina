namespace TryBallerina.Interactive

open System.Threading.Tasks

open Microsoft.DotNet.Interactive
open Microsoft.DotNet.Interactive.Commands
open Microsoft.DotNet.Interactive.Formatting
open Microsoft.AspNetCore.Html

type TryBallerinaKernelExtension () =
      
      let formatHtml (value: HtmlString) =
        FormattedValue(
          HtmlFormatter.MimeType,
          value.ToDisplayString(HtmlFormatter.MimeType))
        
      interface IKernelExtension with 

        member this.OnLoadAsync(kernel: Kernel): Task =
          Formatter.Register(
              typeof<InstallPackagesMessage>,
              FormatDelegate<_>(fun (v: obj) (ctx: FormatContext) ->
                  let m = v :?> InstallPackagesMessage
                  ctx.Writer.Write($"Installed {m.InstalledPackages.Count} packages")
                  true),
              "text/html"
          )
          Formatter.SetPreferredMimeTypesFor(typeof<InstallPackagesMessage>, "text/html")
          
          FormsCommand.attachTo kernel
          BogusCommand.attachTo kernel
          
          task {
            do! Dependencies.load ()
            let _ = ConvenientCodeExecute._code
            let _ = ConvenientCodeExecute._blp
            let! _markup =
                    """<div><img style="width:200px" src ="https://github.com/Ballerina-Org/ballerina/raw/main/docs/pics/Ballerina_logo-04.svg"/><p>Try Ballerina!</p></div>"""
                 
                    |> HtmlString
                    |> formatHtml
                    |> DisplayValue
                    |> Kernel.Root.SendAsync 
            
            return ()
          }