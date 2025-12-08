namespace Ballerina.Collections.TaskSum

open System.Threading.Tasks
open Ballerina.Collections.Sum

// It's not a correct implementation but should do for now.
// This one will have a slight performance hit (not much, seems to be around 10% worse),
// and implementation of try/catch/finally here would be dangerous - or at least it looks like it to me.
// To implement it properly, the one should write his own TaskCode implementation, and all the related stuff
type TaskSumBuilder() =
  member inline _.Bind(taskSum, f: 'a -> Task<Sum<'b, _>>) =
    task {
      let! sum = taskSum

      match sum |> Sum.map f with
      | Left left -> return! left
      | Right right -> return Right right
    }

  member inline _.ReturnFrom x : Task<Sum<_, _>> = x

  member inline _.Return value : Task<Sum<_, _>> = value |> sum.Return |> Task.FromResult

  member inline _.Source(s: Sum<_, _> Task) = s

  member inline _.Source(s: Sum<_, _>) = Task.FromResult s

  // there also supposed to be Source : Task<'a> -> Task<Sum<'a, 'b>>
  // but if you add it, the compiler will no longer be able to differentiate
  // Task<'a> from Task<Sum<'a, 'b>> which is weird, and I don't know why it happens

  // would like to have it for just Task it will allow do! Task<'a> without Source for Task<'a>
  member inline _.Source(s: Task<unit>) =
    task {
      do! s
      return Left()
    }

  // this should be Source : Task<'a> -> Task<Sum<'a, 'b>>
  member inline _.Lift t =
    task {
      let! result = t
      return Left result
    }

[<AutoOpen>]
module ComputationExpression =
  let taskSum = TaskSumBuilder()
