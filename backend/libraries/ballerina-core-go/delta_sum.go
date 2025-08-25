package ballerina

import (
	"encoding/json"
	"fmt"
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
	onReplace func(Sum[a, b]) func(ReaderWithError[Unit, Sum[a, b]]) (Result, error),
	onLeft func(deltaA) func(ReaderWithError[Unit, a]) (Result, error),
	onRight func(deltaB) func(ReaderWithError[Unit, b]) (Result, error),
) func(DeltaSum[a, b, deltaA, deltaB]) func(ReaderWithError[Unit, Sum[a, b]]) (Result, error) {
	return func(delta DeltaSum[a, b, deltaA, deltaB]) func(ReaderWithError[Unit, Sum[a, b]]) (Result, error) {
		return func(value ReaderWithError[Unit, Sum[a, b]]) (Result, error) {
			var result Result
			switch delta.discriminator {
			case sumReplace:
				return onReplace(*delta.replace)(value)
			case sumLeft:
				var leftValue ReaderWithError[Unit, a] = BindReaderWithError(
					func(value Sum[a, b]) ReaderWithError[Unit, a] {
						return PureReader[Unit, Sum[error, a]](
							Fold(
								value,
								Right[error, a],
								func(right b) Sum[error, a] {
									return Left[error, a](fmt.Errorf("sum is right"))
								},
							),
						)
					},
				)(value)
				return onLeft(*delta.left)(leftValue)
			case sumRight:
				var rightValue ReaderWithError[Unit, b] = BindReaderWithError(
					func(value Sum[a, b]) ReaderWithError[Unit, b] {
						return PureReader[Unit, Sum[error, b]](
							Fold(
								value,
								func(left a) Sum[error, b] {
									return Left[error, b](fmt.Errorf("sum is left"))
								},
								Right[error, b],
							),
						)
					},
				)(value)
				return onRight(*delta.right)(rightValue)
			}
			return result, NewInvalidDiscriminatorError(string(delta.discriminator), "DeltaSum")
		}
	}
}
