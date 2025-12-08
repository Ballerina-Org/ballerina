namespace Ballerina.DSL.Next.StdLib.Int64

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

  let private boolTypeValue = TypeValue.CreateBool()
  let private int64TypeValue = TypeValue.CreateInt64()
  let private int32TypeValue = TypeValue.CreateInt32()

  let Int64Extension<'ext>
    (operationLens: PartialLens<'ext, Int64Operations<'ext>>)
    : OperationsExtension<'ext, Int64Operations<'ext>> =

    let int64PlusId =
      Identifier.FullyQualified([ "int64" ], "+") |> TypeCheckScope.Empty.Resolve

    let plusOperation: ResolvedIdentifier * OperationExtension<'ext, Int64Operations<'ext>> =
      int64PlusId,
      { Type = TypeValue.CreateArrow(int64TypeValue, TypeValue.CreateArrow(int64TypeValue, int64TypeValue))
        Kind = Kind.Star
        Operation = Int64Operations.Plus {| v1 = None |}
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
                |> sum.MapError(Errors.FromErrors loc0)
                |> reader.OfSum

              let! v = v |> Value.AsPrimitive |> sum.MapError(Errors.FromErrors loc0) |> reader.OfSum

              let! v =
                v
                |> PrimitiveValue.AsInt64
                |> sum.MapError(Errors.FromErrors loc0)
                |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return Int64Operations.Plus({| v1 = Some v |}) |> operationLens.Set |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue, 'ext>.Primitive(PrimitiveValue.Int64(vClosure + v))
            } }

    let int64MinusId =
      Identifier.FullyQualified([ "int64" ], "-") |> TypeCheckScope.Empty.Resolve

    let minusOperation: ResolvedIdentifier * OperationExtension<'ext, Int64Operations<'ext>> =
      int64MinusId,
      { Type = TypeValue.CreateArrow(int64TypeValue, TypeValue.CreateArrow(int64TypeValue, int64TypeValue))
        Kind = Kind.Star
        Operation = Int64Operations.Minus {| v1 = None |}
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
                |> sum.MapError(Errors.FromErrors loc0)
                |> reader.OfSum

              let! v = v |> Value.AsPrimitive |> sum.MapError(Errors.FromErrors loc0) |> reader.OfSum

              let! v =
                v
                |> PrimitiveValue.AsInt64
                |> sum.MapError(Errors.FromErrors loc0)
                |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return Int64Operations.Minus({| v1 = Some v |}) |> operationLens.Set |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue, 'ext>.Primitive(PrimitiveValue.Int64(vClosure - v))
            } }

    let int64DivideId =
      Identifier.FullyQualified([ "int64" ], "/") |> TypeCheckScope.Empty.Resolve

    let divideOperation: ResolvedIdentifier * OperationExtension<'ext, Int64Operations<'ext>> =
      int64DivideId,
      { Type = TypeValue.CreateArrow(int64TypeValue, TypeValue.CreateArrow(int64TypeValue, int64TypeValue))
        Kind = Kind.Star
        Operation = Int64Operations.Divide {| v1 = None |}
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
                |> sum.MapError(Errors.FromErrors loc0)
                |> reader.OfSum

              let! v = v |> Value.AsPrimitive |> sum.MapError(Errors.FromErrors loc0) |> reader.OfSum

              let! v =
                v
                |> PrimitiveValue.AsInt64
                |> sum.MapError(Errors.FromErrors loc0)
                |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return Int64Operations.Divide({| v1 = Some v |}) |> operationLens.Set |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue, 'ext>.Primitive(PrimitiveValue.Int64(vClosure / v))
            } }

    let int64PowerId =
      Identifier.FullyQualified([ "int64" ], "**") |> TypeCheckScope.Empty.Resolve

    let powerOperation: ResolvedIdentifier * OperationExtension<'ext, Int64Operations<'ext>> =
      int64PowerId,
      { Type = TypeValue.CreateArrow(int64TypeValue, TypeValue.CreateArrow(int32TypeValue, int64TypeValue))
        Kind = Kind.Star
        Operation = Int64Operations.Power {| v1 = None |}
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
                |> sum.MapError(Errors.FromErrors loc0)
                |> reader.OfSum

              let! v = v |> Value.AsPrimitive |> sum.MapError(Errors.FromErrors loc0) |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                let! v =
                  v
                  |> PrimitiveValue.AsInt64
                  |> sum.MapError(Errors.FromErrors loc0)
                  |> reader.OfSum

                return Int64Operations.Power({| v1 = Some v |}) |> operationLens.Set |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application
                let! v =
                  v
                  |> PrimitiveValue.AsInt32
                  |> sum.MapError(Errors.FromErrors loc0)
                  |> reader.OfSum

                return Value<TypeValue, 'ext>.Primitive(PrimitiveValue.Int64(pown vClosure v))
            } }

    let int64ModId =
      Identifier.FullyQualified([ "int64" ], "%") |> TypeCheckScope.Empty.Resolve

    let modOperation: ResolvedIdentifier * OperationExtension<'ext, Int64Operations<'ext>> =
      int64ModId,
      { Type = TypeValue.CreateArrow(int64TypeValue, TypeValue.CreateArrow(int64TypeValue, int64TypeValue))
        Kind = Kind.Star
        Operation = Int64Operations.Mod {| v1 = None |}
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
                |> sum.MapError(Errors.FromErrors loc0)
                |> reader.OfSum

              let! v = v |> Value.AsPrimitive |> sum.MapError(Errors.FromErrors loc0) |> reader.OfSum

              let! v =
                v
                |> PrimitiveValue.AsInt64
                |> sum.MapError(Errors.FromErrors loc0)
                |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return Int64Operations.Mod({| v1 = Some v |}) |> operationLens.Set |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue, 'ext>.Primitive(PrimitiveValue.Int64(vClosure % v))
            } }

    let int64EqualId =
      Identifier.FullyQualified([ "int64" ], "==") |> TypeCheckScope.Empty.Resolve

    let equalOperation: ResolvedIdentifier * OperationExtension<'ext, Int64Operations<'ext>> =
      int64EqualId,
      { Type = TypeValue.CreateArrow(int64TypeValue, TypeValue.CreateArrow(int64TypeValue, boolTypeValue))
        Kind = Kind.Star
        Operation = Int64Operations.Equal {| v1 = None |}
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
                |> sum.MapError(Errors.FromErrors loc0)
                |> reader.OfSum

              let! v = v |> Value.AsPrimitive |> sum.MapError(Errors.FromErrors loc0) |> reader.OfSum

              let! v =
                v
                |> PrimitiveValue.AsInt64
                |> sum.MapError(Errors.FromErrors loc0)
                |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return Int64Operations.Equal({| v1 = Some v |}) |> operationLens.Set |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue, 'ext>.Primitive(PrimitiveValue.Bool(vClosure = v))
            } }

    let int64NotEqualId =
      Identifier.FullyQualified([ "int64" ], "!=") |> TypeCheckScope.Empty.Resolve

    let notEqualOperation: ResolvedIdentifier * OperationExtension<'ext, Int64Operations<'ext>> =
      int64NotEqualId,
      { Type = TypeValue.CreateArrow(int64TypeValue, TypeValue.CreateArrow(int64TypeValue, boolTypeValue))
        Kind = Kind.Star
        Operation = Int64Operations.NotEqual {| v1 = None |}
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
                |> sum.MapError(Errors.FromErrors loc0)
                |> reader.OfSum

              let! v = v |> Value.AsPrimitive |> sum.MapError(Errors.FromErrors loc0) |> reader.OfSum

              let! v =
                v
                |> PrimitiveValue.AsInt64
                |> sum.MapError(Errors.FromErrors loc0)
                |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return Int64Operations.NotEqual({| v1 = Some v |}) |> operationLens.Set |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue, 'ext>.Primitive(PrimitiveValue.Bool(vClosure <> v))
            } }

    let int64GreaterThanId =
      Identifier.FullyQualified([ "int64" ], ">") |> TypeCheckScope.Empty.Resolve

    let greaterThanOperation: ResolvedIdentifier * OperationExtension<'ext, Int64Operations<'ext>> =
      int64GreaterThanId,
      { Type = TypeValue.CreateArrow(int64TypeValue, TypeValue.CreateArrow(int64TypeValue, boolTypeValue))
        Kind = Kind.Star
        Operation = Int64Operations.GreaterThan {| v1 = None |}
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
                |> sum.MapError(Errors.FromErrors loc0)
                |> reader.OfSum

              let! v = v |> Value.AsPrimitive |> sum.MapError(Errors.FromErrors loc0) |> reader.OfSum

              let! v =
                v
                |> PrimitiveValue.AsInt64
                |> sum.MapError(Errors.FromErrors loc0)
                |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return Int64Operations.GreaterThan({| v1 = Some v |}) |> operationLens.Set |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue, 'ext>.Primitive(PrimitiveValue.Bool(vClosure > v))
            } }

    let int64GreaterThanOrEqualId =
      Identifier.FullyQualified([ "int64" ], ">=") |> TypeCheckScope.Empty.Resolve

    let greaterThanOrEqualOperation: ResolvedIdentifier * OperationExtension<'ext, Int64Operations<'ext>> =
      int64GreaterThanOrEqualId,
      { Type = TypeValue.CreateArrow(int64TypeValue, TypeValue.CreateArrow(int64TypeValue, boolTypeValue))
        Kind = Kind.Star
        Operation = Int64Operations.GreaterThanOrEqual {| v1 = None |}
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
                |> sum.MapError(Errors.FromErrors loc0)
                |> reader.OfSum

              let! v = v |> Value.AsPrimitive |> sum.MapError(Errors.FromErrors loc0) |> reader.OfSum

              let! v =
                v
                |> PrimitiveValue.AsInt64
                |> sum.MapError(Errors.FromErrors loc0)
                |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return
                  Int64Operations.GreaterThanOrEqual({| v1 = Some v |})
                  |> operationLens.Set
                  |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue, 'ext>.Primitive(PrimitiveValue.Bool(vClosure >= v))
            } }

    let int64LessThanId =
      Identifier.FullyQualified([ "int64" ], "<") |> TypeCheckScope.Empty.Resolve

    let lessThanOperation: ResolvedIdentifier * OperationExtension<'ext, Int64Operations<'ext>> =
      int64LessThanId,
      { Type = TypeValue.CreateArrow(int64TypeValue, TypeValue.CreateArrow(int64TypeValue, boolTypeValue))
        Kind = Kind.Star
        Operation = Int64Operations.LessThan {| v1 = None |}
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
                |> sum.MapError(Errors.FromErrors loc0)
                |> reader.OfSum

              let! v = v |> Value.AsPrimitive |> sum.MapError(Errors.FromErrors loc0) |> reader.OfSum

              let! v =
                v
                |> PrimitiveValue.AsInt64
                |> sum.MapError(Errors.FromErrors loc0)
                |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return Int64Operations.LessThan({| v1 = Some v |}) |> operationLens.Set |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue, 'ext>.Primitive(PrimitiveValue.Bool(vClosure < v))
            } }

    let int64LessThanOrEqualId =
      Identifier.FullyQualified([ "int64" ], "<=") |> TypeCheckScope.Empty.Resolve

    let lessThanOrEqualOperation: ResolvedIdentifier * OperationExtension<'ext, Int64Operations<'ext>> =
      int64LessThanOrEqualId,
      { Type = TypeValue.CreateArrow(int64TypeValue, TypeValue.CreateArrow(int64TypeValue, boolTypeValue))
        Kind = Kind.Star
        Operation = Int64Operations.LessThanOrEqual {| v1 = None |}
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
                |> sum.MapError(Errors.FromErrors loc0)
                |> reader.OfSum

              let! v = v |> Value.AsPrimitive |> sum.MapError(Errors.FromErrors loc0) |> reader.OfSum

              let! v =
                v
                |> PrimitiveValue.AsInt64
                |> sum.MapError(Errors.FromErrors loc0)
                |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return Int64Operations.LessThanOrEqual({| v1 = Some v |}) |> operationLens.Set |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue, 'ext>.Primitive(PrimitiveValue.Bool(vClosure <= v))
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
