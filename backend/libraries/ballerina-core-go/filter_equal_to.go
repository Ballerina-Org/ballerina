package ballerina

import (
	"bytes"
	"encoding/json"
)

type EqualsTo[T any] struct {
	equalsTo T
}

func NewEqualsTo[T any](value T) EqualsTo[T] {
	return EqualsTo[T]{equalsTo: value}
}

func (e EqualsTo[T]) GetEqualsTo() T {
	return e.equalsTo
}

var _ json.Unmarshaler = &EqualsTo[Unit]{}
var _ json.Marshaler = EqualsTo[Unit]{}

func (d EqualsTo[T]) MarshalJSON() ([]byte, error) {
	return json.Marshal(struct {
		EqualsTo T
	}{
		EqualsTo: d.GetEqualsTo(),
	})
}

func (d *EqualsTo[T]) UnmarshalJSON(data []byte) error {
	var aux struct {
		EqualsTo T
	}
	dec := json.NewDecoder(bytes.NewReader(data))
	dec.DisallowUnknownFields()
	if err := dec.Decode(&aux); err != nil {
		return err
	}
	d.equalsTo = aux.EqualsTo
	return nil
}
