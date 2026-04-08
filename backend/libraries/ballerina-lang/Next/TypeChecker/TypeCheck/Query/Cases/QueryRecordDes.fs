namespace Ballerina.DSL.Next.Types.TypeChecker

module QueryCaseRecordDes =
  open System
  open Ballerina
  open Ballerina.State.WithError
  open Ballerina.Collections.Map
  open Ballerina.Collections.Sum
  open Ballerina.LocalizedErrors
  open Ballerina.Errors
  open Ballerina.Cat.Collections.OrderedMap
  open Ballerina.StdLib.OrderPreservingMap
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Types.Patterns
  open Ballerina.DSL.Next.Terms.Model
  open Ballerina.DSL.Next.Terms.Patterns
  open Ballerina.DSL.Next.Types.TypeChecker.Model
  open Ballerina.DSL.Next.Types.TypeChecker.Eval
  open Ballerina.DSL.Next.Types.TypeChecker.LiftOtherSteps

  let private formatAvailableFieldNames (fieldNames: seq<string>) : string =
    let names = fieldNames |> Seq.distinct |> Seq.sort |> Seq.toList

    match names with
    | [] -> "none"
    | _ -> String.Join(", ", names)

  let typeCheckQueryRecordDes<'valueExt when 'valueExt: comparison>
    loc0
    (recur:
      ExprQueryExpr<TypeExpr<'valueExt>, Identifier, 'valueExt>
        -> TypeCheckerResult<
          (TypeCheckedExprQueryExpr<'valueExt> * TypeQueryRow<'valueExt>),
          'valueExt
         >)
    (expr: ExprQueryExpr<TypeExpr<'valueExt>, Identifier, 'valueExt>)
    (record: ExprQueryExpr<TypeExpr<'valueExt>, Identifier, 'valueExt>)
    (field: Identifier)
    : TypeCheckerResult<
        (TypeCheckedExprQueryExpr<'valueExt> * TypeQueryRow<'valueExt>),
        'valueExt
       >
    =
    let ofSum (p: Sum<'a, Errors<Unit>>) =
      p |> Sum.mapRight (Errors.MapContext(replaceWith loc0)) |> state.OfSum

    state {
      let! record_e, record_t = recur record

      return!
        state.Either
          (state {
            let! record_t = record_t |> TypeQueryRow.AsRecord |> ofSum

            return!
              state {
                let! field_t =
                  let availableFields =
                    record_t
                    |> Map.toSeq
                    |> Seq.map (fun (fieldId, _fieldType) -> fieldId.Name)
                    |> formatAvailableFieldNames

                  record_t
                  |> Map.tryFindWithError
                    (field.LocalName |> LocalIdentifier.Create)
                    "field in query record lookup"
                    (fun () ->
                      $"Type checking error: record lookup failed, field %s{field.LocalName} not found. Available fields: %s{availableFields}")
                    ()
                  |> ofSum

                return
                  TypeCheckedExprQueryExprRec.QueryRecordDes(
                    record_e,
                    field.LocalName |> ResolvedIdentifier.Create,
                    false
                  )
                  |> TypeCheckedExprQueryExpr.Create expr.Location,
                  field_t
              }
              |> state.MapError(
                Errors<_>.MapPriority(replaceWith ErrorPriority.High)
              )
          })
          (state {
            let! json_t = record_t |> TypeQueryRow.AsJson |> ofSum

            return!
              state.Either
                (state {
                  let! record_t = json_t |> TypeValue.AsRecord |> ofSum

                  let! field_id =
                    TypeCheckState.TryResolveIdentifier(field, loc0)

                  let! field_sym =
                    TypeCheckState.tryFindRecordFieldSymbol (field_id, loc0)
                    |> state.OfStateReader
                    |> Expr.liftTypeEval

                  let! field_t, _ =
                    let availableFields =
                      record_t
                      |> OrderedMap.toSeq
                      |> Seq.map (fun (fieldSym, _fieldType) ->
                        fieldSym.Name.LocalName)
                      |> formatAvailableFieldNames

                    record_t
                    |> OrderedMap.tryFindWithError
                      field_sym
                      "field in query record lookup"
                      ($"Type checking error: record lookup failed, field %s{field.LocalName} not found. Available fields: %s{availableFields}")
                    |> ofSum

                  return
                    TypeCheckedExprQueryExprRec.QueryRecordDes(
                      record_e,
                      field_id,
                      true
                    )
                    |> TypeCheckedExprQueryExpr.Create expr.Location,
                    field_t |> TypeQueryRow.Json
                })
                (state {
                  let! record_t = json_t |> TypeValue.AsRecord |> ofSum

                  let! _, (field_t, _) =
                    let availableFields =
                      record_t
                      |> OrderedMap.toSeq
                      |> Seq.map (fun (fieldSym, _fieldType) ->
                        fieldSym.Name.LocalName)
                      |> formatAvailableFieldNames

                    record_t
                    |> OrderedMap.toSeq
                    |> Seq.tryFind (fun (field_sym, _) ->
                      field_sym.Name = field)
                    |> sum.OfOption(
                      Errors.Singleton loc0 (fun () ->
                        $"Type checking error: record lookup failed, field %s{field.LocalName} not found. Available fields: %s{availableFields}")
                    )
                    |> state.OfSum

                  return
                    TypeCheckedExprQueryExprRec.QueryRecordDes(
                      record_e,
                      field.LocalName |> ResolvedIdentifier.Create,
                      true
                    )
                    |> TypeCheckedExprQueryExpr.Create expr.Location,
                    field_t |> TypeQueryRow.Json
                })
              |> state.MapError(
                Errors<_>.MapPriority(replaceWith ErrorPriority.High)
              )
          })
        |> state.MapError(Errors<_>.FilterHighestPriorityOnly)
    }
