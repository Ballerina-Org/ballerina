namespace Ballerina.DSL.FormEngine.Parser

module Model =

  open Ballerina.DSL.FormEngine.Model
  open Ballerina.DSL.Expr.Model
  open Ballerina.DSL.Expr.Patterns
  open Ballerina.DSL.Expr.Types.Model
  open Ballerina.DSL.Expr.Types.Patterns
  open System
  open Ballerina.Collections.Sum
  open Ballerina.Collections.Map
  open Ballerina.State.WithError
  open Ballerina.Errors
  open Ballerina.StdLib.Json.Patterns
  open Ballerina.StdLib.String
  open Ballerina.StdLib.Object
  open FSharp.Data
  open Ballerina.Collections.NonEmptyList

  type TopLevel =
    { Types: (string * JsonValue)[]
      Forms: (string * JsonValue)[]
      Launchers: (string * JsonValue)[]
      Enums: (string * JsonValue)[]
      Streams: (string * JsonValue)[]
      Entities: (string * JsonValue)[]
      Tables: (string * JsonValue)[]
      Lookups: (string * JsonValue)[] }

    static member Merge (main1: TopLevel) (main2: TopLevel) : Sum<TopLevel, Errors> =
      sum {
        let inline ensureNoOverlap (m1: seq<'k * 'v>) (m2: seq<'k * 'v>) name =
          sum {
            let m1 = m1 |> Map.ofSeq
            let m2 = m2 |> Map.ofSeq

            let overlappingKeys =
              Set.intersect (m1 |> Map.keys |> Set.ofSeq) (m2 |> Map.keys |> Set.ofSeq)

            if overlappingKeys |> Set.isEmpty then
              return ()
            else
              return! sum.Throw(Errors.Singleton $"Error: {overlappingKeys} multiple definitions of the same {name} ")
          }

        do! ensureNoOverlap main1.Types main2.Types "Types"
        do! ensureNoOverlap main1.Forms main2.Forms "Forms"
        do! ensureNoOverlap main1.Launchers main2.Launchers "Launchers"
        do! ensureNoOverlap main1.Enums main2.Enums "Enums"
        do! ensureNoOverlap main1.Streams main2.Streams "Streams"
        do! ensureNoOverlap main1.Entities main2.Entities "Entities"
        do! ensureNoOverlap main1.Tables main2.Tables "Tables"
        do! ensureNoOverlap main1.Lookups main2.Lookups "Lookups"

        let inline naiveMerge (m1: seq<'k * 'v>) (m2: seq<'k * 'v>) =
          seq {
            yield! m1
            yield! m2
          }
          |> Array.ofSeq

        return
          { Types = naiveMerge main1.Types main2.Types
            Forms = naiveMerge main1.Forms main2.Forms
            Launchers = naiveMerge main1.Launchers main2.Launchers
            Enums = naiveMerge main1.Enums main2.Enums
            Streams = naiveMerge main1.Streams main2.Streams
            Entities = naiveMerge main1.Entities main2.Entities
            Tables = naiveMerge main1.Tables main2.Tables
            Lookups = naiveMerge main1.Lookups main2.Lookups }
      }

    static member MergeMany(mains: List<TopLevel>) : Sum<TopLevel, Errors> =
      sum {
        let liftedMerge (m1: Sum<TopLevel, Errors>) (m2: Sum<TopLevel, Errors>) =
          sum {
            let! m1 = m1
            let! m2 = m2
            return! TopLevel.Merge m1 m2
          }

        let (mains: List<Sum<TopLevel, Errors>>) = mains |> List.map sum.Return
        return! mains |> List.reduce liftedMerge
      }
