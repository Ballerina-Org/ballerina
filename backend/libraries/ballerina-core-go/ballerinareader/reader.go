package ballerinareader

import (
	"fmt"

	ballerina "ballerina.com/core"
)

type Reader[Ctx, O any] struct {
	Apply func(Ctx) O
}

type ReaderWithError[Ctx, O any] struct {
	Apply func(Ctx) (O, error)
}

func Map[Ctx, A, B any](reader Reader[Ctx, A], f func(A) B) Reader[Ctx, B] {
	return Reader[Ctx, B]{
		Apply: ballerina.Then(reader.Apply, f),
	}
}

func MapWithError[Ctx, A, B any](reader ReaderWithError[Ctx, A], f func(a A) B) ReaderWithError[Ctx, B] {
	return MapReaderWithError(reader, ballerina.ReturningNilError(f))
}

func MapReaderWithError[Ctx, A, B any](reader ReaderWithError[Ctx, A], f func(a A) (B, error)) ReaderWithError[Ctx, B] {
	return ReaderWithError[Ctx, B]{
		Apply: ballerina.ThenWithError(reader.Apply, f),
	}
}

func Parallel2[Ctx, O1, O2 any](reader1 Reader[Ctx, O1], reader2 Reader[Ctx, O2]) Reader[Ctx, ballerina.Tuple2[O1, O2]] {
	return Reader[Ctx, ballerina.Tuple2[O1, O2]]{
		Apply: func(ctx Ctx) ballerina.Tuple2[O1, O2] {
			return ballerina.NewTuple2(reader1.Apply(ctx), reader2.Apply(ctx))
		},
	}
}

func Parallel3[Ctx, O1, O2, O3 any](reader1 Reader[Ctx, O1], reader2 Reader[Ctx, O2], reader3 Reader[Ctx, O3]) Reader[Ctx, ballerina.Tuple3[O1, O2, O3]] {
	return Reader[Ctx, ballerina.Tuple3[O1, O2, O3]]{
		Apply: func(ctx Ctx) ballerina.Tuple3[O1, O2, O3] {
			return ballerina.NewTuple3(reader1.Apply(ctx), reader2.Apply(ctx), reader3.Apply(ctx))
		},
	}
}

func Parallel4[Ctx, O1, O2, O3, O4 any](reader1 Reader[Ctx, O1], reader2 Reader[Ctx, O2], reader3 Reader[Ctx, O3], reader4 Reader[Ctx, O4]) Reader[Ctx, ballerina.Tuple4[O1, O2, O3, O4]] {
	return Reader[Ctx, ballerina.Tuple4[O1, O2, O3, O4]]{
		Apply: func(ctx Ctx) ballerina.Tuple4[O1, O2, O3, O4] {
			return ballerina.NewTuple4(reader1.Apply(ctx), reader2.Apply(ctx), reader3.Apply(ctx), reader4.Apply(ctx))
		},
	}
}

func Parallel5[Ctx, O1, O2, O3, O4, O5 any](reader1 Reader[Ctx, O1], reader2 Reader[Ctx, O2], reader3 Reader[Ctx, O3], reader4 Reader[Ctx, O4], reader5 Reader[Ctx, O5]) Reader[Ctx, ballerina.Tuple5[O1, O2, O3, O4, O5]] {
	return Reader[Ctx, ballerina.Tuple5[O1, O2, O3, O4, O5]]{
		Apply: func(ctx Ctx) ballerina.Tuple5[O1, O2, O3, O4, O5] {
			return ballerina.NewTuple5(reader1.Apply(ctx), reader2.Apply(ctx), reader3.Apply(ctx), reader4.Apply(ctx), reader5.Apply(ctx))
		},
	}
}

