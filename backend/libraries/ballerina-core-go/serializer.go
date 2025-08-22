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
					return Left[error, Unit](fmt.Errorf("expected kind to be 'unit', got '%s'", unitForSerialization.Kind))
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
	return withContext("on sum", func(data json.RawMessage) Sum[error, Sum[L, R]] {
		return Bind(wrappedUnmarshal[_sumForSerialization](data),
			func(sumForSerialization _sumForSerialization) Sum[error, Sum[L, R]] {
				switch sumForSerialization.Case {
				case "Sum.Left":
					return MapRight(withContext("on left", leftDeserializer)(sumForSerialization.Value), Left[L, R])
				case "Sum.Right":
					return MapRight(withContext("on right", rightDeserializer)(sumForSerialization.Value), Right[L, R])
				}
				return Left[error, Sum[L, R]](fmt.Errorf("expected case to be 'Sum.Left' or 'Sum.Right', got %s", sumForSerialization.Case))
			},
		)
	})
}

func OptionSerializer[T any](serializer Serializer[T]) Serializer[Option[T]] {
	return withContext("on option", func(value Option[T]) Sum[error, json.RawMessage] {
		return Bind(Fold(value.Sum,
			func(left Unit) Sum[error, _sumForSerialization] {
				return MapRight(withContext("on none", UnitSerializer())(left), func(value json.RawMessage) _sumForSerialization {
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
	return withContext("on option", func(data json.RawMessage) Sum[error, Option[T]] {
		return Bind(wrappedUnmarshal[_sumForSerialization](data),
			func(sumForSerialization _sumForSerialization) Sum[error, Option[T]] {
				switch sumForSerialization.Case {
				case "none":
					return MapRight(withContext("on none", UnitDeserializer())(sumForSerialization.Value), func(unit Unit) Option[T] {
						return None[T]()
					})
				case "some":
					return MapRight(withContext("on some", deserializer)(sumForSerialization.Value), Some[T])
				}
				return Left[error, Option[T]](fmt.Errorf("expected case to be 'none' or 'some', got %s", sumForSerialization.Case))
			},
		)
	})
}

type _tuple2ForSerialization struct {
	Kind     string            `json:"kind"`
	Elements []json.RawMessage `json:"elements"`
}

func Tuple2Serializer[A any, B any](serializerA Serializer[A], serializerB Serializer[B]) Serializer[Tuple2[A, B]] {
	return withContext("on tuple2", func(value Tuple2[A, B]) Sum[error, json.RawMessage] {
		return Bind(withContext("on item1", serializerA)(value.Item1), func(item1 json.RawMessage) Sum[error, json.RawMessage] {
			return Bind(withContext("on item2", serializerB)(value.Item2), func(item2 json.RawMessage) Sum[error, json.RawMessage] {
				return wrappedMarshal(_tuple2ForSerialization{
					Kind:     "tuple",
					Elements: []json.RawMessage{item1, item2},
				})
			})
		})
	})
}

func Tuple2Deserializer[A any, B any](deserializerA Deserializer[A], deserializerB Deserializer[B]) Deserializer[Tuple2[A, B]] {
	return withContext("on tuple2", func(data json.RawMessage) Sum[error, Tuple2[A, B]] {
		return Bind(wrappedUnmarshal[_tuple2ForSerialization](data),
			func(tuple2ForSerialization _tuple2ForSerialization) Sum[error, Tuple2[A, B]] {
				if len(tuple2ForSerialization.Elements) != 2 {
					return Left[error, Tuple2[A, B]](fmt.Errorf("expected 2 elements in tuple, got %d", len(tuple2ForSerialization.Elements)))
				}
				return Bind(withContext("on item1", deserializerA)(tuple2ForSerialization.Elements[0]), func(item1 A) Sum[error, Tuple2[A, B]] {
					return MapRight(withContext("on item2", deserializerB)(tuple2ForSerialization.Elements[1]), func(item2 B) Tuple2[A, B] {
						return Tuple2[A, B]{Item1: item1, Item2: item2}
					})
				})
			},
		)
	})
}

func ListSerializer[T any](serializer Serializer[T]) Serializer[[]T] {
	return withContext("on list", func(elements []T) Sum[error, json.RawMessage] {
		return Bind(SumSequence(ListFoldLeft(elements, []Sum[error, json.RawMessage]{},
			func(acc []Sum[error, json.RawMessage], value T) []Sum[error, json.RawMessage] {
				return append(acc, serializer(value))
			})),
			func(serializedElements []json.RawMessage) Sum[error, json.RawMessage] {
				return wrappedMarshal(_tuple2ForSerialization{
					Kind:     "list",
					Elements: serializedElements,
				})
			})
	})
}

func ListDeserializer[T any](deserializer Deserializer[T]) Deserializer[[]T] {
	return withContext("on list", func(data json.RawMessage) Sum[error, []T] {
		return Bind(wrappedUnmarshal[_tuple2ForSerialization](data),
			func(tuple2ForSerialization _tuple2ForSerialization) Sum[error, []T] {
				return Bind(SumSequence(ListFoldLeft(tuple2ForSerialization.Elements, []Sum[error, T]{},
					func(acc []Sum[error, T], value json.RawMessage) []Sum[error, T] {
						return append(acc, deserializer(value))
					}),
				),
					Right[error, []T],
				)
			})
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

func serializePrimitiveTypeFrom[T any](kind string, serialize func(T) string) func(T) Sum[error, json.RawMessage] {
	return withContext("on "+kind, func(value T) Sum[error, json.RawMessage] {
		return wrappedMarshal(_primitiveTypeForSerialization{Kind: kind, Value: serialize(value)})
	})
}

func deserializePrimitiveTypeTo[T any](kind string, parse func(string) (T, error)) func(json.RawMessage) Sum[error, T] {
	return withContext("on "+kind, func(data json.RawMessage) Sum[error, T] {
		return Bind(wrappedUnmarshal[_primitiveTypeForSerialization](data),
			func(primitiveTypeForSerialization _primitiveTypeForSerialization) Sum[error, T] {
				return Bind(primitiveTypeForSerialization.getValueWithKind(kind), SumWrap(parse))
			},
		)
	})
}

func IntSerializer() Serializer[int64] {
	return serializePrimitiveTypeFrom("int", func(value int64) string {
		return strconv.FormatInt(value, 10)
	})
}

func IntDeserializer() Deserializer[int64] {
	return deserializePrimitiveTypeTo("int", func(value string) (int64, error) {
		return strconv.ParseInt(value, 10, 64)
	})
}

func FloatSerializer() Serializer[float64] {
	return serializePrimitiveTypeFrom("float", func(value float64) string {
		return strconv.FormatFloat(value, 'f', -1, 64)
	})
}

func FloatDeserializer() Deserializer[float64] {
	return deserializePrimitiveTypeTo("float", func(value string) (float64, error) {
		return strconv.ParseFloat(value, 64)
	})
}

func DateSerializer() Serializer[time.Time] {
	return serializePrimitiveTypeFrom("date", func(value time.Time) string {
		return value.Format(time.DateOnly)
	})
}

func DateDeserializer() Deserializer[time.Time] {
	return deserializePrimitiveTypeTo("date", func(value string) (time.Time, error) {
		return time.Parse(time.DateOnly, value)
	})
}
