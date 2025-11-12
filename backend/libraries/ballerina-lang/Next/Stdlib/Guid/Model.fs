namespace Ballerina.DSL.Next.StdLib.Guid

[<AutoOpen>]
module Model =
  open System

  type GuidConstructors =
    | Guid_New
    | Guid_V4

    override self.ToString() : string =
      match self with
      | Guid_New -> "guid::new"
      | Guid_V4 -> "guid::v4"

  type GuidOperations<'ext> =
    | Equal of {| v1: Option<Guid> |}
    | NotEqual of {| v1: Option<Guid> |}
