namespace Ballerina.StdLib.Algorithms

module TopologicalSort =
  open Ballerina
  open Ballerina.Collections.Sum
  open Ballerina.Errors

  (*
    FP-style topological sort (Kahn by layers).
    
    Complexity: O(V + E) * log(V)

    The extra log comes from computing nodes with no dependencies. In the imperative version, indegrees are decremented (mutation), so there is no extra log.

    Invariant: the frontier always contains nodes without dependencies (i.e. ready to be output to the top sorted List)
    *)
  let private filterM (predicate: 'a -> Sum<bool, Errors<Unit>>) (xs: seq<'a>) : Sum<List<'a>, Errors<Unit>> =

    let folder (x: 'a) (acc: Sum<List<'a>, Errors<Unit>>) : Sum<List<'a>, Errors<Unit>> =
      sum {
        let! acc = acc
        let! ok = predicate x
        return if ok then x :: acc else acc
      }

    List.foldBack folder (Seq.toList xs) (sum.Return([]: List<'a>))

  let sort (graph: Map<'T, Set<'T>>) : Sum<List<'T>, Errors<Unit>> when 'T: comparison =
    let allNodes =
      graph
      |> Map.toSeq
      |> Seq.collect (fun (n, ds) ->
        seq {
          yield n
          yield! ds
        })
      |> Set.ofSeq

    let noDependencies (visited: Set<'T>) : Sum<Set<'T>, Errors<Unit>> =
      sum {
        let! result =
          allNodes
          |> Set.filter (fun n -> not (visited.Contains n))
          |> filterM (fun n ->
            sum {
              let! dependencies =
                graph
                |> Map.tryFindWithError n "graph" (fun () -> sprintf "cannot find node: %A" n) ()

              return dependencies.IsSubsetOf visited // logV step
            })

        return result |> Set.ofSeq
      }

    let rec loop (visited: Set<'T>) (frontier: Set<'T>) (acc: List<'T>) : Sum<List<'T>, Errors<Unit>> =
      sum {
        if Set.isEmpty frontier then
          if visited = allNodes then
            acc |> List.rev
          else
            return!
              sum.Throw(
                Errors.Singleton () (fun () ->
                  sprintf
                    "Cycle detected in graph: %s"
                    (Set.difference allNodes visited
                     |> Set.toList
                     |> List.map string
                     |> String.concat ", "))
              )
        else
          let visited' = Set.union visited frontier
          let acc' = (frontier |> Set.toList) @ acc // Set is ordered -> deterministic
          let! frontier' = noDependencies visited'
          return! loop visited' frontier' acc'
      }

    sum {
      let! initialFrontier = noDependencies Set.empty
      return! loop Set.empty initialFrontier []
    }
