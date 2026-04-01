namespace Ballerina.DSL.Next.StdLib.DB.Extension

#nowarn "0040"

[<AutoOpen>]
module DBRunQuery =
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

  let DBRunQueryExtension<'runtimeContext, 'db, 'ext, 'extDTO
    when 'ext: comparison and 'extDTO: not null and 'extDTO: not struct>
    (db_ops: DBTypeClass<'runtimeContext, 'db, 'ext>)
    (listSet: List<Value<TypeValue<'ext>, 'ext>> -> 'ext)
    (valueLens: PartialLens<'ext, DBValues<'runtimeContext, 'db, 'ext>>)
    (queryTypeSymbol: Option<TypeSymbol>)
    : TypeLambdaExtension<'runtimeContext, 'ext, 'extDTO, DBValues<'runtimeContext, 'db, 'ext>> *
      TypeSymbol *
      (Schema<'ext> -> TypeQueryRow<'ext> -> TypeValue<'ext>)
    =

    let dbQueryId = Identifier.FullyQualified([ "DB" ], "Query")
    let dbQueryResolvedId = dbQueryId |> TypeCheckScope.Empty.Resolve

    let dbQuerySymbol =
      queryTypeSymbol |> Option.defaultWith (fun () -> dbQueryId |> TypeSymbol.Create)

    let dbQueryType: TypeValue<'ext> =
      TypeValue.Imported
        { Id = dbQueryResolvedId
          Sym = dbQuerySymbol
          Parameters =
            [ TypeParameter.Create("schema", Kind.Schema)
              TypeParameter.Create("row", Kind.QueryRow) ]
          Arguments = []

        }

    let make_dbQueryType =
      fun s qr ->
        TypeValue.Imported
          { Id = dbQueryResolvedId
            Sym = dbQuerySymbol
            Parameters = []
            //  TypeParameter.Create("schema", Kind.Schema)
            //   TypeParameter.Create("row", Kind.QueryRow) ]
            Arguments = [ TypeValue.Schema s; TypeValue.QueryRow qr ]

          }

    let dbQueryKind = Kind.Arrow(Kind.Schema, Kind.Arrow(Kind.QueryRow, Kind.Star))


    let memoryDBRunId =
      Identifier.FullyQualified([ "DB" ], "runQuery") |> TypeCheckScope.Empty.Resolve

    // runQuery : [s:Schema] [qt:QueryRow] => Query[s][qt] -> () + int*int -> List[QueryToType[qt]]
    let memoryDBRunType =
      TypeValue.CreateLambda(
        TypeParameter.Create("schema", Kind.Schema),
        TypeExpr.Lambda(
          TypeParameter.Create("row", Kind.QueryRow),
          TypeExpr.Arrow(
            TypeExpr.Apply(
              TypeExpr.Apply(TypeExpr.Lookup(dbQueryId), TypeExpr.Lookup("schema" |> Identifier.LocalScope)),
              TypeExpr.Lookup("row" |> Identifier.LocalScope)
            ),
            TypeExpr.Arrow(
              TypeExpr.Sum
                [ TypeExpr.Primitive PrimitiveType.Unit
                  TypeExpr.Tuple
                    [ TypeExpr.Primitive PrimitiveType.Int32
                      TypeExpr.Primitive PrimitiveType.Int32 ] ],
              TypeExpr.Apply(
                TypeExpr.Lookup("List" |> Identifier.LocalScope),
                TypeExpr.Apply(TypeExpr.FromQueryRow, TypeExpr.Lookup("row" |> Identifier.LocalScope))
              )
            )
          )
        )
      )

    let memoryDBRunKind = Kind.Arrow(Kind.Schema, Kind.Arrow(Kind.QueryRow, Kind.Star))

    let apply loc0 (query: Option<ValueQuery<TypeValue<'ext>, 'ext>>) value =
      reader {
        // let! ctx = reader.GetContext()

        match query with
        | None ->
          let! q =
            value
            |> Value.AsQuery
            |> sum.MapError(Errors.MapContext(replaceWith loc0))
            |> reader.OfSum

          return
            (DBValues.QueryRun {| Query = Some q |} |> valueLens.Set, Some memoryDBRunId)
            |> Ext
        | Some q ->
          // unpack value into range, should either be 1Of2() or 2Of2(skip, take)
          match value with
          | Value.Sum({ Case = 1; Count = 2 }, Value.Primitive PrimitiveValue.Unit) ->
            let! res =
              db_ops.RunQuery q None
              // |> Reader.mapContext (replaceWith ctx.RuntimeContext)
              |> Reader.mapError (Errors.MapContext(replaceWith loc0))

            return (res |> listSet, None) |> Ext
          | Value.Sum({ Case = 2; Count = 2 },
                      Value.Tuple [ Value.Primitive(PrimitiveValue.Int32 skip)
                                    Value.Primitive(PrimitiveValue.Int32 take) ]) ->
            let! res =
              db_ops.RunQuery q (Some(skip, take))
              // |> Reader.mapContext (replaceWith ctx.RuntimeContext)
              |> Reader.mapError (Errors.MapContext(replaceWith loc0))

            return (res |> listSet, None) |> Ext
          | _ ->
            return!
              (fun () -> $"Error (extension runtime): unexpected range pattern")
              |> Errors.Singleton loc0
              |> reader.Throw
      }

    let evalToTypeApplicable
      (loc0: Location)
      (_rest: List<Expr<TypeValue<'ext>, ResolvedIdentifier, 'ext>>)
      (v: 'ext)
      : ExprEvaluator<'runtimeContext, 'ext, ExtEvalResult<'runtimeContext, 'ext>> =
      reader {
        let! db_value =
          valueLens.Get v
          |> sum.OfOption((fun () -> $"Error: cannot get value from extension") |> Errors.Singleton loc0)
          |> reader.OfSum

        let! query =
          db_value
          |> DBValues.AsQueryRun
          |> sum.MapError(Errors.MapContext(replaceWith loc0))
          |> reader.OfSum

        return Applicable(apply loc0 query)
      }


    let evalToApplicable
      (loc0: Location)
      (_rest: List<Expr<TypeValue<'ext>, ResolvedIdentifier, 'ext>>)
      (v: 'ext)
      : ExprEvaluator<'runtimeContext, 'ext, ExtEvalResult<'runtimeContext, 'ext>> =
      reader {
        let! v =
          v
          |> valueLens.Get
          |> sum.OfOption((fun () -> $"Error: cannot get value from extension") |> Errors.Singleton loc0)
          |> reader.OfSum

        let! query =
          v
          |> DBValues.AsQueryRun
          |> sum.MapError(Errors.MapContext(replaceWith loc0))
          |> reader.OfSum

        return Applicable(apply loc0 query)
      }

    { ExtensionType = memoryDBRunId, memoryDBRunType, memoryDBRunKind
      ExtraBindings = [ (dbQueryResolvedId, (dbQueryType, dbQueryKind)) ] |> Map.ofList
      Value = DBValues.QueryRun {| Query = None |}
      ValueLens = valueLens
      EvalToTypeApplicable = evalToTypeApplicable
      EvalToApplicable = evalToApplicable },
    dbQuerySymbol,
    make_dbQueryType
