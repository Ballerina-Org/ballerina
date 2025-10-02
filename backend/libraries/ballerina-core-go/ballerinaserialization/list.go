package ballerinaserialization

import (
	"encoding/json"

	ballerina "ballerina.com/core"
)

func ListSerializer[T any](serializer Serializer[T]) Serializer[[]T] {
	return withContext("on list", func(elements []T) ballerina.Sum[error, json.RawMessage] {
		return ballerina.Bind(ballerina.SumAll(ballerina.MapArray(elements, serializer)),
			func(serializedElements []json.RawMessage) ballerina.Sum[error, json.RawMessage] {
				return wrappedMarshal(_sequentialForSerialization{
					Kind:     "list",
					Elements: serializedElements,
				})
			})
	})
}

func ListDeserializer[T any](deserializer Deserializer[T]) Deserializer[[]T] {
	return unmarshalWithContext("on list", func(sequentialForSerialization _sequentialForSerialization) ballerina.Sum[error, []T] {
		return ballerina.Bind(sequentialForSerialization.getElementsWithKind("list"),
			func(elements []json.RawMessage) ballerina.Sum[error, []T] {
				return ballerina.SumAll(ballerina.MapArray(elements, deserializer))
			})
	},
	)
}
