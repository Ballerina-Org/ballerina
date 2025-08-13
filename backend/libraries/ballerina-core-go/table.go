package ballerina

type Table[T any] struct {
	Values  []T
	From    int
	To      int
	HasMore bool
}

func NewTable[T any](values []T, from int, to int, hasMore bool) Table[T] {
	return Table[T]{
		Values:  values,
		From:    from,
		To:      to,
		HasMore: hasMore,
	}
}

func MapTable[T, U any](ts Table[T], f func(T) U) Table[U] {
	us := make([]U, len(ts.Values))
	for i := range ts.Values {
		us[i] = f(ts.Values[i])
	}
	return Table[U]{Values: us, HasMore: ts.HasMore}
}
func DefaultTable[T any]() Table[T] {
	return Table[T]{Values: make([]T, 0), From: 0, To: 0, HasMore: false}
}
