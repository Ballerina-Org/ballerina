namespace Ballerina.DSL.Next.StdLib.Option

[<AutoOpen>]
module Extension =
  open Ballerina.Collections.Sum
  open Ballerina.Reader.WithError
  open Ballerina.LocalizedErrors
  open Ballerina.DSL.Next.Terms
  open Ballerina.DSL.Next.Terms.Patterns
  open Ballerina.DSL.Next.Types
  open Ballerina.DSL.Next.Types.Patterns
  open Ballerina.Lenses
  open Ballerina.DSL.Next.Extensions
  open Ballerina.DSL.Next.StdLib.Option
  open FSharp.Data

  let OptionExtension<'ext>
    (valueLens: PartialLens<'ext, OptionValues<'ext>>)
    (consLens: PartialLens<'ext, OptionConstructors>)
    (operationLens: PartialLens<'ext, OptionOperations<'ext>>)
    : TypeExtension<'ext, OptionConstructors, OptionValues<'ext>, OptionOperations<'ext>> =
    let optionId = Identifier.LocalScope "Option"
    let optionSymbolId = optionId |> TypeSymbol.Create
    let optionSomeId = Identifier.FullyQualified([ "Option" ], "Some")
    let optionNoneId = Identifier.FullyQualified([ "Option" ], "None")
    let optionSomeSymbol = optionSomeId |> TypeSymbol.Create
    let optionNoneSymbol = optionNoneId |> TypeSymbol.Create
    let aVar, aKind = TypeVar.Create("a"), Kind.Star
    let optionId = optionId |> TypeCheckScope.Empty.Resolve
    let optionSomeId = optionSomeId |> TypeCheckScope.Empty.Resolve
    let optionNoneId = optionNoneId |> TypeCheckScope.Empty.Resolve

    let optionMapId =
      Identifier.FullyQualified([ "Option" ], "map") |> TypeCheckScope.Empty.Resolve

    let someCase =
      (optionSomeId, optionSomeSymbol),
      { CaseType = TypeExpr.Lookup(Identifier.LocalScope aVar.Name)
        ConstructorType =
          TypeValue.CreateLambda(
            TypeParameter.Create(aVar.Name, aKind),
            TypeExpr.Arrow(
              TypeExpr.Lookup(Identifier.LocalScope aVar.Name),
              TypeExpr.Apply(
                TypeExpr.Lookup(Identifier.LocalScope "Option"),
                TypeExpr.Lookup(Identifier.LocalScope aVar.Name)
              )
            )
          )
        Constructor = Option_Some
        Apply = fun _loc0 _rest (_, v) -> reader { return OptionValues.Option(Some v) |> valueLens.Set |> Ext }
        ValueLens =
          valueLens
          |> PartialLens.BindGet (function
            | OptionValues.Option(Some v) -> Some(OptionValues.Option(Some v))
            | _ -> None)
        ConsLens =
          consLens
          |> PartialLens.BindGet (function
            | Option_Some -> Some Option_Some
            | _ -> None) }

    let noneCase =
      (optionNoneId, optionNoneSymbol),
      { CaseType = TypeExpr.Primitive PrimitiveType.Unit
        ConstructorType =
          TypeValue.CreateLambda(
            TypeParameter.Create(aVar.Name, aKind),
            TypeExpr.Arrow(
              TypeExpr.Primitive PrimitiveType.Unit,
              TypeExpr.Apply(
                TypeExpr.Lookup(Identifier.LocalScope "Option"),
                TypeExpr.Lookup(Identifier.LocalScope aVar.Name)
              )
            )
          )
        Constructor = Option_None
        Apply = fun _loc0 _rest (_, _) -> reader { return OptionValues.Option None |> valueLens.Set |> Ext }
        ValueLens =
          valueLens
          |> PartialLens.BindGet (function
            | OptionValues.Option(None) -> Some(OptionValues.Option None)
            | _ -> None)
        ConsLens =
          consLens
          |> PartialLens.BindGet (function
            | Option_None -> Some Option_None
            | _ -> None) }

    let mapOperation
      : ResolvedIdentifier *
        TypeOperationExtension<'ext, OptionConstructors, OptionValues<'ext>, OptionOperations<'ext>> =
      optionMapId,
      { Type =
          TypeValue.CreateLambda(
            TypeParameter.Create("a", aKind),
            TypeExpr.Lambda(
              TypeParameter.Create("b", Kind.Star),
              TypeExpr.Arrow(
                TypeExpr.Arrow(TypeExpr.Lookup(Identifier.LocalScope "a"), TypeExpr.Lookup(Identifier.LocalScope "b")),
                TypeExpr.Arrow(
                  TypeExpr.Apply(
                    TypeExpr.Lookup(Identifier.LocalScope "Option"),
                    TypeExpr.Lookup(Identifier.LocalScope "a")
                  ),
                  TypeExpr.Apply(
                    TypeExpr.Lookup(Identifier.LocalScope "Option"),
                    TypeExpr.Lookup(Identifier.LocalScope "b")
                  )
                )
              )
            )
          )
        Kind = Kind.Arrow(Kind.Star, Kind.Arrow(Kind.Star, Kind.Star))
        Operation = Option_Map {| f = None |}
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | Option_Map v -> Some(Option_Map v))
        Apply =
          fun loc0 rest (op, v) ->
            reader {
              let! op =
                op
                |> OptionOperations.AsMap
                |> sum.MapError(Errors.FromErrors loc0)
                |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return OptionOperations.Option_Map({| f = Some v |}) |> operationLens.Set |> Ext
              | Some f -> // the closure has the function - second step in the application
                let! v = v |> Value.AsExt |> sum.MapError(Errors.FromErrors loc0) |> reader.OfSum

                let! v =
                  valueLens.Get v
                  |> sum.OfOption((loc0, "cannot get option value") |> Errors.Singleton)
                  |> reader.OfSum

                let! v =
                  v
                  |> OptionValues.AsOption
                  |> sum.MapError(Errors.FromErrors loc0)
                  |> reader.OfSum

                let! v' =
                  v
                  |> FSharp.Core.Option.map (fun v -> Expr.EvalApply loc0 rest (f, v))
                  |> reader.RunOption

                return OptionValues.Option v' |> valueLens.Set |> Ext
            } //: 'extOperations * Value<TypeValue, 'ext> -> ExprEvaluator<'ext, 'extValues> }
      }

    { TypeName = optionId, optionSymbolId
      TypeVars = [ (aVar, aKind) ]
      Cases = [ someCase; noneCase ] |> Map.ofList
      Operations = [ mapOperation ] |> Map.ofList
      Deconstruct =
        fun (v: OptionValues<'ext>) ->
          match v with
          | OptionValues.Option(Some v) -> v
          | _ -> Value<TypeValue, 'ext>.Primitive PrimitiveValue.Unit }
