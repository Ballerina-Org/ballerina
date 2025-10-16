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

func Sum3Serializer[A any, B any, C any](aSerializer Serializer[A], bSerializer Serializer[B], cSerializer Serializer[C]) Serializer[ballerina.Sum3[A, B, C]] {
	return WithContext("on sum3", func(value ballerina.Sum3[A, B, C]) ballerina.Sum[error, json.RawMessage] {
		return ballerina.Bind(
			ballerina.GoErrorToSum(ballerina.FoldSum3(
				func(v1 A) (_sequentialForSerialization, error) {
					return ballerina.Fold(
						WithContext("on case 1/3", aSerializer)(v1),
						func(err error) _sequentialForSerialization { return _sequentialForSerialization{} },
						func(payload json.RawMessage) _sequentialForSerialization {
							return _sequentialForSerialization{Discriminator: sumDiscriminator, Value: []json.RawMessage{json.RawMessage("1"), json.RawMessage("3"), payload}}
						},
					), nil
				},
				func(v2 B) (_sequentialForSerialization, error) {
					return ballerina.Fold(
						WithContext("on case 2/3", bSerializer)(v2),
						func(err error) _sequentialForSerialization { return _sequentialForSerialization{} },
						func(payload json.RawMessage) _sequentialForSerialization {
							return _sequentialForSerialization{Discriminator: sumDiscriminator, Value: []json.RawMessage{json.RawMessage("2"), json.RawMessage("3"), payload}}
						},
					), nil
				},
				func(v3 C) (_sequentialForSerialization, error) {
					return ballerina.Fold(
						WithContext("on case 3/3", cSerializer)(v3),
						func(err error) _sequentialForSerialization { return _sequentialForSerialization{} },
						func(payload json.RawMessage) _sequentialForSerialization {
							return _sequentialForSerialization{Discriminator: sumDiscriminator, Value: []json.RawMessage{json.RawMessage("3"), json.RawMessage("3"), payload}}
						},
					), nil
				},
			))(value),
			WrappedMarshal,
		)
	})
}

func Sum3Deserializer[A any, B any, C any](deserializer1 Deserializer[A], deserializer2 Deserializer[B], deserializer3 Deserializer[C]) Deserializer[ballerina.Sum3[A, B, C]] {
	return unmarshalWithContext(
		"on sum3",
		func(sumForSerialization _sequentialForSerialization) ballerina.Sum[error, ballerina.Sum3[A, B, C]] {
			return ballerina.Bind(parseSumHeader(sumForSerialization), func(header sumHeader) ballerina.Sum[error, ballerina.Sum3[A, B, C]] {
				if header.arity != 3 {
					return ballerina.Left[error, ballerina.Sum3[A, B, C]](fmt.Errorf("expected arity to be 3, got %d", header.arity))
				}
				switch header.index {
				case 1:
					return ballerina.MapRight(WithContext("on case 1/3", deserializer1)(header.payload), ballerina.Case1Of3[A, B, C])
				case 2:
					return ballerina.MapRight(WithContext("on case 2/3", deserializer2)(header.payload), ballerina.Case2Of3[A, B, C])
				case 3:
					return ballerina.MapRight(WithContext("on case 3/3", deserializer3)(header.payload), ballerina.Case3Of3[A, B, C])
				}
				return ballerina.Left[error, ballerina.Sum3[A, B, C]](fmt.Errorf("expected index to be 1, 2 or 3, got %d", header.index))
			})
		},
	)
}

