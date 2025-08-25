package ballerina

import (
	"encoding/json"
	"fmt"
)

type deltaOneEffectsEnum string

const (
	oneReplace     deltaOneEffectsEnum = "OneReplace"
	oneValue       deltaOneEffectsEnum = "OneValue"
	oneCreateValue deltaOneEffectsEnum = "OneCreateValue"
	oneDeleteValue deltaOneEffectsEnum = "OneDeleteValue"
)

var allDeltaOneEffectsEnumCases = [...]deltaOneEffectsEnum{oneReplace, oneValue, oneCreateValue, oneDeleteValue}

func DefaultDeltaOneEffectsEnum() deltaOneEffectsEnum { return allDeltaOneEffectsEnumCases[0] }

type DeltaOne[a any, deltaA any] struct {
	DeltaBase
	discriminator deltaOneEffectsEnum
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
		Discriminator deltaOneEffectsEnum
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
		Discriminator deltaOneEffectsEnum
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
		discriminator: oneReplace,
		replace:       &value,
	}
}
func NewDeltaOneValue[a any, deltaA any](delta deltaA) DeltaOne[a, deltaA] {
	return DeltaOne[a, deltaA]{
		discriminator: oneValue,
		value:         &delta,
	}
}
func NewDeltaOneCreateValue[a any, deltaA any](value a) DeltaOne[a, deltaA] {
	return DeltaOne[a, deltaA]{
		discriminator: oneCreateValue,
		createValue:   &value,
	}
}
func NewDeltaOneDeleteValue[a any, deltaA any]() DeltaOne[a, deltaA] {
	unit := NewUnit()
	return DeltaOne[a, deltaA]{
		discriminator: oneDeleteValue,
		deleteValue:   &unit,
	}
}
func MatchDeltaOne[a any, deltaA any, Result any](
	onReplace func(a) func(ReaderWithError[Unit, One[a]]) (Result, error),
	onValue func(deltaA) func(ReaderWithError[Unit, a]) (Result, error),
	onCreateValue func(a) (Result, error),
	onDeleteValue func() (Result, error),
) func(DeltaOne[a, deltaA]) func(ReaderWithError[Unit, One[a]]) (Result, error) {
	return func(delta DeltaOne[a, deltaA]) func(ReaderWithError[Unit, One[a]]) (Result, error) {
		return func(one ReaderWithError[Unit, One[a]]) (Result, error) {
			var result Result
			switch delta.discriminator {
			case oneReplace:
				if delta.replace == nil {
					return result, NewInvalidDiscriminatorError("nil replace", "DeltaOne")
				}
				return onReplace(*delta.replace)(one)
			case oneValue:
				if delta.value == nil {
					return result, NewInvalidDiscriminatorError("nil value", "DeltaOne")
				}
				value := BindReaderWithError[Unit, One[a], a](
					func(one One[a]) ReaderWithError[Unit, a] {
						return PureReader[Unit, Sum[error, a]](
							Fold(
								one.Sum,
								func(Unit) Sum[error, a] {
									return Left[error, a](fmt.Errorf("one is not set"))
								},
								Right[error, a],
							),
						)
					},
				)(one)
				return onValue(*delta.value)(value)
			case oneCreateValue:
				if delta.createValue == nil {
					return result, NewInvalidDiscriminatorError("nil createValue", "DeltaOne")
				}
				return onCreateValue(*delta.createValue)
			case oneDeleteValue:
				return onDeleteValue()
			}
			return result, NewInvalidDiscriminatorError(string(delta.discriminator), "DeltaOne")
		}
	}
}
