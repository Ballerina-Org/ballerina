namespace Ballerina.DSL.Next.Types

[<AutoOpen>]
module Model =
  open System
  open Ballerina.StdLib.OrderPreservingMap

  type Identifier =
    | LocalScope of string
    | FullyQualified of List<string> * string

    override id.ToString() =
      match id with
      | LocalScope name -> name
      | FullyQualified(names, name) -> String.Join("::", names) + "::" + name

  type TypeParameter = { Name: string; Kind: Kind }

  and Kind =
    | Symbol
    | Star
    | Arrow of Kind * Kind

  and TypeSymbol = { Name: Identifier; Guid: Guid }

  and TypeVar =
    { Name: string
      Guid: Guid }

    override v.ToString() = v.Name

  and TypeVarIdentifier =
    { Name: string }

    override v.ToString() = v.Name

  and TypeExpr =
    | Primitive of PrimitiveType
    | Let of string * TypeExpr * TypeExpr
    | LetSymbols of List<string> * TypeExpr
    | NewSymbol of string
    | Lookup of Identifier
    | Apply of TypeExpr * TypeExpr
    | Lambda of TypeParameter * TypeExpr
    | Arrow of TypeExpr * TypeExpr
    | Record of List<TypeExpr * TypeExpr>
    | Tuple of List<TypeExpr>
    | Union of List<TypeExpr * TypeExpr>
    | Set of TypeExpr
    | Map of TypeExpr * TypeExpr
    | KeyOf of TypeExpr
    | Sum of List<TypeExpr>
    | Flatten of TypeExpr * TypeExpr
    | Exclude of TypeExpr * TypeExpr
    | Rotate of TypeExpr
    | Imported of ImportedTypeValue

    override self.ToString() =
      match self with
      | Imported i ->
        let comma = ", "
        $"{i.Sym.Name}[{String.Join(comma, i.Arguments)}]"
      | Primitive p -> p.ToString()
      | Let(name, value, body) -> $"Let {name} = {value} in {body})"
      | LetSymbols(names, body) -> let comma = ", " in $"LetSymbols({String.Join(comma, names)}) in {body})"
      | NewSymbol name -> $"NewSymbol({name})"
      | Lookup id -> id.ToString()
      | Apply(f, a) -> $"{f}[{a}]"
      | Lambda(param, body) -> $"Fun {param.Name}::{param.Kind} => {body}"
      | Arrow(t1, t2) -> $"({t1} -> {t2})"
      | Record fields ->
        let comma = ", "
        let fieldStrs = fields |> List.map (fun (name, typ) -> $"{name}: {typ}")
        $"{{{String.Join(comma, fieldStrs)}}}"
      | Tuple types ->
        let comma = ", "
        $"[{String.Join(comma, types)}]"
      | Union types ->
        let comma = " | "
        let typeStrs = types |> List.map (fun (name, typ) -> $"{name}: {typ}")
        $"({String.Join(comma, typeStrs)})"
      | Set t -> $"Set[{t}]"
      | Map(k, v) -> $"Map[{k}, {v}]"
      | KeyOf t -> $"KeyOf[{t}]"
      | Sum types ->
        let comma = " + "
        $"({String.Join(comma, types)})"

      | Flatten(t1, t2) -> $"Flatten[{t1}, {t2}]"
      | Exclude(t1, t2) -> $"Exclude[{t1}, {t2}]"
      | Rotate t -> $"Rotate[{t}]"



  and TypeBinding =
    { Identifier: Identifier
      Type: TypeExpr }

  and TypeValue =
    | Primitive of WithTypeExprSourceMapping<PrimitiveType>
    | Var of TypeVar
    | Lookup of Identifier // TODO: Figure out what to do with this (orig name wise) after recursion in type checking is implement correctly
    | Lambda of WithTypeExprSourceMapping<TypeParameter * TypeExpr>
    | Apply of WithTypeExprSourceMapping<TypeVar * TypeValue>
    | Arrow of WithTypeExprSourceMapping<TypeValue * TypeValue>
    | Record of WithTypeExprSourceMapping<OrderedMap<TypeSymbol, TypeValue>>
    | Tuple of WithTypeExprSourceMapping<List<TypeValue>>
    | Union of WithTypeExprSourceMapping<OrderedMap<TypeSymbol, TypeValue>>
    | Sum of WithTypeExprSourceMapping<List<TypeValue>>
    | Set of WithTypeExprSourceMapping<TypeValue>
    | Map of WithTypeExprSourceMapping<TypeValue * TypeValue>
    | Imported of ImportedTypeValue // FIXME: This should also have an orig name, implement once the extension is implemented completely

    override self.ToString() =
      match self with
      | Imported i ->
        let comma = ", "
        $"{i.Sym.Name}[{String.Join(comma, i.Arguments)}]"
      | Primitive p -> p.value.ToString()
      | Lookup id -> id.ToString()
      | Var v -> v.Name.ToString()
      | Apply({ value = (f, a) }) -> $"{f}[{a}]"
      | Lambda({ value = (param, body) }) -> $"Fun {param.Name} => {body}"
      | Arrow({ value = (t1, t2) }) -> $"({t1} -> {t2})"
      | Record({ value = fields }) ->
        let comma = ", "

        let fieldStrs =
          fields |> OrderedMap.toList |> List.map (fun (name, typ) -> $"{name}: {typ}")

        $"{{{String.Join(comma, fieldStrs)}}}"
      | Tuple types ->
        let comma = ", "
        $"[{String.Join(comma, types)}]"
      | Union({ value = types }) ->
        let comma = " | "

        let typeStrs =
          types |> OrderedMap.toList |> List.map (fun (name, typ) -> $"{name}: {typ}")

        $"({String.Join(comma, typeStrs)})"
      | Set t -> $"Set[{t}]"
      | Map({ value = (k, v) }) -> $"Map[{k}, {v}]"
      | Sum({ value = types }) ->
        let comma = " + "
        $"({String.Join(comma, types)})"


  and ExprTypeLetBindingName = ExprTypeLetBindingName of string

  and TypeExprSourceMapping =
    | OriginExprTypeLet of ExprTypeLetBindingName * TypeExpr
    | OriginTypeExpr of TypeExpr
    | NoSourceMapping of string

  and WithTypeExprSourceMapping<'v> =
    { value: 'v
      source: TypeExprSourceMapping }

  and ImportedTypeValue =
    { Id: Identifier
      Sym: TypeSymbol
      Parameters: List<TypeParameter>
      Arguments: List<TypeValue>
      UnionLike: Option<OrderedMap<TypeSymbol, TypeExpr>>
      RecordLike: Option<OrderedMap<TypeSymbol, TypeExpr>> }

  and PrimitiveType =
    | Unit
    | Guid
    | Int32
    | Int64
    | Float32
    | Float64
    | Decimal
    | Bool
    | String
    | DateTime
    | DateOnly
    | TimeSpan
