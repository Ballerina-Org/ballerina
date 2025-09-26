package ballerina

type Lazy[a any] struct{}

func NewLazy[a any]() Lazy[a] {
	return Lazy[a]{}
}

func MapLazy[a any, b any](self Lazy[a], f func(a) b) Lazy[b] {
	return Lazy[b]{}
}
