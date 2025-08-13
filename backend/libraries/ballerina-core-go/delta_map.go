package ballerina

import (
	"encoding/json"
)

type DeltaMapEffectsEnum string

const (
	MapKey    DeltaMapEffectsEnum = "MapKey"
	MapValue  DeltaMapEffectsEnum = "MapValue"
	MapAdd    DeltaMapEffectsEnum = "MapAdd"
	MapRemove DeltaMapEffectsEnum = "MapRemove"
)

var AllDeltaMapEffectsEnumCases = [...]DeltaMapEffectsEnum{MapKey, MapValue, MapAdd, MapRemove}

func DefaultDeltaMapEffectsEnum() DeltaMapEffectsEnum { return AllDeltaMapEffectsEnumCases[0] }

type DeltaMap[k comparable, v any, deltaK any, deltaV any] struct {
	DeltaBase
	discriminator DeltaMapEffectsEnum
	key           *Tuple2[int, deltaK]
	value         *Tuple2[int, deltaV]
	add           *Tuple2[k, v]
	remove        *int
}

var _ json.Unmarshaler = &DeltaMap[Unit, Unit, Unit, Unit]{}
var _ json.Marshaler = DeltaMap[Unit, Unit, Unit, Unit]{}

func (d DeltaMap[k, v, deltaK, deltaV]) MarshalJSON() ([]byte, error) {
	return json.Marshal(struct {
		DeltaBase
		Discriminator DeltaMapEffectsEnum
		Key           *Tuple2[int, deltaK]
		Value         *Tuple2[int, deltaV]
		Add           *Tuple2[k, v]
		Remove        *int
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
		Discriminator DeltaMapEffectsEnum
		Key           *Tuple2[int, deltaK]
		Value         *Tuple2[int, deltaV]
		Add           *Tuple2[k, v]
		Remove        *int
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

func NewDeltaMapKey[k comparable, v any, deltaK any, deltaV any](index int, delta deltaK) DeltaMap[k, v, deltaK, deltaV] {
	t := NewTuple2(index, delta)
	return DeltaMap[k, v, deltaK, deltaV]{
		discriminator: MapKey,
		key:           &t,
	}
}
func NewDeltaMapValue[k comparable, v any, deltaK any, deltaV any](index int, delta deltaV) DeltaMap[k, v, deltaK, deltaV] {
	t := NewTuple2(index, delta)
	return DeltaMap[k, v, deltaK, deltaV]{
		discriminator: MapValue,
		value:         &t,
	}
}
func NewDeltaMapAdd[k comparable, v any, deltaK any, deltaV any](newElement Tuple2[k, v]) DeltaMap[k, v, deltaK, deltaV] {
	return DeltaMap[k, v, deltaK, deltaV]{
		discriminator: MapAdd,
		add:           &newElement,
	}
}
func NewDeltaMapRemove[k comparable, v any, deltaK any, deltaV any](index int) DeltaMap[k, v, deltaK, deltaV] {
	return DeltaMap[k, v, deltaK, deltaV]{
		discriminator: MapRemove,
		remove:        &index,
	}
}

func MatchDeltaMap[k comparable, v any, deltaK any, deltaV any, Result any](
	onKey func(Tuple2[int, deltaK]) (Result, error),
	onValue func(Tuple2[int, deltaV]) (Result, error),
	onAdd func(Tuple2[k, v]) (Result, error),
	onRemove func(int) (Result, error),
) func(DeltaMap[k, v, deltaK, deltaV]) (Result, error) {
	return func(delta DeltaMap[k, v, deltaK, deltaV]) (Result, error) {
		var result Result
		switch delta.discriminator {
		case MapKey:
			return onKey(*delta.key)
		case MapValue:
			return onValue(*delta.value)
		case MapAdd:
			return onAdd(*delta.add)
		case MapRemove:
			return onRemove(*delta.remove)
		}
		return result, NewInvalidDiscriminatorError(string(delta.discriminator), "DeltaMap")
	}
}
