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

type sum2CasesEnum string

const (
	case1Of2 sum2CasesEnum = "case1Of2"
	case2Of2 sum2CasesEnum = "case2Of2"
)

var AllSum2CasesEnum = [...]sum2CasesEnum{case1Of2, case2Of2}

func DefaultSum2CasesEnum() sum2CasesEnum { return AllSum2CasesEnum[0] }

type Sum2[case1 any, case2 any] struct {
	discriminator sum2CasesEnum

	case1 *case1
	case2 *case2
}

func Case1Of2[case1 any, case2 any](value case1) Sum2[case1, case2] {
	return Sum2[case1, case2]{
		discriminator: case1Of2,
		case1:         &value,
	}
}

func Case2Of2[case1 any, case2 any](value case2) Sum2[case1, case2] {
	return Sum2[case1, case2]{
		discriminator: case2Of2,
		case2:         &value,
	}
}

func FoldSum2[case1 any, case2 any, Result any](onCase1 func(case1) (Result, error), onCase2 func(case2) (Result, error)) func(s Sum2[case1, case2]) (Result, error) {
	return func(s Sum2[case1, case2]) (Result, error) {
		switch s.discriminator {
		case case1Of2:
			return onCase1(*s.case1)
		case case2Of2:
			return onCase2(*s.case2)
		}
		var nilResult Result
		return nilResult, NewInvalidDiscriminatorError(string(s.discriminator), "Sum2")
	}
}

var _ json.Unmarshaler = &Sum2[Unit, Unit]{}
var _ json.Marshaler = Sum2[Unit, Unit]{}

func (d Sum2[case1, case2]) MarshalJSON() ([]byte, error) {
	return json.Marshal(struct {
		Discriminator sum2CasesEnum
		Case1         *case1
		Case2         *case2
	}{
		Discriminator: d.discriminator,
		Case1:         d.case1,
		Case2:         d.case2,
	})
}

func (d *Sum2[case1, case2]) UnmarshalJSON(data []byte) error {
	var aux struct {
		Discriminator sum2CasesEnum
		Case1         *case1
		Case2         *case2
	}
	if err := json.Unmarshal(data, &aux); err != nil {
		return err
	}
	d.discriminator = aux.Discriminator
	d.case1 = aux.Case1
	d.case2 = aux.Case2
	return nil
}

type sum3CasesEnum string

const (
	case1Of3 sum3CasesEnum = "case1Of3"
	case2Of3 sum3CasesEnum = "case2Of3"
	case3Of3 sum3CasesEnum = "case3Of3"
)

var AllSum3CasesEnum = [...]sum3CasesEnum{case1Of3, case2Of3, case3Of3}

func DefaultSum3CasesEnum() sum3CasesEnum { return AllSum3CasesEnum[0] }

type Sum3[case1 any, case2 any, case3 any] struct {
	discriminator sum3CasesEnum

	case1 *case1
	case2 *case2
	case3 *case3
}

func Case1Of3[case1 any, case2 any, case3 any](value case1) Sum3[case1, case2, case3] {
	return Sum3[case1, case2, case3]{
		discriminator: case1Of3,
		case1:         &value,
	}
}

func Case2Of3[case1 any, case2 any, case3 any](value case2) Sum3[case1, case2, case3] {
	return Sum3[case1, case2, case3]{
		discriminator: case2Of3,
		case2:         &value,
	}
}

func Case3Of3[case1 any, case2 any, case3 any](value case3) Sum3[case1, case2, case3] {
	return Sum3[case1, case2, case3]{
		discriminator: case3Of3,
		case3:         &value,
	}
}

func FoldSum3[case1 any, case2 any, case3 any, Result any](onCase1 func(case1) (Result, error), onCase2 func(case2) (Result, error), onCase3 func(case3) (Result, error)) func(s Sum3[case1, case2, case3]) (Result, error) {
	return func(s Sum3[case1, case2, case3]) (Result, error) {
		switch s.discriminator {
		case case1Of3:
			return onCase1(*s.case1)
		case case2Of3:
			return onCase2(*s.case2)
		case case3Of3:
			return onCase3(*s.case3)
		}
		var nilResult Result
		return nilResult, NewInvalidDiscriminatorError(string(s.discriminator), "Sum3")
	}
}

var _ json.Unmarshaler = &Sum3[Unit, Unit, Unit]{}
var _ json.Marshaler = Sum3[Unit, Unit, Unit]{}

func (d Sum3[case1, case2, case3]) MarshalJSON() ([]byte, error) {
	return json.Marshal(struct {
		Discriminator sum3CasesEnum
		Case1         *case1
		Case2         *case2
		Case3         *case3
	}{
		Discriminator: d.discriminator,
		Case1:         d.case1,
		Case2:         d.case2,
		Case3:         d.case3,
	})
}

