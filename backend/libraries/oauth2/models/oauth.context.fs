module OAuth.Models

open System
open System.Threading.Tasks

type OAuthContext<'accessToken, 'refreshToken, 'error> =
  { GetToken: unit -> Task<Choice<'error, 'accessToken * 'refreshToken>>
    SaveAccessToken: 'accessToken -> Task<unit>
    RefreshToken:
      'refreshToken -> Task<Choice<'error, 'accessToken * 'refreshToken>>
    GetExpiration: 'accessToken -> TimeSpan }
