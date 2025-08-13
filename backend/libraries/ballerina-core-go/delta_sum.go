package ballerina

import (
	"encoding/json"
)

type DeltaSumEffectsEnum string

const (
	SumReplace DeltaSumEffectsEnum = "SumReplace"
	SumLeft    DeltaSumEffectsEnum = "SumLeft"
	SumRight   DeltaSumEffectsEnum = "SumRight"
)

var AllDeltaSumEffectsEnumCases = [...]DeltaSumEffectsEnum{SumReplace, SumLeft, SumRight}

func DefaultDeltaSumEffectsEnum() DeltaSumEffectsEnum { return AllDeltaSumEffectsEnumCases[0] }

type DeltaSum[a any, b any, deltaA any, deltaB any] struct {
	DeltaBase
	discriminator DeltaSumEffectsEnum
	replace       *Sum[a, b]
	left          *deltaA
	right         *deltaB
}

var _ json.Unmarshaler = &DeltaSum[Unit, Unit, Unit, Unit]{}
var _ json.Marshaler = DeltaSum[Unit, Unit, Unit, Unit]{}

func (d DeltaSum[a, b, deltaA, deltaB]) MarshalJSON() ([]byte, error) {
	return json.Marshal(struct {
		DeltaBase
		Discriminator DeltaSumEffectsEnum
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
		Discriminator DeltaSumEffectsEnum
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
		discriminator: SumReplace,
		replace:       &value,
	}
}
func NewDeltaSumLeft[a any, b any, deltaA any, deltaB any](delta deltaA) DeltaSum[a, b, deltaA, deltaB] {
	return DeltaSum[a, b, deltaA, deltaB]{
		discriminator: SumLeft,
		left:          &delta,
	}
}
func NewDeltaSumRight[a any, b any, deltaA any, deltaB any](delta deltaB) DeltaSum[a, b, deltaA, deltaB] {
	return DeltaSum[a, b, deltaA, deltaB]{
		discriminator: SumRight,
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
		case SumReplace:
			return onReplace(*delta.replace)
		case SumLeft:
			return onLeft(*delta.left)
		case SumRight:
			return onRight(*delta.right)
		}
		return result, NewInvalidDiscriminatorError(string(delta.discriminator), "DeltaSum")
	}
}