func (d *Sum3[case1, case2, case3]) UnmarshalJSON(data []byte) error {
	var aux struct {
		Discriminator sum3CasesEnum
		Case1         *case1
		Case2         *case2
		Case3         *case3
	}
	if err := json.Unmarshal(data, &aux); err != nil {
		return err
	}
	d.discriminator = aux.Discriminator
	d.case1 = aux.Case1
	d.case2 = aux.Case2
	d.case3 = aux.Case3
	return nil
}

type sum4CasesEnum string

const (
	case1Of4 sum4CasesEnum = "case1Of4"
	case2Of4 sum4CasesEnum = "case2Of4"
	case3Of4 sum4CasesEnum = "case3Of4"
	case4Of4 sum4CasesEnum = "case4Of4"
)

var AllSum4CasesEnum = [...]sum4CasesEnum{case1Of4, case2Of4, case3Of4, case4Of4}

func DefaultSum4CasesEnum() sum4CasesEnum { return AllSum4CasesEnum[0] }

type Sum4[case1 any, case2 any, case3 any, case4 any] struct {
	discriminator sum4CasesEnum

	case1 *case1
	case2 *case2
	case3 *case3
	case4 *case4
}

func Case1Of4[case1 any, case2 any, case3 any, case4 any](value case1) Sum4[case1, case2, case3, case4] {
	return Sum4[case1, case2, case3, case4]{
		discriminator: case1Of4,
		case1:         &value,
	}
}

func Case2Of4[case1 any, case2 any, case3 any, case4 any](value case2) Sum4[case1, case2, case3, case4] {
	return Sum4[case1, case2, case3, case4]{
		discriminator: case2Of4,
		case2:         &value,
	}
}

func Case3Of4[case1 any, case2 any, case3 any, case4 any](value case3) Sum4[case1, case2, case3, case4] {
	return Sum4[case1, case2, case3, case4]{
		discriminator: case3Of4,
		case3:         &value,
	}
}

func Case4Of4[case1 any, case2 any, case3 any, case4 any](value case4) Sum4[case1, case2, case3, case4] {
	return Sum4[case1, case2, case3, case4]{
		discriminator: case4Of4,
		case4:         &value,
	}
}

func FoldSum4[case1 any, case2 any, case3 any, case4 any, Result any](
	onCase1 func(case1) (Result, error),
	onCase2 func(case2) (Result, error),
	onCase3 func(case3) (Result, error),
	onCase4 func(case4) (Result, error),
) func(s Sum4[case1, case2, case3, case4]) (Result, error) {
	return func(s Sum4[case1, case2, case3, case4]) (Result, error) {
		switch s.discriminator {
		case case1Of4:
			return onCase1(*s.case1)
		case case2Of4:
			return onCase2(*s.case2)
		case case3Of4:
			return onCase3(*s.case3)
		case case4Of4:
			return onCase4(*s.case4)
		}
		var nilResult Result
		return nilResult, NewInvalidDiscriminatorError(string(s.discriminator), "Sum4")
	}
}

var _ json.Unmarshaler = &Sum4[Unit, Unit, Unit, Unit]{}
var _ json.Marshaler = Sum4[Unit, Unit, Unit, Unit]{}

func (d Sum4[case1, case2, case3, case4]) MarshalJSON() ([]byte, error) {
	return json.Marshal(struct {
		Discriminator sum4CasesEnum
		Case1         *case1
		Case2         *case2
		Case3         *case3
		Case4         *case4
	}{
		Discriminator: d.discriminator,
		Case1:         d.case1,
		Case2:         d.case2,
		Case3:         d.case3,
		Case4:         d.case4,
	})
}

func (d *Sum4[case1, case2, case3, case4]) UnmarshalJSON(data []byte) error {
	var aux struct {
		Discriminator sum4CasesEnum
		Case1         *case1
		Case2         *case2
		Case3         *case3
		Case4         *case4
	}
	if err := json.Unmarshal(data, &aux); err != nil {
		return err
	}
	d.discriminator = aux.Discriminator
	d.case1 = aux.Case1
	d.case2 = aux.Case2
	d.case3 = aux.Case3
	d.case4 = aux.Case4
	return nil
}

type sum5CasesEnum string

const (
	case1Of5 sum5CasesEnum = "case1Of5"
	case2Of5 sum5CasesEnum = "case2Of5"
	case3Of5 sum5CasesEnum = "case3Of5"
	case4Of5 sum5CasesEnum = "case4Of5"
	case5Of5 sum5CasesEnum = "case5Of5"
)

var AllSum5CasesEnum = [...]sum5CasesEnum{case1Of5, case2Of5, case3Of5, case4Of5, case5Of5}

func DefaultSum5CasesEnum() sum5CasesEnum { return AllSum5CasesEnum[0] }

type Sum5[case1 any, case2 any, case3 any, case4 any, case5 any] struct {
	discriminator sum5CasesEnum

	case1 *case1
	case2 *case2
	case3 *case3
	case4 *case4
	case5 *case5
}

func Case1Of5[case1 any, case2 any, case3 any, case4 any, case5 any](value case1) Sum5[case1, case2, case3, case4, case5] {
	return Sum5[case1, case2, case3, case4, case5]{
		discriminator: case1Of5,
		case1:         &value,
	}
}

