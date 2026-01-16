namespace Ballerina.DSL.Next.StdLib.Float64

[<AutoOpen>]
module Extension =
  open Ballerina.Collections.Sum
  open Ballerina.Reader.WithError
  open Ballerina.LocalizedErrors
  open Ballerina.DSL.Next.Terms.Model
  open Ballerina.DSL.Next.Terms.Patterns
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Types.Patterns
  open Ballerina.Lenses
  open Ballerina.DSL.Next.Extensions
  open Ballerina.DSL.Next.StdLib.Option


  let Float64Extension<'ext>
    (operationLens: PartialLens<'ext, Float64Operations<'ext>>)
    : OperationsExtension<'ext, Float64Operations<'ext>> =
    let float64PlusId =
      Identifier.FullyQualified([ "float64" ], "+") |> TypeCheckScope.Empty.Resolve


    let boolTypeValue = TypeValue.CreatePrimitive PrimitiveType.Bool
    let int32TypeValue = TypeValue.CreatePrimitive PrimitiveType.Int32
    let float64TypeValue = TypeValue.CreatePrimitive PrimitiveType.Float64

    let plusOperation: ResolvedIdentifier * OperationExtension<'ext, Float64Operations<'ext>> =
      float64PlusId,
      { PublicIdentifiers =
          Some
          <| (TypeValue.CreateArrow(float64TypeValue, TypeValue.CreateArrow(float64TypeValue, float64TypeValue)),
              Kind.Star,
              Float64Operations.Plus {| v1 = None |})
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | Float64Operations.Plus v -> Some(Float64Operations.Plus v)
            | _ -> None)
        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              let! op =
                op
                |> Float64Operations.AsPlus
                |> sum.MapError(Errors.FromErrors loc0)
                |> reader.OfSum

              let! v = v |> Value.AsPrimitive |> sum.MapError(Errors.FromErrors loc0) |> reader.OfSum

              let! v =
                v
                |> PrimitiveValue.AsFloat64
                |> sum.MapError(Errors.FromErrors loc0)
                |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return Float64Operations.Plus({| v1 = Some v |}) |> operationLens.Set |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue<'ext>, 'ext>.Primitive(PrimitiveValue.Float64(vClosure + v))
            } }

    let float64MinusId =
      Identifier.FullyQualified([ "float64" ], "-") |> TypeCheckScope.Empty.Resolve

    let minusOperation: ResolvedIdentifier * OperationExtension<'ext, Float64Operations<'ext>> =
      float64MinusId,
      { PublicIdentifiers =
          Some
          <| (TypeValue.CreateArrow(float64TypeValue, TypeValue.CreateArrow(float64TypeValue, float64TypeValue)),
              Kind.Star,
              Float64Operations.Minus {| v1 = None |})
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | Float64Operations.Minus v -> Some(Float64Operations.Minus v)
            | _ -> None)
        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              let! op =
                op
                |> Float64Operations.AsMinus
                |> sum.MapError(Errors.FromErrors loc0)
                |> reader.OfSum

              let! v = v |> Value.AsPrimitive |> sum.MapError(Errors.FromErrors loc0) |> reader.OfSum

              let! v =
                v
                |> PrimitiveValue.AsFloat64
                |> sum.MapError(Errors.FromErrors loc0)
                |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return Float64Operations.Minus({| v1 = Some v |}) |> operationLens.Set |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application
                return Value<TypeValue<'ext>, 'ext>.Primitive(PrimitiveValue.Float64(vClosure - v))
            } }

    let float64DivideId =
      Identifier.FullyQualified([ "float64" ], "/") |> TypeCheckScope.Empty.Resolve

    let divideOperation: ResolvedIdentifier * OperationExtension<'ext, Float64Operations<'ext>> =
      float64DivideId,
      { PublicIdentifiers =
          Some
          <| (TypeValue.CreateArrow(float64TypeValue, TypeValue.CreateArrow(float64TypeValue, float64TypeValue)),
              Kind.Star,
              Float64Operations.Divide {| v1 = None |})
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | Float64Operations.Divide v -> Some(Float64Operations.Divide v)
            | _ -> None)
        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              let! op =
                op
                |> Float64Operations.AsDivide
                |> sum.MapError(Errors.FromErrors loc0)
                |> reader.OfSum

              let! v = v |> Value.AsPrimitive |> sum.MapError(Errors.FromErrors loc0) |> reader.OfSum

              let! v =
                v
                |> PrimitiveValue.AsFloat64
                |> sum.MapError(Errors.FromErrors loc0)
                |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return Float64Operations.Divide({| v1 = Some v |}) |> operationLens.Set |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue<'ext>, 'ext>.Primitive(PrimitiveValue.Float64(vClosure / v))
            } }

    let float64PowerId =
      Identifier.FullyQualified([ "float64" ], "**") |> TypeCheckScope.Empty.Resolve

    let powerOperation: ResolvedIdentifier * OperationExtension<'ext, Float64Operations<'ext>> =
      float64PowerId,
      { PublicIdentifiers =
          Some
          <| (TypeValue.CreateArrow(float64TypeValue, TypeValue.CreateArrow(float64TypeValue, int32TypeValue)),
              Kind.Star,
              Float64Operations.Power {| v1 = None |})
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | Float64Operations.Power v -> Some(Float64Operations.Power v)
            | _ -> None)

        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              let! op =
                op
                |> Float64Operations.AsPower
                |> sum.MapError(Errors.FromErrors loc0)
                |> reader.OfSum

              let! v = v |> Value.AsPrimitive |> sum.MapError(Errors.FromErrors loc0) |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                let! v =
                  v
                  |> PrimitiveValue.AsFloat64
                  |> sum.MapError(Errors.FromErrors loc0)
                  |> reader.OfSum

                return Float64Operations.Power({| v1 = Some v |}) |> operationLens.Set |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application
                let! v =
                  v
                  |> PrimitiveValue.AsInt32
                  |> sum.MapError(Errors.FromErrors loc0)
                  |> reader.OfSum

                return Value<TypeValue<'ext>, 'ext>.Primitive(PrimitiveValue.Float64(pown vClosure v))
            } }

    let float64ModId =
      Identifier.FullyQualified([ "float64" ], "%") |> TypeCheckScope.Empty.Resolve

    let modOperation: ResolvedIdentifier * OperationExtension<'ext, Float64Operations<'ext>> =
      float64ModId,
      { PublicIdentifiers =
          Some
          <| (TypeValue.CreateArrow(float64TypeValue, TypeValue.CreateArrow(float64TypeValue, float64TypeValue)),
              Kind.Star,
              Float64Operations.Mod {| v1 = None |})
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | Float64Operations.Mod v -> Some(Float64Operations.Mod v)
            | _ -> None)
        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              let! op =
                op
                |> Float64Operations.AsMod
                |> sum.MapError(Errors.FromErrors loc0)
                |> reader.OfSum

              let! v = v |> Value.AsPrimitive |> sum.MapError(Errors.FromErrors loc0) |> reader.OfSum

              let! v =
                v
                |> PrimitiveValue.AsFloat64
                |> sum.MapError(Errors.FromErrors loc0)
                |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return Float64Operations.Mod({| v1 = Some v |}) |> operationLens.Set |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue<'ext>, 'ext>.Primitive(PrimitiveValue.Float64(vClosure % v))
            } }

    let float64EqualId =
      Identifier.FullyQualified([ "float64" ], "==") |> TypeCheckScope.Empty.Resolve

    let equalOperation: ResolvedIdentifier * OperationExtension<'ext, Float64Operations<'ext>> =
      float64EqualId,
      { PublicIdentifiers =
          Some
          <| (TypeValue.CreateArrow(float64TypeValue, TypeValue.CreateArrow(float64TypeValue, boolTypeValue)),
              Kind.Star,
              Float64Operations.Equal {| v1 = None |})
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | Float64Operations.Equal v -> Some(Float64Operations.Equal v)
            | _ -> None)
        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              let! op =
                op
                |> Float64Operations.AsEqual
                |> sum.MapError(Errors.FromErrors loc0)
                |> reader.OfSum

              let! v = v |> Value.AsPrimitive |> sum.MapError(Errors.FromErrors loc0) |> reader.OfSum

              let! v =
                v
                |> PrimitiveValue.AsFloat64
                |> sum.MapError(Errors.FromErrors loc0)
                |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return Float64Operations.Equal({| v1 = Some v |}) |> operationLens.Set |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue<'ext>, 'ext>.Primitive(PrimitiveValue.Bool(vClosure = v))
            } }

    let float64NotEqualId =
      Identifier.FullyQualified([ "float64" ], "!=") |> TypeCheckScope.Empty.Resolve

    let notEqualOperation: ResolvedIdentifier * OperationExtension<'ext, Float64Operations<'ext>> =
      float64NotEqualId,
      { PublicIdentifiers =
          Some
          <| (TypeValue.CreateArrow(float64TypeValue, TypeValue.CreateArrow(float64TypeValue, boolTypeValue)),
              Kind.Star,
              Float64Operations.NotEqual {| v1 = None |})
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | Float64Operations.NotEqual v -> Some(Float64Operations.NotEqual v)
            | _ -> None)
        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              let! op =
                op
                |> Float64Operations.AsNotEqual
                |> sum.MapError(Errors.FromErrors loc0)
                |> reader.OfSum

              let! v = v |> Value.AsPrimitive |> sum.MapError(Errors.FromErrors loc0) |> reader.OfSum

              let! v =
                v
                |> PrimitiveValue.AsFloat64
                |> sum.MapError(Errors.FromErrors loc0)
                |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return Float64Operations.NotEqual({| v1 = Some v |}) |> operationLens.Set |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue<'ext>, 'ext>.Primitive(PrimitiveValue.Bool(vClosure <> v))
            } }

    let float64GreaterThanId =
      Identifier.FullyQualified([ "float64" ], ">") |> TypeCheckScope.Empty.Resolve

    let greaterThanOperation: ResolvedIdentifier * OperationExtension<'ext, Float64Operations<'ext>> =
      float64GreaterThanId,
      { PublicIdentifiers =
          Some
          <| (TypeValue.CreateArrow(float64TypeValue, TypeValue.CreateArrow(float64TypeValue, boolTypeValue)),
              Kind.Star,
              Float64Operations.GreaterThan {| v1 = None |})
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | Float64Operations.GreaterThan v -> Some(Float64Operations.GreaterThan v)
            | _ -> None)
        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              let! op =
                op
                |> Float64Operations.AsGreaterThan
                |> sum.MapError(Errors.FromErrors loc0)
                |> reader.OfSum

              let! v = v |> Value.AsPrimitive |> sum.MapError(Errors.FromErrors loc0) |> reader.OfSum

              let! v =
                v
                |> PrimitiveValue.AsFloat64
                |> sum.MapError(Errors.FromErrors loc0)
                |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return Float64Operations.GreaterThan({| v1 = Some v |}) |> operationLens.Set |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue<'ext>, 'ext>.Primitive(PrimitiveValue.Bool(vClosure > v))
            } }

    let float64GreaterThanOrEqualId =
      Identifier.FullyQualified([ "float64" ], ">=") |> TypeCheckScope.Empty.Resolve

    let greaterThanOrEqualOperation: ResolvedIdentifier * OperationExtension<'ext, Float64Operations<'ext>> =
      float64GreaterThanOrEqualId,
      { PublicIdentifiers =
          Some
          <| (TypeValue.CreateArrow(float64TypeValue, TypeValue.CreateArrow(float64TypeValue, boolTypeValue)),
              Kind.Star,
              Float64Operations.GreaterThanOrEqual {| v1 = None |})
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | Float64Operations.GreaterThanOrEqual v -> Some(Float64Operations.GreaterThanOrEqual v)
            | _ -> None)
        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              let! op =
                op
                |> Float64Operations.AsGreaterThanOrEqual
                |> sum.MapError(Errors.FromErrors loc0)
                |> reader.OfSum

              let! v = v |> Value.AsPrimitive |> sum.MapError(Errors.FromErrors loc0) |> reader.OfSum

              let! v =
                v
                |> PrimitiveValue.AsFloat64
                |> sum.MapError(Errors.FromErrors loc0)
                |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return
                  Float64Operations.GreaterThanOrEqual({| v1 = Some v |})
                  |> operationLens.Set
                  |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue<'ext>, 'ext>.Primitive(PrimitiveValue.Bool(vClosure >= v))
            } }

    let float64LessThanId =
      Identifier.FullyQualified([ "float64" ], "<") |> TypeCheckScope.Empty.Resolve

    let lessThanOperation: ResolvedIdentifier * OperationExtension<'ext, Float64Operations<'ext>> =
      float64LessThanId,
      { PublicIdentifiers =
          Some
          <| (TypeValue.CreateArrow(float64TypeValue, TypeValue.CreateArrow(float64TypeValue, boolTypeValue)),
              Kind.Star,
              Float64Operations.LessThan {| v1 = None |})
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | Float64Operations.LessThan v -> Some(Float64Operations.LessThan v)
            | _ -> None)
        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              let! op =
                op
                |> Float64Operations.AsLessThan
                |> sum.MapError(Errors.FromErrors loc0)
                |> reader.OfSum

              let! v = v |> Value.AsPrimitive |> sum.MapError(Errors.FromErrors loc0) |> reader.OfSum

              let! v =
                v
                |> PrimitiveValue.AsFloat64
                |> sum.MapError(Errors.FromErrors loc0)
                |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return Float64Operations.LessThan({| v1 = Some v |}) |> operationLens.Set |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue<'ext>, 'ext>.Primitive(PrimitiveValue.Bool(vClosure < v))
            } }

    let float64LessThanOrEqualId =
      Identifier.FullyQualified([ "float64" ], "<=") |> TypeCheckScope.Empty.Resolve

    let lessThanOrEqualOperation: ResolvedIdentifier * OperationExtension<'ext, Float64Operations<'ext>> =
      float64LessThanOrEqualId,
      { PublicIdentifiers =
          Some
          <| (TypeValue.CreateArrow(float64TypeValue, TypeValue.CreateArrow(float64TypeValue, boolTypeValue)),
              Kind.Star,
              Float64Operations.LessThanOrEqual {| v1 = None |})
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | Float64Operations.LessThanOrEqual v -> Some(Float64Operations.LessThanOrEqual v)
            | _ -> None)
        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              let! op =
                op
                |> Float64Operations.AsLessThanOrEqual
                |> sum.MapError(Errors.FromErrors loc0)
                |> reader.OfSum

              let! v = v |> Value.AsPrimitive |> sum.MapError(Errors.FromErrors loc0) |> reader.OfSum

              let! v =
                v
                |> PrimitiveValue.AsFloat64
                |> sum.MapError(Errors.FromErrors loc0)
                |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return Float64Operations.LessThanOrEqual({| v1 = Some v |}) |> operationLens.Set |> Ext
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
