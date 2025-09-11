package ballerina

import (
	"bytes"
	"encoding/json"
)

type sum4CasesEnum string

const (
	case1Of4 sum4CasesEnum = "case1Of4"
	case2Of4 sum4CasesEnum = "case2Of4"
	case3Of4 sum4CasesEnum = "case3Of4"
	case4Of4 sum4CasesEnum = "case4Of4"
)

var allSum4CasesEnum = [...]sum4CasesEnum{case1Of4, case2Of4, case3Of4, case4Of4}

func DefaultSum4CasesEnum() sum4CasesEnum { return allSum4CasesEnum[0] }

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
	return nil
}