func Case2Of5[case1 any, case2 any, case3 any, case4 any, case5 any](value case2) Sum5[case1, case2, case3, case4, case5] {
	return Sum5[case1, case2, case3, case4, case5]{
		discriminator: case2Of5,
		case2:         &value,
	}
}

func Case3Of5[case1 any, case2 any, case3 any, case4 any, case5 any](value case3) Sum5[case1, case2, case3, case4, case5] {
	return Sum5[case1, case2, case3, case4, case5]{
		discriminator: case3Of5,
		case3:         &value,
	}
}

func Case4Of5[case1 any, case2 any, case3 any, case4 any, case5 any](value case4) Sum5[case1, case2, case3, case4, case5] {
	return Sum5[case1, case2, case3, case4, case5]{
		discriminator: case4Of5,
		case4:         &value,
	}
}

func Case5Of5[case1 any, case2 any, case3 any, case4 any, case5 any](value case5) Sum5[case1, case2, case3, case4, case5] {
	return Sum5[case1, case2, case3, case4, case5]{
		discriminator: case5Of5,
		case5:         &value,
	}
}

func FoldSum5[case1 any, case2 any, case3 any, case4 any, case5 any, Result any](
	onCase1 func(case1) (Result, error),
	onCase2 func(case2) (Result, error),
	onCase3 func(case3) (Result, error),
	onCase4 func(case4) (Result, error),
	onCase5 func(case5) (Result, error),
) func(s Sum5[case1, case2, case3, case4, case5]) (Result, error) {
	return func(s Sum5[case1, case2, case3, case4, case5]) (Result, error) {
		switch s.discriminator {
		case case1Of5:
			return onCase1(*s.case1)
		case case2Of5:
			return onCase2(*s.case2)
		case case3Of5:
			return onCase3(*s.case3)
		case case4Of5:
			return onCase4(*s.case4)
		case case5Of5:
			return onCase5(*s.case5)
		}
		var nilResult Result
		return nilResult, NewInvalidDiscriminatorError(string(s.discriminator), "Sum5")
	}
}

var _ json.Unmarshaler = &Sum5[Unit, Unit, Unit, Unit, Unit]{}
var _ json.Marshaler = Sum5[Unit, Unit, Unit, Unit, Unit]{}

func (d Sum5[case1, case2, case3, case4, case5]) MarshalJSON() ([]byte, error) {
	return json.Marshal(struct {
		Discriminator sum5CasesEnum
		Case1         *case1
		Case2         *case2
		Case3         *case3
		Case4         *case4
		Case5         *case5
	}{
		Discriminator: d.discriminator,
		Case1:         d.case1,
		Case2:         d.case2,
		Case3:         d.case3,
		Case4:         d.case4,
		Case5:         d.case5,
	})
}

func (d *Sum5[case1, case2, case3, case4, case5]) UnmarshalJSON(data []byte) error {
	var aux struct {
		Discriminator sum5CasesEnum
		Case1         *case1
		Case2         *case2
		Case3         *case3
		Case4         *case4
		Case5         *case5
	}
	if err := json.Unmarshal(data, &aux); err != nil {
		return err
	}
	d.discriminator = aux.Discriminator
	d.case1 = aux.Case1
	d.case2 = aux.Case2
	d.case3 = aux.Case3
	d.case4 = aux.Case4
	d.case5 = aux.Case5
	return nil
}

type sum6CasesEnum string

const (
	case1Of6 sum6CasesEnum = "case1Of6"
	case2Of6 sum6CasesEnum = "case2Of6"
	case3Of6 sum6CasesEnum = "case3Of6"
	case4Of6 sum6CasesEnum = "case4Of6"
	case5Of6 sum6CasesEnum = "case5Of6"
	case6Of6 sum6CasesEnum = "case6Of6"
)

var AllSum6CasesEnum = [...]sum6CasesEnum{case1Of6, case2Of6, case3Of6, case4Of6, case5Of6, case6Of6}

func DefaultSum6CasesEnum() sum6CasesEnum { return AllSum6CasesEnum[0] }

type Sum6[case1 any, case2 any, case3 any, case4 any, case5 any, case6 any] struct {
	discriminator sum6CasesEnum

	case1 *case1
	case2 *case2
	case3 *case3
	case4 *case4
	case5 *case5
	case6 *case6
}

func Case1Of6[case1 any, case2 any, case3 any, case4 any, case5 any, case6 any](value case1) Sum6[case1, case2, case3, case4, case5, case6] {
	return Sum6[case1, case2, case3, case4, case5, case6]{
		discriminator: case1Of6,
		case1:         &value,
	}
}

func Case2Of6[case1 any, case2 any, case3 any, case4 any, case5 any, case6 any](value case2) Sum6[case1, case2, case3, case4, case5, case6] {
	return Sum6[case1, case2, case3, case4, case5, case6]{
		discriminator: case2Of6,
		case2:         &value,
	}
}

