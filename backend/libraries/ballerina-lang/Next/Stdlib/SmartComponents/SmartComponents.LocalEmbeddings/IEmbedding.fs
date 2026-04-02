namespace SmartComponents.LocalEmbeddings

open System

/// Implements a representation for an embedded value.
type IEmbedding<'TEmbedding> =
  /// Computes the similarity between this embedding and another.
  abstract Similarity: 'TEmbedding -> single
