package ballerina

import (
	"bytes"
	"encoding/json"
)

type GreaterThanOrEqualsTo[T any] struct {
	greaterThanOrEqualsTo T
}

func NewGreaterThanOrEqualsTo[T any](value T) GreaterThanOrEqualsTo[T] {
	return GreaterThanOrEqualsTo[T]{greaterThanOrEqualsTo: value}
}

func (e GreaterThanOrEqualsTo[T]) GetGreaterThanOrEqualsTo() T {
	return e.greaterThanOrEqualsTo
}

var _ json.Unmarshaler = &GreaterThanOrEqualsTo[Unit]{}
var _ json.Marshaler = GreaterThanOrEqualsTo[Unit]{}

func (d GreaterThanOrEqualsTo[T]) MarshalJSON() ([]byte, error) {
	return json.Marshal(struct {
		GreaterThanOrEqualsTo T
	}{
		GreaterThanOrEqualsTo: d.GetGreaterThanOrEqualsTo(),
	})
}

func (d *GreaterThanOrEqualsTo[T]) UnmarshalJSON(data []byte) error {
	var aux struct {
		GreaterThanOrEqualsTo T
	}
	dec := json.NewDecoder(bytes.NewReader(data))
	dec.DisallowUnknownFields()
	if err := dec.Decode(&aux); err != nil {
		return err
	}
	d.greaterThanOrEqualsTo = aux.GreaterThanOrEqualsTo
	return nil
}
