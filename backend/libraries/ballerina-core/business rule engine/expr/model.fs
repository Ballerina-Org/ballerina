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
    | ConstFloat of float
    | ConstString of string
    | ConstBool of bool
    | ConstGuid of Guid
    | Var of Var
    | CaseCons of string * Value
    | Tuple of List<Value>
    | Record of Map<string, Value>
    | Lambda of VarName * Expr
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
        let eq = "=" in
        $"{{ {fs |> Seq.map (fun f -> f.Key.ToString() + eq + f.Value.ToString())} |> String.Join ';' }}"
      | Value.Tuple vs -> $"( {vs |> Seq.map (fun v -> v.ToString())} |> String.Join ',' )"
      | Value.Var(_, v) -> v.ToString()

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
        let eq = "=" in
        $"{{ {fs |> Seq.map (fun f -> f.Key.ToString() + eq + f.Value.ToString())} |> String.Join ';' }}"
      | MakeTuple fs -> $"({fs |> Seq.map (fun f -> f.ToString())} |> String.Join ',')"
      | MakeSet fs -> $"{{ {fs |> Seq.map (fun f -> f.ToString())} |> String.Join ';' }}"
      | RecordFieldLookup(e, f) -> $"{e.ToString()}.{f}"
      | MakeCase(c, e) -> $"{c.ToString()}({e.ToString()})"
      | Project(e, f) -> $"{e.ToString()}.π{f}"
      | IsCase(c, e) -> $"{e.ToString()}.Is{c}"
      | Exists(v, t, e) -> $"Exists{v.VarName.ToString()} in {t.EntityName} | {e.ToString()}"
      | SumBy(v, t, e) -> $"SumBy{v.VarName.ToString()} in {t.EntityName} | {e.ToString()}"
      | MatchCase(e, cases) ->
        let bar = "|"
        let arr = "->"

        let cases =
          cases |> Seq.map (fun f -> bar + f.Key.ToString() + arr + f.Value.ToString())

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
