namespace Ballerina.DSL.AI.Codegen

module Runner =
  open FSharp.Data
  open System
  open Ballerina.Collections.Sum
  open Ballerina.Errors
  open Ballerina.Core.StringBuilder
  open Ballerina.DSL.Expr.Types.Model
  open Ballerina.Codegen.Python.Generator.Main

  let taxBlockTypeId = { TypeName = "TaxBlock" }

  let taxBlockType =
    ExprType.RecordType(
      Map.ofList
        [ "rate", ExprType.PrimitiveType FloatType
          "amount", ExprType.PrimitiveType FloatType
          "base", ExprType.PrimitiveType FloatType ]
    )


  let private createEnumCase (caseName: string) : CaseName * UnionCase =
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

  let otherTypes =
    [ taxBlockTypeId, taxBlockType
      currencyTypeId, currencyType
      someMapId, someMapType
      someSetId, someSetType
      taxedPaymentSectionTypeId, taxedPaymentSectionType
      noTaxPaymentSectionTypeId, noTaxPaymentSectionType
      paymentSectionTypeId, paymentSectionType ]

  let runSingle (outputPath: string) =
    match Generator.ToPython typeDefinition otherTypes with
    | Left generatedCode ->
      let outputPath = $"{outputPath}.gen.py"

      try
        do
          System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName outputPath)
          |> ignore

        let generatedCode = generatedCode |> StringBuilder.ToString

        Left
          {| OutputPath = outputPath
             GeneratedCode = generatedCode |}

      with err ->
        Right(Errors.Singleton $"Error when generating output file {{err.Message.ReasonablyClamped}}")
    | Right err -> Right err


  let run (outputPath: string) =
    match runSingle (outputPath: string) with
    | Left res ->
      do System.IO.File.WriteAllText(res.OutputPath, res.GeneratedCode)
      do Console.ForegroundColor <- ConsoleColor.Green
      do Console.WriteLine $$"""Code is generated at {{outputPath}}.  """
      do Console.ResetColor()
    | Right err -> do Errors.Print "no input path" err