func Case3Of6[case1 any, case2 any, case3 any, case4 any, case5 any, case6 any](value case3) Sum6[case1, case2, case3, case4, case5, case6] {
	return Sum6[case1, case2, case3, case4, case5, case6]{
		discriminator: case3Of6,
		case3:         &value,
	}
}

func Case4Of6[case1 any, case2 any, case3 any, case4 any, case5 any, case6 any](value case4) Sum6[case1, case2, case3, case4, case5, case6] {
	return Sum6[case1, case2, case3, case4, case5, case6]{
		discriminator: case4Of6,
		case4:         &value,
	}
}

func Case5Of6[case1 any, case2 any, case3 any, case4 any, case5 any, case6 any](value case5) Sum6[case1, case2, case3, case4, case5, case6] {
	return Sum6[case1, case2, case3, case4, case5, case6]{
		discriminator: case5Of6,
		case5:         &value,
	}
}

func Case6Of6[case1 any, case2 any, case3 any, case4 any, case5 any, case6 any](value case6) Sum6[case1, case2, case3, case4, case5, case6] {
	return Sum6[case1, case2, case3, case4, case5, case6]{
		discriminator: case6Of6,
		case6:         &value,
	}
}

func FoldSum6[case1 any, case2 any, case3 any, case4 any, case5 any, case6 any, Result any](
	onCase1 func(case1) (Result, error),
	onCase2 func(case2) (Result, error),
	onCase3 func(case3) (Result, error),
	onCase4 func(case4) (Result, error),
	onCase5 func(case5) (Result, error),
	onCase6 func(case6) (Result, error),
) func(s Sum6[case1, case2, case3, case4, case5, case6]) (Result, error) {
	return func(s Sum6[case1, case2, case3, case4, case5, case6]) (Result, error) {
		switch s.discriminator {
		case case1Of6:
			return onCase1(*s.case1)
		case case2Of6:
			return onCase2(*s.case2)
		case case3Of6:
			return onCase3(*s.case3)
		case case4Of6:
			return onCase4(*s.case4)
		case case5Of6:
			return onCase5(*s.case5)
		case case6Of6:
			return onCase6(*s.case6)
		}
		var nilResult Result
		return nilResult, NewInvalidDiscriminatorError(string(s.discriminator), "Sum6")
	}
}

var _ json.Unmarshaler = &Sum6[Unit, Unit, Unit, Unit, Unit, Unit]{}
var _ json.Marshaler = Sum6[Unit, Unit, Unit, Unit, Unit, Unit]{}

func (d Sum6[case1, case2, case3, case4, case5, case6]) MarshalJSON() ([]byte, error) {
	return json.Marshal(struct {
		Discriminator sum6CasesEnum
		Case1         *case1
		Case2         *case2
		Case3         *case3
		Case4         *case4
		Case5         *case5
		Case6         *case6
	}{
		Discriminator: d.discriminator,
		Case1:         d.case1,
		Case2:         d.case2,
		Case3:         d.case3,
		Case4:         d.case4,
		Case5:         d.case5,
		Case6:         d.case6,
	})
}

func (d *Sum6[case1, case2, case3, case4, case5, case6]) UnmarshalJSON(data []byte) error {
	var aux struct {
		Discriminator sum6CasesEnum
		Case1         *case1
		Case2         *case2
		Case3         *case3
		Case4         *case4
		Case5         *case5
		Case6         *case6
	}
	if err := json.Unmarshal(data, &aux); err != nil {
		return err
	}
	d.discriminator = aux.Discriminator
	d.case1 = aux.Case1
	d.case2 = aux.Case2
	d.case3 = aux.Case3
	d.case4 = aux.Case4
	d.case5 = aux.Case5
	d.case6 = aux.Case6
	return nil
}

type sum7CasesEnum string

const (
	case1Of7 sum7CasesEnum = "case1Of7"
	case2Of7 sum7CasesEnum = "case2Of7"
	case3Of7 sum7CasesEnum = "case3Of7"
	case4Of7 sum7CasesEnum = "case4Of7"
	case5Of7 sum7CasesEnum = "case5Of7"
	case6Of7 sum7CasesEnum = "case6Of7"
	case7Of7 sum7CasesEnum = "case7Of7"
)

var AllSum7CasesEnum = [...]sum7CasesEnum{case1Of7, case2Of7, case3Of7, case4Of7, case5Of7, case6Of7, case7Of7}

func DefaultSum7CasesEnum() sum7CasesEnum { return AllSum7CasesEnum[0] }

type Sum7[case1 any, case2 any, case3 any, case4 any, case5 any, case6 any, case7 any] struct {
	discriminator sum7CasesEnum

	case1 *case1
	case2 *case2
	case3 *case3
	case4 *case4
	case5 *case5
	case6 *case6
	case7 *case7
}

func Case1Of7[case1 any, case2 any, case3 any, case4 any, case5 any, case6 any, case7 any](value case1) Sum7[case1, case2, case3, case4, case5, case6, case7] {
	return Sum7[case1, case2, case3, case4, case5, case6, case7]{
		discriminator: case1Of7,
		case1:         &value,
	}
}

