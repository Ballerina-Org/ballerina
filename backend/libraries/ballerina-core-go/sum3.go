package ballerina

import (
	"encoding/json"
)

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
