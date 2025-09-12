namespace Ballerina.Lenses

[<AutoOpen>]
module Partial =
  open Ballerina.Collections.Option

  type PartialLens<'a, 'b> =
    { Get: 'a -> Option<'b>; Set: 'b -> 'a }

  type PartialLens<'a, 'b> with
    static member BindGet (f: 'b -> Option<'b>) (lens: PartialLens<'a, 'b>) : PartialLens<'a, 'b> =
      { lens with
          Get =
            fun v ->
              option {
                let! v = lens.Get v
                return! f v
              } }
