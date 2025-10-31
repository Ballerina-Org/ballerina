package ballerina

func ReturningNilError[A, B any](f func(A) B) func(A) (B, error) {
	return func(a A) (B, error) {
		return f(a), nil
	}
}

func Identity[A any](a A) A {
	return a
}

func Then[A, B, C any](f func(A) B, g func(B) C) func(A) C {
	return func(a A) C {
		return g(f(a))
	}
}

func ThenWithError[A, B, C any](f func(A) (B, error), g func(B) (C, error)) func(A) (C, error) {
	return func(a A) (C, error) {
		b, err := f(a)
		if err != nil {
			return *new(C), err
		}
		return g(b)
	}
}
