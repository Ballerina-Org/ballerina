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

  let private updateImports imports =
    imports
    |> Set.union
    |> PythonCodeGenState.Updaters.UsedImports
    |> state.SetState

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

    static member private GenerateUnionType (typeName: string) (cases: Map<CaseName, UnionCase>) =
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
          { Name = typeName
            Cases = nonEmptyCaseValues }
          |> PythonUnion.Generate

        do! updateImports imports
        return unionCode
      }

    static member private GenerateRecordType (typeName: string) (fields: Map<string, ExprType>) =
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
          { Name = typeName
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
            let unitCode, imports = PythonUnit.Generate { Name = t.TypeName }
            do! updateImports imports
            return unitCode
          | ExprType.UnionType cases -> return! PythonGeneratedType.GenerateUnionType t.TypeName cases
          | ExprType.RecordType fields -> return! PythonGeneratedType.GenerateRecordType t.TypeName fields
          | ExprType.MapType _
          | ExprType.TupleType _
          | ExprType.OptionType _
          | ExprType.PrimitiveType _
          | ExprType.LookupType _
          | ExprType.ListType _
          | ExprType.SumType _
          | ExprType.SetType _ -> return! alias { TypeName = t.TypeName } t.Type
          | ExprType.OneType _
          | ExprType.SchemaLookupType _
          | ExprType.TableType _
          | ExprType.VarType _
          | ExprType.CustomType _
          | ExprType.ManyType _ -> return! Errors.Singleton $"Error: type {t.TypeName} is not supported" |> state.Throw
        }
        |> state.WithErrorContext $"...when generating type {t.TypeName}"

      typesToGenerate
      |> List.map generateType
      |> state.All
      |> state.Map(Seq.ofList >> StringBuilder.Many)
