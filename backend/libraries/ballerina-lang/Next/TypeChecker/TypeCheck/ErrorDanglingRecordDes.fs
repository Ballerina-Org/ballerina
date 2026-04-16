namespace Ballerina.DSL.Next.Types.TypeChecker

module ErrorDanglingRecordDes =
  open Ballerina
  open Ballerina.State.WithError
  open Ballerina.LocalizedErrors
  open Ballerina.Errors
  open System
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Types.Patterns
  open Ballerina.DSL.Next.Terms.Model
  open Ballerina.DSL.Next.Terms.Patterns
  open Ballerina.DSL.Next.Types.TypeChecker.Model
  open Ballerina.DSL.Next.Types.TypeChecker.Patterns
  open Ballerina.StdLib.OrderPreservingMap
  open Ballerina.Cat.Collections.OrderedMap

  type Expr<'T, 'Id, 've when 'Id: comparison> with
    static member internal TypeCheckErrorDanglingRecordDes<'valueExt
      when 'valueExt: comparison>
      (typeCheckExpr: ExprTypeChecker<'valueExt>)
      (context_t: Option<TypeValue<'valueExt>>)
      (record_expr: Expr<TypeExpr<'valueExt>, Identifier, 'valueExt>)
      (loc0: Location)
      : TypeCheckerResult<
          TypeCheckedExpr<'valueExt> * TypeCheckContext<'valueExt>,
          'valueExt
         >
      =
        let (!) = typeCheckExpr context_t

        state {
          let! ctx = state.GetContext()
          let! record_v, _ = !record_expr
          let record_t = record_v.Type
          let record_k = record_v.Kind

          match record_k, record_t with
          | Kind.Star, TypeValue.Record { value = fields_t } ->
            let availableFieldsMap =
              fields_t
              |> OrderedMap.toSeq
              |> Seq.map (fun (fieldSym, (fieldType, _)) ->
                fieldSym.Name.LocalName, fieldType)
              |> Map.ofSeq

            do!
              TypeCheckState.bindDotAccessHint(
                loc0,
                record_t,
                availableFieldsMap
              )
          | Kind.Schema, TypeValue.Schema schema_t ->
            let availableFieldsMap =
              [ "Entities", TypeValue.CreateEntities(schema_t)
                "Relations", TypeValue.CreateRelations(schema_t) ]
              |> Map.ofList

            do!
              TypeCheckState.bindDotAccessHint(
                loc0,
                record_t,
                availableFieldsMap
              )
          | Kind.Star, TypeValue.Entities schema_t ->
            let availableFieldsMap =
              schema_t.Entities
              |> OrderedMap.toSeq
              |> Seq.map (fun (name, _) ->
                name.Name, TypeValue.CreateUnit())
              |> Map.ofSeq

            do!
              TypeCheckState.bindDotAccessHint(
                loc0,
                record_t,
                availableFieldsMap
              )
          | Kind.Star, TypeValue.Relations schema_t ->
            let availableFieldsMap =
              schema_t.Relations
              |> OrderedMap.toSeq
              |> Seq.map (fun (name, _) ->
                name.Name, TypeValue.CreateUnit())
              |> Map.ofSeq

            do!
              TypeCheckState.bindDotAccessHint(
                loc0,
                record_t,
                availableFieldsMap
              )
          | Kind.Star, TypeValue.Relation(_, _, _, _, _, _, _, _, _) ->
            let availableFieldsMap =
              [ "From", TypeValue.CreateUnit()
                "To", TypeValue.CreateUnit() ]
              |> Map.ofList

            do!
              TypeCheckState.bindDotAccessHint(
                loc0,
                record_t,
                availableFieldsMap
              )
          | _ -> ()

          return
            { TypeCheckedExpr.Expr =
                TypeCheckedExprRec.ErrorDanglingRecordDes(
                  { TypeCheckedExprErrorDanglingRecordDes.Expr = record_v
                    Field = None }
                )
              Location = loc0
              Type = TypeValue.CreateUnit()
              Kind = Kind.Star
              Scope = ctx.Scope },
            ctx
        }
