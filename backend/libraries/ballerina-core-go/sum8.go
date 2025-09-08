package ballerina

import (
	"bytes"
	"encoding/json"
)

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

var allSum8CasesEnum = [...]sum8CasesEnum{case1Of8, case2Of8, case3Of8, case4Of8, case5Of8, case6Of8, case7Of8, case8Of8}

func DefaultSum8CasesEnum() sum8CasesEnum { return allSum8CasesEnum[0] }

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
	d.case6 = aux.Case6
	d.case7 = aux.Case7
	d.case8 = aux.Case8
	return nil
}
