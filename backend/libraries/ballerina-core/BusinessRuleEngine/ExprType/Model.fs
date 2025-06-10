namespace Ballerina.DSL.Expr.Types

module Model =
  open System
  open Ballerina.Collections.Map
  open Ballerina.Collections.Sum
  open Ballerina.Errors
  open Ballerina.DSL.Expr.Model

  type TypeVarBindings = Map<VarName, ExprType>

  and TypeBinding =
    { TypeId: ExprTypeId
      Type: ExprType
      Const: bool }

  and TypeContext = Map<string, TypeBinding>

  and TypeBindings = Map<ExprTypeId, ExprType>

  type TypeBinding with
    static member Create(name, exprType) =
      { TypeBinding.TypeId = name
        TypeBinding.Type = exprType
        Const = false }

  type ExprTypeId with
    static member Create name = { TypeName = name }

  type ExprType with
    static member Extend t1 t2 =
      match t1, t2 with
      | RecordType fields1, RecordType fields2 when
        fields1
        |> Map.keys
        |> Set.ofSeq
        |> Set.intersect (fields2 |> Map.keys |> Set.ofSeq)
        |> Set.isEmpty
        ->
        Map.merge (fun a _ -> a) fields1 fields2 |> ExprType.RecordType |> Left
      | _ -> Right(Errors.Singleton $$"""Error: cannot merge types {{t1}} and {{t2}}.""")

    static member GetTypesFreeVars(t: ExprType) : Set<ExprTypeId> =
      let (!) = ExprType.GetTypesFreeVars

      match t with
      | ExprType.UnitType
      | ExprType.CustomType _
      | ExprType.VarType _ -> Set.empty
      | ExprType.TupleType ts -> ts |> Seq.map (!) |> Seq.fold (+) Set.empty
      | ExprType.ListType t
      | ExprType.TableType t
      | ExprType.SetType t
      | ExprType.OptionType t
      | ExprType.OneType t
      | ExprType.ManyType t -> !t
      | ExprType.LookupType t -> Set.singleton t
      | ExprType.MapType(k, v) -> !k + !v
      | ExprType.SumType(l, r) -> !l + !r
      | ExprType.PrimitiveType _ -> Set.empty
      | ExprType.UnionType cs ->
        let cs = cs |> Map.values |> List.ofSeq
        cs |> Seq.map (fun c -> !c.Fields) |> Seq.fold (+) Set.empty
      | ExprType.RecordType fs -> fs |> Map.values |> Seq.map (!) |> Seq.fold (+) Set.empty
      | ExprType.ArrowType(l, r) -> !l + !r
      | ExprType.GenericApplicationType(l, r) -> !l + !r
      | ExprType.GenericType(_, _, e) -> !e

    static member Substitute (tvars: TypeVarBindings) (t: ExprType) : ExprType =
      let (!) = ExprType.Substitute tvars
      let (!!) = List.map (!)

      match t with
      | ExprType.CustomType _
      | ExprType.LookupType _
      | ExprType.PrimitiveType _
      | ExprType.UnitType -> t
      | ExprType.VarType v ->
        match tvars |> Map.tryFind v with
        | None -> t
        | Some t -> t
      | ExprType.ListType t -> ExprType.ListType(!t)
      | ExprType.TableType t -> ExprType.TableType(!t)
      | ExprType.SetType t -> ExprType.SetType(!t)
      | ExprType.OptionType t -> ExprType.OptionType(!t)
      | ExprType.OneType t -> ExprType.OneType(!t)
      | ExprType.ManyType t -> ExprType.ManyType(!t)
      | ExprType.MapType(k, v) -> ExprType.MapType(!k, !v)
      | ExprType.SumType(l, r) -> ExprType.SumType(!l, !r)
      | ExprType.TupleType ts -> ExprType.TupleType(!!ts)
      | ExprType.UnionType cs -> ExprType.UnionType(cs |> Map.map (fun _ c -> { c with Fields = !c.Fields }))
      | ExprType.RecordType fs -> ExprType.RecordType(fs |> Map.map (fun _ -> (!)))
      | ExprType.ArrowType(l, r) -> ExprType.ArrowType(!l, !r)
      | ExprType.GenericApplicationType(l, r) -> ExprType.GenericApplicationType(!l, !r)
      | ExprType.GenericType(v, k, e) -> ExprType.GenericType(v, k, !e)