func Case2Of7[case1 any, case2 any, case3 any, case4 any, case5 any, case6 any, case7 any](value case2) Sum7[case1, case2, case3, case4, case5, case6, case7] {
	return Sum7[case1, case2, case3, case4, case5, case6, case7]{
		discriminator: case2Of7,
		case2:         &value,
	}
}

func Case3Of7[case1 any, case2 any, case3 any, case4 any, case5 any, case6 any, case7 any](value case3) Sum7[case1, case2, case3, case4, case5, case6, case7] {
	return Sum7[case1, case2, case3, case4, case5, case6, case7]{
		discriminator: case3Of7,
		case3:         &value,
	}
}

func Case4Of7[case1 any, case2 any, case3 any, case4 any, case5 any, case6 any, case7 any](value case4) Sum7[case1, case2, case3, case4, case5, case6, case7] {
	return Sum7[case1, case2, case3, case4, case5, case6, case7]{
		discriminator: case4Of7,
		case4:         &value,
	}
}

func Case5Of7[case1 any, case2 any, case3 any, case4 any, case5 any, case6 any, case7 any](value case5) Sum7[case1, case2, case3, case4, case5, case6, case7] {
	return Sum7[case1, case2, case3, case4, case5, case6, case7]{
		discriminator: case5Of7,
		case5:         &value,
	}
}

func Case6Of7[case1 any, case2 any, case3 any, case4 any, case5 any, case6 any, case7 any](value case6) Sum7[case1, case2, case3, case4, case5, case6, case7] {
	return Sum7[case1, case2, case3, case4, case5, case6, case7]{
		discriminator: case6Of7,
		case6:         &value,
	}
}

func Case7Of7[case1 any, case2 any, case3 any, case4 any, case5 any, case6 any, case7 any](value case7) Sum7[case1, case2, case3, case4, case5, case6, case7] {
	return Sum7[case1, case2, case3, case4, case5, case6, case7]{
		discriminator: case7Of7,
		case7:         &value,
	}
}

func FoldSum7[case1 any, case2 any, case3 any, case4 any, case5 any, case6 any, case7 any, Result any](
	onCase1 func(case1) (Result, error),
	onCase2 func(case2) (Result, error),
	onCase3 func(case3) (Result, error),
	onCase4 func(case4) (Result, error),
	onCase5 func(case5) (Result, error),
	onCase6 func(case6) (Result, error),
	onCase7 func(case7) (Result, error),
) func(s Sum7[case1, case2, case3, case4, case5, case6, case7]) (Result, error) {
	return func(s Sum7[case1, case2, case3, case4, case5, case6, case7]) (Result, error) {
		switch s.discriminator {
		case case1Of7:
			return onCase1(*s.case1)
		case case2Of7:
			return onCase2(*s.case2)
		case case3Of7:
			return onCase3(*s.case3)
		case case4Of7:
			return onCase4(*s.case4)
		case case5Of7:
			return onCase5(*s.case5)
		case case6Of7:
			return onCase6(*s.case6)
		case case7Of7:
			return onCase7(*s.case7)
		}
		var nilResult Result
		return nilResult, NewInvalidDiscriminatorError(string(s.discriminator), "Sum7")
	}
}

var _ json.Unmarshaler = &Sum7[Unit, Unit, Unit, Unit, Unit, Unit, Unit]{}
var _ json.Marshaler = Sum7[Unit, Unit, Unit, Unit, Unit, Unit, Unit]{}

func (d Sum7[case1, case2, case3, case4, case5, case6, case7]) MarshalJSON() ([]byte, error) {
	return json.Marshal(struct {
		Discriminator sum7CasesEnum
		Case1         *case1
		Case2         *case2
		Case3         *case3
		Case4         *case4
		Case5         *case5
		Case6         *case6
		Case7         *case7
	}{
		Discriminator: d.discriminator,
		Case1:         d.case1,
		Case2:         d.case2,
		Case3:         d.case3,
		Case4:         d.case4,
		Case5:         d.case5,
		Case6:         d.case6,
		Case7:         d.case7,
	})
}

func (d *Sum7[case1, case2, case3, case4, case5, case6, case7]) UnmarshalJSON(data []byte) error {
	var aux struct {
		Discriminator sum7CasesEnum
		Case1         *case1
		Case2         *case2
		Case3         *case3
		Case4         *case4
		Case5         *case5
		Case6         *case6
		Case7         *case7
	}
	if err := json.Unmarshal(data, &aux); err != nil {
		return err
	}
	d.discriminator = aux.Discriminator
	d.case1 = aux.Case1
	d.case2 = aux.Case2
	d.case3 = aux.Case3
	d.case4 = aux.Case4
	d.case5 = aux.Case5
	d.case6 = aux.Case6
	d.case7 = aux.Case7
	return nil
}

type sum8CasesEnum string

