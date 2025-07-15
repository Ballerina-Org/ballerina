package ballerina

type ReadOnly[T any] struct {
	ReadOnly T
}

type DeltaReadOnly[a any, deltaA any] struct {
	DeltaBase
}

func DefaultReadOnly[T any]() ReadOnly[T] {
	return ReadOnly[T]{}
}

func MatchDeltaReadOnly[a any, deltaA any, Result any]() func(DeltaReadOnly[a, deltaA]) (Result, error) {
	return func(delta DeltaReadOnly[a, deltaA]) (Result, error) {
		var result Result
		return result, NewReadOnlyDeltaCalledError()
	}
}
