namespace Ballerina.DSL.Next.StdLib.Email

[<AutoOpen>]
module Patterns =
  open Ballerina
  open Ballerina.Collections.Sum
  open Ballerina.Errors

  type EmailOperations<'ext> with
    static member AsSend
      (op: EmailOperations<'ext>)
      : Sum<{| toEmail: Option<string>; subject: Option<string> |}, Errors<Unit>> =
      match op with
      | EmailOperations.Send state -> sum.Return state
