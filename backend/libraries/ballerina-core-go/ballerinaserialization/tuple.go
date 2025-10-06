package ballerinaserialization

import (
	"encoding/json"
	"fmt"

	ballerina "ballerina.com/core"
)

const (
	tupleDiscriminator = "tuple"
)

func Tuple2Serializer[A any, B any](serializerA Serializer[A], serializerB Serializer[B]) Serializer[ballerina.Tuple2[A, B]] {
	return withContext("on tuple2", func(value ballerina.Tuple2[A, B]) ballerina.Sum[error, json.RawMessage] {
		return ballerina.Bind(withContext("on item1", serializerA)(value.Item1), func(item1 json.RawMessage) ballerina.Sum[error, json.RawMessage] {
			return ballerina.Bind(withContext("on item2", serializerB)(value.Item2), func(item2 json.RawMessage) ballerina.Sum[error, json.RawMessage] {
				return wrappedMarshal(_sequentialForSerialization{
					Discriminator: tupleDiscriminator,
					Value:         []json.RawMessage{item1, item2},
				})
			})
		})
	})
}

func Tuple2Deserializer[A any, B any](deserializerA Deserializer[A], deserializerB Deserializer[B]) Deserializer[ballerina.Tuple2[A, B]] {
	return unmarshalWithContext("on tuple2", func(sequentialForSerialization _sequentialForSerialization) ballerina.Sum[error, ballerina.Tuple2[A, B]] {
		return ballerina.Bind(sequentialForSerialization.getElementsWithDiscriminator(tupleDiscriminator),
			func(elements []json.RawMessage) ballerina.Sum[error, ballerina.Tuple2[A, B]] {
				if len(elements) != 2 {
					return ballerina.Left[error, ballerina.Tuple2[A, B]](fmt.Errorf("expected 2 elements in tuple, got %d", len(elements)))
				}
				return ballerina.Bind(withContext("on item1", deserializerA)(elements[0]), func(item1 A) ballerina.Sum[error, ballerina.Tuple2[A, B]] {
					return ballerina.MapRight(withContext("on item2", deserializerB)(elements[1]), func(item2 B) ballerina.Tuple2[A, B] {
						return ballerina.Tuple2[A, B]{Item1: item1, Item2: item2}
					})
				})
			})
	},
	)
}
