namespace Ballerina.DSL.Expr.Types

module Patterns =
  open Ballerina.Collections.Sum
  open Ballerina.Errors
  open Ballerina.Collections.NonEmptyList
  open Ballerina.DSL.Expr.Model

  type ExprType with
    static member GetFields(t: ExprType) : Sum<List<string * ExprType>, Errors> =
      match t with
      | ExprType.RecordType fs -> sum { return fs |> Seq.map (fun v -> v.Key, v.Value) |> List.ofSeq }
      | _ ->
        sum.Throw(
          sprintf "Error: type %A is no record and thus has no fields" t
          |> Errors.Singleton
        )

    static member GetCases(t: ExprType) : Sum<Map<CaseName, UnionCase>, Errors> =
      match t with
      | ExprType.UnionType cs -> sum { return cs }
      | _ -> sum.Throw(sprintf "Error: type %A is no union and thus has no cases" t |> Errors.Singleton)

    static member AsSet(t: ExprType) : Sum<ExprType, Errors> =
      sum {
        match t with
        | ExprType.SetType e -> return e
        | _ -> return! sum.Throw(Errors.Singleton $$"""Error: type {{t}} cannot be converted to a Set.""")
      }

    static member AsTable(t: ExprType) : Sum<ExprType, Errors> =
      sum {
        match t with
        | ExprType.TableType e -> return e
        | _ -> return! sum.Throw(Errors.Singleton $$"""Error: type {{t}} cannot be converted to a Table.""")
      }

    static member AsLookupId(t: ExprType) : Sum<ExprTypeId, Errors> =
      sum {
        match t with
        | ExprType.LookupType l -> return l
        | _ -> return! sum.Throw(Errors.Singleton $$"""Error: type {{t}} cannot be converted to a lookup.""")
      }

    static member AsRecord(t: ExprType) : Sum<Map<string, ExprType>, Errors> =
      sum {
        match t with
        | ExprType.RecordType l -> return l
        | _ -> return! sum.Throw(Errors.Singleton $$"""Error: type {{t}} cannot be converted to a record.""")
      }

    static member AsTuple(t: ExprType) : Sum<List<ExprType>, Errors> =
      sum {
        match t with
        | ExprType.TupleType l -> return l
        | _ -> return! sum.Throw(Errors.Singleton $$"""Error: type {{t}} cannot be converted to a tuple.""")
      }

    static member AsUnion(t: ExprType) : Sum<_, Errors> =
      sum {
        match t with
        | ExprType.UnionType c -> return c
        | _ -> return! sum.Throw(Errors.Singleton $$"""Error: type {{t}} cannot be converted to a union.""")
      }

    static member AsUnit(t: ExprType) : Sum<Unit, Errors> =
      sum {
        match t with
        | ExprType.UnitType -> return ()
        | _ -> return! sum.Throw(Errors.Singleton $$"""Error: type {{t}} cannot be converted to a lookup.""")
      }

    static member AsLambda(t: ExprType) : Sum<_, Errors> =
      sum {
        match t with
        | ExprType.ArrowType(i, o) -> return (i, o)
        | _ -> return! sum.Throw(Errors.Singleton $$"""Error: type {{t}} cannot be converted to a lambda.""")
      }

    static member CreateEnum(cases: NonEmptyList<string>) : Map<CaseName, UnionCase> =
      let createEnumCase (caseName: string) : CaseName * UnionCase =
        { CaseName = caseName },
        { CaseName = caseName
          Fields = ExprType.UnitType }

      cases |> NonEmptyList.map createEnumCase |> NonEmptyList.ToList |> Map.ofList
