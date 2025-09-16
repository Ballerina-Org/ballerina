namespace Ballerina.DSL.Next.StdLib.Bool

[<AutoOpen>]
module Extension =
  open Ballerina.Collections.Sum
  open Ballerina.Reader.WithError
  open Ballerina.Errors
  open Ballerina.DSL.Next.Terms.Model
  open Ballerina.DSL.Next.Terms.Patterns
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Types.Patterns
  open Ballerina.Lenses
  open Ballerina.DSL.Next.Extensions

  let BoolExtension<'ext>
    (operationLens: PartialLens<'ext, BoolOperations<'ext>>)
    : OperationsExtension<'ext, BoolOperations<'ext>> =

    let boolAndId = Identifier.FullyQualified([ "Bool" ], "&&")

    let andOperation: Identifier * OperationExtension<'ext, BoolOperations<'ext>> =
      boolAndId,
      { Type =
          TypeValue.Arrow(
            TypeValue.Primitive PrimitiveType.Bool,
            TypeValue.Arrow(TypeValue.Primitive PrimitiveType.Bool, TypeValue.Primitive PrimitiveType.Bool)
          )
        Kind = Kind.Star
        Operation = BoolOperations.And {| v1 = None |}
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | BoolOperations.And v -> Some(BoolOperations.And v)
            | _ -> None)

        Apply =
          fun (op, v) ->
            reader {
              let! op = op |> BoolOperations.AsAnd |> reader.OfSum
              let! v = v |> Value.AsPrimitive |> reader.OfSum
              let! v = v |> PrimitiveValue.AsBool |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return BoolOperations.And({| v1 = Some v |}) |> operationLens.Set |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue, 'ext>.Primitive(PrimitiveValue.Bool(vClosure && v))
            } }

    let boolOrId = Identifier.FullyQualified([ "Bool" ], "||")

    let orOperation: Identifier * OperationExtension<'ext, BoolOperations<'ext>> =
      boolOrId,
      { Type =
          TypeValue.Arrow(
            TypeValue.Primitive PrimitiveType.Bool,
            TypeValue.Arrow(TypeValue.Primitive PrimitiveType.Bool, TypeValue.Primitive PrimitiveType.Bool)
          )
        Kind = Kind.Star
        Operation = BoolOperations.Or {| v1 = None |}
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | BoolOperations.Or v -> Some(BoolOperations.Or v)
            | _ -> None)

        Apply =
          fun (op, v) ->
            reader {
              let! op = op |> BoolOperations.AsOr |> reader.OfSum
              let! v = v |> Value.AsPrimitive |> reader.OfSum
              let! v = v |> PrimitiveValue.AsBool |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return BoolOperations.Or({| v1 = Some v |}) |> operationLens.Set |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue, 'ext>.Primitive(PrimitiveValue.Bool(vClosure || v))
            } }

    let boolNotId = Identifier.FullyQualified([ "Bool" ], "!")

    let notOperation: Identifier * OperationExtension<'ext, BoolOperations<'ext>> =
      boolNotId,
      { Type = TypeValue.Arrow(TypeValue.Primitive PrimitiveType.Bool, TypeValue.Primitive PrimitiveType.Bool)
        Kind = Kind.Star
        Operation = BoolOperations.Not {| v1 = () |}
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | BoolOperations.Not v -> Some(BoolOperations.Not v)
            | _ -> None)

        Apply =
          fun (_, v) ->
            reader {
              let! v = v |> Value.AsPrimitive |> reader.OfSum
              let! v = v |> PrimitiveValue.AsBool |> reader.OfSum

              return Value<TypeValue, 'ext>.Primitive(PrimitiveValue.Bool(not v))
            } }

    { TypeVars = []
      Operations = [ andOperation; orOperation; notOperation ] |> Map.ofList }
