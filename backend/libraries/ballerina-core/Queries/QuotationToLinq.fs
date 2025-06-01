namespace Ballerina

module Queries =
  open System.Linq.Expressions
  open System

  /// Converts a F# Expression to a LINQ Lambda
  let private toLambda (exp: Quotations.Expr) =
    let linq =
      exp
      |> Microsoft.FSharp.Linq.RuntimeHelpers.LeafExpressionConverter.QuotationToExpression
      :?> MethodCallExpression

    linq.Arguments.[0] :?> LambdaExpression

  /// Converts a Lambda quotation into a Linq Lamba Expression with 1 parameter
  let ToLinq (exp: Quotations.Expr<'a -> 'b>) =
    let lambda = toLambda exp
    Expression.Lambda<Func<'a, 'b>>(lambda.Body, lambda.Parameters)
