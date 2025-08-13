package ballerina

import (
	"encoding/json"
)

type Sum[a any, b any] struct {
	// NOTE: Important: all of these attributes must be private.
	// Getting values out of Sum can only be done through Fold
	isRight bool

	left  a
	right b
}

func Left[L any, R any](value L) Sum[L, R] {
	return Sum[L, R]{
		isRight: false,
		left:    value,
	}
}

func Right[L any, R any](value R) Sum[L, R] {
	return Sum[L, R]{
		isRight: true,
		right:   value,
	}
}

func BiMap[L any, R any, LO any, RO any](e Sum[L, R], leftMap func(L) LO, rightMap func(R) RO) Sum[LO, RO] {
	if e.isRight {
		return Right[LO, RO](rightMap(e.right))
	}
	return Left[LO, RO](leftMap(e.left))
}

func BiMapWithError[L any, R any, LO any, RO any](e Sum[L, R], leftMap func(L) (LO, error), rightMap func(R) (RO, error)) (Sum[LO, RO], error) {
	if e.isRight {
		output, err := rightMap(e.right)
		if err != nil {
			return Sum[LO, RO]{}, err
		}
		return Right[LO, RO](output), nil
	}
	output, err := leftMap(e.left)
	if err != nil {
		return Sum[LO, RO]{}, err
	}
	return Left[LO, RO](output), nil
}

func Fold[L any, R any, O any](e Sum[L, R], leftMap func(L) O, rightMap func(R) O) O {
	if e.isRight {
		return rightMap(e.right)
	}
	return leftMap(e.left)
}

func FoldWithError[L any, R any, O any](e Sum[L, R], leftMap func(L) (O, error), rightMap func(R) (O, error)) (O, error) {
	if e.isRight {
		return rightMap(e.right)
	}
	return leftMap(e.left)
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
