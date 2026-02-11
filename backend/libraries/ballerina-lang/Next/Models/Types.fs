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

  type LocalIdentifier = { Name: string }

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



  type TypeParameter =
    { Name: string
      Kind: Kind }

    override p.ToString() = $"[{p.Name}:{p.Kind}]"

  and Kind =
    | Symbol
    | Star
    | Schema
    | Arrow of Kind * Kind

    override k.ToString() =
      match k with
      | Symbol -> "Symbol"
      | Star -> "*"
      | Schema -> "Schema"
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

  and SumConsSelector = { Case: int; Count: int }
  and TupleDesSelector = { Index: int }

  and SchemaEntityName = { Name: string }
  and SchemaRelationName = { Name: string }

  and SearchByLookup =
    { Identifier: Identifier
      Lookups: List<string> }

  and SchemaEntityPropertyExpr<'valueExt> =
    { Name: LocalIdentifier
      Path: Option<SchemaPathExpr>
      Type: TypeExpr<'valueExt>
      Body: Expr<TypeExpr<'valueExt>, Identifier, 'valueExt> }

  and SchemaEntityHook =
    | Creating
    | Created
    | Updating
    | Updated
    | Deleting
    | Deleted

  and SchemaEntityExpr<'valueExt> =
    { Name: SchemaEntityName
      Type: TypeExpr<'valueExt>
      Id: TypeExpr<'valueExt>
      SearchBy: List<SearchByLookup>
      Properties: List<SchemaEntityPropertyExpr<'valueExt>>
      OnCreating: Option<Expr<TypeExpr<'valueExt>, Identifier, 'valueExt>>
      OnCreated: Option<Expr<TypeExpr<'valueExt>, Identifier, 'valueExt>>
      OnUpdating: Option<Expr<TypeExpr<'valueExt>, Identifier, 'valueExt>>
      OnUpdated: Option<Expr<TypeExpr<'valueExt>, Identifier, 'valueExt>>
      OnDeleting: Option<Expr<TypeExpr<'valueExt>, Identifier, 'valueExt>>
      OnDeleted: Option<Expr<TypeExpr<'valueExt>, Identifier, 'valueExt>> }

  and Cardinality =
    | Zero
    | One
    | Many

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
      | Iterator collection -> $"Iterator({collection.Mapper}::{collection.TypeDef})"

  and SchemaPathSegmentExpr = Option<LocalIdentifier> * SchemaPathTypeDecompositionExpr
  and SchemaPathExpr = List<SchemaPathSegmentExpr>

  and SchemaRelationHook =
    | Linking
    | Unlinking
    | Linked
    | Unlinked

  and SchemaRelationExpr<'valueExt> =
    { Name: SchemaRelationName
      From: Identifier * Option<SchemaPathExpr>
      To: Identifier * Option<SchemaPathExpr>
      Cardinality: Option<SchemaRelationCardinality>
      OnLinking: Option<Expr<TypeExpr<'valueExt>, Identifier, 'valueExt>>
      OnUnlinking: Option<Expr<TypeExpr<'valueExt>, Identifier, 'valueExt>>
      OnLinked: Option<Expr<TypeExpr<'valueExt>, Identifier, 'valueExt>>
      OnUnlinked: Option<Expr<TypeExpr<'valueExt>, Identifier, 'valueExt>> }

  and SchemaExpr<'valueExt> =
    { DeclaredAtForNominalEquality: Location
      Entities: List<SchemaEntityExpr<'valueExt>>
      Relations: List<SchemaRelationExpr<'valueExt>> }

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
    | Entity of s: TypeExpr<'valueExt> * e: TypeExpr<'valueExt> * e': TypeExpr<'valueExt> * e_id: TypeExpr<'valueExt>
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
        let comma = ", " in $"LetSymbols({String.Join(comma, names)}):{symbolsKind} in {body})"
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
      | Schema s -> $"Schema[{s.Entities.Length} Entities, {s.Relations.Length} Relations]"
      | Entities e -> $"SchemaEntities[{e}]"
      | Relations e -> $"SchemaRelations[{e}]"
      | Entity(s, e, e_with_props, id) -> $"SchemaEntity[Schema[{s}][{e}][{e_with_props}][{id}]"
      | Relation(s, f, f_with_props, f_id, t, t_with_props, t_id) ->
        $"SchemaRelation[Schema[{s}][{f}][{f_with_props}][{f_id}][{t}][{t_with_props}][{t_id}]"
      | RelationLookupOne(s, t', f_id, t_id) -> $"SchemaLookupOne[Schema[{s}][{t'}][{f_id}][{t_id}]"
      | RelationLookupOption(s, t', f_id, t_id) -> $"SchemaLookupOption[Schema[{s}][{t'}][{f_id}][{t_id}]"
      | RelationLookupMany(s, t', f_id, t_id) -> $"SchemaLookupMany[Schema[{s}][{t'}][{f_id}][{t_id}]"


  and TypeBinding<'valueExt> =
    { Identifier: Identifier
      Type: TypeExpr<'valueExt> }

  and TypeVariablesScope<'valueExt> = Map<string, TypeValue<'valueExt> * Kind>
  and TypeParametersScope = Map<string, Kind>

  // all the applicables in type expressions minus lambdas (clear from type expr eval impl)
  and SymbolicTypeApplication<'valueExt> =
    | Lookup of Identifier * TypeValue<'valueExt>
    | Application of SymbolicTypeApplication<'valueExt> * TypeValue<'valueExt>

    override sta.ToString() =
      match sta with
      | Lookup(id, arg) -> $"{id}[{arg}]"
      | Application(f, a) -> $"({f})[{a}]"

  and SchemaPathTypeDecomposition<'valueExt> =
    | Field of ResolvedIdentifier
    | Item of TupleDesSelector
    | UnionCase of ResolvedIdentifier
    | SumCase of SumConsSelector
    | Iterator of
      {| Mapper: Expr<TypeValue<'valueExt>, ResolvedIdentifier, 'valueExt>
         Container: TypeValue<'valueExt>
         TypeDef: TypeValue<'valueExt> |}

    override sps.ToString() =
      match sps with
      | Field name -> $"Field({name})"
      | Item i -> $"Item({i})"
      | UnionCase name -> $"UnionCase({name})"
      | SumCase name -> $"SumCase({name})"
      | Iterator collection -> $"Iterator({collection.Mapper}::{collection.TypeDef})"

  and SchemaPathSegment<'valueExt> = Option<LocalIdentifier> * SchemaPathTypeDecomposition<'valueExt>
  and SchemaPath<'valueExt> = List<SchemaPathSegment<'valueExt>>

  and SchemaEntityProperty<'valueExt> =
    { PropertyName: LocalIdentifier
      Path: SchemaPath<'valueExt>
      ReturnType: TypeValue<'valueExt>
      ReturnKind: Kind
      Body: Expr<TypeValue<'valueExt>, ResolvedIdentifier, 'valueExt> }

  and SchemaEntity<'valueExt> =
    { Name: SchemaEntityName
      TypeOriginal: TypeValue<'valueExt>
      TypeWithProps: TypeValue<'valueExt>
      Id: TypeValue<'valueExt>
      SearchBy: List<SearchByLookup>
      Properties: List<SchemaEntityProperty<'valueExt>>
      OnCreating: Option<Expr<TypeValue<'valueExt>, ResolvedIdentifier, 'valueExt>>
      OnCreated: Option<Expr<TypeValue<'valueExt>, ResolvedIdentifier, 'valueExt>>
      OnUpdating: Option<Expr<TypeValue<'valueExt>, ResolvedIdentifier, 'valueExt>>
      OnUpdated: Option<Expr<TypeValue<'valueExt>, ResolvedIdentifier, 'valueExt>>
      OnDeleting: Option<Expr<TypeValue<'valueExt>, ResolvedIdentifier, 'valueExt>>
      OnDeleted: Option<Expr<TypeValue<'valueExt>, ResolvedIdentifier, 'valueExt>> }

    override entity.ToString() =
      $"{entity.Name.Name} (Id: {entity.Id}): {entity.TypeOriginal} -> {entity.TypeWithProps}"

  and SchemaRelation<'valueExt> =
    { Name: SchemaRelationName
      From: Identifier
      To: Identifier
      Cardinality: Option<SchemaRelationCardinality>
      OnLinking: Option<Expr<TypeValue<'valueExt>, ResolvedIdentifier, 'valueExt>>
      OnUnlinking: Option<Expr<TypeValue<'valueExt>, ResolvedIdentifier, 'valueExt>>
      OnLinked: Option<Expr<TypeValue<'valueExt>, ResolvedIdentifier, 'valueExt>>
      OnUnlinked: Option<Expr<TypeValue<'valueExt>, ResolvedIdentifier, 'valueExt>> }

    override r.ToString() =
      let cardStr =
        match r.Cardinality with
        | Some c -> $" ({c})"
        | None -> ""

      $"{r.Name.Name}: {r.From} -> {r.To}{cardStr}"

  and Schema<'valueExt> =
    { DeclaredAtForNominalEquality: Location
      Entities: OrderedMap<SchemaEntityName, SchemaEntity<'valueExt>>
      Relations: OrderedMap<SchemaRelationName, SchemaRelation<'valueExt>> }

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
    | Lambda of WithSourceMapping<TypeParameter * TypeExpr<'valueExt>, 'valueExt>
    | Application of WithSourceMapping<SymbolicTypeApplication<'valueExt>, 'valueExt>
    | Arrow of WithSourceMapping<TypeValue<'valueExt> * TypeValue<'valueExt>, 'valueExt>
    | Record of WithSourceMapping<OrderedMap<TypeSymbol, TypeValue<'valueExt> * Kind>, 'valueExt>
    | Tuple of WithSourceMapping<List<TypeValue<'valueExt>>, 'valueExt>
    | Union of WithSourceMapping<OrderedMap<TypeSymbol, TypeValue<'valueExt>>, 'valueExt>
    | Sum of WithSourceMapping<List<TypeValue<'valueExt>>, 'valueExt>
    | Set of WithSourceMapping<TypeValue<'valueExt>, 'valueExt>
    | Imported of ImportedTypeValue<'valueExt> // FIXME: This should also have an orig name, implement once the extension is implemented completely
    | Schema of Schema<'valueExt>
    | Entities of Schema<'valueExt>
    | Relations of Schema<'valueExt>
    | Entity of Schema<'valueExt> * e: TypeValue<'valueExt> * e': TypeValue<'valueExt> * e_id: TypeValue<'valueExt>
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

    override self.ToString() =
      match self with
      | Union({ typeExprSource = OriginExprTypeLet(id, _) })
      | Record({ typeExprSource = OriginExprTypeLet(id, _) }) -> id.ToString()
      | Record({ typeExprSource = OriginTypeExpr(TypeExpr.Lookup id) })
      | Primitive({ typeExprSource = OriginTypeExpr(TypeExpr.Lookup id) })
      | Lambda({ typeExprSource = OriginTypeExpr(TypeExpr.Lookup id) })
      | Arrow({ typeExprSource = OriginTypeExpr(TypeExpr.Lookup id) })
      | Tuple({ typeExprSource = OriginTypeExpr(TypeExpr.Lookup id) })
      | Union({ typeExprSource = OriginTypeExpr(TypeExpr.Lookup id) })
      | Sum({ typeExprSource = OriginTypeExpr(TypeExpr.Lookup id) }) -> id.ToString()
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
      | Sum({ value = types }) ->
        let comma = " + "
        $"({String.Join(comma, types)})"
      | Schema s -> $"Schema[{s.Entities.Count} Entities, {s.Relations.Count} Relations]"
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


  and ExprTypeLetBindingName =
    | ExprTypeLetBindingName of string

    override self.ToString() =
      let (ExprTypeLetBindingName s) = self in s

  and TypeExprSourceMapping<'valueExt> =
    | OriginExprTypeLet of ExprTypeLetBindingName * TypeExpr<'valueExt>
    | OriginTypeExpr of TypeExpr<'valueExt>
    | NoSourceMapping of string

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

      let args = String.Join(" ", self.Arguments |> List.map (fun a -> $"[{a}]"))
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

    override self.ToString() =
      match self with
      | Unit -> "()"
      | Guid -> "guid"
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

  and Var =
    { Name: string }

    static member Create name : Var = { Var.Name = name }

  and ExprLookup<'T, 'Id, 'valueExt when 'Id: comparison> =
    { Id: 'Id }

    override self.ToString() = self.Id.ToString()

  and ExprLambda<'T, 'Id, 'valueExt when 'Id: comparison> =
    { Param: Var
      ParamType: Option<'T>
      Body: Expr<'T, 'Id, 'valueExt> }

  and ExprApply<'T, 'Id, 'valueExt when 'Id: comparison> =
    { F: Expr<'T, 'Id, 'valueExt>
      Arg: Expr<'T, 'Id, 'valueExt> }

  and ExprLet<'T, 'Id, 'valueExt when 'Id: comparison> =
    { Var: Var
      Type: Option<'T>
      Val: Expr<'T, 'Id, 'valueExt>
      Rest: Expr<'T, 'Id, 'valueExt> }

  and ExprIf<'T, 'Id, 'valueExt when 'Id: comparison> =
    { Cond: Expr<'T, 'Id, 'valueExt>
      Then: Expr<'T, 'Id, 'valueExt>
      Else: Expr<'T, 'Id, 'valueExt> }

  and ExprTypeLambda<'T, 'Id, 'valueExt when 'Id: comparison> =
    { Param: TypeParameter
      Body: Expr<'T, 'Id, 'valueExt> }

  and ExprTypeApply<'T, 'Id, 'valueExt when 'Id: comparison> =
    { Func: Expr<'T, 'Id, 'valueExt>
      TypeArg: 'T }

  and ExprTypeLet<'T, 'Id, 'valueExt when 'Id: comparison> =
    { Name: string
      TypeDef: 'T
      Body: Expr<'T, 'Id, 'valueExt> }

  and ExprRecordCons<'T, 'Id, 'valueExt when 'Id: comparison> =
    { Fields: List<'Id * Expr<'T, 'Id, 'valueExt>> }

  and ExprRecordWith<'T, 'Id, 'valueExt when 'Id: comparison> =
    { Record: Expr<'T, 'Id, 'valueExt>
      Fields: List<'Id * Expr<'T, 'Id, 'valueExt>> }

  and ExprTupleCons<'T, 'Id, 'valueExt when 'Id: comparison> =
    { Items: List<Expr<'T, 'Id, 'valueExt>> }

  and ExprTupleDes<'T, 'Id, 'valueExt when 'Id: comparison> =
    { Tuple: Expr<'T, 'Id, 'valueExt>
      Item: TupleDesSelector }

  and ExprSumCons<'T, 'Id, 'valueExt when 'Id: comparison> = { Selector: SumConsSelector }

  and ExprRecordDes<'T, 'Id, 'valueExt when 'Id: comparison> =
    { Expr: Expr<'T, 'Id, 'valueExt>
      Field: 'Id }

  and ExprEntitiesDes<'T, 'Id, 'valueExt when 'Id: comparison> = { Expr: Expr<'T, 'Id, 'valueExt> }

  and ExprRelationsDes<'T, 'Id, 'valueExt when 'Id: comparison> = { Expr: Expr<'T, 'Id, 'valueExt> }

  and ExprLookupsDes<'T, 'Id, 'valueExt when 'Id: comparison> = { Expr: Expr<'T, 'Id, 'valueExt> }

  and ExprEntityDes<'T, 'Id, 'valueExt when 'Id: comparison> =
    { Expr: Expr<'T, 'Id, 'valueExt>
      EntityName: SchemaEntityName }

  and ExprRelationDes<'T, 'Id, 'valueExt when 'Id: comparison> =
    { Expr: Expr<'T, 'Id, 'valueExt>
      RelationName: SchemaRelationName }

  and RelationLookupDirection =
    | FromTo
    | ToFrom

  and ExprRelationLookupDes<'T, 'Id, 'valueExt when 'Id: comparison> =
    { Expr: Expr<'T, 'Id, 'valueExt>
      RelationName: SchemaRelationName
      Direction: RelationLookupDirection }

  and ExprUnionDes<'T, 'Id, 'valueExt when 'Id: comparison> =
    { Handlers: Map<'Id, CaseHandler<'T, 'Id, 'valueExt>>
      Fallback: Option<Expr<'T, 'Id, 'valueExt>> }

  and ExprSumDes<'T, 'Id, 'valueExt when 'Id: comparison> =
    { Handlers: Map<SumConsSelector, CaseHandler<'T, 'Id, 'valueExt>> }

  and ExprFromValue<'T, 'Id, 'valueExt when 'Id: comparison> =
    { Value: Value<TypeValue<'valueExt>, 'valueExt>
      ValueType: TypeValue<'valueExt>
      ValueKind: Kind }

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

    override self.ToString() : string =
      match self with
      | TypeLambda({ ExprTypeLambda.Param = tp
                     Body = body }) -> $"(Λ{tp.ToString()} => {body.ToString()})"
      | TypeApply({ Func = e; TypeArg = t }) -> $"{e.ToString()} [{t.ToString()}]"
      | Lambda({ Param = v
                 ParamType = topt
                 Body = body }) ->
        match topt with
        | Some t -> $"(fun ({v.Name}: {t.ToString()}) -> {body.ToString()})"
        | None -> $"(fun {v.Name} -> {body.ToString()})"
      | Apply({ F = e1; Arg = e2 }) -> $"({e1.ToString()} {e2.ToString()})"
      | FromValue({ Value = v
                    ValueType = t
                    ValueKind = k }) -> $"({v.ToString()} : {t.ToString()} :: {k.ToString()}])"
      | Let({ Var = v
              Type = topt
              Val = e1
              Rest = e2 }) ->
        match topt with
        | Some t -> $"(let {v.Name}: {t.ToString()} = {e1.ToString()} in {e2.ToString()})"
        | None -> $"(let {v.Name} = {e1.ToString()} in {e2.ToString()})"
      | TypeLet({ Name = name
                  TypeDef = t
                  Body = body }) -> $"(type {name} = {t.ToString()}; {body.ToString()})"
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
        let itemStr = items |> List.map (fun v -> v.ToString()) |> String.concat ", "
        $"({itemStr})"
      | SumCons({ Selector = selector }) -> $"{selector.Case}Of{selector.Count}"
      | RecordDes({ Expr = record; Field = field }) -> $"{record.ToString()}.{field.ToString()}"
      | EntitiesDes({ Expr = entities }) -> $"{entities.ToString()}.Entities"
      | RelationsDes({ Expr = relations }) -> $"{relations.ToString()}.Relations"
      | EntityDes({ Expr = entity
                    EntityName = entityName }) -> $"{entity.ToString()}.{entityName.ToString()}"
      | RelationDes({ Expr = relation
                      RelationName = relationName }) -> $"{relation.ToString()}.{relationName.ToString()}"
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
        let handlerStr =
          handlers
          |> Map.toList
          |> List.map (fun (k, (v, body)) -> $"{k.ToString()}({v.Name}) => {body.ToString()}")
          |> String.concat " | "

        match defaultOpt with
        | Some defaultExpr -> $"(match {handlerStr} | _ => {defaultExpr.ToString()})"
        | None -> $"(match {handlerStr})"
      | TupleDes({ ExprTupleDes.Tuple = tuple
                   Item = selector }) -> $"{tuple.ToString()}.{selector.Index}"
      | SumDes { Handlers = handlers } ->
        let handlerStr =
          handlers
          |> Map.toList
          |> List.map (fun (k, (v, body)) -> $"{k.Case}Of{k.Count} ({v.Name} => {body.ToString()})")
          |> String.concat " | "

        $"(match {handlerStr})"
      | Primitive p -> p.ToString()
      | Lookup id -> id.ToString()
      | If({ Cond = cond
             Then = thenExpr
             Else = elseExpr }) -> $"(if {cond.ToString()} then {thenExpr.ToString()} else {elseExpr.ToString()})"

  and Expr<'T, 'Id, 'valueExt when 'Id: comparison> =
    { Expr: ExprRec<'T, 'Id, 'valueExt>
      Location: Location
      Scope: TypeCheckScope }

    override self.ToString() : string = self.Expr.ToString()

  and CaseHandler<'T, 'Id, 'valueExt when 'Id: comparison> = Var * Expr<'T, 'Id, 'valueExt>

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

  and Value<'T, 'valueExt> =
    | TypeLambda of TypeParameter * Expr<'T, ResolvedIdentifier, 'valueExt>
    | Lambda of
      Var *
      Expr<'T, ResolvedIdentifier, 'valueExt> *
      Map<ResolvedIdentifier, Value<'T, 'valueExt>> *
      TypeCheckScope
    | Record of Map<ResolvedIdentifier, Value<'T, 'valueExt>>
    | UnionCase of ResolvedIdentifier * Value<'T, 'valueExt>
    | RecordDes of ResolvedIdentifier
    | UnionCons of ResolvedIdentifier
    | Tuple of List<Value<'T, 'valueExt>>
    | Sum of SumConsSelector * Value<'T, 'valueExt>
    | Primitive of PrimitiveValue
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
        let valueStr = values |> List.map (fun v -> v.ToString()) |> String.concat ", "
        $"({valueStr})"
      | Sum(selector, value) -> $"{selector.Case}Of{selector.Count}({value.ToString()})"
      | Primitive p -> p.ToString()
      | Var v -> v.Name
      | Ext(e, _) -> e.ToString()
