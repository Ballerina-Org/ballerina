package ballerina

type ManyItem[T any] struct {
	Value         T
	IsLinked      bool
	CanChangeLink bool
}

func NewManyItem[T any](value T, isLinked bool, canChangeLink bool) ManyItem[T] {
	return ManyItem[T]{
		Value:         value,
		IsLinked:      isLinked,
		CanChangeLink: canChangeLink,
	}
}

func MapManyItem[T, U any](item ManyItem[T], f func(T) U) ManyItem[U] {
	return ManyItem[U]{
		Value:         f(item.Value),
		IsLinked:      item.IsLinked,
		CanChangeLink: item.CanChangeLink,
	}
}

func DefaultManyItem[T any](value T) ManyItem[T] {
	return ManyItem[T]{Value: value, IsLinked: false, CanChangeLink: true}
}
