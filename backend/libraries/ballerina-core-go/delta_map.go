package ballerina

import (
	"encoding/json"
	"fmt"
)

type deltaMapEffectsEnum string

const (
	mapKey    deltaMapEffectsEnum = "MapKey"
	mapValue  deltaMapEffectsEnum = "MapValue"
	mapAdd    deltaMapEffectsEnum = "MapAdd"
	mapRemove deltaMapEffectsEnum = "MapRemove"
)

type DeltaMap[k comparable, v any, deltaK any, deltaV any] struct {
	DeltaBase
	discriminator deltaMapEffectsEnum
	key           *Tuple2[k, deltaK]
	value         *Tuple2[k, deltaV]
	add           *Tuple2[k, v]
	remove        *k
}

var _ json.Unmarshaler = &DeltaMap[Unit, Unit, Unit, Unit]{}
var _ json.Marshaler = DeltaMap[Unit, Unit, Unit, Unit]{}

func (d DeltaMap[k, v, deltaK, deltaV]) MarshalJSON() ([]byte, error) {
	return json.Marshal(struct {
		DeltaBase
		Discriminator deltaMapEffectsEnum
		Key           *Tuple2[k, deltaK]
		Value         *Tuple2[k, deltaV]
		Add           *Tuple2[k, v]
		Remove        *k
	}{
		DeltaBase:     d.DeltaBase,
		Discriminator: d.discriminator,
		Key:           d.key,
		Value:         d.value,
		Add:           d.add,
		Remove:        d.remove,
	})
}

func (d *DeltaMap[k, v, deltaK, deltaV]) UnmarshalJSON(data []byte) error {
	var a struct {
		DeltaBase
		Discriminator deltaMapEffectsEnum
		Key           *Tuple2[k, deltaK]
		Value         *Tuple2[k, deltaV]
		Add           *Tuple2[k, v]
		Remove        *k
	}
	if err := json.Unmarshal(data, &a); err != nil {
		return err
	}
	d.DeltaBase = a.DeltaBase
	d.discriminator = a.Discriminator
	d.key = a.Key
	d.value = a.Value
	d.add = a.Add
	d.remove = a.Remove
	return nil
}

func NewDeltaMapKey[k comparable, v any, deltaK any, deltaV any](key k, delta deltaK) DeltaMap[k, v, deltaK, deltaV] {
	t := NewTuple2(key, delta)
	return DeltaMap[k, v, deltaK, deltaV]{
		discriminator: mapKey,
		key:           &t,
	}
}
func NewDeltaMapValue[k comparable, v any, deltaK any, deltaV any](key k, delta deltaV) DeltaMap[k, v, deltaK, deltaV] {
	t := NewTuple2(key, delta)
	return DeltaMap[k, v, deltaK, deltaV]{
		discriminator: mapValue,
		value:         &t,
	}
}
func NewDeltaMapAdd[k comparable, v any, deltaK any, deltaV any](newElement Tuple2[k, v]) DeltaMap[k, v, deltaK, deltaV] {
	return DeltaMap[k, v, deltaK, deltaV]{
		discriminator: mapAdd,
		add:           &newElement,
	}
}
func NewDeltaMapRemove[k comparable, v any, deltaK any, deltaV any](key k) DeltaMap[k, v, deltaK, deltaV] {
	return DeltaMap[k, v, deltaK, deltaV]{
		discriminator: mapRemove,
		remove:        &key,
	}
}

func MatchDeltaMap[k comparable, v any, deltaK any, deltaV any, Result any](
	onKey func(Tuple2[k, deltaK]) func(ReaderWithError[Unit, k]) (Result, error),
	onValue func(Tuple2[k, deltaV]) func(ReaderWithError[Unit, v]) (Result, error),
	onAdd func(Tuple2[k, v]) (Result, error),
	onRemove func(k) (Result, error),
) func(DeltaMap[k, v, deltaK, deltaV]) func(ReaderWithError[Unit, Map[k, v]]) (Result, error) {
	return func(delta DeltaMap[k, v, deltaK, deltaV]) func(ReaderWithError[Unit, Map[k, v]]) (Result, error) {
		return func(mapReader ReaderWithError[Unit, Map[k, v]]) (Result, error) {
			var result Result
			switch delta.discriminator {
			case mapKey:
				key := BindReaderWithError(
					func(m Map[k, v]) ReaderWithError[Unit, k] {
						expectedKey := delta.key.Item1
						return PureReader[Unit, Sum[error, k]](
							MatchOption(
								m.Get(expectedKey),
								func(_ v) Sum[error, k] {
									return Right[error, k](expectedKey)
								},
								func() Sum[error, k] {
									return Left[error, k](fmt.Errorf("key %v not found in current map value", expectedKey))
								},
							),
						)
					},
				)(mapReader)
				return onKey(*delta.key)(key)
			case mapValue:
				value := BindReaderWithError(
					func(m Map[k, v]) ReaderWithError[Unit, v] {
						return PureReader[Unit, Sum[error, v]](
							MatchOption(
								m.Get(delta.value.Item1),
								Right[error, v],
								func() Sum[error, v] {
									return Left[error, v](fmt.Errorf("key %v not found in current map value", delta.value.Item1))
								},
							),
						)
					},
				)(mapReader)
				return onValue(*delta.value)(value)
			case mapAdd:
				return onAdd(*delta.add)
			case mapRemove:
				return onRemove(*delta.remove)
			}
			return result, NewInvalidDiscriminatorError(string(delta.discriminator), "DeltaMap")
		}
	}
}
