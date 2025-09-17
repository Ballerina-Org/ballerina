namespace Ballerina.DSL.Next.StdLib.Guid

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
  open Ballerina.DSL.Next.StdLib.Option

  let GuidExtension<'ext>
    (operationLens: PartialLens<'ext, GuidOperations<'ext>>)
    : OperationsExtension<'ext, GuidOperations<'ext>> =

    let guidEqualId = Identifier.FullyQualified([ "Guid" ], "==")

    let equalOperation: Identifier * OperationExtension<'ext, GuidOperations<'ext>> =
      guidEqualId,
      { Type =
          TypeValue.Arrow(
            TypeValue.Primitive PrimitiveType.Guid,
            TypeValue.Arrow(TypeValue.Primitive PrimitiveType.Guid, TypeValue.Primitive PrimitiveType.Bool)
          )
        Kind = Kind.Star
        Operation = GuidOperations.Equal {| v1 = None |}
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | GuidOperations.Equal v -> Some(GuidOperations.Equal v)
            | _ -> None)

        Apply =
          fun (op, v) ->
            reader {
              let! op = op |> GuidOperations.AsEqual |> reader.OfSum
              let! v = v |> Value.AsPrimitive |> reader.OfSum
              let! v = v |> PrimitiveValue.AsGuid |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return GuidOperations.Equal({| v1 = Some v |}) |> operationLens.Set |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue, 'ext>.Primitive(PrimitiveValue.Bool(vClosure = v))
            } }

    let guidNotEqualId = Identifier.FullyQualified([ "Guid" ], "!=")

    let notEqualOperation: Identifier * OperationExtension<'ext, GuidOperations<'ext>> =
      guidNotEqualId,
      { Type =
          TypeValue.Arrow(
            TypeValue.Primitive PrimitiveType.Guid,
            TypeValue.Arrow(TypeValue.Primitive PrimitiveType.Guid, TypeValue.Primitive PrimitiveType.Bool)
          )
        Kind = Kind.Star
        Operation = GuidOperations.NotEqual {| v1 = None |}
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | GuidOperations.NotEqual v -> Some(GuidOperations.NotEqual v)
            | _ -> None)

        Apply =
          fun (op, v) ->
            reader {
              let! op = op |> GuidOperations.AsNotEqual |> reader.OfSum
              let! v = v |> Value.AsPrimitive |> reader.OfSum
              let! v = v |> PrimitiveValue.AsGuid |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return GuidOperations.NotEqual({| v1 = Some v |}) |> operationLens.Set |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue, 'ext>.Primitive(PrimitiveValue.Bool(vClosure <> v))
            } }

    { TypeVars = []
      Operations = [ equalOperation; notEqualOperation ] |> Map.ofList }
