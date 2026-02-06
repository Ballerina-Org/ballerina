namespace Ballerina.StdLib

module Object =

  open System

  let inline (!>) (x: ^a) : ^b =
    ((^a or ^b): (static member op_Implicit: ^a -> ^b) x)

  type Object with
    member self.AsFSharpString = sprintf "%A" self
