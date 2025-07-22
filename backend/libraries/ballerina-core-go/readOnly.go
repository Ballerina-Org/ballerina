package ballerina

type ReadOnly[T any] struct {
	ReadOnly T
}

type DeltaReadOnly[a any, deltaA any] struct {
	DeltaBase
}

func DefaultReadOnly[T any](value T) ReadOnly[T] {
	return ReadOnly[T]{ReadOnly: value}
}

func NewReadOnly[T any](t T) ReadOnly[T] {
	return ReadOnly[T]{
		ReadOnly: t,
	}
}

func MatchDeltaReadOnly[a any, deltaA any, Result any]() func(DeltaReadOnly[a, deltaA]) (Result, error) {
	return func(delta DeltaReadOnly[a, deltaA]) (Result, error) {
		var result Result
		return result, NewReadOnlyDeltaCalledError()
	}
}
