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
  open Ballerina.Grammar

  // ═══════════════════════════════════════════════════════════════════════════
  // JSX View Parser
  // ═══════════════════════════════════════════════════════════════════════════

  let lessThanOpRule = { Name = "less-than-op"; Rule = Terminal "<" }
  let greaterThanOpRule = { Name = "greater-than-op"; Rule = Terminal ">" }
  let tagSelfCloseOpRule = { Name = "tag-self-close-op"; Rule = Terminal "/>" }
  let tagCloseOpRule = { Name = "tag-close-op"; Rule = Terminal "</" }

  let private lessThanOperator = parseOperator Operator.LessThan |> AnnotatedParser.withNamedRule lessThanOpRule
  let private greaterThanOperator = parseOperator Operator.GreaterThan |> AnnotatedParser.withNamedRule greaterThanOpRule
  let private tagSelfCloseOperator = parseOperator Operator.TagSelfClose |> AnnotatedParser.withNamedRule tagSelfCloseOpRule
  let private tagCloseOperator = parseOperator Operator.TagClose |> AnnotatedParser.withNamedRule tagCloseOpRule

  let tagIdentifierRule = { Name = "tag-identifier"; Rule = Terminal "<tag-identifier>" }

  let private tagIdentifier =
    parser.Exactly(fun t ->
      match t.Token with
      | Token.Identifier id -> Some id
      | _ -> None)
    |> AnnotatedParser.withNamedRule tagIdentifierRule

  let attrIdentifierRule = { Name = "attr-identifier"; Rule = Terminal "<attr-identifier>" }

  /// Attribute name parser: accepts identifiers and keywords (e.g. type, for)
  let private attrIdentifier =
    parser.Exactly(fun t ->
      match t.Token with
      | Token.Identifier id -> Some id
      | Token.Keyword k -> Some (k.ToString().ToLowerInvariant())
      | _ -> None)
    |> AnnotatedParser.withNamedRule attrIdentifierRule

  let stringAttrValueRule = { Name = "string-attr-value"; Rule = Terminal "<string-literal>" }

  /// Parse a string attribute value: attr="value"
  let private stringAttrValue =
    parser.Exactly(fun t ->
      match t.Token with
      | Token.StringLiteral s -> Some s
      | _ -> None)
    |> AnnotatedParser.withNamedRule stringAttrValueRule

  let viewExprRule =
    { Name = "view-expr"
      Rule = Alt [ NonTerminal "view-element"; NonTerminal "view-fragment"; NonTerminal "view-expr-container"; NonTerminal "view-text-node" ] }

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
        do! lessThanOperator.Parser
        let! tag = tagIdentifier.Parser
        let! attrs = viewAttributes () |> parser.Many

        return!
          parser.Any
            [ // Self-closing: <tag attrs />
              parser {
                do! tagSelfCloseOperator.Parser
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
                do! greaterThanOperator.Parser
                let! children = viewChildren ()
                do! tagCloseOperator.Parser
                let! _closeTag = tagIdentifier.Parser
                do! greaterThanOperator.Parser
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
        do! lessThanOperator.Parser
        do! greaterThanOperator.Parser
        // children
        let! children = viewChildren ()
        // </>
        do! tagCloseOperator.Parser
        do! greaterThanOperator.Parser
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
        let! name = attrIdentifier.Parser
        do! equalsOperator
        return!
          parser.Any
            [ // String value: attr="value"
              parser {
                let! value = stringAttrValue.Parser
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
              let! paramName = singleIdentifier.Parser
              do! colonOperator
              let! typeDecl = (typeDecl (expr ()) parseAllComplexTypeShapes).Parser
              do! closeRoundBracketOperator
              return paramName, Some typeDecl
            }
            parser {
              let! paramName = singleIdentifier.Parser
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
    |> AnnotatedParser.withNamedRule viewExprRule

  let viewNodeExprRule =
    { Name = "view-node-expr"
      Rule = Alt [ NonTerminal "view-element"; NonTerminal "view-fragment"; NonTerminal "view-expr-container"; NonTerminal "view-text-node" ] }

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
        do! lessThanOperator.Parser
        let! tag = tagIdentifier.Parser
        let! attrs = viewAttributes () |> parser.Many

        return!
          parser.Any
            [ parser {
                do! tagSelfCloseOperator.Parser
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
                do! greaterThanOperator.Parser
                let! children = viewChildren ()
                do! tagCloseOperator.Parser
                let! _closeTag = tagIdentifier.Parser
                do! greaterThanOperator.Parser
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
        do! lessThanOperator.Parser
        do! greaterThanOperator.Parser
        let! children = viewChildren ()
        do! tagCloseOperator.Parser
        do! greaterThanOperator.Parser
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
        let! name = attrIdentifier.Parser
        do! equalsOperator
        return!
          parser.Any
            [ parser {
                let! value = stringAttrValue.Parser
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
    |> AnnotatedParser.withNamedRule viewNodeExprRule

  let coExprRule =
    { Name = "co-expr"
      Rule = Seq [ Terminal "co"; Terminal "{"; Repeat (Alt [ NonTerminal "co-let-bang"; NonTerminal "co-do-bang"; NonTerminal "co-return"; NonTerminal "co-return-bang"; NonTerminal "co-let" ]); Terminal "}" ] }

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
    =

    let rec coStep () : Parser<ExprCoStep<TypeExpr<'valueExt>, Identifier, 'valueExt>, LocalizedToken, Location, Errors<Location>> =
      parser.Any
        [ coLetBang ()
          coLet ()
          coDoBang ()
          coReturnBang ()
          coReturn () ]
      |> parser.MapError(Errors<_>.FilterHighestPriorityOnly)

    and coLetBang () : Parser<ExprCoStep<TypeExpr<'valueExt>, Identifier, 'valueExt>, LocalizedToken, Location, Errors<Location>> =
      parser {
        let! loc = parser.Location
        do! parseKeyword Keyword.Let
        do! parseOperator Operator.Bang
        let! varName = singleIdentifier.Parser
        do! equalsOperator
        let! value = expr ()
        do! semicolonOperator
        let! rest = coStep ()
        return
          { Location = loc
            Step = ExprCoStepRec.CoLetBang(Var.Create varName, value, rest) }
      }

    and coLet () : Parser<ExprCoStep<TypeExpr<'valueExt>, Identifier, 'valueExt>, LocalizedToken, Location, Errors<Location>> =
      parser {
        let! loc = parser.Location
        do! parseKeyword Keyword.Let
        let! varName = singleIdentifier.Parser
        do! equalsOperator
        let! value = expr ()
        do! semicolonOperator
        let! rest = coStep ()
        return
          { Location = loc
            Step = ExprCoStepRec.CoLet(Var.Create varName, value, rest) }
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
    |> AnnotatedParser.withNamedRule coExprRule

  let viewElementRule: NamedRule =
    { Name = "view-element"
      Rule = Alt [
        Seq [ NonTerminal "less-than-op"; NonTerminal "tag-identifier";
              Repeat (Seq [ NonTerminal "attr-identifier"; Terminal "="; Alt [ NonTerminal "string-attr-value"; Seq [ Terminal "{"; NonTerminal "expr"; Terminal "}" ] ] ]);
              NonTerminal "tag-self-close-op" ];
        Seq [ NonTerminal "less-than-op"; NonTerminal "tag-identifier";
              Repeat (Seq [ NonTerminal "attr-identifier"; Terminal "="; Alt [ NonTerminal "string-attr-value"; Seq [ Terminal "{"; NonTerminal "expr"; Terminal "}" ] ] ]);
              NonTerminal "greater-than-op"; Repeat (NonTerminal "view-node-expr"); NonTerminal "tag-close-op"; NonTerminal "tag-identifier"; NonTerminal "greater-than-op" ] ] }

  let viewFragmentRule: NamedRule =
    { Name = "view-fragment"
      Rule = Seq [ NonTerminal "less-than-op"; NonTerminal "greater-than-op";
                   Repeat (NonTerminal "view-node-expr");
                   NonTerminal "tag-close-op"; NonTerminal "greater-than-op" ] }

  let viewExprContainerRule: NamedRule =
    { Name = "view-expr-container"
      Rule = Seq [ Terminal "{"; NonTerminal "expr"; Terminal "}" ] }

  let viewTextNodeRule: NamedRule =
    { Name = "view-text-node"
      Rule = Terminal "<text>" }

  let coLetBangRule: NamedRule =
    { Name = "co-let-bang"
      Rule = Seq [ Terminal "let"; Terminal "!"; NonTerminal "identifier"; Terminal "="; NonTerminal "expr"; Terminal ";" ] }

  let coLetRule: NamedRule =
    { Name = "co-let"
      Rule = Seq [ Terminal "let"; NonTerminal "identifier"; Terminal "="; NonTerminal "expr"; Terminal ";" ] }

  let coDoBangRule: NamedRule =
    { Name = "co-do-bang"
      Rule = Seq [ Terminal "do"; Terminal "!"; NonTerminal "expr"; Terminal ";" ] }

  let coReturnRule: NamedRule =
    { Name = "co-return"
      Rule = Seq [ Terminal "return"; NonTerminal "expr" ] }

  let coReturnBangRule: NamedRule =
    { Name = "co-return-bang"
      Rule = Seq [ Terminal "return"; Terminal "!"; NonTerminal "expr" ] }

  let grammarRules: NamedRule list =
    [ lessThanOpRule; greaterThanOpRule; tagSelfCloseOpRule; tagCloseOpRule
      tagIdentifierRule; attrIdentifierRule; stringAttrValueRule
      viewElementRule; viewFragmentRule; viewExprContainerRule; viewTextNodeRule
      viewExprRule; viewNodeExprRule; coExprRule
      coLetBangRule; coLetRule; coDoBangRule; coReturnRule; coReturnBangRule ]