func Parallel6[Ctx, O1, O2, O3, O4, O5, O6 any](reader1 Reader[Ctx, O1], reader2 Reader[Ctx, O2], reader3 Reader[Ctx, O3], reader4 Reader[Ctx, O4], reader5 Reader[Ctx, O5], reader6 Reader[Ctx, O6]) Reader[Ctx, ballerina.Tuple6[O1, O2, O3, O4, O5, O6]] {
	return Reader[Ctx, ballerina.Tuple6[O1, O2, O3, O4, O5, O6]]{
		Apply: func(ctx Ctx) ballerina.Tuple6[O1, O2, O3, O4, O5, O6] {
			return ballerina.NewTuple6(reader1.Apply(ctx), reader2.Apply(ctx), reader3.Apply(ctx), reader4.Apply(ctx), reader5.Apply(ctx), reader6.Apply(ctx))
		},
	}
}

func Parallel7[Ctx, O1, O2, O3, O4, O5, O6, O7 any](reader1 Reader[Ctx, O1], reader2 Reader[Ctx, O2], reader3 Reader[Ctx, O3], reader4 Reader[Ctx, O4], reader5 Reader[Ctx, O5], reader6 Reader[Ctx, O6], reader7 Reader[Ctx, O7]) Reader[Ctx, ballerina.Tuple7[O1, O2, O3, O4, O5, O6, O7]] {
	return Reader[Ctx, ballerina.Tuple7[O1, O2, O3, O4, O5, O6, O7]]{
		Apply: func(ctx Ctx) ballerina.Tuple7[O1, O2, O3, O4, O5, O6, O7] {
			return ballerina.NewTuple7(reader1.Apply(ctx), reader2.Apply(ctx), reader3.Apply(ctx), reader4.Apply(ctx), reader5.Apply(ctx), reader6.Apply(ctx), reader7.Apply(ctx))
		},
	}
}

func Parallel8[Ctx, O1, O2, O3, O4, O5, O6, O7, O8 any](reader1 Reader[Ctx, O1], reader2 Reader[Ctx, O2], reader3 Reader[Ctx, O3], reader4 Reader[Ctx, O4], reader5 Reader[Ctx, O5], reader6 Reader[Ctx, O6], reader7 Reader[Ctx, O7], reader8 Reader[Ctx, O8]) Reader[Ctx, ballerina.Tuple8[O1, O2, O3, O4, O5, O6, O7, O8]] {
	return Reader[Ctx, ballerina.Tuple8[O1, O2, O3, O4, O5, O6, O7, O8]]{
		Apply: func(ctx Ctx) ballerina.Tuple8[O1, O2, O3, O4, O5, O6, O7, O8] {
			return ballerina.NewTuple8(reader1.Apply(ctx), reader2.Apply(ctx), reader3.Apply(ctx), reader4.Apply(ctx), reader5.Apply(ctx), reader6.Apply(ctx), reader7.Apply(ctx), reader8.Apply(ctx))
		},
	}
}

func Parallel9[Ctx, O1, O2, O3, O4, O5, O6, O7, O8, O9 any](reader1 Reader[Ctx, O1], reader2 Reader[Ctx, O2], reader3 Reader[Ctx, O3], reader4 Reader[Ctx, O4], reader5 Reader[Ctx, O5], reader6 Reader[Ctx, O6], reader7 Reader[Ctx, O7], reader8 Reader[Ctx, O8], reader9 Reader[Ctx, O9]) Reader[Ctx, ballerina.Tuple9[O1, O2, O3, O4, O5, O6, O7, O8, O9]] {
	return Reader[Ctx, ballerina.Tuple9[O1, O2, O3, O4, O5, O6, O7, O8, O9]]{
		Apply: func(ctx Ctx) ballerina.Tuple9[O1, O2, O3, O4, O5, O6, O7, O8, O9] {
			return ballerina.NewTuple9(reader1.Apply(ctx), reader2.Apply(ctx), reader3.Apply(ctx), reader4.Apply(ctx), reader5.Apply(ctx), reader6.Apply(ctx), reader7.Apply(ctx), reader8.Apply(ctx), reader9.Apply(ctx))
		},
	}
}

