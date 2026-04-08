module OAuth.MSGraph.API

open System
open System.Net.Http
open System.Net
open System.Text.Json
open System.Text.Json.Serialization
open OAuth.Common

type EntraUser =
  { [<JsonPropertyName("id")>]
    Id: Guid
    [<JsonPropertyName("mail")>]
    Mail: string
    [<JsonPropertyName("displayName")>]
    Name: string }

type OdataUserResult =
  { [<JsonPropertyName("value")>]
    Value: EntraUser[] }

let requestToken tenant clientId secret =
  task {
    let url = $"https://login.microsoftonline.com/{tenant}/oauth2/v2.0/token"

    let content =
      new FormUrlEncodedContent(
        Map.empty
          .Add("client_id", clientId.ToString())
          .Add("client_secret", secret)
          .Add("scope", "https://graph.microsoft.com/.default")
          .Add("grant_type", "client_credentials")
          .Add("response_type", "code")
      )

    let request = new HttpRequestMessage(HttpMethod.Post, url)
    request.Content <- content
    let! response = httpClient.SendAsync(request)

    match response.StatusCode with
    | HttpStatusCode.OK ->
      let! resultContent = response.Content.ReadAsStringAsync()

      let deserializedContent =
        JsonSerializer.Deserialize<OAuthResponse>(resultContent)

      return
        Choice2Of2(
          {| AccessToken = deserializedContent.AccessToken
             Duration = deserializedContent.Duration |},
          ()
        )
    | _ -> return Choice1Of2()
  }

let getUsers (limit: int) (accessToken: string) =
  task {
    let url =
      $"https://graph.microsoft.com/v1.0/users?$select=id,mail,displayName&$top={limit}"

    let request = new HttpRequestMessage(HttpMethod.Get, url)
    request.Headers.Add("Authorization", $"Bearer {accessToken}")
    let! response = httpClient.SendAsync(request)

    match response.StatusCode with
    | HttpStatusCode.OK ->
      let! resultContent = response.Content.ReadAsStringAsync()

      let deserializedContent =
        JsonSerializer.Deserialize<OdataUserResult>(resultContent)

      return Choice2Of2(deserializedContent.Value)
    | _ -> return Choice1Of2 response.StatusCode
  }
