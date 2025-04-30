module Ballerina.Core.Tests.LLM.TGI.Local

open NUnit.Framework

open Ballerina.Collections.Sum
open Ballerina.DSL.Expr.Types.Model
open Ballerina.DSL.Expr.Model

module LLM = Ballerina.AI.LLM.LLM
module Model = Ballerina.DSL.Expr.Types.Model
module JSONSchemaIntegration = Ballerina.AI.LLM.JSONSchemaIntegration
module TGI = Ballerina.AI.TGI.TGIIntegration

open System.Net.Http
open System

// Text from sample-invoice.pdf
let context =
  LLM.TextContext
    "CPB Software (Germany) GmbH - Im Bruch 3 - 63897 Miltenberg/Main
Musterkunde AG
Mr. John Doe
Musterstr. 23
12345 Musterstadt Name: Stefanie Müller
Phone: +49 9371 9786-0
Invoice WMACCESS Internet
VAT No. DE199378386
Invoice No
Customer No
Invoice Period
Date
123100401
12345
01.02.2024 - 29.02.2024
1. März 2024
Service Description Amount
-without VAT- quantity
Total Amount
Basic Fee wmView
130,00 € 1
130,00 €
Basis fee for additional user accounts 10,00 € 0
0,00 €
Basic Fee wmPos
50,00 € 0
0,00 €
Basic Fee wmGuide
1.000,00 € 0
0,00 €
Change of user accounts
10,00 € 0
0,00 €
Transaction Fee T1
0,58 € 14
8,12 €
Transaction Fee T2 0,70 € 0
0,00 €
Transaction Fee T3
1,50 € 162
243,00 €
Transaction Fee T4
0,50 € 0
0,00 €
Transaction Fee T5 0,80 € 0
0,00 €
Transaction Fee T6 1,80 € 0
0,00 €
Transaction Fee G1 0,30 € 0
0,00 €
Transaction Fee G2
0,30 € 0
0,00 €
Transaction Fee G3 0,40 € 0
0,00 €
Transaction Fee G4
0,40 € 0
0,00 €
Transaction Fee G5 0,30 € 0
0,00 €
Transaction Fee G6 0,30 € 0
0,00 €
Total 381,12 €
VAT 19 % 72,41 €
Gross Amount incl. VAT 453,53 €
Terms of Payment: Immediate payment without discount. Any bank charges must be paid by the invoice recipient.
Bank fees at our expense will be charged to the invoice recipient!
Please credit the amount invoiced to IBAN DE29 1234 5678 9012 3456 78 | BIC GENODE51MIC (SEPA Credit Transfer)
This invoice is generated automatically and will not be signed
Invoice Details
Period: 01.02.2024 to 29.02.2024
Unit: Musterkunde AG 12345
Request sections: T1: T2: T3: T4: T5: T6: G1: G2: G3: G4: G5: G6:
Amount in Euro: 0,58 0,70 1,50 0,50 0,80 1,80 0,30 0,30 0,40 0,40 0,30 0,30
user-account-1 10 0 99 0 0 0 0 0 0 0 0 0 154,30 €
user-account-2 4 0 63 0 0 0 0 0 0 0 0 0 96,82 €
Transaction Fee Seg T1: T2: T3: T4: T5: T6: G1: G2: G3: G4: G5: G6:
Queries in Total: 14 0 162 0 0 0 0 0 0 0 0 0
Total in Euro: 8,12 € 0,00 € 243,00 € 0,00 € 0,00 € 0,00 € 0,00 € 0,00 € 0,00 € 0,00 € 0,00 € 0,00 € 251,12 €
The explanation of the query fee categories (T1 to T6 and G1 to G6) can be found on our website:
https://www.wmaccess.com/abfragekategorien
Invoice Details for wmView Query Reference
Period: 01.02.2024 to 29.02.2024
Unit: Musterkunde AG 12345
wmview, wmProfile and User Profiles Query Segments:
Query Reference:
T1: T2: T3: T4: T5: T6:
*Not specified*
4 0 9 0 0 0
AZR/31/27439
0 0 12 0 0 0
CCL/3715
0 0 4 0 0 0
CRS/28432
5 0 36 0 0 0
Cs/52113
0 0 19 0 0 0
GS 32090 1 0 7 0 0 0
Kpi/22695 2 0 6 0 0 0
PG 7772
0 0 11 0 0 0
Rjn/11138
0 0 15 0 0 0
SF-M 596/99-08 0 0 5 0 0 0
Ttrb/17885 1 0 23 0 0 0
WPN:24791 1 0 4 0 0 0
Wwt/15658
0 0 11 0 0 0
Price for each Query in Euro:
0,58 0,70 1,50 0,50 0,80 1,80
15,82 €
18,00 €
6,00 €
56,90 €
28,50 €
11,08 €
10,16 €
16,50 €
22,50 €
7,50 €
35,08 €
6,58 €
16,50 €"


