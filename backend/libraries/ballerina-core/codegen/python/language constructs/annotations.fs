namespace Ballerina.DSL.Codegen.Python.LanguageConstructs

module TypeAnnotations =
  open Ballerina.DSL.Expr.Types.Model
  open Ballerina.State.WithError
  open Ballerina.Errors
  open Ballerina.Collections.Sum
  open Ballerina.DSL.Codegen.Python.LanguageConstructs.Model

  type ExprType with
    static member GenerateTypeAnnotation(t: ExprType) : State<string, PythonCodeGenConfig, PythonCodeGenState, Errors> =
      let (!) = ExprType.GenerateTypeAnnotation

      let error =
        sum.Throw(
          sprintf "Error: cannot generate type annotation for type %A" t
          |> Errors.Singleton
        )
        |> state.OfSum

      let registerImport =
        Option.toList
        >> Set.ofList
        >> Set.union
        >> PythonCodeGenState.Updaters.UsedImports
        >> state.SetState

      let registerImportAndReturn (t: CodegenConfigTypeDef) =
        state {
          do! registerImport t.RequiredImport
          return t.GeneratedTypeName
        }

      let formatKind (kindName: string) (items: string list) =
        match items with
        | [] -> $"{kindName}"
        | _ -> $$$"""{{{kindName}}}[{{{System.String.Join(", ", items)}}}]"""

      state {
        let! config = state.GetContext()

        match t with
        | ExprType.UnitType -> return config.Unit.GeneratedTypeName
        | ExprType.LookupType t -> return t.TypeName
        | ExprType.PrimitiveType p ->
          match p with
          | PrimitiveType.BoolType -> return! config.Bool |> registerImportAndReturn
          | PrimitiveType.DateOnlyType -> return! config.Date |> registerImportAndReturn
          | PrimitiveType.DateTimeType -> return! config.DateTime |> registerImportAndReturn
          | PrimitiveType.FloatType -> return! config.Float |> registerImportAndReturn
          | PrimitiveType.GuidType -> return! config.Guid |> registerImportAndReturn
          | PrimitiveType.IntType -> return! config.Int |> registerImportAndReturn
          | PrimitiveType.RefType _ -> return! error
          | PrimitiveType.StringType -> return! config.String |> registerImportAndReturn
        | ExprType.TupleType items ->
          do! config.Tuple.RequiredImport |> registerImport
          let! items = items |> Seq.map (!) |> state.All
          formatKind config.Tuple.GeneratedTypeName items
        | ExprType.ListType e ->
          do! config.List.RequiredImport |> registerImport
          let! e = !e
          formatKind config.List.GeneratedTypeName [ e ]
        | ExprType.SetType e ->
          do! config.Set.RequiredImport |> registerImport
          let! e = !e
          formatKind config.Set.GeneratedTypeName [ e ]
        | ExprType.OptionType e ->
          do! config.Option.RequiredImport |> registerImport
          let! e = !e
          formatKind config.Option.GeneratedTypeName [ e ]
        | ExprType.MapType(k, v) ->
          do! config.Map.RequiredImport |> registerImport
          let! k = !k
          let! v = !v
          formatKind config.Map.GeneratedTypeName [ k; v ]
        | ExprType.SumType(l, r) ->
          do! config.Sum.RequiredImport |> registerImport
          let! l = !l
          let! r = !r
          formatKind config.Sum.GeneratedTypeName [ l; r ]
        | ExprType.CustomType _ -> return! error
        | ExprType.VarType _ -> return! error
        | ExprType.SchemaLookupType _ -> return! error
        | ExprType.RecordType _ -> return! error
        | ExprType.UnionType _ -> return! error
        | ExprType.OneType _ -> return! error
        | ExprType.ManyType _ -> return! error
        | ExprType.TableType _ -> return! error
      }
