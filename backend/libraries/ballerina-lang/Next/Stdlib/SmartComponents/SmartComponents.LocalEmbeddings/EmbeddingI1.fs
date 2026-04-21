namespace SmartComponents.LocalEmbeddings

open System
open System.Numerics
open System.Text.Json
open System.Text.Json.Serialization

[<Struct; NoComparison; NoEquality; JsonConverter(typeof<BitEmbeddingJsonConverter>)>]
type EmbeddingI1 =
  val private buffer: ReadOnlyMemory<byte>

  new(buffer: ReadOnlyMemory<byte>) = { buffer = buffer }

  member this.Buffer: ReadOnlyMemory<byte> = this.buffer

  static member private Quantize
    (input: ReadOnlySpan<single>, result: Span<byte>)
    : unit =
    let mutable j = 0

    while j < input.Length do
      let sources = input.Slice(j, 8)
      let mutable packed = 0uy

      if sources[0] >= 0.0f then
        packed <- packed ||| 128uy

      if sources[1] >= 0.0f then
        packed <- packed ||| 64uy

      if sources[2] >= 0.0f then
        packed <- packed ||| 32uy

      if sources[3] >= 0.0f then
        packed <- packed ||| 16uy

      if sources[4] >= 0.0f then
        packed <- packed ||| 8uy

      if sources[5] >= 0.0f then
        packed <- packed ||| 4uy

      if sources[6] >= 0.0f then
        packed <- packed ||| 2uy

      if sources[7] >= 0.0f then
        packed <- packed ||| 1uy

      result[j / 8] <- packed
      j <- j + 8

  static member FromModelOutput
    (input: ReadOnlySpan<single>, buffer: Memory<byte>)
    : EmbeddingI1 =
    let remainder = input.Length % 8

    if remainder <> 0 then
      invalidOp "Input length must be a multiple of 8"

    let expectedBufferLength = input.Length / 8

    if buffer.Length <> expectedBufferLength then
      invalidOp (
        sprintf
          "Buffer length was %i, but must be %i for an input with %i dimensions."
          buffer.Length
          expectedBufferLength
          input.Length
      )

    EmbeddingI1.Quantize(input, buffer.Span)
    EmbeddingI1(buffer)

  member this.Similarity(other: EmbeddingI1) : single =
    if other.buffer.Length <> this.buffer.Length then
      invalidOp (
        sprintf
          "Cannot compare a %s of length %i against one of length %i"
          "EmbeddingI1"
          other.buffer.Length
          this.buffer.Length
      )

    let lhs = this.buffer.Span
    let rhs = other.buffer.Span

    let mutable differences = 0
    let mutable i = 0

    while i < lhs.Length do
      let x = lhs[i] ^^^ rhs[i]
      differences <- differences + BitOperations.PopCount(uint32 x)
      i <- i + 1

    1.0f - (single differences / single (this.buffer.Length * 8))

  static member GetBufferByteLength(dimensions: int) : int = dimensions / 8

  interface IEmbedding<EmbeddingI1> with
    member this.Similarity(other: EmbeddingI1) : single = this.Similarity(other)

and BitEmbeddingJsonConverter() =
  inherit JsonConverter<EmbeddingI1>()

  override _.Read
    (
      reader: byref<Utf8JsonReader>,
      _typeToConvert: Type,
      _options: JsonSerializerOptions
    ) : EmbeddingI1 =
    EmbeddingI1(reader.GetBytesFromBase64())

  override _.Write
    (writer: Utf8JsonWriter, value: EmbeddingI1, _options: JsonSerializerOptions) : unit =
    writer.WriteBase64StringValue(value.Buffer.Span)
