namespace Ballerina.DSL.Next.Types

[<AutoOpen>]
module Model =
  open System
  open Ballerina.StdLib.String
  open Ballerina.StdLib.OrderPreservingMap
  open Ballerina.StdLib.Formats
  open Ballerina.Cat.Collections.OrderedMap
  open Ballerina.StdLib.Object
  open Ballerina.LocalizedErrors
  open Ballerina.Errors
  open Ballerina
  open Ballerina.Collections.Sum
  open Ballerina.Collections.Map
  open Ballerina.Collections.NonEmptyList

  type LocalIdentifier =
    { Name: string }

    override self.ToString() = self.Name

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

    static member Compare (id1: ResolvedIdentifier) (id2: ResolvedIdentifier) =
      match compare id1.Name id2.Name with
      | 0 ->
        match id1.Type, id2.Type with
        | Some t1, Some t2 -> compare t1 t2
        | Some _, None -> -1
        | None, Some _ -> 1
        | None, None ->
          match compare id1.Module id2.Module with
          | 0 -> compare id1.Assembly id2.Assembly
          | comparison -> comparison
      | comparison -> comparison

    static member Create
      (assembly: string, module_: string, type_: Option<string>, name: string)
      : ResolvedIdentifier =
      { Assembly = assembly
        Module = module_
        Type = type_
        Name = name }

    static member FromIdentifier(id: Identifier) : ResolvedIdentifier =
      match id with
      | Identifier.FullyQualified(t :: _, name) ->
        { Assembly = ""
          Module = ""
          Type = Some t
          Name = name }
      | Identifier.FullyQualified(_, name)
      | Identifier.LocalScope(name) ->
        { Assembly = ""
          Module = ""
          Type = None
          Name = name }

    static member FromLocalIdentifier
      (id: LocalIdentifier)
      : ResolvedIdentifier =
      { Assembly = ""
        Module = ""
        Type = None
        Name = id.Name }

    static member Create(name: string) : ResolvedIdentifier =
      { Assembly = ""
        Module = ""
        Type = None
        Name = name }

    static member Create(``type``: string, name: string) =
      { Assembly = ""
        Module = ""
        Type = Some ``type``
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

    static member Resolve
      (id: Identifier, scope: TypeCheckScope)
      : ResolvedIdentifier =
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

    member scope.Resolve(id: Identifier) : ResolvedIdentifier =
      TypeCheckScope.Resolve(id, scope)



  type TypeParameter =
    { Name: string
      Kind: Kind }

    override p.ToString() = $"[{p.Name}:{p.Kind}]"

  and Kind =
    | Symbol
    | Star
    | Schema
    | QueryRow
    | Arrow of Kind * Kind

    override k.ToString() =
      match k with
      | Symbol -> "Symbol"
      | Star -> "*"
      | Schema -> "Schema"
      | QueryRow -> "QueryRow"
      | Arrow(k1, k2) -> $"({k1} -> {k2})"

  and TypeSymbol =
    { Name: Identifier
      Guid: Guid }

    override s.ToString() = s.Name.ToString()

  and TypeVar =
    { Name: string
      Synthetic: bool // this is the case for type vars created as placeholders - they should all be instantiated away during type checking
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

  and SumConsSelector =
    { Case: int
      Count: int }

    override self.ToString() = $"{self.Case}Of{self.Count}"

  and TupleDesSelector =
    { Index: int }

    override self.ToString() = self.Index.ToString()

  and SchemaEntityName =
    { Name: string }

    override self.ToString() = self.Name

  and SchemaRelationName =
    { Name: string }

    override self.ToString() = self.Name

  and SchemaEntityPropertyExpr<'valueExt> =
    { Name: LocalIdentifier
      Path: Option<SchemaPathExpr>
      Type: TypeExpr<'valueExt>
      Body: Expr<TypeExpr<'valueExt>, Identifier, 'valueExt> }

    override self.ToString() =
      $"property {self.Name}: {self.Type} = {self.Body}"

  and SchemaEntityVectorExpr<'valueExt> =
    { Name: LocalIdentifier
      Body: Expr<TypeExpr<'valueExt>, Identifier, 'valueExt> }

    override self.ToString() = $"vector {self.Name} = {self.Body}"

  and SchemaEntityHook =
    | Creating
    | Created
    | Updating
    | Updated
    | Deleting
    | Deleted
    | Background
    | CanCreate
    | CanRead
    | CanUpdate
    | CanDelete

    override self.ToString() =
      match self with
      | Creating -> "creating"
      | Created -> "created"
      | Updating -> "updating"
      | Updated -> "updated"
      | Deleting -> "deleting"
      | Deleted -> "deleted"
      | Background -> "background"
      | CanCreate -> "canCreate"
      | CanRead -> "canRead"
      | CanUpdate -> "canUpdate"
      | CanDelete -> "canDelete"

  and SchemaEntityHooksExpr<'valueExt> =
    { OnCreating: Option<Expr<TypeExpr<'valueExt>, Identifier, 'valueExt>>
      OnCreated: Option<Expr<TypeExpr<'valueExt>, Identifier, 'valueExt>>
      OnUpdating: Option<Expr<TypeExpr<'valueExt>, Identifier, 'valueExt>>
      OnUpdated: Option<Expr<TypeExpr<'valueExt>, Identifier, 'valueExt>>
      OnDeleting: Option<Expr<TypeExpr<'valueExt>, Identifier, 'valueExt>>
      OnDeleted: Option<Expr<TypeExpr<'valueExt>, Identifier, 'valueExt>>
      OnBackground: Option<Expr<TypeExpr<'valueExt>, Identifier, 'valueExt>>
      CanCreate: Option<Expr<TypeExpr<'valueExt>, Identifier, 'valueExt>>
      CanRead: Option<Expr<TypeExpr<'valueExt>, Identifier, 'valueExt>>
      CanUpdate: Option<Expr<TypeExpr<'valueExt>, Identifier, 'valueExt>>
      CanDelete: Option<Expr<TypeExpr<'valueExt>, Identifier, 'valueExt>> }

    override self.ToString() =
      let enabledHooks =
        [ ("creating", self.OnCreating)
          ("created", self.OnCreated)
          ("updating", self.OnUpdating)
          ("updated", self.OnUpdated)
          ("deleting", self.OnDeleting)
          ("deleted", self.OnDeleted)
          ("background", self.OnBackground)
          ("canCreate", self.CanCreate)
          ("canRead", self.CanRead)
          ("canUpdate", self.CanUpdate)
          ("canDelete", self.CanDelete) ]
        |> List.choose (fun (name, body) ->
          body |> Option.map (fun b -> $"{name} -> {b}"))

      let hooks = String.Join("; ", enabledHooks)
      $"hooks {{ {hooks} }}"

  and SchemaEntityExpr<'valueExt> =
    { Name: SchemaEntityName
      Location: Location
      Type: TypeExpr<'valueExt>
      Id: TypeExpr<'valueExt>
      Properties: List<SchemaEntityPropertyExpr<'valueExt>>
      Vectors: List<SchemaEntityVectorExpr<'valueExt>>
      Hooks: SchemaEntityHooksExpr<'valueExt> }

    override self.ToString() =
      let props =
        self.Properties |> List.map (fun p -> p.ToString()) |> String.join "; "

      let vectors =
        self.Vectors |> List.map (fun v -> v.ToString()) |> String.join "; "

      $"entity {self.Name} : {self.Type} id {self.Id} {{ {props} }} vectors {{ {vectors} }} {self.Hooks}"

  and Cardinality =
    | Zero
    | One
    | Many

    override self.ToString() =
      match self with
      | Zero -> "0"
      | One -> "1"
      | Many -> "*"

  and SchemaRelationCardinality =
    { From: Cardinality
      To: Cardinality }

    override c.ToString() =
      let fromStr =
        match c.From with
        | Zero -> "0"
        | One -> "1"
        | Many -> "*"

      let toStr =
        match c.To with
        | Zero -> "0"
        | One -> "1"
        | Many -> "*"

      $"{fromStr}..{toStr}"

  and SchemaPathTypeDecompositionExpr =
    | Field of Identifier
    | Item of TupleDesSelector
    | UnionCase of Identifier
    | SumCase of SumConsSelector
    | Iterator of
      {| Mapper: Identifier
         Container: Identifier
         TypeDef: Identifier |}

    override sps.ToString() =
      match sps with
      | Field name -> $"Field({name})"
      | Item i -> $"Item({i})"
      | UnionCase name -> $"UnionCase({name})"
      | SumCase name -> $"SumCase({name})"
      | Iterator collection ->
        $"Iterator({collection.Mapper}::{collection.TypeDef})"

  and SchemaPathSegmentExpr =
    Option<LocalIdentifier> * SchemaPathTypeDecompositionExpr

  and SchemaPathExpr = List<SchemaPathSegmentExpr>

  and SchemaRelationHook =
    | Linking
    | Unlinking
    | Linked
    | Unlinked

    override self.ToString() =
      match self with
      | Linking -> "linking"
      | Unlinking -> "unlinking"
      | Linked -> "linked"
      | Unlinked -> "unlinked"

  and SchemaRelationHooksExpr<'valueExt> =
    { OnLinking: Option<Expr<TypeExpr<'valueExt>, Identifier, 'valueExt>>
      OnUnlinking: Option<Expr<TypeExpr<'valueExt>, Identifier, 'valueExt>>
      OnLinked: Option<Expr<TypeExpr<'valueExt>, Identifier, 'valueExt>>
      OnUnlinked: Option<Expr<TypeExpr<'valueExt>, Identifier, 'valueExt>> }

    override self.ToString() =
      let enabledHooks =
        [ ("linking", self.OnLinking)
          ("unlinking", self.OnUnlinking)
          ("linked", self.OnLinked)
          ("unlinked", self.OnUnlinked) ]
        |> List.choose (fun (name, body) ->
          body |> Option.map (fun b -> $"{name} -> {b}"))

      let hooks = String.Join("; ", enabledHooks)
      $"hooks {{ {hooks} }}"

  and SchemaRelationExpr<'valueExt> =
    { Name: SchemaRelationName
      Location: Location
      From: Identifier * Option<SchemaPathExpr>
      To: Identifier * Option<SchemaPathExpr>
      Cardinality: Option<SchemaRelationCardinality>
      Hooks: SchemaRelationHooksExpr<'valueExt> }

    override self.ToString() =
      let fromStr = fst self.From
      let toStr = fst self.To

      let cardStr =
        self.Cardinality
        |> Option.map (fun c -> $" {c}")
        |> Option.defaultValue ""

      $"relation {self.Name} : {fromStr} -> {toStr}{cardStr} {self.Hooks}"

  and SchemaExpr<'valueExt> =
    { DeclaredAtForNominalEquality: Location
      Includes:
        Option<
          LocalIdentifier *
          List<SchemaEntityName * SchemaEntityHooksExpr<'valueExt>> *
          List<SchemaRelationName * SchemaRelationHooksExpr<'valueExt>>
         >
      Entities: List<SchemaEntityExpr<'valueExt>>
      Relations: List<SchemaRelationExpr<'valueExt>> }

    override self.ToString() =
      let entities =
        self.Entities |> List.map (fun e -> e.ToString()) |> String.join "; "

      let relations =
        self.Relations |> List.map (fun r -> r.ToString()) |> String.join "; "

      $"schema {{ entities: [{entities}] relations: [{relations}] }}"

  and TypeExpr<'valueExt> =
    | FromTypeValue of TypeValue<'valueExt>
    | Primitive of PrimitiveType
    | RecordDes of TypeExpr<'valueExt> * Sum<LocalIdentifier, int>
    | Let of string * TypeExpr<'valueExt> * TypeExpr<'valueExt>
    | LetSymbols of List<string> * SymbolsKind * TypeExpr<'valueExt>
    | NewSymbol of string
    | Lookup of Identifier
    | Apply of TypeExpr<'valueExt> * TypeExpr<'valueExt>
    | Lambda of TypeParameter * TypeExpr<'valueExt>
    | Arrow of TypeExpr<'valueExt> * TypeExpr<'valueExt>
    | Record of List<TypeExpr<'valueExt> * TypeExpr<'valueExt>>
    | Tuple of List<TypeExpr<'valueExt>>
    | Union of List<TypeExpr<'valueExt> * TypeExpr<'valueExt>>
    | Set of TypeExpr<'valueExt>
    | KeyOf of TypeExpr<'valueExt>
    | Sum of List<TypeExpr<'valueExt>>
    | Flatten of TypeExpr<'valueExt> * TypeExpr<'valueExt>
    | Exclude of TypeExpr<'valueExt> * TypeExpr<'valueExt>
    | Rotate of TypeExpr<'valueExt>
    | Schema of SchemaExpr<'valueExt>
    | Entities of TypeExpr<'valueExt>
    | Relations of TypeExpr<'valueExt>
    | Entity of
      s: TypeExpr<'valueExt> *
      e: TypeExpr<'valueExt> *
      e': TypeExpr<'valueExt> *
      e_id: TypeExpr<'valueExt>
    | Relation of
      s: TypeExpr<'valueExt> *
      f: TypeExpr<'valueExt> *
      f': TypeExpr<'valueExt> *
      f_id: TypeExpr<'valueExt> *
      t: TypeExpr<'valueExt> *
      t': TypeExpr<'valueExt> *
      t_id: TypeExpr<'valueExt>
    | RelationLookupOne of
      s: TypeExpr<'valueExt> *
      f_id: TypeExpr<'valueExt> *
      t_id: TypeExpr<'valueExt> *
      t': TypeExpr<'valueExt>
    | RelationLookupOption of
      s: TypeExpr<'valueExt> *
      f_id: TypeExpr<'valueExt> *
      t_id: TypeExpr<'valueExt> *
      t': TypeExpr<'valueExt>
    | RelationLookupMany of
      s: TypeExpr<'valueExt> *
      f_id: TypeExpr<'valueExt> *
      t_id: TypeExpr<'valueExt> *
      t': TypeExpr<'valueExt>
    | FromQueryRow
    | QueryRow of TypeQueryRowExpr<'valueExt>
    | Imported of ImportedTypeValue<'valueExt>

    override self.ToString() =
      match self with
      | FromTypeValue tv -> tv.ToString()
      | Imported i -> i.ToString()
      | Primitive p -> p.ToString()
      | RecordDes(t, selector) ->
        match selector with
        | Sum.Left field_name -> $"{t.ToString()}.{field_name.Name}"
        | Sum.Right index -> $"{t.ToString()}.{index}"
      | Let(name, value, body) -> $"Let {name} = {value} in {body})"
      | LetSymbols(names, symbolsKind, body) ->
        let comma = ", " in
        $"LetSymbols({String.Join(comma, names)}):{symbolsKind} in {body})"
      | NewSymbol name -> $"NewSymbol({name})"
      | Lookup id -> id.ToString()
      | Apply(f, a) -> $"({f})[{a}]"
      | Lambda(param, body) -> $"({param.Name}::{param.Kind} => {body})"
      | Arrow(t1, t2) -> $"({t1} -> {t2})"
      | Record fields ->
        let comma = ", "
        let fieldStrs = fields |> List.map (fun (name, typ) -> $"{name}: {typ}")
        $"{{{String.Join(comma, fieldStrs)}}}"
      | Tuple types ->
        let comma = " * "
        $"({String.Join(comma, types)})"
      | Union types ->
        let comma = " | "
        let typeStrs = types |> List.map (fun (name, typ) -> $"{name}: {typ}")
        $"({String.Join(comma, typeStrs)})"
      | Set t -> $"Set[{t}]"
      | KeyOf t -> $"KeyOf[{t}]"
      | Sum types ->
        let comma = " + "
        $"({String.Join(comma, types)})"

      | Flatten(t1, t2) -> $"Flatten[{t1}, {t2}]"
      | Exclude(t1, t2) -> $"Exclude[{t1}, {t2}]"
      | Rotate t -> $"Rotate[{t}]"
      | Schema s ->
        $"Schema[{s.Entities.Length} Entities, {s.Relations.Length} Relations]"
      | Entities e -> $"SchemaEntities[{e}]"
      | Relations e -> $"SchemaRelations[{e}]"
      | Entity(s, e, e_with_props, id) ->
        $"SchemaEntity[Schema[{s}][{e}][{e_with_props}][{id}]"
      | Relation(s, f, f_with_props, f_id, t, t_with_props, t_id) ->
        $"SchemaRelation[Schema[{s}][{f}][{f_with_props}][{f_id}][{t}][{t_with_props}][{t_id}]"
      | RelationLookupOne(s, t', f_id, t_id) ->
        $"SchemaLookupOne[Schema[{s}][{t'}][{f_id}][{t_id}]"
      | RelationLookupOption(s, t', f_id, t_id) ->
        $"SchemaLookupOption[Schema[{s}][{t'}][{f_id}][{t_id}]"
      | RelationLookupMany(s, t', f_id, t_id) ->
        $"SchemaLookupMany[Schema[{s}][{t'}][{f_id}][{t_id}]"
      | FromQueryRow -> $"FromQueryRow"
      | QueryRow q -> $"QueryRow[{q}]"


  and TypeQueryRowExpr<'valueExt> =
    | PrimaryKey of TypeExpr<'valueExt>
    | Json of TypeExpr<'valueExt>
    | PrimitiveType of PrimitiveType * IsNullable: bool
    | Tuple of List<TypeExpr<'valueExt>>
    | Record of Map<LocalIdentifier, TypeExpr<'valueExt>>

    override self.ToString() =
      match self with
      | PrimaryKey t -> $"PrimaryKey[{t}]"
      | Json t -> $"Json[{t}]"
      | PrimitiveType(p, isNullable) ->
        $"PrimitiveType[{p}, IsNullable={isNullable}]"
      | Tuple types ->
        let comma = " * "
        $"({String.Join(comma, types)})"
      | Record fields ->
        let comma = ", "

        let fieldStrs =
          fields |> Map.toList |> List.map (fun (name, typ) -> $"{name}: {typ}")

        $"{{{String.Join(comma, fieldStrs)}}}"

  and TypeBinding<'valueExt> =
    { Identifier: Identifier
      Type: TypeExpr<'valueExt> }

    override self.ToString() = $"{self.Identifier}: {self.Type}"

  and TypeVariablesScope<'valueExt> = Map<string, TypeValue<'valueExt> * Kind>
  and TypeParametersScope = Map<string, Kind>

  // all the applicables in type expressions minus lambdas (clear from type expr eval impl)
  and SymbolicTypeApplication<'valueExt> =
    | FromQueryRow of Identifier
    | Lookup of Identifier * TypeValue<'valueExt>
    | Application of SymbolicTypeApplication<'valueExt> * TypeValue<'valueExt>

    override sta.ToString() =
      match sta with
      | FromQueryRow t -> $"FromQueryRow[{t}]"
      | Lookup(id, arg) -> $"{id}[{arg}]"
      | Application(f, a) -> $"({f})[{a}]"

  and SchemaPathTypeDecomposition<'valueExt> =
    | Field of ResolvedIdentifier
    | Item of TupleDesSelector
    | UnionCase of ResolvedIdentifier
    | SumCase of SumConsSelector
    | Iterator of
      {| Mapper: RunnableExpr<'valueExt>
         Container: TypeValue<'valueExt>
         TypeDef: TypeValue<'valueExt> |}

    override sps.ToString() =
      match sps with
      | Field name -> $"Field({name})"
      | Item i -> $"Item({i})"
      | UnionCase name -> $"UnionCase({name})"
      | SumCase name -> $"SumCase({name})"
      | Iterator collection ->
        $"Iterator({collection.Mapper}::{collection.TypeDef})"

  and SchemaPathSegment<'valueExt> =
    Option<LocalIdentifier> * SchemaPathTypeDecomposition<'valueExt>

  and SchemaPath<'valueExt> = List<SchemaPathSegment<'valueExt>>

  and SchemaEntityProperty<'valueExt> =
    { PropertyName: LocalIdentifier
      Path: SchemaPath<'valueExt>
      ReturnType: TypeValue<'valueExt>
      ReturnKind: Kind
      Body: RunnableExpr<'valueExt> }

    override self.ToString() =
      $"property {self.PropertyName}: {self.ReturnType} = {self.Body}"

  and SchemaEntityVector<'valueExt> =
    { VectorName: LocalIdentifier
      Body: RunnableExpr<'valueExt> }

    override self.ToString() =
      $"vector {self.VectorName} = {self.Body}"

  and SchemaEntityHooks<'valueExt> =
    { OnCreating: Option<RunnableExpr<'valueExt>>
      OnCreated: Option<RunnableExpr<'valueExt>>
      OnUpdating: Option<RunnableExpr<'valueExt>>
      OnUpdated: Option<RunnableExpr<'valueExt>>
      OnDeleting: Option<RunnableExpr<'valueExt>>
      OnDeleted: Option<RunnableExpr<'valueExt>>
      OnBackground: Option<RunnableExpr<'valueExt>>
      CanCreate: Option<RunnableExpr<'valueExt>>
      CanRead: Option<RunnableExpr<'valueExt>>
      CanUpdate: Option<RunnableExpr<'valueExt>>
      CanDelete: Option<RunnableExpr<'valueExt>> }

    override self.ToString() =
      let enabledHooks =
        [ ("creating", self.OnCreating)
          ("created", self.OnCreated)
          ("updating", self.OnUpdating)
          ("updated", self.OnUpdated)
          ("deleting", self.OnDeleting)
          ("deleted", self.OnDeleted)
          ("background", self.OnBackground)
          ("canCreate", self.CanCreate)
          ("canRead", self.CanRead)
          ("canUpdate", self.CanUpdate)
          ("canDelete", self.CanDelete) ]
        |> List.choose (fun (name, body) ->
          body |> Option.map (fun b -> $"{name} -> {b}"))

      let hooks = String.Join("; ", enabledHooks)
      $"hooks {{ {hooks} }}"

  and SchemaEntity<'valueExt> =
    { Name: SchemaEntityName
      TypeOriginal: TypeValue<'valueExt>
      TypeWithProps: TypeValue<'valueExt>
      Id: TypeValue<'valueExt>
      Properties: List<SchemaEntityProperty<'valueExt>>
      Vectors: List<SchemaEntityVector<'valueExt>>
      Hooks: SchemaEntityHooks<'valueExt> }

    override entity.ToString() =
      $"{entity.Name.Name} (Id: {entity.Id}): {entity.TypeOriginal} -> {entity.TypeWithProps}"

  and SchemaRelationHooks<'valueExt> =
    { OnLinking: Option<RunnableExpr<'valueExt>>
      OnUnlinking: Option<RunnableExpr<'valueExt>>
      OnLinked: Option<RunnableExpr<'valueExt>>
      OnUnlinked: Option<RunnableExpr<'valueExt>> }

    override self.ToString() =
      let enabledHooks =
        [ ("linking", self.OnLinking)
          ("unlinking", self.OnUnlinking)
          ("linked", self.OnLinked)
          ("unlinked", self.OnUnlinked) ]
        |> List.choose (fun (name, body) ->
          body |> Option.map (fun b -> $"{name} -> {b}"))

      let hooks = String.Join("; ", enabledHooks)
      $"hooks {{ {hooks} }}"

  and SchemaRelation<'valueExt> =
    { Name: SchemaRelationName
      From: Identifier
      To: Identifier
      Cardinality: Option<SchemaRelationCardinality>
      Hooks: SchemaRelationHooks<'valueExt> }

    override r.ToString() =
      let cardStr =
        match r.Cardinality with
        | Some c -> $" ({c})"
        | None -> ""

      $"{r.Name.Name}: {r.From} -> {r.To}{cardStr}"

  and Schema<'valueExt> =
    { DeclaredAtForNominalEquality: Location
      Source: TypeExprSourceMapping<'valueExt>
      Entities: OrderedMap<SchemaEntityName, SchemaEntity<'valueExt>>
      Relations: OrderedMap<SchemaRelationName, SchemaRelation<'valueExt>>
      Included: Option<Schema<'valueExt>> }

    override s.ToString() =
      let entities =
        s.Entities
        |> OrderedMap.toList
        |> List.map (fun (_name, entity) -> $"{entity}")
        |> String.join "; "

      let relations =
        s.Relations
        |> OrderedMap.toList
        |> List.map (fun (_name, relation) -> $"{relation}")
        |> String.join "; "

      $"[Entities: {entities}, Relations: {relations}]"


  and TypeValue<'valueExt> =
    | Primitive of WithSourceMapping<PrimitiveType, 'valueExt>
    | Var of TypeVar
    | Lookup of Identifier // TODO: Figure out what to do with this (orig name wise) after recursion in type checking is implement correctly
    | Lambda of
      WithSourceMapping<TypeParameter * TypeExpr<'valueExt>, 'valueExt>
    | Application of
      WithSourceMapping<SymbolicTypeApplication<'valueExt>, 'valueExt>
    | Arrow of
      WithSourceMapping<TypeValue<'valueExt> * TypeValue<'valueExt>, 'valueExt>
    | Record of
      WithSourceMapping<
        OrderedMap<TypeSymbol, TypeValue<'valueExt> * Kind>,
        'valueExt
       >
    | Tuple of WithSourceMapping<List<TypeValue<'valueExt>>, 'valueExt>
    | Union of
      WithSourceMapping<OrderedMap<TypeSymbol, TypeValue<'valueExt>>, 'valueExt>
    | Sum of WithSourceMapping<List<TypeValue<'valueExt>>, 'valueExt>
    | Set of WithSourceMapping<TypeValue<'valueExt>, 'valueExt>
    | Imported of ImportedTypeValue<'valueExt> // FIXME: This should also have an orig name, implement once the extension is implemented completely
    | Schema of Schema<'valueExt>
    | Entities of Schema<'valueExt>
    | Relations of Schema<'valueExt>
    | Entity of
      Schema<'valueExt> *
      e: TypeValue<'valueExt> *
      e': TypeValue<'valueExt> *
      e_id: TypeValue<'valueExt>
    | Relation of
      Schema<'valueExt> *
      rn: SchemaRelationName *
      c: Option<SchemaRelationCardinality> *
      f: TypeValue<'valueExt> *
      f': TypeValue<'valueExt> *
      f_id: TypeValue<'valueExt> *
      t: TypeValue<'valueExt> *
      t': TypeValue<'valueExt> *
      t_id: TypeValue<'valueExt>
    | RelationLookupOption of
      Schema<'valueExt> *
      source_id: TypeValue<'valueExt> *
      target': TypeValue<'valueExt> *
      target_id: TypeValue<'valueExt>
    | RelationLookupOne of
      Schema<'valueExt> *
      source_id: TypeValue<'valueExt> *
      target': TypeValue<'valueExt> *
      target_id: TypeValue<'valueExt>
    | RelationLookupMany of
      Schema<'valueExt> *
      source_id: TypeValue<'valueExt> *
      target': TypeValue<'valueExt> *
      target_id: TypeValue<'valueExt>
    | ForeignKeyRelation of
      Schema<'valueExt> *
      rn: SchemaRelationName *
      f: TypeValue<'valueExt> *
      f': TypeValue<'valueExt> *
      f_id: TypeValue<'valueExt> *
      t: TypeValue<'valueExt> *
      t': TypeValue<'valueExt> *
      t_id: TypeValue<'valueExt>
    | QueryTypeFunction
    | QueryRow of TypeQueryRow<'valueExt>

    override self.ToString() =
      let schemaNameFromSource (source: TypeExprSourceMapping<'valueExt>) =
        match source with
        | OriginExprTypeLet(id, _) -> Some(id.ToString())
        | OriginTypeExpr(TypeExpr.Lookup id) -> Some(id.ToString())
        | _ -> None

      match self with
      | Union({ typeExprSource = OriginExprTypeLet(id, _) })
      | Record({ typeExprSource = OriginExprTypeLet(id, _) }) -> id.ToString()
      | Record({ typeExprSource = OriginTypeExpr(TypeExpr.Lookup id) })
      | Primitive({ typeExprSource = OriginTypeExpr(TypeExpr.Lookup id) })
      | Lambda({ typeExprSource = OriginTypeExpr(TypeExpr.Lookup id) })
      | Arrow({ typeExprSource = OriginTypeExpr(TypeExpr.Lookup id) })
      | Tuple({ typeExprSource = OriginTypeExpr(TypeExpr.Lookup id) })
      | Union({ typeExprSource = OriginTypeExpr(TypeExpr.Lookup id) })
      | Sum({ typeExprSource = OriginTypeExpr(TypeExpr.Lookup id) }) ->
        id.ToString()
      | Application { value = a } -> a.ToString()
      | Imported i -> i.ToString()
      | Primitive p -> p.value.ToString()
      | Lookup id -> id.ToString()
      | Var v -> v.Name.ToString()
      // | Apply({ value = (f, a) }) -> $"({f})[{a}]"
      | Lambda({ value = (param, body) }) -> $"({param.Name} => {body})"
      | Arrow({ value = (t1, t2) }) -> $"({t1} -> {t2})"
      | Record({ value = fields }) ->
        let comma = ", "

        let fieldStrs =
          fields
          |> OrderedMap.toList
          |> List.map (fun (name, typ) -> $"{name}: {typ}")

        $"{{{String.Join(comma, fieldStrs)}}}"
      | Tuple({ value = types }) ->
        let comma = " * "
        $"({String.Join(comma, types)})"
      | Union({ value = types }) ->
        let comma = " | "

        let typeStrs =
          types
          |> OrderedMap.toList
          |> List.map (fun (name, typ) -> $"{name}: {typ}")

        $"({String.Join(comma, typeStrs)})"
      | Set t -> $"Set[{t}]"
      | Sum({ value = types }) ->
        let comma = " + "
        $"({String.Join(comma, types)})"
      | Schema s ->
        match schemaNameFromSource s.Source with
        | Some schemaName -> schemaName
        | None ->
          $"Schema[{s.Entities.Count} Entities, {s.Relations.Count} Relations]"
      | Entities s -> $"SchemaEntities[{s.Entities.Count}]"
      | Entity(s, e, e_with_props, id) ->
        $"SchemaEntity[Schema[{s.Entities.Count} Entities, {s.Relations.Count} Relations]][{e}][{e_with_props}][{id}]"
      | Relations s -> $"SchemaRelations[{s.Relations.Count}]"
      | RelationLookupOption(s, f', t_id, target_id) ->
        $"SchemaLookupOption[Schema[{s.Entities.Count} Entities, {s.Relations.Count} Relations]][{f'}][{t_id}][{target_id}]"
      | RelationLookupOne(s, f', t_id, target_id) ->
        $"SchemaLookupOne[Schema[{s.Entities.Count} Entities, {s.Relations.Count} Relations]][{f'}][{t_id}][{target_id}]"
      | RelationLookupMany(s, f', t_id, target_id) ->
        $"SchemaLookupMany[Schema[{s.Entities.Count} Entities, {s.Relations.Count} Relations]][{f'}][{t_id}][{target_id}]"
      | Relation(s, rn, _c, f, f', f_id, t, t', t_id) ->
        $"SchemaRelation[{rn.Name} Schema[{s.Entities.Count} Entities, {s.Relations.Count} Relations]][{f}][{f'}][{f_id}][{t}][{t'}][{t_id}]"
      | ForeignKeyRelation(s, rn, f, f', f_id, t, t', t_id) ->
        $"SchemaForeignKeyRelation[{rn.Name} Schema[{s.Entities.Count} Entities, {s.Relations.Count} Relations]][{f}][{f'}][{f_id}][{t}][{t'}][{t_id}]"
      | QueryTypeFunction -> $"Query"
      | QueryRow q -> $"QueryRow[{q}]"

    member self.ToInlayString() : string =
      match self with
      | QueryRow q -> q.ToInlayString()
      | Imported i ->
        let args =
          String.Join(
            " ",
            i.Arguments |> List.map (fun a -> $"[{a.ToInlayString()}]")
          )

        $"{i.Sym.Name}{args}"
      | Arrow({ value = (t1, t2) }) ->
        $"({t1.ToInlayString()} -> {t2.ToInlayString()})"
      | Tuple({ value = types }) ->
        let comma = " * "
        $"({String.Join(comma, types |> List.map (fun t -> t.ToInlayString()))})"
      | Record({ value = fields }) ->
        let comma = ", "

        let fieldStrs =
          fields
          |> OrderedMap.toList
          |> List.map (fun (name, (fieldType, _)) ->
            $"{name}: {fieldType.ToInlayString()}")

        $"{{{String.Join(comma, fieldStrs)}}}"
      | Union({ value = types }) ->
        let comma = " | "

        let typeStrs =
          types
          |> OrderedMap.toList
          |> List.map (fun (name, typ) -> $"{name}: {typ.ToInlayString()}")

        $"({String.Join(comma, typeStrs)})"
      | _ -> self.ToString()

    static member IsOptionalPrimitive(t: TypeValue<_>) =
      match t with
      | TypeValue.Sum({ value = [ TypeValue.Primitive { value = PrimitiveType.Unit }
                                  TypeValue.Primitive _ ] }) -> true
      | _ -> false

  and TypeQueryRow<'valueExt> =
    | PrimaryKey of TypeValue<'valueExt>
    | Json of TypeValue<'valueExt>
    | PrimitiveType of PrimitiveType * IsNullable: bool
    | Tuple of List<TypeQueryRow<'valueExt>>
    | Record of Map<LocalIdentifier, TypeQueryRow<'valueExt>>
    | Array of TypeQueryRow<'valueExt>

    override self.ToString() =
      match self with
      | PrimaryKey t -> $"PrimaryKey[{t}]"
      | Json t -> $"Json[{t}]"
      | PrimitiveType(p, isNullable) ->
        $"PrimitiveType[{p}, IsNullable={isNullable}]"
      | Tuple types ->
        let comma = " * "
        $"({String.Join(comma, types)})"
      | Record fields ->
        let comma = ", "

        let fieldStrs =
          fields |> Map.toList |> List.map (fun (name, typ) -> $"{name}: {typ}")

        $"{{{String.Join(comma, fieldStrs)}}}"
      | Array t -> $"Array[{t}]"

    member self.ToInlayString() : string =
      match self with
      | PrimaryKey t -> $"{t.ToInlayString()}*"
      | Json t -> t.ToInlayString()
      | PrimitiveType(p, _) -> p.ToString()
      | Tuple types ->
        let comma = " * "
        $"({String.Join(comma, types |> List.map (fun t -> t.ToInlayString()))})"
      | Record fields ->
        let comma = ", "

        let fieldStrs =
          fields
          |> Map.toList
          |> List.map (fun (name, typ) -> $"{name}: {typ.ToInlayString()}")

        $"{{{String.Join(comma, fieldStrs)}}}"
      | Array t -> $"{t.ToInlayString()}[]"

  and ExprTypeLetBindingName =
    | ExprTypeLetBindingName of string

    override self.ToString() =
      let (ExprTypeLetBindingName s) = self in s

  and TypeExprSourceMapping<'valueExt> =
    | OriginExprTypeLet of ExprTypeLetBindingName * TypeExpr<'valueExt>
    | OriginTypeExpr of TypeExpr<'valueExt>
    | NoSourceMapping of string

    override self.ToString() =
      match self with
      | OriginExprTypeLet(bindingName, _) -> $"origin:type-let:{bindingName}"
      | OriginTypeExpr t -> $"origin:type-expr:{t}"
      | NoSourceMapping name -> $"origin:none:{name}"

  and WithSourceMapping<'v, 'valueExt> =
    { value: 'v
      typeExprSource: TypeExprSourceMapping<'valueExt>
      typeCheckScopeSource: TypeCheckScope }

    override self.ToString() = self.value.ToString()

  and ImportedTypeValue<'valueExt> =
    { Id: ResolvedIdentifier
      Sym: TypeSymbol
      Parameters: List<TypeParameter>
      Arguments: List<TypeValue<'valueExt>> }

    override self.ToString() =
      // let pars = String.Join(" ", self.Parameters |> List.map (fun a -> $"{a} => "))

      // let appliedPars =
      //   String.Join("", self.Parameters |> List.map (fun a -> $"[{a.Name}]"))

      let args =
        String.Join(" ", self.Arguments |> List.map (fun a -> $"[{a}]"))
      // $"{pars}{self.Sym.Name}{appliedPars}{args}"
      $"{self.Sym.Name}{args}"

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
    | Vector

    override self.ToString() =
      match self with
      | Unit -> "()"
      | Guid -> "guid"
      | Int32 -> "int32"
      | Int64 -> "int64"
      | Float32 -> "float32"
      | Float64 -> "float64"
      | Decimal -> "decimal"
      | Bool -> "boolean"
      | String -> "string"
      | DateTime -> "dateTime"
      | DateOnly -> "dateOnly"
      | TimeSpan -> "timeSpan"
      | Vector -> "vector"

  and Var =
    { Name: string }

    static member Create name : Var = { Var.Name = name }
    override self.ToString() = self.Name

  and ExprLookup<'T, 'Id, 'valueExt when 'Id: comparison> =
    { Id: 'Id }

    override self.ToString() = self.Id.ToString()

  and ExprLambda<'T, 'Id, 'valueExt when 'Id: comparison> =
    { Param: Var
      ParamType: Option<'T>
      Body: Expr<'T, 'Id, 'valueExt>
      BodyType: Option<'T> }

    override self.ToString() =
      match self.ParamType with
      | Some t -> $"fun ({self.Param}: {t}) -> {self.Body}"
      | None -> $"fun {self.Param} -> {self.Body}"

  and ExprApply<'T, 'Id, 'valueExt when 'Id: comparison> =
    { F: Expr<'T, 'Id, 'valueExt>
      Arg: Expr<'T, 'Id, 'valueExt> }

    override self.ToString() = $"({self.F})({self.Arg})"

  and ExprLet<'T, 'Id, 'valueExt when 'Id: comparison> =
    { Var: Var
      Type: Option<'T>
      Val: Expr<'T, 'Id, 'valueExt>
      Rest: Expr<'T, 'Id, 'valueExt> }

    override self.ToString() =
      match self.Type with
      | Some t -> $"let {self.Var}: {t} = {self.Val} in {self.Rest}"
      | None -> $"let {self.Var} = {self.Val} in {self.Rest}"

  and ExprDo<'T, 'Id, 'valueExt when 'Id: comparison> =
    { Val: Expr<'T, 'Id, 'valueExt>
      Rest: Expr<'T, 'Id, 'valueExt> }

    override self.ToString() = $"do {self.Val}; {self.Rest}"

  and ExprIf<'T, 'Id, 'valueExt when 'Id: comparison> =
    { Cond: Expr<'T, 'Id, 'valueExt>
      Then: Expr<'T, 'Id, 'valueExt>
      Else: Expr<'T, 'Id, 'valueExt> }

    override self.ToString() =
      $"if {self.Cond} then {self.Then} else {self.Else}"

  and ExprTypeLambda<'T, 'Id, 'valueExt when 'Id: comparison> =
    { Param: TypeParameter
      Body: Expr<'T, 'Id, 'valueExt> }

    override self.ToString() = $"Λ{self.Param} => {self.Body}"

  and ExprTypeApply<'T, 'Id, 'valueExt when 'Id: comparison> =
    { Func: Expr<'T, 'Id, 'valueExt>
      TypeArg: 'T }

    override self.ToString() = $"{self.Func}[{self.TypeArg}]"

  and ExprTypeLet<'T, 'Id, 'valueExt when 'Id: comparison> =
    { Name: string
      TypeDef: 'T
      Body: Expr<'T, 'Id, 'valueExt> }

    override self.ToString() =
      $"type {self.Name} = {self.TypeDef}; {self.Body}"

  and ExprRecordCons<'T, 'Id, 'valueExt when 'Id: comparison> =
    { Fields: List<'Id * Expr<'T, 'Id, 'valueExt>> }

    override self.ToString() =
      let fields =
        self.Fields |> List.map (fun (k, v) -> $"{k}: {v}") |> String.join "; "

      $"{{ {fields} }}"

  and ExprRecordWith<'T, 'Id, 'valueExt when 'Id: comparison> =
    { Record: Expr<'T, 'Id, 'valueExt>
      Fields: List<'Id * Expr<'T, 'Id, 'valueExt>> }

    override self.ToString() =
      let fields =
        self.Fields |> List.map (fun (k, v) -> $"{k}: {v}") |> String.join "; "

      $"{{ {self.Record} with {fields} }}"

  and ExprTupleCons<'T, 'Id, 'valueExt when 'Id: comparison> =
    { Items: List<Expr<'T, 'Id, 'valueExt>> }

    override self.ToString() =
      let items = self.Items |> List.map string |> String.join ", "
      $"({items})"

  and ExprTupleDes<'T, 'Id, 'valueExt when 'Id: comparison> =
    { Tuple: Expr<'T, 'Id, 'valueExt>
      Item: TupleDesSelector }

    override self.ToString() = $"{self.Tuple}.{self.Item.Index}"

  and ExprSumCons<'T, 'Id, 'valueExt when 'Id: comparison> =
    { Selector: SumConsSelector }

    override self.ToString() = self.Selector.ToString()

  and ExprRecordDes<'T, 'Id, 'valueExt when 'Id: comparison> =
    { Expr: Expr<'T, 'Id, 'valueExt>
      Field: 'Id }

    override self.ToString() = $"{self.Expr}.{self.Field}"

  and ExprEntitiesDes<'T, 'Id, 'valueExt when 'Id: comparison> =
    { Expr: Expr<'T, 'Id, 'valueExt> }

    override self.ToString() = $"{self.Expr}.Entities"

  and ExprRelationsDes<'T, 'Id, 'valueExt when 'Id: comparison> =
    { Expr: Expr<'T, 'Id, 'valueExt> }

    override self.ToString() = $"{self.Expr}.Relations"

  and ExprLookupsDes<'T, 'Id, 'valueExt when 'Id: comparison> =
    { Expr: Expr<'T, 'Id, 'valueExt> }

    override self.ToString() = $"{self.Expr}.Lookups"

  and ExprEntityDes<'T, 'Id, 'valueExt when 'Id: comparison> =
    { Expr: Expr<'T, 'Id, 'valueExt>
      EntityName: SchemaEntityName }

    override self.ToString() = $"{self.Expr}.{self.EntityName}"

  and ExprRelationDes<'T, 'Id, 'valueExt when 'Id: comparison> =
    { Expr: Expr<'T, 'Id, 'valueExt>
      RelationName: SchemaRelationName }

    override self.ToString() = $"{self.Expr}.{self.RelationName}"

  and RelationLookupDirection =
    | FromTo
    | ToFrom

    override self.ToString() =
      match self with
      | FromTo -> "from-to"
      | ToFrom -> "to-from"

  and ExprRelationLookupDes<'T, 'Id, 'valueExt when 'Id: comparison> =
    { Expr: Expr<'T, 'Id, 'valueExt>
      RelationName: SchemaRelationName
      Direction: RelationLookupDirection }

    override self.ToString() =
      $"{self.Expr}.{self.RelationName}.{self.Direction}"

  and ExprUnionDes<'T, 'Id, 'valueExt when 'Id: comparison> =
    { Handlers: Map<'Id, CaseHandler<'T, 'Id, 'valueExt>>
      Fallback: Option<Expr<'T, 'Id, 'valueExt>> }

    override self.ToString() =
      let handlers =
        self.Handlers
        |> Map.toList
        |> List.map (fun (k, (v, body)) ->
          let p = v |> Option.map string |> Option.defaultValue "_"
          $"{k}({p}) => {body}")
        |> String.join " | "

      match self.Fallback with
      | Some fallback -> $"match {handlers} | _ => {fallback}"
      | None -> $"match {handlers}"

  and ExprSumDes<'T, 'Id, 'valueExt when 'Id: comparison> =
    { Handlers: Map<SumConsSelector, CaseHandler<'T, 'Id, 'valueExt>> }

    override self.ToString() =
      self.Handlers
      |> Map.toList
      |> List.map (fun (k, (v, body)) ->
        let p = v |> Option.map string |> Option.defaultValue "_"
        $"{k}({p}) => {body}")
      |> String.join " | "
      |> sprintf "match %s"

  and ExprFromValue<'T, 'Id, 'valueExt when 'Id: comparison> =
    { Value: Value<TypeValue<'valueExt>, 'valueExt>
      ValueType: TypeValue<'valueExt>
      ValueKind: Kind }

    override self.ToString() =
      $"{self.Value} : {self.ValueType} :: {self.ValueKind}"

  and SimpleQuery<'T, 'Id, 'valueExt when 'Id: comparison> =
    { Iterators: ExprQueryIterators<'T, 'Id, 'valueExt>
      Joins: Option<NonEmptyList<ExprQueryJoin<'T, 'Id, 'valueExt>>>
      Where: Option<ExprQueryExpr<'T, 'Id, 'valueExt>>
      Select: ExprQueryExpr<'T, 'Id, 'valueExt>
      OrderBy: Option<ExprQueryExpr<'T, 'Id, 'valueExt> * OrderByDirection>
      Closure: Map<ResolvedIdentifier, TypeQueryRow<'valueExt>>
      DeserializeFrom: TypeQueryRow<'valueExt>
      Distinct: Option<ExprQueryExpr<'T, 'Id, 'valueExt>> }

    override q.ToString() =
      let joins =
        match q.Joins with
        | Some joins ->
          let join_to_str j = $"({j.Left}) = ({j.Right})"
          let _and = " and "
          $"\njoins {String.Join(_and, joins |> NonEmptyList.toList |> List.map join_to_str)}"
        | None -> ""

      let where_to_str =
        match q.Where with
        | Some w -> $"\nwhere {w}"
        | None -> ""

      let select_to_str = $"\nselect {q.Select}"

      let order_by_to_str =
        match q.OrderBy with
        | Some(ob, dir) -> $"\norderby {ob} {dir}"
        | None -> ""

      let iterator_vars q =
        q.Iterators
        |> NonEmptyList.map (fun i -> i.Var.Name)
        |> NonEmptyList.toSeq
        |> String.join ", "

      let iterator_sources q =
        q.Iterators
        |> NonEmptyList.map (fun i -> $"{i.Source}")
        |> NonEmptyList.toSeq
        |> String.join ", "

      let distinctStr =
        match q.Distinct with
        | Some d -> $"\ndistinct {d}"
        | None -> ""

      $"""(query {{ 
  from {iterator_vars q} in {iterator_sources q}{joins}{where_to_str}{select_to_str}{order_by_to_str}}}){distinctStr}"""


  and ExprQuery<'T, 'Id, 'valueExt when 'Id: comparison> =
    | SimpleQuery of SimpleQuery<'T, 'Id, 'valueExt>
    | UnionQueries of
      ExprQuery<'T, 'Id, 'valueExt> *
      ExprQuery<'T, 'Id, 'valueExt>

    member self.Location =
      match self with
      | SimpleQuery q -> q.Iterators.Head |> fun i -> i.Location
      | UnionQueries(q1, _) -> q1.Location

    member self.Closure =
      match self with
      | SimpleQuery q -> q.Closure
      | UnionQueries(q1, q2) -> Map.merge (fun _ -> id) q1.Closure q2.Closure

    override self.ToString() =
      match self with
      | SimpleQuery q -> q.ToString()
      | UnionQueries(q1, q2) ->
        let unionStrs =
          [ q1; q2 ]
          |> List.map (fun q -> q.ToString())
          |> String.join "\nunion\n"

        $"(\n{unionStrs}\n)"

  and OrderByDirection =
    | Asc
    | Desc

    override self.ToString() =
      match self with
      | Asc -> "asc"
      | Desc -> "desc"

  and ExprQueryJoin<'T, 'Id, 'valueExt when 'Id: comparison> =
    { Location: Location
      Left: ExprQueryExpr<'T, 'Id, 'valueExt>
      Right: ExprQueryExpr<'T, 'Id, 'valueExt> }

    override self.ToString() = $"({self.Left}) = ({self.Right})"

  and ExprQueryExpr<'T, 'Id, 'valueExt when 'Id: comparison> =
    { Location: Location
      Expr: ExprQueryExprRec<'T, 'Id, 'valueExt> }

    override self.ToString() = self.Expr.ToString()

  and ExprQueryExprRec<'T, 'Id, 'valueExt when 'Id: comparison> =
    | QueryTupleCons of List<ExprQueryExpr<'T, 'Id, 'valueExt>>
    | QueryRecordDes of
      Expr: ExprQueryExpr<'T, 'Id, 'valueExt> *
      Field: 'Id *
      IsJsonField: bool
    | QueryTupleDes of
      Expr: ExprQueryExpr<'T, 'Id, 'valueExt> *
      Item: TupleDesSelector *
      IsJsonItem: bool
    | QueryConditional of
      Cond: ExprQueryExpr<'T, 'Id, 'valueExt> *
      Then: ExprQueryExpr<'T, 'Id, 'valueExt> *
      Else: ExprQueryExpr<'T, 'Id, 'valueExt>
    | QueryUnionDes of
      Expr: ExprQueryExpr<'T, 'Id, 'valueExt> *
      Handlers: Map<'Id, QueryCaseHandler<'T, 'Id, 'valueExt>>
    | QuerySumDes of
      Expr: ExprQueryExpr<'T, 'Id, 'valueExt> *
      Handlers: Map<SumConsSelector, QueryCaseHandler<'T, 'Id, 'valueExt>>
    | QueryApply of
      Func: ExprQueryExpr<'T, 'Id, 'valueExt> *
      Arg: ExprQueryExpr<'T, 'Id, 'valueExt>
    | QueryLookup of 'Id
    | QueryIntrinsic of QueryIntrinsic * ExpectedType: TypeQueryRow<'valueExt>
    | QueryConstant of PrimitiveValue
    | QueryClosureValue of
      Value<TypeValue<'valueExt>, 'valueExt> *
      TypeQueryRow<'valueExt>
    | QueryCastTo of ExprQueryExpr<'T, 'Id, 'valueExt> * TypeQueryRow<'valueExt>
    | QueryCount of ExprQuery<'T, 'Id, 'valueExt>
    | QueryExists of ExprQuery<'T, 'Id, 'valueExt>
    | QueryArray of ExprQuery<'T, 'Id, 'valueExt>
    | QueryCountEvaluated of ValueQuery<'T, 'valueExt>
    | QueryExistsEvaluated of ValueQuery<'T, 'valueExt>
    | QueryArrayEvaluated of ValueQuery<'T, 'valueExt>

    override self.ToString() =
      match self with
      | QueryTupleCons items ->
        let itemStrs =
          items |> List.map (fun i -> i.ToString()) |> String.join ", "

        $"({itemStrs})"
      | QueryRecordDes(e, field, isJsonField) ->
        match isJsonField with
        | false -> $"{e}.{field}"
        | true -> $"{e} -> \"{field}\""
      | QueryTupleDes(e, item, isJsonItem) ->
        match isJsonItem with
        | false -> $"{e}.{item.Index}"
        | true -> $"{e} -> {item.Index - 1}"
      | QueryConditional(cond, thenExpr, elseExpr) ->
        $"if {cond} then {thenExpr} else {elseExpr}"
      | QueryUnionDes(e, handlers) ->
        let handlerStrs =
          handlers
          |> Map.toList
          |> List.map (fun (case, handler) ->
            $"| {case} -> ({handler.Param} -> {handler.Body})")

        let space = " "

        $"match {e} with {{ {String.Join(space, handlerStrs)} }}"
      | QuerySumDes(e, handlers) ->
        let handlerStrs =
          handlers
          |> Map.toList
          |> List.map (fun (case, handler) ->
            $"| {case} ({handler.Param} -> {handler.Body})")

        let space = " "

        $"match {e} with {{ {String.Join(space, handlerStrs)} }}"
      | QueryApply(func, args) -> $"{func}({args})"
      | QueryLookup id -> id.ToString()
      | QueryIntrinsic(intrinsic, _) -> intrinsic.ToString()
      | QueryConstant c -> c.ToString()
      | QueryClosureValue(v, _) -> v.ToString()
      | QueryCastTo(v, t) -> $"{v} :: {t}"
      | QueryCount q -> $"count({q})"
      | QueryExists q -> $"any({q})"
      | QueryArray q -> $"array({q})"
      | QueryCountEvaluated v -> $"count({v})"
      | QueryExistsEvaluated v -> $"any({v})"
      | QueryArrayEvaluated v -> $"array({v})"

  and QueryCaseHandler<'T, 'Id, 'valueExt when 'Id: comparison> =
    { Param: Var
      Body: ExprQueryExpr<'T, 'Id, 'valueExt> }

    override self.ToString() = $"{self.Param} -> {self.Body}"

  and QueryIntrinsic =
    | VectorDistance
    | VectorEmbed
    | Plus
    | Minus
    | Multiply
    | Divide
    | Modulo
    | Equals
    | NotEquals
    | GreaterThan
    | LessThan
    | GreaterThanOrEqual
    | LessThanOrEqual
    | And
    | Or
    | Not

    override self.ToString() =
      match self with
      | VectorDistance -> "DB::vector_distance"
      | VectorEmbed -> "DB::vector_embed"
      | Plus -> "+"
      | Minus -> "-"
      | Multiply -> "*"
      | Divide -> "/"
      | Modulo -> "%"
      | Equals -> "="
      | NotEquals -> "<>"
      | GreaterThan -> ">"
      | LessThan -> "<"
      | GreaterThanOrEqual -> ">="
      | LessThanOrEqual -> "<="
      | And -> "&&"
      | Or -> "||"
      | Not -> "!"

  and ExprQueryIterators<'T, 'Id, 'valueExt when 'Id: comparison> =
    NonEmptyList<ExprQueryIterator<'T, 'Id, 'valueExt>>

  and ExprQueryIterator<'T, 'Id, 'valueExt when 'Id: comparison> =
    { Location: Location
      Var: Var
      VarType: Option<TypeQueryRow<'valueExt>>
      Source: Expr<'T, 'Id, 'valueExt> }

    override self.ToString() =
      match self.VarType with
      | Some t -> $"{self.Var}: {t} in {self.Source}"
      | None -> $"{self.Var} in {self.Source}"

  // ═══════════════════════════════════════════════════════════════════════════
  // View AST types (JSX-like syntax)
  // ═══════════════════════════════════════════════════════════════════════════

  and ExprView<'T, 'Id, 'valueExt when 'Id: comparison> =
    { Param: Var
      ParamType: Option<'T>
      Body: ExprViewNode<'T, 'Id, 'valueExt>
      Location: Location }

    override self.ToString() =
      match self.ParamType with
      | Some t -> $"view ({self.Param}: {t}) -> {self.Body}"
      | None -> $"view {self.Param} -> {self.Body}"

  and ExprViewNode<'T, 'Id, 'valueExt when 'Id: comparison> =
    { Location: Location
      Node: ExprViewNodeRec<'T, 'Id, 'valueExt> }

    override self.ToString() = self.Node.ToString()

  and ExprViewNodeRec<'T, 'Id, 'valueExt when 'Id: comparison> =
    | ViewElement of ExprViewElement<'T, 'Id, 'valueExt>
    | ViewFragment of List<ExprViewNode<'T, 'Id, 'valueExt>>
    | ViewExprContainer of Expr<'T, 'Id, 'valueExt>
    | ViewText of string
    // View operation nodes (structurally promoted from extensions)
    | ViewMapContext of Mapper: Expr<'T, 'Id, 'valueExt> * Inner: Expr<'T, 'Id, 'valueExt>
    | ViewMapState of MapDown: Expr<'T, 'Id, 'valueExt> * MapUp: Expr<'T, 'Id, 'valueExt> * Inner: Expr<'T, 'Id, 'valueExt>

    override self.ToString() =
      match self with
      | ViewElement el -> el.ToString()
      | ViewFragment children ->
        let childStrs = children |> List.map (fun c -> c.ToString()) |> String.concat " "
        $"<>{childStrs}</>"
      | ViewExprContainer e -> $"{{{e}}}"
      | ViewText t -> t
      | ViewMapContext(mapper, inner) -> $"View::mapContext ({mapper}) ({inner})"
      | ViewMapState(mapDown, mapUp, inner) -> $"View::mapState ({mapDown}) ({mapUp}) ({inner})"

  and ExprViewElement<'T, 'Id, 'valueExt when 'Id: comparison> =
    { Tag: string
      Attributes: List<ExprViewAttribute<'T, 'Id, 'valueExt>>
      Children: List<ExprViewNode<'T, 'Id, 'valueExt>>
      SelfClosing: bool }

    override self.ToString() =
      let attrStr =
        self.Attributes
        |> List.map (fun a -> a.ToString())
        |> String.concat " "
      if self.SelfClosing then
        $"<{self.Tag} {attrStr} />"
      else
        let childStr = self.Children |> List.map (fun c -> c.ToString()) |> String.concat ""
        $"<{self.Tag} {attrStr}>{childStr}</{self.Tag}>"

  and ExprViewAttribute<'T, 'Id, 'valueExt when 'Id: comparison> =
    | ViewAttrStringValue of Name: string * Value: string
    | ViewAttrExprValue of Name: string * Value: Expr<'T, 'Id, 'valueExt>

    override self.ToString() =
      match self with
      | ViewAttrStringValue(name, value) -> $"{name}=\"{value}\""
      | ViewAttrExprValue(name, _) -> $"{name}={{...}}"

  // ═══════════════════════════════════════════════════════════════════════════
  // Coroutine AST types (co { } computation expressions)
  // ═══════════════════════════════════════════════════════════════════════════

  and ExprCo<'T, 'Id, 'valueExt when 'Id: comparison> =
    { Body: ExprCoStep<'T, 'Id, 'valueExt>
      Location: Location }

    override self.ToString() = $"co {{ {self.Body} }}"

  and ExprCoStep<'T, 'Id, 'valueExt when 'Id: comparison> =
    { Location: Location
      Step: ExprCoStepRec<'T, 'Id, 'valueExt> }

    override self.ToString() = self.Step.ToString()

  and ExprCoStepRec<'T, 'Id, 'valueExt when 'Id: comparison> =
    | CoLetBang of Var: Var * Value: Expr<'T, 'Id, 'valueExt> * Rest: ExprCoStep<'T, 'Id, 'valueExt>
    | CoDoBang of Value: Expr<'T, 'Id, 'valueExt> * Rest: ExprCoStep<'T, 'Id, 'valueExt>
    | CoReturn of Expr<'T, 'Id, 'valueExt>
    | CoReturnBang of Expr<'T, 'Id, 'valueExt>
    // Co operation nodes (structurally promoted from extensions)
    | CoShow of Predicate: Expr<'T, 'Id, 'valueExt> * View: Expr<'T, 'Id, 'valueExt>
    | CoUntil of Predicate: Expr<'T, 'Id, 'valueExt> * Inner: Expr<'T, 'Id, 'valueExt>
    | CoIgnore of Inner: Expr<'T, 'Id, 'valueExt>
    | CoMapContext of Mapper: Expr<'T, 'Id, 'valueExt> * Inner: Expr<'T, 'Id, 'valueExt>
    | CoMapState of MapDown: Expr<'T, 'Id, 'valueExt> * MapUp: Expr<'T, 'Id, 'valueExt> * Inner: Expr<'T, 'Id, 'valueExt>
    | CoGetContext
    | CoGetState
    | CoSetState of Updater: Expr<'T, 'Id, 'valueExt>

    override self.ToString() =
      match self with
      | CoLetBang(var, value, rest) -> $"let! {var} = {value}; {rest}"
      | CoDoBang(value, rest) -> $"do! {value}; {rest}"
      | CoReturn e -> $"return {e}"
      | CoReturnBang e -> $"return! {e}"
      | CoShow(pred, view) -> $"Co::show ({pred}) ({view})"
      | CoUntil(pred, inner) -> $"Co::until ({pred}) ({inner})"
      | CoIgnore inner -> $"Co::ignore ({inner})"
      | CoMapContext(mapper, inner) -> $"Co::mapContext ({mapper}) ({inner})"
      | CoMapState(mapDown, mapUp, inner) -> $"Co::mapState ({mapDown}) ({mapUp}) ({inner})"
      | CoGetContext -> "Co::getContext"
      | CoGetState -> "Co::getState"
      | CoSetState updater -> $"Co::setState ({updater})"

  // ═══════════════════════════════════════════════════════════════════════════

  and ExprRecoveredSyntaxError<'T, 'Id, 'valueExt when 'Id: comparison> =
    { ErrorMessage: string
      ErrorLocation: Location
      RecoveryContext: string }

    override self.ToString() =
      $"<SyntaxError: {self.ErrorMessage}>"

  and ExprErrorDanglingRecordDes<'T, 'Id, 'valueExt when 'Id: comparison> =
    { Expr: Expr<'T, 'Id, 'valueExt>
      Field: Option<'Id> }

    override self.ToString() =
      match self.Field with
      | Some f -> $"{self.Expr}.{f}"
      | None -> $"{self.Expr}.<incomplete>"

  and ExprErrorDanglingScopedIdentifier<'T, 'Id, 'valueExt when 'Id: comparison> =
    { PrefixParts: List<string> }

    override self.ToString() =
      let joined = System.String.Join("::", self.PrefixParts)
      $"{joined}::<incomplete>"

  and [<RequireQualifiedAccess>] CoOperationKind =
    | Show
    | Until
    | Ignore
    | MapContext
    | MapState
    | GetContext
    | GetState
    | SetState

    member self.Name =
      match self with
      | Show -> "show" | Until -> "until" | Ignore -> "ignore"
      | MapContext -> "mapContext" | MapState -> "mapState"
      | GetContext -> "getContext" | GetState -> "getState"
      | SetState -> "setState"

    member self.Arity =
      match self with
      | Show -> 2 | Until -> 2 | Ignore -> 1
      | MapContext -> 2 | MapState -> 3
      | GetContext -> 0 | GetState -> 0 | SetState -> 1

    override self.ToString() = $"Co::{self.Name}"

    static member TryParse(name: string) =
      match name with
      | "show" -> Some Show | "until" -> Some Until | "ignore" -> Some Ignore
      | "mapContext" -> Some MapContext | "mapState" -> Some MapState
      | "getContext" -> Some GetContext | "getState" -> Some GetState
      | "setState" -> Some SetState
      | _ -> None

  and [<RequireQualifiedAccess>] ViewOperationKind =
    | MapContext
    | MapState

    member self.Name =
      match self with
      | MapContext -> "mapContext" | MapState -> "mapState"

    member self.Arity =
      match self with
      | MapContext -> 2 | MapState -> 3

    override self.ToString() = $"View::{self.Name}"

    static member TryParse(name: string) =
      match name with
      | "mapContext" -> Some MapContext | "mapState" -> Some MapState
      | _ -> None

  and ExprRec<'T, 'Id, 'valueExt when 'Id: comparison> =
    | Primitive of PrimitiveValue
    | Lookup of ExprLookup<'T, 'Id, 'valueExt>
    | TypeLambda of ExprTypeLambda<'T, 'Id, 'valueExt>
    | TypeApply of ExprTypeApply<'T, 'Id, 'valueExt>
    | TypeLet of ExprTypeLet<'T, 'Id, 'valueExt>
    | Lambda of ExprLambda<'T, 'Id, 'valueExt>
    | FromValue of ExprFromValue<'T, 'Id, 'valueExt>
    | Apply of ExprApply<'T, 'Id, 'valueExt>
    | Let of ExprLet<'T, 'Id, 'valueExt>
    | Do of ExprDo<'T, 'Id, 'valueExt>
    | If of ExprIf<'T, 'Id, 'valueExt>
    | RecordCons of ExprRecordCons<'T, 'Id, 'valueExt>
    | RecordWith of ExprRecordWith<'T, 'Id, 'valueExt>
    | TupleCons of ExprTupleCons<'T, 'Id, 'valueExt>
    | SumCons of ExprSumCons<'T, 'Id, 'valueExt>
    | RecordDes of ExprRecordDes<'T, 'Id, 'valueExt>
    | EntitiesDes of ExprEntitiesDes<'T, 'Id, 'valueExt>
    | RelationsDes of ExprRelationsDes<'T, 'Id, 'valueExt>
    | EntityDes of ExprEntityDes<'T, 'Id, 'valueExt>
    | RelationDes of ExprRelationDes<'T, 'Id, 'valueExt>
    | RelationLookupDes of ExprRelationLookupDes<'T, 'Id, 'valueExt>
    | UnionDes of ExprUnionDes<'T, 'Id, 'valueExt>
    | TupleDes of ExprTupleDes<'T, 'Id, 'valueExt>
    | SumDes of ExprSumDes<'T, 'Id, 'valueExt>
    | Query of ExprQuery<'T, 'Id, 'valueExt>
    | View of ExprView<'T, 'Id, 'valueExt>
    | Co of ExprCo<'T, 'Id, 'valueExt>
    | CoOp of CoOperationKind
    | ViewOp of ViewOperationKind
    | RecoveredSyntaxError of ExprRecoveredSyntaxError<'T, 'Id, 'valueExt>
    | ErrorDanglingRecordDes of ExprErrorDanglingRecordDes<'T, 'Id, 'valueExt>
    | ErrorDanglingScopedIdentifier of ExprErrorDanglingScopedIdentifier<'T, 'Id, 'valueExt>

    override self.ToString() : string =
      match self with
      | ExprRec.TypeLambda({ ExprTypeLambda.Param = tp
                             Body = body }) ->
        $"(Λ{tp.ToString()} => {body.ToString()})"
      | TypeApply({ Func = e; TypeArg = t }) ->
        $"{e.ToString()} [{t.ToString()}]"
      | Lambda({ Param = v
                 ParamType = topt
                 Body = body }) ->
        match topt with
        | Some t -> $"(fun ({v.Name}: {t.ToString()}) -> {body.ToString()})"
        | None -> $"(fun {v.Name} -> {body.ToString()})"
      | Apply({ F = e1; Arg = e2 }) -> $"({e1.ToString()} {e2.ToString()})"
      | FromValue({ Value = v
                    ValueType = t
                    ValueKind = k }) ->
        $"({v.ToString()} : {t.ToString()} :: {k.ToString()}])"
      | Let({ Var = v
              Type = topt
              Val = e1
              Rest = e2 }) ->
        match topt with
        | Some t ->
          $"(let {v.Name}: {t.ToString()} = {e1.ToString()} in {e2.ToString()})"
        | None -> $"(let {v.Name} = {e1.ToString()} in {e2.ToString()})"
      | Do({ Val = e1; Rest = e2 }) -> $"(do {e1.ToString()}; {e2.ToString()})"
      | TypeLet({ Name = name
                  TypeDef = t
                  Body = body }) ->
        $"(type {name} = {t.ToString()}; {body.ToString()})"
      | RecordCons { Fields = fields } ->
        let fieldStr =
          fields
          |> List.map (fun (k, v) -> $"{k.ToString()} = {v.ToString()}")
          |> String.concat "; "

        $"{{ {fieldStr} }}"
      | RecordWith({ Record = record; Fields = fields }) ->
        let fieldStr =
          fields
          |> List.map (fun (k, v) -> $"{k.ToString()} = {v.ToString()}")
          |> String.concat "; "

        $"{{ {record.ToString()} with {fieldStr} }}"
      | TupleCons { Items = items } ->
        let itemStr =
          items |> List.map (fun v -> v.ToString()) |> String.concat ", "

        $"({itemStr})"
      | SumCons({ Selector = selector }) -> $"{selector.Case}Of{selector.Count}"
      | RecordDes({ Expr = record; Field = field }) ->
        $"{record.ToString()}.{field.ToString()}"
      | EntitiesDes({ Expr = entities }) -> $"{entities.ToString()}.Entities"
      | RelationsDes({ Expr = relations }) ->
        $"{relations.ToString()}.Relations"
      | EntityDes({ Expr = entity
                    EntityName = entityName }) ->
        $"{entity.ToString()}.{entityName.ToString()}"
      | RelationDes({ Expr = relation
                      RelationName = relationName }) ->
        $"{relation.ToString()}.{relationName.ToString()}"
      | RelationLookupDes({ Expr = relation
                            RelationName = _relationName
                            Direction = direction }) ->
        let dirStr =
          match direction with
          | FromTo -> "From"
          | ToFrom -> "To"

        $"{relation.ToString()}.{dirStr}"
      | UnionDes({ Handlers = handlers
                   Fallback = defaultOpt }) ->
        let unit = "()"

        let handlerStr =
          handlers
          |> Map.toList
          |> List.map (fun (k, (v, body)) ->
            $"{k.ToString()}({v |> Option.map (fun var -> var.Name) |> Option.defaultValue unit}) => {body.ToString()}")
          |> String.concat " | "

        match defaultOpt with
        | Some defaultExpr ->
          $"(match {handlerStr} | _ => {defaultExpr.ToString()})"
        | None -> $"(match {handlerStr})"
      | TupleDes({ ExprTupleDes.Tuple = tuple
                   Item = selector }) -> $"{tuple.ToString()}.{selector.Index}"
      | SumDes { Handlers = handlers } ->
        let unit = "()"

        let handlerStr =
          handlers
          |> Map.toList
          |> List.map (fun (k, (v, body)) ->
            $"{k.Case}Of{k.Count} ({v |> Option.map (fun var -> var.Name) |> Option.defaultValue unit} => {body.ToString()})")
          |> String.concat " | "

        $"(match {handlerStr})"
      | Primitive p -> p.ToString()
      | Lookup id -> id.ToString()
      | If({ Cond = cond
             Then = thenExpr
             Else = elseExpr }) ->
        $"(if {cond.ToString()} then {thenExpr.ToString()} else {elseExpr.ToString()})"

      | Query q -> q.ToString()
      | View v -> v.ToString()
      | Co c -> c.ToString()
      | CoOp op -> op.ToString()
      | ViewOp op -> op.ToString()
      | RecoveredSyntaxError err -> err.ToString()
      | ErrorDanglingRecordDes err -> err.ToString()
      | ErrorDanglingScopedIdentifier err -> err.ToString()


  and Expr<'T, 'Id, 'valueExt when 'Id: comparison> =
    { Expr: ExprRec<'T, 'Id, 'valueExt>
      Location: Location
      Scope: TypeCheckScope }

    override self.ToString() : string = self.Expr.ToString()

  and [<RequireQualifiedAccess>] TypeCheckedExprLookup<'valueExt> =
    { Id: ResolvedIdentifier }

    override self.ToString() = self.Id.ToString()

  and [<RequireQualifiedAccess>] TypeCheckedExprLambda<'valueExt> =
    { Param: Var
      ParamType: TypeValue<'valueExt>
      Body: TypeCheckedExpr<'valueExt>
      BodyType: TypeValue<'valueExt> }

    override self.ToString() =
      $"fun ({self.Param}: {self.ParamType}) -> {self.Body}"

  and [<RequireQualifiedAccess>] TypeCheckedExprApply<'valueExt> =
    { F: TypeCheckedExpr<'valueExt>
      Arg: TypeCheckedExpr<'valueExt> }

    override self.ToString() = $"({self.F})({self.Arg})"

  and [<RequireQualifiedAccess>] TypeCheckedExprLet<'valueExt> =
    { Var: Var
      Type: TypeValue<'valueExt>
      Val: TypeCheckedExpr<'valueExt>
      Rest: TypeCheckedExpr<'valueExt> }

    override self.ToString() =
      $"let {self.Var}: {self.Type} = {self.Val} in {self.Rest}"

  and [<RequireQualifiedAccess>] TypeCheckedExprDo<'valueExt> =
    { Val: TypeCheckedExpr<'valueExt>
      Rest: TypeCheckedExpr<'valueExt> }

    override self.ToString() = $"do {self.Val}; {self.Rest}"

  and [<RequireQualifiedAccess>] TypeCheckedExprIf<'valueExt> =
    { Cond: TypeCheckedExpr<'valueExt>
      Then: TypeCheckedExpr<'valueExt>
      Else: TypeCheckedExpr<'valueExt> }

    override self.ToString() =
      $"if {self.Cond} then {self.Then} else {self.Else}"

  and [<RequireQualifiedAccess>] TypeCheckedExprTypeLambda<'valueExt> =
    { Param: TypeParameter
      Body: TypeCheckedExpr<'valueExt> }

    override self.ToString() = $"Λ{self.Param} => {self.Body}"

  and [<RequireQualifiedAccess>] TypeCheckedExprTypeApply<'valueExt> =
    { Func: TypeCheckedExpr<'valueExt>
      TypeArg: TypeValue<'valueExt> }

    override self.ToString() = $"{self.Func}[{self.TypeArg}]"

  and [<RequireQualifiedAccess>] TypeCheckedExprTypeLet<'valueExt> =
    { Name: string
      TypeDef: TypeValue<'valueExt>
      Body: TypeCheckedExpr<'valueExt> }

    override self.ToString() =
      $"type {self.Name} = {self.TypeDef}; {self.Body}"

  and [<RequireQualifiedAccess>] TypeCheckedExprRecordCons<'valueExt> =
    { Fields: List<ResolvedIdentifier * TypeCheckedExpr<'valueExt>> }

    override self.ToString() =
      let fields =
        self.Fields |> List.map (fun (k, v) -> $"{k}: {v}") |> String.join "; "

      $"{{ {fields} }}"

  and [<RequireQualifiedAccess>] TypeCheckedExprRecordWith<'valueExt> =
    { Record: TypeCheckedExpr<'valueExt>
      Fields: List<ResolvedIdentifier * TypeCheckedExpr<'valueExt>> }

    override self.ToString() =
      let fields =
        self.Fields |> List.map (fun (k, v) -> $"{k}: {v}") |> String.join "; "

      $"{{ {self.Record} with {fields} }}"

  and [<RequireQualifiedAccess>] TypeCheckedExprTupleCons<'valueExt> =
    { Items: List<TypeCheckedExpr<'valueExt>> }

    override self.ToString() =
      let items = self.Items |> List.map string |> String.join ", "
      $"({items})"

  and [<RequireQualifiedAccess>] TypeCheckedExprTupleDes<'valueExt> =
    { Tuple: TypeCheckedExpr<'valueExt>
      Item: TupleDesSelector }

    override self.ToString() = $"{self.Tuple}.{self.Item.Index}"

  and [<RequireQualifiedAccess>] TypeCheckedExprSumCons<'valueExt> =
    { Selector: SumConsSelector }

    override self.ToString() = self.Selector.ToString()

  and [<RequireQualifiedAccess>] TypeCheckedExprRecordDes<'valueExt> =
    { Expr: TypeCheckedExpr<'valueExt>
      Field: ResolvedIdentifier }

    override self.ToString() = $"{self.Expr}.{self.Field}"

  and [<RequireQualifiedAccess>] TypeCheckedExprEntitiesDes<'valueExt> =
    { Expr: TypeCheckedExpr<'valueExt> }

    override self.ToString() = $"{self.Expr}.Entities"

  and [<RequireQualifiedAccess>] TypeCheckedExprRelationsDes<'valueExt> =
    { Expr: TypeCheckedExpr<'valueExt> }

    override self.ToString() = $"{self.Expr}.Relations"

  and [<RequireQualifiedAccess>] TypeCheckedExprLookupsDes<'valueExt> =
    { Expr: TypeCheckedExpr<'valueExt> }

    override self.ToString() = $"{self.Expr}.Lookups"

  and [<RequireQualifiedAccess>] TypeCheckedExprEntityDes<'valueExt> =
    { Expr: TypeCheckedExpr<'valueExt>
      EntityName: SchemaEntityName }

    override self.ToString() = $"{self.Expr}.{self.EntityName}"

  and [<RequireQualifiedAccess>] TypeCheckedExprRelationDes<'valueExt> =
    { Expr: TypeCheckedExpr<'valueExt>
      RelationName: SchemaRelationName }

    override self.ToString() = $"{self.Expr}.{self.RelationName}"

  and [<RequireQualifiedAccess>] TypeCheckedExprRelationLookupDes<'valueExt> =
    { Expr: TypeCheckedExpr<'valueExt>
      RelationName: SchemaRelationName
      Direction: RelationLookupDirection }

    override self.ToString() =
      $"{self.Expr}.{self.RelationName}.{self.Direction}"

  and [<RequireQualifiedAccess>] TypeCheckedExprUnionDes<'valueExt> =
    { Handlers: Map<ResolvedIdentifier, TypeCheckedCaseHandler<'valueExt>>
      Fallback: Option<TypeCheckedExpr<'valueExt>> }

    override self.ToString() =
      let handlers =
        self.Handlers
        |> Map.toList
        |> List.map (fun (k, (v, body)) ->
          let p = v |> Option.map string |> Option.defaultValue "_"
          $"{k}({p}) => {body}")
        |> String.join " | "

      match self.Fallback with
      | Some fallback -> $"match {handlers} | _ => {fallback}"
      | None -> $"match {handlers}"

  and [<RequireQualifiedAccess>] TypeCheckedExprSumDes<'valueExt> =
    { Handlers: Map<SumConsSelector, TypeCheckedCaseHandler<'valueExt>> }

    override self.ToString() =
      self.Handlers
      |> Map.toList
      |> List.map (fun (k, (v, body)) ->
        let p = v |> Option.map string |> Option.defaultValue "_"
        $"{k}({p}) => {body}")
      |> String.join " | "
      |> sprintf "match %s"

  and [<RequireQualifiedAccess>] TypeCheckedExprFromValue<'valueExt> =
    { Value: Value<TypeValue<'valueExt>, 'valueExt>
      ValueType: TypeValue<'valueExt>
      ValueKind: Kind }

    override self.ToString() =
      $"{self.Value} : {self.ValueType} :: {self.ValueKind}"

  and [<RequireQualifiedAccess>] TypeCheckedExprQueryIterators<'valueExt> =
    NonEmptyList<TypeCheckedExprQueryIterator<'valueExt>>

  and [<RequireQualifiedAccess>] TypeCheckedExprQueryIterator<'valueExt> =
    { Location: Location
      Var: Var
      VarType: TypeQueryRow<'valueExt>
      Source: TypeCheckedExpr<'valueExt> }

    override self.ToString() =
      $"{self.Var}: {self.VarType} in {self.Source}"

  and [<RequireQualifiedAccess>] TypeCheckedExprQueryJoin<'valueExt> =
    { Location: Location
      Left: TypeCheckedExprQueryExpr<'valueExt>
      Right: TypeCheckedExprQueryExpr<'valueExt> }

    override self.ToString() = $"({self.Left}) = ({self.Right})"

  and [<RequireQualifiedAccess>] TypeCheckedExprQueryExpr<'valueExt> =
    { Location: Location
      Expr: TypeCheckedExprQueryExprRec<'valueExt> }

    override self.ToString() = self.Expr.ToString()

  and [<RequireQualifiedAccess>] TypeCheckedExprQueryExprRec<'valueExt> =
    | QueryTupleCons of List<TypeCheckedExprQueryExpr<'valueExt>>
    | QueryRecordDes of
      Expr: TypeCheckedExprQueryExpr<'valueExt> *
      Field: ResolvedIdentifier *
      IsJsonField: bool
    | QueryTupleDes of
      Expr: TypeCheckedExprQueryExpr<'valueExt> *
      Item: TupleDesSelector *
      IsJsonItem: bool
    | QueryConditional of
      Cond: TypeCheckedExprQueryExpr<'valueExt> *
      Then: TypeCheckedExprQueryExpr<'valueExt> *
      Else: TypeCheckedExprQueryExpr<'valueExt>
    | QueryUnionDes of
      Expr: TypeCheckedExprQueryExpr<'valueExt> *
      Handlers: Map<ResolvedIdentifier, TypeCheckedQueryCaseHandler<'valueExt>>
    | QuerySumDes of
      Expr: TypeCheckedExprQueryExpr<'valueExt> *
      Handlers: Map<SumConsSelector, TypeCheckedQueryCaseHandler<'valueExt>>
    | QueryApply of
      Func: TypeCheckedExprQueryExpr<'valueExt> *
      Arg: TypeCheckedExprQueryExpr<'valueExt>
    | QueryLookup of ResolvedIdentifier
    | QueryIntrinsic of QueryIntrinsic * ExpectedType: TypeQueryRow<'valueExt>
    | QueryConstant of PrimitiveValue
    | QueryClosureValue of
      Value<TypeValue<'valueExt>, 'valueExt> *
      TypeQueryRow<'valueExt>
    | QueryCastTo of
      TypeCheckedExprQueryExpr<'valueExt> *
      TypeQueryRow<'valueExt>
    | QueryCount of TypeCheckedExprQuery<'valueExt>
    | QueryExists of TypeCheckedExprQuery<'valueExt>
    | QueryArray of TypeCheckedExprQuery<'valueExt>
    | QueryCountEvaluated of ValueQuery<TypeValue<'valueExt>, 'valueExt>
    | QueryExistsEvaluated of ValueQuery<TypeValue<'valueExt>, 'valueExt>
    | QueryArrayEvaluated of ValueQuery<TypeValue<'valueExt>, 'valueExt>
    | QueryRecoveredSyntaxError of TypeCheckedExprRecoveredSyntaxError

    override self.ToString() =
      match self with
      | QueryTupleCons items ->
        let itemStrs =
          items |> List.map (fun i -> i.ToString()) |> String.join ", "

        $"({itemStrs})"
      | QueryRecordDes(e, field, isJsonField) ->
        match isJsonField with
        | false -> $"{e}.{field}"
        | true -> $"{e} -> \"{field}\""
      | QueryTupleDes(e, item, isJsonItem) ->
        match isJsonItem with
        | false -> $"{e}.{item.Index}"
        | true -> $"{e} -> {item.Index - 1}"
      | QueryConditional(cond, thenExpr, elseExpr) ->
        $"if {cond} then {thenExpr} else {elseExpr}"
      | QueryUnionDes(e, handlers) ->
        let handlerStrs =
          handlers
          |> Map.toList
          |> List.map (fun (case, handler) ->
            $"| {case} -> ({handler.Param} -> {handler.Body})")

        let handlersText = String.Join(" ", handlerStrs)
        $"match {e} with {{ {handlersText} }}"
      | QuerySumDes(e, handlers) ->
        let handlerStrs =
          handlers
          |> Map.toList
          |> List.map (fun (case, handler) ->
            $"| {case} ({handler.Param} -> {handler.Body})")

        let handlersText = String.Join(" ", handlerStrs)
        $"match {e} with {{ {handlersText} }}"
      | QueryApply(func, args) -> $"{func}({args})"
      | QueryLookup id -> id.ToString()
      | QueryIntrinsic(intrinsic, _) -> intrinsic.ToString()
      | QueryConstant c -> c.ToString()
      | QueryClosureValue(v, _) -> v.ToString()
      | QueryCastTo(v, t) -> $"{v} :: {t}"
      | QueryCount q -> $"count({q})"
      | QueryExists q -> $"any({q})"
      | QueryArray q -> $"array({q})"
      | QueryCountEvaluated v -> $"count({v})"
      | QueryExistsEvaluated v -> $"any({v})"
      | QueryArrayEvaluated v -> $"array({v})"
      | QueryRecoveredSyntaxError err -> err.ToString()


  and [<RequireQualifiedAccess>] TypeCheckedSimpleQuery<'valueExt> =
    { Iterators: TypeCheckedExprQueryIterators<'valueExt>
      Joins: Option<NonEmptyList<TypeCheckedExprQueryJoin<'valueExt>>>
      Where: Option<TypeCheckedExprQueryExpr<'valueExt>>
      Select: TypeCheckedExprQueryExpr<'valueExt>
      OrderBy: Option<TypeCheckedExprQueryExpr<'valueExt> * OrderByDirection>
      Closure: Map<ResolvedIdentifier, TypeQueryRow<'valueExt>>
      DeserializeFrom: TypeQueryRow<'valueExt>
      Distinct: Option<TypeCheckedExprQueryExpr<'valueExt>> }

    override q.ToString() =
      let joins =
        match q.Joins with
        | Some joins ->
          let joinToStr (j: TypeCheckedExprQueryJoin<'valueExt>) =
            $"({j.Left}) = ({j.Right})"

          let joined =
            joins
            |> NonEmptyList.toList
            |> List.map joinToStr
            |> fun xs -> String.Join(" and ", xs)

          $"\njoins {joined}"
        | None -> ""

      let whereToStr =
        q.Where
        |> Option.map (fun w -> $"\nwhere {w}")
        |> Option.defaultValue ""

      let selectToStr = $"\nselect {q.Select}"

      let orderByToStr =
        q.OrderBy
        |> Option.map (fun (ob, dir) -> $"\norderby {ob} {dir}")
        |> Option.defaultValue ""

      let iteratorVars =
        q.Iterators
        |> NonEmptyList.map (fun i -> i.Var.Name)
        |> NonEmptyList.toSeq
        |> String.join ", "

      let iteratorSources =
        q.Iterators
        |> NonEmptyList.map (fun i -> $"{i.Source}")
        |> NonEmptyList.toSeq
        |> String.join ", "

      let distinctStr =
        q.Distinct
        |> Option.map (fun d -> $"\ndistinct {d}")
        |> Option.defaultValue ""

      $"""(query {{ 
  from {iteratorVars} in {iteratorSources}{joins}{whereToStr}{selectToStr}{orderByToStr}}}){distinctStr}"""

  and [<RequireQualifiedAccess>] TypeCheckedExprQuery<'valueExt> =
    | SimpleQuery of TypeCheckedSimpleQuery<'valueExt>
    | UnionQueries of
      TypeCheckedExprQuery<'valueExt> *
      TypeCheckedExprQuery<'valueExt>

    override self.ToString() =
      match self with
      | SimpleQuery q -> q.ToString()
      | UnionQueries(q1, q2) ->
        let unionStrs = [ q1; q2 ] |> List.map string |> String.join "\nunion\n"
        $"(\n{unionStrs}\n)"

    member self.Closure =
      match self with
      | SimpleQuery q -> q.Closure
      | UnionQueries(q1, q2) -> Map.merge (fun _ -> id) q1.Closure q2.Closure


  and [<RequireQualifiedAccess>] TypeCheckedQueryCaseHandler<'valueExt> =
    { Param: Var
      Body: TypeCheckedExprQueryExpr<'valueExt> }

    override self.ToString() = $"{self.Param} -> {self.Body}"

  // ═══════════════════════════════════════════════════════════════════════════
  // Type-checked View AST types
  // ═══════════════════════════════════════════════════════════════════════════

  and [<RequireQualifiedAccess>] TypeCheckedExprView<'valueExt> =
    { Param: Var
      ParamType: TypeValue<'valueExt>
      Body: TypeCheckedViewNode<'valueExt>
      Location: Location }

    override self.ToString() =
      $"view ({self.Param}: {self.ParamType}) -> {self.Body}"

  and [<RequireQualifiedAccess>] TypeCheckedViewNode<'valueExt> =
    { Location: Location
      Node: TypeCheckedViewNodeRec<'valueExt> }

    override self.ToString() = self.Node.ToString()

  and [<RequireQualifiedAccess>] TypeCheckedViewNodeRec<'valueExt> =
    | ViewElement of TypeCheckedViewElement<'valueExt>
    | ViewFragment of List<TypeCheckedViewNode<'valueExt>>
    | ViewExprContainer of TypeCheckedExpr<'valueExt>
    | ViewText of string
    // View operation nodes (structurally promoted from extensions)
    | ViewMapContext of Mapper: TypeCheckedExpr<'valueExt> * Inner: TypeCheckedExpr<'valueExt>
    | ViewMapState of MapDown: TypeCheckedExpr<'valueExt> * MapUp: TypeCheckedExpr<'valueExt> * Inner: TypeCheckedExpr<'valueExt>

    override self.ToString() =
      match self with
      | ViewElement el -> el.ToString()
      | ViewFragment children ->
        let childStrs = children |> List.map string |> String.concat " "
        $"<>{childStrs}</>"
      | ViewExprContainer e -> $"{{{e}}}"
      | ViewText t -> t
      | ViewMapContext(mapper, inner) -> $"View::mapContext ({mapper}) ({inner})"
      | ViewMapState(mapDown, mapUp, inner) -> $"View::mapState ({mapDown}) ({mapUp}) ({inner})"

  and [<RequireQualifiedAccess>] TypeCheckedViewElement<'valueExt> =
    { Tag: string
      Attributes: List<TypeCheckedViewAttribute<'valueExt>>
      Children: List<TypeCheckedViewNode<'valueExt>>
      SelfClosing: bool }

    override self.ToString() =
      let attrStr =
        self.Attributes |> List.map string |> String.concat " "

      if self.SelfClosing then
        $"<{self.Tag} {attrStr} />"
      else
        let childStr =
          self.Children |> List.map string |> String.concat ""

        $"<{self.Tag} {attrStr}>{childStr}</{self.Tag}>"

  and [<RequireQualifiedAccess>] TypeCheckedViewAttribute<'valueExt> =
    | ViewAttrStringValue of Name: string * Value: string
    | ViewAttrExprValue of Name: string * Value: TypeCheckedExpr<'valueExt>

    override self.ToString() =
      match self with
      | ViewAttrStringValue(name, value) -> $"{name}=\"{value}\""
      | ViewAttrExprValue(name, _) -> $"{name}={{...}}"

  // ═══════════════════════════════════════════════════════════════════════════
  // Type-checked Coroutine AST types
  // ═══════════════════════════════════════════════════════════════════════════

  and [<RequireQualifiedAccess>] TypeCheckedExprCo<'valueExt> =
    { Body: TypeCheckedCoStep<'valueExt>
      Location: Location }

    override self.ToString() = $"co {{ {self.Body} }}"

  and [<RequireQualifiedAccess>] TypeCheckedCoStep<'valueExt> =
    { Location: Location
      Step: TypeCheckedCoStepRec<'valueExt> }

    override self.ToString() = self.Step.ToString()

  and [<RequireQualifiedAccess>] TypeCheckedCoStepRec<'valueExt> =
    | CoLetBang of Var: Var * Value: TypeCheckedExpr<'valueExt> * Rest: TypeCheckedCoStep<'valueExt>
    | CoDoBang of Value: TypeCheckedExpr<'valueExt> * Rest: TypeCheckedCoStep<'valueExt>
    | CoReturn of TypeCheckedExpr<'valueExt>
    | CoReturnBang of TypeCheckedExpr<'valueExt>
    // Co operation nodes (structurally promoted from extensions)
    | CoShow of Predicate: TypeCheckedExpr<'valueExt> * View: TypeCheckedExpr<'valueExt>
    | CoUntil of Predicate: TypeCheckedExpr<'valueExt> * Inner: TypeCheckedExpr<'valueExt>
    | CoIgnore of Inner: TypeCheckedExpr<'valueExt>
    | CoMapContext of Mapper: TypeCheckedExpr<'valueExt> * Inner: TypeCheckedExpr<'valueExt>
    | CoMapState of MapDown: TypeCheckedExpr<'valueExt> * MapUp: TypeCheckedExpr<'valueExt> * Inner: TypeCheckedExpr<'valueExt>
    | CoGetContext
    | CoGetState
    | CoSetState of Updater: TypeCheckedExpr<'valueExt>

    override self.ToString() =
      match self with
      | CoLetBang(var, value, rest) -> $"let! {var} = {value}; {rest}"
      | CoDoBang(value, rest) -> $"do! {value}; {rest}"
      | CoReturn e -> $"return {e}"
      | CoReturnBang e -> $"return! {e}"
      | CoShow(pred, view) -> $"Co::show ({pred}) ({view})"
      | CoUntil(pred, inner) -> $"Co::until ({pred}) ({inner})"
      | CoIgnore inner -> $"Co::ignore ({inner})"
      | CoMapContext(mapper, inner) -> $"Co::mapContext ({mapper}) ({inner})"
      | CoMapState(mapDown, mapUp, inner) -> $"Co::mapState ({mapDown}) ({mapUp}) ({inner})"
      | CoGetContext -> "Co::getContext"
      | CoGetState -> "Co::getState"
      | CoSetState updater -> $"Co::setState ({updater})"

  and [<RequireQualifiedAccess>] TypeCheckedExprRecoveredSyntaxError =
    { ErrorMessage: string
      ErrorLocation: Location
      RecoveryContext: string }

    override self.ToString() =
      $"<SyntaxError: {self.ErrorMessage}>"

  and [<RequireQualifiedAccess>] TypeCheckedExprErrorDanglingRecordDes<'valueExt> =
    { Expr: TypeCheckedExpr<'valueExt>
      Field: Option<ResolvedIdentifier> }

    override self.ToString() =
      match self.Field with
      | Some f -> $"{self.Expr}.{f}"
      | None -> $"{self.Expr}.<incomplete>"

  and [<RequireQualifiedAccess>] TypeCheckedExprErrorDanglingScopedIdentifier =
    { PrefixParts: List<string> }

    override self.ToString() =
      let joined = System.String.Join("::", self.PrefixParts)
      $"{joined}::<incomplete>"

  and [<RequireQualifiedAccess>] TypeCheckedExprErrorRecordDesButInvalidField<'valueExt> =
    { Expr: TypeCheckedExpr<'valueExt>
      Field: ResolvedIdentifier }

    override self.ToString() = $"{self.Expr}.{self.Field}"

  and [<RequireQualifiedAccess>] TypeCheckedExprRec<'valueExt> =
    | Primitive of PrimitiveValue
    | Lookup of TypeCheckedExprLookup<'valueExt>
    | TypeLambda of TypeCheckedExprTypeLambda<'valueExt>
    | TypeApply of TypeCheckedExprTypeApply<'valueExt>
    | TypeLet of TypeCheckedExprTypeLet<'valueExt>
    | Lambda of TypeCheckedExprLambda<'valueExt>
    | FromValue of TypeCheckedExprFromValue<'valueExt>
    | Apply of TypeCheckedExprApply<'valueExt>
    | Let of TypeCheckedExprLet<'valueExt>
    | Do of TypeCheckedExprDo<'valueExt>
    | If of TypeCheckedExprIf<'valueExt>
    | RecordCons of TypeCheckedExprRecordCons<'valueExt>
    | RecordWith of TypeCheckedExprRecordWith<'valueExt>
    | TupleCons of TypeCheckedExprTupleCons<'valueExt>
    | SumCons of TypeCheckedExprSumCons<'valueExt>
    | RecordDes of TypeCheckedExprRecordDes<'valueExt>
    | EntitiesDes of TypeCheckedExprEntitiesDes<'valueExt>
    | RelationsDes of TypeCheckedExprRelationsDes<'valueExt>
    | EntityDes of TypeCheckedExprEntityDes<'valueExt>
    | RelationDes of TypeCheckedExprRelationDes<'valueExt>
    | RelationLookupDes of TypeCheckedExprRelationLookupDes<'valueExt>
    | UnionDes of TypeCheckedExprUnionDes<'valueExt>
    | TupleDes of TypeCheckedExprTupleDes<'valueExt>
    | SumDes of TypeCheckedExprSumDes<'valueExt>
    | Query of TypeCheckedExprQuery<'valueExt>
    | View of TypeCheckedExprView<'valueExt>
    | Co of TypeCheckedExprCo<'valueExt>
    | CoOp of CoOperationKind
    | ViewOp of ViewOperationKind
    | RecoveredSyntaxError of TypeCheckedExprRecoveredSyntaxError
    | ErrorDanglingRecordDes of TypeCheckedExprErrorDanglingRecordDes<'valueExt>
    | ErrorDanglingScopedIdentifier of TypeCheckedExprErrorDanglingScopedIdentifier
    | ErrorRecordDesButInvalidField of TypeCheckedExprErrorRecordDesButInvalidField<'valueExt>

    override self.ToString() =
      match self with
      | TypeCheckedExprRec.TypeLambda({ TypeCheckedExprTypeLambda.Param = tp
                                        Body = body }) -> $"(Λ{tp} => {body})"
      | TypeApply({ Func = e; TypeArg = t }) -> $"{e} [{t}]"
      | Lambda({ Param = v
                 ParamType = t
                 Body = body }) -> $"(fun ({v.Name}: {t}) -> {body})"
      | Apply({ F = e1; Arg = e2 }) -> $"({e1} {e2})"
      | FromValue({ Value = v
                    ValueType = t
                    ValueKind = k }) -> $"({v} : {t} :: {k})"
      | Let({ Var = v
              Type = t
              Val = e1
              Rest = e2 }) -> $"(let {v.Name}: {t} = {e1} in {e2})"
      | Do({ Val = e1; Rest = e2 }) -> $"(do {e1}; {e2})"
      | TypeLet({ Name = name
                  TypeDef = t
                  Body = body }) -> $"(type {name} = {t}; {body})"
      | RecordCons { Fields = fields } ->
        let fieldStr =
          fields |> List.map (fun (k, v) -> $"{k} = {v}") |> String.concat "; "

        $"{{ {fieldStr} }}"
      | RecordWith({ Record = record; Fields = fields }) ->
        let fieldStr =
          fields |> List.map (fun (k, v) -> $"{k} = {v}") |> String.concat "; "

        $"{{ {record} with {fieldStr} }}"
      | TupleCons { Items = items } ->
        let itemStr = items |> List.map string |> String.concat ", "
        $"({itemStr})"
      | SumCons({ Selector = selector }) -> selector.ToString()
      | RecordDes({ Expr = record; Field = field }) -> $"{record}.{field}"
      | EntitiesDes({ Expr = entities }) -> $"{entities}.Entities"
      | RelationsDes({ Expr = relations }) -> $"{relations}.Relations"
      | EntityDes({ Expr = entity
                    EntityName = entityName }) -> $"{entity}.{entityName}"
      | RelationDes({ Expr = relation
                      RelationName = relationName }) ->
        $"{relation}.{relationName}"
      | RelationLookupDes({ Expr = relation
                            RelationName = relationName
                            Direction = direction }) ->
        $"{relation}.{relationName}.{direction}"
      | UnionDes({ Handlers = handlers
                   Fallback = defaultOpt }) ->
        let handlerStr =
          handlers
          |> Map.toList
          |> List.map (fun (k, (v, body)) ->
            let p =
              v |> Option.map (fun var -> var.Name) |> Option.defaultValue "_"

            $"{k}({p}) => {body}")
          |> String.concat " | "

        match defaultOpt with
        | Some defaultExpr -> $"(match {handlerStr} | _ => {defaultExpr})"
        | None -> $"(match {handlerStr})"
      | TupleDes({ TypeCheckedExprTupleDes.Tuple = tuple
                   Item = selector }) -> $"{tuple}.{selector.Index}"
      | SumDes { Handlers = handlers } ->
        let handlerStr =
          handlers
          |> Map.toList
          |> List.map (fun (k, (v, body)) ->
            let p =
              v |> Option.map (fun var -> var.Name) |> Option.defaultValue "_"

            $"{k}({p}) => {body}")
          |> String.concat " | "

        $"(match {handlerStr})"
      | Primitive p -> p.ToString()
      | Lookup id -> id.ToString()
      | If({ Cond = cond
             Then = thenExpr
             Else = elseExpr }) ->
        $"(if {cond} then {thenExpr} else {elseExpr})"
      | Query q -> q.ToString()
      | View v -> v.ToString()
      | Co c -> c.ToString()
      | CoOp op -> $"Co::{op.Name}"
      | ViewOp op -> $"View::{op.Name}"
      | RecoveredSyntaxError err -> err.ToString()
      | ErrorDanglingRecordDes err -> err.ToString()
      | ErrorDanglingScopedIdentifier err -> err.ToString()
      | ErrorRecordDesButInvalidField err -> err.ToString()


  and [<RequireQualifiedAccess>] TypeCheckedExpr<'valueExt> =
    { Expr: TypeCheckedExprRec<'valueExt>
      Location: Location
      Type: TypeValue<'valueExt>
      Kind: Kind
      Scope: TypeCheckScope }

    override self.ToString() : string = self.Expr.ToString()

  and [<RequireQualifiedAccess>] TypeCheckedCaseHandler<'valueExt> =
    Option<Var> * TypeCheckedExpr<'valueExt>

  // ── RunnableExpr: validated (non-partial) expression tree ──────────────
  // Identical to TypeCheckedExpr but without RecoveredSyntaxError.
  // Produced by converting TypeCheckedExpr after type checking succeeds.

  and [<RequireQualifiedAccess>] RunnableExprLookup<'valueExt> =
    { Id: ResolvedIdentifier }

    override self.ToString() = self.Id.ToString()

  and [<RequireQualifiedAccess>] RunnableExprLambda<'valueExt> =
    { Param: Var
      ParamType: TypeValue<'valueExt>
      Body: RunnableExpr<'valueExt>
      BodyType: TypeValue<'valueExt> }

    override self.ToString() =
      $"fun ({self.Param}: {self.ParamType}) -> {self.Body}"

  and [<RequireQualifiedAccess>] RunnableExprApply<'valueExt> =
    { F: RunnableExpr<'valueExt>
      Arg: RunnableExpr<'valueExt> }

    override self.ToString() = $"({self.F})({self.Arg})"

  and [<RequireQualifiedAccess>] RunnableExprLet<'valueExt> =
    { Var: Var
      Type: TypeValue<'valueExt>
      Val: RunnableExpr<'valueExt>
      Rest: RunnableExpr<'valueExt> }

    override self.ToString() =
      $"let {self.Var}: {self.Type} = {self.Val} in {self.Rest}"

  and [<RequireQualifiedAccess>] RunnableExprDo<'valueExt> =
    { Val: RunnableExpr<'valueExt>
      Rest: RunnableExpr<'valueExt> }

    override self.ToString() = $"do {self.Val}; {self.Rest}"

  and [<RequireQualifiedAccess>] RunnableExprIf<'valueExt> =
    { Cond: RunnableExpr<'valueExt>
      Then: RunnableExpr<'valueExt>
      Else: RunnableExpr<'valueExt> }

    override self.ToString() =
      $"if {self.Cond} then {self.Then} else {self.Else}"

  and [<RequireQualifiedAccess>] RunnableExprTypeLambda<'valueExt> =
    { Param: TypeParameter
      Body: RunnableExpr<'valueExt> }

    override self.ToString() = $"Λ{self.Param} => {self.Body}"

  and [<RequireQualifiedAccess>] RunnableExprTypeApply<'valueExt> =
    { Func: RunnableExpr<'valueExt>
      TypeArg: TypeValue<'valueExt> }

    override self.ToString() = $"{self.Func}[{self.TypeArg}]"

  and [<RequireQualifiedAccess>] RunnableExprTypeLet<'valueExt> =
    { Name: string
      TypeDef: TypeValue<'valueExt>
      Body: RunnableExpr<'valueExt> }

    override self.ToString() =
      $"type {self.Name} = {self.TypeDef}; {self.Body}"

  and [<RequireQualifiedAccess>] RunnableExprRecordCons<'valueExt> =
    { Fields: List<ResolvedIdentifier * RunnableExpr<'valueExt>> }

    override self.ToString() =
      let fields =
        self.Fields |> List.map (fun (k, v) -> $"{k}: {v}") |> String.join "; "

      $"{{ {fields} }}"

  and [<RequireQualifiedAccess>] RunnableExprRecordWith<'valueExt> =
    { Record: RunnableExpr<'valueExt>
      Fields: List<ResolvedIdentifier * RunnableExpr<'valueExt>> }

    override self.ToString() =
      let fields =
        self.Fields |> List.map (fun (k, v) -> $"{k}: {v}") |> String.join "; "

      $"{{ {self.Record} with {fields} }}"

  and [<RequireQualifiedAccess>] RunnableExprTupleCons<'valueExt> =
    { Items: List<RunnableExpr<'valueExt>> }

    override self.ToString() =
      let items = self.Items |> List.map string |> String.join ", "
      $"({items})"

  and [<RequireQualifiedAccess>] RunnableExprTupleDes<'valueExt> =
    { Tuple: RunnableExpr<'valueExt>
      Item: TupleDesSelector }

    override self.ToString() = $"{self.Tuple}.{self.Item.Index}"

  and [<RequireQualifiedAccess>] RunnableExprSumCons<'valueExt> =
    { Selector: SumConsSelector }

    override self.ToString() = self.Selector.ToString()

  and [<RequireQualifiedAccess>] RunnableExprRecordDes<'valueExt> =
    { Expr: RunnableExpr<'valueExt>
      Field: ResolvedIdentifier }

    override self.ToString() = $"{self.Expr}.{self.Field}"

  and [<RequireQualifiedAccess>] RunnableExprEntitiesDes<'valueExt> =
    { Expr: RunnableExpr<'valueExt> }

    override self.ToString() = $"{self.Expr}.Entities"

  and [<RequireQualifiedAccess>] RunnableExprRelationsDes<'valueExt> =
    { Expr: RunnableExpr<'valueExt> }

    override self.ToString() = $"{self.Expr}.Relations"

  and [<RequireQualifiedAccess>] RunnableExprEntityDes<'valueExt> =
    { Expr: RunnableExpr<'valueExt>
      EntityName: SchemaEntityName }

    override self.ToString() = $"{self.Expr}.{self.EntityName}"

  and [<RequireQualifiedAccess>] RunnableExprRelationDes<'valueExt> =
    { Expr: RunnableExpr<'valueExt>
      RelationName: SchemaRelationName }

    override self.ToString() = $"{self.Expr}.{self.RelationName}"

  and [<RequireQualifiedAccess>] RunnableExprRelationLookupDes<'valueExt> =
    { Expr: RunnableExpr<'valueExt>
      RelationName: SchemaRelationName
      Direction: RelationLookupDirection }

    override self.ToString() =
      $"{self.Expr}.{self.RelationName}.{self.Direction}"

  and [<RequireQualifiedAccess>] RunnableExprUnionDes<'valueExt> =
    { Handlers: Map<ResolvedIdentifier, RunnableCaseHandler<'valueExt>>
      Fallback: Option<RunnableExpr<'valueExt>> }

    override self.ToString() =
      let handlers =
        self.Handlers
        |> Map.toList
        |> List.map (fun (k, (v, body)) ->
          let p = v |> Option.map string |> Option.defaultValue "_"
          $"{k}({p}) => {body}")
        |> String.join " | "

      match self.Fallback with
      | Some fallback -> $"match {handlers} | _ => {fallback}"
      | None -> $"match {handlers}"

  and [<RequireQualifiedAccess>] RunnableExprSumDes<'valueExt> =
    { Handlers: Map<SumConsSelector, RunnableCaseHandler<'valueExt>> }

    override self.ToString() =
      self.Handlers
      |> Map.toList
      |> List.map (fun (k, (v, body)) ->
        let p = v |> Option.map string |> Option.defaultValue "_"
        $"{k}({p}) => {body}")
      |> String.join " | "
      |> sprintf "match %s"

  and [<RequireQualifiedAccess>] RunnableExprFromValue<'valueExt> =
    { Value: Value<TypeValue<'valueExt>, 'valueExt>
      ValueType: TypeValue<'valueExt>
      ValueKind: Kind }

    override self.ToString() =
      $"{self.Value} : {self.ValueType} :: {self.ValueKind}"

  // ── Runnable query types: mirror of TypeCheckedExprQuery* with RunnableExpr ──

  and [<RequireQualifiedAccess>] RunnableExprQueryIterators<'valueExt> =
    NonEmptyList<RunnableExprQueryIterator<'valueExt>>

  and [<RequireQualifiedAccess>] RunnableExprQueryIterator<'valueExt> =
    { Location: Location
      Var: Var
      VarType: TypeQueryRow<'valueExt>
      Source: RunnableExpr<'valueExt> }

    override self.ToString() =
      $"{self.Var}: {self.VarType} in {self.Source}"

  and [<RequireQualifiedAccess>] RunnableExprQueryJoin<'valueExt> =
    { Location: Location
      Left: RunnableExprQueryExpr<'valueExt>
      Right: RunnableExprQueryExpr<'valueExt> }

    override self.ToString() = $"({self.Left}) = ({self.Right})"

  and [<RequireQualifiedAccess>] RunnableExprQueryExpr<'valueExt> =
    { Location: Location
      Expr: RunnableExprQueryExprRec<'valueExt> }

    override self.ToString() = self.Expr.ToString()

  and [<RequireQualifiedAccess>] RunnableExprQueryExprRec<'valueExt> =
    | QueryTupleCons of List<RunnableExprQueryExpr<'valueExt>>
    | QueryRecordDes of
      Expr: RunnableExprQueryExpr<'valueExt> *
      Field: ResolvedIdentifier *
      IsJsonField: bool
    | QueryTupleDes of
      Expr: RunnableExprQueryExpr<'valueExt> *
      Item: TupleDesSelector *
      IsJsonItem: bool
    | QueryConditional of
      Cond: RunnableExprQueryExpr<'valueExt> *
      Then: RunnableExprQueryExpr<'valueExt> *
      Else: RunnableExprQueryExpr<'valueExt>
    | QueryUnionDes of
      Expr: RunnableExprQueryExpr<'valueExt> *
      Handlers: Map<ResolvedIdentifier, RunnableQueryCaseHandler<'valueExt>>
    | QuerySumDes of
      Expr: RunnableExprQueryExpr<'valueExt> *
      Handlers: Map<SumConsSelector, RunnableQueryCaseHandler<'valueExt>>
    | QueryApply of
      Func: RunnableExprQueryExpr<'valueExt> *
      Arg: RunnableExprQueryExpr<'valueExt>
    | QueryLookup of ResolvedIdentifier
    | QueryIntrinsic of QueryIntrinsic * ExpectedType: TypeQueryRow<'valueExt>
    | QueryConstant of PrimitiveValue
    | QueryClosureValue of
      Value<TypeValue<'valueExt>, 'valueExt> *
      TypeQueryRow<'valueExt>
    | QueryCastTo of
      RunnableExprQueryExpr<'valueExt> *
      TypeQueryRow<'valueExt>
    | QueryCount of RunnableExprQuery<'valueExt>
    | QueryExists of RunnableExprQuery<'valueExt>
    | QueryArray of RunnableExprQuery<'valueExt>
    | QueryCountEvaluated of ValueQuery<TypeValue<'valueExt>, 'valueExt>
    | QueryExistsEvaluated of ValueQuery<TypeValue<'valueExt>, 'valueExt>
    | QueryArrayEvaluated of ValueQuery<TypeValue<'valueExt>, 'valueExt>

  and [<RequireQualifiedAccess>] RunnableSimpleQuery<'valueExt> =
    { Iterators: RunnableExprQueryIterators<'valueExt>
      Joins: Option<NonEmptyList<RunnableExprQueryJoin<'valueExt>>>
      Where: Option<RunnableExprQueryExpr<'valueExt>>
      Select: RunnableExprQueryExpr<'valueExt>
      OrderBy: Option<RunnableExprQueryExpr<'valueExt> * OrderByDirection>
      Closure: Map<ResolvedIdentifier, TypeQueryRow<'valueExt>>
      DeserializeFrom: TypeQueryRow<'valueExt>
      Distinct: Option<RunnableExprQueryExpr<'valueExt>> }

  and [<RequireQualifiedAccess>] RunnableExprQuery<'valueExt> =
    | SimpleQuery of RunnableSimpleQuery<'valueExt>
    | UnionQueries of
      RunnableExprQuery<'valueExt> *
      RunnableExprQuery<'valueExt>

    member self.Closure =
      match self with
      | SimpleQuery q -> q.Closure
      | UnionQueries(q1, q2) -> Map.merge (fun _ -> id) q1.Closure q2.Closure

  and [<RequireQualifiedAccess>] RunnableQueryCaseHandler<'valueExt> =
    { Param: Var
      Body: RunnableExprQueryExpr<'valueExt> }

  // ═══════════════════════════════════════════════════════════════════════════
  // Runnable View AST types
  // ═══════════════════════════════════════════════════════════════════════════

  and [<RequireQualifiedAccess>] RunnableExprView<'valueExt> =
    { Param: Var
      ParamType: TypeValue<'valueExt>
      Body: RunnableViewNode<'valueExt>
      Location: Location }

    override self.ToString() =
      $"view ({self.Param}: {self.ParamType}) -> {self.Body}"

  and [<RequireQualifiedAccess>] RunnableViewNode<'valueExt> =
    { Location: Location
      Node: RunnableViewNodeRec<'valueExt> }

    override self.ToString() = self.Node.ToString()

  and [<RequireQualifiedAccess>] RunnableViewNodeRec<'valueExt> =
    | ViewElement of RunnableViewElement<'valueExt>
    | ViewFragment of List<RunnableViewNode<'valueExt>>
    | ViewExprContainer of RunnableExpr<'valueExt>
    | ViewText of string
    | ViewMapContext of Mapper: RunnableExpr<'valueExt> * Inner: RunnableExpr<'valueExt>
    | ViewMapState of MapDown: RunnableExpr<'valueExt> * MapUp: RunnableExpr<'valueExt> * Inner: RunnableExpr<'valueExt>

    override self.ToString() =
      match self with
      | ViewElement el -> el.ToString()
      | ViewFragment children ->
        let childStrs = children |> List.map string |> String.concat " "
        $"<>{childStrs}</>"
      | ViewExprContainer e -> $"{{{e}}}"
      | ViewText t -> t
      | ViewMapContext(mapper, inner) -> $"View::mapContext ({mapper}) ({inner})"
      | ViewMapState(mapDown, mapUp, inner) -> $"View::mapState ({mapDown}) ({mapUp}) ({inner})"

  and [<RequireQualifiedAccess>] RunnableViewElement<'valueExt> =
    { Tag: string
      Attributes: List<RunnableViewAttribute<'valueExt>>
      Children: List<RunnableViewNode<'valueExt>>
      SelfClosing: bool }

    override self.ToString() =
      let attrStr =
        self.Attributes |> List.map string |> String.concat " "

      if self.SelfClosing then
        $"<{self.Tag} {attrStr} />"
      else
        let childStr =
          self.Children |> List.map string |> String.concat ""

        $"<{self.Tag} {attrStr}>{childStr}</{self.Tag}>"

  and [<RequireQualifiedAccess>] RunnableViewAttribute<'valueExt> =
    | ViewAttrStringValue of Name: string * Value: string
    | ViewAttrExprValue of Name: string * Value: RunnableExpr<'valueExt>

    override self.ToString() =
      match self with
      | ViewAttrStringValue(name, value) -> $"{name}=\"{value}\""
      | ViewAttrExprValue(name, _) -> $"{name}={{...}}"

  // ═══════════════════════════════════════════════════════════════════════════
  // Runnable Coroutine AST types
  // ═══════════════════════════════════════════════════════════════════════════

  and [<RequireQualifiedAccess>] RunnableExprCo<'valueExt> =
    { Body: RunnableCoStep<'valueExt>
      Location: Location }

    override self.ToString() = $"co {{ {self.Body} }}"

  and [<RequireQualifiedAccess>] RunnableCoStep<'valueExt> =
    { Location: Location
      Step: RunnableCoStepRec<'valueExt> }

    override self.ToString() = self.Step.ToString()

  and [<RequireQualifiedAccess>] RunnableCoStepRec<'valueExt> =
    | CoLetBang of Var: Var * Value: RunnableExpr<'valueExt> * Rest: RunnableCoStep<'valueExt>
    | CoDoBang of Value: RunnableExpr<'valueExt> * Rest: RunnableCoStep<'valueExt>
    | CoReturn of RunnableExpr<'valueExt>
    | CoReturnBang of RunnableExpr<'valueExt>
    | CoShow of Predicate: RunnableExpr<'valueExt> * View: RunnableExpr<'valueExt>
    | CoUntil of Predicate: RunnableExpr<'valueExt> * Inner: RunnableExpr<'valueExt>
    | CoIgnore of Inner: RunnableExpr<'valueExt>
    | CoMapContext of Mapper: RunnableExpr<'valueExt> * Inner: RunnableExpr<'valueExt>
    | CoMapState of MapDown: RunnableExpr<'valueExt> * MapUp: RunnableExpr<'valueExt> * Inner: RunnableExpr<'valueExt>
    | CoGetContext
    | CoGetState
    | CoSetState of Updater: RunnableExpr<'valueExt>

    override self.ToString() =
      match self with
      | CoLetBang(var, value, rest) -> $"let! {var} = {value}; {rest}"
      | CoDoBang(value, rest) -> $"do! {value}; {rest}"
      | CoReturn e -> $"return {e}"
      | CoReturnBang e -> $"return! {e}"
      | CoShow(pred, view) -> $"Co::show ({pred}) ({view})"
      | CoUntil(pred, inner) -> $"Co::until ({pred}) ({inner})"
      | CoIgnore inner -> $"Co::ignore ({inner})"
      | CoMapContext(mapper, inner) -> $"Co::mapContext ({mapper}) ({inner})"
      | CoMapState(mapDown, mapUp, inner) -> $"Co::mapState ({mapDown}) ({mapUp}) ({inner})"
      | CoGetContext -> "Co::getContext"
      | CoGetState -> "Co::getState"
      | CoSetState updater -> $"Co::setState ({updater})"

  and [<RequireQualifiedAccess>] RunnableExprRec<'valueExt> =
    | Primitive of PrimitiveValue
    | Lookup of RunnableExprLookup<'valueExt>
    | TypeLambda of RunnableExprTypeLambda<'valueExt>
    | TypeApply of RunnableExprTypeApply<'valueExt>
    | TypeLet of RunnableExprTypeLet<'valueExt>
    | Lambda of RunnableExprLambda<'valueExt>
    | FromValue of RunnableExprFromValue<'valueExt>
    | Apply of RunnableExprApply<'valueExt>
    | Let of RunnableExprLet<'valueExt>
    | Do of RunnableExprDo<'valueExt>
    | If of RunnableExprIf<'valueExt>
    | RecordCons of RunnableExprRecordCons<'valueExt>
    | RecordWith of RunnableExprRecordWith<'valueExt>
    | TupleCons of RunnableExprTupleCons<'valueExt>
    | SumCons of RunnableExprSumCons<'valueExt>
    | RecordDes of RunnableExprRecordDes<'valueExt>
    | EntitiesDes of RunnableExprEntitiesDes<'valueExt>
    | RelationsDes of RunnableExprRelationsDes<'valueExt>
    | EntityDes of RunnableExprEntityDes<'valueExt>
    | RelationDes of RunnableExprRelationDes<'valueExt>
    | RelationLookupDes of RunnableExprRelationLookupDes<'valueExt>
    | UnionDes of RunnableExprUnionDes<'valueExt>
    | TupleDes of RunnableExprTupleDes<'valueExt>
    | SumDes of RunnableExprSumDes<'valueExt>
    | Query of RunnableExprQuery<'valueExt>
    | View of RunnableExprView<'valueExt>
    | Co of RunnableExprCo<'valueExt>
    | CoOp of CoOperationKind
    | ViewOp of ViewOperationKind

    override self.ToString() =
      match self with
      | RunnableExprRec.TypeLambda({ RunnableExprTypeLambda.Param = tp
                                     Body = body }) -> $"(Λ{tp} => {body})"
      | TypeApply({ Func = e; TypeArg = t }) -> $"{e} [{t}]"
      | Lambda({ Param = v
                 ParamType = t
                 Body = body }) -> $"(fun ({v.Name}: {t}) -> {body})"
      | Apply({ F = e1; Arg = e2 }) -> $"({e1} {e2})"
      | FromValue({ Value = v
                    ValueType = t
                    ValueKind = k }) -> $"({v} : {t} :: {k})"
      | Let({ Var = v
              Type = t
              Val = e1
              Rest = e2 }) -> $"(let {v.Name}: {t} = {e1} in {e2})"
      | Do({ Val = e1; Rest = e2 }) -> $"(do {e1}; {e2})"
      | TypeLet({ Name = name
                  TypeDef = t
                  Body = body }) -> $"(type {name} = {t}; {body})"
      | RecordCons { Fields = fields } ->
        let fieldStr =
          fields |> List.map (fun (k, v) -> $"{k} = {v}") |> String.concat "; "

        $"{{ {fieldStr} }}"
      | RecordWith({ Record = record; Fields = fields }) ->
        let fieldStr =
          fields |> List.map (fun (k, v) -> $"{k} = {v}") |> String.concat "; "

        $"{{ {record} with {fieldStr} }}"
      | TupleCons { Items = items } ->
        let itemStr = items |> List.map string |> String.concat ", "
        $"({itemStr})"
      | SumCons({ Selector = selector }) -> selector.ToString()
      | RecordDes({ Expr = record; Field = field }) -> $"{record}.{field}"
      | EntitiesDes({ Expr = entities }) -> $"{entities}.Entities"
      | RelationsDes({ Expr = relations }) -> $"{relations}.Relations"
      | EntityDes({ Expr = entity
                    EntityName = entityName }) -> $"{entity}.{entityName}"
      | RelationDes({ Expr = relation
                      RelationName = relationName }) ->
        $"{relation}.{relationName}"
      | RelationLookupDes({ Expr = relation
                            RelationName = relationName
                            Direction = direction }) ->
        $"{relation}.{relationName}.{direction}"
      | UnionDes({ Handlers = handlers
                   Fallback = defaultOpt }) ->
        let handlerStr =
          handlers
          |> Map.toList
          |> List.map (fun (k, (v, body)) ->
            let p =
              v |> Option.map (fun var -> var.Name) |> Option.defaultValue "_"

            $"{k}({p}) => {body}")
          |> String.concat " | "

        match defaultOpt with
        | Some defaultExpr -> $"(match {handlerStr} | _ => {defaultExpr})"
        | None -> $"(match {handlerStr})"
      | TupleDes({ RunnableExprTupleDes.Tuple = tuple
                   Item = selector }) -> $"{tuple}.{selector.Index}"
      | SumDes { Handlers = handlers } ->
        let handlerStr =
          handlers
          |> Map.toList
          |> List.map (fun (k, (v, body)) ->
            let p =
              v |> Option.map (fun var -> var.Name) |> Option.defaultValue "_"

            $"{k}({p}) => {body}")
          |> String.concat " | "

        $"(match {handlerStr})"
      | Primitive p -> p.ToString()
      | Lookup id -> id.ToString()
      | If({ Cond = cond
             Then = thenExpr
             Else = elseExpr }) ->
        $"(if {cond} then {thenExpr} else {elseExpr})"
      | Query q -> q.ToString()
      | View v -> v.ToString()
      | Co c -> c.ToString()
      | CoOp op -> $"Co::{op.Name}"
      | ViewOp op -> $"View::{op.Name}"

  and [<RequireQualifiedAccess>] RunnableExpr<'valueExt> =
    { Expr: RunnableExprRec<'valueExt>
      Location: Location
      Type: TypeValue<'valueExt>
      Kind: Kind
      Scope: TypeCheckScope }

    override self.ToString() : string = self.Expr.ToString()

  and [<RequireQualifiedAccess>] RunnableCaseHandler<'valueExt> =
    Option<Var> * RunnableExpr<'valueExt>

  and CaseHandler<'T, 'Id, 'valueExt when 'Id: comparison> =
    Option<Var> * Expr<'T, 'Id, 'valueExt>

  and PrimitiveValue =
    | Int32 of Int32
    | Int64 of Int64
    | Float32 of float32
    | Float64 of float
    | Decimal of decimal
    | Bool of bool
    | Guid of Guid
    | String of string
    | Date of DateOnly
    | DateTime of DateTime
    | TimeSpan of TimeSpan
    | Unit

    override self.ToString() : string =
      match self with
      | Int32 v -> v.ToString()
      | Int64 v -> v.ToString()
      | Float32 v -> v.ToString()
      | Float64 v -> v.ToString()
      | Decimal v -> v.ToString()
      | Bool v -> v.ToString()
      | Guid v -> v.ToString()
      | String v -> $"\"{v}\""
      | Date v -> Iso8601.DateOnly.print v
      | DateTime v -> Iso8601.DateTime.printUtc v
      | TimeSpan v -> v.ToString()
      | Unit -> "()"

    member self.ToObject() =
      match self with
      | Int32 v -> box v
      | Int64 v -> box v
      | Float32 v -> box v
      | Float64 v -> box v
      | Decimal v -> box v
      | Bool v -> box v
      | Guid v -> box v
      | String v -> box v
      | Date v -> box v
      | DateTime v -> box v
      | TimeSpan v -> box v
      | Unit -> null

  and ValueQueryIterators<'T, 'valueExt> =
    NonEmptyList<ValueQueryIterator<'T, 'valueExt>>

  and ValueQueryIterator<'T, 'valueExt> =
    { Location: Location
      Var: Var
      VarType: TypeQueryRow<'valueExt>
      Source: Value<'T, 'valueExt> }

    override self.ToString() =
      $"{self.Var}: {self.VarType} in {self.Source}"

  and ValueQuerySimple<'T, 'valueExt> =
    { Iterators: ValueQueryIterators<'T, 'valueExt>
      Joins: Option<NonEmptyList<RunnableExprQueryJoin<'valueExt>>>
      Where: Option<RunnableExprQueryExpr<'valueExt>>
      Select: RunnableExprQueryExpr<'valueExt>
      OrderBy: Option<RunnableExprQueryExpr<'valueExt> * OrderByDirection>
      Distinct: Option<RunnableExprQueryExpr<'valueExt>>
      DeserializeFrom: TypeQueryRow<'valueExt> }

    override q.ToString() =
      let joins =
        match q.Joins with
        | Some joins ->
          let joinToStr (j: RunnableExprQueryJoin<'valueExt>) =
            $"({j.Left}) = ({j.Right})"

          let joined =
            joins
            |> NonEmptyList.toList
            |> List.map joinToStr
            |> fun xs -> String.Join(" and ", xs)

          $"\njoins {joined}"
        | None -> ""

      let whereToStr =
        q.Where
        |> Option.map (fun w -> $"\nwhere {w}")
        |> Option.defaultValue ""

      let selectToStr = $"\nselect {q.Select}"

      let orderByToStr =
        q.OrderBy
        |> Option.map (fun (ob, dir) -> $"\norderby {ob} {dir}")
        |> Option.defaultValue ""

      let iteratorVars =
        q.Iterators
        |> NonEmptyList.map (fun i -> i.Var.Name)
        |> NonEmptyList.toSeq
        |> String.join ", "

      let iteratorSources =
        q.Iterators
        |> NonEmptyList.map (fun i -> $"{i.Source}")
        |> NonEmptyList.toSeq
        |> String.join ", "

      let distinctStr =
        q.Distinct
        |> Option.map (fun d -> $"\ndistinct {d}")
        |> Option.defaultValue ""

      $"""(query {{ 
  from {iteratorVars} in {iteratorSources}{joins}{whereToStr}{selectToStr}{orderByToStr}}}){distinctStr}"""


  and ValueQuery<'T, 'valueExt> =
    | ValueQuerySimple of ValueQuerySimple<'T, 'valueExt>
    | ValueUnionQueries of
      ValueQuery<'T, 'valueExt> *
      ValueQuery<'T, 'valueExt> *
      DeserializeFrom: TypeQueryRow<'valueExt>

    member self.DeserializeFrom: TypeQueryRow<'valueExt> =
      match self with
      | ValueQuerySimple q -> q.DeserializeFrom
      | ValueUnionQueries(_, _, deserializeFrom) -> deserializeFrom

    override self.ToString() =
      match self with
      | ValueQuerySimple q -> q.ToString()
      | ValueUnionQueries(q1, q2, _) ->
        let unionStrs =
          [ q1; q2 ]
          |> List.map (fun q -> q.ToString())
          |> String.join "\nunion\n"

        $"(\n{unionStrs}\n)"

  and ValueCo<'T, 'valueExt> =
    | CoOp of CoOperationKind * args: List<Value<'T, 'valueExt>>

    override self.ToString() =
      match self with
      | CoOp(kind, args) ->
        let argsStr = args |> List.map string |> String.concat ", "
        $"Co::{kind.Name}({argsStr})"

  and ValueView<'T, 'valueExt> =
    | ViewOp of ViewOperationKind * args: List<Value<'T, 'valueExt>>

    override self.ToString() =
      match self with
      | ViewOp(kind, args) ->
        let argsStr = args |> List.map string |> String.concat ", "
        $"View::{kind.Name}({argsStr})"

  and Value<'T, 'valueExt> =
    | TypeLambda of TypeParameter * RunnableExpr<'valueExt>
    | Lambda of
      Var *
      RunnableExpr<'valueExt> *
      Map<ResolvedIdentifier, Value<'T, 'valueExt>> *
      TypeCheckScope
    | Record of Map<ResolvedIdentifier, Value<'T, 'valueExt>>
    | UnionCase of ResolvedIdentifier * Value<'T, 'valueExt>
    | RecordDes of ResolvedIdentifier
    | UnionCons of ResolvedIdentifier
    | Tuple of List<Value<'T, 'valueExt>>
    | Sum of SumConsSelector * Value<'T, 'valueExt>
    | Primitive of PrimitiveValue
    | Query of ValueQuery<'T, 'valueExt>
    | Co of ValueCo<'T, 'valueExt>
    | View of ValueView<'T, 'valueExt>
    | Var of Var
    | Ext of 'valueExt * applicableId: Option<ResolvedIdentifier>

    override self.ToString() : string =
      match self with
      | TypeLambda(tp, body) -> $"(Fun {tp.ToString()} => {body})"
      | Lambda(v, body, _closure, _scope) -> $"(fun {v.Name} -> {body})"
      | Record fields ->
        let fieldStr =
          fields
          |> Map.toList
          |> List.map (fun (k, v) -> $"{k.ToString()} = {v.ToString()}")
          |> String.concat "; "

        $"{{ {fieldStr} }}"
      | UnionCase(case, value) -> $"{case}({value.ToString()})"
      | RecordDes ts -> ts.ToString()
      | UnionCons ts -> ts.ToString()
      | Tuple values ->
        let valueStr =
          values |> List.map (fun v -> v.ToString()) |> String.concat ", "

        $"({valueStr})"
      | Sum(selector, value) ->
        $"{selector.Case}Of{selector.Count}({value.ToString()})"
      | Primitive p -> p.ToString()
      | Var v -> v.Name
      | Query q -> q.ToString()
      | Co c -> c.ToString()
      | View v -> v.ToString()
      | Ext(e, _) -> e.ToString()
