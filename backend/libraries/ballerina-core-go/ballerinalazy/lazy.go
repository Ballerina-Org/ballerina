package ballerinalazy

import (
	"fmt"

	ballerina "ballerina.com/core"
)

type Lazy[T any] ballerina.Unit

func NewLazy[T any]() Lazy[T] {
	return Lazy[T](ballerina.NewUnit())
}

type NotLazy interface {
	never()
}

func MatchNotLazy[Context any, Result any]() func(NotLazy) ballerina.ReaderWithError[Context, Result] {
	return func(notLazy NotLazy) ballerina.ReaderWithError[Context, Result] {
		return ballerina.PureReader[Context, ballerina.Sum[error, Result]](
			ballerina.Left[error, Result](
				fmt.Errorf("notLazy called"),
			),
		)
	}
}

type LazyLoader[Context any, T any] = ballerina.ReaderWithError[Context, T]

type loadOrRecurseEffectsEnum string

const (
	load    loadOrRecurseEffectsEnum = "Load"
	recurse loadOrRecurseEffectsEnum = "Recurse"
)

type LoadOrRecurse[Value any, InnerLazy any] struct {
	discriminator loadOrRecurseEffectsEnum
	load          *ballerina.Unit
	recurse       *InnerLazy
}

func MatchLoadOrRecurse[Context any, Value any, InnerLazy any, Result any](
	toResult func(Value) ballerina.Sum[error, Result],
	onLoad ballerina.ReaderWithError[Context, Value],
	onRecurse func(InnerLazy) ballerina.ReaderWithError[Context, Result],
) func(LoadOrRecurse[Value, InnerLazy]) ballerina.ReaderWithError[Context, Result] {
	return func(loadOrRecurse LoadOrRecurse[Value, InnerLazy]) ballerina.ReaderWithError[Context, Result] {
		switch loadOrRecurse.discriminator {
		case load:
			return ballerina.BindReaderWithError[Context, Value, Result](
				func(value Value) ballerina.ReaderWithError[Context, Result] {
					return ballerina.PureReader[Context, ballerina.Sum[error, Result]](toResult(value))
				},
			)(onLoad)
		case recurse:
			return onRecurse(*loadOrRecurse.recurse)
		}
		return ballerina.PureReader[Context, ballerina.Sum[error, Result]](ballerina.Left[error, Result](ballerina.NewInvalidDiscriminatorError(string(loadOrRecurse.discriminator), "LoadOrRecurse")))
	}
}
