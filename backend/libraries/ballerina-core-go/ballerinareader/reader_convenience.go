package ballerinareader

import ballerina "ballerina.com/core"

func Then[Ctx, A, B any](readerA Reader[Ctx, A], readerB Reader[A, B]) Reader[Ctx, B] {
	return Map(readerA, readerB.Apply)
}

func Then3[Ctx, A, B, C any](readerA Reader[Ctx, A], readerB Reader[A, B], readerC Reader[B, C]) Reader[Ctx, C] {
	return Then(Then(readerA, readerB), readerC)
}

func Then4[Ctx, A, B, C, D any](readerA Reader[Ctx, A], readerB Reader[A, B], readerC Reader[B, C], readerD Reader[C, D]) Reader[Ctx, D] {
	return Then(Then(Then(readerA, readerB), readerC), readerD)
}

func ThenWithError[Ctx, A, B any](readerA ReaderWithError[Ctx, A], readerB ReaderWithError[A, B]) ReaderWithError[Ctx, B] {
	return MapReaderWithError(readerA, readerB.Apply)
}

func ThenWithError3[Ctx, A, B, C any](readerA ReaderWithError[Ctx, A], readerB ReaderWithError[A, B], readerC ReaderWithError[B, C]) ReaderWithError[Ctx, C] {
	return ThenWithError(ThenWithError(readerA, readerB), readerC)
}

func DecorateReaderError[Ctx, O any](f func(err error) error) func(m ReaderWithError[Ctx, O]) ReaderWithError[Ctx, O] {
	return func(m ReaderWithError[Ctx, O]) ReaderWithError[Ctx, O] {
		return ReaderWithError[Ctx, O]{
			Apply: func(input Ctx) (O, error) {
				mapped, err := m.Apply(input)
				if err != nil {
					return mapped, f(err)
				}
				return mapped, nil
			},
		}
	}
}

func ToReaderWithError[Ctx, O any](reader Reader[Ctx, O]) ReaderWithError[Ctx, O] {
	return ReaderWithError[Ctx, O]{
		Apply: func(input Ctx) (O, error) {
			return reader.Apply(input), nil
		},
	}
}

func NewReader[T, U any](convert func(T) U) Reader[T, U] {
	return Reader[T, U]{
		Apply: func(input T) U {
			return convert(input)
		},
	}
}

func NewReaderWithError[T, U any](convert func(T) (U, error)) ReaderWithError[T, U] {
	return ReaderWithError[T, U]{
		Apply: func(input T) (U, error) {
			return convert(input)
		},
	}
}

func MergeReader2[I, O1, O2, O any](
	reader1 Reader[I, O1],
	reader2 Reader[I, O2],
	combiner func(value ballerina.Tuple2[O1, O2]) O,
) Reader[I, O] {
	return Map(Parallel2(reader1, reader2), combiner)
}

func MergeReader3[I, O1, O2, O3, O any](
	reader1 Reader[I, O1],
	reader2 Reader[I, O2],
	reader3 Reader[I, O3],
	combiner func(value ballerina.Tuple3[O1, O2, O3]) O,
) Reader[I, O] {
	return Map(Parallel3(reader1, reader2, reader3), combiner)
}

func MergeReader4[I, O1, O2, O3, O4, O any](
	reader1 Reader[I, O1],
	reader2 Reader[I, O2],
	reader3 Reader[I, O3],
	reader4 Reader[I, O4],
	combiner func(value ballerina.Tuple4[O1, O2, O3, O4]) O,
) Reader[I, O] {
	return Map(Parallel4(reader1, reader2, reader3, reader4), combiner)
}

func MergeReader5[I, O1, O2, O3, O4, O5, O any](
	reader1 Reader[I, O1],
	reader2 Reader[I, O2],
	reader3 Reader[I, O3],
	reader4 Reader[I, O4],
	reader5 Reader[I, O5],
	combiner func(value ballerina.Tuple5[O1, O2, O3, O4, O5]) O,
) Reader[I, O] {
	return Map(Parallel5(reader1, reader2, reader3, reader4, reader5), combiner)
}

func MergeReader6[I, O1, O2, O3, O4, O5, O6, O any](
	reader1 Reader[I, O1],
	reader2 Reader[I, O2],
	reader3 Reader[I, O3],
	reader4 Reader[I, O4],
	reader5 Reader[I, O5],
	reader6 Reader[I, O6],
	combiner func(value ballerina.Tuple6[O1, O2, O3, O4, O5, O6]) O,
) Reader[I, O] {
	return Map(Parallel6(reader1, reader2, reader3, reader4, reader5, reader6), combiner)
}

func MergeReader7[I, O1, O2, O3, O4, O5, O6, O7, O any](
	reader1 Reader[I, O1],
	reader2 Reader[I, O2],
	reader3 Reader[I, O3],
	reader4 Reader[I, O4],
	reader5 Reader[I, O5],
	reader6 Reader[I, O6],
	reader7 Reader[I, O7],
	combiner func(value ballerina.Tuple7[O1, O2, O3, O4, O5, O6, O7]) O,
) Reader[I, O] {
	return Map(Parallel7(reader1, reader2, reader3, reader4, reader5, reader6, reader7), combiner)
}

func MergeReader8[I, O1, O2, O3, O4, O5, O6, O7, O8, O any](
	reader1 Reader[I, O1],
	reader2 Reader[I, O2],
	reader3 Reader[I, O3],
	reader4 Reader[I, O4],
	reader5 Reader[I, O5],
	reader6 Reader[I, O6],
	reader7 Reader[I, O7],
	reader8 Reader[I, O8],
	combiner func(value ballerina.Tuple8[O1, O2, O3, O4, O5, O6, O7, O8]) O,
) Reader[I, O] {
	return Map(Parallel8(reader1, reader2, reader3, reader4, reader5, reader6, reader7, reader8), combiner)
}

