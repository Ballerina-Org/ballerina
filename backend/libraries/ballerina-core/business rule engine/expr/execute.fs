namespace Ballerina.DSL

module Execute =

  open System
  open System.Linq
  open Ballerina.Fun
  open Ballerina.Coroutines
  open Ballerina.DSL.Model
  open Ballerina.DSL.Expr.Model
  open Ballerina.DSL.Expr.Eval

  let execute
    (schema: Schema)
    (vars: Vars)
    (assignment: Assignment)
    : list<Map<FieldDescriptorId, EntitiesIdentifiers>> =
    match assignment.Variable, Expr.eval None schema vars assignment.Value with
    | (assignedVar, [ fieldDescriptorId ]), values ->
      [
        // do printfn "assigning values %A (=%A)" values assignment.Value
        // do Console.ReadLine() |> ignore
        for fieldDescriptor in schema.tryFindField fieldDescriptorId |> Option.toList do
          for (vars, value) in values do
            // do printfn "assigning value %A" value
            // do Console.ReadLine() |> ignore
            let variants = Expr.eval None schema vars (Expr.VarLookup assignedVar)
            // do printfn "assigning variants %A" variants
            // do Console.ReadLine() |> ignore
            for (_, res) in variants do
              match res with
              | Value.Var(_, One entityId) ->
                // do printfn "assigning variant %A" res
                // do Console.ReadLine() |> ignore
                match value with
                | Value.ConstInt i ->
                  if fieldDescriptor.Update.AsInt (One entityId) (replaceWith i) = FieldUpdateResult.ValueChanged then
                    yield [ (fieldDescriptorId, Multiple(Set.singleton entityId)) ] |> Map.ofList
                | Value.ConstGuid id ->
                  if fieldDescriptor.Update.AsRef (One entityId) (replaceWith id) = FieldUpdateResult.ValueChanged then
                    yield [ (fieldDescriptorId, Multiple(Set.singleton entityId)) ] |> Map.ofList
                | _ -> ()
              | _ -> () ]
    | _ -> []
