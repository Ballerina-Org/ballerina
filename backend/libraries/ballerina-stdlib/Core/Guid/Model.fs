namespace Ballerina.StdLib

module Guid =
  open System

  let private tryParse (x: string) = Guid.TryParse x

  type Guid with
    static member TryParse(guid: string) =
      match tryParse guid with
      | true, parsedGuid -> Some parsedGuid
      | false, _ -> None
