package ballerina

import (
	"encoding/json"
)

type deltaMapEffectsEnum string

const (
	mapKey    deltaMapEffectsEnum = "MapKey"
	mapValue  deltaMapEffectsEnum = "MapValue"
	mapAdd    deltaMapEffectsEnum = "MapAdd"
	mapRemove deltaMapEffectsEnum = "MapRemove"
)

var allDeltaMapEffectsEnumCases = [...]deltaMapEffectsEnum{mapKey, mapValue, mapAdd, mapRemove}

func DefaultDeltaMapEffectsEnum() deltaMapEffectsEnum { return allDeltaMapEffectsEnumCases[0] }

type DeltaMap[k comparable, v any, deltaK any, deltaV any] struct {
	DeltaBase
	discriminator deltaMapEffectsEnum
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
		Discriminator deltaMapEffectsEnum
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
		Discriminator deltaMapEffectsEnum
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
		discriminator: mapKey,
		key:           &t,
	}
}
func NewDeltaMapValue[k comparable, v any, deltaK any, deltaV any](index int, delta deltaV) DeltaMap[k, v, deltaK, deltaV] {
	t := NewTuple2(index, delta)
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
func NewDeltaMapRemove[k comparable, v any, deltaK any, deltaV any](index int) DeltaMap[k, v, deltaK, deltaV] {
	return DeltaMap[k, v, deltaK, deltaV]{
		discriminator: mapRemove,
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
		case mapKey:
			return onKey(*delta.key)
		case mapValue:
			return onValue(*delta.value)
		case mapAdd:
			return onAdd(*delta.add)
		case mapRemove:
			return onRemove(*delta.remove)
		}
		return result, NewInvalidDiscriminatorError(string(delta.discriminator), "DeltaMap")
	}
}
