package ballerina

import (
	"encoding/json"
)

type DeltaOneEffectsEnum string

const (
	OneReplace     DeltaOneEffectsEnum = "OneReplace"
	OneValue       DeltaOneEffectsEnum = "OneValue"
	OneCreateValue DeltaOneEffectsEnum = "OneCreateValue"
	OneDeleteValue DeltaOneEffectsEnum = "OneDeleteValue"
)

var AllDeltaOneEffectsEnumCases = [...]DeltaOneEffectsEnum{OneReplace, OneValue, OneCreateValue, OneDeleteValue}

func DefaultDeltaOneEffectsEnum() DeltaOneEffectsEnum { return AllDeltaOneEffectsEnumCases[0] }

type DeltaOne[a any, deltaA any] struct {
	DeltaBase
	discriminator DeltaOneEffectsEnum
	replace       *a
	value         *deltaA
	createValue   *a
	deleteValue   *Unit
}

var _ json.Unmarshaler = &DeltaOne[Unit, Unit]{}
var _ json.Marshaler = DeltaOne[Unit, Unit]{}

func (d DeltaOne[a, deltaA]) MarshalJSON() ([]byte, error) {
	return json.Marshal(struct {
		DeltaBase
		Discriminator DeltaOneEffectsEnum
		Replace       *a
		Value         *deltaA
		CreateValue   *a
		DeleteValue   *Unit
	}{
		DeltaBase:     d.DeltaBase,
		Discriminator: d.discriminator,
		Replace:       d.replace,
		Value:         d.value,
		CreateValue:   d.createValue,
		DeleteValue:   d.deleteValue,
	})
}

func (d *DeltaOne[a, deltaA]) UnmarshalJSON(data []byte) error {
	var tmp struct {
		DeltaBase
		Discriminator DeltaOneEffectsEnum
		Replace       *a
		Value         *deltaA
		CreateValue   *a
		DeleteValue   *Unit
	}
	if err := json.Unmarshal(data, &tmp); err != nil {
		return err
	}
	d.DeltaBase = tmp.DeltaBase
	d.discriminator = tmp.Discriminator
	d.replace = tmp.Replace
	d.value = tmp.Value
	d.createValue = tmp.CreateValue
	d.deleteValue = tmp.DeleteValue
	return nil
}

func NewDeltaOneReplace[a any, deltaA any](value a) DeltaOne[a, deltaA] {
	return DeltaOne[a, deltaA]{
		discriminator: OneReplace,
		replace:       &value,
	}
}
func NewDeltaOneValue[a any, deltaA any](delta deltaA) DeltaOne[a, deltaA] {
	return DeltaOne[a, deltaA]{
		discriminator: OneValue,
		value:         &delta,
	}
}
func NewDeltaOneCreateValue[a any, deltaA any](value a) DeltaOne[a, deltaA] {
	return DeltaOne[a, deltaA]{
		discriminator: OneCreateValue,
		createValue:   &value,
	}
}
func NewDeltaOneDeleteValue[a any, deltaA any]() DeltaOne[a, deltaA] {
	unit := NewUnit()
	return DeltaOne[a, deltaA]{
		discriminator: OneDeleteValue,
		deleteValue:   &unit,
	}
}
func MatchDeltaOne[a any, deltaA any, Result any](
	onReplace func(a) (Result, error),
	onValue func(deltaA) (Result, error),
	onCreateValue func(a) (Result, error),
	onDeleteValue func() (Result, error),
) func(DeltaOne[a, deltaA]) (Result, error) {
	return func(delta DeltaOne[a, deltaA]) (Result, error) {
		var result Result
		switch delta.discriminator {
		case OneReplace:
			if delta.replace == nil {
				return result, NewInvalidDiscriminatorError("nil replace", "DeltaOne")
			}
			return onReplace(*delta.replace)
		case OneValue:
			if delta.value == nil {
				return result, NewInvalidDiscriminatorError("nil value", "DeltaOne")
			}
			return onValue(*delta.value)
		case OneCreateValue:
			if delta.createValue == nil {
				return result, NewInvalidDiscriminatorError("nil createValue", "DeltaOne")
			}
			return onCreateValue(*delta.createValue)
		case OneDeleteValue:
			return onDeleteValue()
		}
		return result, NewInvalidDiscriminatorError(string(delta.discriminator), "DeltaOne")
	}
}
