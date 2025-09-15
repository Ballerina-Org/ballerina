package ballerina

type ReadOnly[T any] struct {
	ReadOnly T
}

func NewReadOnly[T any](t T) ReadOnly[T] {
	return ReadOnly[T]{
		ReadOnly: t,
	}
}
