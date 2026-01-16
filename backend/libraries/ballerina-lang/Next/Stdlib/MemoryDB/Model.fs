namespace Ballerina.DSL.Next.StdLib.MemoryDB

[<AutoOpen>]
module Model =
  open System
  open Ballerina.DSL.Next.Types
  open Ballerina.DSL.Next.Terms

  type MutableMemoryDB<'ext when 'ext: comparison> =
    { mutable entities: Map<SchemaEntityName, Map<Value<TypeValue<'ext>, 'ext>, Value<TypeValue<'ext>, 'ext>>> }

  type MemoryDBEvalProperty<'ext> =
    { PropertyName: LocalIdentifier
      Path: SchemaPath<'ext>
      Body: Expr<TypeValue<'ext>, ResolvedIdentifier, 'ext> }

  type MemoryDBValues<'ext when 'ext: comparison> =
    | EntityRef of Schema<'ext> * MutableMemoryDB<'ext> * SchemaEntity<'ext>
    | EvalProperty of MemoryDBEvalProperty<'ext>
    | StripProperty of MemoryDBEvalProperty<'ext>
    | Create of {| EntityRef: Option<Schema<'ext> * MutableMemoryDB<'ext> * SchemaEntity<'ext>> |}
    | Update of {| EntityRef: Option<Schema<'ext> * MutableMemoryDB<'ext> * SchemaEntity<'ext>> |}
    | Delete of {| EntityRef: Option<Schema<'ext> * MutableMemoryDB<'ext> * SchemaEntity<'ext>> |}
    | GetById of {| EntityRef: Option<Schema<'ext> * MutableMemoryDB<'ext> * SchemaEntity<'ext>> |}
    | Run
    | TypeAppliedRun of Schema<'ext> * MutableMemoryDB<'ext>
