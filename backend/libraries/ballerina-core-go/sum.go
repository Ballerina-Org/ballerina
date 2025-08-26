package ballerina

import (
	"encoding/json"
)

type Sum[a any, b any] struct {
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

func (s Sum[L, R]) Swap() Sum[R, L] {
	return Fold(
		s,
		Right[R, L],
		Left[R, L],
	)
}

func MapLeft[L any, R any, LO any](e Sum[L, R], leftMap func(L) LO) Sum[LO, R] {
	return BiMap(e, leftMap, id[R])
}

func MapRight[L any, R any, RO any](e Sum[L, R], rightMap func(R) RO) Sum[L, RO] {
	return BiMap(e, id[L], rightMap)
}

func Bind[L any, R any, RO any](e Sum[L, R], rightMap func(R) Sum[L, RO]) Sum[L, RO] {
	return Fold(e, func(l L) Sum[L, RO] { return Left[L, RO](l) }, rightMap)
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

func GoErrorToSum[T, U any](f func(T) (U, error)) func(T) Sum[error, U] {
	return func(value T) Sum[error, U] {
		result, err := f(value)
		if err != nil {
			return Left[error, U](err)
		}
		return Right[error, U](result)
	}
}

// NOTE: we collect only the first error we encounter
func SumAll[T any](values []Sum[error, T]) Sum[error, []T] {
	return FoldLeftArray(values, Right[error, []T]([]T{}), func(acc Sum[error, []T], value Sum[error, T]) Sum[error, []T] {
		return Bind(value, func(value T) Sum[error, []T] {
			return MapRight(acc, func(acc []T) []T {
				return append(acc, value)
			})
		})
	})
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