const (
	case1Of8 sum8CasesEnum = "case1Of8"
	case2Of8 sum8CasesEnum = "case2Of8"
	case3Of8 sum8CasesEnum = "case3Of8"
	case4Of8 sum8CasesEnum = "case4Of8"
	case5Of8 sum8CasesEnum = "case5Of8"
	case6Of8 sum8CasesEnum = "case6Of8"
	case7Of8 sum8CasesEnum = "case7Of8"
	case8Of8 sum8CasesEnum = "case8Of8"
)

var AllSum8CasesEnum = [...]sum8CasesEnum{case1Of8, case2Of8, case3Of8, case4Of8, case5Of8, case6Of8, case7Of8, case8Of8}

func DefaultSum8CasesEnum() sum8CasesEnum { return AllSum8CasesEnum[0] }

type Sum8[case1 any, case2 any, case3 any, case4 any, case5 any, case6 any, case7 any, case8 any] struct {
	discriminator sum8CasesEnum

	case1 *case1
	case2 *case2
	case3 *case3
	case4 *case4
	case5 *case5
	case6 *case6
	case7 *case7
	case8 *case8
}

func Case1Of8[case1 any, case2 any, case3 any, case4 any, case5 any, case6 any, case7 any, case8 any](value case1) Sum8[case1, case2, case3, case4, case5, case6, case7, case8] {
	return Sum8[case1, case2, case3, case4, case5, case6, case7, case8]{
		discriminator: case1Of8,
		case1:         &value,
	}
}

func Case2Of8[case1 any, case2 any, case3 any, case4 any, case5 any, case6 any, case7 any, case8 any](value case2) Sum8[case1, case2, case3, case4, case5, case6, case7, case8] {
	return Sum8[case1, case2, case3, case4, case5, case6, case7, case8]{
		discriminator: case2Of8,
		case2:         &value,
	}
}

func Case3Of8[case1 any, case2 any, case3 any, case4 any, case5 any, case6 any, case7 any, case8 any](value case3) Sum8[case1, case2, case3, case4, case5, case6, case7, case8] {
	return Sum8[case1, case2, case3, case4, case5, case6, case7, case8]{
		discriminator: case3Of8,
		case3:         &value,
	}
}

func Case4Of8[case1 any, case2 any, case3 any, case4 any, case5 any, case6 any, case7 any, case8 any](value case4) Sum8[case1, case2, case3, case4, case5, case6, case7, case8] {
	return Sum8[case1, case2, case3, case4, case5, case6, case7, case8]{
		discriminator: case4Of8,
		case4:         &value,
	}
}

func Case5Of8[case1 any, case2 any, case3 any, case4 any, case5 any, case6 any, case7 any, case8 any](value case5) Sum8[case1, case2, case3, case4, case5, case6, case7, case8] {
	return Sum8[case1, case2, case3, case4, case5, case6, case7, case8]{
		discriminator: case5Of8,
		case5:         &value,
	}
}

func Case6Of8[case1 any, case2 any, case3 any, case4 any, case5 any, case6 any, case7 any, case8 any](value case6) Sum8[case1, case2, case3, case4, case5, case6, case7, case8] {
	return Sum8[case1, case2, case3, case4, case5, case6, case7, case8]{
		discriminator: case6Of8,
		case6:         &value,
	}
}

func Case7Of8[case1 any, case2 any, case3 any, case4 any, case5 any, case6 any, case7 any, case8 any](value case7) Sum8[case1, case2, case3, case4, case5, case6, case7, case8] {
	return Sum8[case1, case2, case3, case4, case5, case6, case7, case8]{
		discriminator: case7Of8,
		case7:         &value,
	}
}

func Case8Of8[case1 any, case2 any, case3 any, case4 any, case5 any, case6 any, case7 any, case8 any](value case8) Sum8[case1, case2, case3, case4, case5, case6, case7, case8] {
	return Sum8[case1, case2, case3, case4, case5, case6, case7, case8]{
		discriminator: case8Of8,
		case8:         &value,
	}
}

func FoldSum8[case1 any, case2 any, case3 any, case4 any, case5 any, case6 any, case7 any, case8 any, Result any](
	onCase1 func(case1) (Result, error),
	onCase2 func(case2) (Result, error),
	onCase3 func(case3) (Result, error),
	onCase4 func(case4) (Result, error),
	onCase5 func(case5) (Result, error),
	onCase6 func(case6) (Result, error),
	onCase7 func(case7) (Result, error),
	onCase8 func(case8) (Result, error),
) func(s Sum8[case1, case2, case3, case4, case5, case6, case7, case8]) (Result, error) {
	return func(s Sum8[case1, case2, case3, case4, case5, case6, case7, case8]) (Result, error) {
		switch s.discriminator {
		case case1Of8:
			return onCase1(*s.case1)
		case case2Of8:
			return onCase2(*s.case2)
		case case3Of8:
			return onCase3(*s.case3)
		case case4Of8:
			return onCase4(*s.case4)
		case case5Of8:
			return onCase5(*s.case5)
		case case6Of8:
			return onCase6(*s.case6)
		case case7Of8:
			return onCase7(*s.case7)
		case case8Of8:
			return onCase8(*s.case8)
		}
		var nilResult Result
		return nilResult, NewInvalidDiscriminatorError(string(s.discriminator), "Sum8")
	}
}

