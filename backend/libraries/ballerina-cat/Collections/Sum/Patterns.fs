namespace Ballerina.Collections.Sum

[<AutoOpen>]

module Patterns =
  open Ballerina.Collections.Sum.Model
  open Ballerina.Fun
  open System
  open System.Threading.Tasks
  open Ballerina.Collections.NonEmptyList

  type Sum<'a, 'b> with
    static member AsLeft this =
      match this with
      | Left a -> Some a
      | Right _ -> None

    static member AsRight this =
      match this with
      | Left _ -> None
      | Right b -> Some b

    static member All(sums: seq<Sum<'a, 'b>>) : Sum<list<'a>, 'b> =
      Seq.foldBack
        (fun curr acc ->
          sum {
            let! x = curr
            let! xs = acc
            return x :: xs
          })
        sums
        (Left [])
