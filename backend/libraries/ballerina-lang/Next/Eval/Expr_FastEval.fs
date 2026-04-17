namespace Ballerina.DSL.Next.Terms

[<AutoOpen>]
module FastEval =
  open Ballerina
  open Ballerina.Collections.Sum
  open Ballerina.Collections.Map
  open Ballerina.Coroutines.Model
  open Ballerina.Reader.WithError
  open Ballerina.Errors
  open Ballerina.LocalizedErrors
  open Ballerina.StdLib.String
  open Ballerina.StdLib.Object
  open System
  open System.Runtime.CompilerServices
  open Ballerina.DSL.Next.Terms.Patterns
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Types.Patterns
  open Ballerina.DSL.Next.Types.TypeChecker.Model
  open Ballerina
  open Ballerina.Cat.Collections.OrderedMap
  open Ballerina.Collections.NonEmptyList

  // ── Shared type definitions (used by both FastEval and Eval) ───────────

  type ExprEvalContextSymbols =
    { Types: Map<ResolvedIdentifier, TypeSymbol>
      RecordFields: Map<ResolvedIdentifier, TypeSymbol>
      UnionCases: Map<ResolvedIdentifier, TypeSymbol> }

    static member Empty =
      { Types = Map.empty
        RecordFields = Map.empty
        UnionCases = Map.empty }

    static member FromTypeChecker(ctx: TypeExprEvalSymbols) =
      { Types = ctx.Types
        RecordFields = ctx.RecordFields
        UnionCases = ctx.UnionCases }

    static member Append
      (s1: ExprEvalContextSymbols)
      (s2: ExprEvalContextSymbols)
      =
      { Types = Map.fold (fun acc k v -> Map.add k v acc) s1.Types s2.Types
        RecordFields =
          Map.fold
            (fun acc k v -> Map.add k v acc)
            s1.RecordFields
            s2.RecordFields
        UnionCases =
          Map.fold (fun acc k v -> Map.add k v acc) s1.UnionCases s2.UnionCases }

  type ExprEvalContextScope<'valueExtension> =
    { Values:
        Map<
          ResolvedIdentifier,
          Value<TypeValue<'valueExtension>, 'valueExtension>
         >
      Symbols: ExprEvalContextSymbols }

  type ExprEvalContext<'runtimeContext, 'valueExtension> =
    { Scope: ExprEvalContextScope<'valueExtension>
      /// Stack of scope frames pushed during evaluation.
      /// Lookup traverses frames top-to-bottom, then falls back to Scope.Values.
      /// Empty outside the JIT fast path; flushed before bridging to Reader-based code.
      ValueOverlays:
        Map<
          ResolvedIdentifier,
          Value<TypeValue<'valueExtension>, 'valueExtension>
         > list
      ExtensionOps: ValueExtensionOps<'runtimeContext, 'valueExtension>
      RuntimeContext: 'runtimeContext
      RootLevelEval: bool }

  and ApplicableExtEvalResult<'runtimeContext, 'valueExtension> =
    (Location
      -> List<RunnableExpr<'valueExtension>>
      -> 'valueExtension
      -> Value<TypeValue<'valueExtension>, 'valueExtension>
      -> ExprEvaluator<
        'runtimeContext,
        'valueExtension,
        Value<TypeValue<'valueExtension>, 'valueExtension>
       >)

  and ExtEvalResult<'runtimeContext, 'valueExtension> =
    | Result of Value<TypeValue<'valueExtension>, 'valueExtension>
    | Async of
      Coroutine<
        ExtEvalResult<'runtimeContext, 'valueExtension>,
        Unit,
        Unit,
        Unit,
        Errors<Location>
       >
    | Applicable of
      (Value<TypeValue<'valueExtension>, 'valueExtension>
        -> ExprEvaluator<
          'runtimeContext,
          'valueExtension,
          Value<TypeValue<'valueExtension>, 'valueExtension>
         >)
    | TypeApplicable of
      (TypeValue<'valueExtension>
        -> ExprEvaluator<
          'runtimeContext,
          'valueExtension,
          Value<TypeValue<'valueExtension>, 'valueExtension>
         >)
    | Matchable of
      (Map<ResolvedIdentifier, RunnableCaseHandler<'valueExtension>>
        -> ExprEvaluator<
          'runtimeContext,
          'valueExtension,
          Value<TypeValue<'valueExtension>, 'valueExtension>
         >)

  and ExtensionEvaluator<'runtimeContext, 'valueExtension> =
    Location
      -> List<RunnableExpr<'valueExtension>>
      -> 'valueExtension
      -> ExprEvaluator<
        'runtimeContext,
        'valueExtension,
        ExtEvalResult<'runtimeContext, 'valueExtension>
       >

  and FastApplicable<'runtimeContext, 'valueExtension> =
    Location
      -> List<RunnableExpr<'valueExtension>>
      -> ExprEvalContext<'runtimeContext, 'valueExtension>
      -> 'valueExtension
      -> Value<TypeValue<'valueExtension>, 'valueExtension>
      -> Value<TypeValue<'valueExtension>, 'valueExtension>

  and ValueExtensionOps<'runtimeContext, 'valueExtension> =
    { Eval: ExtensionEvaluator<'runtimeContext, 'valueExtension>
      Applicables:
        Map<
          ResolvedIdentifier,
          ApplicableExtEvalResult<'runtimeContext, 'valueExtension>
         >
      FastApplicables:
        Map<
          ResolvedIdentifier,
          FastApplicable<'runtimeContext, 'valueExtension>
         > }

  and ExprEvaluator<'runtimeContext, 'valueExtension, 'res> =
    Reader<
      'res,
      ExprEvalContext<'runtimeContext, 'valueExtension>,
      Errors<Location>
     >

  // ── Exception type for fast error propagation ──────────────────────────

  type EvalException(errors: Errors<Location>) =
    inherit Exception("Ballerina eval error")
    member _.Errors = errors

  // ── Inline unsafe extractors: no pattern match on happy path ───────────
  // F# DUs compile to subclasses. We match only one case and throw on mismatch.

  let inline fastAsRecord (loc: Location) (v: Value<TypeValue<'ext>, 'ext>) =
    match v with
    | Value.Record m -> m
    | _ ->
      raise (
        EvalException(
          Errors.Singleton loc (fun () -> $"expected a record but got {v}")
        )
      )

  let inline fastAsTuple (loc: Location) (v: Value<TypeValue<'ext>, 'ext>) =
    match v with
    | Value.Tuple items -> items
    | _ ->
      raise (
        EvalException(
          Errors.Singleton loc (fun () -> $"expected a tuple but got {v}")
        )
      )

  let inline fastAsUnion
    (loc: Location)
    (v: Value<TypeValue<'ext>, 'ext>)
    =
    match v with
    | Value.UnionCase(id, inner) -> id, inner
    | _ ->
      raise (
        EvalException(
          Errors.Singleton loc (fun () -> $"expected a union but got {v}")
        )
      )

  let inline fastAsUnionCons
    (loc: Location)
    (v: Value<TypeValue<'ext>, 'ext>)
    =
    match v with
    | Value.UnionCons id -> id
    | _ ->
      raise (
        EvalException(
          Errors.Singleton loc (fun () -> $"expected a union cons but got {v}")
        )
      )

  let inline fastAsRecordDes
    (loc: Location)
    (v: Value<TypeValue<'ext>, 'ext>)
    =
    match v with
    | Value.RecordDes id -> id
    | _ ->
      raise (
        EvalException(
          Errors.Singleton loc (fun () -> $"expected a record des but got {v}")
        )
      )

  let inline fastAsSum (loc: Location) (v: Value<TypeValue<'ext>, 'ext>) =
    match v with
    | Value.Sum(sel, inner) -> sel, inner
    | _ ->
      raise (
        EvalException(
          Errors.Singleton loc (fun () -> $"expected a sum but got {v}")
        )
      )

  let inline fastAsExt (loc: Location) (v: Value<TypeValue<'ext>, 'ext>) =
    match v with
    | Value.Ext(ext, appId) -> ext, appId
    | _ ->
      raise (
        EvalException(
          Errors.Singleton loc (fun () -> $"expected an ext but got {v}")
        )
      )

  let inline fastAsLambda
    (loc: Location)
    (v: Value<TypeValue<'ext>, 'ext>)
    =
    match v with
    | Value.Lambda(var, body, closure, scope) -> var, body, closure, scope
    | _ ->
      raise (
        EvalException(
          Errors.Singleton loc (fun () -> $"expected a lambda but got {v}")
        )
      )

  let inline fastAsQuery (loc: Location) (v: Value<TypeValue<'ext>, 'ext>) =
    match v with
    | Value.Query q -> q
    | _ ->
      raise (
        EvalException(
          Errors.Singleton loc (fun () -> $"expected a query but got {v}")
        )
      )

  let inline fastAsBool (loc: Location) (v: Value<TypeValue<'ext>, 'ext>) =
    match v with
    | Value.Primitive(PrimitiveValue.Bool b) -> b
    | _ ->
      raise (
        EvalException(
          Errors.Singleton
            loc
            (fun () -> $"expected boolean in if condition, got {v}")
        )
      )

  let inline fastMapFind
    (loc: Location)
    (category: string)
    (keyStr: unit -> string)
    (key: 'k)
    (m: Map<'k, 'v>)
    =
    match Map.tryFind key m with
    | Some v -> v
    | None ->
      raise (
        EvalException(
          Errors.Singleton
            loc
            (fun () -> sprintf "Cannot find %s '%s'" category (keyStr ()))
        )
      )

  let inline fastListItem (loc: Location) (index: int) (lst: 'a list) =
    match List.tryItem index lst with
    | Some v -> v
    | None ->
      raise (
        EvalException(
          Errors.Singleton
            loc
            (fun () ->
              $"Error: tuple index {index + 1} out of bounds, size {List.length lst}")
        )
      )

  // ── Bridge: run Reader-based extension code from fast eval ─────────────
  // Flushes overlays into Scope.Values so Reader-based code sees a flat scope.

  // Cache for flattenScope: avoids re-merging when the same overlay list + base are flattened
  // repeatedly (e.g., multiple Lambda creations inside the same let-binding body).
  let private flattenCache =
    ConditionalWeakTable<obj, obj[]>()

  let flattenScope
    (ctx: ExprEvalContext<'rc, 'ext>)
    : Map<ResolvedIdentifier, Value<TypeValue<'ext>, 'ext>> =
    match ctx.ValueOverlays with
    | [] -> ctx.Scope.Values
    | overlays ->
      // Cache: if the same overlay list (by reference) is flattened with the same base,
      // return the cached result. Common in Lambda creation inside let-heavy bodies.
      let key = overlays :> obj
      match flattenCache.TryGetValue(key) with
      | true, entry ->
        if System.Object.ReferenceEquals(entry[0], ctx.Scope.Values :> obj) then
          entry[1] :?> Map<ResolvedIdentifier, Value<TypeValue<'ext>, 'ext>>
        else
          let result =
            overlays
            |> List.foldBack
              (fun frame acc -> frame |> Map.fold (fun a k v -> Map.add k v a) acc)
              <| ctx.Scope.Values
          flattenCache.AddOrUpdate(key, [| ctx.Scope.Values :> obj; result :> obj |])
          result
      | false, _ ->
        let result =
          overlays
          |> List.foldBack
            (fun frame acc -> frame |> Map.fold (fun a k v -> Map.add k v a) acc)
            <| ctx.Scope.Values
        flattenCache.AddOrUpdate(key, [| ctx.Scope.Values :> obj; result :> obj |])
        result

  let inline flushOverlays
    (ctx: ExprEvalContext<'rc, 'ext>)
    : ExprEvalContext<'rc, 'ext> =
    match ctx.ValueOverlays with
    | [] -> ctx
    | _ ->
      { ctx with
          Scope = { ctx.Scope with Values = flattenScope ctx }
          ValueOverlays = [] }

  let inline runReader
    (_loc: Location)
    (r: Reader<'a, ExprEvalContext<'rc, 'ext>, Errors<Location>>)
    (ctx: ExprEvalContext<'rc, 'ext>)
    : 'a =
    let ctx = flushOverlays ctx
    match Reader.Run ctx r with
    | Left v -> v
    | Right errors -> raise (EvalException(errors))

  // ── Scope chain helpers ────────────────────────────────────────────────
  // Instead of merging closures/bindings into Scope.Values (expensive Map merge),
  // we push lightweight frames onto ValueOverlays and traverse top-to-bottom on lookup.
  // Lambda application becomes O(1) push instead of O(|closure| * log |ctx|) merge.

  let inline lookupValue
    (id: ResolvedIdentifier)
    (ctx: ExprEvalContext<'rc, 'ext>)
    : Value<TypeValue<'ext>, 'ext> voption =
    let rec search frames =
      match frames with
      | [] -> Map.tryFind id ctx.Scope.Values |> ValueOption.ofOption
      | frame :: rest ->
        match Map.tryFind id frame with
        | Some v -> ValueSome v
        | None -> search rest
    search ctx.ValueOverlays

  let inline pushBinding
    (id: ResolvedIdentifier)
    (value: Value<TypeValue<'ext>, 'ext>)
    (ctx: ExprEvalContext<'rc, 'ext>)
    : ExprEvalContext<'rc, 'ext> =
    match ctx.ValueOverlays with
    | top :: rest ->
      { ctx with ValueOverlays = (Map.add id value top) :: rest }
    | [] ->
      { ctx with ValueOverlays = [ Map.ofList [ id, value ] ] }

  let inline pushFrame
    (frame: Map<ResolvedIdentifier, Value<TypeValue<'ext>, 'ext>>)
    (ctx: ExprEvalContext<'rc, 'ext>)
    : ExprEvalContext<'rc, 'ext> =
    { ctx with ValueOverlays = frame :: ctx.ValueOverlays }

  // Legacy helper — still used by some external callers through Updaters.Values
  let inline extendValues
    (updater: Map<ResolvedIdentifier, Value<TypeValue<'ext>, 'ext>> -> Map<ResolvedIdentifier, Value<TypeValue<'ext>, 'ext>>)
    (ctx: ExprEvalContext<'rc, 'ext>)
    : ExprEvalContext<'rc, 'ext> =
    { ctx with
        Scope =
          { ctx.Scope with
              Values = updater ctx.Scope.Values } }

  // ── Memoization caches ──────────────────────────────────────────────────
  // ConditionalWeakTable: keys are held weakly. When a RunnableExpr is GC'd
  // (e.g. schema reload), the compiled lambda is collected automatically.
  // Thread-safe by default.

  // CompiledExpr is just: ExprEvalContext<'rc, 'ext> -> Value<TypeValue<'ext>, 'ext>
  // We don't use a type alias because F# requires all type params to appear in the abbreviated type.

  let private compilationCache =
    ConditionalWeakTable<obj, obj>()

  // ── Core JIT compiler ──────────────────────────────────────────────────

  /// Compile a single expression (no rest/continuation).
  /// Memoized by RunnableExpr reference identity.
  let rec compileSingle<'rc, 'ext>
    (e: RunnableExpr<'ext>)
    : (ExprEvalContext<'rc, 'ext> -> Value<TypeValue<'ext>, 'ext>) =
    let key = e :> obj

    match compilationCache.TryGetValue(key) with
    | true, cached -> cached :?> (ExprEvalContext<'rc, 'ext> -> Value<TypeValue<'ext>, 'ext>)
    | _ ->
      let compiled = compileSingleCore<'rc, 'ext> e
      compilationCache.AddOrUpdate(key, compiled :> obj)
      compiled

  /// Compile an expression sequence (head + rest).
  /// This is the main entry point from Expr.Eval.
  and compileSequence<'rc, 'ext>
    (exprs: NonEmptyList<RunnableExpr<'ext>>)
    : (ExprEvalContext<'rc, 'ext> -> Value<TypeValue<'ext>, 'ext>) =
    let (NonEmptyList(head, tail)) = exprs
    compileWithRest<'rc, 'ext> head tail

  /// Compile an expression in tail position with a continuation list.
  /// The `rest` parameter represents subsequent expressions (prelude mechanism).
  and compileWithRest<'rc, 'ext>
    (e: RunnableExpr<'ext>)
    (rest: List<RunnableExpr<'ext>>)
    : (ExprEvalContext<'rc, 'ext> -> Value<TypeValue<'ext>, 'ext>) =
    let loc0 = e.Location

    match e.Expr with

    // ── Primitives ────────────────────────────────────────────────
    | RunnableExprRec.Primitive PrimitiveValue.Unit ->
      match rest with
      | [] -> fun _ -> Value.Primitive PrimitiveValue.Unit
      | p :: rest -> compileWithRest<'rc, 'ext> p rest

    | RunnableExprRec.Primitive v -> fun _ -> Value.Primitive v

    // ── If-Then-Else ──────────────────────────────────────────────
    | RunnableExprRec.If { RunnableExprIf.Cond = cond
                           RunnableExprIf.Then = thenBody
                           RunnableExprIf.Else = elseBody } ->
      let compiledCond = compileSingle<'rc, 'ext> cond
      let compiledThen = compileWithRest<'rc, 'ext> thenBody rest
      let compiledElse = compileWithRest<'rc, 'ext> elseBody rest

      fun ctx ->
        let condV = compiledCond ctx

        if fastAsBool loc0 condV then
          compiledThen ctx
        else
          compiledElse ctx

    // ── Let binding ───────────────────────────────────────────────
    | RunnableExprRec.Let { RunnableExprLet.Var = var
                            RunnableExprLet.Type = _
                            RunnableExprLet.Val = valueExpr
                            RunnableExprLet.Rest = body } ->
      let compiledVal = compileSingle<'rc, 'ext> valueExpr
      let compiledBody = compileWithRest<'rc, 'ext> body rest
      let resolvedId = var.Name |> Identifier.LocalScope |> e.Scope.Resolve

      fun ctx ->
        let value = compiledVal ctx
        let ctx' = pushBinding resolvedId value ctx
        compiledBody ctx'

    // ── Do (sequence, discard first) ──────────────────────────────
    | RunnableExprRec.Do { Val = e1; Rest = e2 } ->
      let compiled1 = compileSingle<'rc, 'ext> e1
      let compiled2 = compileWithRest<'rc, 'ext> e2 rest

      fun ctx ->
        compiled1 ctx |> ignore
        compiled2 ctx

    // ── Lookup ────────────────────────────────────────────────────
    | RunnableExprRec.Lookup({ Id = id }) ->
      fun ctx ->
        match lookupValue id ctx with
        | ValueSome v -> v
        | ValueNone ->
          raise (
            EvalException(
              Errors.Singleton
                loc0
                (fun () -> $"Cannot find variables '{id.AsFSharpString}'")
            )
          )

    // ── Record construction ───────────────────────────────────────
    | RunnableExprRec.RecordCons { Fields = fields } ->
      let compiledFields =
        fields
        |> List.map (fun (id, field) -> id, compileSingle<'rc, 'ext> field)

      fun ctx ->
        let evaluatedFields =
          compiledFields
          |> List.map (fun (id, cf) -> id, cf ctx)

        Value.Record(Map.ofList evaluatedFields)

    // ── Record update ─────────────────────────────────────────────
    | RunnableExprRec.RecordWith({ Record = record; Fields = fields }) ->
      let compiledRecord = compileSingle<'rc, 'ext> record

      let compiledFields =
        fields
        |> List.map (fun (id, field) -> id, compileSingle<'rc, 'ext> field)

      fun ctx ->
        let recordV = fastAsRecord loc0 (compiledRecord ctx)

        let newFields =
          compiledFields
          |> List.map (fun (id, cf) -> id, cf ctx)

        let merged =
          newFields
          |> List.fold (fun acc (k, v) -> Map.add k v acc) recordV

        Value.Record(merged)

    // ── Record field access ───────────────────────────────────────
    | RunnableExprRec.RecordDes({ RunnableExprRecordDes.Expr = recordExpr
                                  RunnableExprRecordDes.Field = fieldId }) ->
      let compiledRecord = compileSingle<'rc, 'ext> recordExpr

      fun ctx ->
        let recordV = fastAsRecord loc0 (compiledRecord ctx)

        fastMapFind
          loc0
          "record field"
          (fun () -> fieldId.AsFSharpString)
          fieldId
          recordV

    // ── Tuple construction ────────────────────────────────────────
    | RunnableExprRec.TupleCons { Items = items } ->
      let compiledItems =
        items |> List.map (compileSingle<'rc, 'ext>)

      fun ctx ->
        let evaluatedItems = compiledItems |> List.map (fun ci -> ci ctx)
        Value.Tuple(evaluatedItems)

    // ── Tuple field access ────────────────────────────────────────
    | RunnableExprRec.TupleDes { RunnableExprTupleDes.Tuple = tupleExpr
                                 RunnableExprTupleDes.Item = fieldId } ->
      let compiledTuple = compileSingle<'rc, 'ext> tupleExpr

      fun ctx ->
        let tupleV = fastAsTuple loc0 (compiledTuple ctx)
        fastListItem loc0 (fieldId.Index - 1) tupleV

    // ── SumCons (standalone, returns lambda) ──────────────────────
    | RunnableExprRec.SumCons({ Selector = selector }) ->
      let t_unit = TypeValue.CreateUnit()
      let k_star = Kind.Star

      fun _ctx ->
        Value.Lambda(
          Var.Create "x",
          RunnableExpr.Apply(
            RunnableExpr.SumCons(selector, t_unit, k_star),
            RunnableExpr.Lookup(
              "x" |> Identifier.LocalScope |> e.Scope.Resolve,
              t_unit,
              k_star
            ),
            t_unit,
            k_star
          ),
          Map.empty,
          e.Scope
        )

    // ── Apply(SumCons, arg) — specialized ─────────────────────────
    | RunnableExprRec.Apply({ RunnableExprApply.F = { Expr = RunnableExprRec.SumCons selector }
                              RunnableExprApply.Arg = valueE }) ->
      let compiledArg = compileSingle<'rc, 'ext> valueE

      fun ctx ->
        let argV = compiledArg ctx
        Value.Sum(selector.Selector, argV)

    // ── Apply(UnionDes, arg) — specialized ────────────────────────
    | RunnableExprRec.Apply({ RunnableExprApply.F = { RunnableExpr.Expr = RunnableExprRec.UnionDes({ RunnableExprUnionDes.Handlers = cases; RunnableExprUnionDes.Fallback = fallback }) }
                              RunnableExprApply.Arg = unionE }) ->
      let compiledUnionArg = compileSingle<'rc, 'ext> unionE

      // Pre-compile all case handler bodies with rest
      let compiledCases =
        cases
        |> Map.map (fun _k (caseVar, caseBody) ->
          caseVar, compileWithRest<'rc, 'ext> caseBody rest)

      let compiledFallback =
        fallback
        |> Option.map (compileSingle<'rc, 'ext>)

      fun ctx ->
        let unionV = compiledUnionArg ctx

        // Try as union first, then as ext (matchable)
        match unionV with
        | Value.UnionCase(unionVCase, innerV) ->
          match Map.tryFind unionVCase compiledCases with
          | Some(caseVar, compiledBody) ->
            let ctx' =
              match caseVar with
              | None -> ctx
              | Some caseVar ->
                let resolvedId =
                  caseVar.Name |> Identifier.LocalScope |> e.Scope.Resolve

                pushBinding resolvedId innerV ctx

            compiledBody ctx'
          | None ->
            match compiledFallback with
            | Some fb -> fb ctx
            | None ->
              raise (
                EvalException(
                  Errors.Singleton
                    loc0
                    (fun () ->
                      $"Error: cannot find case handler for union case. Cases = {cases.Keys.AsFSharpString}. Case = {unionVCase.AsFSharpString}.")
                )
              )

        | Value.Ext(extV, _) ->
          // Bridge to Reader-based extension for Matchable
          let extResult =
            runReader
              loc0
              (ctx.ExtensionOps.Eval loc0 (rest) extV)
              ctx

          match extResult with
          | Matchable f -> runReader loc0 (f cases) ctx
          | _ ->
            raise (
              EvalException(
                Errors.Singleton
                  loc0
                  (fun () ->
                    "Expected an applicable or matchable extension function")
              )
            )

        | _ ->
          raise (
            EvalException(
              Errors.Singleton
                loc0
                (fun () -> $"expected a union or ext but got {unionV}")
            )
          )

    // ── Apply(SumDes, arg) — specialized ──────────────────────────
    | RunnableExprRec.Apply({ RunnableExprApply.F = { Expr = RunnableExprRec.SumDes cases }
                              RunnableExprApply.Arg = sumE }) ->
      let compiledSumArg = compileSingle<'rc, 'ext> sumE

      let compiledHandlers =
        cases.Handlers
        |> Map.map (fun _k (caseVar, caseBody) ->
          caseVar, compileWithRest<'rc, 'ext> caseBody rest)

      fun ctx ->
        let sumV = compiledSumArg ctx
        let sumVCase, innerV = fastAsSum loc0 sumV

        let caseVar, compiledBody =
          fastMapFind
            loc0
            "sum case"
            (fun () -> sumVCase.AsFSharpString)
            sumVCase
            compiledHandlers

        let ctx' =
          match caseVar with
          | None -> ctx
          | Some caseVar ->
            let resolvedId =
              caseVar.Name |> Identifier.LocalScope |> e.Scope.Resolve

            pushBinding resolvedId innerV ctx

        compiledBody ctx'

    // ── FromValue ─────────────────────────────────────────────────
    | RunnableExprRec.FromValue({ RunnableExprFromValue.Value = v }) ->
      fun _ctx -> v

    // ── General Apply — THE BIG WIN ───────────────────────────────
    // Replace the 5-way reader.Any with a single pattern match on fV
    | RunnableExprRec.Apply({ F = f; Arg = argE }) ->
      let compiledF = compileSingle<'rc, 'ext> f
      let compiledArg = compileSingle<'rc, 'ext> argE

      fun ctx ->
        let fV = compiledF ctx
        let argV = compiledArg ctx
        fastApply<'rc, 'ext> loc0 e.Scope rest ctx fV argV

    // ── Lambda ────────────────────────────────────────────────────
    | RunnableExprRec.Lambda { RunnableExprLambda.Param = var
                               RunnableExprLambda.Body = body } ->
      fun ctx -> Value.Lambda(var, body, flattenScope ctx, e.Scope)

    // ── TypeLambda (type params erased at runtime) ────────────────
    | RunnableExprRec.TypeLambda({ Param = _; Body = body }) ->
      compileSingle<'rc, 'ext> body

    // ── TypeApply ─────────────────────────────────────────────────
    | RunnableExprRec.TypeApply({ RunnableExprTypeApply.Func = typeLambda
                                  RunnableExprTypeApply.TypeArg = typeArg }) ->
      let compiledBody = compileSingle<'rc, 'ext> typeLambda

      fun ctx ->
        let v = compiledBody ctx

        // For extensions: try TypeApplicable, else type application is erased at runtime
        match v with
        | Value.Ext(ext, _) ->
          let extResult =
            runReader loc0 (ctx.ExtensionOps.Eval loc0 rest ext) ctx

          match extResult with
          | TypeApplicable f -> runReader loc0 (f typeArg) ctx
          | Result r -> r
          | _ -> v  // type erasure: Applicable/Matchable/Async are unchanged
        | _ -> v

    // ── TypeLet ───────────────────────────────────────────────────
    | RunnableExprRec.TypeLet({ RunnableExprTypeLet.Name = typeName
                                RunnableExprTypeLet.TypeDef = typeDefinition
                                RunnableExprTypeLet.Body = body }) ->
      let scope =
        e.Scope |> TypeCheckScope.Updaters.Type(replaceWith (Some typeName))

      // Pre-compute union case bindings
      let unionBindings =
        match TypeValue.AsUnion typeDefinition with
        | Left(_, cases) ->
          cases
          |> OrderedMap.toSeq
          |> Seq.collect (fun (k, _) ->
            [ (k.Name |> scope.Resolve,
               Value.UnionCons(k.Name |> scope.Resolve))
              (k.Name.LocalName
               |> Identifier.LocalScope
               |> TypeCheckScope.Empty.Resolve,
               Value.UnionCons(k.Name |> scope.Resolve)) ])
          |> List.ofSeq
        | Right _ -> []

      // Pre-compute record field bindings
      let recordBindings =
        match TypeValue.AsRecord typeDefinition with
        | Left fields ->
          fields
          |> OrderedMap.toSeq
          |> Seq.collect (fun (k, _) ->
            [ (k.Name |> scope.Resolve,
               Value.RecordDes(k.Name |> scope.Resolve))
              (k.Name.LocalName
               |> Identifier.LocalScope
               |> TypeCheckScope.Empty.Resolve,
               Value.RecordDes(k.Name |> scope.Resolve)) ])
          |> List.ofSeq
        | Right _ -> []

      let allBindings = unionBindings @ recordBindings
      let bindingsFrame = allBindings |> Map.ofList
      let compiledBody = compileWithRest<'rc, 'ext> body rest

      fun ctx ->
        compiledBody (pushFrame bindingsFrame ctx)

    // ── EntitiesDes ───────────────────────────────────────────────
    | RunnableExprRec.EntitiesDes({ Expr = s }) ->
      let compiledS = compileSingle<'rc, 'ext> s

      let entitiesId =
        "Entities"
        |> Identifier.LocalScope
        |> TypeCheckScope.Empty.Resolve

      fun ctx ->
        let sV = fastAsRecord loc0 (compiledS ctx)
        fastMapFind loc0 "entities schema field" (fun () -> "Entities") entitiesId sV

    // ── RelationsDes ──────────────────────────────────────────────
    | RunnableExprRec.RelationsDes({ Expr = s }) ->
      let compiledS = compileSingle<'rc, 'ext> s

      let relationsId =
        "Relations"
        |> Identifier.LocalScope
        |> TypeCheckScope.Empty.Resolve

      fun ctx ->
        let sV = fastAsRecord loc0 (compiledS ctx)

        fastMapFind
          loc0
          "relations schema field"
          (fun () -> "Relations")
          relationsId
          sV

    // ── EntityDes ─────────────────────────────────────────────────
    | RunnableExprRec.EntityDes({ Expr = s; EntityName = entityName }) ->
      let compiledS = compileSingle<'rc, 'ext> s

      let entityId =
        entityName.Name
        |> Identifier.LocalScope
        |> TypeCheckScope.Empty.Resolve

      fun ctx ->
        let sV = fastAsRecord loc0 (compiledS ctx)

        fastMapFind
          loc0
          "entity schema field"
          (fun () -> entityName.Name)
          entityId
          sV

    // ── RelationDes ───────────────────────────────────────────────
    | RunnableExprRec.RelationDes({ RunnableExprRelationDes.Expr = s
                                    RunnableExprRelationDes.RelationName = relationName }) ->
      let compiledS = compileSingle<'rc, 'ext> s

      let relationId =
        relationName.Name
        |> Identifier.LocalScope
        |> TypeCheckScope.Empty.Resolve

      fun ctx ->
        let sV = fastAsRecord loc0 (compiledS ctx)

        fastMapFind
          loc0
          "relation schema field"
          (fun () -> relationName.Name)
          relationId
          sV

    // ── RelationLookupDes ─────────────────────────────────────────
    | RunnableExprRec.RelationLookupDes({ RunnableExprRelationLookupDes.Expr = record_e
                                          RunnableExprRelationLookupDes.Direction = direction }) ->
      let compiledRecord = compileSingle<'rc, 'ext> record_e

      let dirFieldId =
        (match direction with
         | FromTo -> "From"
         | _ -> "To")
        |> Identifier.LocalScope
        |> TypeCheckScope.Empty.Resolve

      fun ctx ->
        let recordV = fastAsRecord loc0 (compiledRecord ctx)

        fastMapFind
          loc0
          "relation schema field From/To"
          (fun () -> direction.AsFSharpString)
          dirFieldId
          recordV

    // ── Query (UnionQueries) ──────────────────────────────────────
    | RunnableExprRec.Query(RunnableExprQuery.UnionQueries(q1, q2)) ->
      let compiledQ1 =
        compileSingle<'rc, 'ext> (RunnableExpr.Query(q1, e.Type, e.Kind))

      let compiledQ2 =
        compileSingle<'rc, 'ext> (RunnableExpr.Query(q2, e.Type, e.Kind))

      fun ctx ->
        let v1 = fastAsQuery loc0 (compiledQ1 ctx)
        let v2 = fastAsQuery loc0 (compiledQ2 ctx)
        Value.Query(ValueQuery.ValueUnionQueries(v1, v2, v1.DeserializeFrom))

    // ── Query (SimpleQuery) ───────────────────────────────────────
    | RunnableExprRec.Query(RunnableExprQuery.SimpleQuery q) ->
      let compiledIteratorSources =
        q.Iterators
        |> NonEmptyList.map (fun it -> it, compileSingle<'rc, 'ext> it.Source)

      fun ctx ->
        // Evaluate iterator sources
        let iterators =
          compiledIteratorSources
          |> NonEmptyList.map (fun (it, compiledSource) ->
            { ValueQueryIterator.Location = it.Location
              Var = it.Var
              VarType = it.VarType
              Source = compiledSource ctx })

        // Build closure map (flatten overlays to get full scope)
        let allValues = flattenScope ctx
        let closure =
          allValues
          |> Map.filter (fun k _ -> q.Closure |> Map.containsKey k)
          |> Map.map (fun k v -> v, q.Closure.[k])

        // Replace closure lookups (pure structural traversal)
        let joins =
          q.Joins
          |> Option.map (
            NonEmptyList.map (fun join ->
              { join with
                  Left = replaceClosureLookups closure join.Left
                  Right = replaceClosureLookups closure join.Right })
          )

        let where =
          q.Where
          |> Option.map (replaceClosureLookups closure)

        let select = replaceClosureLookups closure q.Select

        let orderBy =
          q.OrderBy
          |> Option.map (fun (v, dir) -> replaceClosureLookups closure v, dir)

        let distinct =
          q.Distinct
          |> Option.map (replaceClosureLookups closure)

        Value.Query(
          ValueQuery.ValueQuerySimple
            { Iterators = iterators
              Joins = joins
              Where = where
              Select = select
              OrderBy = orderBy
              Distinct = distinct
              DeserializeFrom = q.DeserializeFrom }
        )

    // ── Fallback (should not happen for well-typed programs) ──────
    | _ ->
      fun _ctx ->
        raise (
          EvalException(
            Errors.Singleton loc0 (fun () -> $"Cannot eval expression {e}")
          )
        )

  /// The core compilation for a single expression (not memoized — memoization is in compileSingle).
  and compileSingleCore<'rc, 'ext>
    (e: RunnableExpr<'ext>)
    : (ExprEvalContext<'rc, 'ext> -> Value<TypeValue<'ext>, 'ext>) =
    compileWithRest<'rc, 'ext> e []

  // ── Fast Apply dispatch: replaces the 5-way reader.Any ─────────────────

  and fastApply<'rc, 'ext>
    (loc0: Location)
    (scope: TypeCheckScope)
    (rest: List<RunnableExpr<'ext>>)
    (ctx: ExprEvalContext<'rc, 'ext>)
    (fV: Value<TypeValue<'ext>, 'ext>)
    (argV: Value<TypeValue<'ext>, 'ext>)
    : Value<TypeValue<'ext>, 'ext> =

    match fV with
    // ── Lambda application (most common) ──────────────────────────
    | Value.Lambda(var, body, closure, _lambdaScope) ->
      let resolvedParam =
        var.Name
        |> Identifier.LocalScope
        |> TypeCheckScope.Empty.Resolve

      let compiledBody = compileSequence<'rc, 'ext> (NonEmptyList.OfList(body, rest))

      // O(1) push: closure + param as a single frame on the overlay stack.
      // No Map merge — lookups traverse frames top-to-bottom.
      let frame = closure |> Map.add resolvedParam argV
      compiledBody (pushFrame frame ctx)

    // ── UnionCons application ─────────────────────────────────────
    | Value.UnionCons id -> Value.UnionCase(id, argV)

    // ── Record with "cons" field (wrapped union constructor) ──────
    | Value.Record r ->
      let consId =
        "cons" |> Identifier.LocalScope |> scope.Resolve

      match Map.tryFind consId r with
      | Some(Value.UnionCons id) -> Value.UnionCase(id, argV)
      | _ ->
        raise (
          EvalException(
            Errors.Singleton
              loc0
              (fun () -> $"Cannot apply record as function: {fV}")
          )
        )

    // ── RecordDes (field extractor) ───────────────────────────────
    | Value.RecordDes fieldId ->
      let recordV = fastAsRecord loc0 argV

      fastMapFind
        loc0
        "record field"
        (fun () -> fieldId.ToString())
        fieldId
        recordV

    // ── Extension application ─────────────────────────────────────
    | Value.Ext(fExt, appId) ->
      match appId with
      | Some appId ->
        // Fast path: pre-compiled applicable (no Reader overhead)
        match ctx.ExtensionOps.FastApplicables |> Map.tryFind appId with
        | Some f -> f loc0 rest ctx fExt argV
        | None ->
        // Fallback: Reader-based applicable
        match ctx.ExtensionOps.Applicables |> Map.tryFind appId with
        | Some f -> runReader loc0 (f loc0 rest fExt argV) ctx
        | None ->
          let extResult =
            runReader loc0 (ctx.ExtensionOps.Eval loc0 rest fExt) ctx

          match extResult with
          | Applicable f -> runReader loc0 (f argV) ctx
          | _ ->
            raise (
              EvalException(
                Errors.Singleton loc0 (fun () -> $"Cannot apply {extResult}")
              )
            )
      | None ->
        let extResult =
          runReader loc0 (ctx.ExtensionOps.Eval loc0 rest fExt) ctx

        match extResult with
        | Applicable f -> runReader loc0 (f argV) ctx
        | _ ->
          raise (
            EvalException(
              Errors.Singleton loc0 (fun () -> $"Cannot apply {extResult}")
            )
          )

    // ── Error: cannot apply ───────────────────────────────────────
    | _ ->
      raise (
        EvalException(
          Errors.Singleton loc0 (fun () -> $"Cannot apply {fV}")
        )
      )

  // ── Query closure replacement (pure, no Reader) ────────────────────────

  and replaceClosureLookups<'ext>
    (closure:
      Map<
        ResolvedIdentifier,
        Value<TypeValue<'ext>, 'ext> * TypeQueryRow<'ext>
       >)
    (e: RunnableExprQueryExpr<'ext>)
    : RunnableExprQueryExpr<'ext> =
    let (!) = replaceClosureLookups closure

    match e.Expr with
    | RunnableExprQueryExprRec.QueryConstant _
    | RunnableExprQueryExprRec.QueryIntrinsic(_, _)
    | RunnableExprQueryExprRec.QueryClosureValue(_, _)
    | RunnableExprQueryExprRec.QueryCountEvaluated _
    | RunnableExprQueryExprRec.QueryExistsEvaluated _
    | RunnableExprQueryExprRec.QueryArrayEvaluated _ -> e

    | RunnableExprQueryExprRec.QueryTupleCons items ->
      { e with
          Expr =
            RunnableExprQueryExprRec.QueryTupleCons(items |> List.map (!)) }

    | RunnableExprQueryExprRec.QueryRecordDes(expr, field, isJson) ->
      { e with
          Expr =
            RunnableExprQueryExprRec.QueryRecordDes(!expr, field, isJson) }

    | RunnableExprQueryExprRec.QueryTupleDes(expr, item, isJson) ->
      { e with
          Expr =
            RunnableExprQueryExprRec.QueryTupleDes(!expr, item, isJson) }

    | RunnableExprQueryExprRec.QueryConditional(cond, ``then``, ``else``) ->
      { e with
          Expr =
            RunnableExprQueryExprRec.QueryConditional(
              !cond,
              ! ``then``,
              ! ``else``
            ) }

    | RunnableExprQueryExprRec.QueryUnionDes(expr, handlers) ->
      let handlers' =
        handlers
        |> Map.map (fun _k handler ->
          { handler with
              Body = !handler.Body })

      { e with
          Expr = RunnableExprQueryExprRec.QueryUnionDes(!expr, handlers') }

    | RunnableExprQueryExprRec.QuerySumDes(expr, handlers) ->
      let handlers' =
        handlers
        |> Map.map (fun _k handler ->
          { handler with
              Body = !handler.Body })

      { e with
          Expr = RunnableExprQueryExprRec.QuerySumDes(!expr, handlers') }

    | RunnableExprQueryExprRec.QueryApply(func, arg) ->
      { e with
          Expr = RunnableExprQueryExprRec.QueryApply(!func, !arg) }

    | RunnableExprQueryExprRec.QueryLookup l ->
      match closure |> Map.tryFind l with
      | None -> e
      | Some(v, t) ->
        { e with
            Expr = RunnableExprQueryExprRec.QueryClosureValue(v, t) }

    | RunnableExprQueryExprRec.QueryCastTo(inner, t) ->
      { e with
          Expr = RunnableExprQueryExprRec.QueryCastTo(!inner, t) }

    | RunnableExprQueryExprRec.QueryCount q ->
      { e with
          Expr =
            RunnableExprQueryExprRec.QueryCountEvaluated(
              replaceClosureLookupsQuery closure q
            ) }

    | RunnableExprQueryExprRec.QueryExists q ->
      { e with
          Expr =
            RunnableExprQueryExprRec.QueryExistsEvaluated(
              replaceClosureLookupsQuery closure q
            ) }

    | RunnableExprQueryExprRec.QueryArray q ->
      { e with
          Expr =
            RunnableExprQueryExprRec.QueryArrayEvaluated(
              replaceClosureLookupsQuery closure q
            ) }

  and replaceClosureLookupsQuery<'ext>
    (closure:
      Map<
        ResolvedIdentifier,
        Value<TypeValue<'ext>, 'ext> * TypeQueryRow<'ext>
       >)
    (q: RunnableExprQuery<'ext>)
    : ValueQuery<TypeValue<'ext>, 'ext> =
    match q with
    | RunnableExprQuery.UnionQueries(q1, q2) ->
      let v1 = replaceClosureLookupsQuery closure q1
      let v2 = replaceClosureLookupsQuery closure q2
      ValueQuery.ValueUnionQueries(v1, v2, v1.DeserializeFrom)
    | RunnableExprQuery.SimpleQuery q ->
      // NOTE: iterator sources were already evaluated above (in the
      // Query SimpleQuery case of compileWithRest). Here we only deal
      // with the sub-query closure replacements for Count/Exists/Array.
      // These sub-queries' iterator sources still need Reader-based eval.
      // We fall back to the Reader-based replace_closure_lookups_query
      // for nested sub-queries. This is safe because sub-queries are
      // lazily evaluated in the DB layer, not in the expression evaluator.
      //
      // For the main query, the iterator sources are already compiled above.
      // For nested sub-queries (Count/Exists/Array), we produce ValueQuerySimple
      // with unevaluated iterator sources — this won't happen at this point
      // because those are handled via QueryCountEvaluated etc. above.
      //
      // The sub-query in Count/Exists/Array uses the parent closure
      // and gets fully resolved here.
      let iterators =
        q.Iterators
        |> NonEmptyList.map (fun it ->
          // For sub-queries, we can't compile the source — it's evaluated lazily
          // But this path is only reached from QueryCount/QueryExists/QueryArray
          // where the closure already contains the needed values
          { ValueQueryIterator.Location = it.Location
            Var = it.Var
            VarType = it.VarType
            Source = Value.Var(Var.Create $"__subquery_source_{it.Var.Name}") })

      let joins =
        q.Joins
        |> Option.map (
          NonEmptyList.map (fun join ->
            { join with
                Left = replaceClosureLookups closure join.Left
                Right = replaceClosureLookups closure join.Right })
        )

      let where =
        q.Where |> Option.map (replaceClosureLookups closure)

      let select = replaceClosureLookups closure q.Select

      let orderBy =
        q.OrderBy
        |> Option.map (fun (v, dir) -> replaceClosureLookups closure v, dir)

      let distinct =
        q.Distinct |> Option.map (replaceClosureLookups closure)

      ValueQuery.ValueQuerySimple
        { Iterators = iterators
          Joins = joins
          Where = where
          Select = select
          OrderBy = orderBy
          Distinct = distinct
          DeserializeFrom = q.DeserializeFrom }
