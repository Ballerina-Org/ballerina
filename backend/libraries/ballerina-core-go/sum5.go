package ballerina

import (
	"bytes"
	"encoding/json"
)

type sum5CasesEnum string

const (
	case1Of5 sum5CasesEnum = "case1Of5"
	case2Of5 sum5CasesEnum = "case2Of5"
	case3Of5 sum5CasesEnum = "case3Of5"
	case4Of5 sum5CasesEnum = "case4Of5"
	case5Of5 sum5CasesEnum = "case5Of5"
)

var allSum5CasesEnum = [...]sum5CasesEnum{case1Of5, case2Of5, case3Of5, case4Of5, case5Of5}

func DefaultSum5CasesEnum() sum5CasesEnum { return allSum5CasesEnum[0] }

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
	dec := json.NewDecoder(bytes.NewReader(data))
	dec.DisallowUnknownFields()
	if err := dec.Decode(&aux); err != nil {
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
