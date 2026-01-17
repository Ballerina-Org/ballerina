package ballerina

type One[a any] struct {
	Value a // NOTE: struct embedding is needed to be able to access Sum's JSON methods
}

func NewOne[a any](value a) One[a] {
	return One[a]{value}
}

func MapOne[a any, b any](self One[a], f func(a) b) One[b] {
	return One[b]{f(self.Value)}
}
