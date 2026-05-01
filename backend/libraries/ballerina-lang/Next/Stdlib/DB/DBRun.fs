namespace Ballerina.DSL.Next.StdLib.DB.Extension

#nowarn "0040"
#nowarn "21"

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
  open Ballerina.DSL.Next.StdLib.DB

  let MemoryDBSchemaToDescriptors<'runtimeContext, 'db, 'ext
    when 'ext: comparison>
    (valueLens: PartialLens<'ext, DBValues<'runtimeContext, 'db, 'ext>>)
    (db: 'db)
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
              |> OrderedMap.tryFind (
                v.From.LocalName |> SchemaEntityName.Create
              )
              |> sum.OfOption(
                Errors.Singleton () (fun () ->
                  $"Entity {v.From.LocalName} not found in schema")
              )

            let! to_ =
              schema.Entities
              |> OrderedMap.tryFind (v.To.LocalName |> SchemaEntityName.Create)
              |> sum.OfOption(
                Errors.Singleton () (fun () ->
                  $"Entity {v.To.LocalName} not found in schema")
              )

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
              (DBValues.EntityRef(schema, db, v, { Value = lazy res })
               |> valueLens.Set,
               None)
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
                  (DBValues.RelationRef(
                    schema,
                    db,
                    v,
                    from,
                    to_,
                    { Value = lazy res }
                   )
                   |> valueLens.Set,
                   None)
                  |> Ext
                  "From" |> ResolvedIdentifier.Create,
                  (DBValues.RelationLookupRef(
                    (schema, db, v, from, to_, { Value = lazy res }),
                    RelationLookupDirection.FromTo
                   )
                   |> valueLens.Set,
                   None)
                  |> Ext
                  "To" |> ResolvedIdentifier.Create,
                  (DBValues.RelationLookupRef(
                    (schema, db, v, from, to_, { Value = lazy res }),
                    RelationLookupDirection.ToFrom
                   )
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

  let BuildDBIO<'runtimeContext, 'db, 'ext when 'ext: comparison>
    (loc0: Location)
    (valueLens: PartialLens<'ext, DBValues<'runtimeContext, 'db, 'ext>>)
    (db: 'db)
    (schema: Schema<'ext>)
    (value: Value<TypeValue<'ext>, 'ext>)
    : ExprEvaluator<'runtimeContext, 'ext, DBIO<'runtimeContext, 'db, 'ext>> =
    reader {
      let! ctx = reader.GetContext()

      let! schema_value =
        MemoryDBSchemaToDescriptors<'runtimeContext, 'db, 'ext>
          valueLens
          db
          schema
        |> sum.MapError(Errors.MapContext(replaceWith loc0))
        |> reader.OfSum

      return
        { Schema = schema
          SchemaAsValue = schema_value
          DB = db
          EvalContext = ctx.Scope
          Main = value }
    }


  let DBRunExtension<'runtimeContext, 'db, 'ext, 'extDTO
    when 'ext: comparison and 'extDTO: not null and 'extDTO: not struct>
    (db_ops: DBTypeClass<'runtimeContext, 'db, 'ext>)
    (valueLens: PartialLens<'ext, DBValues<'runtimeContext, 'db, 'ext>>)
    : TypeLambdaExtension<
        'runtimeContext,
        'ext,
        'extDTO,
        DBValues<'runtimeContext, 'db, 'ext>
       >
    =

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


    let memoryDBRunId =
      Identifier.FullyQualified([ "DB" ], "run") |> TypeCheckScope.Empty.Resolve

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

    let memoryDBRunKind =
      Kind.Arrow(Kind.Schema, Kind.Arrow(Kind.Star, Kind.Star))

    let typeApply
      (typeValue: TypeValue<'ext>)
      : ExprEvaluator<'runtimeContext, 'ext, Value<TypeValue<'ext>, 'ext>> =
      reader {
        let! schema =
          typeValue
          |> TypeValue.AsSchema
          |> Sum.mapRight (Errors.MapContext(replaceWith Location.Unknown))
          |> reader.OfSum

        return
          (DBValues.TypeAppliedRun(schema, db_ops.DB) |> valueLens.Set, None)
          |> Ext
      }

    let evalToTypeApplicable
      (loc0: Location)
      (_rest: List<RunnableExpr<'ext>>)
      (v: 'ext)
      : ExprEvaluator<
          'runtimeContext,
          'ext,
          ExtEvalResult<'runtimeContext, 'ext>
         >
      =
      reader {
        let! v =
          valueLens.Get v
          |> sum.OfOption(
            (fun () -> $"Error: cannot get value from extension")
            |> Errors.Singleton loc0
          )
          |> reader.OfSum

        do!
          v
          |> DBValues.AsRun
          |> sum.MapError(Errors.MapContext(replaceWith loc0))
          |> reader.OfSum

        return TypeApplicable(fun arg -> typeApply arg)
      }

    let apply
      (_loc0: Location)
      (schema: Schema<'ext>)
      (db: 'db)
      (value: Value<TypeValue<'ext>, 'ext>)
      (_rest: List<RunnableExpr<'ext>>)
      : ExprEvaluator<'runtimeContext, 'ext, Value<TypeValue<'ext>, 'ext>> =
      reader {
        let! result =
          BuildDBIO<'runtimeContext, 'db, 'ext>
            _loc0
            valueLens
            db
            schema
            value

        return (result |> DBValues.DBIO |> valueLens.Set, None) |> Ext
      }

    let evalToApplicable
      (loc0: Location)
      (_rest: List<RunnableExpr<'ext>>)
      (v: 'ext)
      : ExprEvaluator<
          'runtimeContext,
          'ext,
          ExtEvalResult<'runtimeContext, 'ext>
         >
      =
      reader {
        let! v =
          valueLens.Get v
          |> sum.OfOption(
            (fun () -> $"Error: cannot get value from extension")
            |> Errors.Singleton loc0
          )
          |> reader.OfSum

        let! schema, db =
          v
          |> DBValues.AsTypeAppliedRun
          |> sum.MapError(Errors.MapContext(replaceWith loc0))
          |> reader.OfSum

        return Applicable(fun arg -> apply loc0 schema db arg _rest)
      }

    { ExtensionType = memoryDBRunId, memoryDBRunType, memoryDBRunKind
      ExtraBindings = [ (dbIOResolvedId, (dbIOType, dbIOKind)) ] |> Map.ofList
      Value = DBValues.Run
      ValueLens = valueLens
      EvalToTypeApplicable = evalToTypeApplicable
      EvalToApplicable = evalToApplicable }
