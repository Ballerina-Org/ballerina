module OAuth.Spotify.API

open System
open System.Net.Http
open System.Net
open System.Text
open System.Text.Encodings
open System.Text.Json
open System.Text.Json.Serialization
open OAuth.Common

type Authorization =
  | Basic of Id: string * Secret: string
  | Bearer of AccessToken: string

  member this.ToHeader() =
    match this with
    | Basic(id, secret) ->
      let basicAuthentication =
        $"{id}:{secret}" |> (Encoding.UTF8.GetBytes >> Convert.ToBase64String)

      "Authorization", $"Basic {basicAuthentication}"
    | Bearer s -> "Authorization", $"Bearer {s}"


type Album =
  { [<JsonPropertyName("name")>]
    Name: string
    [<JsonPropertyName("release_date")>]
    ReleaseDate: DateTime }

type AlbumPage =
  { [<JsonPropertyName("items")>]
    Items: Album[]
    [<JsonPropertyName("total")>]
    Total: int }

type NewReleases =
  { [<JsonPropertyName("albums")>]
    Albums: AlbumPage }

let performRequest<'a, 'e>
  (url: string)
  (method: HttpMethod)
  (authorization: Authorization)
  (content: Option<HttpContent>)
  (error: 'e)
  =
  task {
    let request = new HttpRequestMessage(method, url)
    let authorizationType, authorizationValue = authorization.ToHeader()
    request.Headers.Add(authorizationType, authorizationValue)

    match content with
    | Some content -> request.Content <- content
    | None -> ()

    let! response = httpClient.SendAsync(request)

    match response.StatusCode with
    | HttpStatusCode.OK ->
      let! resultContent = response.Content.ReadAsStringAsync()
      let deserializedContent = JsonSerializer.Deserialize<'a>(resultContent)
      return Choice2Of2 deserializedContent
    | _ -> return Choice1Of2 error
  }

let requestToken
  (clientId: string)
  (clientSecret: string)
  (authorizationCode: string)
  =
  task {
    let url = "https://accounts.spotify.com/api/token"

    let content =
      new FormUrlEncodedContent(
        Map.empty
          .Add("grant_type", "authorization_code")
          .Add("code", authorizationCode)
          .Add("redirect_uri", "http://localhost:5000")
      )

    let! result =
      performRequest<OAuthResponse, unit>
        url
        HttpMethod.Post
        (Basic(clientId, clientSecret))
        (Some content)
        ()

    match result with
    | Choice2Of2 deserializedContent ->
      return
        Choice2Of2(
          {| AccessToken = deserializedContent.AccessToken
             Duration = deserializedContent.Duration |},
          deserializedContent.RefreshToken
        )
    | _ -> return Choice1Of2()
  }

let refreshToken
  (clientId: string)
  (clientSecret: string)
  (refreshToken: string)
  =
  task {
    let url = "https://accounts.spotify.com/api/token"

    let content =
      new FormUrlEncodedContent(
        Map.empty
          .Add("grant_type", "refresh_token")
          .Add("refresh_token", refreshToken)
      )

    let! result =
      performRequest<OAuthResponse, unit>
        url
        HttpMethod.Post
        (Basic(clientId, clientSecret))
        (Some content)
        ()

    match result with
    | Choice2Of2 deserializedContent ->
      return
        Choice2Of2(
          {| AccessToken = deserializedContent.AccessToken
             Duration = deserializedContent.Duration |},
          refreshToken
        )
    | _ -> return Choice1Of2()
  }

let getNewReleases (skip: int) (take: int) (accessToken: string) =
  task {
    let url =
      $"https://api.spotify.com/v1/browse/new-releases?offset={skip}&limit={take}"

    return!
      performRequest<NewReleases, unit>
        url
        HttpMethod.Get
        (Bearer accessToken)
        None
        ()
  }
