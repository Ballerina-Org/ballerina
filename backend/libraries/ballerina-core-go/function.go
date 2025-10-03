package ballerina

func ReturningNilError[A, B any](f func(A) B) func(A) (B, error) {
	return func(a A) (B, error) {
		return f(a), nil
	}
}

func Identity[A any](a A) A {
	return a
}
