namespace Ballerina.DSL.Next.Types

module Patterns =
  open Ballerina.StdLib.Object
  open Ballerina.Collections.Sum
  open Ballerina.Reader.WithError
  open Ballerina.Errors
  open System
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.StdLib.OrderPreservingMap
  open Ballerina.Cat.Collections.OrderedMap

  type TypeVar with
    static member Create(name: string) : TypeVar =
      { Name = name
        Synthetic = false
        Guid = Guid.CreateVersion7() }

    static member CreateSynthetic(name: string) : TypeVar =
      let guid = Guid.CreateVersion7()

      { Name = $"{name}_{guid}"
        Synthetic = true
        Guid = guid }

  type TypeSymbol with
    static member Create(name: Identifier) : TypeSymbol =
      { Name = name
        Guid = Guid.CreateVersion7() }

  type TypeVarIdentifier with
    static member Create(name: string) : TypeVarIdentifier = { Name = name }

  type TypeParameter with
    static member Create(name: string, kind: Kind) : TypeParameter = { Name = name; Kind = kind }

  type Identifier with
    member id.LocalName =
      match id with
      | LocalScope name -> name
      | FullyQualified(_, name) -> name

  type Kind with
    static member AsStar(kind: Kind) =
      match kind with
      | Kind.Star -> sum.Return()
      | _ -> sum.Throw(Errors.Singleton $"Expected star kind, got {kind}")

    static member AsArrow(kind: Kind) =
      match kind with
      | Kind.Arrow(input, output) -> sum.Return(input, output)
      | _ -> sum.Throw(Errors.Singleton $"Expected arrow kind, got {kind}")

    static member AsSymbol(kind: Kind) =
      match kind with
      | Kind.Symbol -> sum.Return()
      | _ -> sum.Throw(Errors.Singleton $"Expected symbol kind, got {kind}")

  type WithSourceMapping<'v> with
    static member Getters = {| Value = fun (v: WithSourceMapping<'v>) -> v.value |}

    static member Setters =
      {| Value = fun (v: WithSourceMapping<'v>, value: 'v) -> { v with value = value }
         Source = fun (v: WithSourceMapping<'v>, source: TypeExprSourceMapping) -> { v with typeExprSource = source } |}

  type TypeValue with

    static member CreateUnit() : TypeValue =
      TypeValue.Primitive
        { value = PrimitiveType.Unit
          typeExprSource = NoSourceMapping "Unit"
          typeCheckScopeSource = TypeCheckScope.Empty }

    static member CreateGuid() : TypeValue =
      TypeValue.Primitive
        { value = PrimitiveType.Guid
          typeExprSource = NoSourceMapping "Guid"
          typeCheckScopeSource = TypeCheckScope.Empty }

    static member CreateInt32() : TypeValue =
      TypeValue.Primitive
        { value = PrimitiveType.Int32
          typeExprSource = NoSourceMapping "Int32"
          typeCheckScopeSource = TypeCheckScope.Empty }

    static member CreateInt64() : TypeValue =
      TypeValue.Primitive
        { value = PrimitiveType.Int64
          typeExprSource = NoSourceMapping "Int64"
          typeCheckScopeSource = TypeCheckScope.Empty }

    static member CreateFloat32() : TypeValue =
      TypeValue.Primitive
        { value = PrimitiveType.Float32
          typeExprSource = NoSourceMapping "Float32"
          typeCheckScopeSource = TypeCheckScope.Empty }

    static member CreateFloat64() : TypeValue =
      TypeValue.Primitive
        { value = PrimitiveType.Float64
          typeExprSource = NoSourceMapping "Float64"
          typeCheckScopeSource = TypeCheckScope.Empty }

    static member CreateDecimal() : TypeValue =
      TypeValue.Primitive
        { value = PrimitiveType.Decimal
          typeExprSource = NoSourceMapping "Decimal"
          typeCheckScopeSource = TypeCheckScope.Empty }

    static member CreateBool() : TypeValue =
      TypeValue.Primitive
        { value = PrimitiveType.Bool
          typeExprSource = NoSourceMapping "Bool"
          typeCheckScopeSource = TypeCheckScope.Empty }

    static member CreateString() : TypeValue =
      TypeValue.Primitive
        { value = PrimitiveType.String
          typeExprSource = NoSourceMapping "String"
          typeCheckScopeSource = TypeCheckScope.Empty }

    static member CreateDateTime() : TypeValue =
      TypeValue.Primitive
        { value = PrimitiveType.DateTime
          typeExprSource = NoSourceMapping "DateTime"
          typeCheckScopeSource = TypeCheckScope.Empty }

    static member CreateDateOnly() : TypeValue =
      TypeValue.Primitive
        { value = PrimitiveType.DateOnly
          typeExprSource = NoSourceMapping "DateOnly"
          typeCheckScopeSource = TypeCheckScope.Empty }

    static member CreateTimeSpan() : TypeValue =
      TypeValue.Primitive
        { value = PrimitiveType.TimeSpan
          typeExprSource = NoSourceMapping "TimeSpan"
          typeCheckScopeSource = TypeCheckScope.Empty }

    static member CreatePrimitive(v: PrimitiveType) : TypeValue =
      match v with
      | PrimitiveType.Unit -> TypeValue.CreateUnit()
      | PrimitiveType.Guid -> TypeValue.CreateGuid()
      | PrimitiveType.Int32 -> TypeValue.CreateInt32()
      | PrimitiveType.Int64 -> TypeValue.CreateInt64()
      | PrimitiveType.Float32 -> TypeValue.CreateFloat32()
      | PrimitiveType.Float64 -> TypeValue.CreateFloat64()
      | PrimitiveType.Decimal -> TypeValue.CreateDecimal()
      | PrimitiveType.Bool -> TypeValue.CreateBool()
      | PrimitiveType.String -> TypeValue.CreateString()
      | PrimitiveType.DateTime -> TypeValue.CreateDateTime()
      | PrimitiveType.DateOnly -> TypeValue.CreateDateOnly()
      | PrimitiveType.TimeSpan -> TypeValue.CreateTimeSpan()

    static member CreateLambda(v: TypeParameter, t: TypeExpr) : TypeValue =
      TypeValue.Lambda
        { value = v, t
          typeExprSource = NoSourceMapping "Lambda"
          typeCheckScopeSource = TypeCheckScope.Empty }

    static member CreateArrow(v: TypeValue * TypeValue) : TypeValue =
      TypeValue.Arrow
        { value = v
          typeExprSource = NoSourceMapping "Arrow"
          typeCheckScopeSource = TypeCheckScope.Empty }

    static member CreateRecord(v: OrderedMap<TypeSymbol, TypeValue * Kind>) : TypeValue =
      TypeValue.Record
        { value = v
          typeExprSource = NoSourceMapping "Record"
          typeCheckScopeSource = TypeCheckScope.Empty }

    static member CreateTuple(v: List<TypeValue>) : TypeValue =
      TypeValue.Tuple
        { value = v
          typeExprSource = NoSourceMapping "Tuple"
          typeCheckScopeSource = TypeCheckScope.Empty }

    static member CreateUnion(v: OrderedMap<TypeSymbol, TypeValue>) : TypeValue =
      TypeValue.Union
        { value = v
          typeExprSource = NoSourceMapping "Union"
          typeCheckScopeSource = TypeCheckScope.Empty }

    static member CreateSum(v: List<TypeValue>) : TypeValue =
      TypeValue.Sum
        { value = v
          typeExprSource = NoSourceMapping "Sum"
          typeCheckScopeSource = TypeCheckScope.Empty }

    static member CreateSet(v: TypeValue) : TypeValue =
      TypeValue.Set
        { value = v
          typeExprSource = NoSourceMapping "Set"
          typeCheckScopeSource = TypeCheckScope.Empty }

    static member CreateMap(v: TypeValue * TypeValue) : TypeValue =
      TypeValue.Map
        { value = v
          typeExprSource = NoSourceMapping "Map"
          typeCheckScopeSource = TypeCheckScope.Empty }

    static member CreateVar(v: TypeVar) : TypeValue = TypeValue.Var v

    static member CreateApplication(v: SymbolicTypeApplication) : TypeValue =
      TypeValue.Application
        { typeExprSource = NoSourceMapping "Application"
          value = v
          typeCheckScopeSource = TypeCheckScope.Empty }

    static member CreateImported(v: ImportedTypeValue) : TypeValue = TypeValue.Imported v

    static member AsLambda(t: TypeValue) =
      sum {
        match t with
        | TypeValue.Lambda v -> return v
        | _ ->
          return!
            $"Error: expected type lambda (ie generic), got {t}"
            |> Errors.Singleton
            |> sum.Throw
      }

    static member AsUnion(t: TypeValue) =
      sum {
        match t with
        | TypeValue.Union { value = cases } -> return ([], cases)
        | TypeValue.Lambda { value = type_par, TypeExpr.FromTypeValue body } ->
          let! type_pars, cases = TypeValue.AsUnion body
          return type_par :: type_pars, cases
        | _ -> return! $"Error: expected union type, got {t}" |> Errors.Singleton |> sum.Throw
      }

    static member AsRecord(t: TypeValue) =
      sum {
        match t with
        | TypeValue.Record(fields) -> return fields.value
        | _ -> return! $"Error: expected record type, got {t}" |> Errors.Singleton |> sum.Throw
      }

    static member AsTuple(t: TypeValue) =
      sum {
        match t with
        | TypeValue.Tuple(fields) -> return fields
        | _ ->
          return!
            $"Error: expected tuple type (ie generic), got {t}"
            |> Errors.Singleton
            |> sum.Throw
      }

    static member AsSum(t: TypeValue) =
      sum {
        match t with
        | TypeValue.Sum(variants) -> return variants
        | _ -> return! $"Error: expected sum type, got {t}" |> Errors.Singleton |> sum.Throw
      }

    static member AsArrow(t: TypeValue) =
      sum {
        match t with
        | TypeValue.Arrow v -> return v
        | _ -> return! $"Error: expected arrow type, got {t}" |> Errors.Singleton |> sum.Throw
      }

    static member AsImported(t: TypeValue) =
      sum {
        match t with
        | TypeValue.Imported i -> return i
        | _ -> return! $"Error: expected imported type, got {t}" |> Errors.Singleton |> sum.Throw
      }

    static member AsImportedUnionLike(t: TypeValue) =
      sum {
        match t with
        | TypeValue.Imported { Sym = _
                               UnionLike = Some u
                               RecordLike = _ } -> return u
        | _ -> return! $"Error: expected imported type, got {t}" |> Errors.Singleton |> sum.Throw
      }

    static member AsMap(t: TypeValue) =
      sum {
        match t with
        | TypeValue.Map v -> return v
        | _ ->
          return!
            $"Error: expected map type (ie generic), got {t}"
            |> Errors.Singleton
            |> sum.Throw
      }

    static member AsSet(t: TypeValue) =
      sum {
        match t with
        | TypeValue.Set(element) -> return element
        | _ ->
          return!
            $"Error: expected set type (ie generic), got {t}"
            |> Errors.Singleton
            |> sum.Throw
      }

    static member AsLookup(t: TypeValue) =
      sum {
        match t with
        | TypeValue.Lookup id -> return id
        | _ -> return! $"Error: expected type lookup, got {t}" |> Errors.Singleton |> sum.Throw
      }

    static member AsVar(t: TypeValue) =
      sum {
        match t with
        | TypeValue.Var id -> return id
        | _ -> return! $"Error: expected type variable, got {t}" |> Errors.Singleton |> sum.Throw
      }

    static member AsPrimitive(t: TypeValue) =
      sum {
        match t with
        | TypeValue.Primitive p -> return p
        | _ -> return! $"Error: expected primitive type, got {t}" |> Errors.Singleton |> sum.Throw
      }

    static member AsApplication(t: TypeValue) =
      sum {
        match t with
        | TypeValue.Application v -> return v
        | _ -> return! $"Error: expected application type, got {t}" |> Errors.Singleton |> sum.Throw
      }

    static member DropSourceMapping(t: TypeValue) =
      match t with
      | TypeValue.Var id -> TypeValue.Var id
      | TypeValue.Lookup id -> TypeValue.Lookup id
      | TypeValue.Imported v -> TypeValue.Imported v
      | TypeValue.Primitive p -> TypeValue.CreatePrimitive p.value
      | TypeValue.Application v -> TypeValue.CreateApplication v.value
      | TypeValue.Lambda { value = (v, t) } -> TypeValue.CreateLambda(v, t)
      | TypeValue.Arrow v -> TypeValue.CreateArrow v.value
      | TypeValue.Record v -> TypeValue.CreateRecord v.value
      | TypeValue.Tuple v -> TypeValue.CreateTuple v.value
      | TypeValue.Union v -> TypeValue.CreateUnion v.value
      | TypeValue.Sum v -> TypeValue.CreateSum v.value
      | TypeValue.Set v -> TypeValue.CreateSet v.value
      | TypeValue.Map v -> TypeValue.CreateMap v.value

    static member SetSourceMapping(t: TypeValue, source: TypeExprSourceMapping) =
      match t with
      | TypeValue.Var _ -> t
      | TypeValue.Lookup _ -> t
      | TypeValue.Imported _ -> t
      | TypeValue.Primitive(p: WithSourceMapping<PrimitiveType>) ->
        WithSourceMapping.Setters.Source(p, source) |> TypeValue.Primitive
      | TypeValue.Application v -> WithSourceMapping.Setters.Source(v, source) |> TypeValue.Application
      | TypeValue.Lambda v -> WithSourceMapping.Setters.Source(v, source) |> TypeValue.Lambda
      | TypeValue.Arrow v -> WithSourceMapping.Setters.Source(v, source) |> TypeValue.Arrow
      | TypeValue.Record v -> WithSourceMapping.Setters.Source(v, source) |> TypeValue.Record
      | TypeValue.Tuple v -> WithSourceMapping.Setters.Source(v, source) |> TypeValue.Tuple
      | TypeValue.Union v -> WithSourceMapping.Setters.Source(v, source) |> TypeValue.Union
      | TypeValue.Sum v -> WithSourceMapping.Setters.Source(v, source) |> TypeValue.Sum
      | TypeValue.Set v -> WithSourceMapping.Setters.Source(v, source) |> TypeValue.Set
      | TypeValue.Map v -> WithSourceMapping.Setters.Source(v, source) |> TypeValue.Map

  type TypeValue with
    member t.AsExpr: TypeExpr = TypeExpr.FromTypeValue t

  type TypeExpr with
    static member AsLookup(t: TypeExpr) =
      sum {
        match t with
        | TypeExpr.Lookup id -> return id
        | _ -> return! $"Error: expected type lookup, got {t}" |> Errors.Singleton |> sum.Throw
      }

    static member AsValue
      (loc0: Ballerina.LocalizedErrors.Location)
      (tryFind: Identifier -> Sum<TypeValue, Ballerina.LocalizedErrors.Errors>)
      (tryFindSymbol: Identifier -> Sum<TypeSymbol, Ballerina.LocalizedErrors.Errors>)
      (t: TypeExpr)
      : Sum<TypeValue, Ballerina.LocalizedErrors.Errors> =
      let (!) = TypeExpr.AsValue loc0 tryFind tryFindSymbol

      sum {
        match t with
        | TypeExpr.Primitive p -> return TypeValue.CreatePrimitive p
        | TypeExpr.Lookup v -> return! tryFind v
        | TypeExpr.Lambda(param, body) -> return TypeValue.CreateLambda(param, body)
        | TypeExpr.Arrow(input, output) ->
          let! input = !input
          let! output = !output
          return TypeValue.CreateArrow(input, output)

        | TypeExpr.Record(fields) ->
          let! fields =
            fields
            |> Seq.map (fun (k, v) ->
              sum {
                let! k =
                  TypeExpr.AsLookup k
                  |> Sum.mapRight (Ballerina.LocalizedErrors.Errors.FromErrors loc0)

                let! k = tryFindSymbol k
                let! v = !v
                return k, v
              })
            |> sum.All
            |> sum.Map OrderedMap.ofSeq

          let fields = fields |> OrderedMap.map (fun _ v -> v, Kind.Star)
          return TypeValue.CreateRecord fields
        | TypeExpr.Tuple(fields) ->
          let! items = fields |> List.map (!) |> sum.All
          return TypeValue.CreateTuple items
        | TypeExpr.Union(cases) ->
          let! cases =
            cases
            |> Seq.map (fun (k, v) ->
              sum {
                let! k =
                  TypeExpr.AsLookup k
                  |> Sum.mapRight (Ballerina.LocalizedErrors.Errors.FromErrors loc0)

                let! k = tryFindSymbol k
                let! v = !v
                return (k, v)
              })
            |> sum.All
            |> sum.Map(OrderedMap.ofSeq)

          return TypeValue.CreateUnion cases
        | TypeExpr.Sum(fields) ->
          let! variants = fields |> List.map (!) |> sum.All
          return TypeValue.CreateSum variants
        | TypeExpr.Map(key, value) ->
          let! key = !key
          let! value = !value
          return TypeValue.CreateMap(key, value)
        | TypeExpr.Set(element) ->
          let! element = !element
          return TypeValue.CreateSet element
        | TypeExpr.FromTypeValue tv -> return tv
        | _ ->
          return!
            (loc0, $"Error: expected type value, got {t}")
            |> Ballerina.LocalizedErrors.Errors.Singleton
            |> sum.Throw
      }

    static member AsUnion(t: TypeExpr) =
      sum {
        match t with
        | TypeExpr.Union(cases) -> return cases
        | _ -> return! $"Error: expected union type, got {t}" |> Errors.Singleton |> sum.Throw
      }


    static member AsTuple(t: TypeExpr) =
      sum {
        match t with
        | TypeExpr.Tuple(fields) -> return fields
        | _ -> return! $"Error: expected tuple type, got {t}" |> Errors.Singleton |> sum.Throw
      }

    static member AsPrimitive(t: TypeExpr) =
      sum {
        match t with
        | TypeExpr.Primitive p -> return p
        | _ -> return! $"Error: expected primitive type, got {t}" |> Errors.Singleton |> sum.Throw
      }

    static member AsKeyOf(t: TypeExpr) =
      sum {
        match t with
        | TypeExpr.KeyOf id -> return id
        | _ -> return! $"Error: expected key of type, got {t}" |> Errors.Singleton |> sum.Throw
      }

    static member AsRecord(t: TypeExpr) =
      sum {
        match t with
        | TypeExpr.Record(fields) -> return fields
        | _ -> return! $"Error: expected record type, got {t}" |> Errors.Singleton |> sum.Throw
      }

    static member AsArrow(t: TypeExpr) =
      sum {
        match t with
        | TypeExpr.Arrow(input, output) -> return (input, output)
        | _ -> return! $"Error: expected arrow type, got {t}" |> Errors.Singleton |> sum.Throw
      }

    static member AsMap(t: TypeExpr) =
      sum {
        match t with
        | TypeExpr.Map(key, value) -> return (key, value)
        | _ -> return! $"Error: expected map type, got {t}" |> Errors.Singleton |> sum.Throw
      }

    static member AsLambda(t: TypeExpr) =
      sum {
        match t with
        | TypeExpr.Lambda(param, body) -> return (param, body)
        | _ -> return! $"Error: expected type lambda, got {t}" |> Errors.Singleton |> sum.Throw
      }

    static member AsSet(t: TypeExpr) =
      sum {
        match t with
        | TypeExpr.Set(element) -> return element
        | _ -> return! $"Error: expected set type, got {t}" |> Errors.Singleton |> sum.Throw
      }

    static member AsExclude(t: TypeExpr) =
      sum {
        match t with
        | TypeExpr.Exclude(a, b) -> return (a, b)
        | _ -> return! $"Error: expected type exclude, got {t}" |> Errors.Singleton |> sum.Throw
      }

    static member AsApply(t: TypeExpr) =
      sum {
        match t with
        | TypeExpr.Apply(id, args) -> return (id, args)
        | _ -> return! $"Error: expected type application, got {t}" |> Errors.Singleton |> sum.Throw
      }

    static member AsFlatten(t: TypeExpr) =
      sum {
        match t with
        | TypeExpr.Flatten(id, args) -> return (id, args)
        | _ -> return! $"Error: expected type flatten, got {t}" |> Errors.Singleton |> sum.Throw
      }

    static member AsRotate(t: TypeExpr) =
      sum {
        match t with
        | TypeExpr.Rotate t -> return t
        | _ -> return! $"Error: expected type rotate, got {t}" |> Errors.Singleton |> sum.Throw
      }

    static member AsSum(t: TypeExpr) =
      sum {
        match t with
        | TypeExpr.Sum(variants) -> return variants
        | _ -> return! $"Error: expected sum type, got {t}" |> Errors.Singleton |> sum.Throw
      }

  type Identifier with
    static member AsLocalScope(i: Identifier) =
      sum {
        match i with
        | Identifier.LocalScope s -> s
        | FullyQualified _ -> return! $"Error: expected local scope, got {i}" |> Errors.Singleton |> sum.Throw
      }

    static member AsFullyQualified(i: Identifier) =
      sum {
        match i with
        | Identifier.FullyQualified(s, x) -> s, x
        | Identifier.LocalScope _ ->
          return!
            $"Error: expected fully qualified identifier, got {i}"
            |> Errors.Singleton
            |> sum.Throw
      }

  type PrimitiveType with
    static member AsUnit(p: PrimitiveType) =
      match p with
      | PrimitiveType.Unit -> sum.Return()
      | _ -> sum.Throw(Errors.Singleton $"Expected Unit primitive type, got {p}")

    static member AsGuid(p: PrimitiveType) =
      match p with
      | PrimitiveType.Guid -> sum.Return()
      | _ -> sum.Throw(Errors.Singleton $"Expected Guid primitive type, got {p}")

    static member AsInt32(p: PrimitiveType) =
      match p with
      | PrimitiveType.Int32 -> sum.Return()
      | _ -> sum.Throw(Errors.Singleton $"Expected Int32 primitive type, got {p}")

    static member AsInt64(p: PrimitiveType) =
      match p with
      | PrimitiveType.Int64 -> sum.Return()
      | _ -> sum.Throw(Errors.Singleton $"Expected Int64 primitive type, got {p}")

    static member AsFloat32(p: PrimitiveType) =
      match p with
      | PrimitiveType.Float32 -> sum.Return()
      | _ -> sum.Throw(Errors.Singleton $"Expected Float32 primitive type, got {p}")

    static member AsFloat64(p: PrimitiveType) =
      match p with
      | PrimitiveType.Float64 -> sum.Return()
      | _ -> sum.Throw(Errors.Singleton $"Expected Float64 primitive type, got {p}")

    static member AsDecimal(p: PrimitiveType) =
      match p with
      | PrimitiveType.Decimal -> sum.Return()
      | _ -> sum.Throw(Errors.Singleton $"Expected Decimal primitive type, got {p}")

    static member AsBool(p: PrimitiveType) =
      match p with
      | PrimitiveType.Bool -> sum.Return()
      | _ -> sum.Throw(Errors.Singleton $"Expected Bool primitive type, got {p}")

    static member AsString(p: PrimitiveType) =
      match p with
      | PrimitiveType.String -> sum.Return()
      | _ -> sum.Throw(Errors.Singleton $"Expected String primitive type, got {p}")

    static member AsDateTime(p: PrimitiveType) =
      match p with
      | PrimitiveType.DateTime -> sum.Return()
      | _ -> sum.Throw(Errors.Singleton $"Expected DateTime primitive type, got {p}")

    static member AsDateOnly(p: PrimitiveType) =
      match p with
      | PrimitiveType.DateOnly -> sum.Return()
      | _ -> sum.Throw(Errors.Singleton $"Expected DateOnly primitive type, got {p}")
