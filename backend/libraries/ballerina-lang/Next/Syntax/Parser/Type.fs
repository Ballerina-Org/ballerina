namespace Ballerina.DSL.Next.Syntax.Parser

[<AutoOpen>]
module Type =

  open System
  open Ballerina.Collections.Option
  open Ballerina.Parser
  open Ballerina.Collections.NonEmptyList
  open Ballerina.Collections.Sum
  open Ballerina.StdLib.Object
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.LocalizedErrors
  open Ballerina.DSL.Next.Terms

  type ComplexTypeKind =
    | ScopedIdentifier
    | ApplicationArguments
    | BinaryExpressionChain

  type ComplexType =
    | ScopedIdentifier of NonEmptyList<string>
    | ApplicationArguments of NonEmptyList<TypeExpr>
    | BinaryExpressionChain of NonEmptyList<BinaryTypeOperator * TypeExpr>

  let parseAllComplexTypeShapes: Set<ComplexTypeKind> =
    [ ComplexTypeKind.ApplicationArguments
      ComplexTypeKind.BinaryExpressionChain
      ComplexTypeKind.ScopedIdentifier ]
    |> Set.ofList

  let parseNoComplexTypeShapes: Set<ComplexTypeKind> = Set.empty

  let rec typeDecl (parseComplexShapes: Set<ComplexTypeKind>) : Parser<TypeExpr, _, _, Errors> =
    let lookupTypeDecl () =
      parser {
        let! id = identifierMatch
        return TypeExpr.Lookup(Identifier.LocalScope id)
      }

    let boolTypeDecl () =
      parser {
        let! id = identifierMatch

        match id with
        | "bool" -> return TypeExpr.Primitive PrimitiveType.Bool
        | _ ->
          let! loc = parser.Location
          return! (loc, $"Error: expected bool, got {id}") |> Errors.Singleton |> parser.Throw
      }

    let int32TypeDecl () =
      parser {
        let! id = identifierMatch

        match id with
        | "int32" -> return TypeExpr.Primitive PrimitiveType.Int32
        | _ ->
          let! loc = parser.Location
          return! (loc, $"Error: expected int32, got {id}") |> Errors.Singleton |> parser.Throw
      }

    let int64TypeDecl () =
      parser {
        let! id = identifierMatch

        match id with
        | "int64" -> return TypeExpr.Primitive PrimitiveType.Int64
        | _ ->
          let! loc = parser.Location
          return! (loc, $"Error: expected int64, got {id}") |> Errors.Singleton |> parser.Throw
      }

    let float32TypeDecl () =
      parser {
        let! id = identifierMatch

        match id with
        | "float32" -> return TypeExpr.Primitive PrimitiveType.Float32
        | _ ->
          let! loc = parser.Location
          return! (loc, $"Error: expected float32, got {id}") |> Errors.Singleton |> parser.Throw
      }

    let float64TypeDecl () =
      parser {
        let! id = identifierMatch

        match id with
        | "float64" -> return TypeExpr.Primitive PrimitiveType.Float64
        | _ ->
          let! loc = parser.Location
          return! (loc, $"Error: expected float64, got {id}") |> Errors.Singleton |> parser.Throw
      }

    let decimalTypeDecl () =
      parser {
        let! id = identifierMatch

        match id with
        | "decimal" -> return TypeExpr.Primitive PrimitiveType.Decimal
        | _ ->
          let! loc = parser.Location
          return! (loc, $"Error: expected decimal, got {id}") |> Errors.Singleton |> parser.Throw
      }

    let stringTypeDecl () =
      parser {
        let! id = identifierMatch

        match id with
        | "string" -> return TypeExpr.Primitive PrimitiveType.String
        | _ ->
          let! loc = parser.Location
          return! (loc, $"Error: expected string, got {id}") |> Errors.Singleton |> parser.Throw
      }

    let guidTypeDecl () =
      parser {
        let! id = identifierMatch

        match id with
        | "guid" -> return TypeExpr.Primitive PrimitiveType.Guid
        | _ ->
          let! loc = parser.Location
          return! (loc, $"Error: expected guid, got {id}") |> Errors.Singleton |> parser.Throw
      }

    let unitTypeDecl () =
      parser {
        do! openRoundBracketOperator
        do! closeRoundBracketOperator
        return TypeExpr.Primitive PrimitiveType.Unit
      }

    let record () =
      parser {
        do! openCurlyBracketOperator

        return!
          parser {
            let! fields =
              parser.Many(
                parser {
                  let! id = identifierMatch
                  do! colonOperator

                  return!
                    parser {
                      let! typeDecl = typeDecl parseAllComplexTypeShapes
                      do! semicolonOperator
                      return (id, typeDecl)
                    }
                    |> parser.MapError(Errors.SetPriority ErrorPriority.High)
                }
              )

            do! closeCurlyBracketOperator

            return
              TypeExpr.Record(
                fields
                |> List.map (fun (id, td) -> (id |> Identifier.LocalScope |> TypeExpr.Lookup, td))
              )
          }
          |> parser.MapError(Errors.SetPriority ErrorPriority.Medium)

      }

    let unionTypeDecl () =
      parser {
        do! pipeOperator |> parser.Lookahead |> parser.Ignore

        return!
          parser {
            let! cases =

              parser.AtLeastOne(
                parser {
                  do! pipeOperator

                  return!
                    parser {
                      let! id = identifierMatch
                      do! ofKeyword
                      let! typeDecl = typeDecl parseAllComplexTypeShapes
                      return (id, typeDecl)
                    }
                    |> parser.MapError(Errors.SetPriority ErrorPriority.High)
                }
              )
              |> parser.Map(NonEmptyList.ToList)

            return
              TypeExpr.Union(
                cases
                |> List.map (fun (id, td) -> (id |> Identifier.LocalScope |> TypeExpr.Lookup, td))
              )
          }
          |> parser.MapError(Errors.SetPriority ErrorPriority.High)
      }

    let scopedIdentifier () =
      parser {
        do! doubleColonOperator |> parser.Lookahead |> parser.Ignore

        return!
          parser {
            let! ids =
              parser.AtLeastOne(
                parser {
                  do! doubleColonOperator
                  return! identifierMatch
                }
              )

            return ids |> ScopedIdentifier
          }
          |> parser.MapError(Errors.SetPriority ErrorPriority.High)
      }

    let binaryExpressionChainTail () =
      parser {
        do! binaryTypeOperator |> parser.Lookahead |> parser.Ignore

        return!
          parser {
            let! fields =
              parser.AtLeastOne(
                parser {
                  let! op = binaryTypeOperator
                  let! value = typeDecl (parseComplexShapes |> Set.remove ComplexTypeKind.BinaryExpressionChain)
                  return op, value
                }
              )

            return fields |> ComplexType.BinaryExpressionChain
          }
          |> parser.MapError(Errors.SetPriority ErrorPriority.High)
      }

    let application () =
      parser {
        let! args = parser.AtLeastOne((fun () -> typeDecl parseAllComplexTypeShapes) |> betweenSquareBrackets)
        return args |> ComplexType.ApplicationArguments
      }

    let simpleShapes =
      [ (fun () -> typeDecl parseAllComplexTypeShapes) |> betweenBrackets
        record ()
        unionTypeDecl ()
        unitTypeDecl ()
        boolTypeDecl ()
        int32TypeDecl ()
        int64TypeDecl ()
        float32TypeDecl ()
        float64TypeDecl ()
        decimalTypeDecl ()
        stringTypeDecl ()
        guidTypeDecl ()
        lookupTypeDecl () ]

    parser {
      // let! s = parser.Stream

      // do
      //   Console.WriteLine(
      //     $"expr(parseComplexShapes={parseComplexShapes}) Stream = {s |> Seq.map (fun t -> t.Token.ToString()) |> Seq.truncate 10 |> Seq.toList}"
      //   )

      // do Console.ReadLine() |> ignore

      if parseComplexShapes |> Set.isEmpty then
        return! simpleShapes |> parser.Any
      else
        // let! s = parser.Stream

        // do
        //   Console.WriteLine(
        //     $"{String.replicate (depth * 2) indent}> nested-expr(parseComplexShapes=false) Stream = {s |> Seq.map (fun t -> t.Token.ToString()) |> Seq.truncate 10 |> Seq.toList}"
        //   )

        // do Console.ReadLine() |> ignore
        let! e = typeDecl parseNoComplexTypeShapes
        // do Console.Write $"{e.ToFSharpString}"
        // do Console.WriteLine $"included = {parseComplexShapes.ToFSharpString}"
        // do Console.ReadLine() |> ignore

        // do
        //   Console.WriteLine(
        //     $"{String.replicate (depth * 2) indent}> Trying to parse complex shape starting with {e.ToFSharpString}"
        //   )

        // do Console.ReadLine() |> ignore

        let complexShapes = []

        let complexShapes =
          if parseComplexShapes.Contains ComplexTypeKind.ScopedIdentifier then
            scopedIdentifier () :: complexShapes
          else
            complexShapes

        let complexShapes =
          if parseComplexShapes.Contains ComplexTypeKind.BinaryExpressionChain then
            binaryExpressionChainTail () :: complexShapes
          else
            complexShapes

        let complexShapes =
          if parseComplexShapes.Contains ComplexTypeKind.ApplicationArguments then
            application () :: complexShapes
          else
            complexShapes

        let! res = complexShapes |> parser.Any |> parser.Many

        // do Console.WriteLine $"~~{res.ToFSharpString}"
        // do Console.ReadLine() |> ignore
        let! loc = parser.Location

        let res =
          res
          |> List.fold
            (fun acc e ->
              sum {
                let! acc = acc

                match e with
                | BinaryExpressionChain fields ->
                  let fields: List<BinaryOperatorsElement<TypeExpr, BinaryTypeOperator>> =
                    fields
                    |> NonEmptyList.ToList
                    |> Seq.collect (fun (op, e) -> [ op |> Precedence.Operator; e |> Precedence.Operand ])
                    |> List.ofSeq

                  let chain = Operand acc :: fields

                  let precedence: List<OperatorsPrecedence<BinaryTypeOperator>> =
                    [ { Operators = [ BinaryTypeOperator.SingleArrow ] |> Set.ofList
                        Associativity = AssociateLeft }
                      { Operators = [ BinaryTypeOperator.Times ] |> Set.ofList
                        Associativity = AssociateLeft }
                      { Operators = [ BinaryTypeOperator.Plus ] |> Set.ofList
                        Associativity = AssociateLeft } ]

                  return!
                    collapseBinaryOperatorsChain
                      { Compose =
                          fun (e1, op, e2) ->
                            match op with
                            | BinaryTypeOperator.Times ->
                              match e1, e2 with
                              | TypeExpr.Tuple l1, TypeExpr.Tuple l2 -> TypeExpr.Tuple(l1 @ l2)
                              | TypeExpr.Tuple l1, _ -> TypeExpr.Tuple(l1 @ [ e2 ])
                              | _, TypeExpr.Tuple l2 -> TypeExpr.Tuple(e1 :: l2)
                              | _ -> TypeExpr.Tuple [ e1; e2 ]
                            | BinaryTypeOperator.Plus ->
                              match e1, e2 with
                              | TypeExpr.Sum l1, TypeExpr.Sum l2 -> TypeExpr.Sum(l1 @ l2)
                              | TypeExpr.Sum l1, _ -> TypeExpr.Sum(l1 @ [ e2 ])
                              | _, TypeExpr.Sum l2 -> TypeExpr.Sum(e1 :: l2)
                              | _ -> TypeExpr.Sum [ e1; e2 ]
                            | BinaryTypeOperator.SingleArrow -> TypeExpr.Arrow(e1, e2)
                        // | _ ->
                        //   TypeExpr.Apply(
                        //     TypeExpr.Apply(TypeExpr.Lookup(Identifier.LocalScope(op.ToString())), e1),
                        //     e2
                        //   )
                        ToExpr = id }
                      loc
                      precedence
                      chain
                | ScopedIdentifier ids ->
                  match acc with
                  | TypeExpr.Lookup(Identifier.LocalScope id) ->
                    let ids = (id :: (ids |> NonEmptyList.ToList)) |> List.rev
                    return TypeExpr.Lookup(Identifier.FullyQualified(ids.Tail, ids.Head))
                  | _ ->
                    return!
                      (loc, $"Error: cannot collapse scoped identifier chain on non-identifier")
                      |> Errors.Singleton
                      |> sum.Throw
                | ApplicationArguments args ->
                  return
                    args
                    |> NonEmptyList.ToList
                    |> List.fold (fun acc e -> TypeExpr.Apply(acc, e)) acc
              })
            (Sum.Left e)

        // let! s = parser.Stream

        // do
        //   Console.WriteLine(
        //     $"{String.replicate (depth * 2) indent}> success! nested-expr(parseComplexShapes=false) = {res.ToFSharpString}, remaining stream = {s |> Seq.map (fun t -> t.Token.ToString()) |> Seq.truncate 10 |> Seq.toList}"
        //   )

        match res with
        | Sum.Right e -> return! e |> parser.Throw
        | Sum.Left res -> return res
    }
