namespace Ballerina.DSL.Codegen.Python.LanguageConstructs

module TypeAnnotations =
  open Ballerina.DSL.Expr.Model
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

      let registerImportAndReturn (t: CodegenConfigTypeDef) =
        state {
          do!
            t.RequiredImport
            |> Option.toList
            |> Set.ofList
            |> Set.union
            |> PythonCodeGenState.Updaters.UsedImports
            |> state.SetState

          return t.GeneratedTypeName
        }

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
          | PrimitiveType.RefType r -> return! error
          | PrimitiveType.StringType -> return! config.String |> registerImportAndReturn
        | ExprType.TupleType items ->
          do!
            config.Tuple.RequiredImport
            |> Option.toList
            |> Set.ofList
            |> Set.union
            |> PythonCodeGenState.Updaters.UsedImports
            |> state.SetState

          let! items = items |> Seq.map (!) |> state.All

          $"{config.Tuple.GeneratedTypeName}[{System.String.Join(',', items)}]"

        | ExprType.ListType e ->
          let! e = !e

          do!
            config.List.RequiredImport
            |> Option.toList
            |> Set.ofList
            |> Set.union
            |> PythonCodeGenState.Updaters.UsedImports
            |> state.SetState

          $"{config.List.GeneratedTypeName}[{e}]"

        | ExprType.SetType e ->
          let! e = !e

          do!
            config.Set.RequiredImport
            |> Option.toList
            |> Set.ofList
            |> Set.union
            |> PythonCodeGenState.Updaters.UsedImports
            |> state.SetState

          $"{config.Set.GeneratedTypeName}[{e}]"
        | ExprType.OptionType e ->
          let! e = !e

          do!
            config.Option.RequiredImport
            |> Option.toList
            |> Set.ofList
            |> Set.union
            |> PythonCodeGenState.Updaters.UsedImports
            |> state.SetState

          $"{config.Option.GeneratedTypeName}[{e}]"

        | ExprType.MapType(k, v) ->
          let! k = !k
          let! v = !v

          do!
            config.Map.RequiredImport
            |> Option.toList
            |> Set.ofList
            |> Set.union
            |> PythonCodeGenState.Updaters.UsedImports
            |> state.SetState

          $"{config.Map.GeneratedTypeName}[{k},{v}]"
        | ExprType.SumType(l, r) ->
          let! l = !l
          let! r = !r

          do!
            config.Sum.RequiredImport
            |> Option.toList
            |> Set.ofList
            |> Set.union
            |> PythonCodeGenState.Updaters.UsedImports
            |> state.SetState

          $"{config.Sum.GeneratedTypeName}[{l},{r}]"
        | _ -> return! error
      }
