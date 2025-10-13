package ballerinaserialization

import (
	"encoding/json"
	"fmt"

	ballerina "ballerina.com/core"
)

const (
	tupleDiscriminator = "tuple"
)

func Tuple2Serializer[A any, B any](serializerA Serializer[A], serializerB Serializer[B]) Serializer[ballerina.Tuple2[A, B]] {
	return WithContext(
		"on tuple2",
		func(value ballerina.Tuple2[A, B]) ballerina.Sum[error, json.RawMessage] {
			return ballerina.Bind(
				WithContext("on item1", serializerA)(value.Item1),
				func(item1 json.RawMessage) ballerina.Sum[error, json.RawMessage] {
					return ballerina.Bind(
						WithContext("on item2", serializerB)(value.Item2),
						func(item2 json.RawMessage) ballerina.Sum[error, json.RawMessage] {
							return WrappedMarshal(
								_sequentialForSerialization{
									Discriminator: tupleDiscriminator,
									Value:         []json.RawMessage{item1, item2},
								},
							)
						},
					)
				},
			)
		},
	)
}

func Tuple2Deserializer[A any, B any](deserializerA Deserializer[A], deserializerB Deserializer[B]) Deserializer[ballerina.Tuple2[A, B]] {
	return unmarshalWithContext(
		"on tuple2",
		func(sequentialForSerialization _sequentialForSerialization) ballerina.Sum[error, ballerina.Tuple2[A, B]] {
			return ballerina.Bind(
				sequentialForSerialization.getElementsWithDiscriminator(tupleDiscriminator),
				func(elements []json.RawMessage) ballerina.Sum[error, ballerina.Tuple2[A, B]] {
					if len(elements) != 2 {
						return ballerina.Left[error, ballerina.Tuple2[A, B]](fmt.Errorf("expected 2 elements in tuple, got %d", len(elements)))
					}
					return ballerina.Bind(
						WithContext("on item1", deserializerA)(elements[0]),
						func(item1 A) ballerina.Sum[error, ballerina.Tuple2[A, B]] {
							return ballerina.MapRight(
								WithContext("on item2", deserializerB)(elements[1]),
								func(item2 B) ballerina.Tuple2[A, B] {
									return ballerina.Tuple2[A, B]{Item1: item1, Item2: item2}
								},
							)
						},
					)
				},
			)
		},
	)
}

func Tuple3Serializer[A any, B any, C any](serializerA Serializer[A], serializerB Serializer[B], serializerC Serializer[C]) Serializer[ballerina.Tuple3[A, B, C]] {
	return WithContext(
		"on tuple3",
		func(value ballerina.Tuple3[A, B, C]) ballerina.Sum[error, json.RawMessage] {
			return ballerina.Bind(
				WithContext("on item1", serializerA)(value.Item1),
				func(item1 json.RawMessage) ballerina.Sum[error, json.RawMessage] {
					return ballerina.Bind(
						WithContext("on item2", serializerB)(value.Item2),
						func(item2 json.RawMessage) ballerina.Sum[error, json.RawMessage] {
							return ballerina.Bind(
								WithContext("on item3", serializerC)(value.Item3),
								func(item3 json.RawMessage) ballerina.Sum[error, json.RawMessage] {
									return WrappedMarshal(
										_sequentialForSerialization{
											Discriminator: tupleDiscriminator,
											Value:         []json.RawMessage{item1, item2, item3},
										},
									)
								},
							)
						},
					)
				},
			)
		},
	)
}

