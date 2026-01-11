[<AutoOpen>]
module Ballerina.StdLib.Core

let memoize f =
  let cache = System.Collections.Generic.Dictionary<_, _>()

  fun arg ->
    match cache.TryGetValue arg with
    | true, value -> value
    | false, _ ->
      let value = f arg
      cache.Add(arg, value)
      value
