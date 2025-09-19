namespace Ballerina.DSL.Parser

module ExprType =
  open Patterns
  open Patterns.TypeContext
  open Ballerina.DSL.Expr.Model
  open Ballerina.DSL.Expr.Types.Model
  open Ballerina.DSL.Expr.Types.Patterns
  open System
  open Ballerina.Errors
  open Ballerina.StdLib.Json.Patterns
  open Ballerina.StdLib.String
  open FSharp.Data
  open Ballerina.Collections.NonEmptyList
  open Ballerina.Collections.Sum
  open Ballerina.StdLib.Object
  open Ballerina.State.WithError

  type ExprType with
    static member ParseUnionCase(json: JsonValue) : Sum<UnionCase, Errors> =
      let (!) = ExprType.Parse

      sum {
        let! args = json |> JsonValue.AsRecord

        let! caseJson, fieldsJson = sum.All2 (args |> sum.TryFindField "caseName") (args |> sum.TryFindField "fields")

        let! caseName = caseJson |> JsonValue.AsString

        let! fieldsType =
          sum.Any2
            (sum {
              let! fieldsJson = fieldsJson |> JsonValue.AsRecord

              let! fields =
                fieldsJson
                |> Seq.map (fun (fieldName, fieldType) ->
                  sum {
                    let! fieldType = !fieldType
                    return fieldName, fieldType
                  }
                  |> sum.MapError(Errors.Map(String.appendNewline $"\n...when parsing field {fieldName}")))
                |> Seq.toList
                |> sum.All

              let fields = fields |> Map.ofList

              if fields |> Map.isEmpty then
                ExprType.UnitType
              else
                ExprType.RecordType fields
            })
            (fieldsJson |> (!))

        return
          { CaseName = caseName
            Fields = fieldsType }
      }

    static member Parse(json: JsonValue) : Sum<ExprType, Errors> =
      let (!) = ExprType.Parse

      sum {
        return!
          sum.Any(
            NonEmptyList.OfList(
              sum {
                do!
                  json
                  |> JsonValue.AsEnum(Set.singleton "unit")

                  |> sum.Map ignore

                return ExprType.UnitType
              },
              [ sum {
                  do!
                    json
                    |> JsonValue.AsEnum(Set.singleton "guid")

                    |> sum.Map ignore

                  return ExprType.PrimitiveType PrimitiveType.GuidType
                }
                sum {
                  do!
                    json
                    |> JsonValue.AsEnum(Set.singleton "string")

                    |> sum.Map ignore

                  return ExprType.PrimitiveType PrimitiveType.StringType
                }
                sum {
                  do!
                    json
                    |> JsonValue.AsEnum(Set.singleton "entityIdString")

                    |> sum.Map ignore

                  return ExprType.PrimitiveType PrimitiveType.EntityIdStringType
                }
                sum {
                  do!
                    json
                    |> JsonValue.AsEnum(Set.singleton "entityIdUUID")

                    |> sum.Map ignore

                  return ExprType.PrimitiveType PrimitiveType.EntityIdUUIDType
                }
                sum {
                  do!
                    json
                    |> JsonValue.AsEnum(Set.singleton "calculatedDisplayValue")

                    |> sum.Map ignore

                  return ExprType.PrimitiveType PrimitiveType.CalculatedDisplayValueType
                }
                sum {
                  do!
                    json
                    |> JsonValue.AsEnum(Set.singleton "number")

                    |> sum.Map ignore

                  return ExprType.PrimitiveType PrimitiveType.IntType
                }
                sum {
                  do! json |> JsonValue.AsEnum(Set.singleton "int") |> sum.Map ignore
                  return ExprType.PrimitiveType PrimitiveType.IntType
                }
                sum {
                  do!
                    json
                    |> JsonValue.AsEnum(Set.singleton "float")

                    |> sum.Map ignore

                  return ExprType.PrimitiveType PrimitiveType.FloatType
                }
                sum {
                  do!
                    json
                    |> JsonValue.AsEnum(Set.singleton "boolean")

                    |> sum.Map ignore

                  return ExprType.PrimitiveType PrimitiveType.BoolType
                }
                sum {
                  do!
                    json
                    |> JsonValue.AsEnum(Set.singleton "Date")

                    |> sum.Map ignore

                  return ExprType.PrimitiveType PrimitiveType.DateOnlyType
                }
                sum {
                  let! typeName = json |> JsonValue.AsString

                  return!
                    sum { return ExprType.LookupType { VarName = typeName } }
                    |> sum.MapError(Errors.WithPriority ErrorPriority.High)
                }
                sum {
                  let! fields = json |> JsonValue.AsRecord
                  let! kindJson = fields |> sum.TryFindField "kind"

                  do! kindJson |> JsonValue.AsEnum(Set.singleton "VarLookup") |> sum.Map ignore

                  let! varNameJson = fields |> sum.TryFindField "VarName"
                  let! varName = varNameJson |> JsonValue.AsString

                  return!
                    sum { return ExprType.VarType { VarName = varName } }
                    |> sum.MapError(Errors.WithPriority ErrorPriority.High)
                }
                sum {
                  let! fields = json |> JsonValue.AsRecord
                  let! fieldsJson = (fields |> sum.TryFindField "fields")
                  let! fieldsJson = fieldsJson |> JsonValue.AsRecord

                  let! fields =
                    fieldsJson
                    |> Seq.map (fun (fieldName, fieldType) ->
                      sum {
                        let! fieldType = !fieldType
                        return fieldName, fieldType
                      }
                      |> sum.MapError(Errors.Map(String.appendNewline $"\n...when parsing field {fieldName}")))
                    |> Seq.toList
                    |> sum.All

                  return ExprType.RecordType(fields |> Map.ofList)
                }
                sum {
                  let! fields = json |> JsonValue.AsRecord
                  let! funJson = (fields |> sum.TryFindField "fun")

                  return!
                    sum.Any(
                      NonEmptyList.OfList(
                        sum {
                          do!
                            funJson
                            |> JsonValue.AsEnum(Set.singleton "SingleSelection")

                            |> sum.Map(ignore)

                          return!
                            sum {
                              let! argsJson = (fields |> sum.TryFindField "args")
                              let! arg = JsonValue.AsSingleton argsJson
                              let! arg = !arg
                              return ExprType.OptionType arg
                            }
                            |> sum.MapError(Errors.WithPriority ErrorPriority.High)
                        },
                        [ sum {
                            do!
                              funJson
                              |> JsonValue.AsEnum(Set.singleton "Option")

                              |> sum.Map(ignore)

                            return!
                              sum {
                                let! argsJson = (fields |> sum.TryFindField "args")
                                let! arg = JsonValue.AsSingleton argsJson
                                let! arg = !arg
                                return ExprType.OptionType arg
                              }
                              |> sum.MapError(Errors.WithPriority ErrorPriority.High)
                          }
                          sum {
                            do!
                              funJson
                              |> JsonValue.AsEnum(Set.singleton "One")

                              |> sum.Map(ignore)

                            return!
                              sum {
                                let! argsJson = (fields |> sum.TryFindField "args")
                                let! arg = JsonValue.AsSingleton argsJson
                                let! arg = !arg
                                return ExprType.OneType arg
                              }
                              |> sum.MapError(Errors.WithPriority ErrorPriority.High)
                          }
                          sum {
                            do!
                              funJson
                              |> JsonValue.AsEnum(Set.singleton "Many")

                              |> sum.Map(ignore)

                            return!
                              sum {
                                let! argsJson = (fields |> sum.TryFindField "args")
                                let! arg = JsonValue.AsSingleton argsJson
                                let! arg = !arg
                                return ExprType.ManyType arg
                              }
                              |> sum.MapError(Errors.WithPriority ErrorPriority.High)
                          }
                          sum {
                            do!
                              funJson
                              |> JsonValue.AsEnum(Set.singleton "MultiSelection")

                              |> sum.Map(ignore)

                            return!
                              sum {
                                let! argsJson = (fields |> sum.TryFindField "args")
                                let! arg = JsonValue.AsSingleton argsJson
                                let! arg = !arg
                                return ExprType.SetType arg
                              }
                              |> sum.MapError(Errors.WithPriority ErrorPriority.High)
                          }
                          sum {
                            do!
                              funJson
                              |> JsonValue.AsEnum(Set.singleton "Tuple")

                              |> sum.Map(ignore)

                            return!
                              sum {
                                let! argsJson = (fields |> sum.TryFindField "args")
                                let! args = JsonValue.AsArray argsJson
                                let! args = args |> Seq.map (!) |> sum.All
                                return ExprType.TupleType args
                              }
                              |> sum.MapError(Errors.WithPriority ErrorPriority.High)
                          }
                          sum {
                            do!
                              funJson
                              |> JsonValue.AsEnum(Set.singleton "List")

                              |> sum.Map(ignore)

                            return!
                              sum {
                                let! argsJson = (fields |> sum.TryFindField "args")
                                let! arg = JsonValue.AsSingleton argsJson
                                let! arg = !arg
                                return ExprType.ListType arg
                              }
                              |> sum.MapError(Errors.WithPriority ErrorPriority.High)
                          }
                          sum {
                            do!
                              funJson
                              |> JsonValue.AsEnum(Set.singleton "Table")

                              |> sum.Map(ignore)

                            return!
                              sum {
                                let! argsJson = (fields |> sum.TryFindField "args")
                                let! arg = JsonValue.AsSingleton argsJson
                                let! arg = !arg
                                return ExprType.TableType arg
                              }
                              |> sum.MapError(Errors.WithPriority ErrorPriority.High)
                          }
                          sum {
                            do!
                              funJson
                              |> JsonValue.AsEnum(Set.singleton "ReadOnly")

                              |> sum.Map(ignore)

                            return!
                              sum {
                                let! argsJson = (fields |> sum.TryFindField "args")
                                let! arg = JsonValue.AsSingleton argsJson
                                let! arg = !arg
                                return ExprType.ReadOnlyType arg
                              }
                              |> sum.MapError(Errors.WithPriority ErrorPriority.High)
                          }
                          sum {
                            do!
                              funJson
                              |> JsonValue.AsEnum(Set.singleton "TranslationOverride")

                              |> sum.Map(ignore)

                            return!
                              sum {
                                let! argsJson = (fields |> sum.TryFindField "args")
                                let! args = JsonValue.AsSingleton argsJson
                                let! arg = JsonValue.AsString args
                                return ExprType.TranslationOverride arg
                              }
                              |> sum.MapError(Errors.WithPriority ErrorPriority.High)
                          }
                          sum {
                            do!
                              funJson
                              |> JsonValue.AsEnum(Set.singleton "Map")

                              |> sum.Map(ignore)

                            return!
                              sum {
                                let! argsJson = (fields |> sum.TryFindField "args")
                                let! key, value = JsonValue.AsPair argsJson
                                let! key, value = sum.All2 !key !value
                                return ExprType.MapType(key, value)
                              }
                              |> sum.MapError(Errors.WithPriority ErrorPriority.High)
                          }
                          sum {
                            do!
                              funJson
                              |> JsonValue.AsEnum(Set.singleton "Sum")

                              |> sum.Map(ignore)

                            return!
                              sum {
                                let! argsJson = (fields |> sum.TryFindField "args")
                                let! leftJson, rightJson = JsonValue.AsPair argsJson
                                let! left, right = sum.All2 !leftJson !rightJson
                                return ExprType.SumType(left, right)
                              }
                              |> sum.MapError(Errors.WithPriority ErrorPriority.High)
                          }
                          sum {
                            do!
                              funJson
                              |> JsonValue.AsEnum(Set.singleton "Union")

                              |> sum.Map(ignore)

                            return!
                              sum {
                                let! argsJson = (fields |> sum.TryFindField "args")
                                let! cases = argsJson |> JsonValue.AsArray

                                let! cases = sum.All(cases |> Seq.map (ExprType.ParseUnionCase))

                                return
                                  ExprType.UnionType(
                                    cases |> Seq.map (fun c -> { CaseName = c.CaseName }, c) |> Map.ofSeq
                                  )
                              }
                              |> sum.MapError(Errors.WithPriority ErrorPriority.High)
                          }
                          sum {
                            do!
                              funJson
                              |> JsonValue.AsEnum(Set.singleton "KeyOf")

                              |> sum.Map(ignore)

                            return!
                              sum {
                                let! argsJson = fields |> sum.TryFindField "args"
                                let! args = argsJson |> JsonValue.AsArray

                                if args.Length = 1 || args.Length = 2 then
                                  let! record = JsonValue.AsString args.[0]

                                  let! excludedKeys =
                                    if args.Length = 2 then
                                      args.[1] |> JsonValue.AsArray
                                    else
                                      sum.Return([||])

                                  let! excludedKeys = sum.All(excludedKeys |> Seq.map JsonValue.AsString)
                                  return ExprType.KeyOf(ExprType.LookupType { VarName = record }, excludedKeys)
                                else
                                  return!
                                    sum.Throw(
                                      Errors.Singleton
                                        $"Error: cannot parse generic type {funJson}. Expected a single type name or a single type name and a list of excluded keys, found {args}"
                                    )

                              // return
                              //   ExprType.UnionType(
                              //     record
                              //     |> Seq.map (fun c ->
                              //       { CaseName = c.Key },
                              //       { CaseName = c.Key
                              //         Fields = ExprType.UnitType })
                              //     |> Map.ofSeq
                              //   )
                              }
                              |> sum.MapError(Errors.WithPriority ErrorPriority.High)
                          }

                          sum.Throw(Errors.Singleton $"Error: cannot parse generic type {funJson}")
                          |> sum.MapError(Errors.WithPriority ErrorPriority.High) ]
                      )
                    )
                    |> sum.MapError(Errors.HighestPriority)
                } ]
            )
          )
      }
      |> sum.MapError(Errors.HighestPriority)

    static member ParseTypes<'context>
      (typesJson: seq<string * JsonValue>)
      : State<Unit, 'context, TypeContext, Errors> =
      state {

        let! typesJson =
          typesJson
          |> Seq.map (fun (name, json) ->
            state {
              let typeId: ExprTypeId = { VarName = name }

              do!
                state.SetState(
                  Map.add
                    name
                    { Type = ExprType.UnitType
                      TypeId = typeId }
                )

              return name, typeId, json
            })
          |> state.All

        for typeName, typeId, typeJson in typesJson do
          return!
            state {
              let! typeJsonArgs = typeJson |> JsonValue.AsRecord |> state.OfSum

              return!
                state.Any(
                  NonEmptyList.OfList(
                    state {
                      let extendsJson =
                        typeJsonArgs
                        |> sum.TryFindField "extends"
                        |> Sum.toOption
                        |> Option.defaultWith (fun () -> JsonValue.Array [||])

                      let! fieldsJson = typeJsonArgs |> sum.TryFindField "fields" |> state.OfSum

                      return!
                        state {
                          let! extends, fields =
                            state.All2
                              (extendsJson |> JsonValue.AsArray |> state.OfSum)
                              (fieldsJson |> JsonValue.AsRecord |> state.OfSum)

                          let! s = state.GetState()

                          let! extendedTypes =
                            extends
                            |> Seq.map (fun extendsJson ->
                              state {
                                let! parsed = ExprType.Parse extendsJson |> state.OfSum
                                return! ExprType.ResolveLookup s parsed |> state.OfSum
                              })
                            |> state.All

                          let! fields =
                            fields
                            |> Seq.map (fun (fieldName, fieldType) ->
                              state {
                                let! fieldType = ExprType.Parse fieldType |> state.OfSum
                                return fieldName, fieldType
                              }
                              |> state.MapError(
                                Errors.Map(String.appendNewline $"\n...when parsing field {fieldName}")
                              ))
                            |> Seq.toList
                            |> state.All

                          let fields = fields |> Map.ofList

                          let! exprType =
                            extendedTypes
                            |> Seq.fold
                              (fun (t1: Sum<ExprType, Errors>) t2 ->
                                sum {
                                  let! t1 = t1
                                  return! ExprType.Extend t1 t2
                                })
                              (Left(ExprType.RecordType fields))
                            |> state.OfSum

                          do! state.SetState(Map.add typeName { Type = exprType; TypeId = typeId })

                          return ()
                        }
                        |> state.MapError(Errors.WithPriority ErrorPriority.High)
                    },
                    [ state {
                        let typeId: ExprTypeId = { VarName = typeName }

                        let! parsedType = ExprType.Parse typeJson |> state.OfSum

                        do! state.SetState(Map.add typeName { Type = parsedType; TypeId = typeId })
                      }
                      state.Throw(
                        Errors.Singleton
                          $"...unexpected json shape for a type body {typeJson.ToFSharpString.ReasonablyClamped}"
                        |> Errors.WithPriority ErrorPriority.High
                      ) ]
                  )

                )
            }
            |> state.MapError(Errors.Map(String.appendNewline $"\n...when parsing type {typeName}"))

          let! ctx = state.GetState()

          return!
            ctx
            |> Map.toSeq
            |> Seq.map (fun (name, typeBinding) ->
              state {
                match typeBinding.Type with
                | KeyOf(LookupType t, excludedKeys) ->
                  let! resolved = TypeContext.TryFindType ctx t.VarName |> state.OfSum

                  match resolved.Type with
                  | RecordType record ->
                    excludedKeys
                    |> List.map (fun key ->
                      record
                      |> Map.tryFind key
                      |> Sum.fromOption (fun () -> Errors.Singleton $"Error: key {key} not found in record {record}"))
                    |> sum.All
                    |> state.OfSum

                    let union =
                      ExprType.UnionType(

                        record
                        |> Map.keys
                        |> Seq.filter (fun (key) -> excludedKeys |> Seq.contains key |> not)
                        |> Seq.map (fun key ->
                          { CaseName = key },
                          { CaseName = key
                            Fields = ExprType.UnitType })
                        |> Map.ofSeq
                      )


                    do! state.SetState(Map.add name { typeBinding with Type = union })
                  | _ -> return! state.Return()
                | _ -> return! state.Return()
              })
            |> state.All
            |> state.Map ignore

      }
      |> state.MapError(Errors.Map(String.appendNewline $"\n...when parsing types"))
