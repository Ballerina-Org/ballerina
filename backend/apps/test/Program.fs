namespace grandeomega2

#nowarn "20"

open System
open System.CommandLine
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Options
open Ballerina.Coroutines
open Ballerina.CRUD
open Migrations
open Microsoft.EntityFrameworkCore
open Microsoft.AspNetCore.Http.Json
open System.Text.Json.Serialization
open absample.repositories
open absample.endpoints
open positions.model
open FSharp.SystemTextJson.Swagger

module Program =
  open abcdsample.eventLoop
  open absample.eventLoop
  open Oauth.Mocked
  open Oauth.MSGraph
  open Oauth.Spotify

  type PositionOptions() =
    member val Title: string = "" with get, set
    member val Name: string = "" with get, set
    static member Position = "Position"
    override p.ToString() = $"title={p.Title} name={p.Name}"

  // let jsonSerializer = FsPickler.CreateJsonSerializer(indent = false)
  // let text = jsonSerializer.PickleToString initialEvals
  // File.WriteAllText("evals.json", text)
  //   let evals = jsonSerializer.UnPickleOfString<EvaluatedCoroutines<{| counter:int |},Unit>>(File.ReadAllText("evals.json"))
  //   ...
  //   let text = jsonSerializer.PickleToString evals'
  //   File.WriteAllText("evals.json", text)

  let exitCode = 0

  type LaunchMode =
    | none = 0
    | web = 1
    | jobs = 2
    | abcdjobs = 3
    | mocked = 4
    | msgraph = 5
    | spotify = 6
    | testunions = 7

  type MyBool =
    | True
    | False

  type EFOrError =
    | Inl of EF
    | Inr of string

  [<EntryPoint>]
  let main args =
    let builder = WebApplication.CreateBuilder(args)

    builder.Services.Configure<PositionOptions>(
      builder.Configuration.GetSection(PositionOptions.Position)
    )

    let fsOptions = JsonFSharpOptions() // setup options here

    builder.Services.Configure<JsonOptions>(fun (options: JsonOptions) ->
      options.SerializerOptions.Converters.Add(new JsonStringEnumConverter())
      options.SerializerOptions.Converters.Add(JsonFSharpConverter(fsOptions)))

    builder.Services.AddSwaggerForSystemTextJson(fsOptions)

    builder.Services.AddDbContext<BallerinaContext>(fun opt ->
      opt.UseNpgsql(builder.Configuration.GetConnectionString("DbConnection"))
      |> ignore)

    builder.Services.AddEndpointsApiExplorer()
    // .AddSwaggerGen(fun options ->
    //     options.UseOneOfForPolymorphism()
    //     options.SelectDiscriminatorNameUsing(fun _ -> "$type")
    //   )

    // app.UseHttpsRedirection()
    let app = builder.Build()
    app.UseSwagger().UseSwaggerUI()

    let testunions () =
      app
        .MapPost(
          "/TestUnions",
          new Func<MyBool, EFOrError>(fun input ->
            let r = System.Random()

            if input.IsTrue then
              Inl
                { EFId = Guid.CreateVersion7()
                  E = r.Next() % 10
                  F = r.Next() % 10 }
            else
              Inr "Greetings!")
        )
        .WithOpenApi()
      |> ignore

      app.Run("http://localhost:5000")

    let web () =
      app
        .UseABSample<BallerinaContext>(
          (fun db -> abrepos.ef.AB db db.ABs),
          (fun db -> abrepos.ef.ABEvent db db.ABEvents),
          (fun db -> abrepos.ef.AEvent db db.AEvents),
          (fun db -> abrepos.ef.BEvent db db.BEvents)
        )
        .UseSwagger()
        .UseSwaggerUI()

      app.Run("http://localhost:5000")

    //ms graph arguments
    let msTenantArg =
      new Option<Guid>(name = "tenant", description = "Tenant Id")

    let msClientArg =
      new Option<Guid>(name = "client", description = "MS Entra Client Id")

    let msSecretArg =
      new Option<string>(
        name = "secret",
        description = "MS Entra Application secret"
      )

    //spotify arguments
    let spotifyClientArg =
      new Option<string>(name = "client", description = "Spotify Client Id")

    let spotifySecretArg =
      new Option<string>(name = "secret", description = "Spotify Secret")

    let spotifyAuthorizationCode =
      new Option<string>(
        name = "code",
        description = "Authorization code obtained after user consent."
      )

    let abCommand =
      new Command(name = "web", description = "Run the AB sample on localhost.")

    let jobsCommand =
      new Command(name = "jobs", description = "Run the AB loop sample.")

    let abcdCommand =
      new Command(name = "abcd", description = "Run the ABCD loop sample.")

    let unionsCommad =
      new Command(
        name = "test-unions",
        description = "Sample for discriminated unions."
      )

    let mockedOauthCommand =
      new Command(
        name = "oauth-mocked",
        description = "Run the OAuth sample with a mocked API."
      )

    let msGraphOauthCommand =
      new Command(
        name = "oauth-ms-graph",
        description =
          "Run the oauth sample using the application permissions flow. 
        Queries the ms-graph user api from a tenant. Pass the tenant id, client id, and client secret as arguments."
      )

    msGraphOauthCommand.AddOption(msTenantArg)
    msGraphOauthCommand.AddOption(msClientArg)
    msGraphOauthCommand.AddOption(msSecretArg)

    let spotifyOauthCommand =
      new Command(
        name = "oauth-spotify",
        description =
          "Run the oauth sample using the delegated permissions flow. Pass the client id, client secret, and authorization code 
      as arguments"
      )

    spotifyOauthCommand.AddOption(spotifyClientArg)
    spotifyOauthCommand.AddOption(spotifySecretArg)
    spotifyOauthCommand.AddOption(spotifyAuthorizationCode)

    let rootCommand = new RootCommand("Sample app for System.CommandLine")

    rootCommand.Add(abCommand)
    rootCommand.Add(jobsCommand)
    rootCommand.Add(abcdCommand)
    rootCommand.Add(unionsCommad)
    rootCommand.Add(mockedOauthCommand)
    rootCommand.Add(msGraphOauthCommand)
    rootCommand.Add(spotifyOauthCommand)

    abCommand.SetHandler(fun () -> web ())
    jobsCommand.SetHandler(fun () -> abEventLoop (app.Services.CreateScope))
    abcdCommand.SetHandler(fun () -> abcdEventLoop ())
    unionsCommad.SetHandler(fun () -> testunions ())
    mockedOauthCommand.SetHandler(fun () -> oauthEventLoop ())

    msGraphOauthCommand.SetHandler(
      (fun tenant clientId clientSecret ->
        if
          tenant = Unchecked.defaultof<Guid>
          || clientId = Unchecked.defaultof<Guid>
          || String.IsNullOrEmpty clientSecret
        then
          let message =
            "Invalid parameters supplied\nREQUIRED:\ntenant: Guid, client: Guid, secret: string"
            + "\nSee https://learn.microsoft.com/en-us/entra/identity-platform/quickstart-configure-app-access-web-apis to setup up app permissions in your tenant."

          Console.WriteLine message
        else
          msGraphEventLoop tenant clientId clientSecret),
      msTenantArg,
      msClientArg,
      msSecretArg
    )

    spotifyOauthCommand.SetHandler(
      (fun clientId clientSecret authorizationCode ->
        if
          String.IsNullOrEmpty clientId
          || String.IsNullOrEmpty clientSecret
          || String.IsNullOrEmpty authorizationCode
        then
          let message =
            "Invalid parameters supplied\nREQUIRED:\nclient: string, secret: string, code: string"
            + "\nSee https://developer.spotify.com/documentation/web-api to setup an API for spotify."
            + "\nYou can obtain the authorization code using"
            + "\nhttps://accounts.spotify.com/authorize?client_id=<api_client_id>&response_type=code&redirect_uri=http://localhost:5000"
            + "\nto redirect to your localhost. After the browser times out, you will see the authorization code appended to the URL."
            + "\nA full smooth implementation requires a front end application that forwards the code to your backend."

          Console.WriteLine message
        else
          spotifyEventLoop clientId clientSecret authorizationCode),
      spotifyClientArg,
      spotifySecretArg,
      spotifyAuthorizationCode
    )

    do rootCommand.Invoke(args) |> ignore

    exitCode
