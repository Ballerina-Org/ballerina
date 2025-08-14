package ballerina

import (
	"encoding/json"
)

type GreaterThan[T any] struct {
	greaterThan T
}

func NewGreaterThan[T any](value T) GreaterThan[T] {
	return GreaterThan[T]{greaterThan: value}
}

func (e GreaterThan[T]) GetGreaterThan() T {
	return e.greaterThan
}

var _ json.Unmarshaler = &GreaterThan[Unit]{}
var _ json.Marshaler = GreaterThan[Unit]{}

func (d GreaterThan[T]) MarshalJSON() ([]byte, error) {
	return json.Marshal(struct {
		GreaterThan T
	}{
		GreaterThan: d.GetGreaterThan(),
	})
}

func (d *GreaterThan[T]) UnmarshalJSON(data []byte) error {
	var aux struct {
		GreaterThan T
	}
	if err := json.Unmarshal(data, &aux); err != nil {
		return err
	}
	d.greaterThan = aux.GreaterThan
	return nil
}
