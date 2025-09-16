namespace Ballerina.Fix

[<AutoOpen>]
module Fix =
  // Definition of the fix point equation: fix f = f (fix f)
  // Note: important that the recursion step, fix f, yields a closure - such that its evaluation is deferred
  let rec fix<'a, 'b> (f: ('a -> 'b) -> ('a -> 'b)) (x: 'a) : 'b = f (fix f) x
