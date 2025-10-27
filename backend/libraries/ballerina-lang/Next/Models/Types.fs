namespace Ballerina.DSL.Next.Types

[<AutoOpen>]
module Model =
  open System
  open Ballerina.StdLib.OrderPreservingMap
  open Ballerina.Cat.Collections.OrderedMap

  type Identifier =
    | LocalScope of string
    | FullyQualified of List<string> * string

    override id.ToString() =
      match id with
      | LocalScope name -> name
      | FullyQualified(names, name) -> String.Join("::", names) + "::" + name

  type ResolvedIdentifier =
    { Assembly: string
      Module: string
      Type: Option<string>
      Name: string }

    static member Create(assembly: string, module_: string, type_: Option<string>, name: string) : ResolvedIdentifier =
      { Assembly = assembly
        Module = module_
        Type = type_
        Name = name }

    override id.ToString() =
      let elements =
        [
          // if not (String.IsNullOrEmpty id.Assembly) then yield id.Assembly
          // if not (String.IsNullOrEmpty id.Module) then yield id.Module
          match id.Type with
          | Some t -> yield t
          | None -> ()
          yield id.Name ]

      String.Join("::", elements)

  type TypeCheckScope =
    { Assembly: string
      Module: string
      Type: Option<string> }

    static member Empty: TypeCheckScope =
      { Assembly = ""
        Module = ""
        Type = None }

    static member Updaters =
      {| Assembly =
          fun u scope ->
            { scope with
                TypeCheckScope.Assembly = u scope.Assembly }
         Module =
          fun u scope ->
            { scope with
                TypeCheckScope.Module = u scope.Module }
         Type =
          fun u scope ->
            { scope with
                TypeCheckScope.Type = u scope.Type } |}

    override s.ToString() =
      let elements =
        [ yield s.Assembly
          yield s.Module
          match s.Type with
          | Some t -> yield t
          | None -> yield "" ]

      String.Join("::", elements)

    static member Resolve(id: Identifier, scope: TypeCheckScope) : ResolvedIdentifier =
      match id with
      | Identifier.FullyQualified(names, name) ->
        match names with
        | assembly :: module_ :: type_ :: [] ->
          { Assembly = assembly
            Module = module_
            Type = Some type_
            Name = name }
        | module_ :: type_ :: [] ->
          { Assembly = scope.Assembly
            Module = module_
            Type = Some type_
            Name = name }
        | type_ :: [] ->
          { Assembly = scope.Assembly
            Module = scope.Module
            Type = Some type_
            Name = name }
        | _ ->
          { Assembly = scope.Assembly
            Module = scope.Module
            Type = scope.Type
            Name = name }
      | Identifier.LocalScope name ->
        { Assembly = scope.Assembly
          Module = scope.Module
          Type = scope.Type
          Name = name }

    member scope.Resolve(id: Identifier) : ResolvedIdentifier = TypeCheckScope.Resolve(id, scope)



  type TypeParameter = { Name: string; Kind: Kind }

  and Kind =
    | Symbol
    | Star
    | Arrow of Kind * Kind

  and TypeSymbol =
    { Name: Identifier
      Guid: Guid }

    override s.ToString() = s.Name.ToString()

  and TypeVar =
    { Name: string
      Guid: Guid }

    override v.ToString() = v.Name

  and TypeVarIdentifier =
    { Name: string }

    override v.ToString() = v.Name

  and SymbolsKind =
    | RecordFields
    | UnionConstructors

    override sk.ToString() =
      match sk with
      | RecordFields -> "RecordFields"
      | UnionConstructors -> "UnionConstructors"

  and TypeExpr =
    | Primitive of PrimitiveType
    | Let of string * TypeExpr * TypeExpr
    | LetSymbols of List<string> * SymbolsKind * TypeExpr
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
      | LetSymbols(names, symbolsKind, body) ->
        let comma = ", " in $"LetSymbols({String.Join(comma, names)}):{symbolsKind} in {body})"
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
      | Union({ source = OriginExprTypeLet(id, _) })
      | Record({ source = OriginExprTypeLet(id, _) }) -> id.ToString()
      | Record({ source = OriginTypeExpr(TypeExpr.Lookup id) })
      | Primitive({ source = OriginTypeExpr(TypeExpr.Lookup id) })
      | Apply({ source = OriginTypeExpr(TypeExpr.Lookup id) })
      | Lambda({ source = OriginTypeExpr(TypeExpr.Lookup id) })
      | Arrow({ source = OriginTypeExpr(TypeExpr.Lookup id) })
      | Tuple({ source = OriginTypeExpr(TypeExpr.Lookup id) })
      | Union({ source = OriginTypeExpr(TypeExpr.Lookup id) })
      | Sum({ source = OriginTypeExpr(TypeExpr.Lookup id) }) -> id.ToString()
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
      | Tuple({ value = types }) ->
        let comma = " * "
        $"({String.Join(comma, types)})"
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


  and ExprTypeLetBindingName =
    | ExprTypeLetBindingName of string

    override self.ToString() =
      let (ExprTypeLetBindingName s) = self in s

  and TypeExprSourceMapping =
    | OriginExprTypeLet of ExprTypeLetBindingName * TypeExpr
    | OriginTypeExpr of TypeExpr
    | NoSourceMapping of string

  and WithTypeExprSourceMapping<'v> =
    { value: 'v
      source: TypeExprSourceMapping }

    override self.ToString() = self.value.ToString()

  and ImportedTypeValue =
    { Id: ResolvedIdentifier
      Sym: TypeSymbol
      Parameters: List<TypeParameter>
      Arguments: List<TypeValue>
      UnionLike: Option<OrderedMap<TypeSymbol, TypeExpr>>
      RecordLike: Option<OrderedMap<TypeSymbol, TypeExpr>> }

    override self.ToString() =
      let comma = ", "
      $"{self.Sym.Name}[{String.Join(comma, self.Arguments)}]"

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

    override self.ToString() =
      match self with
      | Unit -> "()"
      | Guid -> "Guid"
      | Int32 -> "int"
      | Int64 -> "int:Signed64"
      | Float32 -> "float:Float32"
      | Float64 -> "float"
      | Decimal -> "decimal"
      | Bool -> "boolean"
      | String -> "string"
      | DateTime -> "time:Utc"
      | DateOnly -> "time:Date"
      | TimeSpan -> "time:Interval"
