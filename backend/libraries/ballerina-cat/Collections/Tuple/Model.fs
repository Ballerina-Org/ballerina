namespace Ballerina.Collections

module Tuple =

  let fromNested3 ((a, (b, c)): ('a * ('b * 'c))) = a, b, c
  let fromNested4 ((a, (b, (c, d))): ('a * ('b * ('c * 'd)))) = a, b, c, d
  let fromNested5 ((a, (b, (c, (d, e)))): ('a * ('b * ('c * ('d * 'e))))) = a, b, c, d, e
  let dup a = (a, a)
  let (<*>) f g = fun (a, b) -> (f a, g b)
