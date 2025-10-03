namespace Ballerina.DSL.Next.Syntax.Parser

[<AutoOpen>]
module Precedence =

  open System
  open Ballerina.Collections.Option
  open Ballerina.Parser
  open Ballerina.Collections.NonEmptyList
  open Ballerina.Collections.Sum
  open Ballerina.StdLib.Object
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.LocalizedErrors
  open Ballerina.DSL.Next.Terms

  type BinaryOperatorsElement<'operand, 'operator> =
    | Operand of 'operand
    | Operator of 'operator

  type BinaryOperatorsOperations<'operand, 'operator, 'expr> =
    { Compose: 'operand * 'operator * 'operand -> 'operand
      ToExpr: 'operand -> 'expr }

  type OperatorAssociativity =
    | AssociateLeft
    | AssociateRight

  type OperatorsPrecedence<'operator when 'operator: comparison> =
    { Operators: Set<'operator>
      Associativity: OperatorAssociativity }

  let rec collapseBinaryOperatorsChain<'operand, 'operator, 'expr when 'operator: comparison>
    (binOp: BinaryOperatorsOperations<'operand, 'operator, 'expr>)
    (loc: Location)
    (precedence: List<OperatorsPrecedence<'operator>>)
    (l0: List<BinaryOperatorsElement<'operand, 'operator>>)
    =
    let rec collapse (p: OperatorsPrecedence<'operator>) (l: List<BinaryOperatorsElement<'operand, 'operator>>) =
      sum {
        match l with
        | [ _ ] -> return l
        | Operand e1 :: Operator op :: Operand e2 :: rest when p.Operators |> Set.contains op ->
          return! (binOp.Compose(e1, op, e2) |> Operand) :: rest |> collapse p
        | Operand e1 :: Operator op :: rest ->
          let! rest1 = collapse p rest
          return Operand e1 :: Operator op :: rest1
        | _ ->
          return!
            (loc, $"Error: cannot collapse operators sequence {l.ToFSharpString}")
            |> Errors.Singleton
            |> sum.Throw
      }

    sum {
      match precedence with
      | [] ->
        match l0 with
        | [ Operand x ] -> return binOp.ToExpr x
        | l ->
          return!
            (loc, $"Error: invalid operator sequence, residual elements {l} cannot be further processes")
            |> Errors.Singleton
            |> sum.Throw
      | p :: ps ->
        let! l1 =
          sum {
            match p.Associativity with
            | OperatorAssociativity.AssociateLeft ->
              // do Console.WriteLine $"Collapsing left-associative operators chain with precedence {p.Operators.ToFSharpString} on {l0.ToFSharpString}"
              // do Console.ReadLine() |> ignore
              let! res = l0 |> collapse p
              // do Console.WriteLine $"Collapsed left-associative operators chain: {res.ToFSharpString}"
              // do Console.ReadLine() |> ignore
              return res
            | OperatorAssociativity.AssociateRight ->
              let l0 = l0 |> List.rev
              let! res = l0 |> collapse p
              return res |> List.rev
          }

        return! collapseBinaryOperatorsChain binOp loc ps l1
    }
