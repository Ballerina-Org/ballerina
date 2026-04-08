namespace Ballerina.StdLib

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

    static member Map (f: string -> string) (sb: StringBuilder) : StringBuilder =
      let rec traverse: StringBuilder -> StringBuilder =
        function
        | One s -> One(f s)
        | Many sb -> Many(sb |> Seq.map traverse)

      traverse sb
