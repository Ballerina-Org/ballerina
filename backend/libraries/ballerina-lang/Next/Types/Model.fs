namespace Ballerina.DSL.Next.Types

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
    | NewSymbol of string
    | Lookup of Identifier
    | Apply of TypeExpr * TypeExpr
    | Lambda of TypeParameter * TypeExpr
    | Arrow of TypeExpr * TypeExpr
    | Record of List<TypeExpr * TypeExpr>
    | Tuple of List<TypeExpr>
    | Union of List<TypeExpr * TypeExpr>
    | List of TypeExpr
    | Set of TypeExpr
    | Map of TypeExpr * TypeExpr
    | KeyOf of TypeExpr
    | Sum of List<TypeExpr>
    | Flatten of TypeExpr * TypeExpr
    | Exclude of TypeExpr * TypeExpr
    | Rotate of TypeExpr

  and TypeBinding =
    { Identifier: Identifier
      Type: TypeExpr }

  and TypeValue =
    | Primitive of PrimitiveType
    | Var of TypeVar
    | Lookup of Identifier
    | Lambda of TypeParameter * TypeExpr
    | Apply of TypeVar * TypeValue
    | Arrow of TypeValue * TypeValue
    | Record of Map<TypeSymbol, TypeValue>
    | Tuple of List<TypeValue>
    | Union of Map<TypeSymbol, TypeValue>
    | Sum of List<TypeValue>
    | List of TypeValue
    | Set of TypeValue
    | Map of TypeValue * TypeValue

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
