namespace Ballerina.DSL.Next.StdLib.Int64

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

  let Int64Extension<'runtimeContext, 'ext>
    (operationLens: PartialLens<'ext, Int64Operations<'ext>>)
    : OperationsExtension<'runtimeContext, 'ext, Int64Operations<'ext>> =

    let boolTypeValue = TypeValue.CreateBool()
    let int64TypeValue = TypeValue.CreateInt64()
    let int32TypeValue = TypeValue.CreateInt32()
    let stringTypeValue = TypeValue.CreatePrimitive PrimitiveType.String

    let int64ToStringId =
      Identifier.FullyQualified([ "int64" ], "toString")
      |> TypeCheckScope.Empty.Resolve

    let toStringOperation: ResolvedIdentifier * OperationExtension<'runtimeContext, 'ext, Int64Operations<'ext>> =
      int64ToStringId,
      { PublicIdentifiers =
          Some
          <| (TypeValue.CreateArrow(int64TypeValue, stringTypeValue), Kind.Star, Int64Operations.String)
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | Int64Operations.String -> Some(Int64Operations.String)
            | _ -> None)
        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              do!
                op
                |> Int64Operations.AsToString
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! v =
                v
                |> Value.AsPrimitive
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! v =
                v
                |> PrimitiveValue.AsInt64
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              return Value<TypeValue<'ext>, 'ext>.Primitive(PrimitiveValue.String(v |> string))
            } }

    let int64TryParseId =
      Identifier.FullyQualified([ "int64" ], "tryParse")
      |> TypeCheckScope.Empty.Resolve

    let tryParseOperation: ResolvedIdentifier * OperationExtension<'runtimeContext, 'ext, Int64Operations<'ext>> =
      int64TryParseId,
      { PublicIdentifiers =
          Some
          <| (TypeValue.CreateArrow(
                stringTypeValue,
                TypeValue.CreateSum
                  [ TypeValue.CreatePrimitive PrimitiveType.Unit
                    TypeValue.CreatePrimitive PrimitiveType.Int64 ]
              ),
              Kind.Star,
              Int64Operations.TryParse)
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | Int64Operations.TryParse -> Some(Int64Operations.TryParse)
            | _ -> None)
        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              do!
                op
                |> Int64Operations.AsTryParse
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
                match System.Int64.TryParse(v) with
                | true, result -> Value.Sum({ Case = 2; Count = 2 }, Value.Primitive(PrimitiveValue.Int64 result))
                | false, _ -> Value.Sum({ Case = 1; Count = 2 }, Value.Primitive(PrimitiveValue.Unit))
            } }

    let int64PlusId =
      Identifier.FullyQualified([ "int64" ], "+") |> TypeCheckScope.Empty.Resolve

    let plusOperation: ResolvedIdentifier * OperationExtension<'runtimeContext, 'ext, Int64Operations<'ext>> =
      int64PlusId,
      { PublicIdentifiers =
          Some
          <| (TypeValue.CreateArrow(int64TypeValue, TypeValue.CreateArrow(int64TypeValue, int64TypeValue)),
              Kind.Star,
              Int64Operations.Plus {| v1 = None |})
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | Int64Operations.Plus v -> Some(Int64Operations.Plus v)
            | _ -> None)

        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              let! op =
                op
                |> Int64Operations.AsPlus
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! v =
                v
                |> Value.AsPrimitive
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! v =
                v
                |> PrimitiveValue.AsInt64
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return
                  (Int64Operations.Plus({| v1 = Some v |}) |> operationLens.Set, Some int64PlusId)
                  |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue<'ext>, 'ext>.Primitive(PrimitiveValue.Int64(vClosure + v))
            } }

    let int64MinusId =
      Identifier.FullyQualified([ "int64" ], "-") |> TypeCheckScope.Empty.Resolve

    let minusOperation: ResolvedIdentifier * OperationExtension<'runtimeContext, 'ext, Int64Operations<'ext>> =
      int64MinusId,
      { PublicIdentifiers =
          Some
          <| (TypeValue.CreateArrow(int64TypeValue, TypeValue.CreateArrow(int64TypeValue, int64TypeValue)),
              Kind.Star,
              Int64Operations.Minus {| v1 = None |})
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | Int64Operations.Minus v -> Some(Int64Operations.Minus v)
            | _ -> None)

        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              let! op =
                op
                |> Int64Operations.AsMinus
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! v =
                v
                |> Value.AsPrimitive
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! v =
                v
                |> PrimitiveValue.AsInt64
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return
                  (Int64Operations.Minus({| v1 = Some v |}) |> operationLens.Set, Some int64MinusId)
                  |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue<'ext>, 'ext>.Primitive(PrimitiveValue.Int64(vClosure - v))
            } }

    let int64DivideId =
      Identifier.FullyQualified([ "int64" ], "/") |> TypeCheckScope.Empty.Resolve

    let divideOperation: ResolvedIdentifier * OperationExtension<'runtimeContext, 'ext, Int64Operations<'ext>> =
      int64DivideId,
      { PublicIdentifiers =
          Some
          <| (TypeValue.CreateArrow(int64TypeValue, TypeValue.CreateArrow(int64TypeValue, int64TypeValue)),
              Kind.Star,
              Int64Operations.Divide {| v1 = None |})
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | Int64Operations.Divide v -> Some(Int64Operations.Divide v)
            | _ -> None)
        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              let! op =
                op
                |> Int64Operations.AsDivide
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! v =
                v
                |> Value.AsPrimitive
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! v =
                v
                |> PrimitiveValue.AsInt64
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return
                  (Int64Operations.Divide({| v1 = Some v |}) |> operationLens.Set, Some int64DivideId)
                  |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue<'ext>, 'ext>.Primitive(PrimitiveValue.Int64(vClosure / v))
            } }

    let int64PowerId =
      Identifier.FullyQualified([ "int64" ], "**") |> TypeCheckScope.Empty.Resolve

    let powerOperation: ResolvedIdentifier * OperationExtension<'runtimeContext, 'ext, Int64Operations<'ext>> =
      int64PowerId,
      { PublicIdentifiers =
          Some
          <| (TypeValue.CreateArrow(int64TypeValue, TypeValue.CreateArrow(int32TypeValue, int64TypeValue)),
              Kind.Star,
              Int64Operations.Power {| v1 = None |})
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | Int64Operations.Power v -> Some(Int64Operations.Power v)
            | _ -> None)

        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              let! op =
                op
                |> Int64Operations.AsPower
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! v =
                v
                |> Value.AsPrimitive
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                let! v =
                  v
                  |> PrimitiveValue.AsInt64
                  |> sum.MapError(Errors.MapContext(replaceWith loc0))
                  |> reader.OfSum

                return
                  (Int64Operations.Power({| v1 = Some v |}) |> operationLens.Set, Some int64PowerId)
                  |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application
                let! v =
                  v
                  |> PrimitiveValue.AsInt32
                  |> sum.MapError(Errors.MapContext(replaceWith loc0))
                  |> reader.OfSum

                return Value<TypeValue<'ext>, 'ext>.Primitive(PrimitiveValue.Int64(pown vClosure v))
            } }

    let int64ModId =
      Identifier.FullyQualified([ "int64" ], "%") |> TypeCheckScope.Empty.Resolve

    let modOperation: ResolvedIdentifier * OperationExtension<'runtimeContext, 'ext, Int64Operations<'ext>> =
      int64ModId,
      { PublicIdentifiers =
          Some
          <| (TypeValue.CreateArrow(int64TypeValue, TypeValue.CreateArrow(int64TypeValue, int64TypeValue)),
              Kind.Star,
              Int64Operations.Mod {| v1 = None |})
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | Int64Operations.Mod v -> Some(Int64Operations.Mod v)
            | _ -> None)
        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              let! op =
                op
                |> Int64Operations.AsMod
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! v =
                v
                |> Value.AsPrimitive
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! v =
                v
                |> PrimitiveValue.AsInt64
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return
                  (Int64Operations.Mod({| v1 = Some v |}) |> operationLens.Set, Some int64ModId)
                  |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue<'ext>, 'ext>.Primitive(PrimitiveValue.Int64(vClosure % v))
            } }

    let int64EqualId =
      Identifier.FullyQualified([ "int64" ], "==") |> TypeCheckScope.Empty.Resolve

    let equalOperation: ResolvedIdentifier * OperationExtension<'runtimeContext, 'ext, Int64Operations<'ext>> =
      int64EqualId,
      { PublicIdentifiers =
          Some
          <| (TypeValue.CreateArrow(int64TypeValue, TypeValue.CreateArrow(int64TypeValue, boolTypeValue)),
              Kind.Star,
              Int64Operations.Equal {| v1 = None |})
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | Int64Operations.Equal v -> Some(Int64Operations.Equal v)
            | _ -> None)
        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              let! op =
                op
                |> Int64Operations.AsEqual
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! v =
                v
                |> Value.AsPrimitive
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! v =
                v
                |> PrimitiveValue.AsInt64
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return
                  (Int64Operations.Equal({| v1 = Some v |}) |> operationLens.Set, Some int64EqualId)
                  |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue<'ext>, 'ext>.Primitive(PrimitiveValue.Bool(vClosure = v))
            } }

    let int64NotEqualId =
      Identifier.FullyQualified([ "int64" ], "!=") |> TypeCheckScope.Empty.Resolve

    let notEqualOperation: ResolvedIdentifier * OperationExtension<'runtimeContext, 'ext, Int64Operations<'ext>> =
      int64NotEqualId,
      { PublicIdentifiers =
          Some
          <| (TypeValue.CreateArrow(int64TypeValue, TypeValue.CreateArrow(int64TypeValue, boolTypeValue)),
              Kind.Star,
              Int64Operations.NotEqual {| v1 = None |})
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | Int64Operations.NotEqual v -> Some(Int64Operations.NotEqual v)
            | _ -> None)
        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              let! op =
                op
                |> Int64Operations.AsNotEqual
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! v =
                v
                |> Value.AsPrimitive
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! v =
                v
                |> PrimitiveValue.AsInt64
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return
                  (Int64Operations.NotEqual({| v1 = Some v |}) |> operationLens.Set, Some int64NotEqualId)
                  |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue<'ext>, 'ext>.Primitive(PrimitiveValue.Bool(vClosure <> v))
            } }

    let int64GreaterThanId =
      Identifier.FullyQualified([ "int64" ], ">") |> TypeCheckScope.Empty.Resolve

    let greaterThanOperation: ResolvedIdentifier * OperationExtension<'runtimeContext, 'ext, Int64Operations<'ext>> =
      int64GreaterThanId,
      { PublicIdentifiers =
          Some
          <| (TypeValue.CreateArrow(int64TypeValue, TypeValue.CreateArrow(int64TypeValue, boolTypeValue)),
              Kind.Star,
              Int64Operations.GreaterThan {| v1 = None |})
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | Int64Operations.GreaterThan v -> Some(Int64Operations.GreaterThan v)
            | _ -> None)
        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              let! op =
                op
                |> Int64Operations.AsGreaterThan
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! v =
                v
                |> Value.AsPrimitive
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! v =
                v
                |> PrimitiveValue.AsInt64
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return
                  (Int64Operations.GreaterThan({| v1 = Some v |}) |> operationLens.Set, Some int64GreaterThanId)
                  |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue<'ext>, 'ext>.Primitive(PrimitiveValue.Bool(vClosure > v))
            } }

    let int64GreaterThanOrEqualId =
      Identifier.FullyQualified([ "int64" ], ">=") |> TypeCheckScope.Empty.Resolve

    let greaterThanOrEqualOperation
      : ResolvedIdentifier * OperationExtension<'runtimeContext, 'ext, Int64Operations<'ext>> =
      int64GreaterThanOrEqualId,
      { PublicIdentifiers =
          Some
          <| (TypeValue.CreateArrow(int64TypeValue, TypeValue.CreateArrow(int64TypeValue, boolTypeValue)),
              Kind.Star,
              Int64Operations.GreaterThanOrEqual {| v1 = None |})
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | Int64Operations.GreaterThanOrEqual v -> Some(Int64Operations.GreaterThanOrEqual v)
            | _ -> None)
        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              let! op =
                op
                |> Int64Operations.AsGreaterThanOrEqual
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! v =
                v
                |> Value.AsPrimitive
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! v =
                v
                |> PrimitiveValue.AsInt64
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return
                  (Int64Operations.GreaterThanOrEqual({| v1 = Some v |}) |> operationLens.Set,
                   Some int64GreaterThanOrEqualId)
                  |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue<'ext>, 'ext>.Primitive(PrimitiveValue.Bool(vClosure >= v))
            } }

    let int64LessThanId =
      Identifier.FullyQualified([ "int64" ], "<") |> TypeCheckScope.Empty.Resolve

    let lessThanOperation: ResolvedIdentifier * OperationExtension<'runtimeContext, 'ext, Int64Operations<'ext>> =
      int64LessThanId,
      { PublicIdentifiers =
          Some
          <| (TypeValue.CreateArrow(int64TypeValue, TypeValue.CreateArrow(int64TypeValue, boolTypeValue)),
              Kind.Star,
              Int64Operations.LessThan {| v1 = None |})
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | Int64Operations.LessThan v -> Some(Int64Operations.LessThan v)
            | _ -> None)
        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              let! op =
                op
                |> Int64Operations.AsLessThan
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! v =
                v
                |> Value.AsPrimitive
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! v =
                v
                |> PrimitiveValue.AsInt64
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return
                  (Int64Operations.LessThan({| v1 = Some v |}) |> operationLens.Set, Some int64LessThanId)
                  |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue<'ext>, 'ext>.Primitive(PrimitiveValue.Bool(vClosure < v))
            } }

    let int64LessThanOrEqualId =
      Identifier.FullyQualified([ "int64" ], "<=") |> TypeCheckScope.Empty.Resolve

    let lessThanOrEqualOperation: ResolvedIdentifier * OperationExtension<'runtimeContext, 'ext, Int64Operations<'ext>> =
      int64LessThanOrEqualId,
      { PublicIdentifiers =
          Some
          <| (TypeValue.CreateArrow(int64TypeValue, TypeValue.CreateArrow(int64TypeValue, boolTypeValue)),
              Kind.Star,
              Int64Operations.LessThanOrEqual {| v1 = None |})
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | Int64Operations.LessThanOrEqual v -> Some(Int64Operations.LessThanOrEqual v)
            | _ -> None)
        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              let! op =
                op
                |> Int64Operations.AsLessThanOrEqual
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! v =
                v
                |> Value.AsPrimitive
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! v =
                v
                |> PrimitiveValue.AsInt64
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return
                  (Int64Operations.LessThanOrEqual({| v1 = Some v |}) |> operationLens.Set, Some int64LessThanOrEqualId)
                  |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue<'ext>, 'ext>.Primitive(PrimitiveValue.Bool(vClosure <= v))
            } }

    { TypeVars = []
      Operations =
        [ toStringOperation
          tryParseOperation
          plusOperation
          minusOperation
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
