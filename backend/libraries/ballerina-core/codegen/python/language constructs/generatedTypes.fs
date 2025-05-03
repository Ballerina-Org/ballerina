namespace Ballerina.DSL.Codegen.Python.LanguageConstructs

open Ballerina.DSL.Expr.Types.Model
open Ballerina.State.WithError
open Ballerina.Errors
open Ballerina.Collections.Sum
open Ballerina.Collections.NonEmptyList
open Ballerina.DSL.Codegen.Python.LanguageConstructs.Union
open Ballerina.DSL.Codegen.Python.LanguageConstructs.Record
open Ballerina.DSL.Codegen.Python.LanguageConstructs.TypeAnnotations
open Ballerina.DSL.Codegen.Python.LanguageConstructs.Model
open Ballerina.DSL.Codegen.Python.LanguageConstructs.Unit
open Ballerina.Core.StringBuilder

module GeneratedTypes =

  let private alias (typeId: TypeId) (t: ExprType) =
    state {
      let! annotation = ExprType.GenerateTypeAnnotation t
      return StringBuilder.One $"{typeId.TypeName} = {annotation}\n\n"
    }

  type ExprType with
    static member Find (otherTypes: PythonGeneratedType list) (typeId: TypeId) : Sum<ExprType, Errors> =
      sum {
        return!
          otherTypes
          |> List.tryFind (fun t -> t.TypeName = typeId.TypeName)
          |> Sum.fromOption (fun () -> Errors.Singleton $"Error: type {typeId.TypeName} not found")
          |> Sum.map (fun t -> t.Type)
      }

    static member ResolveLookup (types: PythonGeneratedType list) (t: ExprType) : Sum<ExprType, Errors> =
      sum {
        match t with
        | ExprType.LookupType l -> return! ExprType.Find types l
        | _ -> return t
      }

  and PythonGeneratedType =
    { TypeName: string
      Type: ExprType }

    static member Generate (codegenConfig: PythonCodeGenConfig) (typesToGenerate: PythonGeneratedType list) =

      state.All(
        typesToGenerate
        |> Seq.map (fun t ->
          state {
            match t.Type with
            | ExprType.UnitType ->
              let unitCode, imports = PythonUnit.Generate { Name = t.TypeName }

              do!
                imports
                |> Set.union
                |> PythonCodeGenState.Updaters.UsedImports
                |> state.SetState

              unitCode
            | ExprType.UnionType cases ->
              let! caseValues =
                state.All(
                  cases
                  |> Map.values
                  |> Seq.map (fun case ->
                    state {
                      let! caseTypeAnnotation = ExprType.GenerateTypeAnnotation case.Fields


                      return
                        {| Name = case.CaseName
                           Type = caseTypeAnnotation |}
                    })
                  |> List.ofSeq
                )

              let! caseValues =
                caseValues
                |> NonEmptyList.TryOfList
                |> Sum.fromOption (fun () -> Errors.Singleton "Error: expected non-empty list of cases.")
                |> state.OfSum

              let unionCode, imports =
                { Name = t.TypeName
                  Cases = caseValues }
                |> PythonUnion.Generate

              do!
                imports
                |> Set.union
                |> PythonCodeGenState.Updaters.UsedImports
                |> state.SetState

              unionCode

            | ExprType.RecordType fields ->
              let! pythonRecordFields =
                fields
                |> Map.toList
                |> List.map (fun (fieldName, field) ->
                  state {
                    let! fieldType = field |> ExprType.GenerateTypeAnnotation

                    {| FieldName = fieldName
                       FieldType = fieldType |}
                  })
                |> state.All


              let recordCode, imports =
                { Name = t.TypeName
                  Fields = pythonRecordFields }
                |> PythonRecord.Generate

              do!
                imports
                |> Set.union
                |> PythonCodeGenState.Updaters.UsedImports
                |> state.SetState

              recordCode
            | ExprType.MapType(keyType, valueType) ->
              return! alias { TypeName = t.TypeName } (ExprType.MapType(keyType, valueType))
            | ExprType.TupleType elements -> return! alias { TypeName = t.TypeName } (ExprType.TupleType elements)
            | ExprType.OptionType element -> return! alias { TypeName = t.TypeName } (ExprType.OptionType element)
            | ExprType.ListType e -> return! alias { TypeName = t.TypeName } (ExprType.ListType e)
            | ExprType.SetType e -> return! alias { TypeName = t.TypeName } (ExprType.SetType e)
            | _ -> return! Errors.Singleton $"Error: type {t.TypeName} is not supported" |> state.Throw
          }
          |> state.WithErrorContext $"...when generating type {t.TypeName}")
        |> List.ofSeq
      )
      |> state.Map(Seq.ofList >> StringBuilder.Many)
