namespace SmartComponents.LocalEmbeddings

open System
open System.Buffers
open System.Numerics.Tensors
open System.Runtime.InteropServices
open System.Text.Json
open System.Text.Json.Serialization

[<AbstractClass; Sealed>]
type internal Utils private () =
  static member CastByteToSingle(fromMemory: Memory<byte>) : ReadOnlyMemory<single> =
    ReadOnlyMemory<single>(MemoryMarshal.Cast<byte, single>(fromMemory.Span).ToArray())

  static member CastByteToSByte(fromMemory: Memory<byte>) : ReadOnlyMemory<sbyte> =
    ReadOnlyMemory<sbyte>(MemoryMarshal.Cast<byte, sbyte>(fromMemory.Span).ToArray())

[<Struct; JsonConverter(typeof<FloatEmbeddingJsonConverter>)>]
type EmbeddingF32 =
  val private buffer: ReadOnlyMemory<byte>
  val private values: ReadOnlyMemory<single>

  new(buffer: ReadOnlyMemory<byte>) =
    { buffer = buffer
      values = Utils.CastByteToSingle(MemoryMarshal.AsMemory(buffer)) }

  member this.Buffer: ReadOnlyMemory<byte> = this.buffer
  member this.Values: ReadOnlyMemory<single> = this.values

  member this.Similarity(other: EmbeddingF32) : single =
    TensorPrimitives.CosineSimilarity(this.values.Span, other.values.Span)

  static member GetBufferByteLength(dimensions: int) : int = dimensions * sizeof<single>

  static member FromModelOutput(input: ReadOnlySpan<single>, buffer: Memory<byte>) : EmbeddingF32 =
    let requiredBufferLength = EmbeddingF32.GetBufferByteLength(input.Length)

    if buffer.Length <> requiredBufferLength then
      invalidOp (
        sprintf
          "For an input with %i dimensions, the buffer length must be equal to %i, but it was %i."
          input.Length
          requiredBufferLength
          buffer.Length
      )

    MemoryMarshal.AsBytes(input).CopyTo(buffer.Span)
    EmbeddingF32(buffer)

  interface IEmbedding<EmbeddingF32> with
    member this.Similarity(other: EmbeddingF32) : single = this.Similarity(other)

and FloatEmbeddingJsonConverter() =
  inherit JsonConverter<EmbeddingF32>()

  override _.Read(reader: byref<Utf8JsonReader>, _typeToConvert: Type, _options: JsonSerializerOptions) : EmbeddingF32 =
    EmbeddingF32(reader.GetBytesFromBase64())

  override _.Write(writer: Utf8JsonWriter, value: EmbeddingF32, _options: JsonSerializerOptions) : unit =
    writer.WriteBase64StringValue(value.Buffer.Span)
