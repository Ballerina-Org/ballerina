package ballerinalazy

import (
	"bytes"
	"encoding/json"

	ballerina "ballerina.com/core"
)

type lazySumEffectsEnum string

const (
	sumLeft  lazySumEffectsEnum = "SumLeft"
	sumRight lazySumEffectsEnum = "SumRight"
)

type LazySum[lazyA any, lazyB any] struct {
	discriminator lazySumEffectsEnum
	left          *lazyA
	right         *lazyB
}

var _ json.Unmarshaler = &LazySum[ballerina.Unit, ballerina.Unit]{}
var _ json.Marshaler = LazySum[ballerina.Unit, ballerina.Unit]{}

func (d LazySum[lazyA, lazyB]) MarshalJSON() ([]byte, error) {
	return json.Marshal(struct {
		Discriminator lazySumEffectsEnum
		Left          *lazyA
		Right         *lazyB
	}{
		Discriminator: d.discriminator,
		Left:          d.left,
		Right:         d.right,
	})
}

func (d *LazySum[lazyA, lazyB]) UnmarshalJSON(data []byte) error {
	var aux struct {
		Discriminator lazySumEffectsEnum
		Left          *lazyA
		Right         *lazyB
	}
	dec := json.NewDecoder(bytes.NewReader(data))
	dec.DisallowUnknownFields()
	if err := dec.Decode(&aux); err != nil {
		return err
	}
	d.discriminator = aux.Discriminator
	d.left = aux.Left
	d.right = aux.Right
	return nil
}
func NewLazySumLeft[lazyA any, lazyB any](lazy lazyA) LazySum[lazyA, lazyB] {
	return LazySum[lazyA, lazyB]{
		discriminator: sumLeft,
		left:          &lazy,
	}
}
func NewLazySumRight[lazyA any, lazyB any](lazy lazyB) LazySum[lazyA, lazyB] {
	return LazySum[lazyA, lazyB]{
		discriminator: sumRight,
		right:         &lazy,
	}
}

func MatchLazySum[Context any, lazyA any, lazyB any, Result any](
	onLeft func(lazyA) ballerina.ReaderWithError[Context, Result],
	onRight func(lazyB) ballerina.ReaderWithError[Context, Result],
) func(LazySum[lazyA, lazyB]) ballerina.ReaderWithError[Context, Result] {
	return func(lazy LazySum[lazyA, lazyB]) ballerina.ReaderWithError[Context, Result] {
		switch lazy.discriminator {
		case sumLeft:
			return onLeft(*lazy.left)
		case sumRight:
			return onRight(*lazy.right)
		}
		return ballerina.PureReader[Context, ballerina.Sum[error, Result]](ballerina.Left[error, Result](ballerina.NewInvalidDiscriminatorError(string(lazy.discriminator), "LazySum")))
	}
}
