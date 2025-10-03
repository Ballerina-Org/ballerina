package ballerinaserialization

import (
	"encoding/json"
	"fmt"

	ballerina "ballerina.com/core"
)

const (
	sumDiscriminator = "sum"
)

func SumSerializer[L any, R any](leftSerializer Serializer[L], rightSerializer Serializer[R]) Serializer[ballerina.Sum[L, R]] {
	return withContext("on sum", func(value ballerina.Sum[L, R]) ballerina.Sum[error, json.RawMessage] {
		return ballerina.Bind(ballerina.Fold(value,
			func(left L) ballerina.Sum[error, _sequentialForSerialization] {
				return ballerina.MapRight(withContext("on case 1/2", leftSerializer)(left), func(value json.RawMessage) _sequentialForSerialization {
					return _sequentialForSerialization{Discriminator: sumDiscriminator, Value: []json.RawMessage{json.RawMessage("0"), value}}
				})
			},
			func(right R) ballerina.Sum[error, _sequentialForSerialization] {
				return ballerina.MapRight(withContext("on case 2/2", rightSerializer)(right), func(value json.RawMessage) _sequentialForSerialization {
					return _sequentialForSerialization{Discriminator: sumDiscriminator, Value: []json.RawMessage{json.RawMessage("1"), value}}
				})
			},
		), wrappedMarshal)
	})
}

func SumDeserializer[L any, R any](leftDeserializer Deserializer[L], rightDeserializer Deserializer[R]) Deserializer[ballerina.Sum[L, R]] {
	return unmarshalWithContext(
		"on sum",
		func(sumForSerialization _sequentialForSerialization) ballerina.Sum[error, ballerina.Sum[L, R]] {
			if sumForSerialization.Discriminator != sumDiscriminator {
				return ballerina.Left[error, ballerina.Sum[L, R]](fmt.Errorf("expected discriminator to be '%s', got '%s'", sumDiscriminator, sumForSerialization.Discriminator))
			}
			if len(sumForSerialization.Value) != 2 {
				return ballerina.Left[error, ballerina.Sum[L, R]](fmt.Errorf("expected 2 elements in sum, got %d", len(sumForSerialization.Value)))
			}
			serializedIndex := sumForSerialization.Value[0]

			var index int
			err := json.Unmarshal(serializedIndex, &index)
			if err != nil {
				return ballerina.Left[error, ballerina.Sum[L, R]](fmt.Errorf("expected index to be a number, got %s", serializedIndex))
			}

			secondElement := sumForSerialization.Value[1]

			switch index {
			case 0:
				return ballerina.MapRight(withContext("on left", leftDeserializer)(secondElement), ballerina.Left[L, R])
			case 1:
				return ballerina.MapRight(withContext("on right", rightDeserializer)(secondElement), ballerina.Right[L, R])
			}
			return ballerina.Left[error, ballerina.Sum[L, R]](fmt.Errorf("expected index to be 0 or 1, got %d", index))
		},
	)
}
