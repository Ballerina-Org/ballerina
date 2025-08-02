namespace Ballerina.DSL.Next.Types.Json

[<AutoOpen>]
module TypeParameter =
  open Ballerina.Collections.Sum
  open Ballerina.StdLib.Json.Patterns
  open Ballerina.Errors
  open Ballerina.DSL.Next.Types.Model

  open FSharp.Data

  let inline private (>>=) f g = fun x -> sum.Bind(f x, g)

  type TypeParameter with
    static member FromJson(json: JsonValue) : Sum<TypeParameter, Errors> =
      sum {
        let! fields = json |> JsonValue.AsRecordMap

        let! name =
          fields
          |> (Map.tryFindWithError "name" "TypeParameter" "name" >>= JsonValue.AsString)

        let! kind = fields |> (Map.tryFindWithError "kind" "TypeParameter" "kind" >>= Kind.FromJson)

        return { Name = name; Kind = kind }
      }

    static member ToJson: TypeParameter -> JsonValue =
      fun tp -> JsonValue.Record [| "name", JsonValue.String tp.Name; "kind", Kind.ToJson tp.Kind |]
