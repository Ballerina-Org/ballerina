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

  let private updateImports =
    Set.union >> PythonCodeGenState.Updaters.UsedImports >> state.SetState

  type ExprType with
    static member Find (otherTypes: PythonGeneratedType list) (typeId: TypeId) : Sum<ExprType, Errors> =
      sum {
        return!
          otherTypes
          |> List.tryFind (fun t -> t.TypeId = typeId)
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
    { TypeId: TypeId
      Type: ExprType }

    static member private GenerateUnionType (typeId: TypeId) (cases: Map<CaseName, UnionCase>) =
      state {
        let! caseValues =
          cases
          |> Map.values
          |> Seq.map (fun case ->
            state {
              let! caseTypeAnnotation = ExprType.GenerateTypeAnnotation case.Fields

              {| Name = case.CaseName
                 Type = caseTypeAnnotation |}
            })
          |> List.ofSeq
          |> state.All

        let! nonEmptyCaseValues =
          caseValues
          |> NonEmptyList.TryOfList
          |> Sum.fromOption (fun () -> Errors.Singleton "Error: expected non-empty list of cases.")
          |> state.OfSum

        let unionCode, imports =
          { Name = typeId.TypeName
            Cases = nonEmptyCaseValues }
          |> PythonUnion.Generate

        do! updateImports imports
        return unionCode
      }

    static member private GenerateRecordType (typeId: TypeId) (fields: Map<string, ExprType>) =
      state {
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
          { Name = typeId.TypeName
            Fields = pythonRecordFields }
          |> PythonRecord.Generate

        do! updateImports imports
        return recordCode
      }

    static member Generate(typesToGenerate: PythonGeneratedType list) =
      let generateType (t: PythonGeneratedType) =
        state {
          match t.Type with
          | ExprType.UnitType ->
            let unitCode, imports = PythonUnit.Generate { Name = t.TypeId.TypeName }
            do! updateImports imports
            return unitCode
          | ExprType.UnionType cases -> return! PythonGeneratedType.GenerateUnionType t.TypeId cases
          | ExprType.RecordType fields -> return! PythonGeneratedType.GenerateRecordType t.TypeId fields
          | ExprType.MapType _ -> return! alias t.TypeId t.Type
          | ExprType.TupleType _ -> return! alias t.TypeId t.Type
          | ExprType.OptionType _ -> return! alias t.TypeId t.Type
          | ExprType.PrimitiveType _ -> return! alias t.TypeId t.Type
          | ExprType.LookupType _ -> return! alias t.TypeId t.Type
          | ExprType.ListType _ -> return! alias t.TypeId t.Type
          | ExprType.SumType _ -> return! alias t.TypeId t.Type
          | ExprType.SetType _ -> return! alias t.TypeId t.Type
          | ExprType.OneType _ -> return! Errors.Singleton $"Error: type {t.TypeId} is not supported" |> state.Throw
          | ExprType.SchemaLookupType _ ->
            return! Errors.Singleton $"Error: type {t.TypeId} is not supported" |> state.Throw
          | ExprType.TableType _ -> return! Errors.Singleton $"Error: type {t.TypeId} is not supported" |> state.Throw
          | ExprType.VarType _ -> return! Errors.Singleton $"Error: type {t.TypeId} is not supported" |> state.Throw
          | ExprType.CustomType _ -> return! Errors.Singleton $"Error: type {t.TypeId} is not supported" |> state.Throw
          | ExprType.ManyType _ -> return! Errors.Singleton $"Error: type {t.TypeId} is not supported" |> state.Throw
        }
        |> state.WithErrorContext $"...when generating type {t.TypeId}"

      typesToGenerate
      |> List.map generateType
      |> state.All
      |> state.Map(Seq.ofList >> StringBuilder.Many)
