package ballerinaserialization

import (
	"encoding/json"
	"fmt"

	ballerina "ballerina.com/core"
)

type _sumForSerialization struct {
	Case  string          `json:"case"`
	Value json.RawMessage `json:"value"`
}

func SumSerializer[L any, R any](leftSerializer Serializer[L], rightSerializer Serializer[R]) Serializer[ballerina.Sum[L, R]] {
	return withContext("on sum", func(value ballerina.Sum[L, R]) ballerina.Sum[error, json.RawMessage] {
		return ballerina.Bind(ballerina.Fold(value,
			func(left L) ballerina.Sum[error, _sumForSerialization] {
				return ballerina.MapRight(withContext("on left", leftSerializer)(left), func(value json.RawMessage) _sumForSerialization {
					return _sumForSerialization{Case: "Sum.Left", Value: value}
				})
			},
			func(right R) ballerina.Sum[error, _sumForSerialization] {
				return ballerina.MapRight(withContext("on right", rightSerializer)(right), func(value json.RawMessage) _sumForSerialization {
					return _sumForSerialization{Case: "Sum.Right", Value: value}
				})
			},
		), wrappedMarshal)
	})
}

func SumDeserializer[L any, R any](leftDeserializer Deserializer[L], rightDeserializer Deserializer[R]) Deserializer[ballerina.Sum[L, R]] {
	return unmarshalWithContext("on sum", func(sumForSerialization _sumForSerialization) ballerina.Sum[error, ballerina.Sum[L, R]] {
		switch sumForSerialization.Case {
		case "Sum.Left":
			return ballerina.MapRight(withContext("on left", leftDeserializer)(sumForSerialization.Value), ballerina.Left[L, R])
		case "Sum.Right":
			return ballerina.MapRight(withContext("on right", rightDeserializer)(sumForSerialization.Value), ballerina.Right[L, R])
		}
		return ballerina.Left[error, ballerina.Sum[L, R]](fmt.Errorf("expected case to be 'Sum.Left' or 'Sum.Right', got %s", sumForSerialization.Case))
	},
	)
}
