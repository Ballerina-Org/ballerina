namespace Ballerina.DSL.Next.StdLib.MemoryDB.Extension

#nowarn "0040"

[<AutoOpen>]
module DBRun =
  open Ballerina.StdLib.String
  open Ballerina.Collections.Option
  open Ballerina
  open Ballerina.Collections.Sum
  open Ballerina.Reader.WithError
  open Ballerina.LocalizedErrors
  open Ballerina.Errors
  open Ballerina.DSL.Next.Terms
  open Ballerina.DSL.Next.Terms.Patterns
  open Ballerina.DSL.Next.Types
  open Ballerina.DSL.Next.Types.Patterns
  open Ballerina.Lenses
  open Ballerina.DSL.Next.Extensions
  open FSharp.Data
  open Ballerina.Collections.NonEmptyList
  open Ballerina.DSL.Next.Types.TypeChecker
  open System
  open Ballerina.Cat.Collections.OrderedMap
  open Ballerina
  open Ballerina.DSL.Next.StdLib.MemoryDB

  let internal MemoryDBSchemaToDescriptors<'ext when 'ext: comparison>
    (valueLens: PartialLens<'ext, MemoryDBValues<'ext>>)
    (db: MutableMemoryDB<'ext>)
    (schema: Schema<'ext>)
    : Sum<Value<TypeValue<'ext>, 'ext>, Errors<Unit>> =
    sum {
      let! relation_values =
        schema.Relations
        |> OrderedMap.toSeq
        |> Seq.map (fun (k, v) ->
          sum {
            let! from =
              schema.Entities
              |> OrderedMap.tryFind (v.From.LocalName |> SchemaEntityName.Create)
              |> sum.OfOption(Errors.Singleton () (fun () -> $"Entity {v.From.LocalName} not found in schema"))

            let! to_ =
              schema.Entities
              |> OrderedMap.tryFind (v.To.LocalName |> SchemaEntityName.Create)
              |> sum.OfOption(Errors.Singleton () (fun () -> $"Entity {v.To.LocalName} not found in schema"))

            return k.Name |> ResolvedIdentifier.Create, (v, from, to_)
          })
        |> sum.All

      let rec res =
        [ "Entities" |> ResolvedIdentifier.Create,
          Value.Record(
            schema.Entities
            |> OrderedMap.toSeq
            |> Seq.map (fun (k, v) ->
              k.Name |> ResolvedIdentifier.Create,
              (MemoryDBValues.EntityRef(schema, db, v, { Value = lazy res }) |> valueLens.Set, None)
              |> Ext)
            |> Map.ofSeq
          )
          "Relations" |> ResolvedIdentifier.Create,
          Value.Record(
            relation_values
            |> Seq.map (fun (k, (v, from, to_)) ->
              k,
              Value.Record(
                [ "Relation" |> ResolvedIdentifier.Create,
                  (MemoryDBValues.RelationRef(schema, db, v, from, to_, { Value = lazy res })
                   |> valueLens.Set,
                   None)
                  |> Ext
                  "From" |> ResolvedIdentifier.Create,
                  (MemoryDBValues.RelationLookupRef(schema, db, RelationLookupDirection.FromTo, v, from, to_)
                   |> valueLens.Set,
                   None)
                  |> Ext
                  "To" |> ResolvedIdentifier.Create,
                  (MemoryDBValues.RelationLookupRef(schema, db, RelationLookupDirection.ToFrom, v, from, to_)
                   |> valueLens.Set,
                   None)
                  |> Ext ]
                |> Map.ofList
              ))
            |> Map.ofSeq
          ) ]
        |> Map.ofList
        |> Value.Record

      return res
    }


  let MemoryDBRunExtension<'ext, 'extDTO when 'ext: comparison and 'extDTO: not null and 'extDTO: not struct>
    (valueLens: PartialLens<'ext, MemoryDBValues<'ext>>)
    : TypeLambdaExtension<'ext, 'extDTO, MemoryDBValues<'ext>> =

    let dbIOId = Identifier.LocalScope "DBIO"
    let dbIOResolvedId = dbIOId |> TypeCheckScope.Empty.Resolve
    let dbIOSymbol = dbIOId |> TypeSymbol.Create

    let dbIOType: TypeValue<'ext> =
      TypeValue.Imported
        { Id = dbIOResolvedId
          Sym = dbIOSymbol
          Parameters =
            [ TypeParameter.Create("schema", Kind.Schema)
              TypeParameter.Create("result", Kind.Star) ]
          Arguments = []

        }

    let dbIOKind = Kind.Arrow(Kind.Schema, Kind.Arrow(Kind.Star, Kind.Star))

    let dbVectorId = Identifier.FullyQualified([ "MemoryDB" ], "Vector")
    let dbVectorResolvedId = dbVectorId |> TypeCheckScope.Empty.Resolve
    let dbVectorSymbol = dbVectorId |> TypeSymbol.Create

    let dbVectorType: TypeValue<'ext> =
      TypeValue.Imported
        { Id = dbVectorResolvedId
          Sym = dbVectorSymbol
          Parameters = []
          Arguments = []

        }

    let dbVectorKind = Kind.Star


    let memoryDBRunId =
      Identifier.FullyQualified([ "MemoryDB" ], "run") |> TypeCheckScope.Empty.Resolve

    let memoryDBRunType =
      TypeValue.CreateLambda(
        TypeParameter.Create("schema", Kind.Schema),
        TypeExpr.Lambda(
          TypeParameter.Create("result", Kind.Star),
          TypeExpr.Arrow(
            TypeExpr.Arrow(
              TypeExpr.Lookup("schema" |> Identifier.LocalScope),
              TypeExpr.Lookup("result" |> Identifier.LocalScope)
            ),
            TypeExpr.Apply(
              TypeExpr.Apply(
                TypeExpr.Lookup("DBIO" |> Identifier.LocalScope),
                TypeExpr.Lookup("schema" |> Identifier.LocalScope)
              ),
              TypeExpr.Lookup("result" |> Identifier.LocalScope)
            )
          )
        )
      )

    let memoryDBRunKind = Kind.Arrow(Kind.Schema, Kind.Arrow(Kind.Star, Kind.Star))

    let typeApply (typeValue: TypeValue<'ext>) : ExprEvaluator<'ext, Value<TypeValue<'ext>, 'ext>> =
      reader {
        let! schema =
          typeValue
          |> TypeValue.AsSchema
          |> Sum.mapRight (Errors.MapContext(replaceWith Location.Unknown))
          |> reader.OfSum

        return
          (MemoryDBValues.TypeAppliedRun(
            schema,
            { entities = Map.empty
              relations = Map.empty }
           )
           |> valueLens.Set,
           None)
          |> Ext
      }

    let evalToTypeApplicable
      (loc0: Location)
      (_rest: List<Expr<TypeValue<'ext>, ResolvedIdentifier, 'ext>>)
      (v: 'ext)
      : ExprEvaluator<'ext, ExtEvalResult<'ext>> =
      reader {
        let! v =
          valueLens.Get v
          |> sum.OfOption((fun () -> $"Error: cannot get value from extension") |> Errors.Singleton loc0)
          |> reader.OfSum

        do!
          v
          |> MemoryDBValues.AsRun
          |> sum.MapError(Errors.MapContext(replaceWith loc0))
          |> reader.OfSum

        return TypeApplicable(fun arg -> typeApply arg)
      }

    let apply
      (_loc0: Location)
      (schema: Schema<'ext>)
      (db: MutableMemoryDB<'ext>)
      (value: Value<TypeValue<'ext>, 'ext>)
      (_rest: List<Expr<TypeValue<'ext>, ResolvedIdentifier, 'ext>>)
      : ExprEvaluator<'ext, Value<TypeValue<'ext>, 'ext>> =
      reader {

        // return!
        //   Expr.Apply(
        //     Expr.FromValue(value, TypeValue.CreatePrimitive PrimitiveType.Unit, Kind.Star),
        //     Expr.FromValue(arg, TypeValue.CreatePrimitive PrimitiveType.Unit, Kind.Star)
        //   )
        //   |> fun e -> Expr.Eval(NonEmptyList.OfList(e, _rest))
        let! ctx = reader.GetContext()

        let! (schema_value: Value<TypeValue<'ext>, 'ext>) =
          MemoryDBSchemaToDescriptors<'ext> valueLens db schema
          |> sum.MapError(Errors.MapContext(replaceWith _loc0))
          |> reader.OfSum

        let result: MemoryDBIO<'ext> =
          { Schema = schema
            SchemaAsValue = schema_value
            DB = db
            EvalContext = ctx.Scope
            Main = value }

        return (result |> MemoryDBValues.DBIO |> valueLens.Set, None) |> Ext
      }

    let evalToApplicable
      (loc0: Location)
      (_rest: List<Expr<TypeValue<'ext>, ResolvedIdentifier, 'ext>>)
      (v: 'ext)
      : ExprEvaluator<'ext, ExtEvalResult<'ext>> =
      reader {
        let! v =
          valueLens.Get v
          |> sum.OfOption((fun () -> $"Error: cannot get value from extension") |> Errors.Singleton loc0)
          |> reader.OfSum

        let! schema, db =
          v
          |> MemoryDBValues.AsTypeAppliedRun
          |> sum.MapError(Errors.MapContext(replaceWith loc0))
          |> reader.OfSum

        return Applicable(fun arg -> apply loc0 schema db arg _rest)
      }

    { ExtensionType = memoryDBRunId, memoryDBRunType, memoryDBRunKind
      ExtraBindings =
        [ (dbIOResolvedId, (dbIOType, dbIOKind))
          (dbVectorResolvedId, (dbVectorType, dbVectorKind)) ]
        |> Map.ofList
      Value = MemoryDBValues.Run
      ValueLens = valueLens
      EvalToTypeApplicable = evalToTypeApplicable
      EvalToApplicable = evalToApplicable }
