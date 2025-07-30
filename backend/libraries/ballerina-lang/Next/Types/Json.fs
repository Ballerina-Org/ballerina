namespace Ballerina.DSL.Next.Types

module Json =
  open Ballerina.StdLib.Json.Sum
  open Ballerina.StdLib.Json.Reader
  open Ballerina.Collections.Sum
  open Ballerina.StdLib.Json.Patterns
  open Ballerina.Reader.WithError
  open Ballerina.Errors
  open System
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Types.Patterns
  open FSharp.Data

  let inline private (>>=) f g = fun x -> sum.Bind(f x, g) // Using bind


  type TypeIdentifier with
    static member FromJson: JsonValue -> Sum<TypeIdentifier, Errors> =
      sum.AssertKindAndContinueWithField "type-id" "name" (fun nameJson ->
        sum {
          let! name = nameJson |> JsonValue.AsString
          return name |> TypeIdentifier.Create
        })

  type Kind with
    static member private FromJsonSymbol: JsonValue -> Sum<Kind, Errors> =
      sum.AssertKindAndContinue "symbol" (fun _ -> sum { return Kind.Symbol })

    static member private FromJsonStar: JsonValue -> Sum<Kind, Errors> =
      sum.AssertKindAndContinue "star" (fun _ -> sum { return Kind.Star })

    static member private FromJsonArrow: JsonValue -> Sum<Kind, Errors> =
      sum.AssertKindAndContinueWithField "arrow" "arrow" (fun arrowFields ->
        sum {
          let! arrowFields = arrowFields |> JsonValue.AsRecordMap
          let! param = arrowFields |> (Map.tryFindWithError "param" "arrow" "param" >>= Kind.FromJson)

          let! returnType =
            arrowFields
            |> (Map.tryFindWithError "returnType" "arrow" "returnType" >>= Kind.FromJson)

          return Kind.Arrow(param, returnType)
        })

    static member FromJson(json: JsonValue) : Sum<Kind, Errors> =
      sum.Any(Kind.FromJsonStar(json), [ Kind.FromJsonSymbol(json); Kind.FromJsonArrow(json) ])
      |> sum.MapError(Errors.HighestPriority)

  type TypeSymbol with
    static member FromJson(json: JsonValue) : Sum<TypeSymbol, Errors> =
      sum {
        let! fields = json |> JsonValue.AsRecordMap

        let! name =
          fields
          |> (Map.tryFindWithError "name" "TypeSymbol" "name" >>= JsonValue.AsString)

        let! guid =
          fields
          |> (Map.tryFindWithError "guid" "TypeSymbol" "guid" >>= JsonValue.AsString)

        match Guid.TryParse(guid) with
        | true, parsedGuid -> return { Name = name; Guid = parsedGuid }
        | false, _ ->
          return!
            $"Error: Invalid GUID format '{guid}' in 'TypeSymbol'."
            |> Errors.Singleton
            |> sum.Throw
      }

  type TypeParameter with
    static member FromJson(json: JsonValue) : Sum<TypeParameter, Errors> =
      sum {
        let! fields = json |> JsonValue.AsRecordMap

        let! name =
          fields
          |> (Map.tryFindWithError "name" "TypeParameter" "name" >>= JsonValue.AsString)

        let! kind = fields |> (Map.tryFindWithError "kind" "TypeParameter" "kind" >>= Kind.FromJson)

        return { Name = name; Kind = kind }
      }

  type TypeVar with
    static member FromJson(json: JsonValue) : Sum<TypeVar, Errors> =
      sum {
        let! fields = json |> JsonValue.AsRecordMap

        let! name = fields |> (Map.tryFindWithError "name" "TypeVar" "name" >>= JsonValue.AsString)

        let! guid = fields |> (Map.tryFindWithError "guid" "TypeVar" "guid" >>= JsonValue.AsString)

        match Guid.TryParse(guid) with
        | true, parsedGuid -> return { Name = name; Guid = parsedGuid }
        | false, _ ->
          return!
            $"Error: Invalid GUID format '{guid}' in 'TypeVar'."
            |> Errors.Singleton
            |> sum.Throw
      }

  type PrimitiveType with

    static member private FromJsonUnit: JsonValue -> Sum<PrimitiveType, Errors> =
      sum.AssertKindAndContinue "unit" (fun _ -> sum { return PrimitiveType.Unit })

    static member private FromJsonGuid: JsonValue -> Sum<PrimitiveType, Errors> =
      sum.AssertKindAndContinue "guid" (fun _ -> sum { return PrimitiveType.Guid })

    static member private FromJsonInt: JsonValue -> Sum<PrimitiveType, Errors> =
      sum.AssertKindAndContinue "int" (fun _ -> sum { return PrimitiveType.Int })

    static member private FromJsonDecimal: JsonValue -> Sum<PrimitiveType, Errors> =
      sum.AssertKindAndContinue "decimal" (fun _ -> sum { return PrimitiveType.Decimal })

    static member private FromJsonString: JsonValue -> Sum<PrimitiveType, Errors> =
      sum.AssertKindAndContinue "string" (fun _ -> sum { return PrimitiveType.String })

    static member private FromJsonBool: JsonValue -> Sum<PrimitiveType, Errors> =
      sum.AssertKindAndContinue "bool" (fun _ -> sum { return PrimitiveType.Bool })

    static member private FromJsonDateTime: JsonValue -> Sum<PrimitiveType, Errors> =
      sum.AssertKindAndContinue "datetime" (fun _ -> sum { return PrimitiveType.DateTime })

    static member private FromJsonDateOnly: JsonValue -> Sum<PrimitiveType, Errors> =
      sum.AssertKindAndContinue "dateonly" (fun _ -> sum { return PrimitiveType.DateOnly })

    static member FromJson(json: JsonValue) : Sum<PrimitiveType, Errors> =
      sum.Any(
        PrimitiveType.FromJsonUnit(json),
        [ PrimitiveType.FromJsonGuid(json)
          PrimitiveType.FromJsonInt(json)
          PrimitiveType.FromJsonDecimal(json)
          PrimitiveType.FromJsonString(json)
          PrimitiveType.FromJsonBool(json)
          PrimitiveType.FromJsonDateTime(json)
          PrimitiveType.FromJsonDateOnly(json) ]
      )
      |> sum.MapError(Errors.HighestPriority)

  type TypeValue with

    static member private FromJsonVar: JsonValue -> Sum<TypeValue, Errors> =
      sum.AssertKindAndContinueWithField "var" "var" (TypeVar.FromJson >> sum.Map TypeValue.Var)

    static member FromJsonLookup: JsonValue -> Sum<TypeValue, Errors> =
      sum.AssertKindAndContinueWithField "lookup" "lookup" (fun lookupFields ->
        sum {
          let! name = lookupFields |> JsonValue.AsString
          return TypeValue.Lookup { Name = name }
        })

    static member FromJsonLambda: JsonValue -> Sum<TypeValue, Errors> =
      sum.AssertKindAndContinueWithField "lambda" "lambda" (fun lambdaFields ->
        sum {
          let! lambdaFields = lambdaFields |> JsonValue.AsRecordMap

          let! param =
            lambdaFields
            |> (Map.tryFindWithError "param" "lambda" "param" >>= TypeParameter.FromJson)

          let! body =
            lambdaFields
            |> (Map.tryFindWithError "body" "lambda" "body" >>= TypeExpr.FromJson)

          return TypeValue.Lambda(param, body)
        })

    static member FromJsonArrow: JsonValue -> Sum<TypeValue, Errors> =
      sum.AssertKindAndContinueWithField "arrow" "arrow" (fun arrowFields ->
        sum {
          let! arrowFields = arrowFields |> JsonValue.AsRecordMap

          let! param =
            arrowFields
            |> (Map.tryFindWithError "param" "arrow" "param" >>= TypeValue.FromJson)

          let! returnType =
            arrowFields
            |> (Map.tryFindWithError "returnType" "arrow" "returnType" >>= TypeValue.FromJson)

          return TypeValue.Arrow(param, returnType)
        })

    static member FromJsonRecord: JsonValue -> Sum<TypeValue, Errors> =
      sum.AssertKindAndContinueWithField "record" "record" (fun recordFields ->
        sum {
          let! fields = recordFields |> JsonValue.AsArray

          let! fieldTypes =
            fields
            |> Array.map (fun field ->
              sum {
                let! (fieldKey, fieldValue) = field |> JsonValue.AsPair
                let! fieldType = TypeValue.FromJson(fieldValue)
                let! fieldKey = fieldKey |> TypeSymbol.FromJson
                return (fieldKey, fieldType)
              })
            |> sum.All
            |> sum.Map Map.ofSeq

          return TypeValue.Record(fieldTypes)
        })

    static member FromJsonTuple: JsonValue -> Sum<TypeValue, Errors> =
      sum.AssertKindAndContinueWithField "tuple" "tuple" (fun tupleFields ->
        sum {
          let! elements = tupleFields |> JsonValue.AsArray
          let! elementTypes = elements |> Array.map (fun element -> element |> TypeValue.FromJson) |> sum.All
          return TypeValue.Tuple(elementTypes)
        })

    static member FromJsonUnion: JsonValue -> Sum<TypeValue, Errors> =
      sum.AssertKindAndContinueWithField "union" "union" (fun unionFields ->
        sum {
          let! cases = unionFields |> JsonValue.AsArray

          let! caseTypes =
            cases
            |> Array.map (fun case ->
              sum {
                let! (caseKey, caseValue) = case |> JsonValue.AsPair
                let! caseType = TypeValue.FromJson(caseValue)
                let! caseKey = caseKey |> TypeSymbol.FromJson
                return (caseKey, caseType)
              })
            |> sum.All
            |> sum.Map Map.ofSeq

          return TypeValue.Union(caseTypes)
        })

    static member FromJsonSum: JsonValue -> Sum<TypeValue, Errors> =
      sum.AssertKindAndContinueWithField "sum" "sum" (fun sumFields ->
        sum {
          let! cases = sumFields |> JsonValue.AsArray
          let! caseTypes = cases |> Array.map (fun case -> case |> TypeValue.FromJson) |> sum.All
          return TypeValue.Sum(caseTypes)
        })

    static member FromJsonList: JsonValue -> Sum<TypeValue, Errors> =
      sum.AssertKindAndContinueWithField "list" "list" (fun elementType ->
        sum {
          let! elementType = elementType |> TypeValue.FromJson
          return TypeValue.List elementType
        })

    static member FromJsonSet: JsonValue -> Sum<TypeValue, Errors> =
      sum.AssertKindAndContinueWithField "set" "set" (fun elementType ->
        sum {
          let! elementType = elementType |> TypeValue.FromJson
          return TypeValue.Set elementType
        })

    static member FromJsonMap: JsonValue -> Sum<TypeValue, Errors> =
      sum.AssertKindAndContinueWithField "map" "map" (fun mapFields ->
        sum {
          let! (key, value) = mapFields |> JsonValue.AsPair
          let! keyType = key |> TypeValue.FromJson
          let! valueType = value |> TypeValue.FromJson
          return TypeValue.Map(keyType, valueType)
        })

    static member FromJsonPrimitive =
      PrimitiveType.FromJson >>= (TypeValue.Primitive >> sum.Return)

    static member FromJson(json: JsonValue) : Sum<TypeValue, Errors> =
      sum.Any(
        TypeValue.FromJsonPrimitive json,
        [ TypeValue.FromJsonVar(json)
          TypeValue.FromJsonLookup(json)
          TypeValue.FromJsonArrow(json)
          TypeValue.FromJsonLambda(json)
          TypeValue.FromJsonRecord(json)
          TypeValue.FromJsonTuple(json)
          TypeValue.FromJsonUnion(json)
          TypeValue.FromJsonSum(json)
          TypeValue.FromJsonList(json)
          TypeValue.FromJsonSet(json)
          TypeValue.FromJsonMap(json) ]
      )
      |> sum.MapError(Errors.HighestPriority)


  and TypeExpr with
    static member private FromJsonApply: JsonValue -> Sum<TypeExpr, Errors> =
      sum.AssertKindAndContinueWithField "apply" "apply" (fun applyFields ->
        sum {
          let! (functionField, argumentField) = applyFields |> JsonValue.AsPair

          let! functionType = functionField |> TypeExpr.FromJson
          let! argumentType = argumentField |> TypeExpr.FromJson

          return TypeExpr.Apply(functionType, argumentType)
        })

    static member FromJsonPrimitive =
      PrimitiveType.FromJson >>= (TypeExpr.Primitive >> sum.Return)

    static member FromJsonLambda: JsonValue -> Sum<TypeExpr, Errors> =
      sum.AssertKindAndContinueWithField "lambda" "lambda" (fun lambdaFields ->
        sum {
          let! (param, body) = lambdaFields |> JsonValue.AsPair
          let! param = param |> TypeParameter.FromJson
          let! body = body |> TypeExpr.FromJson

          return TypeExpr.Lambda(param, body)
        })

    static member FromJsonArrow =
      sum.AssertKindAndContinueWithField "arrow" "arrow" (fun arrowFields ->
        sum {
          let! (param, returnType) = arrowFields |> JsonValue.AsPair
          let! param = param |> TypeExpr.FromJson
          let! returnType = returnType |> TypeExpr.FromJson

          return TypeExpr.Arrow(param, returnType)
        })

    static member FromJsonRecord: JsonValue -> Sum<TypeExpr, Errors> =
      sum.AssertKindAndContinueWithField "record" "record" (fun recordFields ->
        sum {
          let! fields = recordFields |> JsonValue.AsArray

          let! fieldTypes =
            fields
            |> Array.map (fun field ->
              sum {
                let! (fieldKey, fieldValue) = field |> JsonValue.AsPair
                let! fieldType = TypeExpr.FromJson(fieldValue)
                let! fieldKey = TypeExpr.FromJson fieldKey
                return (fieldKey, fieldType)
              })
            |> sum.All

          return TypeExpr.Record(fieldTypes)
        })

    static member FromJsonTuple: JsonValue -> Sum<TypeExpr, Errors> =
      sum.AssertKindAndContinueWithField "tuple" "tuple" (fun tupleFields ->
        sum {
          let! elements = tupleFields |> JsonValue.AsArray
          let! elementTypes = elements |> Array.map (fun element -> element |> TypeExpr.FromJson) |> sum.All
          return TypeExpr.Tuple(elementTypes)
        })

    static member FromJsonLookup: JsonValue -> Sum<TypeExpr, Errors> =
      sum.AssertKindAndContinueWithField "lookup" "lookup" (JsonValue.AsString >>= (TypeExpr.Lookup >> sum.Return))

    static member FromJsonUnion: JsonValue -> Sum<TypeExpr, Errors> =
      sum.AssertKindAndContinueWithField "union" "union" (fun unionFields ->
        sum {
          let! cases = unionFields |> JsonValue.AsArray

          let! caseTypes =
            cases
            |> Array.map (fun case ->
              sum {
                let! (caseKey, caseValue) = case |> JsonValue.AsPair
                let! caseType = TypeExpr.FromJson(caseValue)
                let! caseKey = TypeExpr.FromJson caseKey
                return (caseKey, caseType)
              })
            |> sum.All

          return TypeExpr.Union(caseTypes)
        })

    static member FromJsonSum: JsonValue -> Sum<TypeExpr, Errors> =
      sum.AssertKindAndContinueWithField "sum" "sum" (fun sumFields ->
        sum {
          let! sumFields = sumFields |> JsonValue.AsArray
          let! caseTypes = sumFields |> Array.map (fun case -> case |> TypeExpr.FromJson) |> sum.All
          return TypeExpr.Sum(caseTypes)
        })

    static member FromJsonList: JsonValue -> Sum<TypeExpr, Errors> =
      sum.AssertKindAndContinueWithField "list" "list" (TypeExpr.FromJson >>= (TypeExpr.List >> sum.Return))

    static member FromJsonSet: JsonValue -> Sum<TypeExpr, Errors> =
      sum.AssertKindAndContinueWithField "set" "set" (TypeExpr.FromJson >>= (TypeExpr.Set >> sum.Return))

    static member FromJsonMap: JsonValue -> Sum<TypeExpr, Errors> =
      sum.AssertKindAndContinueWithField "map" "map" (fun mapFields ->
        sum {
          let! (key, value) = mapFields |> JsonValue.AsPair
          let! keyType = key |> TypeExpr.FromJson
          let! valueType = value |> TypeExpr.FromJson
          return TypeExpr.Map(keyType, valueType)
        })

    static member FromJsonKeyOf: JsonValue -> Sum<TypeExpr, Errors> =
      sum.AssertKindAndContinueWithField "keyOf" "keyOf" (TypeExpr.FromJson >>= (TypeExpr.KeyOf >> sum.Return))

    static member FromJsonFlatten: JsonValue -> Sum<TypeExpr, Errors> =
      sum.AssertKindAndContinueWithField "flatten" "flatten" (fun flattenFields ->
        sum {
          let! (type1, type2) = flattenFields |> JsonValue.AsPair
          let! type1 = type1 |> TypeExpr.FromJson
          let! type2 = type2 |> TypeExpr.FromJson
          return TypeExpr.Flatten(type1, type2)
        })

    static member FromJsonExclude: JsonValue -> Sum<TypeExpr, Errors> =
      sum.AssertKindAndContinueWithField "exclude" "exclude" (fun excludeFields ->
        sum {
          let! (type1, type2) = excludeFields |> JsonValue.AsPair
          let! type1 = type1 |> TypeExpr.FromJson
          let! type2 = type2 |> TypeExpr.FromJson
          return TypeExpr.Exclude(type1, type2)
        })

    static member FromJsonRotate: JsonValue -> Sum<TypeExpr, Errors> =
      sum.AssertKindAndContinueWithField "rotate" "rotate" (TypeExpr.FromJson >>= (TypeExpr.Rotate >> sum.Return))

    static member FromJson(json: JsonValue) : Sum<TypeExpr, Errors> =
      sum.Any(
        TypeExpr.FromJsonPrimitive(json),
        [ TypeExpr.FromJsonApply(json)
          TypeExpr.FromJsonRotate(json)
          TypeExpr.FromJsonLambda(json)
          TypeExpr.FromJsonArrow(json)
          TypeExpr.FromJsonRecord(json)
          TypeExpr.FromJsonTuple(json)
          TypeExpr.FromJsonLookup(json)
          TypeExpr.FromJsonUnion(json)
          TypeExpr.FromJsonSum(json)
          TypeExpr.FromJsonList(json)
          TypeExpr.FromJsonSet(json)
          TypeExpr.FromJsonMap(json)
          TypeExpr.FromJsonKeyOf(json)
          TypeExpr.FromJsonFlatten(json)
          TypeExpr.FromJsonExclude(json) ]
      )
      |> sum.MapError(Errors.HighestPriority)