func ParallelWithError2[Ctx, O1, O2 any](reader1 ReaderWithError[Ctx, O1], reader2 ReaderWithError[Ctx, O2]) ReaderWithError[Ctx, ballerina.Tuple2[O1, O2]] {
	return ReaderWithError[Ctx, ballerina.Tuple2[O1, O2]]{
		Apply: func(ctx Ctx) (ballerina.Tuple2[O1, O2], error) {
			mapped1, err := reader1.Apply(ctx)
			if err != nil {
				return ballerina.NewTuple2(*new(O1), *new(O2)), fmt.Errorf("parallelWithError2 error in reader1: %w", err)
			}

			mapped2, err := reader2.Apply(ctx)
			if err != nil {
				return ballerina.NewTuple2(mapped1, *new(O2)), fmt.Errorf("parallelWithError2 error in reader2: %w", err)
			}

			return ballerina.NewTuple2(mapped1, mapped2), nil
		},
	}
}

func ParallelWithError3[Ctx, O1, O2, O3 any](reader1 ReaderWithError[Ctx, O1], reader2 ReaderWithError[Ctx, O2], reader3 ReaderWithError[Ctx, O3]) ReaderWithError[Ctx, ballerina.Tuple3[O1, O2, O3]] {
	return ReaderWithError[Ctx, ballerina.Tuple3[O1, O2, O3]]{
		Apply: func(ctx Ctx) (ballerina.Tuple3[O1, O2, O3], error) {
			mapped1, err := reader1.Apply(ctx)
			if err != nil {
				return ballerina.NewTuple3(*new(O1), *new(O2), *new(O3)), fmt.Errorf("parallelWithError3 error in reader1: %w", err)
			}

			mapped2, err := reader2.Apply(ctx)
			if err != nil {
				return ballerina.NewTuple3(mapped1, *new(O2), *new(O3)), fmt.Errorf("parallelWithError3 error in reader2: %w", err)
			}

			mapped3, err := reader3.Apply(ctx)
			if err != nil {
				return ballerina.NewTuple3(mapped1, mapped2, *new(O3)), fmt.Errorf("parallelWithError3 error in reader3: %w", err)
			}

			return ballerina.NewTuple3(mapped1, mapped2, mapped3), nil
		},
	}
}

func ParallelWithError4[Ctx, O1, O2, O3, O4 any](reader1 ReaderWithError[Ctx, O1], reader2 ReaderWithError[Ctx, O2], reader3 ReaderWithError[Ctx, O3], reader4 ReaderWithError[Ctx, O4]) ReaderWithError[Ctx, ballerina.Tuple4[O1, O2, O3, O4]] {
	return ReaderWithError[Ctx, ballerina.Tuple4[O1, O2, O3, O4]]{
		Apply: func(ctx Ctx) (ballerina.Tuple4[O1, O2, O3, O4], error) {
			mapped1, err := reader1.Apply(ctx)
			if err != nil {
				return ballerina.NewTuple4(*new(O1), *new(O2), *new(O3), *new(O4)), fmt.Errorf("parallelWithError4 error in reader1: %w", err)
			}

			mapped2, err := reader2.Apply(ctx)
			if err != nil {
				return ballerina.NewTuple4(mapped1, *new(O2), *new(O3), *new(O4)), fmt.Errorf("parallelWithError4 error in reader2: %w", err)
			}

			mapped3, err := reader3.Apply(ctx)
			if err != nil {
				return ballerina.NewTuple4(mapped1, mapped2, *new(O3), *new(O4)), fmt.Errorf("parallelWithError4 error in reader3: %w", err)
			}

			mapped4, err := reader4.Apply(ctx)
			if err != nil {
				return ballerina.NewTuple4(mapped1, mapped2, mapped3, *new(O4)), fmt.Errorf("parallelWithError4 error in reader4: %w", err)
			}

			return ballerina.NewTuple4(mapped1, mapped2, mapped3, mapped4), nil
		},
	}
}