var _ json.Unmarshaler = &Sum8[Unit, Unit, Unit, Unit, Unit, Unit, Unit, Unit]{}
var _ json.Marshaler = Sum8[Unit, Unit, Unit, Unit, Unit, Unit, Unit, Unit]{}

func (d Sum8[case1, case2, case3, case4, case5, case6, case7, case8]) MarshalJSON() ([]byte, error) {
	return json.Marshal(struct {
		Discriminator sum8CasesEnum
		Case1         *case1
		Case2         *case2
		Case3         *case3
		Case4         *case4
		Case5         *case5
		Case6         *case6
		Case7         *case7
		Case8         *case8
	}{
		Discriminator: d.discriminator,
		Case1:         d.case1,
		Case2:         d.case2,
		Case3:         d.case3,
		Case4:         d.case4,
		Case5:         d.case5,
		Case6:         d.case6,
		Case7:         d.case7,
		Case8:         d.case8,
	})
}

func (d *Sum8[case1, case2, case3, case4, case5, case6, case7, case8]) UnmarshalJSON(data []byte) error {
	var aux struct {
		Discriminator sum8CasesEnum
		Case1         *case1
		Case2         *case2
		Case3         *case3
		Case4         *case4
		Case5         *case5
		Case6         *case6
		Case7         *case7
		Case8         *case8
	}
	if err := json.Unmarshal(data, &aux); err != nil {
		return err
	}
	d.discriminator = aux.Discriminator
	d.case1 = aux.Case1
	d.case2 = aux.Case2
	d.case3 = aux.Case3
	d.case4 = aux.Case4
	d.case5 = aux.Case5
	d.case6 = aux.Case6
	d.case7 = aux.Case7
	d.case8 = aux.Case8
	return nil
}

type sum9CasesEnum string

const (
	case1Of9 sum9CasesEnum = "case1Of9"
	case2Of9 sum9CasesEnum = "case2Of9"
	case3Of9 sum9CasesEnum = "case3Of9"
	case4Of9 sum9CasesEnum = "case4Of9"
	case5Of9 sum9CasesEnum = "case5Of9"
	case6Of9 sum9CasesEnum = "case6Of9"
	case7Of9 sum9CasesEnum = "case7Of9"
	case8Of9 sum9CasesEnum = "case8Of9"
	case9Of9 sum9CasesEnum = "case9Of9"
)

var AllSum9CasesEnum = [...]sum9CasesEnum{case1Of9, case2Of9, case3Of9, case4Of9, case5Of9, case6Of9, case7Of9, case8Of9, case9Of9}

func DefaultSum9CasesEnum() sum9CasesEnum { return AllSum9CasesEnum[0] }

type Sum9[case1 any, case2 any, case3 any, case4 any, case5 any, case6 any, case7 any, case8 any, case9 any] struct {
	discriminator sum9CasesEnum

	case1 *case1
	case2 *case2
	case3 *case3
	case4 *case4
	case5 *case5
	case6 *case6
	case7 *case7
	case8 *case8
	case9 *case9
}

func Case1Of9[case1 any, case2 any, case3 any, case4 any, case5 any, case6 any, case7 any, case8 any, case9 any](value case1) Sum9[case1, case2, case3, case4, case5, case6, case7, case8, case9] {
	return Sum9[case1, case2, case3, case4, case5, case6, case7, case8, case9]{
		discriminator: case1Of9,
		case1:         &value,
	}
}

func Case2Of9[case1 any, case2 any, case3 any, case4 any, case5 any, case6 any, case7 any, case8 any, case9 any](value case2) Sum9[case1, case2, case3, case4, case5, case6, case7, case8, case9] {
	return Sum9[case1, case2, case3, case4, case5, case6, case7, case8, case9]{
		discriminator: case2Of9,
		case2:         &value,
	}
}

func Case3Of9[case1 any, case2 any, case3 any, case4 any, case5 any, case6 any, case7 any, case8 any, case9 any](value case3) Sum9[case1, case2, case3, case4, case5, case6, case7, case8, case9] {
	return Sum9[case1, case2, case3, case4, case5, case6, case7, case8, case9]{
		discriminator: case3Of9,
		case3:         &value,
	}
}

func Case4Of9[case1 any, case2 any, case3 any, case4 any, case5 any, case6 any, case7 any, case8 any, case9 any](value case4) Sum9[case1, case2, case3, case4, case5, case6, case7, case8, case9] {
	return Sum9[case1, case2, case3, case4, case5, case6, case7, case8, case9]{
		discriminator: case4Of9,
		case4:         &value,
	}
}

func Case5Of9[case1 any, case2 any, case3 any, case4 any, case5 any, case6 any, case7 any, case8 any, case9 any](value case5) Sum9[case1, case2, case3, case4, case5, case6, case7, case8, case9] {
	return Sum9[case1, case2, case3, case4, case5, case6, case7, case8, case9]{
		discriminator: case5Of9,
		case5:         &value,
	}
}

