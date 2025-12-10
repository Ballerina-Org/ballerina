namespace Ballerina.DSL.Next.StdLib.Bool

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

  let private boolTypeValue = TypeValue.CreateBool()

  let BoolExtension<'ext>
    (operationLens: PartialLens<'ext, BoolOperations<'ext>>)
    : OperationsExtension<'ext, BoolOperations<'ext>> =

    let boolAndId =
      Identifier.FullyQualified([ "bool" ], "&&") |> TypeCheckScope.Empty.Resolve

    let andOperation: ResolvedIdentifier * OperationExtension<'ext, BoolOperations<'ext>> =
      boolAndId,
      { Type = TypeValue.CreateArrow(boolTypeValue, TypeValue.CreateArrow(boolTypeValue, boolTypeValue))
        Kind = Kind.Star
        Operation = BoolOperations.And {| v1 = None |}
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | BoolOperations.And v -> Some(BoolOperations.And v)
            | _ -> None)

        Apply =
          fun loc _rest (op, v) ->
            reader {
              let! op =
                op
                |> BoolOperations.AsAnd
                |> sum.MapError(Errors.FromErrors loc)
                |> reader.OfSum

              let! v = v |> Value.AsPrimitive |> sum.MapError(Errors.FromErrors loc) |> reader.OfSum

              let! v =
                v
                |> PrimitiveValue.AsBool
                |> sum.MapError(Errors.FromErrors loc)
                |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return BoolOperations.And({| v1 = Some v |}) |> operationLens.Set |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue, 'ext>.Primitive(PrimitiveValue.Bool(vClosure && v))
            } }

    let boolOrId =
      Identifier.FullyQualified([ "bool" ], "||") |> TypeCheckScope.Empty.Resolve

    let orOperation: ResolvedIdentifier * OperationExtension<'ext, BoolOperations<'ext>> =
      boolOrId,
      { Type = TypeValue.CreateArrow(boolTypeValue, TypeValue.CreateArrow(boolTypeValue, boolTypeValue))
        Kind = Kind.Star
        Operation = BoolOperations.Or {| v1 = None |}
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | BoolOperations.Or v -> Some(BoolOperations.Or v)
            | _ -> None)

        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              let! op =
                op
                |> BoolOperations.AsOr
                |> sum.MapError(Errors.FromErrors loc0)
                |> reader.OfSum

              let! v = v |> Value.AsPrimitive |> sum.MapError(Errors.FromErrors loc0) |> reader.OfSum

              let! v =
                v
                |> PrimitiveValue.AsBool
                |> sum.MapError(Errors.FromErrors loc0)
                |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return BoolOperations.Or({| v1 = Some v |}) |> operationLens.Set |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue, 'ext>.Primitive(PrimitiveValue.Bool(vClosure || v))
            } }

    let boolNotId =
      Identifier.FullyQualified([ "bool" ], "!") |> TypeCheckScope.Empty.Resolve

    let notOperation: ResolvedIdentifier * OperationExtension<'ext, BoolOperations<'ext>> =
      boolNotId,
      { Type = TypeValue.CreateArrow(boolTypeValue, boolTypeValue)
        Kind = Kind.Star
        Operation = BoolOperations.Not {| v1 = () |}
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | BoolOperations.Not v -> Some(BoolOperations.Not v)
            | _ -> None)

        Apply =
          fun loc0 _rest (_, v) ->
            reader {
              let! v = v |> Value.AsPrimitive |> sum.MapError(Errors.FromErrors loc0) |> reader.OfSum

              let! v =
                v
                |> PrimitiveValue.AsBool
                |> sum.MapError(Errors.FromErrors loc0)
                |> reader.OfSum

              return Value<TypeValue, 'ext>.Primitive(PrimitiveValue.Bool(not v))
            } }

    { TypeVars = []
      Operations = [ andOperation; orOperation; notOperation ] |> Map.ofList }
