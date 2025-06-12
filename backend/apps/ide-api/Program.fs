namespace IDEApi
#nowarn "20"

open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting


module Program =
    let exitCode = 0
    let corsPolicyName = "AllowIDEFrontend"
    [<EntryPoint>]
    let main args =

        let builder = WebApplication.CreateBuilder(args)

        builder.Services.AddControllers()
        builder.Services.AddCors(fun options ->
            options.AddPolicy(corsPolicyName, fun builder ->
              builder
                  .AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  |> ignore
            )
        )
        
        let app = builder.Build()

        app.UseHttpsRedirection()
        app.UseCors(corsPolicyName)
        app.UseAuthorization()
        app.MapControllers()

        app.Run()

        exitCode