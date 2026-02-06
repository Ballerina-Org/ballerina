namespace Ballerina.DSL.FormBuilder.Types

module TypeChecker =
  open Ballerina
  open Ballerina.Collections.Sum
  open Ballerina.DSL.Next.Types
  open Ballerina.DSL.Next.Types.Patterns
  open Ballerina.State.WithError
  open Ballerina.DSL.Next.Unification
  open Ballerina.LocalizedErrors
  open Ballerina.Errors
  open Ballerina.DSL.Next.Types.TypeChecker.Model
  open Ballerina.Cat.Collections.OrderedMap
  open Ballerina.Collections.NonEmptyList
  open Ballerina.DSL.Next.StdLib.Extensions
  open Ballerina.DSL.FormBuilder.Model.FormAST
  open Ballerina.DSL.Next.Types.TypeChecker.LiftOtherSteps
  open Ballerina.DSL.Next.Extensions
  open Ballerina.DSL.Next.StdLib.List.Model

  let makeListType (stdExtensions: StdExtensions<'valueExt>) (typeArgument: TypeValue<'valueExt>) =
    { ImportedTypeValue.Id = stdExtensions.List.TypeName |> fst
      Sym = stdExtensions.List.TypeName |> snd
      Parameters =
        stdExtensions.List.TypeVars
        |> List.map (fun (v, k) -> TypeParameter.Create(v.Name, k))
      Arguments = [ typeArgument ]
      UnionLike = None
      RecordLike = None }

  let makeMapType
    (stdExtensions: StdExtensions<'valueExt>)
    (keyType: TypeValue<'valueExt>)
    (valueType: TypeValue<'valueExt>)
    =
    { ImportedTypeValue.Id = stdExtensions.Map.TypeName |> fst
      Sym = stdExtensions.Map.TypeName |> snd
      Parameters =
        stdExtensions.Map.TypeVars
        |> List.map (fun (v, k) -> TypeParameter.Create(v.Name, k))
      Arguments = [ keyType; valueType ]
      UnionLike = None
      RecordLike = None }

  let assertType (typeAssert: TypeValue<'valueExt> -> Sum<'t, Errors<Unit>>) (typeValue: TypeValue<'valueExt>) =
    typeAssert typeValue
    |> state.OfSum
    |> State.mapError (Errors.MapContext(replaceWith Location.Unknown))

  let assertList (stdExtensions: StdExtensions<'valueExt>) (targetType: TypeValue<'valueExt>) =
    state {
      let! importedType = assertType TypeValue.AsImported targetType

      if importedType.Id = (stdExtensions.List.TypeName |> fst) then
        match importedType.Arguments with
        | [ listArg ] -> return listArg
        | _ ->
          return!
            state.Throw(
              Errors.Singleton Location.Unknown (fun () ->
                $"Expected one type argument for list but {importedType.Arguments.Length} were given.")

            )
      else
        return!
          state.Throw(
            Errors.Singleton Location.Unknown (fun () -> $"Expected list but {importedType.Id} was given.")
            |> Errors.MapPriority(replaceWith ErrorPriority.High)
          )
    }

  let assertMap (stdExtensions: StdExtensions<'valueExt>) (targetType: TypeValue<'valueExt>) =
    state {
      let! importedType = assertType TypeValue.AsImported targetType

      if importedType.Id = (stdExtensions.Map.TypeName |> fst) then
        match importedType.Arguments with
        | [ keyType; valueType ] -> return keyType, valueType
        | _ ->
          return!
            state.Throw(
              Errors.Singleton Location.Unknown (fun () ->
                $"Expected two type arguments for map but {importedType.Arguments.Length} were given.")
            )
      else
        return!
          state.Throw(
            Errors.Singleton Location.Unknown (fun () -> $"Expected map but {importedType.Id} was given.")
            |> Errors.MapPriority(replaceWith ErrorPriority.High)
          )
    }

  type FormTypeCheckerState<'valueExt when 'valueExt: comparison> =
    { BallerinaTypeCheckState: TypeCheckState<'valueExt>
      FormTypes: Map<FormIdentifier, TypeValue<'valueExt>> }

    static member Init typeCheckState =
      { BallerinaTypeCheckState = typeCheckState
        FormTypes = Map.empty }

  type FormTypeCheckingContext<'valueExt when 'valueExt: comparison> =
    { Types: Map<string, TypeValue<'valueExt>>
      TypeCheckContext: TypeCheckContext<'valueExt>
      APIContext: Map<TypeValue<'valueExt>, string * TypeValue<'valueExt>> }

    static member Init types typeCheckContext apiContext =
      { Types = types
        TypeCheckContext = typeCheckContext
        APIContext = apiContext }

    static member FindTargetEntityKeyType
      (typeValue: TypeValue<'valueExt>)
      (context: FormTypeCheckingContext<'valueExt>)
      =
      context.APIContext
      |> Map.tryFind typeValue
      |> Sum.fromOption (fun () ->
        Errors.Singleton Location.Unknown (fun () ->
          $"It was not possible to find the key type {typeValue} in the api context.")
        |> Errors.MapPriority(replaceWith ErrorPriority.High))

  let runUnification
    (unificationState: State<Unit, UnificationContext<'valueExt>, UnificationState<'valueExt>, Errors<Location>>)
    : State<Unit, FormTypeCheckingContext<'valueExt>, FormTypeCheckerState<'valueExt>, Errors<Location>> =
    unificationState
    |> Expr<'T, 'Id, 'valueExt>.liftUnification
    |> State.mapState
      (fun (typeCheckState, _) -> typeCheckState.BallerinaTypeCheckState)
      (fun (unificationState, _) typeCheckState ->
        { typeCheckState with
            BallerinaTypeCheckState = unificationState })
    |> State.mapContext (fun typeCheckContext -> typeCheckContext.TypeCheckContext)

  type FormTypeCheckingError =
    { FormTypeCheckErrors: List<string> }

    static member Concat(err1: FormTypeCheckingError, err2: FormTypeCheckingError) =
      { FormTypeCheckErrors =
          List.concat (
            seq {
              err1.FormTypeCheckErrors
              err2.FormTypeCheckErrors
            }
          ) }

  let unifyPrimitive (targetType: TypeValue<'valueExt>) (rendererType: TypeValue<'valueExt>) =
    state {
      do! TypeValue.Unify(Location.Unknown, targetType, rendererType) |> runUnification
      return targetType
    }

  let checkPrimitive (targetType: TypeValue<'valueExt>) (primitive: PrimitiveRendererKind) =
    state {
      match primitive with
      | PrimitiveRendererKind.String
      | PrimitiveRendererKind.StringId
      | PrimitiveRendererKind.Base64
      | PrimitiveRendererKind.Secret ->
        return! unifyPrimitive targetType (TypeValue.CreatePrimitive PrimitiveType.String)
      | PrimitiveRendererKind.Int32 -> return! unifyPrimitive targetType (TypeValue.CreatePrimitive PrimitiveType.Int32)
      | PrimitiveRendererKind.Int64 -> return! unifyPrimitive targetType (TypeValue.CreatePrimitive PrimitiveType.Int64)
      | PrimitiveRendererKind.Float32 ->
        return! unifyPrimitive targetType (TypeValue.CreatePrimitive PrimitiveType.Float32)
      | PrimitiveRendererKind.Float ->
        return! unifyPrimitive targetType (TypeValue.CreatePrimitive PrimitiveType.Float64)
      | PrimitiveRendererKind.Date ->
        return! unifyPrimitive targetType (TypeValue.CreatePrimitive PrimitiveType.DateTime)
      | PrimitiveRendererKind.DateOnly ->
        return! unifyPrimitive targetType (TypeValue.CreatePrimitive PrimitiveType.DateOnly)
      | PrimitiveRendererKind.Bool -> return! unifyPrimitive targetType (TypeValue.CreatePrimitive PrimitiveType.Bool)
      | PrimitiveRendererKind.Guid -> return! unifyPrimitive targetType (TypeValue.CreatePrimitive PrimitiveType.Guid)
      | PrimitiveRendererKind.Unit -> return! unifyPrimitive targetType (TypeValue.CreatePrimitive PrimitiveType.Unit)
    }

  let rec checkTuple
    (stdExtensions: StdExtensions<'valueExt>)
    (targetType: TypeValue<'valueExt>)
    (tupleRenderer: TupleRenderer<Unchecked>)
    =
    state {
      let! tupleTargetItemTypes = assertType TypeValue.AsTuple targetType

      if tupleTargetItemTypes.Length <> tupleRenderer.Items.Length then
        return!
          state.Throw(
            Errors.Singleton Location.Unknown (fun () ->
              $"Tuple items mismatch: expected {tupleTargetItemTypes.Length} but {tupleRenderer.Items.Length} were given.")

            |> Errors.MapPriority(replaceWith ErrorPriority.High)
          )
      else
        let! itemRenderers =
          state.All(
            List.zip tupleTargetItemTypes tupleRenderer.Items
            |> List.map (fun (targetItemType, renderer) -> checkRenderer stdExtensions targetItemType renderer)
          )

        let rendererTupleType =
          TypeValue.CreateTuple(itemRenderers |> List.map (fun renderer -> renderer.Type))

        do!
          TypeValue.Unify(Location.Unknown, targetType, rendererTupleType)
          |> runUnification

        return
          { Tuple = tupleRenderer.Tuple
            Items = itemRenderers
            Type = targetType }
    }

  and unionCaseMismatchError (expectedCases: int) (givenCases: int) =
    state.Throw(
      Errors.Singleton Location.Unknown (fun () -> $"Expected {expectedCases} cases but {givenCases} given.")
      |> Errors.MapPriority(replaceWith ErrorPriority.High)
    )

  and checkSum
    (stdExtensions: StdExtensions<'valueExt>)
    (targetType: TypeValue<'valueExt>)
    (sumRenderer: SumRenderer<Unchecked>)
    =
    state {
      let! sumTargetType = assertType TypeValue.AsSum targetType

      match sumTargetType with
      | firstCaseType :: [ secondCaseType ] ->
        let! leftRendererTypedExpr = checkRenderer stdExtensions firstCaseType sumRenderer.Left
        let! rightRendererTypedExpr = checkRenderer stdExtensions secondCaseType sumRenderer.Right

        do!
          TypeValue.Unify(
            Location.Unknown,
            targetType,
            TypeValue.CreateSum [ leftRendererTypedExpr.Type; rightRendererTypedExpr.Type ]
          )
          |> runUnification

        return
          { Sum = sumRenderer.Sum
            Left = leftRendererTypedExpr
            Right = rightRendererTypedExpr
            Type = targetType }
      | _ -> return! unionCaseMismatchError 2 sumTargetType.Length
    }

  and checkUnion
    (stdExtensions: StdExtensions<'valueExt>)
    (targetType: TypeValue<'valueExt>)
    (unionRenderer: UnionRenderer<Unchecked>)
    =
    state {
      let! _, targetCases = assertType TypeValue.AsUnion targetType

      let! casesTypedExprs =
        unionRenderer.Cases
        |> Map.toList
        |> List.map (fun (CaseIdentifier case, renderer) ->
          state {
            match
              targetCases.data
              |> Map.tryFindKey (fun targetCase _ -> targetCase.Name = LocalScope case)
            with
            | Some targetCase ->
              let targetCaseType = targetCases.data.[targetCase]
              let! rendererType = checkRenderer stdExtensions targetCaseType renderer
              return targetCase, rendererType, CaseIdentifier case
            | None ->
              return!
                state.Throw(
                  Errors.Singleton Location.Unknown (fun () -> $"Cannot find union case {case}.")
                  |> Errors.MapPriority(replaceWith ErrorPriority.High)
                )
          })
        |> state.All

      let unionRendererType =
        TypeValue.CreateUnion(
          casesTypedExprs
          |> List.map (fun (symbol, renderer, _) -> symbol, renderer.Type)
          |> OrderedMap.ofList
        )

      do!
        TypeValue.Unify(Location.Unknown, targetType, unionRendererType)
        |> runUnification

      return
        { Union = unionRenderer.Union
          Cases = casesTypedExprs |> List.map (fun (_, expr, case) -> case, expr) |> Map.ofList
          Type = targetType }
    }

  (*
    Type checking one and many requires a Map<TypeValue, string * TypeValue> in the context: the key is the type of the key of the target entity, 
    the string is the name of the API, while the last type value is the type of the target entity. This will be passed as input to the form compiler. 
    The type checker will look up for the key type in the map, then it will check if the api name matches, and then it will try to type check the renderer 
    expression against the target type value. The schema entity needs to contain a field whose type is the same TypeValue as the key in the map.

    EXAMPLE:
    
    type MyKey = {
      Key: guid;
    }

    in type MyEntity = {
      A: int32;
      B: string;
      C: MyKey
    }

    and the corresponding form is:

    form MyEntityForm(myEntityFormRenderer) : MyEntity {
      A: int32(defaultInt);
      B: string(defaultString);
      C: one(oneRenderer) from myApi 
        details ...//here goes the details renderer expression
        ;
    }

    When type checking the field C, the type checker will look for entry in the map where the key is a Record type "MyKey", then it will check that the
    corresponding entry contains "myApi" as api name and that the target api value is compatible with the renderer expression of the details (and the same 
    for the optional preview).

    As for many, it behaves in the same way except it will expect List [MyKey] as type of the field, so the lookup will use the type argument of the list 
    instead of directly using the type value of the field so:
      - check that the target type is a list.
      - check that the type argument of the list exists as a key in the map.
      - check that the api name matches the provided one.
      - check that the type value in the map is compatible with the renderer expression.

  *)

  and checkApi (apiName: string) (keyType: TypeValue<'valueExt>) =
    state {
      let! context = state.GetContext()
      let! api, linkedEntityType = FormTypeCheckingContext.FindTargetEntityKeyType keyType context |> state.OfSum

      if api <> apiName then
        return!
          state.Throw(
            Errors.Singleton Location.Unknown (fun () -> $"Undefined api {apiName}.")
            |> Errors.MapPriority(replaceWith ErrorPriority.High)
          )

      return linkedEntityType
    }

  and checkOne
    (stdExtensions: StdExtensions<'valueExt>)
    (targetType: TypeValue<'valueExt>)
    (oneRenderer: OneRenderer<Unchecked>)
    : State<
        OneRenderer<TypeValue<'valueExt>>,
        FormTypeCheckingContext<'valueExt>,
        FormTypeCheckerState<'valueExt>,
        Errors<Location>
       >
    =
    state {
      let (ApiIdentifier apiName) = oneRenderer.Api
      let! linkedEntityType = checkApi apiName targetType

      let! typedPreviewExpression =
        state {
          match oneRenderer.Preview with
          | None -> return None
          | Some expr -> return! checkRenderer stdExtensions linkedEntityType expr |> state.Map Some
        }

      let! typedDetailsExpression = checkRenderer stdExtensions linkedEntityType oneRenderer.Details

      return
        { Api = oneRenderer.Api
          Details = typedDetailsExpression
          Preview = typedPreviewExpression
          One = oneRenderer.One
          Type = targetType }
    }

  and checkMany
    (stdExtensions: StdExtensions<'valueExt>)
    (targetType: TypeValue<'valueExt>)
    (manyRenderer: ManyRenderer<Unchecked>)
    =
    state {
      let! keyListType = assertList stdExtensions targetType
      let (ApiIdentifier apiName) = manyRenderer.Api
      let! linkedEntityType = checkApi apiName keyListType

      let! typedBody =
        state {
          match manyRenderer.Body with
          | LinkedUnlinked linkedUnlinked ->
            let! linkedTypedExpr = checkRenderer stdExtensions linkedEntityType linkedUnlinked.Linked

            let! unlinkedTypedExpr =
              state {
                match linkedUnlinked.Unlinked with
                | None -> return None
                | Some unlinked -> return! checkRenderer stdExtensions linkedEntityType unlinked |> state.Map Some
              }

            return
              LinkedUnlinked
                { Linked = linkedTypedExpr
                  Unlinked = unlinkedTypedExpr }
          | Element renderer ->
            let! typedRendererExpr = checkRenderer stdExtensions linkedEntityType renderer
            return Element typedRendererExpr
        }

      return
        { Api = manyRenderer.Api
          Body = typedBody
          Many = manyRenderer.Many
          Type = targetType }
    }

  (*
    Type checking a list requires to verify that the target type is an imported type, since lists are not first-class citizens in ballerina-lang, rather they 
    come from a standard library. We then check that the provided imported type is a list, and that it contains exactly one type argument. If that succeeds, 
    we then type check the type argument of the given list in ballerina-lang agains the renderer expression of the list renderer. We then create a list type with 
    the type of the renderer expression and unify it with the target type.
  *)
  and checkList
    (stdExtensions: StdExtensions<'valueExt>)
    (targetType: TypeValue<'valueExt>)
    (listRenderer: ListRenderer<Unchecked>)
    =
    state {
      let! listArgType = assertList stdExtensions targetType
      let! rendererElementTypedExpr = checkRenderer stdExtensions listArgType listRenderer.Element

      do!
        TypeValue.Unify(
          Location.Unknown,
          targetType,
          makeListType stdExtensions rendererElementTypedExpr.Type
          |> TypeValue.CreateImported
        )
        |> runUnification

      return
        { Element = rendererElementTypedExpr
          List = listRenderer.List
          Type = targetType }
    }

  and checkMap
    (stdExtensions: StdExtensions<'valueExt>)
    (targetType: TypeValue<'valueExt>)
    (mapRenderer: MapRenderer<Unchecked>)
    =
    state {
      let! keyType, valueType = assertMap stdExtensions targetType
      let! keyExpr = checkRenderer stdExtensions keyType mapRenderer.Key
      let! valueExpr = checkRenderer stdExtensions valueType mapRenderer.Value

      do!
        TypeValue.Unify(
          Location.Unknown,
          targetType,
          makeMapType stdExtensions keyExpr.Type valueExpr.Type
          |> TypeValue.CreateImported
        )
        |> runUnification

      return
        { Map = mapRenderer.Map
          Key = keyExpr
          Value = valueExpr
          Type = targetType }
    }

  and checkReadonly
    (stdExtensions: StdExtensions<'valueExt>)
    (targetType: TypeValue<'valueExt>)
    (readonlyRenderer: ReadonlyRenderer<Unchecked>)
    =
    state {
      let! rendererTypedExpr = checkRenderer stdExtensions targetType readonlyRenderer.Value

      return
        { Readonly = readonlyRenderer.Readonly
          Type = targetType
          Value = rendererTypedExpr }
    }


  and checkFormByReference
    (formIdentifier: FormIdentifier)
    : State<TypeValue<'valueExt>, FormTypeCheckingContext<'valueExt>, FormTypeCheckerState<'valueExt>, Errors<Location>> =
    state {
      match!
        state.GetState()
        |> state.Map(fun state -> state.FormTypes |> Map.tryFind formIdentifier)
      with
      | Some typeValue -> return typeValue
      | None ->
        let (FormIdentifier formName) = formIdentifier

        return!
          state.Throw(
            Errors.Singleton Location.Unknown (fun () -> $"Undefined form {formName}")
            |> Errors.MapPriority(replaceWith ErrorPriority.High)
          )
    }

  //check that the field exists for the given type
  and findFieldType
    (FormIdentifier formName: FormIdentifier)
    (fieldTypes: OrderedMap<TypeSymbol, TypeValue<'valueExt> * Kind>)
    (fieldIdentifier: FieldIdentifier)
    =
    state {
      let (FieldIdentifier fieldName) = fieldIdentifier

      match
        fieldTypes.data
        |> Map.tryFindKey (fun typeSymbol _ ->
          match typeSymbol.Name with
          | LocalScope id -> id = fieldName
          | _ -> false)
      with
      | None ->
        return!
          state.Throw(
            Errors.Singleton Location.Unknown (fun () -> $"The field {fieldName} cannot be found in type {formName}")
            |> Errors.MapPriority(replaceWith ErrorPriority.High)
          )
      | Some fieldSymbol ->
        let fieldType, _ = fieldTypes.data.[fieldSymbol]
        return fieldType
    }

  and checkField
    (stdExtensions: StdExtensions<'valueExt>)
    (formIdentifier: FormIdentifier)
    (fieldTypes: OrderedMap<TypeSymbol, TypeValue<'valueExt> * Kind>)
    (field: Field<Unchecked>)
    : State<
        Field<TypeValue<'valueExt>>,
        FormTypeCheckingContext<'valueExt>,
        FormTypeCheckerState<'valueExt>,
        Errors<Location>
       >
    =
    state {
      let! fieldType = findFieldType formIdentifier fieldTypes field.Name
      let! fieldRendererTypedExpr = checkRenderer stdExtensions fieldType field.Renderer

      do!
        TypeValue.Unify(Location.Unknown, fieldType, fieldRendererTypedExpr.Type)
        |> runUnification

      return
        { Details = field.Details
          Label = field.Label
          Name = field.Name
          Renderer = fieldRendererTypedExpr
          Tooltip = field.Tooltip
          Type = fieldType }
    }

  and checkTab (formId: FormIdentifier) (fieldTypes: OrderedMap<TypeSymbol, TypeValue<'valueExt> * Kind>) (tab: Tab) =
    let checkAllTabFields =
      [ for _, column in tab.Columns |> Map.toList do
          for _, group in column.Groups |> Map.toList do
            let checkGroupFields =
              seq {
                for fieldId in group do
                  yield findFieldType formId fieldTypes fieldId
              }

            yield! checkGroupFields ]

    checkAllTabFields |> state.All |> state.Ignore

  and checkMembers
    (stdExtensions: StdExtensions<'valueExt>)
    (formIdentifier: FormIdentifier)
    (fieldTypes: OrderedMap<TypeSymbol, TypeValue<'valueExt> * Kind>)
    (members: Members<Unchecked>)
    =
    state {
      let! fieldWithTypes =
        members.Fields
        |> Map.map (fun _ field -> checkField stdExtensions formIdentifier fieldTypes field)
        |> Map.values
        |> state.All

      do!
        members.Tabs
        |> Map.map (fun _ tab -> checkTab formIdentifier fieldTypes tab)
        |> Map.values
        |> state.All
        |> state.Ignore

      return
        { Fields = fieldWithTypes |> List.map (fun field -> field.Name, field) |> Map.ofList
          Tabs = members.Tabs }
    }

  and checkDisabledFields
    (formIdentifier: FormIdentifier)
    (fieldTypes: OrderedMap<TypeSymbol, TypeValue<'valueExt> * Kind>)
    (disabledFields: Set<FieldIdentifier>)
    =
    disabledFields
    |> Set.toList
    |> List.map (fun field -> findFieldType formIdentifier fieldTypes field)
    |> state.All
    |> state.Ignore

  and checkHighlights
    (formIdentifier: FormIdentifier)
    (fieldTypes: OrderedMap<TypeSymbol, TypeValue<'valueExt> * Kind>)
    (highlights: Set<FieldIdentifier>)
    =
    highlights
    |> Set.toList
    |> List.map (fun field -> findFieldType formIdentifier fieldTypes field)
    |> state.All
    |> state.Ignore

  and checkFormBody
    (stdExtensions: StdExtensions<'valueExt>)
    (formId: FormIdentifier)
    (fieldTypes: OrderedMap<TypeSymbol, TypeValue<'valueExt> * Kind>)
    (formBody: FormBody<Unchecked>)
    =
    state {
      let! checkedMembers = checkMembers stdExtensions formId fieldTypes formBody.Members
      do! checkDisabledFields formId fieldTypes formBody.DisabledFields
      do! checkHighlights formId fieldTypes formBody.Highlights

      let! detailsTypedExpr =
        state {
          match formBody.Details with
          | None -> return None
          | Some details ->
            let! detailsExpr = checkRenderer stdExtensions (TypeValue.CreateRecord fieldTypes) details

            do!
              TypeValue.Unify(Location.Unknown, TypeValue.CreateRecord fieldTypes, detailsExpr.Type)
              |> runUnification

            return Some detailsExpr
        }

      return
        { Details = detailsTypedExpr
          DisabledFields = formBody.DisabledFields
          Highlights = formBody.Highlights
          Members = checkedMembers }
    }

  and expectedFieldRecordTypeError (fieldId: string) =
    state.Throw(
      Errors.Singleton Location.Unknown (fun () -> $"Expected record type for field {fieldId}")
      |> Errors.MapPriority(replaceWith ErrorPriority.High)
    )

  and checkRecord
    (stdExtensions: StdExtensions<'valueExt>)
    (targetType: TypeValue<'valueExt>)
    (recordRenderer: RecordRenderer<Unchecked>)
    =
    state {
      let! recordTargetType = assertType TypeValue.AsRecord targetType
      let anonymousFormId = FormIdentifier "anonymous record"
      do! checkDisabledFields anonymousFormId recordTargetType recordRenderer.DisabledFields
      let! checkedMembers = checkMembers stdExtensions anonymousFormId recordTargetType recordRenderer.Members

      return
        { DisabledFields = recordRenderer.DisabledFields
          Members = checkedMembers
          Renderer = recordRenderer.Renderer
          Type = targetType }
    }

  and checkInlineForm
    (stdExtensions: StdExtensions<'valueExt>)
    (targetType: TypeValue<'valueExt>)
    (inlineFormRenderer: InlineFormRenderer<Unchecked>)
    =
    state {
      let! recordTargetType = assertType TypeValue.AsRecord targetType
      let formIdentifier = FormIdentifier "anonymous form"
      let! typedFormBody = checkFormBody stdExtensions formIdentifier recordTargetType inlineFormRenderer.Body

      return
        { Body = typedFormBody
          InlineForm = inlineFormRenderer.InlineForm
          Type = targetType }
    }


  and checkRenderer
    (stdExtensions: StdExtensions<'valueExt>)
    (targetType: TypeValue<'valueExt>)
    (renderer: RendererExpression<Unchecked>)
    : State<
        RendererExpression<TypeValue<'valueExt>>,
        FormTypeCheckingContext<'valueExt>,
        FormTypeCheckerState<'valueExt>,
        Errors<Location>
       >
    =
    state {
      match renderer with
      | RendererExpression.Primitive primitive ->
        return!
          state {
            let! primitiveType = checkPrimitive targetType primitive.Renderer

            return
              RendererExpression.Primitive
                { Primitive = primitive.Primitive
                  Renderer = primitive.Renderer
                  Type = primitiveType }
          }
      | Map map -> return! checkMap stdExtensions targetType map |> state.Map Map
      | RendererExpression.Tuple tuple ->
        return! checkTuple stdExtensions targetType tuple |> state.Map RendererExpression.Tuple
      | RendererExpression.List list ->
        return! checkList stdExtensions targetType list |> state.Map RendererExpression.List
      | Readonly readonly -> return! checkReadonly stdExtensions targetType readonly |> state.Map Readonly
      | RendererExpression.Sum sum -> return! checkSum stdExtensions targetType sum |> state.Map RendererExpression.Sum
      | Union union -> return! checkUnion stdExtensions targetType union |> state.Map Union
      | RendererExpression.Record record ->
        return!
          checkRecord stdExtensions targetType record
          |> state.Map RendererExpression.Record
      | Form(formId as FormIdentifier formName, Unchecked) ->
        match!
          state.GetState()
          |> state.Map(fun state -> state.FormTypes |> Map.tryFind formId)
        with
        | None ->
          return!
            state.Throw(
              Errors.Singleton Location.Unknown (fun () -> $"Undefined form {formName}")
              |> Errors.MapPriority(replaceWith ErrorPriority.High)
            )
        | Some formType -> return Form(formId, formType)
      | InlineForm inlineform -> return! checkInlineForm stdExtensions targetType inlineform |> state.Map InlineForm
      | _ -> return! state.Throw(Errors.Singleton Location.Unknown (fun () -> $"Unsupported renderer {renderer}."))
    }

  let checkForm
    (form: Form<Unchecked>)
    (stdExtensions: StdExtensions<'valueExt>)
    : State<
        Form<TypeValue<'valueExt>>,
        FormTypeCheckingContext<'valueExt>,
        FormTypeCheckerState<'valueExt>,
        Errors<Location>
       >
    =
    state {
      let (TypeIdentifier formTypeId) = form.TypeIdentifier

      match!
        state.GetContext()
        |> state.Map(fun ctxt -> ctxt.Types |> Map.tryFind formTypeId)
      with
      | None ->
        return!
          state.Throw(
            Errors.Singleton Location.Unknown (fun () -> $"Type {formTypeId} is undefined.")
            |> Errors.MapPriority(replaceWith ErrorPriority.High)
          )
      | Some formType ->
        let! recordType = assertType TypeValue.AsRecord formType
        let! typeCheckedBody = checkFormBody stdExtensions form.Form recordType form.Body

        let typeCheckedForm =
          { Body = typeCheckedBody
            Form = form.Form
            IsEntryPoint = form.IsEntryPoint
            RendererId = form.RendererId
            Type = formType
            TypeIdentifier = form.TypeIdentifier }

        do!
          state.SetState(fun state ->
            { state with
                FormTypes = state.FormTypes.Add(typeCheckedForm.Form, typeCheckedForm.Type) })

        return typeCheckedForm
    }

  let checkFormDefinitions (formDefinition: FormDefinitions<Unchecked>) (stdExtensions: StdExtensions<'valueExt>) =
    state {
      let! checkedForms =
        state.All(
          formDefinition.Forms
          |> OrderedMap.values
          |> List.map (fun form -> checkForm form stdExtensions)
        )

      let checkedFormMap =
        [ for form in checkedForms do
            yield form.Form, form ]
        |> OrderedMap.ofList

      return { Forms = checkedFormMap }
    }
