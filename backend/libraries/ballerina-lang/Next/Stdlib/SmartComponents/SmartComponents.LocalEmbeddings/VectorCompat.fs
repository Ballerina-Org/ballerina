namespace SmartComponents.LocalEmbeddings

open System.Runtime.CompilerServices

[<AbstractClass; Sealed>]
type internal VectorCompat private () =
  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  static member Identity<'T>(value: 'T) : 'T = value
