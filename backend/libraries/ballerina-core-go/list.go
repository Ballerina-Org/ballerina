package ballerina

func ListFoldLeft[T any, U any](list []T, initial U, f func(U, T) U) U {
	result := initial
	for _, item := range list {
		result = f(result, item)
	}
	return result
}

func ListMap[T any, U any](list []T, f func(T) U) []U {
	result := make([]U, len(list))
	for i, item := range list {
		result[i] = f(item)
	}
	return result
}