let private createEnumCase (caseName: string) : Model.CaseName * Model.UnionCase =
  { CaseName = caseName },
  { CaseName = caseName
    Fields = ExprType.UnitType }

let private getTokenAndEndpointFromEnvironment () =
  let token = Environment.GetEnvironmentVariable "HUGGINGFACE_TOKEN"
  Assert.That(token, Is.Not.Null, "HUGGINGFACE_TOKEN is not set")
  let endpoint = Environment.GetEnvironmentVariable "HUGGINGFACE_ENDPOINT"
  Assert.That(endpoint, Is.Not.Null, "HUGGINGFACE_ENDPOINT is not set")
  token, endpoint

[<Test>]
[<Explicit("Requires Huggingface token and endpoint")>]
let TestInvoiceDetection () =
  let token, endpoint = getTokenAndEndpointFromEnvironment ()
  // ARRANGE

  let taxBlockTypeId = { TypeName = "TaxBlock" }

  let taxBlockType =
    ExprType.RecordType(
      Map.ofList
        [ "rate", ExprType.PrimitiveType FloatType
          "amount", ExprType.PrimitiveType FloatType
          "base", ExprType.PrimitiveType FloatType ]
    )

  let paymentSectionTypeId = { TypeName = "PaymentSection" }

  let paymentSectionType =
    ExprType.RecordType(
      Map.ofList
        [ "total", ExprType.OptionType(ExprType.PrimitiveType FloatType)
          "currency",
          ExprType.OptionType(ExprType.UnionType(List.map createEnumCase [ "USD"; "EUR"; "CHF"; "GBP" ] |> Map.ofList))
          "taxBlocks", ExprType.ListType(ExprType.LookupType taxBlockTypeId) ]
    )

  let invoiceType =
    ExprType.RecordType(Map.ofList [ "paymentSection", ExprType.OptionType(ExprType.LookupType paymentSectionTypeId) ])

  use httpClient = new HttpClient(BaseAddress = Uri endpoint)
  httpClient.Timeout <- TimeSpan.FromMinutes 5.0
  httpClient.DefaultRequestHeaders.Authorization <- Headers.AuthenticationHeaderValue("Bearer", token)
  let client = TGI.newTGIHttpClient httpClient

  let correction = "I want the payment details in EUR"

  // ACT
  let result =
    LLM.LLM.Call
      { LLMIntegration = TGI.llmIntegration client
        StructuredOutputIntegration = JSONSchemaIntegration.JSONSchemaIntegration }
      { OutputType = invoiceType
        Refs = [ taxBlockTypeId, taxBlockType; paymentSectionTypeId, paymentSectionType ] }
      (LLM.TaskExplanation $"Extract the information requested in the schema from this invoice. {correction}")
      context
      None

  let expected =
    Value.Record(
      Map.ofList
        [ "paymentSection",
          Value.CaseCons(
            "some",
            Value.Record(
              Map.ofList
                [ "total", Value.CaseCons("some", Value.ConstFloat 453.53)
                  "currency", Value.CaseCons("some", Value.CaseCons("EUR", Value.Unit))
                  "taxBlocks",
                  Value.Tuple
                    [ Value.Record(
                        Map.ofList
                          [ "amount", Value.ConstFloat 72.41
                            "base", Value.ConstFloat 381.12
                            "rate", Value.ConstFloat 19.0 ]
                      ) ] ]
            )
          ) ]
    )

  // ASSERT
  match result with
  | Left r -> Assert.That(r, Is.EqualTo expected, sprintf "Expected %A\n, but got \n%A" expected r)
  | Right err -> Assert.Fail(err.ToString())
