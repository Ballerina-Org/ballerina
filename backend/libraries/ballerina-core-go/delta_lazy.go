package ballerina

import (
	"encoding/json"
	"fmt"
)

type deltaLazyEffectsEnum string

const (
	lazyValue deltaLazyEffectsEnum = "LazyValue"
)

type DeltaLazy[a any, deltaA any] struct {
	DeltaBase
	discriminator deltaLazyEffectsEnum
	value         *deltaA
}

var _ json.Unmarshaler = &DeltaLazy[Unit, Unit]{}
var _ json.Marshaler = DeltaLazy[Unit, Unit]{}

func (d DeltaLazy[a, deltaA]) MarshalJSON() ([]byte, error) {
	return json.Marshal(struct {
		DeltaBase
		Discriminator deltaLazyEffectsEnum
		Value         *deltaA
	}{
		DeltaBase:     d.DeltaBase,
		Discriminator: d.discriminator,
		Value:         d.value,
	})
}

func (d *DeltaLazy[a, deltaA]) UnmarshalJSON(data []byte) error {
	var tmp struct {
		DeltaBase
		Discriminator deltaLazyEffectsEnum
		Value         *deltaA
	}
	if err := json.Unmarshal(data, &tmp); err != nil {
		return err
	}
	d.DeltaBase = tmp.DeltaBase
	d.discriminator = tmp.Discriminator
	d.value = tmp.Value
	return nil
}

func NewDeltaLazyValue[a any, deltaA any](value deltaA) DeltaLazy[a, deltaA] {
	return DeltaLazy[a, deltaA]{
		discriminator: lazyValue,
		value:         &value,
	}
}

func MatchDeltaLazy[a any, deltaA any, Result any](
	onValue func(deltaA) func(ReaderWithError[Unit, a]) (Result, error),
) func(DeltaLazy[a, deltaA]) func(ReaderWithError[Unit, Lazy[a]]) (Result, error) {
	return func(delta DeltaLazy[a, deltaA]) func(ReaderWithError[Unit, Lazy[a]]) (Result, error) {
		return func(lazy ReaderWithError[Unit, Lazy[a]]) (Result, error) {
			switch delta.discriminator {
			case lazyValue:
				value := BindReaderWithError[Unit, Lazy[a], a](
					func(lazy Lazy[a]) ReaderWithError[Unit, a] {
						return PureReader[Unit, Sum[error, a]](Left[error, a](fmt.Errorf("cannot get current value as it is lazy")))
					},
				)(lazy)
				return onValue(*delta.value)(value)
			}
			var result Result
			return result, NewInvalidDiscriminatorError(string(delta.discriminator), "DeltaLazy")
		}
	}
}
