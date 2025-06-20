namespace Ballerina.DSL.Expr.Types

open Ballerina.DSL.Expr.Model
open FSharp.Data

type TraversalScope =
  | Never
  | Once
  | All

  member this.stepDeeper =
    match this with
    | All -> All
    | Once -> Never
    | Never -> Never

  member this.isAllowed =
    match this with
    | Never -> false
    | _ -> true
        
type WithScope<'T> = WithScope of TraversalScope * 'T

type ScopeBuilder() =
  member _.Return(x: 'T) : WithScope<'T> = WithScope(Once, x)

  member _.ReturnFrom(w: WithScope<'T>) = w

  member _.Zero() : WithScope<'T> =
    WithScope(Never, Unchecked.defaultof<'T>)

  member _.Bind(WithScope(scope, x), f: 'T -> WithScope<'U>) : WithScope<'U> =
    if scope.isAllowed then
      let (WithScope(nextScope, y)) = f x
      let combined =
        match scope, nextScope with
        | Never, _ -> Never
        | _, Never -> Never
        | All, _ -> All
        | _, All -> All
        | Once, Once -> Once
      WithScope(combined, y)
    else
      WithScope(Never, Unchecked.defaultof<'U>)
            
module ScopeBuilder =

  let rec mergeJson (a: JsonValue) (b: JsonValue) : JsonValue =
    match a, b with
    | JsonValue.Record propsA, JsonValue.Record propsB ->
        Array.append propsA propsB
        |> Array.groupBy fst
        |> Array.map (fun (key, values) ->
          let mergedValue =
            values
            |> Array.map snd
            |> Array.toList
            |> function
                | [v] -> v
                | [v1; v2] -> mergeJson v1 v2
                | more -> more |> List.last
          key, mergedValue
          )
        |> JsonValue.Record 

    | JsonValue.Array arrA, JsonValue.Array arrB ->
        JsonValue.Array (Array.append arrA arrB)
    | _a, b -> b

  let mergeJsonList (values: JsonValue list) : JsonValue =
    match values with
    | [] -> JsonValue.Null
    | head :: tail -> List.fold mergeJson head tail
      
  let rec scope (xs: List<WithScope<'T>>) : WithScope<List<'T>> =
    match xs with
    | [] -> WithScope(Once, [])
    | WithScope(Never, _) :: _ -> WithScope(Never, [])
    | WithScope(s1, x) :: rest ->
      match scope rest with
      | WithScope(s2, restResult) ->
        let combined =
          match s1, s2 with
          | Never, _ | _, Never -> Never
          | All, _ | _, All -> All
          | Once, Once -> Once
        WithScope(combined, x :: restResult)
              
  let list (f: 'T -> WithScope<'U>) (xs: List<'T>) : WithScope<List<'U>> =
    xs |> List.map f |> scope
    
  let traverse = ScopeBuilder()
  
  let runScope (WithScope(_, v)) = v
  
  let rec eval (acc: JsonValue) (scopeIn: TraversalScope) (expr: ExprType) : JsonValue =
    match expr with
    | UnitType ->
      JsonValue.Record [||]
    | RecordType map ->
      map
      |> Map.toList
      |> list (fun (name, expr) ->
          traverse {
            let! value = WithScope(scopeIn, eval acc scopeIn.stepDeeper expr)
            return name, value
          }
      )
      |> runScope
      |> Array.ofList
      |> JsonValue.Record
    | _ -> JsonValue.Record [||]
    |> mergeJson acc