namespace Ballerina.DSL.Parser

module Patterns =

  open Ballerina.Collections.Sum
  open Ballerina.State.WithError
  open Ballerina.Errors
  open Ballerina.DSL.Expr.Model
  open Ballerina.DSL.Expr.Types.Model

  type ContextOperations<'context> =
    { TryFindType: 'context -> string -> Sum<TypeBinding, Errors> }

  module TypeContext =
    let ContextOperations: ContextOperations<TypeContext> =
      { TryFindType = fun ctx name -> ctx |> Map.tryFindWithError<string, TypeBinding> name "type" name }

    let TryFindType (ctx: TypeContext) (name: string) : Sum<TypeBinding, Errors> =
      ctx |> Map.tryFindWithError<string, TypeBinding> name "type" name

  type SumBuilder with
    member sum.TryFindField name fields =
      fields
      |> Seq.tryFind (fst >> (=) name)
      |> Option.map snd
      |> Sum.fromOption (fun () -> Errors.Singleton $"Error: cannot find field '{name}'")

  type StateBuilder with
    member state.TryFindField name fields =
      fields |> sum.TryFindField name |> state.OfSum

  type ExprType with
    static member Find (ctx: TypeContext) (typeId: ExprTypeId) : Sum<ExprType, Errors> =
      sum { return! TypeContext.TryFindType ctx typeId.VarName |> Sum.map (fun tb -> tb.Type) }

    static member ResolveLookup (ctx: TypeContext) (t: ExprType) : Sum<ExprType, Errors> =
      sum {
        match t with
        | ExprType.LookupType l -> return! ExprType.Find ctx l
        | ExprType.TableType t ->
          let! t = ExprType.ResolveLookup ctx t
          return ExprType.TableType t
        | _ -> return t
      }
