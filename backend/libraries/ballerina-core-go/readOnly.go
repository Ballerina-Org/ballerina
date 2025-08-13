package ballerina

type ReadOnly[T any] struct {
	ReadOnly T
}

func DefaultReadOnly[T any](value T) ReadOnly[T] {
	return ReadOnly[T]{ReadOnly: value}
}

func NewReadOnly[T any](t T) ReadOnly[T] {
	return ReadOnly[T]{
		ReadOnly: t,
	}
}
