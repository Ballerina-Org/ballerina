package ballerinaserialization

import (
	"encoding/json"
	"fmt"

	ballerina "ballerina.com/core"
)

const listDiscriminator = "list"

func ListSerializer[T any](serializer Serializer[T]) Serializer[[]T] {
	return withContext(fmt.Sprintf("on %s", listDiscriminator), func(elements []T) ballerina.Sum[error, json.RawMessage] {
		return ballerina.Bind(ballerina.SumAll(ballerina.MapArray(elements, serializer)),
			func(serializedElements []json.RawMessage) ballerina.Sum[error, json.RawMessage] {
				return wrappedMarshal(_sequentialForSerialization{
					Kind:     listDiscriminator,
					Elements: serializedElements,
				})
			})
	})
}

func ListDeserializer[T any](deserializer Deserializer[T]) Deserializer[[]T] {
	return unmarshalWithContext(fmt.Sprintf("on %s", listDiscriminator), func(sequentialForSerialization _sequentialForSerialization) ballerina.Sum[error, []T] {
		return ballerina.Bind(sequentialForSerialization.getElementsWithKind(listDiscriminator),
			func(elements []json.RawMessage) ballerina.Sum[error, []T] {
				return ballerina.SumAll(ballerina.MapArray(elements, deserializer))
			})
	},
	)
}