func Tuple3Deserializer[A any, B any, C any](deserializerA Deserializer[A], deserializerB Deserializer[B], deserializerC Deserializer[C]) Deserializer[ballerina.Tuple3[A, B, C]] {
	return unmarshalWithContext(
		"on tuple3",
		func(sequentialForSerialization _sequentialForSerialization) ballerina.Sum[error, ballerina.Tuple3[A, B, C]] {
			return ballerina.Bind(
				sequentialForSerialization.getElementsWithDiscriminator(tupleDiscriminator),
				func(elements []json.RawMessage) ballerina.Sum[error, ballerina.Tuple3[A, B, C]] {
					if len(elements) != 3 {
						return ballerina.Left[error, ballerina.Tuple3[A, B, C]](fmt.Errorf("expected 3 elements in tuple, got %d", len(elements)))
					}
					return ballerina.Bind(
						WithContext("on item1", deserializerA)(elements[0]),
						func(item1 A) ballerina.Sum[error, ballerina.Tuple3[A, B, C]] {
							return ballerina.Bind(
								WithContext("on item2", deserializerB)(elements[1]),
								func(item2 B) ballerina.Sum[error, ballerina.Tuple3[A, B, C]] {
									return ballerina.MapRight(
										WithContext("on item3", deserializerC)(elements[2]),
										func(item3 C) ballerina.Tuple3[A, B, C] {
											return ballerina.Tuple3[A, B, C]{Item1: item1, Item2: item2, Item3: item3}
										},
									)
								},
							)
						},
					)
				},
			)
		},
	)
}

func Tuple4Serializer[A any, B any, C any, D any](serializerA Serializer[A], serializerB Serializer[B], serializerC Serializer[C], serializerD Serializer[D]) Serializer[ballerina.Tuple4[A, B, C, D]] {
	return WithContext(
		"on tuple4",
		func(value ballerina.Tuple4[A, B, C, D]) ballerina.Sum[error, json.RawMessage] {
			return ballerina.Bind(
				WithContext("on item1", serializerA)(value.Item1),
				func(item1 json.RawMessage) ballerina.Sum[error, json.RawMessage] {
					return ballerina.Bind(
						WithContext("on item2", serializerB)(value.Item2),
						func(item2 json.RawMessage) ballerina.Sum[error, json.RawMessage] {
							return ballerina.Bind(
								WithContext("on item3", serializerC)(value.Item3),
								func(item3 json.RawMessage) ballerina.Sum[error, json.RawMessage] {
									return ballerina.Bind(
										WithContext("on item4", serializerD)(value.Item4),
										func(item4 json.RawMessage) ballerina.Sum[error, json.RawMessage] {
											return WrappedMarshal(
												_sequentialForSerialization{
													Discriminator: tupleDiscriminator,
													Value:         []json.RawMessage{item1, item2, item3, item4},
												},
											)
										},
									)
								},
							)
						},
					)
				},
			)
		},
	)
}

func Tuple4Deserializer[A any, B any, C any, D any](deserializerA Deserializer[A], deserializerB Deserializer[B], deserializerC Deserializer[C], deserializerD Deserializer[D]) Deserializer[ballerina.Tuple4[A, B, C, D]] {
	return unmarshalWithContext(
		"on tuple4",
		func(sequentialForSerialization _sequentialForSerialization) ballerina.Sum[error, ballerina.Tuple4[A, B, C, D]] {
			return ballerina.Bind(
				sequentialForSerialization.getElementsWithDiscriminator(tupleDiscriminator),
				func(elements []json.RawMessage) ballerina.Sum[error, ballerina.Tuple4[A, B, C, D]] {
					if len(elements) != 4 {
						return ballerina.Left[error, ballerina.Tuple4[A, B, C, D]](fmt.Errorf("expected 4 elements in tuple, got %d", len(elements)))
					}
					return ballerina.Bind(
						WithContext("on item1", deserializerA)(elements[0]),
						func(item1 A) ballerina.Sum[error, ballerina.Tuple4[A, B, C, D]] {
							return ballerina.Bind(
								WithContext("on item2", deserializerB)(elements[1]),
								func(item2 B) ballerina.Sum[error, ballerina.Tuple4[A, B, C, D]] {
									return ballerina.Bind(
										WithContext("on item3", deserializerC)(elements[2]),
										func(item3 C) ballerina.Sum[error, ballerina.Tuple4[A, B, C, D]] {
											return ballerina.MapRight(
												WithContext("on item4", deserializerD)(elements[3]),
												func(item4 D) ballerina.Tuple4[A, B, C, D] {
													return ballerina.Tuple4[A, B, C, D]{Item1: item1, Item2: item2, Item3: item3, Item4: item4}
												},
											)
										},
									)
								},
							)
						},
					)
				},
			)
		},
	)
}
