package ballerina

import (
	"encoding/json"
)

type deltaSumEffectsEnum string

const (
	sumReplace deltaSumEffectsEnum = "SumReplace"
	sumLeft    deltaSumEffectsEnum = "SumLeft"
	sumRight   deltaSumEffectsEnum = "SumRight"
)

var allDeltaSumEffectsEnumCases = [...]deltaSumEffectsEnum{sumReplace, sumLeft, sumRight}

func DefaultDeltaSumEffectsEnum() deltaSumEffectsEnum { return allDeltaSumEffectsEnumCases[0] }

type DeltaSum[a any, b any, deltaA any, deltaB any] struct {
	DeltaBase
	discriminator deltaSumEffectsEnum
	replace       *Sum[a, b]
	left          *deltaA
	right         *deltaB
}

var _ json.Unmarshaler = &DeltaSum[Unit, Unit, Unit, Unit]{}
var _ json.Marshaler = DeltaSum[Unit, Unit, Unit, Unit]{}

func (d DeltaSum[a, b, deltaA, deltaB]) MarshalJSON() ([]byte, error) {
	return json.Marshal(struct {
		DeltaBase
		Discriminator deltaSumEffectsEnum
		Replace       *Sum[a, b]
		Left          *deltaA
		Right         *deltaB
	}{
		DeltaBase:     d.DeltaBase,
		Discriminator: d.discriminator,
		Replace:       d.replace,
		Left:          d.left,
		Right:         d.right,
	})
}

func (d *DeltaSum[a, b, deltaA, deltaB]) UnmarshalJSON(data []byte) error {
	var aux struct {
		DeltaBase
		Discriminator deltaSumEffectsEnum
		Replace       *Sum[a, b]
		Left          *deltaA
		Right         *deltaB
	}
	if err := json.Unmarshal(data, &aux); err != nil {
		return err
	}
	d.DeltaBase = aux.DeltaBase
	d.discriminator = aux.Discriminator
	d.replace = aux.Replace
	d.left = aux.Left
	d.right = aux.Right
	return nil
}

func NewDeltaSumReplace[a any, b any, deltaA any, deltaB any](value Sum[a, b]) DeltaSum[a, b, deltaA, deltaB] {
	return DeltaSum[a, b, deltaA, deltaB]{
		discriminator: sumReplace,
		replace:       &value,
	}
}
func NewDeltaSumLeft[a any, b any, deltaA any, deltaB any](delta deltaA) DeltaSum[a, b, deltaA, deltaB] {
	return DeltaSum[a, b, deltaA, deltaB]{
		discriminator: sumLeft,
		left:          &delta,
	}
}
func NewDeltaSumRight[a any, b any, deltaA any, deltaB any](delta deltaB) DeltaSum[a, b, deltaA, deltaB] {
	return DeltaSum[a, b, deltaA, deltaB]{
		discriminator: sumRight,
		right:         &delta,
	}
}
func MatchDeltaSum[a any, b any, deltaA any, deltaB any, Result any](
	onReplace func(Sum[a, b]) (Result, error),
	onLeft func(deltaA) (Result, error),
	onRight func(deltaB) (Result, error),
) func(DeltaSum[a, b, deltaA, deltaB]) (Result, error) {
	return func(delta DeltaSum[a, b, deltaA, deltaB]) (Result, error) {
		var result Result
		switch delta.discriminator {
		case sumReplace:
			return onReplace(*delta.replace)
		case sumLeft:
			return onLeft(*delta.left)
		case sumRight:
			return onRight(*delta.right)
		}
		return result, NewInvalidDiscriminatorError(string(delta.discriminator), "DeltaSum")
	}
}
