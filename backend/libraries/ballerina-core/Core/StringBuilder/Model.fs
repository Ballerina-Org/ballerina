namespace Ballerina.Core

module StringBuilder =

  open System
  open System.Text.RegularExpressions

  type StringBuilder =
    | One of string
    | Many of seq<StringBuilder>

    static member ToString(sb: StringBuilder) : string =
      let acc = new System.Text.StringBuilder()

      let rec traverse: StringBuilder -> Unit =
        function
        | One s -> acc.Append s |> ignore
        | Many sb -> sb |> Seq.iter traverse

      traverse sb
      acc.ToString()
