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
	Discriminator DeltaSumEffectsEnum
	Replace       Sum[a, b]
	Left          deltaA
	Right         deltaB
}

func NewDeltaSumReplace[a any, b any, deltaA any, deltaB any](value Sum[a, b]) DeltaSum[a, b, deltaA, deltaB] {
	return DeltaSum[a, b, deltaA, deltaB]{
		Discriminator: SumReplace,
		Replace:       value,
	}
}
func NewDeltaSumLeft[a any, b any, deltaA any, deltaB any](delta deltaA) DeltaSum[a, b, deltaA, deltaB] {
	return DeltaSum[a, b, deltaA, deltaB]{
		Discriminator: SumLeft,
		Left:          delta,
	}
}
func NewDeltaSumRight[a any, b any, deltaA any, deltaB any](delta deltaB) DeltaSum[a, b, deltaA, deltaB] {
	return DeltaSum[a, b, deltaA, deltaB]{
		Discriminator: SumRight,
		Right:         delta,
	}
}
func MatchDeltaSum[a any, b any, deltaA any, deltaB any, Result any](
	onReplace func(Sum[a, b]) (Result, error),
	onLeft func(deltaA) (Result, error),
	onRight func(deltaB) (Result, error),
) func(DeltaSum[a, b, deltaA, deltaB]) (Result, error) {
	return func(delta DeltaSum[a, b, deltaA, deltaB]) (Result, error) {
		var result Result
		switch delta.Discriminator {
		case "SumReplace":
			return onReplace(delta.Replace)
		case "SumLeft":
			return onLeft(delta.Left)
		case "SumRight":
			return onRight(delta.Right)
		}
		return result, NewInvalidDiscriminatorError(string(delta.Discriminator), "DeltaSum")
	}
}

// Serialization

type sumForSerialization[a any] struct {
	IsRight bool
	Value   a
}

func (s Sum[a, b]) MarshalJSON() ([]byte, error) {
	return FoldWithError(
		s,
		func(left a) ([]byte, error) {
			return json.Marshal(sumForSerialization[a]{
				IsRight: false,
				Value:   left,
			})
		},
		func(right b) ([]byte, error) {
			return json.Marshal(sumForSerialization[b]{
				IsRight: true,
				Value:   right,
			})
		},
	)
}

type inspectIsRight struct {
	IsRight bool
}

func (s *Sum[a, b]) UnmarshalJSON(data []byte) error {
	var i inspectIsRight
	err := json.Unmarshal(data, &i)
	if err != nil {
		return err
	}
	s.isRight = i.IsRight
	if s.isRight {
		var right sumForSerialization[b]
		err = json.Unmarshal(data, &right)
		if err != nil {
			return err
		}
		s.right = right.Value
	} else {
		var left sumForSerialization[a]
		err = json.Unmarshal(data, &left)
		if err != nil {
			return err
		}
		s.left = left.Value
	}
	return nil
}
