package ballerina

import (
	"bytes"
	"encoding/json"
)

type Contains[T any] struct {
	contains T
}

func NewContains[T any](value T) Contains[T] {
	return Contains[T]{contains: value}
}

func (e Contains[T]) GetContains() T {
	return e.contains
}

var _ json.Unmarshaler = &Contains[Unit]{}
var _ json.Marshaler = Contains[Unit]{}

func (d Contains[T]) MarshalJSON() ([]byte, error) {
	return json.Marshal(struct {
		Contains T
	}{
		Contains: d.contains,
	})
}

func (d *Contains[T]) UnmarshalJSON(data []byte) error {
	var aux struct {
		Contains T
	}
	dec := json.NewDecoder(bytes.NewReader(data))
	dec.DisallowUnknownFields()
	if err := dec.Decode(&aux); err != nil {
		return err
	}
	d.contains = aux.Contains
	return nil
}
