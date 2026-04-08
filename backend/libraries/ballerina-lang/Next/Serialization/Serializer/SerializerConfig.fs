namespace Ballerina.DSL.Next.Serialization

module SerializerConfig =
  open System.Text.Json
  open System.Text.Json.Serialization

  let jsonSerializationConfiguration =
    let options =
      new JsonSerializerOptions(DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)

    options
