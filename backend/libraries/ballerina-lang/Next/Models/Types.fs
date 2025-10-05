namespace Ballerina.DSL.Next.Types

[<AutoOpen>]
module Model =
  open System

  type Identifier =
    | LocalScope of string
    | FullyQualified of List<string> * string

    override id.ToString() =
      match id with
      | LocalScope name -> name
      | FullyQualified(names, name) -> String.Join(".", names) + "." + name

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
    | Record of WithTypeExprSourceMapping<Map<TypeSymbol, TypeValue>>
    | Tuple of WithTypeExprSourceMapping<List<TypeValue>>
    | Union of WithTypeExprSourceMapping<Map<TypeSymbol, TypeValue>>
    | Sum of WithTypeExprSourceMapping<List<TypeValue>>
    | Set of WithTypeExprSourceMapping<TypeValue>
    | Map of WithTypeExprSourceMapping<TypeValue * TypeValue>
    | Imported of ImportedTypeValue // FIXME: This should also have an orig name, implement once the extension is implemented completely

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
      UnionLike: Option<Map<TypeSymbol, TypeExpr>>
      RecordLike: Option<Map<TypeSymbol, TypeExpr>> }

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
