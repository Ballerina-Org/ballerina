namespace Ballerina.DSL.Next.StdLib.Guid

[<AutoOpen>]
module Model =
  open System

  type GuidOperations<'ext> =
    | Equal of {| v1: Option<Guid> |}
    | NotEqual of {| v1: Option<Guid> |}
