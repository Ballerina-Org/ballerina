namespace Ballerina.Data.Arity

module Model =
  open MathNet.Numerics.Distributions

  type SimpleArity = { Min: Option<int>; Max: Option<int> }

  type LookupArity = SimpleArity

  // Δ fair enough for most of the cases now
  // ∇ not yet used, proposition for the next version, depends on the needed depth
  // created at the same time as the `SimpleArity` type by other dev without knowing
  // check example on the very bottom

  type Arity =
    | Exact of n: int
    | Range of min: int * max: int
    | AtLeast of n: int
    | Any

  let private thunk (x: unit -> 'a) = x ()

  let private truncatedSample
    (distributionFactory: unit -> IDiscreteDistribution)
    (minValue: int)
    (maxValue: int)
    : unit -> int =
    fun () ->
      let mutable sample = minValue - 1

      while sample < minValue || sample > maxValue do
        sample <- distributionFactory().Sample()

      sample

  let private createSamplerWithClamp
    (distributionFactory: unit -> IDiscreteDistribution)
    (minValue: int)
    (maxValue: int)
    : unit -> int =
    fun () -> distributionFactory () |> _.Sample() |> max minValue |> min maxValue

  let private minVal (a: Arity) =
    match a with
    | Exact n -> n
    | Range(m, _) -> m
    | AtLeast n -> n
    | Any -> 0

  [<RequireQualifiedAccess>]
  module Arity =
    let value maxVal (a: Arity) (distributionFactory: unit -> IDiscreteDistribution) =
      match a with
      | Exact n -> n
      | Range(minVal, maxVal) -> truncatedSample distributionFactory minVal maxVal |> thunk
      | AtLeast _
      | Any -> createSamplerWithClamp distributionFactory (minVal a) maxVal |> thunk

    let anyFrom (distributionFactory: unit -> IDiscreteDistribution) c =
      distributionFactory () |> _.Sample() |> min c

  module private Example =
    let _usage =
      [ 1..100 ]
      |> List.map (fun _ -> Arity.anyFrom (fun () -> Poisson(1.7)) 7)
      |> List.max
