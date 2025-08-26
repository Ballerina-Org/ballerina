package ballerina

func ListFoldLeft[T any, U any](list []T, initial U, f func(U, T) U) U {
	result := initial
	for _, item := range list {
		result = f(result, item)
	}
	return result
}

func ListMap[T any, U any](list []T, f func(T) U) []U {
	return ListFoldLeft(list, []U{}, func(acc []U, item T) []U {
		return append(acc, f(item))
	})
}
