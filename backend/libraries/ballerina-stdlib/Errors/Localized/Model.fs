namespace Ballerina

module LocalizedErrors =

  open Ballerina
  open Ballerina.Collections.Sum
  open Ballerina.Errors
  open System

  type Location =
    { File: string
      Line: int
      Column: int }

    override this.ToString() = $"{this.Line}:{this.Column}"

  type Location with
    static member Initial(file: string) = { File = file; Line = 1; Column = 1 }

    static member Unknown: Location =
      { Location.File = "unknown"
        Line = 1
        Column = 1 }

    static member Step (c: Char) (loc: Location) : Location =
      match c with
      | '\n' ->
        { loc with
            Line = loc.Line + 1
            Column = 1 }
      | _ -> { loc with Column = loc.Column + 1 }

  type LocalizedError = Errors.Error<Location>

  type SumBuilder with
    member _.AtUnknownLocation(source: Sum<'a, Errors<Unit>>) =
      source |> sum.MapError(Errors.MapContext(replaceWith Location.Unknown))
