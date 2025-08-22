package ballerina

func ListFoldLeft[T any, U any](list []T, initial U, f func(U, T) U) U {
	result := initial
	for _, item := range list {
		result = f(result, item)
	}
	return result
}
