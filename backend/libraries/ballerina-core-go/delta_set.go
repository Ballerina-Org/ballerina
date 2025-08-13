package ballerina

import "encoding/json"

type DeltaSetEffectsEnum string

const (
	SetValue  DeltaSetEffectsEnum = "SetValue"
	SetAdd    DeltaSetEffectsEnum = "SetAdd"
	SetRemove DeltaSetEffectsEnum = "SetRemove"
)

var AllDeltaSetEffectsEnumCases = [...]DeltaSetEffectsEnum{SetValue, SetAdd, SetRemove}

func DefaultDeltaSetEffectsEnum() DeltaSetEffectsEnum { return AllDeltaSetEffectsEnumCases[0] }

type DeltaSet[a comparable, deltaA any] struct {
	DeltaBase
	discriminator DeltaSetEffectsEnum
	value         *Tuple2[a, deltaA]
	add           *a
	remove        *a
}

var _ json.Unmarshaler = &DeltaSet[Unit, Unit]{}
var _ json.Marshaler = DeltaSet[Unit, Unit]{}

func (d DeltaSet[a, deltaA]) MarshalJSON() ([]byte, error) {
	return json.Marshal(struct {
		DeltaBase
		Discriminator DeltaSetEffectsEnum
		Value         *Tuple2[a, deltaA]
		Add           *a
		Remove        *a
	}{
		DeltaBase:     d.DeltaBase,
		Discriminator: d.discriminator,
		Value:         d.value,
		Add:           d.add,
		Remove:        d.remove,
	})
}

func (d *DeltaSet[a, deltaA]) UnmarshalJSON(data []byte) error {
	var aux struct {
		DeltaBase
		Discriminator DeltaSetEffectsEnum
		Value         *Tuple2[a, deltaA]
		Add           *a
		Remove        *a
	}
	if err := json.Unmarshal(data, &aux); err != nil {
		return err
	}
	d.DeltaBase = aux.DeltaBase
	d.discriminator = aux.Discriminator
	d.value = aux.Value
	d.add = aux.Add
	d.remove = aux.Remove
	return nil
}

func NewDeltaSetValue[a comparable, deltaA any](index a, delta deltaA) DeltaSet[a, deltaA] {
	tmp := NewTuple2(index, delta)
	return DeltaSet[a, deltaA]{
		discriminator: SetValue,
		value:         &tmp,
	}
}
func NewDeltaSetAdd[a comparable, deltaA any](newElement a) DeltaSet[a, deltaA] {
	return DeltaSet[a, deltaA]{
		discriminator: SetAdd,
		add:           &newElement,
	}
}
func NewDeltaSetRemove[a comparable, deltaA any](element a) DeltaSet[a, deltaA] {
	return DeltaSet[a, deltaA]{
		discriminator: SetRemove,
		remove:        &element,
	}
}

func MatchDeltaSet[a comparable, deltaA any, Result any](
	onValue func(Tuple2[a, deltaA]) (Result, error),
	onAdd func(a) (Result, error),
	onRemove func(a) (Result, error),
) func(DeltaSet[a, deltaA]) (Result, error) {
	return func(delta DeltaSet[a, deltaA]) (Result, error) {
		var result Result
		switch delta.discriminator {
		case SetValue:
			return onValue(*delta.value)
		case SetAdd:
			return onAdd(*delta.add)
		case SetRemove:
			return onRemove(*delta.remove)
		}
		return result, NewInvalidDiscriminatorError(string(delta.discriminator), "DeltaSet")
	}
}
