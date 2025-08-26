package ballerina

import (
	"encoding/json"
)

type sum1CasesEnum string

const (
	case1Of1 sum1CasesEnum = "case1Of1"
)

var AllSum1CasesEnum = [...]sum1CasesEnum{case1Of1}

func DefaultSum1CasesEnum() sum1CasesEnum { return AllSum1CasesEnum[0] }

type Sum1[case1 any] struct {
	discriminator sum1CasesEnum

	case1 *case1
}

func Case1Of1[case1 any](value case1) Sum1[case1] {
	return Sum1[case1]{
		discriminator: case1Of1,
		case1:         &value,
	}
}

func FoldSum1[case1 any, Result any](onCase1 func(case1) (Result, error)) func(s Sum1[case1]) (Result, error) {
	return func(s Sum1[case1]) (Result, error) {
		switch s.discriminator {
		case case1Of1:
			return onCase1(*s.case1)
		}
		var nilResult Result
		return nilResult, NewInvalidDiscriminatorError(string(s.discriminator), "Sum1")
	}
}

var (
	_ json.Unmarshaler = &Sum1[Unit]{}
	_ json.Marshaler   = Sum1[Unit]{}
)

func (d Sum1[case1]) MarshalJSON() ([]byte, error) {
	return json.Marshal(struct {
		Discriminator sum1CasesEnum
		Case1         *case1
	}{
		Discriminator: d.discriminator,
		Case1:         d.case1,
	})
}

func (d *Sum1[case1]) UnmarshalJSON(data []byte) error {
	var aux struct {
		Discriminator sum1CasesEnum
		Case1         *case1
	}
	if err := json.Unmarshal(data, &aux); err != nil {
		return err
	}
	d.discriminator = aux.Discriminator
	d.case1 = aux.Case1
	return nil
}
