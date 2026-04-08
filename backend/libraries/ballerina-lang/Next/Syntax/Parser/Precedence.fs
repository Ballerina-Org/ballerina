namespace Ballerina.DSL.Next.Syntax.Parser

[<AutoOpen>]
module Precedence =

  open System
  open Ballerina.Collections.Option
  open Ballerina.Parser
  open Ballerina.Collections.NonEmptyList
  open Ballerina
  open Ballerina.Collections.Sum
  open Ballerina.StdLib.Object
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.LocalizedErrors
  open Ballerina.Errors
  open Ballerina.DSL.Next.Terms

  type OperandMergeability =
    | Mergeable
    | NonMergeable

  type BinaryOperatorsElement<'operand, 'operator> =
    | Operand of 'operand * OperandMergeability
    | Operator of 'operator

  type BinaryOperatorsOperations<'operand, 'operator, 'expr> =
    { Compose:
        'operand * OperandMergeability * 'operator * 'operand * OperandMergeability -> 'operand * OperandMergeability
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
    : Sum<'expr, Errors<Location>> =
    let rec collapse (p: OperatorsPrecedence<'operator>) (l: List<BinaryOperatorsElement<'operand, 'operator>>) =
      sum {
        match l with
        | [ _ ] -> return l
        | Operand(e1, src1) :: Operator op :: Operand(e2, src2) :: rest when p.Operators |> Set.contains op ->
          return! (binOp.Compose(e1, src1, op, e2, src2) |> Operand) :: rest |> collapse p
        | Operand(e1, src1) :: Operator op :: rest ->
          let! rest1 = collapse p rest
          return Operand(e1, src1) :: Operator op :: rest1
        | _ ->
          return!
            (fun () -> $"Error: cannot collapse operators sequence {l.AsFSharpString}")
            |> Errors.Singleton loc
            |> sum.Throw
      }

    let rec collapseRight (p: OperatorsPrecedence<'operator>) (l: List<BinaryOperatorsElement<'operand, 'operator>>) =
      sum {
        // do
        //   Console.WriteLine
        //     $"Collapsing right-associative operators chain step: l={l.AsFSharpString}, prefix={prefix.AsFSharpString}"

        // do Console.ReadLine() |> ignore

        match l with
        | [ _ ] -> l
        | [ Operand(e1, src1); Operator op; Operand(e2, src2) ] when p.Operators |> Set.contains op ->
          return [ binOp.Compose(e1, src1, op, e2, src2) |> Operand ]
        | Operand(e1, src1) :: Operator op :: rest ->
          match! collapseRight p rest with
          | Operand(e2, src2) :: rest ->
            if p.Operators |> Set.contains op then
              return (binOp.Compose(e1, src1, op, e2, src2) |> Operand) :: rest
            else
              return Operand(e1, src1) :: Operator op :: Operand(e2, src2) :: rest
          | _ ->
            return!
              (fun () -> $"Error: cannot collapse operators sequence {l.AsFSharpString}")
              |> Errors.Singleton loc
              |> sum.Throw
        // | Operand(e1, src1) :: Operator op :: Operand(e2, src2) :: rest ->
        //   return! collapseRight p (Operand(e2, src2) :: rest) (Operator op :: (Operand(e1, src1) :: prefix))
        | _ ->
          return!
            (fun () -> $"Error: cannot collapse operators sequence {l.AsFSharpString}")
            |> Errors.Singleton loc
            |> sum.Throw
      }

    sum {
      match precedence with
      | [] ->
        match l0 with
        | [ Operand(x, _) ] -> return binOp.ToExpr x
        | l ->
          return!
            (fun () -> $"Error: invalid operator sequence, residual elements {l} cannot be further processes")
            |> Errors.Singleton loc
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
              let! res = collapseRight p l0
              return res
          }

        return! collapseBinaryOperatorsChain binOp loc ps l1
    }
