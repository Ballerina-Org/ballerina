namespace Ballerina.StdLib

module Map =
  type Map<'Key, 'Value when 'Key: comparison> with
    static member merge (m1: Map<'Key, 'Value>) (m2: Map<'Key, 'Value>) : Map<'Key, 'Value> =
      Seq.concat [ m1 |> Map.toSeq; m2 |> Map.toSeq ] |> Map.ofSeq
