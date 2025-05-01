namespace Ballerina.DSL

module Predicate =

  open System
  open Ballerina.Collections.Option
  open Ballerina.Collections.Map
  open Ballerina.DSL.Model

  type RuleDependency with
    member dep.Predicate (schema: Schema) (changedEntitiesIds: Set<Guid>) =
      option {
        let! changedEntityType = schema.tryFindEntity dep.ChangedEntityType
        // do printfn "changedEntityType = %A" (changedEntityType.ToEntityDescriptorId)
        // do Console.ReadLine() |> ignore
        let! restrictedVariableType = schema.tryFindEntity dep.RestrictedVariableType
        // do printfn "restrictedVariableType = %A" (restrictedVariableType.ToEntityDescriptorId)
        // do Console.ReadLine() |> ignore
        return
          fun (restrictedVariable: obj) ->
            option {
              // do printfn "restrictedVariable = %A" (restrictedVariable)
              // do Console.ReadLine() |> ignore
              // do printfn "dep.PathFromVariableToChange = %A" (dep.PathFromVariableToChange)
              // do Console.ReadLine() |> ignore
              // do printfn "restrictedVariableType = %A" (restrictedVariableType)
              // do Console.ReadLine() |> ignore
              let! variableValue = restrictedVariableType.Lookup(restrictedVariable, dep.PathFromVariableToChange)
              // do printfn "variableValue = %A" (variableValue)
              // do Console.ReadLine() |> ignore
              let! variableValueId = changedEntityType.GetId variableValue
              // do printfn "variableValueId = %A" (variableValueId)
              // do Console.ReadLine() |> ignore
              return changedEntitiesIds |> Set.contains variableValueId
            }
            |> Option.defaultValue true
      }
      |> Option.defaultValue (fun _ -> true)


  type RuleDependencies with
    member deps.PredicatesByRestrictedVariable (schema: Schema) (changedEntitiesIds: Set<Guid>) =
      let (||.) = fun p1 p2 -> fun (o: obj) -> p1 o || p2 o
      let dependencies = deps.dependencies |> Map.values
      // do printfn "dependencies in predicates by restricted variable = %A" dependencies
      // do Console.ReadLine() |> ignore
      let dependencies =
        seq {
          for depsByChangeType in dependencies do
            for dep in depsByChangeType do
              yield
                [ dep.RestrictedVariable, [ dep.Predicate schema changedEntitiesIds ] ]
                |> Map.ofList
        }

      dependencies
      |> Map.mergeMany (fun l1 l2 -> l1 @ l2)
      |> Map.map (fun _ ps -> ps |> Seq.reduce (||.))
