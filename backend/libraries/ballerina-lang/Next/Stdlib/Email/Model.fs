namespace Ballerina.DSL.Next.StdLib.Email

[<AutoOpen>]
module Model =
  type EmailOperations<'ext> =
    | Send of {| toEmail: Option<string>; subject: Option<string> |}