func Case6Of9[case1 any, case2 any, case3 any, case4 any, case5 any, case6 any, case7 any, case8 any, case9 any](value case6) Sum9[case1, case2, case3, case4, case5, case6, case7, case8, case9] {
	return Sum9[case1, case2, case3, case4, case5, case6, case7, case8, case9]{
		discriminator: case6Of9,
		case6:         &value,
	}
}

func Case7Of9[case1 any, case2 any, case3 any, case4 any, case5 any, case6 any, case7 any, case8 any, case9 any](value case7) Sum9[case1, case2, case3, case4, case5, case6, case7, case8, case9] {
	return Sum9[case1, case2, case3, case4, case5, case6, case7, case8, case9]{
		discriminator: case7Of9,
		case7:         &value,
	}
}

func Case8Of9[case1 any, case2 any, case3 any, case4 any, case5 any, case6 any, case7 any, case8 any, case9 any](value case8) Sum9[case1, case2, case3, case4, case5, case6, case7, case8, case9] {
	return Sum9[case1, case2, case3, case4, case5, case6, case7, case8, case9]{
		discriminator: case8Of9,
		case8:         &value,
	}
}

func Case9Of9[case1 any, case2 any, case3 any, case4 any, case5 any, case6 any, case7 any, case8 any, case9 any](value case9) Sum9[case1, case2, case3, case4, case5, case6, case7, case8, case9] {
	return Sum9[case1, case2, case3, case4, case5, case6, case7, case8, case9]{
		discriminator: case9Of9,
		case9:         &value,
	}
}

func FoldSum9[case1 any, case2 any, case3 any, case4 any, case5 any, case6 any, case7 any, case8 any, case9 any, Result any](
	onCase1 func(case1) (Result, error),
	onCase2 func(case2) (Result, error),
	onCase3 func(case3) (Result, error),
	onCase4 func(case4) (Result, error),
	onCase5 func(case5) (Result, error),
	onCase6 func(case6) (Result, error),
	onCase7 func(case7) (Result, error),
	onCase8 func(case8) (Result, error),
	onCase9 func(case9) (Result, error),
) func(s Sum9[case1, case2, case3, case4, case5, case6, case7, case8, case9]) (Result, error) {
	return func(s Sum9[case1, case2, case3, case4, case5, case6, case7, case8, case9]) (Result, error) {
		switch s.discriminator {
		case case1Of9:
			return onCase1(*s.case1)
		case case2Of9:
			return onCase2(*s.case2)
		case case3Of9:
			return onCase3(*s.case3)
		case case4Of9:
			return onCase4(*s.case4)
		case case5Of9:
			return onCase5(*s.case5)
		case case6Of9:
			return onCase6(*s.case6)
		case case7Of9:
			return onCase7(*s.case7)
		case case8Of9:
			return onCase8(*s.case8)
		case case9Of9:
			return onCase9(*s.case9)
		}
		var nilResult Result
		return nilResult, NewInvalidDiscriminatorError(string(s.discriminator), "Sum9")
	}
}

var _ json.Unmarshaler = &Sum9[Unit, Unit, Unit, Unit, Unit, Unit, Unit, Unit, Unit]{}
var _ json.Marshaler = Sum9[Unit, Unit, Unit, Unit, Unit, Unit, Unit, Unit, Unit]{}

func (d Sum9[case1, case2, case3, case4, case5, case6, case7, case8, case9]) MarshalJSON() ([]byte, error) {
	return json.Marshal(struct {
		Discriminator sum9CasesEnum
		Case1         *case1
		Case2         *case2
		Case3         *case3
		Case4         *case4
		Case5         *case5
		Case6         *case6
		Case7         *case7
		Case8         *case8
		Case9         *case9
	}{
		Discriminator: d.discriminator,
		Case1:         d.case1,
		Case2:         d.case2,
		Case3:         d.case3,
		Case4:         d.case4,
		Case5:         d.case5,
		Case6:         d.case6,
		Case7:         d.case7,
		Case8:         d.case8,
		Case9:         d.case9,
	})
}

func (d *Sum9[case1, case2, case3, case4, case5, case6, case7, case8, case9]) UnmarshalJSON(data []byte) error {
	var aux struct {
		Discriminator sum8CasesEnum
		Case1         *case1
		Case2         *case2
		Case3         *case3
		Case4         *case4
		Case5         *case5
		Case6         *case6
		Case7         *case7
		Case8         *case8
		Case9         *case9
	}
	if err := json.Unmarshal(data, &aux); err != nil {
		return err
	}
	d.discriminator = aux.Discriminator
	d.case1 = aux.Case1
	d.case2 = aux.Case2
	d.case3 = aux.Case3
	d.case4 = aux.Case4
	d.case5 = aux.Case5
	d.case6 = aux.Case6
	d.case7 = aux.Case7
	d.case8 = aux.Case8
	d.case9 = aux.Case9
	return nil
}
