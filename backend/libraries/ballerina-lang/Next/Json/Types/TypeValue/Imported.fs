namespace Ballerina.DSL.Next.Types.Json

open Ballerina.DSL.Next.Json

[<AutoOpen>]
module TypeValueImported =
  open FSharp.Data
  open Ballerina.StdLib.Json.Patterns
  open Ballerina
  open Ballerina.Collections.Sum
  open Ballerina.Errors
  open Ballerina.StdLib.Json.Sum
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Types.Patterns
  open Ballerina.DSL.Next.Json.Keys

  let private discriminator = "imported"

  type TypeValue<'valueExt> with
    static member FromJsonImported
      (fromRootJson: JsonValue -> Sum<TypeValue<'valueExt>, Errors<unit>>)
      : JsonValue -> Sum<TypeValue<'valueExt>, Errors<unit>> =
      Sum.assertDiscriminatorAndContinueWithValue discriminator (fun applyFields ->
        sum {

          let! (id, sym, pars, args) = applyFields |> JsonValue.AsQuadruple
          let! id = id |> ResolvedIdentifier.FromJson
          let! sym = sym |> TypeSymbol.FromJson
          let! args = args |> JsonValue.AsArray
          let! args = args |> Seq.map fromRootJson |> sum.All
          let! pars = pars |> JsonValue.AsArray
          let! pars = pars |> Seq.map TypeParameter.FromJson |> sum.All

          return
            TypeValue.CreateImported(
              { Id = id
                Sym = sym
                Parameters = pars
                Arguments = args
                UnionLike = None
                RecordLike = None }
            )
        })

    static member ToJsonImported
      (toRootJson: TypeValue<'valueExt> -> JsonValue)
      : ImportedTypeValue<'valueExt> -> JsonValue =
      fun i ->
        let idJson = i.Id |> ResolvedIdentifier.ToJson
        let symJson = i.Sym |> TypeSymbol.ToJson
        let args = i.Arguments |> List.map toRootJson |> Seq.toArray |> JsonValue.Array

        let params_ =
          i.Parameters |> List.map TypeParameter.ToJson |> Seq.toArray |> JsonValue.Array

        JsonValue.Array [| idJson; symJson; params_; args |]
        |> Json.discriminator discriminator
