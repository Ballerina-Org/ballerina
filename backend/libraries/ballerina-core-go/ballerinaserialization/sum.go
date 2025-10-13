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
	return WithContext("on sum", func(value ballerina.Sum[L, R]) ballerina.Sum[error, json.RawMessage] {
		return ballerina.Bind(ballerina.Fold(value,
			func(left L) ballerina.Sum[error, _sequentialForSerialization] {
				return ballerina.MapRight(WithContext("on case 1/2", leftSerializer)(left), func(value json.RawMessage) _sequentialForSerialization {
					return _sequentialForSerialization{Discriminator: sumDiscriminator, Value: []json.RawMessage{json.RawMessage("1"), json.RawMessage("2"), value}}
				})
			},
			func(right R) ballerina.Sum[error, _sequentialForSerialization] {
				return ballerina.MapRight(WithContext("on case 2/2", rightSerializer)(right), func(value json.RawMessage) _sequentialForSerialization {
					return _sequentialForSerialization{Discriminator: sumDiscriminator, Value: []json.RawMessage{json.RawMessage("2"), json.RawMessage("2"), value}}
				})
			},
		), WrappedMarshal)
	})
}

func SumDeserializer[L any, R any](leftDeserializer Deserializer[L], rightDeserializer Deserializer[R]) Deserializer[ballerina.Sum[L, R]] {
	return unmarshalWithContext(
		"on sum",
		func(sumForSerialization _sequentialForSerialization) ballerina.Sum[error, ballerina.Sum[L, R]] {
			return ballerina.Bind(parseSumHeader(sumForSerialization), func(header sumHeader) ballerina.Sum[error, ballerina.Sum[L, R]] {
				if header.arity != 2 {
					return ballerina.Left[error, ballerina.Sum[L, R]](fmt.Errorf("expected arity to be 2, got %d", header.arity))
				}
				switch header.index {
				case 1:
					return ballerina.MapRight(WithContext("on left", leftDeserializer)(header.payload), ballerina.Left[L, R])
				case 2:
					return ballerina.MapRight(WithContext("on right", rightDeserializer)(header.payload), ballerina.Right[L, R])
				}
				return ballerina.Left[error, ballerina.Sum[L, R]](fmt.Errorf("expected index to be 1 or 2, got %d", header.index))
			})
		},
	)
}

type sumHeader struct {
	index   int
	arity   int
	payload json.RawMessage
}

func parseSumHeader(sumForSerialization _sequentialForSerialization) ballerina.Sum[error, sumHeader] {
	if sumForSerialization.Discriminator != sumDiscriminator {
		return ballerina.Left[error, sumHeader](fmt.Errorf("expected discriminator to be '%s', got '%s'", sumDiscriminator, sumForSerialization.Discriminator))
	}
	if len(sumForSerialization.Value) != 3 {
		return ballerina.Left[error, sumHeader](fmt.Errorf("expected 2 elements in sum, got %d", len(sumForSerialization.Value)))
	}

	serializedIndex := sumForSerialization.Value[0]
	var index int
	if err := json.Unmarshal(serializedIndex, &index); err != nil {
		return ballerina.Left[error, sumHeader](fmt.Errorf("expected index to be a number, got %s", serializedIndex))
	}

	serializedArity := sumForSerialization.Value[1]
	var arity int
	if err := json.Unmarshal(serializedArity, &arity); err != nil {
		return ballerina.Left[error, sumHeader](fmt.Errorf("expected arity to be a number, got %s", serializedArity))
	}

	payload := sumForSerialization.Value[2]
	return ballerina.Right[error, sumHeader](sumHeader{index: index, arity: arity, payload: payload})
}
