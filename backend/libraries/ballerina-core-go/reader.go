package ballerina

// https://hackage.haskell.org/package/mtl-2.3.1/docs/Control-Monad-Reader.html#g:2
type Reader[Context, A any] func(Context) A

func PureReader[Context, A any](a A) Reader[Context, A] {
	return func(context Context) A {
		return a
	}
}

func MapReader[Context, A any, B any](f func(A) B) func(Reader[Context, A]) Reader[Context, B] {
	return func(reader Reader[Context, A]) Reader[Context, B] {
		return func(context Context) B {
			return f(reader(context))
		}
	}
}

type ReaderWithError[Context, A any] = Reader[Context, Sum[error, A]]

func MapReaderWithError[Context, A any, B any](f func(A) B) func(ReaderWithError[Context, A]) ReaderWithError[Context, B] {
	return func(a ReaderWithError[Context, A]) ReaderWithError[Context, B] {
		return func(context Context) Sum[error, B] {
			return BiMap(
				a(context),
				id[error],
				f,
			)
		}
	}
}

func BindReaderWithError[Context, A any, B any](f func(A) ReaderWithError[Context, B]) func(ReaderWithError[Context, A]) ReaderWithError[Context, B] {
	return func(a ReaderWithError[Context, A]) ReaderWithError[Context, B] {
		return func(context Context) Sum[error, B] {
			return Fold(
				a(context),
				Left[error, B],
				func(a A) Sum[error, B] {
					return f(a)(context)
				},
			)
		}
	}
}
