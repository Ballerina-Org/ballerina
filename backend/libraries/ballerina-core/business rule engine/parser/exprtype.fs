namespace Ballerina.DSL.Parser

module ExprType =
  open Patterns

  open Ballerina.DSL.Expr.Types.Model
  open Ballerina.DSL.Expr.Types.Patterns
  open System
  open Ballerina.State.WithError
  open Ballerina.Errors
  open Ballerina.Core.Json
  open Ballerina.Core.String
  open FSharp.Data
  open Ballerina.Collections.NonEmptyList
  open Ballerina.Collections.Sum
  open Ballerina.Core.Object

  type ExprType with
    static member ParseUnionCase<'config, 'context>
      (contextActions: ContextOperations<'context>)
      (json: JsonValue)
      : State<UnionCase, 'config, 'context, Errors> =
      let (!) = ExprType.Parse contextActions

      state {
        let! args = json |> JsonValue.AsRecord |> state.OfSum

        let! caseJson, fieldsJson =
          state.All2 (args |> state.TryFindField "caseName") (args |> state.TryFindField "fields")

        let! caseName = caseJson |> JsonValue.AsString |> state.OfSum

        let! fieldsType =
          state.Either
            (state {
              let! fieldsJson = fieldsJson |> JsonValue.AsRecord |> state.OfSum

              let! fields =
                fieldsJson
                |> Seq.map (fun (fieldName, fieldType) ->
                  state {
                    let! fieldType = !fieldType
                    return fieldName, fieldType
                  }
                  |> state.MapError(Errors.Map(String.appendNewline $"\n...when parsing field {fieldName}")))
                |> Seq.toList
                |> state.All

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

    static member Parse<'config, 'context>
      (contextActions: ContextOperations<'context>)
      (json: JsonValue)
      : State<ExprType, 'config, 'context, Errors> =
      let (!) = ExprType.Parse contextActions

      state {
        return!
          state.Any(
            NonEmptyList.OfList(
              state {
                do!
                  json
                  |> JsonValue.AsEnum(Set.singleton "unit")
                  |> state.OfSum
                  |> state.Map ignore

                return ExprType.UnitType
              },
              [ state {
                  do!
                    json
                    |> JsonValue.AsEnum(Set.singleton "guid")
                    |> state.OfSum
                    |> state.Map ignore

                  return ExprType.PrimitiveType PrimitiveType.GuidType
                }
                state {
                  do!
                    json
                    |> JsonValue.AsEnum(Set.singleton "string")
                    |> state.OfSum
                    |> state.Map ignore

                  return ExprType.PrimitiveType PrimitiveType.StringType
                }
                state {
                  do!
                    json
                    |> JsonValue.AsEnum(Set.singleton "number")
                    |> state.OfSum
                    |> state.Map ignore

                  return ExprType.PrimitiveType PrimitiveType.IntType
                }
                state {
                  do! json |> JsonValue.AsEnum(Set.singleton "int") |> state.OfSum |> state.Map ignore
                  return ExprType.PrimitiveType PrimitiveType.IntType
                }
                state {
                  do!
                    json
                    |> JsonValue.AsEnum(Set.singleton "float")
                    |> state.OfSum
                    |> state.Map ignore

                  return ExprType.PrimitiveType PrimitiveType.FloatType
                }
                state {
                  do!
                    json
                    |> JsonValue.AsEnum(Set.singleton "boolean")
                    |> state.OfSum
                    |> state.Map ignore

                  return ExprType.PrimitiveType PrimitiveType.BoolType
                }
                state {
                  do!
                    json
                    |> JsonValue.AsEnum(Set.singleton "Date")
                    |> state.OfSum
                    |> state.Map ignore

                  return ExprType.PrimitiveType PrimitiveType.DateOnlyType
                }
                state {
                  let! typeName = json |> JsonValue.AsString |> state.OfSum

                  return!
                    state {
                      let! s = state.GetState()
                      let! typeId = contextActions.TryFindType s typeName |> state.OfSum
                      return ExprType.LookupType typeId.TypeId
                    }
                    |> state.MapError(Errors.WithPriority ErrorPriority.High)
                }
                state {
                  let! fields = json |> JsonValue.AsRecord |> state.OfSum
                  let! funJson = (fields |> state.TryFindField "fun")

                  return!
                    state.Any(
                      NonEmptyList.OfList(
                        state {
                          do!
                            funJson
                            |> JsonValue.AsEnum(Set.singleton "SingleSelection")
                            |> state.OfSum
                            |> state.Map(ignore)

                          return!
                            state {
                              let! argsJson = (fields |> state.TryFindField "args")
                              let! arg = JsonValue.AsSingleton argsJson |> state.OfSum
                              let! arg = !arg
                              return ExprType.OptionType arg
                            }
                            |> state.MapError(Errors.WithPriority ErrorPriority.High)
                        },
                        [ state {
                            do!
                              funJson
                              |> JsonValue.AsEnum(Set.singleton "Option")
                              |> state.OfSum
                              |> state.Map(ignore)

                            return!
                              state {
                                let! argsJson = (fields |> state.TryFindField "args")
                                let! arg = JsonValue.AsSingleton argsJson |> state.OfSum
                                let! arg = !arg
                                return ExprType.OptionType arg
                              }
                              |> state.MapError(Errors.WithPriority ErrorPriority.High)
                          }
                          state {
                            do!
                              funJson
                              |> JsonValue.AsEnum(Set.singleton "One")
                              |> state.OfSum
                              |> state.Map(ignore)

                            return!
                              state {
                                let! argsJson = (fields |> state.TryFindField "args")
                                let! arg = JsonValue.AsSingleton argsJson |> state.OfSum
                                let! arg = !arg
                                return ExprType.OneType arg
                              }
                              |> state.MapError(Errors.WithPriority ErrorPriority.High)
                          }
                          state {
                            do!
                              funJson
                              |> JsonValue.AsEnum(Set.singleton "Many")
                              |> state.OfSum
                              |> state.Map(ignore)

                            return!
                              state {
                                let! argsJson = (fields |> state.TryFindField "args")
                                let! arg = JsonValue.AsSingleton argsJson |> state.OfSum
                                let! arg = !arg
                                return ExprType.ManyType arg
                              }
                              |> state.MapError(Errors.WithPriority ErrorPriority.High)
                          }
                          state {
                            do!
                              funJson
                              |> JsonValue.AsEnum(Set.singleton "MultiSelection")
                              |> state.OfSum
                              |> state.Map(ignore)

                            return!
                              state {
                                let! argsJson = (fields |> state.TryFindField "args")
                                let! arg = JsonValue.AsSingleton argsJson |> state.OfSum
                                let! arg = !arg
                                return ExprType.SetType arg
                              }
                              |> state.MapError(Errors.WithPriority ErrorPriority.High)
                          }
                          state {
                            do!
                              funJson
                              |> JsonValue.AsEnum(Set.singleton "Tuple")
                              |> state.OfSum
                              |> state.Map(ignore)

                            return!
                              state {
                                let! argsJson = (fields |> state.TryFindField "args")
                                let! args = JsonValue.AsArray argsJson |> state.OfSum
                                let! args = args |> Seq.map (!) |> state.All
                                return ExprType.TupleType args
                              }
                              |> state.MapError(Errors.WithPriority ErrorPriority.High)
                          }
                          state {
                            do!
                              funJson
                              |> JsonValue.AsEnum(Set.singleton "List")
                              |> state.OfSum
                              |> state.Map(ignore)

                            return!
                              state {
                                let! argsJson = (fields |> state.TryFindField "args")
                                let! arg = JsonValue.AsSingleton argsJson |> state.OfSum
                                let! arg = !arg
                                return ExprType.ListType arg
                              }
                              |> state.MapError(Errors.WithPriority ErrorPriority.High)
                          }
                          state {
                            do!
                              funJson
                              |> JsonValue.AsEnum(Set.singleton "Table")
                              |> state.OfSum
                              |> state.Map(ignore)

                            return!
                              state {
                                let! argsJson = (fields |> state.TryFindField "args")
                                let! arg = JsonValue.AsSingleton argsJson |> state.OfSum
                                let! arg = !arg
                                return ExprType.TableType arg
                              }
                              |> state.MapError(Errors.WithPriority ErrorPriority.High)
                          }
                          state {
                            do!
                              funJson
                              |> JsonValue.AsEnum(Set.singleton "Map")
                              |> state.OfSum
                              |> state.Map(ignore)

                            return!
                              state {
                                let! argsJson = (fields |> state.TryFindField "args")
                                let! key, value = JsonValue.AsPair argsJson |> state.OfSum
                                let! key, value = state.All2 !key !value
                                return ExprType.MapType(key, value)
                              }
                              |> state.MapError(Errors.WithPriority ErrorPriority.High)
                          }
                          state {
                            do!
                              funJson
                              |> JsonValue.AsEnum(Set.singleton "Sum")
                              |> state.OfSum
                              |> state.Map(ignore)

                            return!
                              state {
                                let! argsJson = (fields |> state.TryFindField "args")
                                let! leftJson, rightJson = JsonValue.AsPair argsJson |> state.OfSum
                                let! left, right = state.All2 !leftJson !rightJson
                                return ExprType.SumType(left, right)
                              }
                              |> state.MapError(Errors.WithPriority ErrorPriority.High)
                          }
                          state {
                            do!
                              funJson
                              |> JsonValue.AsEnum(Set.singleton "Union")
                              |> state.OfSum
                              |> state.Map(ignore)

                            return!
                              state {
                                let! argsJson = (fields |> state.TryFindField "args")
                                let! cases = argsJson |> JsonValue.AsArray |> state.OfSum

                                let! cases = state.All(cases |> Seq.map (ExprType.ParseUnionCase contextActions))

                                return
                                  ExprType.UnionType(
                                    cases |> Seq.map (fun c -> { CaseName = c.CaseName }, c) |> Map.ofSeq
                                  )
                              }
                              |> state.MapError(Errors.WithPriority ErrorPriority.High)
                          }
                          state {
                            do!
                              funJson
                              |> JsonValue.AsEnum(Set.singleton "KeyOf")
                              |> state.OfSum
                              |> state.Map(ignore)

                            return!
                              state {
                                let! argsJson = (fields |> state.TryFindField "args")
                                let! records = argsJson |> JsonValue.AsArray |> state.OfSum

                                let! records = state.All(records |> Seq.map (JsonValue.AsString >> state.OfSum))

                                if records.Length <> 1 then
                                  return!
                                    state.Throw(
                                      Errors.Singleton
                                        $"Error: cannot parse generic type {funJson}. Expected a single type name, found {records}"
                                    )
                                else
                                  let! s = state.GetState()
                                  let! record = contextActions.TryFindType s records.[0] |> state.OfSum
                                  let! record = record.Type |> ExprType.AsRecord |> state.OfSum

                                  return
                                    ExprType.UnionType(
                                      record
                                      |> Seq.map (fun c ->
                                        { CaseName = c.Key },
                                        { CaseName = c.Key
                                          Fields = ExprType.UnitType })
                                      |> Map.ofSeq
                                    )
                              }
                              |> state.MapError(Errors.WithPriority ErrorPriority.High)
                          }

                          state.Throw(Errors.Singleton $"Error: cannot parse generic type {funJson}")
                          |> state.MapError(Errors.WithPriority ErrorPriority.High) ]
                      )
                    )
                    |> state.MapError(Errors.HighestPriority)
                } ]
            )
          )
      }
      |> state.MapError(Errors.HighestPriority)

    static member ParseTypes<'context>
      (typesJson: seq<string * JsonValue>)
      : State<Unit, 'context, TypeContext, Errors> =
      state {

        let! typesJson =
          typesJson
          |> Seq.map (fun (name, json) ->
            state {
              let typeId: TypeId = { TypeName = name }

              do!
                state.SetState(
                  Map.add
                    name
                    { Type = ExprType.UnitType
                      TypeId = typeId
                      Const = false }
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

                      let isConstJson =
                        typeJsonArgs
                        |> sum.TryFindField "const"
                        |> Sum.toOption
                        |> Option.defaultWith (fun () -> JsonValue.Boolean false)

                      let! fieldsJson = typeJsonArgs |> sum.TryFindField "fields" |> state.OfSum

                      return!
                        state {
                          let! extends, fields, isConst =
                            state.All3
                              (extendsJson |> JsonValue.AsArray |> state.OfSum)
                              (fieldsJson |> JsonValue.AsRecord |> state.OfSum)
                              (isConstJson |> JsonValue.AsBoolean |> state.OfSum)

                          let! s = state.GetState()

                          let! extendedTypes =
                            extends
                            |> Seq.map (fun extendsJson ->
                              state {
                                let! parsed = ExprType.Parse TypeContext.ContextOperations extendsJson
                                return! ExprType.ResolveLookup s parsed |> state.OfSum
                              })
                            |> state.All

                          let! fields =
                            fields
                            |> Seq.map (fun (fieldName, fieldType) ->
                              state {
                                let! fieldType = ExprType.Parse TypeContext.ContextOperations fieldType
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

                          do!
                            state.SetState(
                              Map.add
                                typeName
                                { Type = exprType
                                  TypeId = typeId
                                  Const = isConst }
                            )

                          return ()
                        }
                        |> state.MapError(Errors.WithPriority ErrorPriority.High)
                    },
                    [ state {
                        let typeId: TypeId = { TypeName = typeName }

                        let! parsedType = ExprType.Parse TypeContext.ContextOperations typeJson

                        do!
                          state.SetState(
                            Map.add
                              typeName
                              { Type = parsedType
                                TypeId = typeId
                                Const = false }
                          )
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
      }
      |> state.MapError(Errors.Map(String.appendNewline $"\n...when parsing types"))
