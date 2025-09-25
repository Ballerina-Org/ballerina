package ballerinalazy

import (
	"encoding/json"

	ballerina "ballerina.com/core"
)

type lazyTuple2EffectsEnum string

const (
	tuple2Item1 lazyTuple2EffectsEnum = "Tuple2Item1"
	tuple2Item2 lazyTuple2EffectsEnum = "Tuple2Item2"
)

type LazyTuple2[lazyA any, lazyB any] struct {
	discriminator lazyTuple2EffectsEnum
	item1         *lazyA
	item2         *lazyB
}

var _ json.Unmarshaler = &LazyTuple2[ballerina.Unit, ballerina.Unit]{}
var _ json.Marshaler = LazyTuple2[ballerina.Unit, ballerina.Unit]{}

func (v *LazyTuple2[deltaA, deltaB]) UnmarshalJSON(data []byte) error {
	var tmp struct {
		Discriminator lazyTuple2EffectsEnum
		Item1         *deltaA
		Item2         *deltaB
	}
	if err := json.Unmarshal(data, &tmp); err != nil {
		return err
	}
	v.discriminator = tmp.Discriminator
	v.item1 = tmp.Item1
	v.item2 = tmp.Item2
	return nil
}

func (v LazyTuple2[deltaA, deltaB]) MarshalJSON() ([]byte, error) {
	return json.Marshal(struct {
		Discriminator lazyTuple2EffectsEnum
		Item1         *deltaA
		Item2         *deltaB
	}{
		Discriminator: v.discriminator,
		Item1:         v.item1,
		Item2:         v.item2,
	})
}

func NewLazyTuple2Item1[deltaA any, deltaB any](lazy deltaA) LazyTuple2[deltaA, deltaB] {
	return LazyTuple2[deltaA, deltaB]{
		discriminator: tuple2Item1,
		item1:         &lazy,
	}
}
func NewLazyTuple2Item2[deltaA any, deltaB any](lazy deltaB) LazyTuple2[deltaA, deltaB] {
	return LazyTuple2[deltaA, deltaB]{
		discriminator: tuple2Item2,
		item2:         &lazy,
	}
}

func MatchLazyTuple2[Context any, lazyA any, lazyB any, Result any](
	onItem1 func(lazyA) ballerina.ReaderWithError[Context, Result],
	onItem2 func(lazyB) ballerina.ReaderWithError[Context, Result],
) func(LazyTuple2[lazyA, lazyB]) ballerina.ReaderWithError[Context, Result] {
	return func(lazy LazyTuple2[lazyA, lazyB]) ballerina.ReaderWithError[Context, Result] {
		switch lazy.discriminator {
		case tuple2Item1:
			return onItem1(*lazy.item1)
		case tuple2Item2:
			return onItem2(*lazy.item2)
		}
		return ballerina.PureReader[Context, ballerina.Sum[error, Result]](ballerina.Left[error, Result](ballerina.NewInvalidDiscriminatorError(string(lazy.discriminator), "LazyTuple2")))
	}
}
