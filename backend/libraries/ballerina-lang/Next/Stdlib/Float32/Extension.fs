namespace Ballerina.DSL.Next.StdLib.Float32

[<AutoOpen>]
module Extension =
  open Ballerina
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
  open Ballerina.DSL.Next.StdLib.Option

  let Float32Extension<'ext>
    (operationLens: PartialLens<'ext, Float32Operations<'ext>>)
    : OperationsExtension<'ext, Float32Operations<'ext>> =
    let float32PlusId =
      Identifier.FullyQualified([ "float32" ], "+") |> TypeCheckScope.Empty.Resolve

    let int32TypeValue = TypeValue.CreateInt32()
    let float32TypeValue = TypeValue.CreateFloat32()
    let boolTypeValue = TypeValue.CreateBool()

    let plusOperation: ResolvedIdentifier * OperationExtension<'ext, Float32Operations<'ext>> =
      float32PlusId,
      { PublicIdentifiers =
          Some
          <| (TypeValue.CreateArrow(float32TypeValue, TypeValue.CreateArrow(float32TypeValue, float32TypeValue)),
              Kind.Star,
              Float32Operations.Plus {| v1 = None |})
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | Float32Operations.Plus v -> Some(Float32Operations.Plus v)
            | _ -> None)
        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              let! op =
                op
                |> Float32Operations.AsPlus
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! v =
                v
                |> Value.AsPrimitive
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! v =
                v
                |> PrimitiveValue.AsFloat32
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return
                  (Float32Operations.Plus({| v1 = Some v |}) |> operationLens.Set, Some float32PlusId)
                  |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue<'ext>, 'ext>.Primitive(PrimitiveValue.Float32(vClosure + v))
            } }

    let float32MinusId =
      Identifier.FullyQualified([ "float32" ], "-") |> TypeCheckScope.Empty.Resolve

    let minusOperation: ResolvedIdentifier * OperationExtension<'ext, Float32Operations<'ext>> =
      float32MinusId,
      { PublicIdentifiers =
          Some
          <| (TypeValue.CreateArrow(float32TypeValue, TypeValue.CreateArrow(float32TypeValue, float32TypeValue)),
              Kind.Star,
              Float32Operations.Minus {| v1 = None |})
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | Float32Operations.Minus v -> Some(Float32Operations.Minus v)
            | _ -> None)
        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              let! op =
                op
                |> Float32Operations.AsMinus
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! v =
                v
                |> Value.AsPrimitive
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! v =
                v
                |> PrimitiveValue.AsFloat32
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return
                  (Float32Operations.Minus({| v1 = Some v |}) |> operationLens.Set, Some float32MinusId)
                  |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue<'ext>, 'ext>.Primitive(PrimitiveValue.Float32(vClosure - v))
            } }

    let float32DivideId =
      Identifier.FullyQualified([ "float32" ], "/") |> TypeCheckScope.Empty.Resolve

    let divideOperation: ResolvedIdentifier * OperationExtension<'ext, Float32Operations<'ext>> =
      float32DivideId,
      { PublicIdentifiers =
          Some
          <| (TypeValue.CreateArrow(float32TypeValue, TypeValue.CreateArrow(float32TypeValue, float32TypeValue)),
              Kind.Star,
              Float32Operations.Divide {| v1 = None |})
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | Float32Operations.Divide v -> Some(Float32Operations.Divide v)
            | _ -> None)
        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              let! op =
                op
                |> Float32Operations.AsDivide
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! v =
                v
                |> Value.AsPrimitive
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! v =
                v
                |> PrimitiveValue.AsFloat32
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return
                  (Float32Operations.Divide({| v1 = Some v |}) |> operationLens.Set, Some float32DivideId)
                  |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue<'ext>, 'ext>.Primitive(PrimitiveValue.Float32(vClosure / v))
            } }

    let float32PowerId =
      Identifier.FullyQualified([ "float32" ], "**") |> TypeCheckScope.Empty.Resolve

    let powerOperation: ResolvedIdentifier * OperationExtension<'ext, Float32Operations<'ext>> =
      float32PowerId,
      { PublicIdentifiers =
          Some
          <| (TypeValue.CreateArrow(float32TypeValue, TypeValue.CreateArrow(int32TypeValue, float32TypeValue)),
              Kind.Star,
              Float32Operations.Power {| v1 = None |})
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | Float32Operations.Power v -> Some(Float32Operations.Power v)
            | _ -> None)

        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              let! op =
                op
                |> Float32Operations.AsPower
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
                  |> PrimitiveValue.AsFloat32
                  |> sum.MapError(Errors.MapContext(replaceWith loc0))
                  |> reader.OfSum

                return
                  (Float32Operations.Power({| v1 = Some v |}) |> operationLens.Set, Some float32PowerId)
                  |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application
                let! v =
                  v
                  |> PrimitiveValue.AsInt32
                  |> sum.MapError(Errors.MapContext(replaceWith loc0))
                  |> reader.OfSum

                return Value<TypeValue<'ext>, 'ext>.Primitive(PrimitiveValue.Float32(pown vClosure v))
            } }

    let float32ModId =
      Identifier.FullyQualified([ "float32" ], "%") |> TypeCheckScope.Empty.Resolve

    let modOperation: ResolvedIdentifier * OperationExtension<'ext, Float32Operations<'ext>> =
      float32ModId,
      { PublicIdentifiers =
          Some
          <| (TypeValue.CreateArrow(float32TypeValue, TypeValue.CreateArrow(float32TypeValue, float32TypeValue)),
              Kind.Star,
              Float32Operations.Mod {| v1 = None |})
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | Float32Operations.Mod v -> Some(Float32Operations.Mod v)
            | _ -> None)
        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              let! op =
                op
                |> Float32Operations.AsMod
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! v =
                v
                |> Value.AsPrimitive
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! v =
                v
                |> PrimitiveValue.AsFloat32
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return
                  (Float32Operations.Mod({| v1 = Some v |}) |> operationLens.Set, Some float32ModId)
                  |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue<'ext>, 'ext>.Primitive(PrimitiveValue.Float32(vClosure % v))
            } }

    let float32EqualId =
      Identifier.FullyQualified([ "float32" ], "==") |> TypeCheckScope.Empty.Resolve

    let equalOperation: ResolvedIdentifier * OperationExtension<'ext, Float32Operations<'ext>> =
      float32EqualId,
      { PublicIdentifiers =
          Some
          <| (TypeValue.CreateArrow(float32TypeValue, TypeValue.CreateArrow(float32TypeValue, boolTypeValue)),
              Kind.Star,
              Float32Operations.Equal {| v1 = None |})
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | Float32Operations.Equal v -> Some(Float32Operations.Equal v)
            | _ -> None)
        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              let! op =
                op
                |> Float32Operations.AsEqual
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! v =
                v
                |> Value.AsPrimitive
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! v =
                v
                |> PrimitiveValue.AsFloat32
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return
                  (Float32Operations.Equal({| v1 = Some v |}) |> operationLens.Set, Some float32EqualId)
                  |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue<'ext>, 'ext>.Primitive(PrimitiveValue.Bool(vClosure = v))
            } }

    let float32NotEqualId =
      Identifier.FullyQualified([ "float32" ], "!=") |> TypeCheckScope.Empty.Resolve

    let notEqualOperation: ResolvedIdentifier * OperationExtension<'ext, Float32Operations<'ext>> =
      float32NotEqualId,
      { PublicIdentifiers =
          Some
          <| (TypeValue.CreateArrow(float32TypeValue, TypeValue.CreateArrow(float32TypeValue, boolTypeValue)),
              Kind.Star,
              Float32Operations.NotEqual {| v1 = None |})
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | Float32Operations.NotEqual v -> Some(Float32Operations.NotEqual v)
            | _ -> None)
        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              let! op =
                op
                |> Float32Operations.AsNotEqual
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! v =
                v
                |> Value.AsPrimitive
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! v =
                v
                |> PrimitiveValue.AsFloat32
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return
                  (Float32Operations.NotEqual({| v1 = Some v |}) |> operationLens.Set, Some float32NotEqualId)
                  |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue<'ext>, 'ext>.Primitive(PrimitiveValue.Bool(vClosure <> v))
            } }

    let float32GreaterThanId =
      Identifier.FullyQualified([ "float32" ], ">") |> TypeCheckScope.Empty.Resolve

    let greaterThanOperation: ResolvedIdentifier * OperationExtension<'ext, Float32Operations<'ext>> =
      float32GreaterThanId,
      { PublicIdentifiers =
          Some
          <| (TypeValue.CreateArrow(float32TypeValue, TypeValue.CreateArrow(float32TypeValue, boolTypeValue)),
              Kind.Star,
              Float32Operations.GreaterThan {| v1 = None |})
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | Float32Operations.GreaterThan v -> Some(Float32Operations.GreaterThan v)
            | _ -> None)
        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              let! op =
                op
                |> Float32Operations.AsGreaterThan
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! v =
                v
                |> Value.AsPrimitive
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! v =
                v
                |> PrimitiveValue.AsFloat32
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return
                  (Float32Operations.GreaterThan({| v1 = Some v |}) |> operationLens.Set, Some float32GreaterThanId)
                  |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue<'ext>, 'ext>.Primitive(PrimitiveValue.Bool(vClosure > v))
            } }

    let float32GreaterThanOrEqualId =
      Identifier.FullyQualified([ "float32" ], ">=") |> TypeCheckScope.Empty.Resolve

    let greaterThanOrEqualOperation: ResolvedIdentifier * OperationExtension<'ext, Float32Operations<'ext>> =
      float32GreaterThanOrEqualId,
      { PublicIdentifiers =
          Some
          <| (TypeValue.CreateArrow(float32TypeValue, TypeValue.CreateArrow(float32TypeValue, boolTypeValue)),
              Kind.Star,
              Float32Operations.GreaterThanOrEqual {| v1 = None |})
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | Float32Operations.GreaterThanOrEqual v -> Some(Float32Operations.GreaterThanOrEqual v)
            | _ -> None)
        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              let! op =
                op
                |> Float32Operations.AsGreaterThanOrEqual
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! v =
                v
                |> Value.AsPrimitive
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! v =
                v
                |> PrimitiveValue.AsFloat32
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return
                  (Float32Operations.GreaterThanOrEqual({| v1 = Some v |}) |> operationLens.Set,
                   Some float32GreaterThanOrEqualId)
                  |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue<'ext>, 'ext>.Primitive(PrimitiveValue.Bool(vClosure >= v))
            } }

    let float32LessThanId =
      Identifier.FullyQualified([ "float32" ], "<") |> TypeCheckScope.Empty.Resolve

    let lessThanOperation: ResolvedIdentifier * OperationExtension<'ext, Float32Operations<'ext>> =
      float32LessThanId,
      { PublicIdentifiers =
          Some
          <| (TypeValue.CreateArrow(float32TypeValue, TypeValue.CreateArrow(float32TypeValue, boolTypeValue)),
              Kind.Star,
              Float32Operations.LessThan {| v1 = None |})
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | Float32Operations.LessThan v -> Some(Float32Operations.LessThan v)
            | _ -> None)
        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              let! op =
                op
                |> Float32Operations.AsLessThan
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! v =
                v
                |> Value.AsPrimitive
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! v =
                v
                |> PrimitiveValue.AsFloat32
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return
                  (Float32Operations.LessThan({| v1 = Some v |}) |> operationLens.Set, Some float32LessThanId)
                  |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue<'ext>, 'ext>.Primitive(PrimitiveValue.Bool(vClosure < v))
            } }

    let float32LessThanOrEqualId =
      Identifier.FullyQualified([ "float32" ], "<=") |> TypeCheckScope.Empty.Resolve

    let lessThanOrEqualOperation: ResolvedIdentifier * OperationExtension<'ext, Float32Operations<'ext>> =
      float32LessThanOrEqualId,
      { PublicIdentifiers =
          Some
          <| (TypeValue.CreateArrow(float32TypeValue, TypeValue.CreateArrow(float32TypeValue, boolTypeValue)),
              Kind.Star,
              Float32Operations.LessThanOrEqual {| v1 = None |})
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | Float32Operations.LessThanOrEqual v -> Some(Float32Operations.LessThanOrEqual v)
            | _ -> None)
        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              let! op =
                op
                |> Float32Operations.AsLessThanOrEqual
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! v =
                v
                |> Value.AsPrimitive
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! v =
                v
                |> PrimitiveValue.AsFloat32
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return
                  (Float32Operations.LessThanOrEqual({| v1 = Some v |}) |> operationLens.Set,
                   Some float32LessThanOrEqualId)
                  |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue<'ext>, 'ext>.Primitive(PrimitiveValue.Bool(vClosure <= v))
            } }

    { TypeVars = []
      Operations =
        [ plusOperation
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
