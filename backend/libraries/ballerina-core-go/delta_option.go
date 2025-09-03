package ballerina

import (
	"encoding/json"
	"fmt"
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
	onReplace func(Option[a]) func(ReaderWithError[Unit, Option[a]]) (Result, error),
	onValue func(deltaA) func(ReaderWithError[Unit, a]) (Result, error),
) func(DeltaOption[a, deltaA]) func(ReaderWithError[Unit, Option[a]]) (Result, error) {
	return func(delta DeltaOption[a, deltaA]) func(ReaderWithError[Unit, Option[a]]) (Result, error) {
		return func(option ReaderWithError[Unit, Option[a]]) (Result, error) {
			var result Result
			switch delta.discriminator {
			case optionReplace:
				return onReplace(*delta.replace)(option)
			case optionValue:
				value := BindReaderWithError[Unit, Option[a], a](
					func(one Option[a]) ReaderWithError[Unit, a] {
						return PureReader[Unit, Sum[error, a]](
							Fold(
								one.Sum,
								func(Unit) Sum[error, a] {
									return Left[error, a](fmt.Errorf("option is not set"))
								},
								Right[error, a],
							),
						)
					},
				)(option)
				return onValue(*delta.value)(value)
			}
			return result, NewInvalidDiscriminatorError(string(delta.discriminator), "DeltaOption")
		}
	}
}
