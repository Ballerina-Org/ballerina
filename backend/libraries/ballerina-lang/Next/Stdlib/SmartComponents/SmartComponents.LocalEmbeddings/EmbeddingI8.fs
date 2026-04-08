namespace SmartComponents.LocalEmbeddings

open System
open System.Numerics.Tensors
open System.Runtime.InteropServices
open System.Text.Json
open System.Text.Json.Serialization

[<Struct; JsonConverter(typeof<ByteEmbeddingJsonConverter>)>]
type EmbeddingI8 =
  val private buffer: ReadOnlyMemory<byte>
  val private values: ReadOnlyMemory<sbyte>
  val private magnitude: single

  new(buffer: ReadOnlyMemory<byte>) =
    { buffer = buffer
      values = Utils.CastByteToSByte(MemoryMarshal.AsMemory(buffer.Slice(4)))
      magnitude = BitConverter.ToSingle(buffer.Span) }

  member this.Buffer: ReadOnlyMemory<byte> = this.buffer
  member this.Values: ReadOnlyMemory<sbyte> = this.values
  member this.Magnitude: single = this.magnitude

  static member FromModelOutput
    (input: ReadOnlySpan<single>, buffer: Memory<byte>)
    : EmbeddingI8 =
    let requiredBufferLength = EmbeddingI8.GetBufferByteLength(input.Length)

    if buffer.Length <> requiredBufferLength then
      invalidOp (
        sprintf
          "For an input with %i dimensions, the buffer length must be equal to %i, but it was %i."
          input.Length
          requiredBufferLength
          buffer.Length
      )

    let maxComponent = MathF.Abs(TensorPrimitives.MaxMagnitude(input))
    let scaleFactor = 127.0f / maxComponent
    let quantized = MemoryMarshal.Cast<byte, sbyte>(buffer.Span.Slice(4))

    let mutable magnitudeSquared = 0.0f
    let mutable i = 0

    while i < input.Length do
      let scaled = input[i] * scaleFactor
      let rounded = int (MathF.Round(scaled))
      let clamped = Math.Clamp(rounded, -128, 127)
      let value = sbyte clamped
      quantized[i] <- value
      magnitudeSquared <- magnitudeSquared + (single (clamped * clamped))
      i <- i + 1

    BitConverter.TryWriteBytes(buffer.Span, MathF.Sqrt(magnitudeSquared))
    |> ignore

    EmbeddingI8(buffer)

  member this.Similarity(other: EmbeddingI8) : single =
    let length = this.values.Length

    if other.values.Length <> length then
      invalidOp (
        sprintf
          "This is of length %i, whereas %s is of length %i. They must be equal length."
          this.values.Length
          "other"
          other.values.Length
      )

    let lhs = this.values.Span
    let rhs = other.values.Span

    let mutable dot = 0.0f
    let mutable i = 0

    while i < length do
      dot <- dot + single ((int lhs[i]) * (int rhs[i]))
      i <- i + 1

    dot / (this.magnitude * other.magnitude)

  static member GetBufferByteLength(dimensions: int) : int = 4 + dimensions

  interface IEmbedding<EmbeddingI8> with
    member this.Similarity(other: EmbeddingI8) : single = this.Similarity(other)

and ByteEmbeddingJsonConverter() =
  inherit JsonConverter<EmbeddingI8>()

  override _.Read
    (
      reader: byref<Utf8JsonReader>,
      _typeToConvert: Type,
      _options: JsonSerializerOptions
    ) : EmbeddingI8 =
    EmbeddingI8(reader.GetBytesFromBase64())

  override _.Write
    (writer: Utf8JsonWriter, value: EmbeddingI8, _options: JsonSerializerOptions) : unit =
    writer.WriteBase64StringValue(value.Buffer.Span)
