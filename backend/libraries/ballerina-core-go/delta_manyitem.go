package ballerina

import (
	"encoding/json"
)

type deltaManyItemEffectsEnum string

const (
	manyItemValue         deltaManyItemEffectsEnum = "ManyItemValue"
	manyItemIsLinked      deltaManyItemEffectsEnum = "ManyItemIsLinked"
	manyItemCanChangeLink deltaManyItemEffectsEnum = "ManyItemCanChangeLink"
)

var allDeltaManyItemEffectsEnumCases = [...]deltaManyItemEffectsEnum{manyItemValue, manyItemIsLinked, manyItemCanChangeLink}

func DefaultDeltaManyItemEffectsEnum() deltaManyItemEffectsEnum {
	return allDeltaManyItemEffectsEnumCases[0]
}

type DeltaManyItem[T any, deltaT any] struct {
	DeltaBase
	discriminator deltaManyItemEffectsEnum
	value         *deltaT
	isLinked      *bool
	canChangeLink *bool
}

var _ json.Unmarshaler = &DeltaManyItem[Unit, Unit]{}
var _ json.Marshaler = DeltaManyItem[Unit, Unit]{}

func (d DeltaManyItem[T, deltaT]) MarshalJSON() ([]byte, error) {
	return json.Marshal(struct {
		DeltaBase
		Discriminator deltaManyItemEffectsEnum
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
		Discriminator deltaManyItemEffectsEnum
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
		discriminator: manyItemValue,
		value:         &delta,
	}
}

func NewDeltaManyItemIsLinked[T any, deltaT any](isLinked bool) DeltaManyItem[T, deltaT] {
	return DeltaManyItem[T, deltaT]{
		discriminator: manyItemIsLinked,
		isLinked:      &isLinked,
	}
}

func NewDeltaManyItemCanChangeLink[T any, deltaT any](canChangeLink bool) DeltaManyItem[T, deltaT] {
	return DeltaManyItem[T, deltaT]{
		discriminator: manyItemCanChangeLink,
		canChangeLink: &canChangeLink,
	}
}

func MatchDeltaManyItem[T any, deltaT any, Result any](
	onValue func(deltaT) func(ReaderWithError[Unit, T]) (Result, error),
	onIsLinked func(bool) (Result, error),
	onCanChangeLink func(bool) (Result, error),
) func(DeltaManyItem[T, deltaT]) func(ReaderWithError[Unit, ManyItem[T]]) (Result, error) {
	return func(delta DeltaManyItem[T, deltaT]) func(ReaderWithError[Unit, ManyItem[T]]) (Result, error) {
		return func(manyItem ReaderWithError[Unit, ManyItem[T]]) (Result, error) {
			var result Result
			switch delta.discriminator {
			case manyItemValue:
				value := MapReaderWithError[Unit, ManyItem[T], T](
					func(manyItem ManyItem[T]) T {
						return manyItem.Value
					},
				)(manyItem)
				return onValue(*delta.value)(value)
			case manyItemIsLinked:
				return onIsLinked(*delta.isLinked)
			case manyItemCanChangeLink:
				return onCanChangeLink(*delta.canChangeLink)
			}
			return result, NewInvalidDiscriminatorError(string(delta.discriminator), "DeltaManyItem")
		}
	}
}
