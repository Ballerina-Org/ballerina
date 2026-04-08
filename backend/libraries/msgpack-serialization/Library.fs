module Ballerina.Serialization.MessagePack

// Note: we need custom converters are needed for types with private constructors (NonEmptyString, NonEmptySet)
// because the default reflection-based deserializer cannot call private constructors.
// See: https://aarnott.github.io/Nerdbank.MessagePack/docs/custom-converters.html
// Note: there are no specific F# examples, but there is always a mechanical translation (cursor can write it)

open Ballerina.Collections.Sum
open Ballerina.Collections.NonEmptySet
open Ballerina.Errors
open Ballerina.StdLib.String
open Nerdbank.MessagePack
open PolyType
open PolyType.ReflectionProvider
open System.Threading
open System.IO

type internal NonEmptyStringConverter() =
  inherit MessagePackConverter<NonEmptyString>()

  override _.Read(reader, _context) =
    let s = reader.ReadString()

    match NonEmptyString.TryCreate s with
    | Some v -> v
    | None -> raise (MessagePackSerializationException "Empty string cannot be deserialized as NonEmptyString")

  override _.Write(writer, value, _context) =
    writer.Write(NonEmptyString.AsString value)

// Generic converter for NonEmptySet<'e>, which has a private constructor.
// Serializes as a msgpack array of elements; deserializes via NonEmptySet.TryOfList.
//
// Accepts a ConverterContext in the constructor so the serializer can inject it
// when activating the open generic type on demand.
// See: https://aarnott.github.io/Nerdbank.MessagePack/docs/custom-converters.html#caching-sub-converters-with-convertercontext
type internal NonEmptySetConverter<'e when 'e: comparison>(context: ConverterContext) =
  inherit MessagePackConverter<NonEmptySet<'e>>()

  let elementConverter = context.GetConverter<'e>(ReflectionTypeShapeProvider.Default)

  override _.Read(reader, context) =
    context.DepthStep()
    let count = reader.ReadArrayHeader()
    let mutable items = []

    for _ in 0 .. count - 1 do
      let item = elementConverter.Read(&reader, context)
      do items <- item :: items

    match NonEmptySet.TryOfList(List.rev items) with
    | Some s -> s
    | None -> raise (MessagePackSerializationException "Cannot deserialize empty collection as NonEmptySet")

  override _.Write(writer, value, context) =
    context.DepthStep()
    let items = NonEmptySet.ToList value
    writer.WriteArrayHeader items.Length // C-style populating the buffer to be written, very low-level API but we work with what we have

    for item in items do
      elementConverter.Write(&writer, &item, context)

let private createSerializer (converters: MessagePackConverter seq) =
  let defaultConverters = seq { NonEmptyStringConverter() :> MessagePackConverter }
  let converters = defaultConverters |> Seq.append converters

  MessagePackSerializer(
    PreserveReferences = ReferencePreservationMode.AllowCycles,
    // PredictionSchema contains deeply recursive form structures (forms -> renderers -> nested renderers
    // -> inline forms -> renderers -> ...). The default MaxDepth (64) is insufficient for real production
    // data. 256 accommodates realistic nesting while still guarding against infinite recursion.
    // See: https://aarnott.github.io/Nerdbank.MessagePack/docs/security.html#stack-overflows
    StartingContext = SerializationContext(MaxDepth = 256),
    // Concrete (non-generic) converters are registered as instances via Converters.
    Converters = ConverterCollection.Create(System.ReadOnlySpan(Array.ofSeq converters)),
    // Open generic converters are registered as type definitions via ConverterTypes.
    // The serializer constructs closed instances on demand (e.g. NonEmptySetConverter<NonEmptyString>)
    // and injects the ConverterContext into the constructor automatically.
    ConverterTypes =
      ConverterTypeCollection.Create(
        System.ReadOnlySpan [| ConverterTypeCollection.ConverterType typedefof<NonEmptySetConverter<_>> |]
      )
  )

type MessagePackSerializer<'value> = 'value -> Sum<byte array, Errors<Unit>>
type MessagePackDeserializer<'value> = byte array -> Sum<'value, Errors<Unit>>
type MessagePackAsyncDeserializer<'value> = CancellationToken -> Stream -> Tasks.ValueTask<'value>

type MessagePackSerializerAdapter(serializer) =
  let serializer: MessagePackSerializer = serializer

  new() = MessagePackSerializerAdapter(createSerializer [])

  new(converters) = MessagePackSerializerAdapter(createSerializer converters)

  member _.Serialize<'value>(value: 'value) : Sum<byte array, Errors<Unit>> =
    try
      let typeShape = ReflectionTypeShapeProvider.Default.GetTypeShapeOrThrow<'value>()
      Left(serializer.Serialize(&value, typeShape))
    with ex ->
      Right(Errors.Singleton () (fun () -> ex.ToString()))

  member _.Deserialize<'value>(bytes: byte array) : Sum<'value, Errors<Unit>> =
    try
      let typeShape = ReflectionTypeShapeProvider.Default.GetTypeShapeOrThrow<'value>()
      Left(serializer.Deserialize(bytes, typeShape))
    with ex ->
      Right(Errors.Singleton () (fun () -> ex.ToString()))

  member _.DeserializeAsync<'value> (cancellation: CancellationToken) (stream: Stream) =
    let typeShape = ReflectionTypeShapeProvider.Default.GetTypeShapeOrThrow<'value>()
    serializer.DeserializeAsync(stream, typeShape, cancellation)
