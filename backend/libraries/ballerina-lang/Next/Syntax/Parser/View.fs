namespace Ballerina.DSL.Next.Syntax.Parser

[<AutoOpen>]
module View =

  open Ballerina.Parser
  open Ballerina.Collections.NonEmptyList
  open Ballerina
  open Ballerina.Collections.Sum
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.LocalizedErrors
  open Ballerina.Errors
  open Ballerina.DSL.Next.Types
  open Ballerina.DSL.Next.Terms
  open Ballerina.DSL.Next.Terms.Patterns
  open Model
  open Common
  open Type
  open Ballerina.DSL.Next.Syntax
  open Ballerina

  // ═══════════════════════════════════════════════════════════════════════════
  // JSX View Parser
  // ═══════════════════════════════════════════════════════════════════════════

  let private lessThanOperator = parseOperator Operator.LessThan
  let private greaterThanOperator = parseOperator Operator.GreaterThan
  let private tagSelfCloseOperator = parseOperator Operator.TagSelfClose
  let private tagCloseOperator = parseOperator Operator.TagClose

  let private tagIdentifier =
    parser.Exactly(fun t ->
      match t.Token with
      | Token.Identifier id -> Some id
      | _ -> None)

  /// Attribute name parser: accepts identifiers and keywords (e.g. type, for)
  let private attrIdentifier =
    parser.Exactly(fun t ->
      match t.Token with
      | Token.Identifier id -> Some id
      | Token.Keyword k -> Some (k.ToString().ToLowerInvariant())
      | _ -> None)

  /// Parse a string attribute value: attr="value"
  let private stringAttrValue =
    parser.Exactly(fun t ->
      match t.Token with
      | Token.StringLiteral s -> Some s
      | _ -> None)

  let viewExpr<'valueExt>
    (expr:
      unit
        -> Parser<
          Expr<TypeExpr<'valueExt>, Identifier, 'valueExt>,
          LocalizedToken,
          Location,
          Errors<Location>
         >)
    ()
    : Parser<
        Expr<TypeExpr<'valueExt>, Identifier, 'valueExt>,
        LocalizedToken,
        Location,
        Errors<Location>
       >
    =

    let rec viewNode () : Parser<ExprViewNode<TypeExpr<'valueExt>, Identifier, 'valueExt>, LocalizedToken, Location, Errors<Location>> =
      parser.Any
        [ viewElement ()
          viewFragment ()
          viewExprContainer ()
          viewTextNode () ]
      |> parser.MapError(Errors<_>.FilterHighestPriorityOnly)

    and viewElement () : Parser<ExprViewNode<TypeExpr<'valueExt>, Identifier, 'valueExt>, LocalizedToken, Location, Errors<Location>> =
      parser {
        let! loc = parser.Location
        do! lessThanOperator
        let! tag = tagIdentifier
        let! attrs = viewAttributes () |> parser.Many

        return!
          parser.Any
            [ // Self-closing: <tag attrs />
              parser {
                do! tagSelfCloseOperator
                return
                  { Location = loc
                    Node =
                      ExprViewNodeRec.ViewElement
                        { Tag = tag
                          Attributes = attrs
                          Children = []
                          SelfClosing = true } }
              }
              // Opening tag: <tag attrs>children</tag>
              parser {
                do! greaterThanOperator
                let! children = viewChildren ()
                do! tagCloseOperator
                let! _closeTag = tagIdentifier
                do! greaterThanOperator
                return
                  { Location = loc
                    Node =
                      ExprViewNodeRec.ViewElement
                        { Tag = tag
                          Attributes = attrs
                          Children = children
                          SelfClosing = false } }
              } ]
          |> parser.MapError(Errors<_>.FilterHighestPriorityOnly)
      }

    and viewFragment () : Parser<ExprViewNode<TypeExpr<'valueExt>, Identifier, 'valueExt>, LocalizedToken, Location, Errors<Location>> =
      parser {
        let! loc = parser.Location
        // <>
        do! lessThanOperator
        do! greaterThanOperator
        // children
        let! children = viewChildren ()
        // </>
        do! tagCloseOperator
        do! greaterThanOperator
        return
          { Location = loc
            Node = ExprViewNodeRec.ViewFragment children }
      }

    and viewExprContainer () : Parser<ExprViewNode<TypeExpr<'valueExt>, Identifier, 'valueExt>, LocalizedToken, Location, Errors<Location>> =
      parser {
        let! loc = parser.Location
        do! openCurlyBracketOperator
        let! e = expr ()
        do! closeCurlyBracketOperator
        return
          { Location = loc
            Node = ExprViewNodeRec.ViewExprContainer e }
      }

    and viewTextNode () : Parser<ExprViewNode<TypeExpr<'valueExt>, Identifier, 'valueExt>, LocalizedToken, Location, Errors<Location>> =
      parser {
        let! loc = parser.Location
        let! text =
          parser.Exactly(fun t ->
            match t.Token with
            | Token.StringLiteral s -> Some s
            | Token.Identifier s -> Some s
            | Token.Keyword k -> Some (k.ToString())
            | Token.IntLiteral n -> Some (string n)
            | Token.BoolLiteral b -> Some (string b)
            | Token.Operator op ->
              match op with
              | Operator.LessThan
              | Operator.GreaterThan
              | Operator.TagSelfClose
              | Operator.TagClose -> None
              | Operator.CurlyBracket Open
              | Operator.CurlyBracket Close -> None
              | _ -> Some (op.ToString())
            | _ -> None)
        return
          { Location = loc
            Node = ExprViewNodeRec.ViewText text }
      }

    and viewChildren () : Parser<List<ExprViewNode<TypeExpr<'valueExt>, Identifier, 'valueExt>>, LocalizedToken, Location, Errors<Location>> =
      viewNode () |> parser.Many

    and viewAttributes () : Parser<ExprViewAttribute<TypeExpr<'valueExt>, Identifier, 'valueExt>, LocalizedToken, Location, Errors<Location>> =
      parser {
        let! name = attrIdentifier
        do! equalsOperator
        return!
          parser.Any
            [ // String value: attr="value"
              parser {
                let! value = stringAttrValue
                return ExprViewAttribute.ViewAttrStringValue(name, value)
              }
              // Expression value: attr={expr}
              parser {
                do! openCurlyBracketOperator
                let! e = expr ()
                do! closeCurlyBracketOperator
                return ExprViewAttribute.ViewAttrExprValue(name, e)
              } ]
          |> parser.MapError(Errors<_>.FilterHighestPriorityOnly)
      }

    // Main view parser: view (param:Type) -> <body>
    parser {
      do! parseKeyword Keyword.View
      let! loc = parser.Location

      // Parse parameter: (name:Type) or just name
      let! paramName, paramType =
        parser.Any
          [ parser {
              do! openRoundBracketOperator
              let! paramName = singleIdentifier
              do! colonOperator
              let! typeDecl = typeDecl (expr ()) parseAllComplexTypeShapes
              do! closeRoundBracketOperator
              return paramName, Some typeDecl
            }
            parser {
              let! paramName = singleIdentifier
              return paramName, None
            } ]
        |> parser.MapError(Errors<_>.FilterHighestPriorityOnly)

      do! parseOperator Operator.SingleArrow
      let! body = viewNode ()

      return
        { Expr =
            ExprRec.View
              { Param = Var.Create paramName
                ParamType = paramType
                Body = body
                Location = loc }
          Location = loc
          Scope = TypeCheckScope.Empty }
    }

  /// Standalone JSX element expression: <tag .../> or <tag ...>children</tag>
  /// Used when JSX elements appear inside expression contexts (e.g. lambda bodies).
  /// Wraps the JSX node in an ExprRec.View with a synthesized unit parameter.
  let viewNodeExpr<'valueExt>
    (expr:
      unit
        -> Parser<
          Expr<TypeExpr<'valueExt>, Identifier, 'valueExt>,
          LocalizedToken,
          Location,
          Errors<Location>
         >)
    ()
    : Parser<
        Expr<TypeExpr<'valueExt>, Identifier, 'valueExt>,
        LocalizedToken,
        Location,
        Errors<Location>
       >
    =

    let rec viewNode () : Parser<ExprViewNode<TypeExpr<'valueExt>, Identifier, 'valueExt>, LocalizedToken, Location, Errors<Location>> =
      parser.Any
        [ viewElement ()
          viewFragment ()
          viewExprContainer ()
          viewTextNode () ]
      |> parser.MapError(Errors<_>.FilterHighestPriorityOnly)

    and viewElement () : Parser<ExprViewNode<TypeExpr<'valueExt>, Identifier, 'valueExt>, LocalizedToken, Location, Errors<Location>> =
      parser {
        let! loc = parser.Location
        do! lessThanOperator
        let! tag = tagIdentifier
        let! attrs = viewAttributes () |> parser.Many

        return!
          parser.Any
            [ parser {
                do! tagSelfCloseOperator
                return
                  { Location = loc
                    Node =
                      ExprViewNodeRec.ViewElement
                        { Tag = tag
                          Attributes = attrs
                          Children = []
                          SelfClosing = true } }
              }
              parser {
                do! greaterThanOperator
                let! children = viewChildren ()
                do! tagCloseOperator
                let! _closeTag = tagIdentifier
                do! greaterThanOperator
                return
                  { Location = loc
                    Node =
                      ExprViewNodeRec.ViewElement
                        { Tag = tag
                          Attributes = attrs
                          Children = children
                          SelfClosing = false } }
              } ]
          |> parser.MapError(Errors<_>.FilterHighestPriorityOnly)
      }

    and viewFragment () : Parser<ExprViewNode<TypeExpr<'valueExt>, Identifier, 'valueExt>, LocalizedToken, Location, Errors<Location>> =
      parser {
        let! loc = parser.Location
        do! lessThanOperator
        do! greaterThanOperator
        let! children = viewChildren ()
        do! tagCloseOperator
        do! greaterThanOperator
        return
          { Location = loc
            Node = ExprViewNodeRec.ViewFragment children }
      }

    and viewExprContainer () : Parser<ExprViewNode<TypeExpr<'valueExt>, Identifier, 'valueExt>, LocalizedToken, Location, Errors<Location>> =
      parser {
        let! loc = parser.Location
        do! openCurlyBracketOperator
        let! e = expr ()
        do! closeCurlyBracketOperator
        return
          { Location = loc
            Node = ExprViewNodeRec.ViewExprContainer e }
      }

    and viewTextNode () : Parser<ExprViewNode<TypeExpr<'valueExt>, Identifier, 'valueExt>, LocalizedToken, Location, Errors<Location>> =
      parser {
        let! loc = parser.Location
        let! text =
          parser.Exactly(fun t ->
            match t.Token with
            | Token.StringLiteral s -> Some s
            | Token.Identifier s -> Some s
            | Token.Keyword k -> Some (k.ToString().ToLowerInvariant())
            | Token.IntLiteral n -> Some (string n)
            | Token.BoolLiteral b -> Some (string b)
            | Token.Operator op ->
              match op with
              | Operator.LessThan
              | Operator.GreaterThan
              | Operator.TagSelfClose
              | Operator.TagClose -> None
              | Operator.CurlyBracket Open
              | Operator.CurlyBracket Close -> None
              | _ -> Some (op.ToString())
            | _ -> None)
        return
          { Location = loc
            Node = ExprViewNodeRec.ViewText text }
      }

    and viewChildren () : Parser<List<ExprViewNode<TypeExpr<'valueExt>, Identifier, 'valueExt>>, LocalizedToken, Location, Errors<Location>> =
      viewNode () |> parser.Many

    and viewAttributes () : Parser<ExprViewAttribute<TypeExpr<'valueExt>, Identifier, 'valueExt>, LocalizedToken, Location, Errors<Location>> =
      parser {
        let! name = attrIdentifier
        do! equalsOperator
        return!
          parser.Any
            [ parser {
                let! value = stringAttrValue
                return ExprViewAttribute.ViewAttrStringValue(name, value)
              }
              parser {
                do! openCurlyBracketOperator
                let! e = expr ()
                do! closeCurlyBracketOperator
                return ExprViewAttribute.ViewAttrExprValue(name, e)
              } ]
          |> parser.MapError(Errors<_>.FilterHighestPriorityOnly)
      }

    parser {
      let! loc = parser.Location
      let! body = viewNode ()
      return
        { Expr =
            ExprRec.View
              { Param = Var.Create "_"
                ParamType = None
                Body = body
                Location = loc }
          Location = loc
          Scope = TypeCheckScope.Empty }
    }

  // ═══════════════════════════════════════════════════════════════════════════
  // Coroutine Parser (co { ... })
  // ═══════════════════════════════════════════════════════════════════════════

  let coExpr<'valueExt>
    (expr:
      unit
        -> Parser<
          Expr<TypeExpr<'valueExt>, Identifier, 'valueExt>,
          LocalizedToken,
          Location,
          Errors<Location>
         >)
    ()
    : Parser<
        Expr<TypeExpr<'valueExt>, Identifier, 'valueExt>,
        LocalizedToken,
        Location,
        Errors<Location>
       >
    =

    let rec coStep () : Parser<ExprCoStep<TypeExpr<'valueExt>, Identifier, 'valueExt>, LocalizedToken, Location, Errors<Location>> =
      parser.Any
        [ coLetBang ()
          coDoBang ()
          coReturnBang ()
          coReturn () ]
      |> parser.MapError(Errors<_>.FilterHighestPriorityOnly)

    and coLetBang () : Parser<ExprCoStep<TypeExpr<'valueExt>, Identifier, 'valueExt>, LocalizedToken, Location, Errors<Location>> =
      parser {
        let! loc = parser.Location
        do! parseKeyword Keyword.Let
        do! parseOperator Operator.Bang
        let! varName = singleIdentifier
        do! equalsOperator
        let! value = expr ()
        do! semicolonOperator
        let! rest = coStep ()
        return
          { Location = loc
            Step = ExprCoStepRec.CoLetBang(Var.Create varName, value, rest) }
      }

    and coDoBang () : Parser<ExprCoStep<TypeExpr<'valueExt>, Identifier, 'valueExt>, LocalizedToken, Location, Errors<Location>> =
      parser {
        let! loc = parser.Location
        do! parseKeyword Keyword.Do
        do! parseOperator Operator.Bang
        let! value = expr ()
        do! semicolonOperator
        let! rest = coStep ()
        return
          { Location = loc
            Step = ExprCoStepRec.CoDoBang(value, rest) }
      }

    and coReturn () : Parser<ExprCoStep<TypeExpr<'valueExt>, Identifier, 'valueExt>, LocalizedToken, Location, Errors<Location>> =
      parser {
        let! loc = parser.Location
        // "return" is not a keyword, parse as identifier
        let! _ = parser.Exactly(fun t ->
          match t.Token with
          | Token.Identifier "return" -> Some ()
          | _ -> None)
        let! value = expr ()
        return
          { Location = loc
            Step = ExprCoStepRec.CoReturn value }
      }

    and coReturnBang () : Parser<ExprCoStep<TypeExpr<'valueExt>, Identifier, 'valueExt>, LocalizedToken, Location, Errors<Location>> =
      parser {
        let! loc = parser.Location
        // "return!" is "return" followed by "!"
        let! _ = parser.Exactly(fun t ->
          match t.Token with
          | Token.Identifier "return" -> Some ()
          | _ -> None)
        do! parseOperator Operator.Bang
        let! value = expr ()
        return
          { Location = loc
            Step = ExprCoStepRec.CoReturnBang value }
      }

    // Main co parser: co { steps }
    parser {
      do! parseKeyword Keyword.Co
      let! loc = parser.Location
      do! openCurlyBracketOperator
      let! body = coStep ()
      do! closeCurlyBracketOperator

      return
        { Expr =
            ExprRec.Co
              { Body = body
                Location = loc }
          Location = loc
          Scope = TypeCheckScope.Empty }
    }
