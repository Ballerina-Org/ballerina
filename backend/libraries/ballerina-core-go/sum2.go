package ballerina

import (
	"encoding/json"
)

type sum2CasesEnum string

const (
	case1Of2 sum2CasesEnum = "case1Of2"
	case2Of2 sum2CasesEnum = "case2Of2"
)

var allSum2CasesEnum = [...]sum2CasesEnum{case1Of2, case2Of2}

func DefaultSum2CasesEnum() sum2CasesEnum { return allSum2CasesEnum[0] }

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