func Sum4Serializer[A any, B any, C any, D any](aSerializer Serializer[A], bSerializer Serializer[B], cSerializer Serializer[C], dSerializer Serializer[D]) Serializer[ballerina.Sum4[A, B, C, D]] {
	return WithContext("on sum4", func(value ballerina.Sum4[A, B, C, D]) ballerina.Sum[error, json.RawMessage] {
		return ballerina.Bind(
			ballerina.GoErrorToSum(ballerina.FoldSum4(
				func(v1 A) (_sequentialForSerialization, error) {
					return ballerina.Fold(
						WithContext("on case 1/4", aSerializer)(v1),
						func(err error) _sequentialForSerialization { return _sequentialForSerialization{} },
						func(payload json.RawMessage) _sequentialForSerialization {
							return _sequentialForSerialization{Discriminator: sumDiscriminator, Value: []json.RawMessage{json.RawMessage("1"), json.RawMessage("4"), payload}}
						},
					), nil
				},
				func(v2 B) (_sequentialForSerialization, error) {
					return ballerina.Fold(
						WithContext("on case 2/4", bSerializer)(v2),
						func(err error) _sequentialForSerialization { return _sequentialForSerialization{} },
						func(payload json.RawMessage) _sequentialForSerialization {
							return _sequentialForSerialization{Discriminator: sumDiscriminator, Value: []json.RawMessage{json.RawMessage("2"), json.RawMessage("4"), payload}}
						},
					), nil
				},
				func(v3 C) (_sequentialForSerialization, error) {
					return ballerina.Fold(
						WithContext("on case 3/4", cSerializer)(v3),
						func(err error) _sequentialForSerialization { return _sequentialForSerialization{} },
						func(payload json.RawMessage) _sequentialForSerialization {
							return _sequentialForSerialization{Discriminator: sumDiscriminator, Value: []json.RawMessage{json.RawMessage("3"), json.RawMessage("4"), payload}}
						},
					), nil
				},
				func(v4 D) (_sequentialForSerialization, error) {
					return ballerina.Fold(
						WithContext("on case 4/4", dSerializer)(v4),
						func(err error) _sequentialForSerialization { return _sequentialForSerialization{} },
						func(payload json.RawMessage) _sequentialForSerialization {
							return _sequentialForSerialization{Discriminator: sumDiscriminator, Value: []json.RawMessage{json.RawMessage("4"), json.RawMessage("4"), payload}}
						},
					), nil
				},
			))(value),
			WrappedMarshal,
		)
	})
}

func Sum4Deserializer[A any, B any, C any, D any](aDeserializer Deserializer[A], bDeserializer Deserializer[B], cDeserializer Deserializer[C], dDeserializer Deserializer[D]) Deserializer[ballerina.Sum4[A, B, C, D]] {
	return unmarshalWithContext(
		"on sum4",
		func(sumForSerialization _sequentialForSerialization) ballerina.Sum[error, ballerina.Sum4[A, B, C, D]] {
			return ballerina.Bind(parseSumHeader(sumForSerialization), func(header sumHeader) ballerina.Sum[error, ballerina.Sum4[A, B, C, D]] {
				if header.arity != 4 {
					return ballerina.Left[error, ballerina.Sum4[A, B, C, D]](fmt.Errorf("expected arity to be 4, got %d", header.arity))
				}
				switch header.index {
				case 1:
					return ballerina.MapRight(WithContext("on case 1/4", aDeserializer)(header.payload), ballerina.Case1Of4[A, B, C, D])
				case 2:
					return ballerina.MapRight(WithContext("on case 2/4", bDeserializer)(header.payload), ballerina.Case2Of4[A, B, C, D])
				case 3:
					return ballerina.MapRight(WithContext("on case 3/4", cDeserializer)(header.payload), ballerina.Case3Of4[A, B, C, D])
				case 4:
					return ballerina.MapRight(WithContext("on case 4/4", dDeserializer)(header.payload), ballerina.Case4Of4[A, B, C, D])
				}
				return ballerina.Left[error, ballerina.Sum4[A, B, C, D]](fmt.Errorf("expected index to be 1, 2, 3 or 4, got %d", header.index))
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
		return ballerina.Left[error, sumHeader](fmt.Errorf("expected 3 elements in sum, got %d", len(sumForSerialization.Value)))
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
