package ballerina

import (
	"encoding/json"
)

type deltaOptionEffectsEnum string

const (
	optionReplace deltaOptionEffectsEnum = "OptionReplace"
	optionValue   deltaOptionEffectsEnum = "OptionValue"
)

var allDeltaOptionEffectsEnumCases = [...]deltaOptionEffectsEnum{optionReplace, optionValue}

func DefaultDeltaOptionEffectsEnum() deltaOptionEffectsEnum { return allDeltaOptionEffectsEnumCases[0] }

type DeltaOption[a any, deltaA any] struct {
	DeltaBase
	discriminator deltaOptionEffectsEnum
	replace       *Option[a]
	value         *deltaA
}

var _ json.Unmarshaler = &DeltaOption[Unit, Unit]{}
var _ json.Marshaler = DeltaOption[Unit, Unit]{}

func (d DeltaOption[a, deltaA]) MarshalJSON() ([]byte, error) {
	return json.Marshal(struct {
		DeltaBase
		Discriminator deltaOptionEffectsEnum
		Replace       *Option[a]
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
		Discriminator deltaOptionEffectsEnum
		Replace       *Option[a]
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

func NewDeltaOptionReplace[a any, deltaA any](val Option[a]) DeltaOption[a, deltaA] {
	return DeltaOption[a, deltaA]{
		discriminator: optionReplace,
		replace:       &val,
	}
}
func NewDeltaOptionValue[a any, deltaA any](delta deltaA) DeltaOption[a, deltaA] {
	return DeltaOption[a, deltaA]{
		discriminator: optionValue,
		value:         &delta,
	}
}
func MatchDeltaOption[a any, deltaA any, Result any](
	onReplace func(Option[a]) (Result, error),
	onValue func(deltaA) (Result, error),
) func(DeltaOption[a, deltaA]) (Result, error) {
	return func(delta DeltaOption[a, deltaA]) (Result, error) {
		var result Result
		switch delta.discriminator {
		case optionReplace:
			return onReplace(*delta.replace)
		case optionValue:
			return onValue(*delta.value)
		}
		return result, NewInvalidDiscriminatorError(string(delta.discriminator), "DeltaOption")
	}
}
