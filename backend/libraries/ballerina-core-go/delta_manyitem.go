package ballerina

import (
	"encoding/json"
)

type DeltaManyItemEffectsEnum string

const (
	ManyItemValue         DeltaManyItemEffectsEnum = "ManyItemValue"
	ManyItemIsLinked      DeltaManyItemEffectsEnum = "ManyItemIsLinked"
	ManyItemCanChangeLink DeltaManyItemEffectsEnum = "ManyItemCanChangeLink"
)

var AllDeltaManyItemEffectsEnumCases = [...]DeltaManyItemEffectsEnum{ManyItemValue, ManyItemIsLinked, ManyItemCanChangeLink}

func DefaultDeltaManyItemEffectsEnum() DeltaManyItemEffectsEnum {
	return AllDeltaManyItemEffectsEnumCases[0]
}

type DeltaManyItem[T any, deltaT any] struct {
	DeltaBase
	discriminator DeltaManyItemEffectsEnum
	value         *deltaT
	isLinked      *bool
	canChangeLink *bool
}

var _ json.Unmarshaler = &DeltaManyItem[Unit, Unit]{}
var _ json.Marshaler = DeltaManyItem[Unit, Unit]{}

func (d DeltaManyItem[T, deltaT]) MarshalJSON() ([]byte, error) {
	return json.Marshal(struct {
		DeltaBase
		Discriminator DeltaManyItemEffectsEnum
		Value         *deltaT
		IsLinked      *bool
		CanChangeLink *bool
	}{
		DeltaBase:     d.DeltaBase,
		Discriminator: d.discriminator,
		Value:         d.value,
		IsLinked:      d.isLinked,
		CanChangeLink: d.canChangeLink,
	})
}

func (d *DeltaManyItem[T, deltaT]) UnmarshalJSON(data []byte) error {
	var a struct {
		DeltaBase
		Discriminator DeltaManyItemEffectsEnum
		Value         *deltaT
		IsLinked      *bool
		CanChangeLink *bool
	}
	if err := json.Unmarshal(data, &a); err != nil {
		return err
	}
	d.DeltaBase = a.DeltaBase
	d.discriminator = a.Discriminator
	d.value = a.Value
	d.isLinked = a.IsLinked
	d.canChangeLink = a.CanChangeLink
	return nil
}

func NewDeltaManyItemValue[T any, deltaT any](delta deltaT) DeltaManyItem[T, deltaT] {
	return DeltaManyItem[T, deltaT]{
		discriminator: ManyItemValue,
		value:         &delta,
	}
}

func NewDeltaManyItemIsLinked[T any, deltaT any](isLinked bool) DeltaManyItem[T, deltaT] {
	return DeltaManyItem[T, deltaT]{
		discriminator: ManyItemIsLinked,
		isLinked:      &isLinked,
	}
}

func NewDeltaManyItemCanChangeLink[T any, deltaT any](canChangeLink bool) DeltaManyItem[T, deltaT] {
	return DeltaManyItem[T, deltaT]{
		discriminator: ManyItemCanChangeLink,
		canChangeLink: &canChangeLink,
	}
}

func MatchDeltaManyItem[T any, deltaT any, Result any](
	onValue func(deltaT) (Result, error),
	onIsLinked func(bool) (Result, error),
	onCanChangeLink func(bool) (Result, error),
) func(DeltaManyItem[T, deltaT]) (Result, error) {
	return func(delta DeltaManyItem[T, deltaT]) (Result, error) {
		var result Result
		switch delta.discriminator {
		case ManyItemValue:
			return onValue(*delta.value)
		case ManyItemIsLinked:
			return onIsLinked(*delta.isLinked)
		case ManyItemCanChangeLink:
			return onCanChangeLink(*delta.canChangeLink)
		}
		return result, NewInvalidDiscriminatorError(string(delta.discriminator), "DeltaManyItem")
	}
}
