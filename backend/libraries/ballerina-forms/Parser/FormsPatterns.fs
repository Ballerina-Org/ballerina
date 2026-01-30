namespace Ballerina.DSL.FormEngine.Parser

open Ballerina.LocalizedErrors

module FormsPatterns =
  open Ballerina.DSL.Parser.Patterns
  open Ballerina.DSL.FormEngine.Model
  open Ballerina.DSL.Expr.Model
  open Ballerina.DSL.Expr.Types.Model
  open Ballerina
  open Ballerina.Collections.Sum
  open Ballerina.State.WithError
  open Ballerina.Errors

  type ParsedFormsContext<'ExprExtension, 'ValueExtension> with
    static member ContextOperations: ContextOperations<ParsedFormsContext<'ExprExtension, 'ValueExtension>> =
      { TryFindType = fun ctx -> TypeContext.ContextOperations.TryFindType ctx.Types }

    member ctx.TryFindEnum name =
      ctx.Apis.Enums |> Map.tryFindWithError name "enum" (fun () -> name) ()

    member ctx.TryFindOne typeName name =
      sum {
        let! lookup =
          ctx.Apis.Lookups
          |> Map.tryFindWithError typeName "lookup (for one)" (fun () -> typeName) ()

        return! lookup.Ones |> Map.tryFindWithError name "one" (fun () -> name) ()
      }

    member ctx.TryFindMany typeName name =
      sum {
        let! lookup =
          ctx.Apis.Lookups
          |> Map.tryFindWithError typeName "lookup (for many)" (fun () -> typeName) ()

        return! lookup.Manys |> Map.tryFindWithError name "many" (fun () -> name) ()
      }

    member ctx.TryFindLookupStream typeName name =
      sum {
        let! lookup =
          ctx.Apis.Lookups
          |> Map.tryFindWithError typeName "lookup (for stream)" (fun () -> typeName) ()

        return! lookup.Streams |> Map.tryFindWithError name "stream" (fun () -> name) ()
      }

    member ctx.TryFindStream name =
      ctx.Apis.Streams |> Map.tryFindWithError name "stream" (fun () -> name) ()

    member ctx.TryFindTableApi name =
      ctx.Apis.Tables |> Map.tryFindWithError name "table" (fun () -> name) ()

    member ctx.TryFindEntityApi name =
      ctx.Apis.Entities |> Map.tryFindWithError name "entity api" (fun () -> name) ()

    member ctx.TryFindType name = TypeContext.TryFindType ctx.Types name

    member ctx.TryFindForm name =
      ctx.Forms
      |> Map.tryFindWithError name "form" (fun () -> name |> fun (FormName name) -> name) ()

    member ctx.TryFindLauncher name =
      ctx.Launchers
      |> Map.tryFindWithError name "launcher" (name |> fun (LauncherName name) -> (fun () -> name))

  type StateBuilder with
    member state.TryFindType<'c, 'ExprExtension, 'ValueExtension>
      name
      : State<TypeBinding, 'c, ParsedFormsContext<'ExprExtension, 'ValueExtension>, Errors<Unit>> =
      state {
        let! (s: ParsedFormsContext<'ExprExtension, 'ValueExtension>) = state.GetState()
        return! s.TryFindType name |> state.OfSum
      }

    member state.TryFindForm<'c, 'ExprExtension, 'ValueExtension>
      (name: FormName)
      : State<_, 'c, ParsedFormsContext<'ExprExtension, 'ValueExtension>, Errors<unit>> =
      state {
        let! (s: ParsedFormsContext<'ExprExtension, 'ValueExtension>) = state.GetState()
        return! s.TryFindForm name |> state.OfSum
      }

  type FormBody<'ExprExtension, 'ValueExtension> with
    static member TryGetFields
      (fb: FormBody<'ExprExtension, 'ValueExtension>)
      : State<Map<string, FieldConfig<'ExprExtension, 'ValueExtension>>, _, _, Errors<unit>> =
      match fb with
      | FormBody.Annotated fs ->
        match fs.Renderer with
        | Renderer.RecordRenderer fs -> state.Return fs.Fields.Fields
        | _ ->
          state.Throw(
            Errors.Singleton () (fun () -> sprintf "Error: not a record renderer: %s" (fs.Renderer.ToString()))
          )
      | FormBody.Table _ ->
        state.Throw(Errors.Singleton () (fun () -> $"Error: expected fields in form body, found cases."))

  and FormBody<'ExprExtension, 'ValueExtension> with
    static member Type
      (types: TypeContext)
      (self: FormBody<'ExprExtension, 'ValueExtension>)
      : Sum<ExprType, Errors<unit>> =
      let lookupType (id: ExprTypeId) =
        let name = id.VarName

        types
        |> Map.tryFindWithError name "type" (fun () -> name) ()
        |> Sum.map (fun tb -> tb.Type)

      match self with
      | FormBody.Annotated f -> lookupType f.TypeId
      | FormBody.Table f -> lookupType f.RowTypeId |> Sum.map ExprType.TableType
