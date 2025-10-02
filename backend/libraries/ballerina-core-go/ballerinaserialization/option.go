package ballerinaserialization

import (
	"encoding/json"
	"fmt"

	ballerina "ballerina.com/core"
)

func OptionSerializer[T any](serializer Serializer[T]) Serializer[ballerina.Option[T]] {
	return withContext("on option", func(value ballerina.Option[T]) ballerina.Sum[error, json.RawMessage] {
		return ballerina.Bind(ballerina.Fold(value.Sum,
			func(left ballerina.Unit) ballerina.Sum[error, _sumForSerialization] {
				return ballerina.MapRight(withContext("on none", UnitSerializer)(left), func(value json.RawMessage) _sumForSerialization {
					return _sumForSerialization{Case: "none", Value: value}
				})
			},
			func(right T) ballerina.Sum[error, _sumForSerialization] {
				return ballerina.MapRight(withContext("on some", serializer)(right), func(value json.RawMessage) _sumForSerialization {
					return _sumForSerialization{Case: "some", Value: value}
				})
			},
		), wrappedMarshal)
	})
}

func OptionDeserializer[T any](deserializer Deserializer[T]) Deserializer[ballerina.Option[T]] {
	return unmarshalWithContext("on option", func(sumForSerialization _sumForSerialization) ballerina.Sum[error, ballerina.Option[T]] {
		switch sumForSerialization.Case {
		case "none":
			return ballerina.MapRight(withContext("on none", UnitDeserializer)(sumForSerialization.Value), func(unit ballerina.Unit) ballerina.Option[T] {
				return ballerina.None[T]()
			})
		case "some":
			return ballerina.MapRight(withContext("on some", deserializer)(sumForSerialization.Value), ballerina.Some[T])
		}
		return ballerina.Left[error, ballerina.Option[T]](fmt.Errorf("expected case to be 'none' or 'some', got %s", sumForSerialization.Case))
	},
	)
}
