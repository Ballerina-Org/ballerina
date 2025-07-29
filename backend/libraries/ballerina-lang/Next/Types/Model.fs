namespace Ballerina.DSL.Next.Types

module Model =
  open Ballerina.Collections.Sum
  open Ballerina.Reader.WithError
  open Ballerina.Errors
  open System

  type TypeParameter = { Name: string; Kind: Kind }

  and Kind =
    | Symbol
    | Star
    | Arrow of Kind * Kind

  and TypeIdentifier = { Name: string }

  and TypeSymbol = { Name: string; Guid: Guid }

  and TypeVar =
    { Name: string
      Guid: Guid }

    override v.ToString() = v.Name

  and TypeVarIdentifier =
    { Name: string }

    override v.ToString() = v.Name

  and TypeExpr =
    | Primitive of PrimitiveType
    | Lookup of string
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
    { Identifier: TypeIdentifier
      Type: TypeExpr }

  and TypeValue =
    | Primitive of PrimitiveType
    | Var of TypeVar
    | Lookup of TypeIdentifier
    | Lambda of TypeParameter * TypeExpr
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
    | Int
    | Decimal
    | Bool
    | String
    | DateTime
    | DateOnly
