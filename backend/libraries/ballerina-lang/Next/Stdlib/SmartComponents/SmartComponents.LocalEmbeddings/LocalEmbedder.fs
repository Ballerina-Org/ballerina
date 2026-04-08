namespace SmartComponents.LocalEmbeddings

open System
open System.Collections.Generic
open System.IO
open System.Linq
open System.Threading
open System.Threading.Tasks
open Microsoft.SemanticKernel
open Microsoft.SemanticKernel.Connectors.Onnx
open Microsoft.SemanticKernel.Embeddings

[<Sealed>]
type LocalEmbedder(modelName: string, options: BertOnnxOptions) =
  let embeddingGenerator =
    BertOnnxTextEmbeddingGenerationService.Create(
      LocalEmbedder.GetFullPathToModelFile(modelName, "model.onnx"),
      vocabPath = LocalEmbedder.GetFullPathToModelFile(modelName, "vocab.txt"),
      options = options
    )

  new(options: BertOnnxOptions) = LocalEmbedder("default", options)

  new(?modelName: string, ?caseSensitive: bool, ?maximumTokens: int) =
    let actualModelName = defaultArg modelName "default"
    let actualCaseSensitive = defaultArg caseSensitive false
    let actualMaximumTokens = defaultArg maximumTokens 512

    LocalEmbedder(
      actualModelName,
      BertOnnxOptions(
        CaseSensitive = actualCaseSensitive,
        MaximumTokens = actualMaximumTokens
      )
    )

  static member private GetFullPathToModelFile
    (modelName: string, fileName: string)
    : string =
    let baseDir = AppContext.BaseDirectory

    let fullPath =
      Path.Combine(baseDir, "LocalEmbeddingsModel", modelName, fileName)

    if not (File.Exists(fullPath)) then
      invalidOp (sprintf "Required file %s does not exist" fullPath)

    fullPath

  static member private GetBufferByteLength<'TEmbedding
    when 'TEmbedding :> IEmbedding<'TEmbedding>>
    ()
    : int -> int =
    let t = typeof<'TEmbedding>

    if t = typeof<EmbeddingF32> then
      EmbeddingF32.GetBufferByteLength >> unbox
    elif t = typeof<EmbeddingI1> then
      EmbeddingI1.GetBufferByteLength >> unbox
    elif t = typeof<EmbeddingI8> then
      EmbeddingI8.GetBufferByteLength >> unbox
    else
      invalidOp (sprintf "Unsupported embedding type: %s" t.FullName)

  static member private FromModelOutput<'TEmbedding
    when 'TEmbedding :> IEmbedding<'TEmbedding>>
    (input: ReadOnlySpan<single>, buffer: Memory<byte>)
    : 'TEmbedding =
    let t = typeof<'TEmbedding>

    if t = typeof<EmbeddingF32> then
      box (EmbeddingF32.FromModelOutput(input, buffer)) |> unbox<'TEmbedding>
    elif t = typeof<EmbeddingI1> then
      box (EmbeddingI1.FromModelOutput(input, buffer)) |> unbox<'TEmbedding>
    elif t = typeof<EmbeddingI8> then
      box (EmbeddingI8.FromModelOutput(input, buffer)) |> unbox<'TEmbedding>
    else
      invalidOp (sprintf "Unsupported embedding type: %s" t.FullName)

  member _.Attributes: IReadOnlyDictionary<string, obj> =
    upcast embeddingGenerator.Attributes

  member this.Embed(inputText: string) : EmbeddingF32 =
    this.Embed<EmbeddingF32>(inputText)

  member this.EmbedAsync(inputText: string) : Task<EmbeddingF32> =
    this.EmbedAsync<EmbeddingF32>(inputText)

  member this.Embed<'TEmbedding when 'TEmbedding :> IEmbedding<'TEmbedding>>
    (inputText: string, ?outputBuffer: Memory<byte>)
    : 'TEmbedding =
    this.EmbedAsync<'TEmbedding>(inputText, ?outputBuffer = outputBuffer).Result

  member this.EmbedAsync<'TEmbedding when 'TEmbedding :> IEmbedding<'TEmbedding>>
    (inputText: string, ?outputBuffer: Memory<byte>)
    : Task<'TEmbedding> =
    task {
      let! embeddings =
        embeddingGenerator.GenerateEmbeddingsAsync([| inputText |])

      let embedding = embeddings.Single()
      let getBufferByteLength = LocalEmbedder.GetBufferByteLength<'TEmbedding>()

      let buffer =
        match outputBuffer with
        | Some provided -> provided
        | None ->
          Array.zeroCreate<byte> (getBufferByteLength embedding.Span.Length)
          |> Memory<byte>

      return LocalEmbedder.FromModelOutput<'TEmbedding>(embedding.Span, buffer)
    }

  member this.EmbedRange
    (items: IEnumerable<string>)
    : IList<string * EmbeddingF32> =
    items.Select(fun item -> item, this.Embed<EmbeddingF32>(item)).ToList()

  member this.EmbedRange<'TEmbedding when 'TEmbedding :> IEmbedding<'TEmbedding>>
    (items: IEnumerable<string>)
    : IEnumerable<string * 'TEmbedding> =
    items.Select(fun item -> item, this.Embed<'TEmbedding>(item)).ToList()

  member this.EmbedRange<'TItem>
    (items: IEnumerable<'TItem>, textRepresentation: Func<'TItem, string>)
    : IEnumerable<'TItem * EmbeddingF32> =
    items
      .Select(fun item ->
        item, this.Embed<EmbeddingF32>(textRepresentation.Invoke(item)))
      .ToList()

  member this.EmbedRange<'TItem, 'TEmbedding
    when 'TEmbedding :> IEmbedding<'TEmbedding>>
    (items: IEnumerable<'TItem>, textRepresentation: Func<'TItem, string>)
    : IEnumerable<'TItem * 'TEmbedding> =
    items
      .Select(fun item ->
        item, this.Embed<'TEmbedding>(textRepresentation.Invoke(item)))
      .ToList()

  member _.GenerateEmbeddingsAsync
    (data: IList<string>, ?kernel: Kernel, ?cancellationToken: CancellationToken) : Task<
                                                                                      IList<
                                                                                        ReadOnlyMemory<
                                                                                          single
                                                                                         >
                                                                                       >
                                                                                     >
    =
    embeddingGenerator.GenerateEmbeddingsAsync(
      data,
      ?kernel = kernel,
      ?cancellationToken = cancellationToken
    )

  interface IDisposable with
    member _.Dispose() : unit = embeddingGenerator.Dispose()

  interface ITextEmbeddingGenerationService with
    member this.Attributes = this.Attributes

    member this.GenerateEmbeddingsAsync
      (data: IList<string>, kernel: Kernel, cancellationToken: CancellationToken) =
      this.GenerateEmbeddingsAsync(data, kernel, cancellationToken)