func ParallelWithError5[Ctx, O1, O2, O3, O4, O5 any](reader1 ReaderWithError[Ctx, O1], reader2 ReaderWithError[Ctx, O2], reader3 ReaderWithError[Ctx, O3], reader4 ReaderWithError[Ctx, O4], reader5 ReaderWithError[Ctx, O5]) ReaderWithError[Ctx, ballerina.Tuple5[O1, O2, O3, O4, O5]] {
	return ReaderWithError[Ctx, ballerina.Tuple5[O1, O2, O3, O4, O5]]{
		Apply: func(ctx Ctx) (ballerina.Tuple5[O1, O2, O3, O4, O5], error) {
			mapped1, err := reader1.Apply(ctx)
			if err != nil {
				return ballerina.NewTuple5(*new(O1), *new(O2), *new(O3), *new(O4), *new(O5)), fmt.Errorf("parallelWithError5 error in reader1: %w", err)
			}

			mapped2, err := reader2.Apply(ctx)
			if err != nil {
				return ballerina.NewTuple5(mapped1, *new(O2), *new(O3), *new(O4), *new(O5)), fmt.Errorf("parallelWithError5 error in reader2: %w", err)
			}

			mapped3, err := reader3.Apply(ctx)
			if err != nil {
				return ballerina.NewTuple5(mapped1, mapped2, *new(O3), *new(O4), *new(O5)), fmt.Errorf("parallelWithError5 error in reader3: %w", err)
			}

			mapped4, err := reader4.Apply(ctx)
			if err != nil {
				return ballerina.NewTuple5(mapped1, mapped2, mapped3, *new(O4), *new(O5)), fmt.Errorf("parallelWithError5 error in reader4: %w", err)
			}

			mapped5, err := reader5.Apply(ctx)
			if err != nil {
				return ballerina.NewTuple5(mapped1, mapped2, mapped3, mapped4, *new(O5)), fmt.Errorf("parallelWithError5 error in reader5: %w", err)
			}

			return ballerina.NewTuple5(mapped1, mapped2, mapped3, mapped4, mapped5), nil
		},
	}
}

func ParallelWithError6[Ctx, O1, O2, O3, O4, O5, O6 any](reader1 ReaderWithError[Ctx, O1], reader2 ReaderWithError[Ctx, O2], reader3 ReaderWithError[Ctx, O3], reader4 ReaderWithError[Ctx, O4], reader5 ReaderWithError[Ctx, O5], reader6 ReaderWithError[Ctx, O6]) ReaderWithError[Ctx, ballerina.Tuple6[O1, O2, O3, O4, O5, O6]] {
	return ReaderWithError[Ctx, ballerina.Tuple6[O1, O2, O3, O4, O5, O6]]{
		Apply: func(ctx Ctx) (ballerina.Tuple6[O1, O2, O3, O4, O5, O6], error) {
			mapped1, err := reader1.Apply(ctx)
			if err != nil {
				return ballerina.NewTuple6(*new(O1), *new(O2), *new(O3), *new(O4), *new(O5), *new(O6)), fmt.Errorf("parallelWithError6 error in reader1: %w", err)
			}

			mapped2, err := reader2.Apply(ctx)
			if err != nil {
				return ballerina.NewTuple6(mapped1, *new(O2), *new(O3), *new(O4), *new(O5), *new(O6)), fmt.Errorf("parallelWithError6 error in reader2: %w", err)
			}

			mapped3, err := reader3.Apply(ctx)
			if err != nil {
				return ballerina.NewTuple6(mapped1, mapped2, *new(O3), *new(O4), *new(O5), *new(O6)), fmt.Errorf("parallelWithError6 error in reader3: %w", err)
			}

			mapped4, err := reader4.Apply(ctx)
			if err != nil {
				return ballerina.NewTuple6(mapped1, mapped2, mapped3, *new(O4), *new(O5), *new(O6)), fmt.Errorf("parallelWithError6 error in reader4: %w", err)
			}

			mapped5, err := reader5.Apply(ctx)
			if err != nil {
				return ballerina.NewTuple6(mapped1, mapped2, mapped3, mapped4, *new(O5), *new(O6)), fmt.Errorf("parallelWithError6 error in reader5: %w", err)
			}

			mapped6, err := reader6.Apply(ctx)
			if err != nil {
				return ballerina.NewTuple6(mapped1, mapped2, mapped3, mapped4, mapped5, *new(O6)), fmt.Errorf("parallelWithError6 error in reader6: %w", err)
			}

			return ballerina.NewTuple6(mapped1, mapped2, mapped3, mapped4, mapped5, mapped6), nil
		},
	}
}

