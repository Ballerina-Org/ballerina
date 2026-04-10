namespace Ballerina.DSL.Next.StdLib.Int32

[<AutoOpen>]
module Extension =
  open Ballerina
  open Ballerina.Collections.Sum
  open Ballerina.Reader.WithError
  open Ballerina.LocalizedErrors
  open Ballerina.Errors
  open Ballerina.DSL.Next.Terms.Model
  open Ballerina.DSL.Next.Terms.Patterns
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Types.Patterns
  open Ballerina.Lenses
  open Ballerina.DSL.Next.Extensions

  let Int32Extension<'runtimeContext, 'ext>
    (operationLens: PartialLens<'ext, Int32Operations<'ext>>)
    : OperationsExtension<'runtimeContext, 'ext, Int32Operations<'ext>> =

    let boolTypeValue = TypeValue.CreatePrimitive PrimitiveType.Bool
    let int32TypeValue = TypeValue.CreatePrimitive PrimitiveType.Int32
    let stringTypeValue = TypeValue.CreatePrimitive PrimitiveType.String

    let int32ToStringId =
      Identifier.FullyQualified([ "int32" ], "toString")
      |> TypeCheckScope.Empty.Resolve

    let toStringOperation
      : ResolvedIdentifier *
        OperationExtension<'runtimeContext, 'ext, Int32Operations<'ext>> =
      int32ToStringId,
      { PublicIdentifiers =
          Some
          <| (TypeValue.CreateArrow(int32TypeValue, stringTypeValue),
              Kind.Star,
              Int32Operations.String)
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | Int32Operations.String -> Some(Int32Operations.String)
            | _ -> None)
        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              do!
                op
                |> Int32Operations.AsToString
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! v =
                v
                |> Value.AsPrimitive
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! v =
                v
                |> PrimitiveValue.AsInt32
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              return
                Value<TypeValue<'ext>, 'ext>
                  .Primitive(PrimitiveValue.String(v |> string))
            } }

    let int32TryParseId =
      Identifier.FullyQualified([ "int32" ], "tryParse")
      |> TypeCheckScope.Empty.Resolve

    let tryParseOperation
      : ResolvedIdentifier *
        OperationExtension<'runtimeContext, 'ext, Int32Operations<'ext>> =
      int32TryParseId,
      { PublicIdentifiers =
          Some
          <| (TypeValue.CreateArrow(
                stringTypeValue,
                TypeValue.CreateSum
                  [ TypeValue.CreatePrimitive PrimitiveType.Unit
                    TypeValue.CreatePrimitive PrimitiveType.Int32 ]
              ),
              Kind.Star,
              Int32Operations.TryParse)
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | Int32Operations.TryParse -> Some(Int32Operations.TryParse)
            | _ -> None)
        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              do!
                op
                |> Int32Operations.AsTryParse
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! v =
                v
                |> Value.AsPrimitive
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! v =
                v
                |> PrimitiveValue.AsString
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              return
                match System.Int32.TryParse(v) with
                | true, result ->
                  Value.Sum(
                    { Case = 2; Count = 2 },
                    Value.Primitive(PrimitiveValue.Int32 result)
                  )
                | false, _ ->
                  Value.Sum(
                    { Case = 1; Count = 2 },
                    Value.Primitive(PrimitiveValue.Unit)
                  )
            } }

    let int32PlusId =
      Identifier.FullyQualified([ "int32" ], "+")
      |> TypeCheckScope.Empty.Resolve

    let plusOperation
      : ResolvedIdentifier *
        OperationExtension<'runtimeContext, 'ext, Int32Operations<'ext>> =
      int32PlusId,
      { PublicIdentifiers =
          Some
          <| (TypeValue.CreateArrow(
                int32TypeValue,
                TypeValue.CreateArrow(int32TypeValue, int32TypeValue)
              ),
              Kind.Star,
              Int32Operations.Plus {| v1 = None |})
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | Int32Operations.Plus v -> Some(Int32Operations.Plus v)
            | _ -> None)
        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              let! op =
                op
                |> Int32Operations.AsPlus
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! v =
                v
                |> Value.AsPrimitive
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! v =
                v
                |> PrimitiveValue.AsInt32
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return
                  (Int32Operations.Plus({| v1 = Some v |}) |> operationLens.Set,
                   Some int32PlusId)
                  |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return
                  Value<TypeValue<'ext>, 'ext>
                    .Primitive(PrimitiveValue.Int32(vClosure + v))
            } }

    let int32MinusId =
      Identifier.FullyQualified([ "int32" ], "-")
      |> TypeCheckScope.Empty.Resolve

    let minusOperation
      : ResolvedIdentifier *
        OperationExtension<'runtimeContext, 'ext, Int32Operations<'ext>> =
      int32MinusId,
      { PublicIdentifiers =
          Some
          <| (TypeValue.CreateArrow(
                int32TypeValue,
                TypeValue.CreateArrow(int32TypeValue, int32TypeValue)
              ),
              Kind.Star,
              Int32Operations.Minus {| v1 = None |})
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | Int32Operations.Minus v -> Some(Int32Operations.Minus v)
            | _ -> None)
        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              let! op =
                op
                |> Int32Operations.AsMinus
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! v =
                v
                |> Value.AsPrimitive
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! v =
                v
                |> PrimitiveValue.AsInt32
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return
                  (Int32Operations.Minus({| v1 = Some v |}) |> operationLens.Set,
                   Some int32MinusId)
                  |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return
                  Value<TypeValue<'ext>, 'ext>
                    .Primitive(PrimitiveValue.Int32(vClosure - v))
            } }


    let int32TimesId =
      Identifier.FullyQualified([ "int32" ], "*")
      |> TypeCheckScope.Empty.Resolve

    let timesOperation
      : ResolvedIdentifier *
        OperationExtension<'runtimeContext, 'ext, Int32Operations<'ext>> =
      int32TimesId,
      { PublicIdentifiers =
          Some
          <| (TypeValue.CreateArrow(
                int32TypeValue,
                TypeValue.CreateArrow(int32TypeValue, int32TypeValue)
              ),
              Kind.Star,
              Int32Operations.Times {| v1 = None |})
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | Int32Operations.Times v -> Some(Int32Operations.Times v)
            | _ -> None)
        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              let! op =
                op
                |> Int32Operations.AsTimes
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! v =
                v
                |> Value.AsPrimitive
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! v =
                v
                |> PrimitiveValue.AsInt32
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return
                  (Int32Operations.Times({| v1 = Some v |}) |> operationLens.Set,
                   Some int32TimesId)
                  |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return
                  Value<TypeValue<'ext>, 'ext>
                    .Primitive(PrimitiveValue.Int32(vClosure * v))
            } }


    let int32DivideId =
      Identifier.FullyQualified([ "int32" ], "/")
      |> TypeCheckScope.Empty.Resolve

    let divideOperation
      : ResolvedIdentifier *
        OperationExtension<'runtimeContext, 'ext, Int32Operations<'ext>> =
      int32DivideId,
      { PublicIdentifiers =
          Some
          <| (TypeValue.CreateArrow(
                int32TypeValue,
                TypeValue.CreateArrow(int32TypeValue, int32TypeValue)
              ),
              Kind.Star,
              Int32Operations.Divide {| v1 = None |})
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | Int32Operations.Divide v -> Some(Int32Operations.Divide v)
            | _ -> None)
        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              let! op =
                op
                |> Int32Operations.AsDivide
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! v =
                v
                |> Value.AsPrimitive
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! v =
                v
                |> PrimitiveValue.AsInt32
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return
                  (Int32Operations.Divide({| v1 = Some v |})
                   |> operationLens.Set,
                   Some int32DivideId)
                  |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return
                  Value<TypeValue<'ext>, 'ext>
                    .Primitive(PrimitiveValue.Int32(vClosure / v))
            } }

    let int32PowerId =
      Identifier.FullyQualified([ "int32" ], "**")
      |> TypeCheckScope.Empty.Resolve

    let powerOperation
      : ResolvedIdentifier *
        OperationExtension<'runtimeContext, 'ext, Int32Operations<'ext>> =
      int32PowerId,
      { PublicIdentifiers =
          Some
          <| (TypeValue.CreateArrow(
                int32TypeValue,
                TypeValue.CreateArrow(int32TypeValue, int32TypeValue)
              ),
              Kind.Star,
              Int32Operations.Power {| v1 = None |})
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | Int32Operations.Power v -> Some(Int32Operations.Power v)
            | _ -> None)

        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              let! op =
                op
                |> Int32Operations.AsPower
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! v =
                v
                |> Value.AsPrimitive
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! v =
                v
                |> PrimitiveValue.AsInt32
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return
                  (Int32Operations.Power({| v1 = Some v |}) |> operationLens.Set,
                   Some int32PowerId)
                  |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return
                  Value<TypeValue<'ext>, 'ext>
                    .Primitive(PrimitiveValue.Int32(pown vClosure v))
            } }

    let int32ModId =
      Identifier.FullyQualified([ "int32" ], "%")
      |> TypeCheckScope.Empty.Resolve

    let modOperation
      : ResolvedIdentifier *
        OperationExtension<'runtimeContext, 'ext, Int32Operations<'ext>> =
      int32ModId,
      { PublicIdentifiers =
          Some
          <| (TypeValue.CreateArrow(
                int32TypeValue,
                TypeValue.CreateArrow(int32TypeValue, int32TypeValue)
              ),
              Kind.Star,
              Int32Operations.Mod {| v1 = None |})
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | Int32Operations.Mod v -> Some(Int32Operations.Mod v)
            | _ -> None)
        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              let! op =
                op
                |> Int32Operations.AsMod
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! v =
                v
                |> Value.AsPrimitive
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! v =
                v
                |> PrimitiveValue.AsInt32
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return
                  (Int32Operations.Mod({| v1 = Some v |}) |> operationLens.Set,
                   Some int32ModId)
                  |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return
                  Value<TypeValue<'ext>, 'ext>
                    .Primitive(PrimitiveValue.Int32(vClosure % v))
            } }

    let int32EqualId =
      Identifier.FullyQualified([ "int32" ], "==")
      |> TypeCheckScope.Empty.Resolve

    let equalOperation
      : ResolvedIdentifier *
        OperationExtension<'runtimeContext, 'ext, Int32Operations<'ext>> =
      int32EqualId,
      { PublicIdentifiers =
          Some
          <| (TypeValue.CreateArrow(
                int32TypeValue,
                TypeValue.CreateArrow(int32TypeValue, boolTypeValue)
              ),
              Kind.Star,
              Int32Operations.Equal {| v1 = None |})
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | Int32Operations.Equal v -> Some(Int32Operations.Equal v)
            | _ -> None)
        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              let! op =
                op
                |> Int32Operations.AsEqual
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! v =
                v
                |> Value.AsPrimitive
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! v =
                v
                |> PrimitiveValue.AsInt32
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return
                  (Int32Operations.Equal({| v1 = Some v |}) |> operationLens.Set,
                   Some int32EqualId)
                  |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return
                  Value<TypeValue<'ext>, 'ext>
                    .Primitive(PrimitiveValue.Bool(vClosure = v))
            } }

    let int32NotEqualId =
      Identifier.FullyQualified([ "int32" ], "!=")
      |> TypeCheckScope.Empty.Resolve

    let notEqualOperation
      : ResolvedIdentifier *
        OperationExtension<'runtimeContext, 'ext, Int32Operations<'ext>> =
      int32NotEqualId,
      { PublicIdentifiers =
          Some
          <| (TypeValue.CreateArrow(
                int32TypeValue,
                TypeValue.CreateArrow(int32TypeValue, boolTypeValue)
              ),
              Kind.Star,
              Int32Operations.NotEqual {| v1 = None |})
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | Int32Operations.NotEqual v -> Some(Int32Operations.NotEqual v)
            | _ -> None)
        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              let! op =
                op
                |> Int32Operations.AsNotEqual
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! v =
                v
                |> Value.AsPrimitive
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! v =
                v
                |> PrimitiveValue.AsInt32
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return
                  (Int32Operations.NotEqual({| v1 = Some v |})
                   |> operationLens.Set,
                   Some int32NotEqualId)
                  |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return
                  Value<TypeValue<'ext>, 'ext>
                    .Primitive(PrimitiveValue.Bool(vClosure <> v))
            } }

    let int32GreaterThanId =
      Identifier.FullyQualified([ "int32" ], ">")
      |> TypeCheckScope.Empty.Resolve

    let greaterThanOperation
      : ResolvedIdentifier *
        OperationExtension<'runtimeContext, 'ext, Int32Operations<'ext>> =
      int32GreaterThanId,
      { PublicIdentifiers =
          Some
          <| (TypeValue.CreateArrow(
                int32TypeValue,
                TypeValue.CreateArrow(int32TypeValue, boolTypeValue)
              ),
              Kind.Star,
              Int32Operations.GreaterThan {| v1 = None |})
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | Int32Operations.GreaterThan v ->
              Some(Int32Operations.GreaterThan v)
            | _ -> None)
        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              let! op =
                op
                |> Int32Operations.AsGreaterThan
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! v =
                v
                |> Value.AsPrimitive
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! v =
                v
                |> PrimitiveValue.AsInt32
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return
                  (Int32Operations.GreaterThan({| v1 = Some v |})
                   |> operationLens.Set,
                   Some int32GreaterThanId)
                  |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application
                let res = vClosure > v

                return
                  Value<TypeValue<'ext>, 'ext>
                    .Primitive(PrimitiveValue.Bool(res))
            } }

    let int32GreaterThanOrEqualId =
      Identifier.FullyQualified([ "int32" ], ">=")
      |> TypeCheckScope.Empty.Resolve

    let greaterThanOrEqualOperation
      : ResolvedIdentifier *
        OperationExtension<'runtimeContext, 'ext, Int32Operations<'ext>> =
      int32GreaterThanOrEqualId,
      { PublicIdentifiers =
          Some
          <| (TypeValue.CreateArrow(
                int32TypeValue,
                TypeValue.CreateArrow(int32TypeValue, boolTypeValue)
              ),
              Kind.Star,
              Int32Operations.GreaterThanOrEqual {| v1 = None |})
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | Int32Operations.GreaterThanOrEqual v ->
              Some(Int32Operations.GreaterThanOrEqual v)
            | _ -> None)
        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              let! op =
                op
                |> Int32Operations.AsGreaterThanOrEqual
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! v =
                v
                |> Value.AsPrimitive
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! v =
                v
                |> PrimitiveValue.AsInt32
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return
                  (Int32Operations.GreaterThanOrEqual({| v1 = Some v |})
                   |> operationLens.Set,
                   Some int32GreaterThanOrEqualId)
                  |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return
                  Value<TypeValue<'ext>, 'ext>
                    .Primitive(PrimitiveValue.Bool(vClosure >= v))
            } }

    let int32LessThanId =
      Identifier.FullyQualified([ "int32" ], "<")
      |> TypeCheckScope.Empty.Resolve

    let lessThanOperation
      : ResolvedIdentifier *
        OperationExtension<'runtimeContext, 'ext, Int32Operations<'ext>> =
      int32LessThanId,
      { PublicIdentifiers =
          Some
          <| (TypeValue.CreateArrow(
                int32TypeValue,
                TypeValue.CreateArrow(int32TypeValue, boolTypeValue)
              ),
              Kind.Star,
              Int32Operations.LessThan {| v1 = None |})
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | Int32Operations.LessThan v -> Some(Int32Operations.LessThan v)
            | _ -> None)
        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              let! op =
                op
                |> Int32Operations.AsLessThan
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! v =
                v
                |> Value.AsPrimitive
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! v =
                v
                |> PrimitiveValue.AsInt32
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return
                  (Int32Operations.LessThan({| v1 = Some v |})
                   |> operationLens.Set,
                   Some int32LessThanId)
                  |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return
                  Value<TypeValue<'ext>, 'ext>
                    .Primitive(PrimitiveValue.Bool(vClosure < v))
            } }

    let int32LessThanOrEqualId =
      Identifier.FullyQualified([ "int32" ], "<=")
      |> TypeCheckScope.Empty.Resolve

    let lessThanOrEqualOperation
      : ResolvedIdentifier *
        OperationExtension<'runtimeContext, 'ext, Int32Operations<'ext>> =
      int32LessThanOrEqualId,
      { PublicIdentifiers =
          Some
          <| (TypeValue.CreateArrow(
                int32TypeValue,
                TypeValue.CreateArrow(int32TypeValue, boolTypeValue)
              ),
              Kind.Star,
              Int32Operations.LessThanOrEqual {| v1 = None |})
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | Int32Operations.LessThanOrEqual v ->
              Some(Int32Operations.LessThanOrEqual v)
            | _ -> None)
        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              let! op =
                op
                |> Int32Operations.AsLessThanOrEqual
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! v =
                v
                |> Value.AsPrimitive
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! v =
                v
                |> PrimitiveValue.AsInt32
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return
                  (Int32Operations.LessThanOrEqual({| v1 = Some v |})
                   |> operationLens.Set,
                   Some int32LessThanOrEqualId)
                  |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return
                  Value<TypeValue<'ext>, 'ext>
                    .Primitive(PrimitiveValue.Bool(vClosure <= v))
            } }

    { TypeVars = []
      Operations =
        [ toStringOperation
          tryParseOperation
          plusOperation
          minusOperation
          timesOperation
          divideOperation
          powerOperation
          modOperation
          equalOperation
          notEqualOperation
          greaterThanOperation
          greaterThanOrEqualOperation
          lessThanOperation
          lessThanOrEqualOperation ]
        |> Map.ofList }
