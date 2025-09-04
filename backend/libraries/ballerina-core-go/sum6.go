package ballerina

import (
	"encoding/json"
)

type sum6CasesEnum string

const (
	case1Of6 sum6CasesEnum = "case1Of6"
	case2Of6 sum6CasesEnum = "case2Of6"
	case3Of6 sum6CasesEnum = "case3Of6"
	case4Of6 sum6CasesEnum = "case4Of6"
	case5Of6 sum6CasesEnum = "case5Of6"
	case6Of6 sum6CasesEnum = "case6Of6"
)

var allSum6CasesEnum = [...]sum6CasesEnum{case1Of6, case2Of6, case3Of6, case4Of6, case5Of6, case6Of6}

func DefaultSum6CasesEnum() sum6CasesEnum { return allSum6CasesEnum[0] }

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
