package ballerinaserialization

import (
	"encoding/json"

	ballerina "ballerina.com/core"
)

func OptionSerializer[T any](serializer Serializer[T]) Serializer[ballerina.Option[T]] {
	return withContext("on option", func(value ballerina.Option[T]) ballerina.Sum[error, json.RawMessage] {
		return SumSerializer(UnitSerializer, serializer)(value.Sum)
	})
}

func OptionDeserializer[T any](deserializer Deserializer[T]) Deserializer[ballerina.Option[T]] {
	sumDeserializer := SumDeserializer(UnitDeserializer, deserializer)
	return withContext(
		"on option",
		func(data json.RawMessage) ballerina.Sum[error, ballerina.Option[T]] {
			return ballerina.MapRight(
				sumDeserializer(data),
				func(sum ballerina.Sum[ballerina.Unit, T]) ballerina.Option[T] {
					return ballerina.Option[T]{Sum: sum}
				})
		},
	)
}
