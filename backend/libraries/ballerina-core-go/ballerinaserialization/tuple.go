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

func Tuple5Serializer[A any, B any, C any, D any, E any](serializerA Serializer[A], serializerB Serializer[B], serializerC Serializer[C], serializerD Serializer[D], serializerE Serializer[E]) Serializer[ballerina.Tuple5[A, B, C, D, E]] {
	return WithContext(
		"on tuple5",
		func(value ballerina.Tuple5[A, B, C, D, E]) ballerina.Sum[error, json.RawMessage] {
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
											return ballerina.Bind(
												WithContext("on item5", serializerE)(value.Item5),
												func(item5 json.RawMessage) ballerina.Sum[error, json.RawMessage] {
													return WrappedMarshal(
														_sequentialForSerialization{
															Discriminator: tupleDiscriminator,
															Value:         []json.RawMessage{item1, item2, item3, item4, item5},
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
		},
	)
}

func Tuple5Deserializer[A any, B any, C any, D any, E any](deserializerA Deserializer[A], deserializerB Deserializer[B], deserializerC Deserializer[C], deserializerD Deserializer[D], deserializerE Deserializer[E]) Deserializer[ballerina.Tuple5[A, B, C, D, E]] {
	return unmarshalWithContext(
		"on tuple5",
		func(sequentialForSerialization _sequentialForSerialization) ballerina.Sum[error, ballerina.Tuple5[A, B, C, D, E]] {
			return ballerina.Bind(
				sequentialForSerialization.getElementsWithDiscriminator(tupleDiscriminator),
				func(elements []json.RawMessage) ballerina.Sum[error, ballerina.Tuple5[A, B, C, D, E]] {
					if len(elements) != 5 {
						return ballerina.Left[error, ballerina.Tuple5[A, B, C, D, E]](fmt.Errorf("expected 5 elements in tuple, got %d", len(elements)))
					}
					return ballerina.Bind(
						WithContext("on item1", deserializerA)(elements[0]),
						func(item1 A) ballerina.Sum[error, ballerina.Tuple5[A, B, C, D, E]] {
							return ballerina.Bind(
								WithContext("on item2", deserializerB)(elements[1]),
								func(item2 B) ballerina.Sum[error, ballerina.Tuple5[A, B, C, D, E]] {
									return ballerina.Bind(
										WithContext("on item3", deserializerC)(elements[2]),
										func(item3 C) ballerina.Sum[error, ballerina.Tuple5[A, B, C, D, E]] {
											return ballerina.Bind(
												WithContext("on item4", deserializerD)(elements[3]),
												func(item4 D) ballerina.Sum[error, ballerina.Tuple5[A, B, C, D, E]] {
													return ballerina.MapRight(
														WithContext("on item5", deserializerE)(elements[4]),
														func(item5 E) ballerina.Tuple5[A, B, C, D, E] {
															return ballerina.Tuple5[A, B, C, D, E]{Item1: item1, Item2: item2, Item3: item3, Item4: item4, Item5: item5}
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
		},
	)
}

func Tuple6Serializer[A any, B any, C any, D any, E any, F any](serializerA Serializer[A], serializerB Serializer[B], serializerC Serializer[C], serializerD Serializer[D], serializerE Serializer[E], serializerF Serializer[F]) Serializer[ballerina.Tuple6[A, B, C, D, E, F]] {
	return WithContext(
		"on tuple6",
		func(value ballerina.Tuple6[A, B, C, D, E, F]) ballerina.Sum[error, json.RawMessage] {
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
											return ballerina.Bind(
												WithContext("on item5", serializerE)(value.Item5),
												func(item5 json.RawMessage) ballerina.Sum[error, json.RawMessage] {
													return ballerina.Bind(
														WithContext("on item6", serializerF)(value.Item6),
														func(item6 json.RawMessage) ballerina.Sum[error, json.RawMessage] {
															return WrappedMarshal(
																_sequentialForSerialization{
																	Discriminator: tupleDiscriminator,
																	Value:         []json.RawMessage{item1, item2, item3, item4, item5, item6},
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
				},
			)
		},
	)
}

func Tuple6Deserializer[A any, B any, C any, D any, E any, F any](deserializerA Deserializer[A], deserializerB Deserializer[B], deserializerC Deserializer[C], deserializerD Deserializer[D], deserializerE Deserializer[E], deserializerF Deserializer[F]) Deserializer[ballerina.Tuple6[A, B, C, D, E, F]] {
	return unmarshalWithContext(
		"on tuple6",
		func(sequentialForSerialization _sequentialForSerialization) ballerina.Sum[error, ballerina.Tuple6[A, B, C, D, E, F]] {
			return ballerina.Bind(
				sequentialForSerialization.getElementsWithDiscriminator(tupleDiscriminator),
				func(elements []json.RawMessage) ballerina.Sum[error, ballerina.Tuple6[A, B, C, D, E, F]] {
					if len(elements) != 6 {
						return ballerina.Left[error, ballerina.Tuple6[A, B, C, D, E, F]](fmt.Errorf("expected 6 elements in tuple, got %d", len(elements)))
					}
					return ballerina.Bind(
						WithContext("on item1", deserializerA)(elements[0]),
						func(item1 A) ballerina.Sum[error, ballerina.Tuple6[A, B, C, D, E, F]] {
							return ballerina.Bind(
								WithContext("on item2", deserializerB)(elements[1]),
								func(item2 B) ballerina.Sum[error, ballerina.Tuple6[A, B, C, D, E, F]] {
									return ballerina.Bind(
										WithContext("on item3", deserializerC)(elements[2]),
										func(item3 C) ballerina.Sum[error, ballerina.Tuple6[A, B, C, D, E, F]] {
											return ballerina.Bind(
												WithContext("on item4", deserializerD)(elements[3]),
												func(item4 D) ballerina.Sum[error, ballerina.Tuple6[A, B, C, D, E, F]] {
													return ballerina.Bind(
														WithContext("on item5", deserializerE)(elements[4]),
														func(item5 E) ballerina.Sum[error, ballerina.Tuple6[A, B, C, D, E, F]] {
															return ballerina.MapRight(
																WithContext("on item6", deserializerF)(elements[5]),
																func(item6 F) ballerina.Tuple6[A, B, C, D, E, F] {
																	return ballerina.Tuple6[A, B, C, D, E, F]{Item1: item1, Item2: item2, Item3: item3, Item4: item4, Item5: item5, Item6: item6}
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
				},
			)
		},
	)
}

func Tuple7Serializer[A any, B any, C any, D any, E any, F any, G any](serializerA Serializer[A], serializerB Serializer[B], serializerC Serializer[C], serializerD Serializer[D], serializerE Serializer[E], serializerF Serializer[F], serializerG Serializer[G]) Serializer[ballerina.Tuple7[A, B, C, D, E, F, G]] {
	return WithContext(
		"on tuple7",
		func(value ballerina.Tuple7[A, B, C, D, E, F, G]) ballerina.Sum[error, json.RawMessage] {
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
											return ballerina.Bind(
												WithContext("on item5", serializerE)(value.Item5),
												func(item5 json.RawMessage) ballerina.Sum[error, json.RawMessage] {
													return ballerina.Bind(
														WithContext("on item6", serializerF)(value.Item6),
														func(item6 json.RawMessage) ballerina.Sum[error, json.RawMessage] {
															return ballerina.Bind(
																WithContext("on item7", serializerG)(value.Item7),
																func(item7 json.RawMessage) ballerina.Sum[error, json.RawMessage] {
																	return WrappedMarshal(
																		_sequentialForSerialization{
																			Discriminator: tupleDiscriminator,
																			Value:         []json.RawMessage{item1, item2, item3, item4, item5, item6, item7},
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
						},
					)
				},
			)
		},
	)
}

func Tuple7Deserializer[A any, B any, C any, D any, E any, F any, G any](deserializerA Deserializer[A], deserializerB Deserializer[B], deserializerC Deserializer[C], deserializerD Deserializer[D], deserializerE Deserializer[E], deserializerF Deserializer[F], deserializerG Deserializer[G]) Deserializer[ballerina.Tuple7[A, B, C, D, E, F, G]] {
	return unmarshalWithContext(
		"on tuple7",
		func(sequentialForSerialization _sequentialForSerialization) ballerina.Sum[error, ballerina.Tuple7[A, B, C, D, E, F, G]] {
			return ballerina.Bind(
				sequentialForSerialization.getElementsWithDiscriminator(tupleDiscriminator),
				func(elements []json.RawMessage) ballerina.Sum[error, ballerina.Tuple7[A, B, C, D, E, F, G]] {
					if len(elements) != 7 {
						return ballerina.Left[error, ballerina.Tuple7[A, B, C, D, E, F, G]](fmt.Errorf("expected 7 elements in tuple, got %d", len(elements)))
					}
					return ballerina.Bind(
						WithContext("on item1", deserializerA)(elements[0]),
						func(item1 A) ballerina.Sum[error, ballerina.Tuple7[A, B, C, D, E, F, G]] {
							return ballerina.Bind(
								WithContext("on item2", deserializerB)(elements[1]),
								func(item2 B) ballerina.Sum[error, ballerina.Tuple7[A, B, C, D, E, F, G]] {
									return ballerina.Bind(
										WithContext("on item3", deserializerC)(elements[2]),
										func(item3 C) ballerina.Sum[error, ballerina.Tuple7[A, B, C, D, E, F, G]] {
											return ballerina.Bind(
												WithContext("on item4", deserializerD)(elements[3]),
												func(item4 D) ballerina.Sum[error, ballerina.Tuple7[A, B, C, D, E, F, G]] {
													return ballerina.Bind(
														WithContext("on item5", deserializerE)(elements[4]),
														func(item5 E) ballerina.Sum[error, ballerina.Tuple7[A, B, C, D, E, F, G]] {
															return ballerina.Bind(
																WithContext("on item6", deserializerF)(elements[5]),
																func(item6 F) ballerina.Sum[error, ballerina.Tuple7[A, B, C, D, E, F, G]] {
																	return ballerina.MapRight(
																		WithContext("on item7", deserializerG)(elements[6]),
																		func(item7 G) ballerina.Tuple7[A, B, C, D, E, F, G] {
																			return ballerina.Tuple7[A, B, C, D, E, F, G]{Item1: item1, Item2: item2, Item3: item3, Item4: item4, Item5: item5, Item6: item6, Item7: item7}
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
						},
					)
				},
			)
		},
	)
}

func Tuple8Serializer[A any, B any, C any, D any, E any, F any, G any, H any](serializerA Serializer[A], serializerB Serializer[B], serializerC Serializer[C], serializerD Serializer[D], serializerE Serializer[E], serializerF Serializer[F], serializerG Serializer[G], serializerH Serializer[H]) Serializer[ballerina.Tuple8[A, B, C, D, E, F, G, H]] {
	return WithContext(
		"on tuple8",
		func(value ballerina.Tuple8[A, B, C, D, E, F, G, H]) ballerina.Sum[error, json.RawMessage] {
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
											return ballerina.Bind(
												WithContext("on item5", serializerE)(value.Item5),
												func(item5 json.RawMessage) ballerina.Sum[error, json.RawMessage] {
													return ballerina.Bind(
														WithContext("on item6", serializerF)(value.Item6),
														func(item6 json.RawMessage) ballerina.Sum[error, json.RawMessage] {
															return ballerina.Bind(
																WithContext("on item7", serializerG)(value.Item7),
																func(item7 json.RawMessage) ballerina.Sum[error, json.RawMessage] {
																	return ballerina.Bind(
																		WithContext("on item8", serializerH)(value.Item8),
																		func(item8 json.RawMessage) ballerina.Sum[error, json.RawMessage] {
																			return WrappedMarshal(
																				_sequentialForSerialization{
																					Discriminator: tupleDiscriminator,
																					Value:         []json.RawMessage{item1, item2, item3, item4, item5, item6, item7, item8},
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
								},
							)
						},
					)
				},
			)
		},
	)
}

func Tuple8Deserializer[A any, B any, C any, D any, E any, F any, G any, H any](deserializerA Deserializer[A], deserializerB Deserializer[B], deserializerC Deserializer[C], deserializerD Deserializer[D], deserializerE Deserializer[E], deserializerF Deserializer[F], deserializerG Deserializer[G], deserializerH Deserializer[H]) Deserializer[ballerina.Tuple8[A, B, C, D, E, F, G, H]] {
	return unmarshalWithContext(
		"on tuple8",
		func(sequentialForSerialization _sequentialForSerialization) ballerina.Sum[error, ballerina.Tuple8[A, B, C, D, E, F, G, H]] {
			return ballerina.Bind(
				sequentialForSerialization.getElementsWithDiscriminator(tupleDiscriminator),
				func(elements []json.RawMessage) ballerina.Sum[error, ballerina.Tuple8[A, B, C, D, E, F, G, H]] {
					if len(elements) != 8 {
						return ballerina.Left[error, ballerina.Tuple8[A, B, C, D, E, F, G, H]](fmt.Errorf("expected 8 elements in tuple, got %d", len(elements)))
					}
					return ballerina.Bind(
						WithContext("on item1", deserializerA)(elements[0]),
						func(item1 A) ballerina.Sum[error, ballerina.Tuple8[A, B, C, D, E, F, G, H]] {
							return ballerina.Bind(
								WithContext("on item2", deserializerB)(elements[1]),
								func(item2 B) ballerina.Sum[error, ballerina.Tuple8[A, B, C, D, E, F, G, H]] {
									return ballerina.Bind(
										WithContext("on item3", deserializerC)(elements[2]),
										func(item3 C) ballerina.Sum[error, ballerina.Tuple8[A, B, C, D, E, F, G, H]] {
											return ballerina.Bind(
												WithContext("on item4", deserializerD)(elements[3]),
												func(item4 D) ballerina.Sum[error, ballerina.Tuple8[A, B, C, D, E, F, G, H]] {
													return ballerina.Bind(
														WithContext("on item5", deserializerE)(elements[4]),
														func(item5 E) ballerina.Sum[error, ballerina.Tuple8[A, B, C, D, E, F, G, H]] {
															return ballerina.Bind(
																WithContext("on item6", deserializerF)(elements[5]),
																func(item6 F) ballerina.Sum[error, ballerina.Tuple8[A, B, C, D, E, F, G, H]] {
																	return ballerina.Bind(
																		WithContext("on item7", deserializerG)(elements[6]),
																		func(item7 G) ballerina.Sum[error, ballerina.Tuple8[A, B, C, D, E, F, G, H]] {
																			return ballerina.MapRight(
																				WithContext("on item8", deserializerH)(elements[7]),
																				func(item8 H) ballerina.Tuple8[A, B, C, D, E, F, G, H] {
																					return ballerina.Tuple8[A, B, C, D, E, F, G, H]{Item1: item1, Item2: item2, Item3: item3, Item4: item4, Item5: item5, Item6: item6, Item7: item7, Item8: item8}
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
								},
							)
						},
					)
				},
			)
		},
	)
}

func Tuple9Serializer[A any, B any, C any, D any, E any, F any, G any, H any, I any](serializerA Serializer[A], serializerB Serializer[B], serializerC Serializer[C], serializerD Serializer[D], serializerE Serializer[E], serializerF Serializer[F], serializerG Serializer[G], serializerH Serializer[H], serializerI Serializer[I]) Serializer[ballerina.Tuple9[A, B, C, D, E, F, G, H, I]] {
	return WithContext(
		"on tuple9",
		func(value ballerina.Tuple9[A, B, C, D, E, F, G, H, I]) ballerina.Sum[error, json.RawMessage] {
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
											return ballerina.Bind(
												WithContext("on item5", serializerE)(value.Item5),
												func(item5 json.RawMessage) ballerina.Sum[error, json.RawMessage] {
													return ballerina.Bind(
														WithContext("on item6", serializerF)(value.Item6),
														func(item6 json.RawMessage) ballerina.Sum[error, json.RawMessage] {
															return ballerina.Bind(
																WithContext("on item7", serializerG)(value.Item7),
																func(item7 json.RawMessage) ballerina.Sum[error, json.RawMessage] {
																	return ballerina.Bind(
																		WithContext("on item8", serializerH)(value.Item8),
																		func(item8 json.RawMessage) ballerina.Sum[error, json.RawMessage] {
																			return ballerina.Bind(
																				WithContext("on item9", serializerI)(value.Item9),
																				func(item9 json.RawMessage) ballerina.Sum[error, json.RawMessage] {
																					return WrappedMarshal(
																						_sequentialForSerialization{
																							Discriminator: tupleDiscriminator,
																							Value:         []json.RawMessage{item1, item2, item3, item4, item5, item6, item7, item8, item9},
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

func Tuple9Deserializer[A any, B any, C any, D any, E any, F any, G any, H any, I any](deserializerA Deserializer[A], deserializerB Deserializer[B], deserializerC Deserializer[C], deserializerD Deserializer[D], deserializerE Deserializer[E], deserializerF Deserializer[F], deserializerG Deserializer[G], deserializerH Deserializer[H], deserializerI Deserializer[I]) Deserializer[ballerina.Tuple9[A, B, C, D, E, F, G, H, I]] {
	return unmarshalWithContext(
		"on tuple9",
		func(sequentialForSerialization _sequentialForSerialization) ballerina.Sum[error, ballerina.Tuple9[A, B, C, D, E, F, G, H, I]] {
			return ballerina.Bind(
				sequentialForSerialization.getElementsWithDiscriminator(tupleDiscriminator),
				func(elements []json.RawMessage) ballerina.Sum[error, ballerina.Tuple9[A, B, C, D, E, F, G, H, I]] {
					if len(elements) != 9 {
						return ballerina.Left[error, ballerina.Tuple9[A, B, C, D, E, F, G, H, I]](fmt.Errorf("expected 9 elements in tuple, got %d", len(elements)))
					}
					return ballerina.Bind(
						WithContext("on item1", deserializerA)(elements[0]),
						func(item1 A) ballerina.Sum[error, ballerina.Tuple9[A, B, C, D, E, F, G, H, I]] {
							return ballerina.Bind(
								WithContext("on item2", deserializerB)(elements[1]),
								func(item2 B) ballerina.Sum[error, ballerina.Tuple9[A, B, C, D, E, F, G, H, I]] {
									return ballerina.Bind(
										WithContext("on item3", deserializerC)(elements[2]),
										func(item3 C) ballerina.Sum[error, ballerina.Tuple9[A, B, C, D, E, F, G, H, I]] {
											return ballerina.Bind(
												WithContext("on item4", deserializerD)(elements[3]),
												func(item4 D) ballerina.Sum[error, ballerina.Tuple9[A, B, C, D, E, F, G, H, I]] {
													return ballerina.Bind(
														WithContext("on item5", deserializerE)(elements[4]),
														func(item5 E) ballerina.Sum[error, ballerina.Tuple9[A, B, C, D, E, F, G, H, I]] {
															return ballerina.Bind(
																WithContext("on item6", deserializerF)(elements[5]),
																func(item6 F) ballerina.Sum[error, ballerina.Tuple9[A, B, C, D, E, F, G, H, I]] {
																	return ballerina.Bind(
																		WithContext("on item7", deserializerG)(elements[6]),
																		func(item7 G) ballerina.Sum[error, ballerina.Tuple9[A, B, C, D, E, F, G, H, I]] {
																			return ballerina.Bind(
																				WithContext("on item8", deserializerH)(elements[7]),
																				func(item8 H) ballerina.Sum[error, ballerina.Tuple9[A, B, C, D, E, F, G, H, I]] {
																					return ballerina.MapRight(
																						WithContext("on item9", deserializerI)(elements[8]),
																						func(item9 I) ballerina.Tuple9[A, B, C, D, E, F, G, H, I] {
																							return ballerina.Tuple9[A, B, C, D, E, F, G, H, I]{Item1: item1, Item2: item2, Item3: item3, Item4: item4, Item5: item5, Item6: item6, Item7: item7, Item8: item8, Item9: item9}
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

func Tuple10Serializer[A any, B any, C any, D any, E any, F any, G any, H any, I any, J any](serializerA Serializer[A], serializerB Serializer[B], serializerC Serializer[C], serializerD Serializer[D], serializerE Serializer[E], serializerF Serializer[F], serializerG Serializer[G], serializerH Serializer[H], serializerI Serializer[I], serializerJ Serializer[J]) Serializer[ballerina.Tuple10[A, B, C, D, E, F, G, H, I, J]] {
	return WithContext(
		"on tuple10",
		func(value ballerina.Tuple10[A, B, C, D, E, F, G, H, I, J]) ballerina.Sum[error, json.RawMessage] {
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
											return ballerina.Bind(
												WithContext("on item5", serializerE)(value.Item5),
												func(item5 json.RawMessage) ballerina.Sum[error, json.RawMessage] {
													return ballerina.Bind(
														WithContext("on item6", serializerF)(value.Item6),
														func(item6 json.RawMessage) ballerina.Sum[error, json.RawMessage] {
															return ballerina.Bind(
																WithContext("on item7", serializerG)(value.Item7),
																func(item7 json.RawMessage) ballerina.Sum[error, json.RawMessage] {
																	return ballerina.Bind(
																		WithContext("on item8", serializerH)(value.Item8),
																		func(item8 json.RawMessage) ballerina.Sum[error, json.RawMessage] {
																			return ballerina.Bind(
																				WithContext("on item9", serializerI)(value.Item9),
																				func(item9 json.RawMessage) ballerina.Sum[error, json.RawMessage] {
																					return ballerina.Bind(
																						WithContext("on item10", serializerJ)(value.Item10),
																						func(item10 json.RawMessage) ballerina.Sum[error, json.RawMessage] {
																							return WrappedMarshal(
																								_sequentialForSerialization{
																									Discriminator: tupleDiscriminator,
																									Value:         []json.RawMessage{item1, item2, item3, item4, item5, item6, item7, item8, item9, item10},
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

func Tuple10Deserializer[A any, B any, C any, D any, E any, F any, G any, H any, I any, J any](deserializerA Deserializer[A], deserializerB Deserializer[B], deserializerC Deserializer[C], deserializerD Deserializer[D], deserializerE Deserializer[E], deserializerF Deserializer[F], deserializerG Deserializer[G], deserializerH Deserializer[H], deserializerI Deserializer[I], deserializerJ Deserializer[J]) Deserializer[ballerina.Tuple10[A, B, C, D, E, F, G, H, I, J]] {
	return unmarshalWithContext(
		"on tuple10",
		func(sequentialForSerialization _sequentialForSerialization) ballerina.Sum[error, ballerina.Tuple10[A, B, C, D, E, F, G, H, I, J]] {
			return ballerina.Bind(
				sequentialForSerialization.getElementsWithDiscriminator(tupleDiscriminator),
				func(elements []json.RawMessage) ballerina.Sum[error, ballerina.Tuple10[A, B, C, D, E, F, G, H, I, J]] {
					if len(elements) != 10 {
						return ballerina.Left[error, ballerina.Tuple10[A, B, C, D, E, F, G, H, I, J]](fmt.Errorf("expected 10 elements in tuple, got %d", len(elements)))
					}
					return ballerina.Bind(
						WithContext("on item1", deserializerA)(elements[0]),
						func(item1 A) ballerina.Sum[error, ballerina.Tuple10[A, B, C, D, E, F, G, H, I, J]] {
							return ballerina.Bind(
								WithContext("on item2", deserializerB)(elements[1]),
								func(item2 B) ballerina.Sum[error, ballerina.Tuple10[A, B, C, D, E, F, G, H, I, J]] {
									return ballerina.Bind(
										WithContext("on item3", deserializerC)(elements[2]),
										func(item3 C) ballerina.Sum[error, ballerina.Tuple10[A, B, C, D, E, F, G, H, I, J]] {
											return ballerina.Bind(
												WithContext("on item4", deserializerD)(elements[3]),
												func(item4 D) ballerina.Sum[error, ballerina.Tuple10[A, B, C, D, E, F, G, H, I, J]] {
													return ballerina.Bind(
														WithContext("on item5", deserializerE)(elements[4]),
														func(item5 E) ballerina.Sum[error, ballerina.Tuple10[A, B, C, D, E, F, G, H, I, J]] {
															return ballerina.Bind(
																WithContext("on item6", deserializerF)(elements[5]),
																func(item6 F) ballerina.Sum[error, ballerina.Tuple10[A, B, C, D, E, F, G, H, I, J]] {
																	return ballerina.Bind(
																		WithContext("on item7", deserializerG)(elements[6]),
																		func(item7 G) ballerina.Sum[error, ballerina.Tuple10[A, B, C, D, E, F, G, H, I, J]] {
																			return ballerina.Bind(
																				WithContext("on item8", deserializerH)(elements[7]),
																				func(item8 H) ballerina.Sum[error, ballerina.Tuple10[A, B, C, D, E, F, G, H, I, J]] {
																					return ballerina.Bind(
																						WithContext("on item9", deserializerI)(elements[8]),
																						func(item9 I) ballerina.Sum[error, ballerina.Tuple10[A, B, C, D, E, F, G, H, I, J]] {
																							return ballerina.MapRight(
																								WithContext("on item10", deserializerJ)(elements[9]),
																								func(item10 J) ballerina.Tuple10[A, B, C, D, E, F, G, H, I, J] {
																									return ballerina.Tuple10[A, B, C, D, E, F, G, H, I, J]{Item1: item1, Item2: item2, Item3: item3, Item4: item4, Item5: item5, Item6: item6, Item7: item7, Item8: item8, Item9: item9, Item10: item10}
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
