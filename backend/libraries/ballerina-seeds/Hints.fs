namespace Ballerina.Seeds

open System
open Bogus
open Bogus.Extensions.Poland

module Hints =
  let programmingLanguages =
    [ "C#"
      "F#"
      "Java"
      "JavaScript"
      "Python"
      "Ruby"
      "Go"
      "Rust"
      "Kotlin"
      "Swift" ]

  let designTools =
    [ "Adobe Photoshop"
      "Adobe Illustrator"
      "Figma"
      "Sketch"
      "InVision"
      "Canva" ]

  let colors =
    [ "Red"
      "Green"
      "Blue"
      "Yellow"
      "Purple"
      "Orange"
      "Pink"
      "Brown"
      "Black"
      "White" ]

  let removeEnding (suffix: string) (input: string) =
    if input.EndsWith(suffix) then
      input.Substring(0, input.Length - suffix.Length)
    else
      input

  let ``for`` (faker: Faker) (person: Bogus.Person) (source: string) =
    match source.ToLower() |> removeEnding "ref" |> removeEnding "enum" with
    | "city" -> faker.Address.City()
    | "department" -> faker.Commerce.Department()
    | "product"
    | "productname" -> faker.Commerce.ProductName()
    | "abbreviation" -> faker.Hacker.Abbreviation()
    | "street" -> faker.Address.StreetAddress()
    | "streetnumberandcity" -> faker.Address.StreetAddress() + " " + faker.Address.City()
    | "fullname" -> person.FullName
    | "language" -> faker.PickRandom programmingLanguages
    | "designtool" -> faker.PickRandom designTools
    | "colors"
    | "color" -> faker.Commerce.Color()
    | "firstname"
    | "name" -> person.FirstName
    | "emails"
    | "email" -> person.Email
    | "surname" -> person.LastName
    | "word" -> faker.Lorem.Word()
    | "guid"
    | "salary" -> faker.Finance.Amount(30000.0M, 150000.0M) |> string
    | "id" -> faker.Random.Guid().ToString()
    | "sentence" -> faker.Lorem.Sentence()
    | "paragraph" -> faker.Lorem.Paragraph()
    | "text" -> faker.Lorem.Text()
    | "title" -> faker.Lorem.Sentence(Nullable 3, Nullable 5)
    | "jobtitle" -> faker.Name.JobTitle()
    | "company"
    | "companyname" -> faker.Company.CompanyName()
    | "companysuffix" -> faker.Company.CompanySuffix()
    | "jobdescriptor" -> faker.Name.JobDescriptor()
    | "jobarea" -> faker.Name.JobArea()
    | "job" -> faker.Name.JobType()
    | "certifications" -> faker.Name.JobArea() + " " + faker.Name.JobType()

    | "userid" -> person.Pesel.ToString()
    | "organizationid" -> faker.Company.Nip().ToString()
    | "projectmanagername" -> person.FullName
    | "jiragoliveticket" -> faker.Internet.Url()
    | "timetrackingproject" -> $"{faker.Commerce.Department()}-{faker.Random.AlphaNumeric(4).ToUpper()}"
    | _ -> faker.Lorem.Sentence(Nullable 3, Nullable 5)
