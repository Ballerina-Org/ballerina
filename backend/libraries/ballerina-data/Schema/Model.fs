namespace Ballerina.Data.Schema

module Model =
  open Ballerina.Data.Arity.Model
  open Ballerina.DSL.Next.Terms.Model
  open Ballerina.StdLib.OrderPreservingMap
  open Ballerina.DSL.Next.Types

  type EntityName = { EntityName: string }
  type LookupName = { LookupName: string }

  type LookupMethod =
    | Get
    | GetMany
    | Create
    | Delete
    | Link
    | Unlink

  type EntityMethod =
    | Get
    | GetMany
    | Create
    | Delete

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

  and Updater<'Type, 'Id when 'Id: comparison> =
    { Path: List<UpdaterPathStep>
      Condition: Expr<'Type, 'Id>
      Expr: Expr<'Type, 'Id> }

  and EntityDescriptor<'Type, 'Id when 'Id: comparison> =
    { Type: 'Type
      Methods: Set<EntityMethod>
      Updaters: List<Updater<'Type, 'Id>>
      Predicates: Map<string, Expr<'Type, 'Id>> }

  and LookupDescriptor =
    { Source: EntityName
      Target: EntityName
      Forward: DirectedLookupDescriptor
      Backward: Option<LookupName * DirectedLookupDescriptor> }

  type Schema<'Type, 'Id when 'Id: comparison> =
    { Types: OrderedMap<Identifier, 'Type>
      Entities: Map<EntityName, EntityDescriptor<'Type, 'Id>>
      Lookups: Map<LookupName, LookupDescriptor> }
