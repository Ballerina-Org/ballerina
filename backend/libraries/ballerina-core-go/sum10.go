package ballerina

import (
	"bytes"
	"encoding/json"
)

type sum10CasesEnum string

const (
	case1Of10  sum10CasesEnum = "case1Of10"
	case2Of10  sum10CasesEnum = "case2Of10"
	case3Of10  sum10CasesEnum = "case3Of10"
	case4Of10  sum10CasesEnum = "case4Of10"
	case5Of10  sum10CasesEnum = "case5Of10"
	case6Of10  sum10CasesEnum = "case6Of10"
	case7Of10  sum10CasesEnum = "case7Of10"
	case8Of10  sum10CasesEnum = "case8Of10"
	case9Of10  sum10CasesEnum = "case9Of10"
	case10Of10 sum10CasesEnum = "case10Of10"
)

var allSum10CasesEnum = [...]sum10CasesEnum{
	case1Of10, case2Of10, case3Of10, case4Of10, case5Of10,
	case6Of10, case7Of10, case8Of10, case9Of10, case10Of10,
}

func DefaultSum10CasesEnum() sum10CasesEnum { return allSum10CasesEnum[0] }

type Sum10[case1 any, case2 any, case3 any, case4 any, case5 any, case6 any, case7 any, case8 any, case9 any, case10 any] struct {
	discriminator sum10CasesEnum

	case1  *case1
	case2  *case2
	case3  *case3
	case4  *case4
	case5  *case5
	case6  *case6
	case7  *case7
	case8  *case8
	case9  *case9
	case10 *case10
}

func Case1Of10[case1 any, case2 any, case3 any, case4 any, case5 any, case6 any, case7 any, case8 any, case9 any, case10 any](value case1) Sum10[case1, case2, case3, case4, case5, case6, case7, case8, case9, case10] {
	return Sum10[case1, case2, case3, case4, case5, case6, case7, case8, case9, case10]{
		discriminator: case1Of10,
		case1:         &value,
	}
}

func Case2Of10[case1 any, case2 any, case3 any, case4 any, case5 any, case6 any, case7 any, case8 any, case9 any, case10 any](value case2) Sum10[case1, case2, case3, case4, case5, case6, case7, case8, case9, case10] {
	return Sum10[case1, case2, case3, case4, case5, case6, case7, case8, case9, case10]{
		discriminator: case2Of10,
		case2:         &value,
	}
}

func Case3Of10[case1 any, case2 any, case3 any, case4 any, case5 any, case6 any, case7 any, case8 any, case9 any, case10 any](value case3) Sum10[case1, case2, case3, case4, case5, case6, case7, case8, case9, case10] {
	return Sum10[case1, case2, case3, case4, case5, case6, case7, case8, case9, case10]{
		discriminator: case3Of10,
		case3:         &value,
	}
}

func Case4Of10[case1 any, case2 any, case3 any, case4 any, case5 any, case6 any, case7 any, case8 any, case9 any, case10 any](value case4) Sum10[case1, case2, case3, case4, case5, case6, case7, case8, case9, case10] {
	return Sum10[case1, case2, case3, case4, case5, case6, case7, case8, case9, case10]{
		discriminator: case4Of10,
		case4:         &value,
	}
}

func Case5Of10[case1 any, case2 any, case3 any, case4 any, case5 any, case6 any, case7 any, case8 any, case9 any, case10 any](value case5) Sum10[case1, case2, case3, case4, case5, case6, case7, case8, case9, case10] {
	return Sum10[case1, case2, case3, case4, case5, case6, case7, case8, case9, case10]{
		discriminator: case5Of10,
		case5:         &value,
	}
}

func Case6Of10[case1 any, case2 any, case3 any, case4 any, case5 any, case6 any, case7 any, case8 any, case9 any, case10 any](value case6) Sum10[case1, case2, case3, case4, case5, case6, case7, case8, case9, case10] {
	return Sum10[case1, case2, case3, case4, case5, case6, case7, case8, case9, case10]{
		discriminator: case6Of10,
		case6:         &value,
	}
}

func Case7Of10[case1 any, case2 any, case3 any, case4 any, case5 any, case6 any, case7 any, case8 any, case9 any, case10 any](value case7) Sum10[case1, case2, case3, case4, case5, case6, case7, case8, case9, case10] {
	return Sum10[case1, case2, case3, case4, case5, case6, case7, case8, case9, case10]{
		discriminator: case7Of10,
		case7:         &value,
	}
}