func ParallelWithError7[Ctx, O1, O2, O3, O4, O5, O6, O7 any](reader1 ReaderWithError[Ctx, O1], reader2 ReaderWithError[Ctx, O2], reader3 ReaderWithError[Ctx, O3], reader4 ReaderWithError[Ctx, O4], reader5 ReaderWithError[Ctx, O5], reader6 ReaderWithError[Ctx, O6], reader7 ReaderWithError[Ctx, O7]) ReaderWithError[Ctx, ballerina.Tuple7[O1, O2, O3, O4, O5, O6, O7]] {
	return ReaderWithError[Ctx, ballerina.Tuple7[O1, O2, O3, O4, O5, O6, O7]]{
		Apply: func(ctx Ctx) (ballerina.Tuple7[O1, O2, O3, O4, O5, O6, O7], error) {
			mapped1, err := reader1.Apply(ctx)
			if err != nil {
				return ballerina.NewTuple7(*new(O1), *new(O2), *new(O3), *new(O4), *new(O5), *new(O6), *new(O7)), fmt.Errorf("parallelWithError7 error in reader1: %w", err)
			}

			mapped2, err := reader2.Apply(ctx)
			if err != nil {
				return ballerina.NewTuple7(mapped1, *new(O2), *new(O3), *new(O4), *new(O5), *new(O6), *new(O7)), fmt.Errorf("parallelWithError7 error in reader2: %w", err)
			}

			mapped3, err := reader3.Apply(ctx)
			if err != nil {
				return ballerina.NewTuple7(mapped1, mapped2, *new(O3), *new(O4), *new(O5), *new(O6), *new(O7)), fmt.Errorf("parallelWithError7 error in reader3: %w", err)
			}

			mapped4, err := reader4.Apply(ctx)
			if err != nil {
				return ballerina.NewTuple7(mapped1, mapped2, mapped3, *new(O4), *new(O5), *new(O6), *new(O7)), fmt.Errorf("parallelWithError7 error in reader4: %w", err)
			}

			mapped5, err := reader5.Apply(ctx)
			if err != nil {
				return ballerina.NewTuple7(mapped1, mapped2, mapped3, mapped4, *new(O5), *new(O6), *new(O7)), fmt.Errorf("parallelWithError7 error in reader5: %w", err)
			}

			mapped6, err := reader6.Apply(ctx)
			if err != nil {
				return ballerina.NewTuple7(mapped1, mapped2, mapped3, mapped4, mapped5, *new(O6), *new(O7)), fmt.Errorf("parallelWithError7 error in reader6: %w", err)
			}

			mapped7, err := reader7.Apply(ctx)
			if err != nil {
				return ballerina.NewTuple7(mapped1, mapped2, mapped3, mapped4, mapped5, mapped6, *new(O7)), fmt.Errorf("parallelWithError7 error in reader7: %w", err)
			}

			return ballerina.NewTuple7(mapped1, mapped2, mapped3, mapped4, mapped5, mapped6, mapped7), nil
		},
	}
}

