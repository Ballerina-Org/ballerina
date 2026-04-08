namespace Ballerina.DSL.Next.Types.Json

[<AutoOpen>]
module TypeParameter =
  open Ballerina
  open Ballerina.Collections.Sum
  open Ballerina.StdLib.Json.Patterns
  open Ballerina.Errors
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Json.Keys

  open FSharp.Data

  let inline private (>>=) f g = fun x -> sum.Bind(f x, g)

  let private nameKey = "name"

  type TypeParameter with
    static member FromJson(json: JsonValue) : Sum<TypeParameter, Errors<_>> =
      sum {
        let! fields = json |> JsonValue.AsRecordMap

        let! name =
          fields
          |> (Map.tryFindWithError nameKey "TypeParameter" (fun () -> nameKey) ()
              >>= JsonValue.AsString)

        let! kind =
          fields
          |> (Map.tryFindWithError kindKey "TypeParameter" (fun () -> kindKey) ()
              >>= Kind.FromJson)

        return { Name = name; Kind = kind }
      }

    static member ToJson: TypeParameter -> JsonValue =
      fun tp -> JsonValue.Record [| nameKey, JsonValue.String tp.Name; kindKey, Kind.ToJson tp.Kind |]
