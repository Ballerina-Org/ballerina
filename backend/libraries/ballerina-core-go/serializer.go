package ballerina

import (
	"encoding/json"
	"fmt"
)

// The Serializer function should be total (i.e. never return an error).
// However, we use the json.Marshal to serialize the value under the hood (mainly because of the string serialization in json),
// which can return an error. In theory, we could wrap json.Marshal and panic on error. For that to be safe,
// we would have to prove that it can never happen (which we believe is true, but did not formally prove).
// Thus, this partial signature.

type Serializer[T any] func(T) Sum[error, json.RawMessage]

type Deserializer[T any] func(json.RawMessage) Sum[error, T]

func wrappedMarshal[T any](value T) Sum[error, json.RawMessage] {
	serializedValue, err := json.Marshal(value)
	if err != nil {
		return Left[error, json.RawMessage](err)
	}
	return Right[error, json.RawMessage](serializedValue)
}

func wrappedUnmarshal[T any](data json.RawMessage) Sum[error, T] {
	var value T
	err := json.Unmarshal(data, &value)
	if err != nil {
		return Left[error, T](err)
	}
	return Right[error, T](value)
}

func withContext[I any, T any](context string, f func(I) Sum[error, T]) func(I) Sum[error, T] {
	return func(value I) Sum[error, T] {
		return MapLeft[error, T](f(value), func(err error) error {
			return fmt.Errorf("%s: %w", context, err)
		})
	}
}

type _unitForSerialization struct {
	Kind string `json:"kind"`
}

func UnitSerializer() Serializer[Unit] {
	return withContext("on unit", func(value Unit) Sum[error, json.RawMessage] {
		return wrappedMarshal(_unitForSerialization{Kind: "unit"})
	})
}

func UnitDeserializer() Deserializer[Unit] {
	return withContext("on unit", func(data json.RawMessage) Sum[error, Unit] {
		return Bind(wrappedUnmarshal[_unitForSerialization](data),
			func(unitForSerialization _unitForSerialization) Sum[error, Unit] {
				if unitForSerialization.Kind != "unit" {
					return Left[error, Unit](fmt.Errorf("expected kind to be 'unit', got %s", unitForSerialization.Kind))
				}
				return Right[error, Unit](Unit{})
			},
		)
	})
}

type _sumForSerialization struct {
	Case  string          `json:"case"`
	Value json.RawMessage `json:"value"`
}

func SumSerializer[L any, R any](leftSerializer Serializer[L], rightSerializer Serializer[R]) Serializer[Sum[L, R]] {
	return withContext("on sum", func(value Sum[L, R]) Sum[error, json.RawMessage] {
		return Bind(Fold(value,
			func(left L) Sum[error, _sumForSerialization] {
				return MapRight(leftSerializer(left), func(value json.RawMessage) _sumForSerialization {
					return _sumForSerialization{Case: "Sum.Left", Value: value}
				})
			},
			func(right R) Sum[error, _sumForSerialization] {
				return MapRight(rightSerializer(right), func(value json.RawMessage) _sumForSerialization {
					return _sumForSerialization{Case: "Sum.Right", Value: value}
				})
			},
		), wrappedMarshal)
	})
}

func SumDeserializer[L any, R any](leftDeserializer Deserializer[L], rightDeserializer Deserializer[R]) Deserializer[Sum[L, R]] {
	return withContext("on sum", func(data json.RawMessage) Sum[error, Sum[L, R]] {
		return Bind(wrappedUnmarshal[_sumForSerialization](data),
			func(sumForSerialization _sumForSerialization) Sum[error, Sum[L, R]] {
				switch sumForSerialization.Case {
				case "Sum.Left":
					return MapRight(leftDeserializer(sumForSerialization.Value), Left[L, R])
				case "Sum.Right":
					return MapRight(rightDeserializer(sumForSerialization.Value), Right[L, R])
				}
				return Left[error, Sum[L, R]](fmt.Errorf("expected case to be 'Sum.Left' or 'Sum.Right', got %s", sumForSerialization.Case))
			},
		)
	})
}
