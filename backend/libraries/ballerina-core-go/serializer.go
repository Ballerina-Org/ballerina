package ballerina

import (
	"encoding/json"
	"fmt"
	"strconv"
	"time"
)

// The Serializer function should be total (i.e. never return an error).
// However, we use the json.Marshal to serialize the value under the hood (mainly because of the string serialization in json),
// which can return an error. In theory, we could wrap json.Marshal and panic on error. For that to be safe,
// we would have to prove that it can never happen (which we believe is true, but did not formally prove).
// Thus, this partial signature.

type Serializer[T any] func(T) Sum[error, json.RawMessage]

type Deserializer[T any] func(json.RawMessage) Sum[error, T]

func wrappedMarshal[T any](value T) Sum[error, json.RawMessage] {
	return SumWrap(func(value T) (json.RawMessage, error) {
		return json.Marshal(value)
	})(value)
}

func wrappedUnmarshal[T any](data json.RawMessage) Sum[error, T] {
	return SumWrap(func(data json.RawMessage) (T, error) {
		var value T
		err := json.Unmarshal(data, &value)
		return value, err
	})(data)
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

func StringSerializer() Serializer[string] {
	return withContext("on string", func(value string) Sum[error, json.RawMessage] {
		return wrappedMarshal(value)
	})
}

func StringDeserializer() Deserializer[string] {
	return withContext("on string", func(data json.RawMessage) Sum[error, string] {
		return wrappedUnmarshal[string](data)
	})
}

func BoolSerializer() Serializer[bool] {
	return withContext("on bool", func(value bool) Sum[error, json.RawMessage] {
		return wrappedMarshal(value)
	})
}

func BoolDeserializer() Deserializer[bool] {
	return withContext("on bool", func(data json.RawMessage) Sum[error, bool] {
		return wrappedUnmarshal[bool](data)
	})
}

type _primitiveTypeForSerialization struct {
	Kind  string `json:"kind"`
	Value string `json:"value"`
}

func (s _primitiveTypeForSerialization) getValueWithKind(kind string) Sum[error, string] {
	if s.Kind != kind {
		return Left[error, string](fmt.Errorf("expected kind to be '%s', got %s", kind, s.Kind))
	}
	return Right[error, string](s.Value)
}

func IntSerializer() Serializer[int64] {
	return withContext("on int", func(value int64) Sum[error, json.RawMessage] {
		return wrappedMarshal(_primitiveTypeForSerialization{Kind: "int", Value: strconv.FormatInt(value, 10)})
	})
}

func IntDeserializer() Deserializer[int64] {
	return withContext("on int", func(data json.RawMessage) Sum[error, int64] {
		return Bind(wrappedUnmarshal[_primitiveTypeForSerialization](data),
			func(primitiveTypeForSerialization _primitiveTypeForSerialization) Sum[error, int64] {
				return Bind(primitiveTypeForSerialization.getValueWithKind("int"), SumWrap(func(value string) (int64, error) {
					return strconv.ParseInt(value, 10, 64)
				}))
			},
		)
	})
}

func FloatSerializer() Serializer[float64] {
	return withContext("on float", func(value float64) Sum[error, json.RawMessage] {
		return wrappedMarshal(_primitiveTypeForSerialization{Kind: "float", Value: strconv.FormatFloat(value, 'f', -1, 64)})
	})
}

func FloatDeserializer() Deserializer[float64] {
	return withContext("on float", func(data json.RawMessage) Sum[error, float64] {
		return Bind(wrappedUnmarshal[_primitiveTypeForSerialization](data),
			func(primitiveTypeForSerialization _primitiveTypeForSerialization) Sum[error, float64] {
				return Bind(primitiveTypeForSerialization.getValueWithKind("float"), SumWrap(func(value string) (float64, error) {
					return strconv.ParseFloat(value, 64)
				}))
			},
		)
	})
}

func DateSerializer() Serializer[time.Time] {
	return withContext("on date", func(value time.Time) Sum[error, json.RawMessage] {
		return wrappedMarshal(_primitiveTypeForSerialization{Kind: "date", Value: value.Format(time.DateOnly)})
	})
}

func DateDeserializer() Deserializer[time.Time] {
	return withContext("on date", func(data json.RawMessage) Sum[error, time.Time] {
		return Bind(wrappedUnmarshal[_primitiveTypeForSerialization](data),
			func(primitiveTypeForSerialization _primitiveTypeForSerialization) Sum[error, time.Time] {
				return Bind(primitiveTypeForSerialization.getValueWithKind("date"), SumWrap(func(value string) (time.Time, error) {
					return time.Parse(time.DateOnly, value)
				}))
			},
		)
	})
}