func ParallelWithError8[Ctx, O1, O2, O3, O4, O5, O6, O7, O8 any](reader1 ReaderWithError[Ctx, O1], reader2 ReaderWithError[Ctx, O2], reader3 ReaderWithError[Ctx, O3], reader4 ReaderWithError[Ctx, O4], reader5 ReaderWithError[Ctx, O5], reader6 ReaderWithError[Ctx, O6], reader7 ReaderWithError[Ctx, O7], reader8 ReaderWithError[Ctx, O8]) ReaderWithError[Ctx, ballerina.Tuple8[O1, O2, O3, O4, O5, O6, O7, O8]] {
	return ReaderWithError[Ctx, ballerina.Tuple8[O1, O2, O3, O4, O5, O6, O7, O8]]{
		Apply: func(ctx Ctx) (ballerina.Tuple8[O1, O2, O3, O4, O5, O6, O7, O8], error) {
			mapped1, err := reader1.Apply(ctx)
			if err != nil {
				return ballerina.NewTuple8(*new(O1), *new(O2), *new(O3), *new(O4), *new(O5), *new(O6), *new(O7), *new(O8)), fmt.Errorf("parallelWithError8 error in reader1: %w", err)
			}

			mapped2, err := reader2.Apply(ctx)
			if err != nil {
				return ballerina.NewTuple8(mapped1, *new(O2), *new(O3), *new(O4), *new(O5), *new(O6), *new(O7), *new(O8)), fmt.Errorf("parallelWithError8 error in reader2: %w", err)
			}

			mapped3, err := reader3.Apply(ctx)
			if err != nil {
				return ballerina.NewTuple8(mapped1, mapped2, *new(O3), *new(O4), *new(O5), *new(O6), *new(O7), *new(O8)), fmt.Errorf("parallelWithError8 error in reader3: %w", err)
			}

			mapped4, err := reader4.Apply(ctx)
			if err != nil {
				return ballerina.NewTuple8(mapped1, mapped2, mapped3, *new(O4), *new(O5), *new(O6), *new(O7), *new(O8)), fmt.Errorf("parallelWithError8 error in reader4: %w", err)
			}

			mapped5, err := reader5.Apply(ctx)
			if err != nil {
				return ballerina.NewTuple8(mapped1, mapped2, mapped3, mapped4, *new(O5), *new(O6), *new(O7), *new(O8)), fmt.Errorf("parallelWithError8 error in reader5: %w", err)
			}

			mapped6, err := reader6.Apply(ctx)
			if err != nil {
				return ballerina.NewTuple8(mapped1, mapped2, mapped3, mapped4, mapped5, *new(O6), *new(O7), *new(O8)), fmt.Errorf("parallelWithError8 error in reader6: %w", err)
			}

			mapped7, err := reader7.Apply(ctx)
			if err != nil {
				return ballerina.NewTuple8(mapped1, mapped2, mapped3, mapped4, mapped5, mapped6, *new(O7), *new(O8)), fmt.Errorf("parallelWithError8 error in reader7: %w", err)
			}

			mapped8, err := reader8.Apply(ctx)
			if err != nil {
				return ballerina.NewTuple8(mapped1, mapped2, mapped3, mapped4, mapped5, mapped6, mapped7, *new(O8)), fmt.Errorf("parallelWithError8 error in reader8: %w", err)
			}

			return ballerina.NewTuple8(mapped1, mapped2, mapped3, mapped4, mapped5, mapped6, mapped7, mapped8), nil
		},
	}
}

