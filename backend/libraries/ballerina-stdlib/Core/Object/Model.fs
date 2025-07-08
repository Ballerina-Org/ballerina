namespace Ballerina.StdLib

module Object =

  open System
  open System.Text.RegularExpressions

  type Object with
    member self.ToFSharpString = sprintf "%A" self
