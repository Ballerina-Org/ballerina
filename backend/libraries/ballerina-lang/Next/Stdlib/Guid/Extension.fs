namespace Ballerina.DSL.Next.StdLib.Guid

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
  open FSharp.Data

  let private boolTypeValue = TypeValue.CreatePrimitive PrimitiveType.Bool
  let private guidTypeValue = TypeValue.CreatePrimitive PrimitiveType.Guid
  let private stringTypeValue = TypeValue.CreatePrimitive PrimitiveType.String
  let private unitTypeValue = TypeValue.CreatePrimitive PrimitiveType.Unit

  let GuidExtension<'ext>
    (consLens: PartialLens<'ext, GuidConstructors>)
    (operationLens: PartialLens<'ext, GuidOperations<'ext>>)
    : TypeExtension<'ext, GuidConstructors, PrimitiveValue, GuidOperations<'ext>> =

    let guidId = Identifier.LocalScope "guid"
    let guidSymbolId = guidId |> TypeSymbol.Create
    let guidId = guidId |> TypeCheckScope.Empty.Resolve

    let guidConstructors = GuidConstructorsExtension<'ext> consLens

    let guidEqualId =
      Identifier.FullyQualified([ "guid" ], "==") |> TypeCheckScope.Empty.Resolve

    let equalOperation
      : ResolvedIdentifier * TypeOperationExtension<'ext, GuidConstructors, PrimitiveValue, GuidOperations<'ext>> =
      guidEqualId,
      { Type = TypeValue.CreateArrow(guidTypeValue, TypeValue.CreateArrow(guidTypeValue, boolTypeValue))
        Kind = Kind.Star
        Operation = GuidOperations.Equal {| v1 = None |}
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | GuidOperations.Equal v -> Some(GuidOperations.Equal v)
            | _ -> None)

        Apply =
          fun loc0 (op, v) ->
            reader {
              let! op =
                op
                |> GuidOperations.AsEqual
                |> sum.MapError(Errors.FromErrors loc0)
                |> reader.OfSum

              let! v = v |> Value.AsPrimitive |> sum.MapError(Errors.FromErrors loc0) |> reader.OfSum

              let! v =
                v
                |> PrimitiveValue.AsGuid
                |> sum.MapError(Errors.FromErrors loc0)
                |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return GuidOperations.Equal({| v1 = Some v |}) |> operationLens.Set |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue, 'ext>.Primitive(PrimitiveValue.Bool(vClosure = v))
            } }

    let guidNotEqualId =
      Identifier.FullyQualified([ "guid" ], "!=") |> TypeCheckScope.Empty.Resolve

    let notEqualOperation
      : ResolvedIdentifier * TypeOperationExtension<'ext, GuidConstructors, PrimitiveValue, GuidOperations<'ext>> =
      guidNotEqualId,
      { Type = TypeValue.CreateArrow(guidTypeValue, TypeValue.CreateArrow(guidTypeValue, boolTypeValue))
        Kind = Kind.Star
        Operation = GuidOperations.NotEqual {| v1 = None |}
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | GuidOperations.NotEqual v -> Some(GuidOperations.NotEqual v)
            | _ -> None)

        Apply =
          fun loc0 (op, v) ->
            reader {
              let! op =
                op
                |> GuidOperations.AsNotEqual
                |> sum.MapError(Errors.FromErrors loc0)
                |> reader.OfSum

              let! v = v |> Value.AsPrimitive |> sum.MapError(Errors.FromErrors loc0) |> reader.OfSum

              let! v =
                v
                |> PrimitiveValue.AsGuid
                |> sum.MapError(Errors.FromErrors loc0)
                |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return GuidOperations.NotEqual({| v1 = Some v |}) |> operationLens.Set |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue, 'ext>.Primitive(PrimitiveValue.Bool(vClosure <> v))
            } }

    { TypeVars = []
      TypeName = guidId, guidSymbolId
      Cases = guidConstructors |> Map.ofList
      WrapTypeVars =
        fun t ->
          match t with
          | TypeExpr.Arrow(TypeExpr.Primitive PrimitiveType.Unit, _) ->
            TypeValue.CreateArrow(unitTypeValue, guidTypeValue)
          | TypeExpr.Arrow(TypeExpr.Primitive PrimitiveType.String, _) ->
            TypeValue.CreateArrow(stringTypeValue, guidTypeValue)
          | TypeExpr.Imported _ -> guidTypeValue
          | _ -> failwith $"Expected a Arrow or Imported, got {t}"
      Deconstruct =
        fun (v) ->
          match v with
          | PrimitiveValue.Guid v -> Value.Primitive(PrimitiveValue.Guid v)
          | _ -> Value.Primitive(PrimitiveValue.Unit)
      Operations = [ equalOperation; notEqualOperation ] |> Map.ofList }