func MergeReader9[I, O1, O2, O3, O4, O5, O6, O7, O8, O9, O any](
	reader1 Reader[I, O1],
	reader2 Reader[I, O2],
	reader3 Reader[I, O3],
	reader4 Reader[I, O4],
	reader5 Reader[I, O5],
	reader6 Reader[I, O6],
	reader7 Reader[I, O7],
	reader8 Reader[I, O8],
	reader9 Reader[I, O9],
	combiner func(value ballerina.Tuple9[O1, O2, O3, O4, O5, O6, O7, O8, O9]) O,
) Reader[I, O] {
	return Map(Parallel9(reader1, reader2, reader3, reader4, reader5, reader6, reader7, reader8, reader9), combiner)
}

func MergeReader2WithError[I, O1, O2, O any](
	reader1 ReaderWithError[I, O1],
	reader2 ReaderWithError[I, O2],
	combiner func(value ballerina.Tuple2[O1, O2]) O,
) ReaderWithError[I, O] {
	return MapWithError(ParallelWithError2(reader1, reader2), combiner)
}

func MergeReader3WithError[I, O1, O2, O3, O any](
	reader1 ReaderWithError[I, O1],
	reader2 ReaderWithError[I, O2],
	reader3 ReaderWithError[I, O3],
	combiner func(value ballerina.Tuple3[O1, O2, O3]) O,
) ReaderWithError[I, O] {
	return MapWithError(ParallelWithError3(reader1, reader2, reader3), combiner)
}

func MergeReader4WithError[I, O1, O2, O3, O4, O any](
	reader1 ReaderWithError[I, O1],
	reader2 ReaderWithError[I, O2],
	reader3 ReaderWithError[I, O3],
	reader4 ReaderWithError[I, O4],
	combiner func(value ballerina.Tuple4[O1, O2, O3, O4]) O,
) ReaderWithError[I, O] {
	return MapWithError(ParallelWithError4(reader1, reader2, reader3, reader4), combiner)
}

func MergeReader5WithError[I, O1, O2, O3, O4, O5, O any](
	reader1 ReaderWithError[I, O1],
	reader2 ReaderWithError[I, O2],
	reader3 ReaderWithError[I, O3],
	reader4 ReaderWithError[I, O4],
	reader5 ReaderWithError[I, O5],
	combiner func(value ballerina.Tuple5[O1, O2, O3, O4, O5]) O,
) ReaderWithError[I, O] {
	return MapWithError(ParallelWithError5(reader1, reader2, reader3, reader4, reader5), combiner)
}

func MergeReader6WithError[I, O1, O2, O3, O4, O5, O6, O any](
	reader1 ReaderWithError[I, O1],
	reader2 ReaderWithError[I, O2],
	reader3 ReaderWithError[I, O3],
	reader4 ReaderWithError[I, O4],
	reader5 ReaderWithError[I, O5],
	reader6 ReaderWithError[I, O6],
	combiner func(value ballerina.Tuple6[O1, O2, O3, O4, O5, O6]) O,
) ReaderWithError[I, O] {
	return MapWithError(ParallelWithError6(reader1, reader2, reader3, reader4, reader5, reader6), combiner)
}

func MergeReader7WithError[I, O1, O2, O3, O4, O5, O6, O7, O any](
	reader1 ReaderWithError[I, O1],
	reader2 ReaderWithError[I, O2],
	reader3 ReaderWithError[I, O3],
	reader4 ReaderWithError[I, O4],
	reader5 ReaderWithError[I, O5],
	reader6 ReaderWithError[I, O6],
	reader7 ReaderWithError[I, O7],
	combiner func(value ballerina.Tuple7[O1, O2, O3, O4, O5, O6, O7]) O,
) ReaderWithError[I, O] {
	return MapWithError(ParallelWithError7(reader1, reader2, reader3, reader4, reader5, reader6, reader7), combiner)
}

func MergeReader8WithError[I, O1, O2, O3, O4, O5, O6, O7, O8, O any](
	reader1 ReaderWithError[I, O1],
	reader2 ReaderWithError[I, O2],
	reader3 ReaderWithError[I, O3],
	reader4 ReaderWithError[I, O4],
	reader5 ReaderWithError[I, O5],
	reader6 ReaderWithError[I, O6],
	reader7 ReaderWithError[I, O7],
	reader8 ReaderWithError[I, O8],
	combiner func(value ballerina.Tuple8[O1, O2, O3, O4, O5, O6, O7, O8]) O,
) ReaderWithError[I, O] {
	return MapWithError(ParallelWithError8(reader1, reader2, reader3, reader4, reader5, reader6, reader7, reader8), combiner)
}

func MergeReader9WithError[I, O1, O2, O3, O4, O5, O6, O7, O8, O9, O any](
	reader1 ReaderWithError[I, O1],
	reader2 ReaderWithError[I, O2],
	reader3 ReaderWithError[I, O3],
	reader4 ReaderWithError[I, O4],
	reader5 ReaderWithError[I, O5],
	reader6 ReaderWithError[I, O6],
	reader7 ReaderWithError[I, O7],
	reader8 ReaderWithError[I, O8],
	reader9 ReaderWithError[I, O9],
	combiner func(value ballerina.Tuple9[O1, O2, O3, O4, O5, O6, O7, O8, O9]) O,
) ReaderWithError[I, O] {
	return MapWithError(ParallelWithError9(reader1, reader2, reader3, reader4, reader5, reader6, reader7, reader8, reader9), combiner)
}
