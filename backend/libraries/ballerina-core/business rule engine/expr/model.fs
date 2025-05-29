namespace Ballerina.DSL.Expr

#nowarn FS0060

module Model =
  open System
  open Ballerina.Fun
  open Ballerina.Collections.Option
  open Ballerina.Collections.Map
  open Ballerina.Collections.Sum
  open Ballerina.Errors

  type Vars = Map<VarName, Var>

  and VarName =
    { VarName: string }

    static member Create s = { VarName = s }

  and EntityDescriptorId =
    { EntityDescriptorId: Guid
      EntityName: string }

  and FieldDescriptorId =
    { FieldDescriptorId: Guid
      FieldName: string }

  and Var = EntityDescriptorId * EntityIdentifier
  and EntityIdentifier = One of Guid

  and Value =
    | Unit
    | ConstInt of int
    | ConstFloat of decimal
    | ConstString of string
    | ConstBool of bool
    | ConstGuid of Guid
    | Var of Var
    | CaseCons of string * Value
    | Tuple of List<Value>
    | Record of Map<string, Value>
    | Lambda of VarName * Expr
    | List of List<Value>
  // | Field of FieldDescriptor
  and Expr =
    | Value of Value
    | Apply of Expr * Expr
    | Binary of BinaryOperator * Expr * Expr
    | Unary of UnaryOperator * Expr
    | VarLookup of VarName
    | FieldLookup of Expr * FieldDescriptorId
    | MakeRecord of Map<string, Expr>
    | RecordFieldLookup of Expr * string
    | Exists of VarName * EntityDescriptorId * Expr
    | SumBy of VarName * EntityDescriptorId * Expr
    | MakeTuple of List<Expr>
    | MakeSet of List<Expr>
    | Project of Expr * int
    | MakeCase of string * Expr
    | MatchCase of Expr * Map<string, VarName * Expr>
    | IsCase of string * Expr

  and UnaryOperator =
    | Not
    | Minus

  and BinaryOperator =
    | Plus
    | Minus
    | GreaterThan
    | Equals
    | GreaterThanEquals
    | Times
    | DividedBy
    | And
    | Or


  type Value with
    override v.ToString() =
      match v with
      | Value.Unit -> "()"
      | Value.CaseCons(c, v) -> $"{c}({v})"
      | Value.ConstBool v -> v.ToString()
      | Value.ConstGuid v -> v.ToString()
      | Value.ConstInt v -> v.ToString()
      | Value.ConstFloat v -> v.ToString()
      | Value.ConstString v -> v.ToString()
      | Value.Lambda(v, b) -> $"fun {v.VarName} -> {b.ToString()}"
      | Value.Record fs ->
        let formattedFields =
          fs
          |> Map.toSeq
          |> Seq.sortBy (fun (fieldName, _) -> fieldName)
          |> Seq.map (fun (fieldName, expr) -> $"{fieldName} = {expr.ToString()}")

        $"""{{ {String.Join("; ", formattedFields)} }}"""
      | Value.Tuple vs ->
        let formattedValues = vs |> Seq.map (fun v -> v.ToString())
        $"""({String.Join(", ", formattedValues)})"""
      | Value.Var(_, v) -> v.ToString()
      | Value.List vs ->
        let formattedValues = vs |> Seq.map (fun v -> v.ToString())
        $"""[{String.Join(", ", formattedValues)}]"""

    member self.toObject =
      match self with
      | Value.ConstInt v -> Some(v :> obj)
      | Value.ConstBool v -> Some(v :> obj)
      | Value.ConstFloat v -> Some(v :> obj)
      | Value.ConstGuid v -> Some(v :> obj)
      | Value.ConstString v -> Some(v :> obj)
      | _ -> None

  type Expr with
    override e.ToString() =
      match e with
      | Binary(op, e1, e2) -> $"({e1.ToString()} {op.ToString()} {e2.ToString()})"
      | Unary(op, e) -> $"({op.ToString()}{e.ToString()}"
      | VarLookup v -> v.VarName
      | FieldLookup(e, _) -> $"{e.ToString()}.f.FieldName"
      | Value v -> v.ToString()
      | Apply(f, a) -> $"({f.ToString()})({a.ToString()})"
      | MakeRecord fs ->
        let formattedFields =
          fs
          |> Map.toSeq
          |> Seq.sortBy (fun (fieldName, _) -> fieldName)
          |> Seq.map (fun (fieldName, expr) -> $"{fieldName} = {expr.ToString()}")

        $"""{{ {String.Join("; ", formattedFields)} }}"""
      | MakeTuple fs -> $"""({String.Join(", ", fs |> Seq.map (fun f -> f.ToString()))})"""
      | MakeSet fs -> $"""{{{String.Join("; ", fs |> Seq.map (fun f -> f.ToString()))}}}"""
      | RecordFieldLookup(e, f) -> $"{e.ToString()}.{f}"
      | MakeCase(c, e) -> $"{c.ToString()}({e.ToString()})"
      | Project(e, f) -> $"{e.ToString()}.π{f}"
      | IsCase(c, e) -> $"{e.ToString()}.Is{c}"
      | Exists(v, t, e) -> $"Exists{v.VarName.ToString()} in {t.EntityName} | {e.ToString()}"
      | SumBy(v, t, e) -> $"SumBy{v.VarName.ToString()} in {t.EntityName} | {e.ToString()}"
      | MatchCase(e, cases) ->
        let cases =
          cases
          |> Map.toSeq
          |> Seq.sortBy (fun (caseName, _) -> caseName)
          |> Seq.map (fun (caseName, (varName, expr)) -> $"| {caseName}({varName.VarName}) -> {expr.ToString()}")

        let casesJoined = String.Join(' ', cases)
        $"match {e.ToString()} with {casesJoined}"

    static member op_BooleanOr(e1: Expr, e2: Expr) = Binary(Or, e1, e2)
    static member (+)(e1: Expr, e2: Expr) = Binary(Plus, e1, e2)

    static member (=>>)(e: Expr, fields: List<FieldDescriptorId>) =
      match fields with
      | [] -> e
      | f :: fs -> Expr.FieldLookup(e, f) =>> fs

    static member (=>)(varname: VarName, field: FieldDescriptorId) =
      FieldLookup(Expr.VarLookup varname, field)

    static member op_GreaterThan(e1: Expr, e2: Expr) = Binary(GreaterThan, e1, e2)
