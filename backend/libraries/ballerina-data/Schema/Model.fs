namespace Ballerina.Data

module Model =
  open Ballerina.Data.Arity.Model
  open Ballerina.DSL.Next.Terms.Model

  type LookupMethod =
    | Get
    | GetMany
    | Create
    | Delete
    | Update
    | Link
    | Unlink

  type EntityMethod =
    | Get
    | GetMany
    | Create
    | Delete
    | Update

  type DirectedLookupDescriptor =
    { Arity: LookupArity
      Methods: Set<LookupMethod>
      Path: List<UpdaterPathStep> }

  and UpdaterPathStep =
    | Field of string
    | TupleItem of int
    | ListItem of Var
    | UnionCase of string * Var
    | SumCase of int * Var

  and Updater<'Type> =
    { Path: List<UpdaterPathStep>
      Condition: Expr<'Type>
      Expr: Expr<'Type> }

  and EntityDescriptor<'Type> =
    { Type: 'Type
      Methods: Set<EntityMethod>
      Updaters: List<Updater<'Type>>
      Predicates: Map<string, Expr<'Type>> }

  and LookupDescriptor =
    { Source: string
      Target: string
      Forward: DirectedLookupDescriptor
      Backward: Option<string * DirectedLookupDescriptor> }

  type Schema<'Type> =
    { Entities: Map<string, EntityDescriptor<'Type>>
      Lookups: Map<string, LookupDescriptor> }
