package ballerina

import (
	"bytes"
	"encoding/json"
	"fmt"
	"strconv"
	"time"
)

// The Serializer function should be total (i.e. never return an error).
// However, we use the json.Marshal to serialize the value under the hood (mainly because of the string serialization in json),
// which can return an error. In theory, we could wrap json.Marshal and panic on error. For that to be safe,
// we would have to prove that we never encounter an error when we serialize. Since we did not prove this for
// the current implementation -- and it is hard to enforce this invariant if we extend the serialization.
// Thus, this partial signature.

type Serializer[T any] func(T) Sum[error, json.RawMessage]

type Deserializer[T any] func(json.RawMessage) Sum[error, T]

func wrappedMarshal[T any](value T) Sum[error, json.RawMessage] {
	return GoErrorToSum(func(value T) (json.RawMessage, error) {
		return json.Marshal(value)
	})(value)
}

func wrappedUnmarshal[T any](data json.RawMessage) Sum[error, T] {
	return GoErrorToSum(func(data json.RawMessage) (T, error) {
		var value T
		decoder := json.NewDecoder(bytes.NewReader(data))
		decoder.DisallowUnknownFields()
		err := decoder.Decode(&value)
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

func unmarshalWithContext[T any, U any](context string, f func(T) Sum[error, U]) func(json.RawMessage) Sum[error, U] {
	return withContext(context, func(data json.RawMessage) Sum[error, U] {
		return Bind(wrappedUnmarshal[T](data), f)
	})
}

type _unitForSerialization struct {
	Kind string `json:"kind"`
}

var UnitSerializer Serializer[Unit] = withContext("on unit", func(value Unit) Sum[error, json.RawMessage] {
	return wrappedMarshal(_unitForSerialization{Kind: "unit"})
})

var UnitDeserializer Deserializer[Unit] = unmarshalWithContext("on unit", func(unitForSerialization _unitForSerialization) Sum[error, Unit] {
	if unitForSerialization.Kind != "unit" {
		return Left[error, Unit](fmt.Errorf("expected kind to be 'unit', got '%s'", unitForSerialization.Kind))
	}
	return Right[error, Unit](Unit{})
})

type _sumForSerialization struct {
	Case  string          `json:"case"`
	Value json.RawMessage `json:"value"`
}

func SumSerializer[L any, R any](leftSerializer Serializer[L], rightSerializer Serializer[R]) Serializer[Sum[L, R]] {
	return withContext("on sum", func(value Sum[L, R]) Sum[error, json.RawMessage] {
		return Bind(Fold(value,
			func(left L) Sum[error, _sumForSerialization] {
				return MapRight(withContext("on left", leftSerializer)(left), func(value json.RawMessage) _sumForSerialization {
					return _sumForSerialization{Case: "Sum.Left", Value: value}
				})
			},
			func(right R) Sum[error, _sumForSerialization] {
				return MapRight(withContext("on right", rightSerializer)(right), func(value json.RawMessage) _sumForSerialization {
					return _sumForSerialization{Case: "Sum.Right", Value: value}
				})
			},
		), wrappedMarshal)
	})
}

func SumDeserializer[L any, R any](leftDeserializer Deserializer[L], rightDeserializer Deserializer[R]) Deserializer[Sum[L, R]] {
	return unmarshalWithContext("on sum", func(sumForSerialization _sumForSerialization) Sum[error, Sum[L, R]] {
		switch sumForSerialization.Case {
		case "Sum.Left":
			return MapRight(withContext("on left", leftDeserializer)(sumForSerialization.Value), Left[L, R])
		case "Sum.Right":
			return MapRight(withContext("on right", rightDeserializer)(sumForSerialization.Value), Right[L, R])
		}
		return Left[error, Sum[L, R]](fmt.Errorf("expected case to be 'Sum.Left' or 'Sum.Right', got %s", sumForSerialization.Case))
	},
	)
}

func OptionSerializer[T any](serializer Serializer[T]) Serializer[Option[T]] {
	return withContext("on option", func(value Option[T]) Sum[error, json.RawMessage] {
		return Bind(Fold(value.Sum,
			func(left Unit) Sum[error, _sumForSerialization] {
				return MapRight(withContext("on none", UnitSerializer)(left), func(value json.RawMessage) _sumForSerialization {
					return _sumForSerialization{Case: "none", Value: value}
				})
			},
			func(right T) Sum[error, _sumForSerialization] {
				return MapRight(withContext("on some", serializer)(right), func(value json.RawMessage) _sumForSerialization {
					return _sumForSerialization{Case: "some", Value: value}
				})
			},
		), wrappedMarshal)
	})
}

func OptionDeserializer[T any](deserializer Deserializer[T]) Deserializer[Option[T]] {
	return unmarshalWithContext("on option", func(sumForSerialization _sumForSerialization) Sum[error, Option[T]] {
		switch sumForSerialization.Case {
		case "none":
			return MapRight(withContext("on none", UnitDeserializer)(sumForSerialization.Value), func(unit Unit) Option[T] {
				return None[T]()
			})
		case "some":
			return MapRight(withContext("on some", deserializer)(sumForSerialization.Value), Some[T])
		}
		return Left[error, Option[T]](fmt.Errorf("expected case to be 'none' or 'some', got %s", sumForSerialization.Case))
	},
	)
}

type _sequentialForSerialization struct {
	Kind     string            `json:"kind"`
	Elements []json.RawMessage `json:"elements"`
}

func (s _sequentialForSerialization) getElementsWithKind(kind string) Sum[error, []json.RawMessage] {
	if s.Kind != kind {
		return Left[error, []json.RawMessage](fmt.Errorf("expected kind to be '%s', got %s", kind, s.Kind))
	}
	return Right[error, []json.RawMessage](s.Elements)
}

func Tuple2Serializer[A any, B any](serializerA Serializer[A], serializerB Serializer[B]) Serializer[Tuple2[A, B]] {
	return withContext("on tuple2", func(value Tuple2[A, B]) Sum[error, json.RawMessage] {
		return Bind(withContext("on item1", serializerA)(value.Item1), func(item1 json.RawMessage) Sum[error, json.RawMessage] {
			return Bind(withContext("on item2", serializerB)(value.Item2), func(item2 json.RawMessage) Sum[error, json.RawMessage] {
				return wrappedMarshal(_sequentialForSerialization{
					Kind:     "tuple",
					Elements: []json.RawMessage{item1, item2},
				})
			})
		})
	})
}

func Tuple2Deserializer[A any, B any](deserializerA Deserializer[A], deserializerB Deserializer[B]) Deserializer[Tuple2[A, B]] {
	return unmarshalWithContext("on tuple2", func(sequentialForSerialization _sequentialForSerialization) Sum[error, Tuple2[A, B]] {
		return Bind(sequentialForSerialization.getElementsWithKind("tuple"),
			func(elements []json.RawMessage) Sum[error, Tuple2[A, B]] {
				if len(elements) != 2 {
					return Left[error, Tuple2[A, B]](fmt.Errorf("expected 2 elements in tuple, got %d", len(elements)))
				}
				return Bind(withContext("on item1", deserializerA)(elements[0]), func(item1 A) Sum[error, Tuple2[A, B]] {
					return MapRight(withContext("on item2", deserializerB)(elements[1]), func(item2 B) Tuple2[A, B] {
						return Tuple2[A, B]{Item1: item1, Item2: item2}
					})
				})
			})
	},
	)
}

func ListSerializer[T any](serializer Serializer[T]) Serializer[[]T] {
	return withContext("on list", func(elements []T) Sum[error, json.RawMessage] {
		return Bind(SumAll(MapArray(elements, serializer)),
			func(serializedElements []json.RawMessage) Sum[error, json.RawMessage] {
				return wrappedMarshal(_sequentialForSerialization{
					Kind:     "list",
					Elements: serializedElements,
				})
			})
	})
}

func ListDeserializer[T any](deserializer Deserializer[T]) Deserializer[[]T] {
	return unmarshalWithContext("on list", func(sequentialForSerialization _sequentialForSerialization) Sum[error, []T] {
		return Bind(sequentialForSerialization.getElementsWithKind("list"),
			func(elements []json.RawMessage) Sum[error, []T] {
				return SumAll(MapArray(elements, deserializer))
			})
	},
	)
}

var StringSerializer Serializer[string] = withContext("on string", wrappedMarshal[string])

var StringDeserializer Deserializer[string] = withContext("on string", wrappedUnmarshal[string])

var BoolSerializer Serializer[bool] = withContext("on bool", wrappedMarshal[bool])

var BoolDeserializer Deserializer[bool] = withContext("on bool", wrappedUnmarshal[bool])

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

func serializePrimitiveTypeFrom[T any](kind string, serialize func(T) string) func(T) Sum[error, json.RawMessage] {
	return withContext("on "+kind, func(value T) Sum[error, json.RawMessage] {
		return wrappedMarshal(_primitiveTypeForSerialization{Kind: kind, Value: serialize(value)})
	})
}

func deserializePrimitiveTypeTo[T any](kind string, parse func(string) (T, error)) func(json.RawMessage) Sum[error, T] {
	return unmarshalWithContext("on "+kind, func(primitiveTypeForSerialization _primitiveTypeForSerialization) Sum[error, T] {
		return Bind(primitiveTypeForSerialization.getValueWithKind(kind), GoErrorToSum(parse))
	})
}

var IntSerializer Serializer[int64] = serializePrimitiveTypeFrom("int", func(value int64) string {
	return strconv.FormatInt(value, 10)
})

var IntDeserializer Deserializer[int64] = deserializePrimitiveTypeTo("int", func(value string) (int64, error) {
	return strconv.ParseInt(value, 10, 64)
})

var FloatSerializer Serializer[float64] = serializePrimitiveTypeFrom("float", func(value float64) string {
	return strconv.FormatFloat(value, 'f', -1, 64)
})

var FloatDeserializer Deserializer[float64] = deserializePrimitiveTypeTo("float", func(value string) (float64, error) {
	return strconv.ParseFloat(value, 64)
})

var DateSerializer Serializer[time.Time] = serializePrimitiveTypeFrom("date", func(value time.Time) string {
	return value.Format(time.DateOnly)
})

var DateDeserializer Deserializer[time.Time] = deserializePrimitiveTypeTo("date", func(value string) (time.Time, error) {
	return time.Parse(time.DateOnly, value)
})
