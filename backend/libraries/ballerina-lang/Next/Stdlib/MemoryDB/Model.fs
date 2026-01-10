namespace Ballerina.DSL.Next.StdLib.MemoryDB

[<AutoOpen>]
module Model =
  open System
  open Ballerina.DSL.Next.Types
  open Ballerina.DSL.Next.Terms

  type MemoryDBValues<'ext> =
    | EntityRef of Schema<'ext> * SchemaEntity<'ext>
    | Create of {| EntityRef: Option<Schema<'ext> * SchemaEntity<'ext>> |}
    | Update of {| EntityRef: Option<Schema<'ext> * SchemaEntity<'ext>> |}
    | Delete of {| EntityRef: Option<Schema<'ext> * SchemaEntity<'ext>> |}
    | GetById of {| EntityRef: Option<Schema<'ext> * SchemaEntity<'ext>> |}
    | Run
    | TypeAppliedRun of Schema<'ext>
