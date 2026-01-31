namespace Ballerina.DSL.Next.StdLib.MemoryDB.Extension

[<AutoOpen>]
module CommonTypeDefinitions =
  open Ballerina.DSL.Next.Types
  open Ballerina.DSL.Next.Types.TypeChecker

  /// Creates a common schema entity type application
  let createSchemaEntityTypeApplication
    (schemaParam: string)
    (entityParam: string)
    (entityWithPropsParam: string)
    (entityIdParam: string)
    : TypeExpr<'ext> =
    TypeExpr.Apply(
      TypeExpr.Apply(
        TypeExpr.Apply(
          TypeExpr.Apply(
            TypeExpr.Lookup("SchemaEntity" |> Identifier.LocalScope),
            TypeExpr.Lookup(schemaParam |> Identifier.LocalScope)
          ),
          TypeExpr.Lookup(entityParam |> Identifier.LocalScope)
        ),
        TypeExpr.Lookup(entityWithPropsParam |> Identifier.LocalScope)
      ),
      TypeExpr.Lookup(entityIdParam |> Identifier.LocalScope)
    )

  /// Creates a common schema relation type application
  let createSchemaRelationTypeApplication
    (schemaParam: string)
    (fromParam: string)
    (fromWithPropsParam: string)
    (fromIdParam: string)
    (toParam: string)
    (toWithPropsParam: string)
    (toIdParam: string)
    : TypeExpr<'ext> =
    TypeExpr.Apply(
      TypeExpr.Apply(
        TypeExpr.Apply(
          TypeExpr.Apply(
            TypeExpr.Apply(
              TypeExpr.Apply(
                TypeExpr.Apply(
                  TypeExpr.Lookup("SchemaRelation" |> Identifier.LocalScope),
                  TypeExpr.Lookup(schemaParam |> Identifier.LocalScope)
                ),
                TypeExpr.Lookup(fromParam |> Identifier.LocalScope)
              ),
              TypeExpr.Lookup(fromWithPropsParam |> Identifier.LocalScope)
            ),
            TypeExpr.Lookup(fromIdParam |> Identifier.LocalScope)
          ),
          TypeExpr.Lookup(toParam |> Identifier.LocalScope)
        ),
        TypeExpr.Lookup(toWithPropsParam |> Identifier.LocalScope)
      ),
      TypeExpr.Lookup(toIdParam |> Identifier.LocalScope)
    )

  /// Creates a Map type application
  let createMapTypeApplication
    (keyParam: string)
    (valueType: TypeExpr<ResolvedIdentifier>)
    : TypeExpr<ResolvedIdentifier> =
    TypeExpr.Apply(
      TypeExpr.Apply(
        TypeExpr.Lookup("Map" |> Identifier.LocalScope),
        TypeExpr.Lookup(keyParam |> Identifier.LocalScope)
      ),
      valueType
    )

  /// Creates the standard kind for schema operations
  let standardSchemaOperationKind: Kind =
    Kind.Arrow(Kind.Schema, Kind.Arrow(Kind.Star, Kind.Arrow(Kind.Star, Kind.Arrow(Kind.Star, Kind.Star))))

  /// Creates the standard kind for schema relation operations
  let standardSchemaRelationOperationKind: Kind =
    Kind.Arrow(
      Kind.Schema,
      Kind.Arrow(
        Kind.Star,
        Kind.Arrow(
          Kind.Star,
          Kind.Arrow(Kind.Star, Kind.Arrow(Kind.Star, Kind.Arrow(Kind.Star, Kind.Arrow(Kind.Star, Kind.Star))))
        )
      )
    )
