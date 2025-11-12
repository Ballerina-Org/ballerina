namespace Ballerina

module ErrorsPatterns =

  open Ballerina.Collections.NonEmptyList
  open Ballerina.Errors
  open Ballerina.LocalizedErrors

  type Errors.Errors with
    static member OfLocalizedError(localizedError: LocalizedError) : Errors.Error =
      { Message = localizedError.Message
        Priority = localizedError.Priority }

    static member OfLocalizedErrors(localizedErrors: Errors) : Errors.Errors =
      { Errors = localizedErrors.Errors |> NonEmptyList.map Errors.OfLocalizedError }
