namespace Ballerina.DSL.FormEngine.Parser

module FormsPatterns =
  open Ballerina.DSL.Parser.Patterns
  open Ballerina.DSL.FormEngine.Model
  open Ballerina.DSL.Expr.Model
  open Ballerina.DSL.Expr.Types.Model
  open Ballerina.Collections.Sum
  open Ballerina.State.WithError
  open Ballerina.Errors

  type ParsedFormsContext<'ExprExtension, 'ValueExtension> with
    static member ContextOperations: ContextOperations<ParsedFormsContext<'ExprExtension, 'ValueExtension>> =
      { TryFindType = fun ctx -> TypeContext.ContextOperations.TryFindType ctx.Types }

    member ctx.TryFindEnum name =
      ctx.Apis.Enums |> Map.tryFindWithError name "enum" name

    member ctx.TryFindOne typeName name =
      sum {
        let! lookup = ctx.Apis.Lookups |> Map.tryFindWithError typeName "lookup (for one)" typeName
        return! lookup.Ones |> Map.tryFindWithError name "one" name
      }

    member ctx.TryFindMany typeName name =
      sum {
        let! lookup = ctx.Apis.Lookups |> Map.tryFindWithError typeName "lookup (for many)" typeName
        return! lookup.Manys |> Map.tryFindWithError name "many" name
      }

    member ctx.TryFindLookupStream typeName name =
      sum {
        let! lookup = ctx.Apis.Lookups |> Map.tryFindWithError typeName "lookup (for stream)" typeName
        return! lookup.Streams |> Map.tryFindWithError name "stream" name
      }

    member ctx.TryFindStream name =
      ctx.Apis.Streams |> Map.tryFindWithError name "stream" name

    member ctx.TryFindTableApi name =
      ctx.Apis.Tables |> Map.tryFindWithError name "table" name

    member ctx.TryFindEntityApi name =
      ctx.Apis.Entities |> Map.tryFindWithError name "entity api" name

    member ctx.TryFindType name = TypeContext.TryFindType ctx.Types name

    member ctx.TryFindForm name =
      ctx.Forms
      |> Map.tryFindWithError name "form" (name |> fun (FormName name) -> name)

    member ctx.TryFindLauncher name =
      ctx.Launchers
      |> Map.tryFindWithError name "launcher" (name |> fun (LauncherName name) -> name)

  type StateBuilder with
    member state.TryFindType<'c, 'ExprExtension, 'ValueExtension>
      name
      : State<TypeBinding, 'c, ParsedFormsContext<'ExprExtension, 'ValueExtension>, Errors> =
      state {
        let! (s: ParsedFormsContext<'ExprExtension, 'ValueExtension>) = state.GetState()
        return! s.TryFindType name |> state.OfSum
      }

    member state.TryFindForm<'c, 'ExprExtension, 'ValueExtension>
      (name: FormName)
      : State<_, 'c, ParsedFormsContext<'ExprExtension, 'ValueExtension>, Errors> =
      state {
        let! (s: ParsedFormsContext<'ExprExtension, 'ValueExtension>) = state.GetState()
        return! s.TryFindForm name |> state.OfSum
      }

  type FormBody<'ExprExtension, 'ValueExtension> with
    static member TryGetFields
      (fb: FormBody<'ExprExtension, 'ValueExtension>)
      : State<Map<string, FieldConfig<'ExprExtension, 'ValueExtension>>, _, _, Errors> =
      match fb with
      | FormBody.Annotated fs ->
        match fs.Renderer with
        | Renderer.RecordRenderer fs -> state.Return fs.Fields.Fields
        | _ -> state.Throw(Errors.Singleton(sprintf "Error: not a record renderer: %s" (fs.Renderer.ToString())))
      | FormBody.Table _ -> state.Throw(Errors.Singleton $"Error: expected fields in form body, found cases.")

  and FormBody<'ExprExtension, 'ValueExtension> with
    static member Type (types: TypeContext) (self: FormBody<'ExprExtension, 'ValueExtension>) : Sum<ExprType, Errors> =
      let lookupType (id: ExprTypeId) =
        let name = id.VarName

        types
        |> Map.tryFindWithError<string, TypeBinding> name "type" name
        |> Sum.map (fun tb -> tb.Type)

      match self with
      | FormBody.Annotated f -> lookupType f.TypeId
      | FormBody.Table f -> lookupType f.RowTypeId |> Sum.map ExprType.TableType
