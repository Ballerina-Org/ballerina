package ballerina

type Array[T any] []T

func MapArray[T, U any](ts []T, f func(T) U) []U {
	us := make([]U, len(ts))
	for i := range ts {
		us[i] = f(ts[i])
	}
	return us
}
func DefaultArray[T any]() Array[T] {
	var res Array[T] = make([]T, 0)
	return res
}
