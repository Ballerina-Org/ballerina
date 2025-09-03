namespace Ballerina.DSL.Next.Terms

module Model =
  open Ballerina.Collections.Sum
  open Ballerina.Reader.WithError
  open Ballerina.Errors
  open System
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Types.Patterns

  type Var =
    { Name: string }

    static member Create name : Var = { Var.Name = name }

  type Expr<'T> =
    | TypeLambda of TypeParameter * Expr<'T>
    | TypeApply of Expr<'T> * 'T
    | Lambda of Var * Option<'T> * Expr<'T>
    | Apply of Expr<'T> * Expr<'T>
    | Let of Var * Expr<'T> * Expr<'T>
    | TypeLet of string * 'T * Expr<'T>
    | RecordCons of List<Identifier * Expr<'T>>
    | UnionCons of Identifier * Expr<'T>
    | TupleCons of List<Expr<'T>>
    | SumCons of SumConsSelector * Expr<'T>
    | RecordDes of Expr<'T> * Identifier
    | UnionDes of Map<Identifier, CaseHandler<'T>>
    | TupleDes of Expr<'T> * TupleDesSelector
    | SumDes of List<CaseHandler<'T>>
    | Primitive of PrimitiveValue
    | Lookup of Identifier
    | If of Expr<'T> * Expr<'T> * Expr<'T>

  and SumConsSelector = { Case: int; Count: int }
  and TupleDesSelector = { Index: int }

  and CaseHandler<'T> = Var * Expr<'T>

  and PrimitiveValue =
    | Int of int
    | Decimal of decimal
    | Bool of bool
    | Guid of Guid
    | String of string
    | Date of DateOnly
    | DateTime of DateTime
    | Unit

  and Value<'T> =
    | TypeLambda of TypeParameter * Expr<'T>
    | Lambda of Var * Expr<'T>
    | Record of Map<TypeSymbol, Value<'T>>
    | UnionCase of TypeSymbol * Value<'T>
    | Tuple of List<Value<'T>>
    | Sum of int * Value<'T>
    | Primitive of PrimitiveValue
    | Var of Var
