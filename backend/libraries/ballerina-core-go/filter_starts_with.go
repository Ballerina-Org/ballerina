package ballerina

import (
	"encoding/json"
)

type StartsWith[T any] struct {
	startsWith T
}

func NewStartsWith[T any](value T) StartsWith[T] {
	return StartsWith[T]{startsWith: value}
}

func (c StartsWith[T]) GetStartsWith() T {
	return c.startsWith
}

var _ json.Unmarshaler = &StartsWith[Unit]{}
var _ json.Marshaler = StartsWith[Unit]{}

func (d StartsWith[T]) MarshalJSON() ([]byte, error) {
	return json.Marshal(struct {
		StartsWith T
	}{
		StartsWith: d.GetStartsWith(),
	})
}

func (d *StartsWith[T]) UnmarshalJSON(data []byte) error {
	var aux struct {
		StartsWith T
	}
	if err := json.Unmarshal(data, &aux); err != nil {
		return err
	}
	d.startsWith = aux.StartsWith
	return nil
}
