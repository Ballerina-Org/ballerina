package ballerina

import (
	"encoding/json"
)

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
