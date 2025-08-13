package ballerina

import (
	"encoding/json"
)

type DeltaOptionEffectsEnum string

const (
	OptionReplace DeltaOptionEffectsEnum = "OptionReplace"
	OptionValue   DeltaOptionEffectsEnum = "OptionValue"
)

var AllDeltaOptionEffectsEnumCases = [...]DeltaOptionEffectsEnum{OptionReplace, OptionValue}

func DefaultDeltaOptionEffectsEnum() DeltaOptionEffectsEnum { return AllDeltaOptionEffectsEnumCases[0] }

type DeltaOption[a any, deltaA any] struct {
	DeltaBase
	discriminator DeltaOptionEffectsEnum
	replace       *a
	value         *deltaA
}

var _ json.Unmarshaler = &DeltaOption[Unit, Unit]{}
var _ json.Marshaler = DeltaOption[Unit, Unit]{}

func (d DeltaOption[a, deltaA]) MarshalJSON() ([]byte, error) {
	return json.Marshal(struct {
		DeltaBase
		Discriminator DeltaOptionEffectsEnum
		Replace       *a
		Value         *deltaA
	}{
		DeltaBase:     d.DeltaBase,
		Discriminator: d.discriminator,
		Replace:       d.replace,
		Value:         d.value,
	})
}

func (d *DeltaOption[a, deltaA]) UnmarshalJSON(data []byte) error {
	var tmp struct {
		DeltaBase
		Discriminator DeltaOptionEffectsEnum
		Replace       *a
		Value         *deltaA
	}
	if err := json.Unmarshal(data, &tmp); err != nil {
		return err
	}
	d.DeltaBase = tmp.DeltaBase
	d.discriminator = tmp.Discriminator
	d.replace = tmp.Replace
	d.value = tmp.Value
	return nil
}

func NewDeltaOptionReplace[a any, deltaA any](val a) DeltaOption[a, deltaA] {
	return DeltaOption[a, deltaA]{
		discriminator: OptionReplace,
		replace:       &val,
	}
}
func NewDeltaOptionValue[a any, deltaA any](delta deltaA) DeltaOption[a, deltaA] {
	return DeltaOption[a, deltaA]{
		discriminator: OptionValue,
		value:         &delta,
	}
}
func MatchDeltaOption[a any, deltaA any, Result any](
	onReplace func(a) (Result, error),
	onValue func(deltaA) (Result, error),
) func(DeltaOption[a, deltaA]) (Result, error) {
	return func(delta DeltaOption[a, deltaA]) (Result, error) {
		var result Result
		switch delta.discriminator {
		case OptionReplace:
			return onReplace(*delta.replace)
		case OptionValue:
			return onValue(*delta.value)
		}
		return result, NewInvalidDiscriminatorError(string(delta.discriminator), "DeltaOption")
	}
}
