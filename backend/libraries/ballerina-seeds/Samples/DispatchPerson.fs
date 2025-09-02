module Ballerina.Seeds.Samples.DispatchPerson

open Ballerina.DSL.Next.Types.Model
open Ballerina.DSL.Next.Types.Patterns
open Ballerina.Data.Model

let entityMethods: Set<EntityMethod> = Set.empty
let lookupMethods: Set<LookupMethod> = Set.empty
let name = TypeSymbol.Create

let private (!) = Identifier.LocalScope

let context: Map<Identifier, TypeValue> =
  [ "Birthday", TypeValue.Primitive PrimitiveType.DateOnly
    "SubscribeToNewsletter", TypeValue.Primitive PrimitiveType.Bool
    "Emails", TypeValue.Tuple [ TypeValue.Primitive PrimitiveType.String ]
    "Fullname", TypeValue.Tuple [ TypeValue.Primitive PrimitiveType.String ]
    "Firstname", TypeValue.Primitive PrimitiveType.String
    "Surname", TypeValue.Primitive PrimitiveType.String
    "StreetNumberAndCity",
    TypeValue.Record(
      Map.ofList
        [ name "Street", TypeValue.Primitive PrimitiveType.String
          name "Number", TypeValue.Primitive PrimitiveType.Int32
          name "City", TypeValue.Lookup !"City" ]
    )
    "DeveloperJob",
    TypeValue.Record(
      Map.ofList
        [ name "Language", TypeValue.Primitive PrimitiveType.String
          name "Name", TypeValue.Primitive PrimitiveType.String
          name "Salary", TypeValue.Primitive PrimitiveType.Int32 ]
    )
    "DesignerJob",
    TypeValue.Record(
      Map.ofList
        [ name "DesignTool", TypeValue.Primitive PrimitiveType.String
          name "Certification", TypeValue.List(TypeValue.Primitive PrimitiveType.String) ]
    )
    "Job",
    TypeValue.Union(
      Map.ofList
        [ name "DeveloperJob", TypeValue.Lookup !"DeveloperJob"
          name "DesignerJob", TypeValue.Lookup !"DesignerJob" ]
    )
    "City",
    TypeValue.Record(
      Map.ofList
        [
          //name "Id",           TypeValue.Primitive PrimitiveType.Guid
          name "DisplayValue", TypeValue.Primitive PrimitiveType.String ]
    )
    "Colors", TypeValue.List(TypeValue.Primitive PrimitiveType.String)
    "Color", TypeValue.Primitive PrimitiveType.String
    "FavoriteColor",
    TypeValue.Sum
      [ TypeValue.Record(
          Map.ofList
            [ name "IsSome", TypeValue.Primitive PrimitiveType.Bool
              name "Value", TypeValue.Lookup !"Colors" ]
        ) ]
    "Person",
    TypeValue.Record(
      Map.ofList
        [ name "Firstname", TypeValue.Lookup !"Firstname"
          name "Surname", TypeValue.Lookup !"Surname"
          name "Birthday", TypeValue.Lookup !"Birthday"
          name "Colors", TypeValue.Lookup !"Colors"
          name "Color", TypeValue.Lookup !"Color"
          name "Emails", TypeValue.Lookup !"Emails"
          name "Fullname", TypeValue.Lookup !"Fullname"
          name "StreetNumberAndCity", TypeValue.Lookup !"StreetNumberAndCity"
          name "Job", TypeValue.Lookup !"Job"
          name "FavoriteColor", TypeValue.Lookup !"FavoriteColor" ]
    ) ]
  |> Seq.map (fun (k, v) -> !k, v)
  |> Map.ofSeq

let unlimited = 10

let private variousPeopleLookups: Map<string, LookupDescriptor> =
  Map.ofList
    [ "Follows",
      { Source = "PlainPeopleTable"
        Target = "FamousPeopleTable"
        Forward =
          { Arity = { Min = Some 0; Max = Some unlimited }
            Methods = lookupMethods
            Path = [] }
        Backward =
          Some(
            "Blocking",
            { Arity = { Min = Some 0; Max = Some unlimited }
              Methods = lookupMethods
              Path = [] }
          ) } ]

let private familyLookups: Map<string, LookupDescriptor> =
  Map.ofList
    [ "Raise",
      { Source = "Parents"
        Target = "Children"
        Forward =
          { Arity = { Min = Some 0; Max = Some 4 }
            Methods = lookupMethods
            Path = [] }
        Backward =
          Some(
            "Admire",
            { Arity = { Min = Some 2; Max = Some 2 }
              Methods = lookupMethods
              Path = [] }
          ) } ]

let private variousPeopleEntities: Map<string, EntityDescriptor<TypeValue>> =
  Map.ofList
    [ "PlainPeopleTable",
      { Type = context |> Map.find !"Person"
        Methods = entityMethods
        Updaters = []
        Predicates = Map.empty }
      "FamousPeopleTable",
      { Type = context |> Map.find !"Person"
        Methods = entityMethods
        Updaters = []
        Predicates = Map.empty } ]

let private familyEntities: Map<string, EntityDescriptor<TypeValue>> =
  Map.ofList
    [ "Parents",
      { Type = context |> Map.find !"Person"
        Methods = entityMethods
        Updaters = []
        Predicates = Map.empty }
      "Children",
      { Type = context |> Map.find !"Person"
        Methods = entityMethods
        Updaters = []
        Predicates = Map.empty } ]

let variousPeopleSchema: Schema<TypeValue> =
  { Entities = variousPeopleEntities
    Lookups = variousPeopleLookups }

let familySchema: Schema<TypeValue> =
  { Entities = familyEntities
    Lookups = familyLookups }

let exprSchema: Schema<TypeExpr> =
  { Entities =
      Map.ofList
        [ ("PlainPeopleTable",
           { Type = "Person" |> Identifier.LocalScope |> TypeExpr.Lookup
             Methods = Set.ofList [ Get; GetMany; Create; Delete; Update ]
             Updaters = []
             Predicates = Map.empty })
          ("FamousPeopleTable",
           { Type = "Person" |> Identifier.LocalScope |> TypeExpr.Lookup
             Methods = Set.ofList [ Get; GetMany; Create; Delete; Update ]
             Updaters = []
             Predicates = Map.empty }) ]
    Lookups =
      Map.ofList
        [ "Follows",
          { Source = "PlainPeopleTable"
            Target = "FamousPeopleTable"
            Forward =
              { Arity = { Min = Some 0; Max = Some unlimited }
                Methods = lookupMethods
                Path = [] }
            Backward =
              Some(
                "Blocks",
                { Arity = { Min = Some 0; Max = Some unlimited }
                  Methods = lookupMethods
                  Path = [] }
              ) } ] }
