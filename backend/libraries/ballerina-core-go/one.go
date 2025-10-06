package ballerina

type LazyOneValue Unit

type One[a any] struct {
	Sum[LazyOneValue, a] // NOTE: struct embedding is needed to be able to access Sum's JSON methods
}

// type some[a any] struct { One[a]; Value a; Kind string }
func NewLoadedOne[a any](value a) One[a] {
	return One[a]{Right[LazyOneValue, a](value)}
}

// type none[a any] struct { One[a]; Kind string }
func NewLazyOne[a any]() One[a] {
	return One[a]{Left[LazyOneValue, a](LazyOneValue(NewUnit()))}
}

// func MatchOne[a any, c any](self One[a], onSome func(a) c, onNone func() c) c {
// 	return Fold(self.Sum, func(_ Unit) c { return onNone() }, onSome)
// }

func MapOne[a any, b any](self One[a], f func(a) b) One[b] {
	return Fold(
		self.Sum,
		func(_ LazyOneValue) One[b] { return NewLazyOne[b]() },
		func(value a) One[b] { return NewLoadedOne[b](f(value)) },
	)
}
