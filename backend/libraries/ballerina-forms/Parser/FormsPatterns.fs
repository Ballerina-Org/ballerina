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
      ctx.Forms |> Map.tryFindWithError name "form" name

    member ctx.TryFindLauncher name =
      ctx.Launchers |> Map.tryFindWithError name "launcher" name

  type StateBuilder with
    member state.TryFindType<'c, 'ExprExtension, 'ValueExtension>
      name
      : State<TypeBinding, 'c, ParsedFormsContext<'ExprExtension, 'ValueExtension>, Errors> =
      state {
        let! (s: ParsedFormsContext<'ExprExtension, 'ValueExtension>) = state.GetState()
        return! s.TryFindType name |> state.OfSum
      }

    member state.TryFindForm<'c, 'ExprExtension, 'ValueExtension>
      name
      : State<_, 'c, ParsedFormsContext<'ExprExtension, 'ValueExtension>, Errors> =
      state {
        let! (s: ParsedFormsContext<'ExprExtension, 'ValueExtension>) = state.GetState()
        return! s.TryFindForm name |> state.OfSum
      }

  type FormBody<'ExprExtension, 'ValueExtension> with
    static member TryGetFields fb =
      match fb with
      | FormBody.Record fs -> state { return fs }
      | FormBody.Union _
      | FormBody.Table _ -> state.Throw(Errors.Singleton $"Error: expected fields in form body, found cases.")

  type NestedRenderer<'ExprExtension, 'ValueExtension> with
    member self.Type = self.Renderer.Type

  and Renderer<'ExprExtension, 'ValueExtension> with
    member self.Type =
      match self with
      | Multiple r -> r.First.NestedRenderer.Type
      | PrimitiveRenderer p -> p.Type
      | MapRenderer r -> ExprType.MapType(r.Key.Type, r.Value.Type)
      | SumRenderer r -> ExprType.SumType(r.Left.Type, r.Right.Type)
      | ListRenderer r -> ExprType.ListType r.Element.Type
      | OptionRenderer r -> ExprType.OptionType r.Some.Type
      | OneRenderer r -> ExprType.OneType r.Details.Type
      | ManyRenderer(ManyAllRenderer r) -> ManyType r.Element.Type
      | ManyRenderer(ManyLinkedUnlinkedRenderer r) -> ManyType r.Linked.Type
      | ReadOnlyRenderer r -> ExprType.ReadOnlyType r.Value.Type
      // | TableRenderer r -> ExprType.TableType r.Row.Type
      | EnumRenderer(_, r)
      | StreamRenderer(_, r) -> r.Type
      | TupleRenderer i -> ExprType.TupleType(i.Elements |> Seq.map (fun e -> e.Type) |> List.ofSeq)
      | FormRenderer(_, t)
      | TableFormRenderer(_, t, _) -> t
      | InlineFormRenderer i -> i.Body.Type
  // | UnionRenderer r ->
  //   ExprType.UnionType(
  //     r.Cases
  //     |> Map.map (fun cn c ->
  //       { CaseName = cn.CaseName
  //         Fields = c.Type })
  //   )

  and FormBody<'ExprExtension, 'ValueExtension> with
    member self.Type =
      match self with
      | FormBody.Record f -> f.RecordType
      | FormBody.Union f -> f.UnionType
      | FormBody.Table f -> f.RowType |> ExprType.TableType