func ParallelWithError9[Ctx, O1, O2, O3, O4, O5, O6, O7, O8, O9 any](reader1 ReaderWithError[Ctx, O1], reader2 ReaderWithError[Ctx, O2], reader3 ReaderWithError[Ctx, O3], reader4 ReaderWithError[Ctx, O4], reader5 ReaderWithError[Ctx, O5], reader6 ReaderWithError[Ctx, O6], reader7 ReaderWithError[Ctx, O7], reader8 ReaderWithError[Ctx, O8], reader9 ReaderWithError[Ctx, O9]) ReaderWithError[Ctx, ballerina.Tuple9[O1, O2, O3, O4, O5, O6, O7, O8, O9]] {
	return ReaderWithError[Ctx, ballerina.Tuple9[O1, O2, O3, O4, O5, O6, O7, O8, O9]]{
		Apply: func(ctx Ctx) (ballerina.Tuple9[O1, O2, O3, O4, O5, O6, O7, O8, O9], error) {
			mapped1, err := reader1.Apply(ctx)
			if err != nil {
				return ballerina.NewTuple9(*new(O1), *new(O2), *new(O3), *new(O4), *new(O5), *new(O6), *new(O7), *new(O8), *new(O9)), fmt.Errorf("parallelWithError9 error in reader1: %w", err)
			}

			mapped2, err := reader2.Apply(ctx)
			if err != nil {
				return ballerina.NewTuple9(mapped1, *new(O2), *new(O3), *new(O4), *new(O5), *new(O6), *new(O7), *new(O8), *new(O9)), fmt.Errorf("parallelWithError9 error in reader2: %w", err)
			}

			mapped3, err := reader3.Apply(ctx)
			if err != nil {
				return ballerina.NewTuple9(mapped1, mapped2, *new(O3), *new(O4), *new(O5), *new(O6), *new(O7), *new(O8), *new(O9)), fmt.Errorf("parallelWithError9 error in reader3: %w", err)
			}

			mapped4, err := reader4.Apply(ctx)
			if err != nil {
				return ballerina.NewTuple9(mapped1, mapped2, mapped3, *new(O4), *new(O5), *new(O6), *new(O7), *new(O8), *new(O9)), fmt.Errorf("parallelWithError9 error in reader4: %w", err)
			}

			mapped5, err := reader5.Apply(ctx)
			if err != nil {
				return ballerina.NewTuple9(mapped1, mapped2, mapped3, mapped4, *new(O5), *new(O6), *new(O7), *new(O8), *new(O9)), fmt.Errorf("parallelWithError9 error in reader5: %w", err)
			}

			mapped6, err := reader6.Apply(ctx)
			if err != nil {
				return ballerina.NewTuple9(mapped1, mapped2, mapped3, mapped4, mapped5, *new(O6), *new(O7), *new(O8), *new(O9)), fmt.Errorf("parallelWithError9 error in reader6: %w", err)
			}

			mapped7, err := reader7.Apply(ctx)
			if err != nil {
				return ballerina.NewTuple9(mapped1, mapped2, mapped3, mapped4, mapped5, mapped6, *new(O7), *new(O8), *new(O9)), fmt.Errorf("parallelWithError9 error in reader7: %w", err)
			}

			mapped8, err := reader8.Apply(ctx)
			if err != nil {
				return ballerina.NewTuple9(mapped1, mapped2, mapped3, mapped4, mapped5, mapped6, mapped7, *new(O8), *new(O9)), fmt.Errorf("parallelWithError9 error in reader8: %w", err)
			}

			mapped9, err := reader9.Apply(ctx)
			if err != nil {
				return ballerina.NewTuple9(mapped1, mapped2, mapped3, mapped4, mapped5, mapped6, mapped7, mapped8, *new(O9)), fmt.Errorf("parallelWithError9 error in reader9: %w", err)
			}

			return ballerina.NewTuple9(mapped1, mapped2, mapped3, mapped4, mapped5, mapped6, mapped7, mapped8, mapped9), nil
		},
	}
}

func Flatten[Ctx, A any](reader Reader[Ctx, Reader[Ctx, A]]) Reader[Ctx, A] {
	return Reader[Ctx, A]{
		Apply: func(ctx Ctx) A {
			return reader.Apply(ctx).Apply(ctx)
		},
	}
}

func FlattenWithError[Ctx, A any](reader ReaderWithError[Ctx, ReaderWithError[Ctx, A]]) ReaderWithError[Ctx, A] {
	return ReaderWithError[Ctx, A]{
		Apply: func(ctx Ctx) (A, error) {
			decoratedOuterReader := DecorateReaderError[Ctx, ReaderWithError[Ctx, A]](func(err error) error {
				return fmt.Errorf("flattenWithError error applying decoratedOuterReader: %w", err)
			})(reader)
			inner, err := decoratedOuterReader.Apply(ctx)
			if err != nil {
				return *new(A), err
			}
			decoratedInnerReader := DecorateReaderError[Ctx, A](func(err error) error {
				return fmt.Errorf("flattenWithError error applying decoratedInnerReader: %w", err)
			})(inner)
			return decoratedInnerReader.Apply(ctx)
		},
	}
}

func Pipeline2[Ctx, A, B any](readerA Reader[Ctx, A], fn func(A) Reader[Ctx, B]) Reader[Ctx, B] {
	return Flatten(Map(readerA, fn))
}

func Pipeline2WithError[Ctx, A, B any](readerA ReaderWithError[Ctx, A], fn func(A) ReaderWithError[Ctx, B]) ReaderWithError[Ctx, B] {
	return FlattenWithError(MapWithError(readerA, fn))
}
