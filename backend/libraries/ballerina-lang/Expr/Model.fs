namespace Ballerina.DSL.Expr

module Model =
  open System

  let sumCaseNames =
    {| Left = "Sum.Left"
       Right = "Sum.Right" |}

  let optionCaseNames = {| Some = "some"; None = "none" |}

  type Vars<'ExprExtension, 'ValueExtension> = Map<VarName, Value<'ExprExtension, 'ValueExtension>>

  and TranslationOverride = { Label: string; KeyType: ExprType }

  and VarName =
    { VarName: string }

    static member Create s = { VarName = s }

  and Value<'ExprExtension, 'ValueExtension> =
    | Unit
    | Var of VarName
    | CaseCons of string * Value<'ExprExtension, 'ValueExtension>
    | Tuple of List<Value<'ExprExtension, 'ValueExtension>>
    | Record of Map<string, Value<'ExprExtension, 'ValueExtension>>
    | Lambda of VarName * option<ExprType> * option<ExprType> * Expr<'ExprExtension, 'ValueExtension>
    | GenericLambda of ExprTypeId * Expr<'ExprExtension, 'ValueExtension>
    | Extension of 'ValueExtension

    override v.ToString() =
      match v with
      | Value.Unit -> "()"
      | Value.CaseCons(c, v) -> $"{c}({v})"
      | Value.Lambda(v, t, returnType, b) ->
        let t =
          t
          |> Option.map (fun exprType -> $":{exprType.ToString()}")
          |> Option.defaultValue ""

        let returnType =
          returnType
          |> Option.map (fun exprType -> $":{exprType.ToString()}")
          |> Option.defaultValue ""

        $"fun ({v.VarName}{t}){returnType} -> {b.ToString()}"
      | Value.GenericLambda(v, b) -> $"FUN {v.VarName} => {b.ToString()}"
      | Value.Record fs ->
        let formattedFields =
          fs
          |> Map.toSeq
          |> Seq.sortBy (fun (fieldName, _) -> fieldName)
          |> Seq.map (fun (fieldName, expr) -> $"{fieldName} = {expr.ToString()}")

        $"""{{ {String.Join("; ", formattedFields)} }}"""
      | Value.Tuple vs ->
        let formattedValues = vs |> Seq.map (fun v -> v.ToString())
        $"""({String.Join(", ", formattedValues)})"""
      | Value.Var(v) -> v.VarName
      | Value.Extension ext -> ext.ToString()

  and Expr<'ExprExtension, 'ValueExtension> =
    | Value of Value<'ExprExtension, 'ValueExtension>
    | Apply of Expr<'ExprExtension, 'ValueExtension> * Expr<'ExprExtension, 'ValueExtension>
    | VarLookup of VarName
    | MakeRecord of Map<string, Expr<'ExprExtension, 'ValueExtension>>
    | RecordFieldLookup of Expr<'ExprExtension, 'ValueExtension> * string
    | MakeTuple of List<Expr<'ExprExtension, 'ValueExtension>>
    | MakeSet of List<Expr<'ExprExtension, 'ValueExtension>>
    | Project of Expr<'ExprExtension, 'ValueExtension> * int
    | MakeCase of string * Expr<'ExprExtension, 'ValueExtension>
    | MatchCase of Expr<'ExprExtension, 'ValueExtension> * Map<string, VarName * Expr<'ExprExtension, 'ValueExtension>>
    | Let of VarName * Expr<'ExprExtension, 'ValueExtension> * Expr<'ExprExtension, 'ValueExtension>
    | LetType of ExprTypeId * ExprType * Expr<'ExprExtension, 'ValueExtension>
    | GenericApply of Expr<'ExprExtension, 'ValueExtension> * ExprType
    | Annotate of Expr<'ExprExtension, 'ValueExtension> * ExprType
    | Prepend of List<string> * Expr<'ExprExtension, 'ValueExtension>
    | Extension of 'ExprExtension

    override e.ToString() =
      match e with
      | VarLookup v -> v.VarName
      | Value v -> v.ToString()
      | Apply(f, a) -> $"({f.ToString()})({a.ToString()})"
      | MakeRecord fs ->
        let formattedFields =
          fs
          |> Map.toSeq
          |> Seq.sortBy (fun (fieldName, _) -> fieldName)
          |> Seq.map (fun (fieldName, expr) -> $"{fieldName} = {expr.ToString()}")

        $"""{{ {String.Join("; ", formattedFields)} }}"""
      | MakeTuple fs -> $"""({String.Join(", ", fs |> Seq.map (fun f -> f.ToString()))})"""
      | MakeSet fs -> $"""{{{String.Join("; ", fs |> Seq.map (fun f -> f.ToString()))}}}"""
      | RecordFieldLookup(e, f) -> $"{e.ToString()}.{f}"
      | MakeCase(c, e) -> $"{c.ToString()}({e.ToString()})"
      | Project(e, f) -> $"{e.ToString()}.π{f}"
      | MatchCase(e, cases) ->
        let cases =
          cases
          |> Map.toSeq
          |> Seq.sortBy (fun (caseName, _) -> caseName)
          |> Seq.map (fun (caseName, (varName, expr)) -> $"| {caseName}({varName.VarName}) -> {expr.ToString()}")

        let casesJoined = String.Join(' ', cases)
        $"match {e.ToString()} with {casesJoined}"
      | Let(varName, expr, rest) -> $"let {varName} = {expr} in {rest}"
      | LetType(typeName, expr, rest) -> $"type {typeName} = {expr} in {rest}"
      | GenericApply(e, t) -> $"{e}[{t}]"
      | Annotate(e, t) -> $"{e} : {t}"
      | Prepend(elements, e) -> $"prepend {e} with {elements}"
      | Extension ext -> ext.ToString()

  and PrimitiveType =
    | DateOnlyType
    | DateTimeType
    | IntType
    | FloatType
    | StringType
    | EntityIdStringType
    | EntityIdUUIDType
    | CalculatedDisplayValueType
    | BoolType
    | GuidType

    override t.ToString() : string =
      match t with
      | PrimitiveType.BoolType -> "Bool"
      | PrimitiveType.DateOnlyType -> "DateOnly"
      | PrimitiveType.DateTimeType -> "DateTime"
      | PrimitiveType.FloatType -> "Float"
      | PrimitiveType.GuidType -> "Guid"
      | PrimitiveType.IntType -> "Int"
      | PrimitiveType.StringType -> "String"
      | PrimitiveType.EntityIdStringType -> "EntityIdString"
      | PrimitiveType.EntityIdUUIDType -> "EntityIdUUID"
      | PrimitiveType.CalculatedDisplayValueType -> "CalculatedDisplayValue"

  and ExprTypeId = VarName

  and ExprTypeKind =
    | Star
    | Arrow of ExprTypeKind * ExprTypeKind

    override k.ToString() : string =
      let (!) (t: ExprTypeKind) = t.ToString()

      match k with
      | Star -> $"*"
      | Arrow(input, output) -> $"({!input}) => ({!output})"

  and ExprType =
    | UnitType
    | CustomType of string
    | VarType of VarName
    | LookupType of ExprTypeId
    | KeyOf of ExprType * List<string>
    | PrimitiveType of PrimitiveType
    | RecordType of Map<string, ExprType>
    | UnionType of Map<CaseName, UnionCase>
    | MapType of ExprType * ExprType
    | SumType of ExprType * ExprType
    | TupleType of List<ExprType>
    | OptionType of ExprType
    | OneType of ExprType
    | ReadOnlyType of ExprType
    | ManyType of ExprType
    | ListType of ExprType
    | TableType of ExprType
    | SetType of ExprType
    | ArrowType of ExprType * ExprType
    | GenericType of ExprTypeId * ExprTypeKind * ExprType
    | GenericApplicationType of ExprType * ExprType
    | TranslationOverride of TranslationOverride

    override t.ToString() : string =
      let (!) (t: ExprType) = t.ToString()

      match t with
      | ExprType.CustomType l -> l
      | ExprType.LookupType l -> l.VarName
      | ExprType.KeyOf(t, excludedKeys) ->
        let excludedKeysRepr =
          excludedKeys
          |> List.map (fun k -> $"'{k}'")
          |> fun keys -> String.Join(", ", keys)

        $"KeyOf<{!t}, {excludedKeysRepr}>"
      | ExprType.PrimitiveType p -> p.ToString()
      | ExprType.UnitType -> "()"
      | ExprType.VarType v -> v.VarName
      | ExprType.ListType t -> $"List<{!t}>"
      | ExprType.TableType t -> $"Table<{!t}>"
      | ExprType.SetType t -> $"Set<{!t}>"
      | ExprType.OptionType t -> $"Option<{!t}>"
      | ExprType.OneType t -> $"One<{!t}>"
      | ExprType.ReadOnlyType t -> $"ReadOnly<{!t}>"
      | ExprType.ManyType t -> $"Many<{!t}>"
      | ExprType.MapType(k, v) -> $"Map<{!k},{!v}>"
      | ExprType.SumType(l, r) -> $"Sum<{!l},{!r}>"
      | ExprType.TupleType ts -> $"({ts |> List.map (!) |> (fun s -> String.Join(',', s))})"
      | ExprType.UnionType cs ->
        let cs = cs |> Map.values |> List.ofSeq
        let printCase (c: UnionCase) = $"{c.CaseName} of {!c.Fields}"
        $"({cs |> List.map printCase |> (fun s -> String.Join('|', s))})"
      | ExprType.RecordType fs ->
        let printField (fieldName: string, fieldType: ExprType) = $"{fieldName}:{!fieldType}"

        $"{{ {fs
              |> Seq.map ((fun kv -> kv.Key, kv.Value) >> printField)
              |> fun s -> String.Join(';', s)} }}"
      | ExprType.ArrowType(l, r) -> $"({!l}) -> {!r}"
      | GenericType(typeName, kind, exprType) -> $"{typeName} :: {kind} => {!exprType}"
      | GenericApplicationType(f, a) -> $"{!f} {!a}"
      | TranslationOverride label -> $"TranslationOverride \"{label}\" "

  and UnionCase = { CaseName: string; Fields: ExprType }
  and CaseName = { CaseName: string }
  and VarTypes = Map<VarName, ExprType>

  type TranslationOverride with
    static member Type({ KeyType = keyType }: TranslationOverride) : ExprType =
      ExprType.MapType(ExprType.OptionType keyType, ExprType.PrimitiveType PrimitiveType.StringType)


// type Value<'ExprExtension, 'ValueExtension> with
//   member self.toObject =
//     match self with
//     | Value.ConstInt v -> Some(v :> obj)
//     | Value.ConstBool v -> Some(v :> obj)
//     | Value.ConstFloat v -> Some(v :> obj)
//     | Value.ConstGuid v -> Some(v :> obj)
//     | Value.ConstString v -> Some(v :> obj)
//     | _ -> None