func Case8Of10[case1 any, case2 any, case3 any, case4 any, case5 any, case6 any, case7 any, case8 any, case9 any, case10 any](value case8) Sum10[case1, case2, case3, case4, case5, case6, case7, case8, case9, case10] {
	return Sum10[case1, case2, case3, case4, case5, case6, case7, case8, case9, case10]{
		discriminator: case8Of10,
		case8:         &value,
	}
}

func Case9Of10[case1 any, case2 any, case3 any, case4 any, case5 any, case6 any, case7 any, case8 any, case9 any, case10 any](value case9) Sum10[case1, case2, case3, case4, case5, case6, case7, case8, case9, case10] {
	return Sum10[case1, case2, case3, case4, case5, case6, case7, case8, case9, case10]{
		discriminator: case9Of10,
		case9:         &value,
	}
}

func Case10Of10[case1 any, case2 any, case3 any, case4 any, case5 any, case6 any, case7 any, case8 any, case9 any, case10 any](value case10) Sum10[case1, case2, case3, case4, case5, case6, case7, case8, case9, case10] {
	return Sum10[case1, case2, case3, case4, case5, case6, case7, case8, case9, case10]{
		discriminator: case10Of10,
		case10:        &value,
	}
}

func FoldSum10[case1 any, case2 any, case3 any, case4 any, case5 any, case6 any, case7 any, case8 any, case9 any, case10 any, Result any](
	onCase1 func(case1) (Result, error),
	onCase2 func(case2) (Result, error),
	onCase3 func(case3) (Result, error),
	onCase4 func(case4) (Result, error),
	onCase5 func(case5) (Result, error),
	onCase6 func(case6) (Result, error),
	onCase7 func(case7) (Result, error),
	onCase8 func(case8) (Result, error),
	onCase9 func(case9) (Result, error),
	onCase10 func(case10) (Result, error),
) func(s Sum10[case1, case2, case3, case4, case5, case6, case7, case8, case9, case10]) (Result, error) {
	return func(s Sum10[case1, case2, case3, case4, case5, case6, case7, case8, case9, case10]) (Result, error) {
		switch s.discriminator {
		case case1Of10:
			return onCase1(*s.case1)
		case case2Of10:
			return onCase2(*s.case2)
		case case3Of10:
			return onCase3(*s.case3)
		case case4Of10:
			return onCase4(*s.case4)
		case case5Of10:
			return onCase5(*s.case5)
		case case6Of10:
			return onCase6(*s.case6)
		case case7Of10:
			return onCase7(*s.case7)
		case case8Of10:
			return onCase8(*s.case8)
		case case9Of10:
			return onCase9(*s.case9)
		case case10Of10:
			return onCase10(*s.case10)
		}
		var nilResult Result
		return nilResult, NewInvalidDiscriminatorError(string(s.discriminator), "Sum10")
	}
}

var _ json.Unmarshaler = &Sum10[Unit, Unit, Unit, Unit, Unit, Unit, Unit, Unit, Unit, Unit]{}
var _ json.Marshaler = Sum10[Unit, Unit, Unit, Unit, Unit, Unit, Unit, Unit, Unit, Unit]{}

func (d Sum10[case1, case2, case3, case4, case5, case6, case7, case8, case9, case10]) MarshalJSON() ([]byte, error) {
	return json.Marshal(struct {
		Discriminator sum10CasesEnum
		Case1         *case1
		Case2         *case2
		Case3         *case3
		Case4         *case4
		Case5         *case5
		Case6         *case6
		Case7         *case7
		Case8         *case8
		Case9         *case9
		Case10        *case10
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
		Case10:        d.case10,
	})
}

func (d *Sum10[case1, case2, case3, case4, case5, case6, case7, case8, case9, case10]) UnmarshalJSON(data []byte) error {
	var aux struct {
		Discriminator sum10CasesEnum
		Case1         *case1
		Case2         *case2
		Case3         *case3
		Case4         *case4
		Case5         *case5
		Case6         *case6
		Case7         *case7
		Case8         *case8
		Case9         *case9
		Case10        *case10
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
	d.case9 = aux.Case9
	d.case10 = aux.Case10
	return nil
}
