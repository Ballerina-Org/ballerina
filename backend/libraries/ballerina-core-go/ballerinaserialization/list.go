package ballerinaserialization

import (
	"encoding/json"
	"fmt"

	ballerina "ballerina.com/core"
)

const listDiscriminator = "list"

func ListSerializer[T any](serializer Serializer[T]) Serializer[ballerina.Array[T]] {
	return WithContext(fmt.Sprintf("on %s", listDiscriminator), func(elements ballerina.Array[T]) ballerina.Sum[error, json.RawMessage] {
		return ballerina.Bind(ballerina.SumAll(ballerina.MapArray(elements, serializer)),
			func(serializedElements []json.RawMessage) ballerina.Sum[error, json.RawMessage] {
				return WrappedMarshal(_sequentialForSerialization{
					Discriminator: listDiscriminator,
					Value:         serializedElements,
				})
			})
	})
}

func ListDeserializer[T any](deserializer Deserializer[T]) Deserializer[ballerina.Array[T]] {
	return unmarshalWithContext(fmt.Sprintf("on %s", listDiscriminator), func(sequentialForSerialization _sequentialForSerialization) ballerina.Sum[error, ballerina.Array[T]] {
		return ballerina.Bind(sequentialForSerialization.getElementsWithDiscriminator(listDiscriminator),
			func(elements []json.RawMessage) ballerina.Sum[error, ballerina.Array[T]] {
				return ballerina.MapRight(
					ballerina.SumAll(ballerina.MapArray(elements, deserializer)),
					func(elements []T) ballerina.Array[T] {
						return ballerina.Array[T](elements)
					},
				)
			})
	},
	)
}
