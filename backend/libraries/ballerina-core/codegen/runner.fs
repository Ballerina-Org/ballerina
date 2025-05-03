namespace Ballerina.DSL.AI.Codegen

module Runner =
  open FSharp.Data
  open System
  open Ballerina.Collections.Sum
  open Ballerina.Errors
  open Ballerina.Core.StringBuilder
  open Ballerina.DSL.Expr.Types.Model
  open Ballerina.DSL.Codegen.Python.LanguageConstructs.Model
  open Ballerina.Codegen.Python.Generator.Main

  let private testTypes =
    let taxBlockTypeId = { TypeName = "TaxBlock" }

    let taxBlockType =
      ExprType.RecordType(
        Map.ofList
          [ "rate", ExprType.PrimitiveType FloatType
            "amount", ExprType.PrimitiveType FloatType
            "base", ExprType.PrimitiveType FloatType ]
      )


    let createEnumCase (caseName: string) : CaseName * UnionCase =
      { CaseName = caseName },
      { CaseName = caseName
        Fields = ExprType.UnitType }

    let currencyTypeId = { TypeName = "Currency" }

    let currencyType =
      ExprType.UnionType([ "USD"; "EUR"; "CHF"; "GBP" ] |> List.map createEnumCase |> Map.ofList)

    let taxedPaymentSectionTypeId = { TypeName = "TaxedPaymentSection" }

    let taxedPaymentSectionType =
      ExprType.RecordType(
        Map.ofList
          [ "total", ExprType.OptionType(ExprType.PrimitiveType FloatType)
            "taxBlocks", ExprType.ListType(ExprType.LookupType taxBlockTypeId) ]
      )

    let noTaxPaymentSectionTypeId = { TypeName = "NoTaxPaymentSection" }

    let noTaxPaymentSectionType =
      ExprType.RecordType(Map.ofList [ "subtotal", ExprType.PrimitiveType FloatType ])

    let paymentSectionTypeId = { TypeName = "PaymentSection" }

    let paymentSectionType =
      ExprType.UnionType(
        [ { CaseName = "Taxed"
            Fields = ExprType.LookupType paymentSectionTypeId }
          { CaseName = "NoTax"
            Fields = ExprType.LookupType noTaxPaymentSectionTypeId } ]
        |> List.map (fun c -> { CaseName = c.CaseName }, c)
        |> Map.ofList
      )

    let typeDefinition =
      { TypeName = "Invoice" },
      ExprType.RecordType(
        Map.ofList
          [ "paymentSection", ExprType.LookupType paymentSectionTypeId
            "currency", ExprType.OptionType(ExprType.LookupType currencyTypeId)
            "documentDate", ExprType.OptionType(ExprType.PrimitiveType DateOnlyType)
            "someTuple", ExprType.TupleType([ ExprType.PrimitiveType StringType; ExprType.PrimitiveType IntType ]) ]
      )

    let someMapId = { TypeName = "SomeMap" }

    let someMapType =
      ExprType.MapType(ExprType.PrimitiveType StringType, ExprType.PrimitiveType IntType)

    let someSetId = { TypeName = "SomeSet" }

    let someSetType = ExprType.SetType(ExprType.PrimitiveType StringType)

    typeDefinition,
    [ taxBlockTypeId, taxBlockType
      currencyTypeId, currencyType
      someMapId, someMapType
      someSetId, someSetType
      taxedPaymentSectionTypeId, taxedPaymentSectionType
      noTaxPaymentSectionTypeId, noTaxPaymentSectionType
      paymentSectionTypeId, paymentSectionType ]

  let hardcodedConfig: PythonCodeGenConfig =
    { Int =
        { GeneratedTypeName = "int"
          RequiredImport = None }
      Float =
        { GeneratedTypeName = "Decimal"
          RequiredImport = Some(Import "from decimal import Decimal") }
      Bool =
        { GeneratedTypeName = "bool"
          RequiredImport = None }
      String =
        { GeneratedTypeName = "str"
          RequiredImport = None }
      Date =
        { GeneratedTypeName = "date"
          RequiredImport = Some(Import "from datetime import date") }
      DateTime =
        { GeneratedTypeName = "datetime"
          RequiredImport = Some(Import "from datetime import datetime") }
      Guid =
        { GeneratedTypeName = "UUID"
          RequiredImport = Some(Import "from uuid import UUID") }
      Unit =
        { GeneratedTypeName = "None"
          RequiredImport = None }
      Option =
        { GeneratedTypeName = "Option"
          RequiredImport = Some(Import "from ballerina_core.primitives import Option") }
      Set =
        { GeneratedTypeName = "frozenset"
          RequiredImport = None }
      List =
        { GeneratedTypeName = "Sequence"
          RequiredImport = Some(Import "from collections.abc import Sequence") }
      Tuple =
        { GeneratedTypeName = "tuple"
          RequiredImport = None }
      Map =
        { GeneratedTypeName = "Mapping"
          RequiredImport = Some(Import "from collections.abc import Mapping") }
      Sum =
        { GeneratedTypeName = "Sum"
          RequiredImport = Some(Import "from ballerina_core.primitives import Sum") } }

  let runSingle (outputPath: string) =
    let typeDefinition, otherTypes = testTypes

    match Generator.ToPython hardcodedConfig typeDefinition otherTypes with
    | Left generatedCode ->
      let outputPath = $"{outputPath}_gen.py"

      try
        do
          System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName outputPath)
          |> ignore

        let generatedCode = generatedCode |> StringBuilder.ToString

        Left
          {| OutputPath = outputPath
             GeneratedCode = generatedCode |}

      with err ->
        Right(Errors.Singleton $"Error when generating output file {err.Message}")
    | Right err -> Right err


  let run (outputPath: string) =
    match runSingle (outputPath: string) with
    | Left res ->
      do System.IO.File.WriteAllText(res.OutputPath, res.GeneratedCode)
      do Console.ForegroundColor <- ConsoleColor.Green
      do Console.WriteLine $$"""Code is generated at {{outputPath}}.  """
      do Console.ResetColor()
    | Right err -> do Errors.Print "no input path" err
