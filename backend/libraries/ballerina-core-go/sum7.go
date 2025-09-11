package ballerina

import (
	"bytes"
	"encoding/json"
)

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

var allSum7CasesEnum = [...]sum7CasesEnum{case1Of7, case2Of7, case3Of7, case4Of7, case5Of7, case6Of7, case7Of7}

func DefaultSum7CasesEnum() sum7CasesEnum { return allSum7CasesEnum[0] }

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
	return nil
}
