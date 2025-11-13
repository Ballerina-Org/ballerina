namespace Ballerina.Data.Patch

module Model =
  open System
  open Ballerina.Data.Delta.Model
  open Ballerina.Data.Schema.Model

  type Patch<'valueExtension, 'deltaExtension> =
    | Structural of Delta<'valueExtension, 'deltaExtension>
    | Relation of Relation<'valueExtension>

  and Relation<'valueExtension> =
    { Path: UpdaterPathStep list
      Method: LookupMethod
      Target: Guid option }
