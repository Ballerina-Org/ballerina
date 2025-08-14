package ballerina

import (
	"encoding/json"
)

type NotEqualsTo[T any] struct {
	notEqualsTo T
}

func NewNotEqualsTo[T any](value T) NotEqualsTo[T] {
	return NotEqualsTo[T]{notEqualsTo: value}
}

func (e NotEqualsTo[T]) GetNotEqualsTo() T {
	return e.notEqualsTo
}

var _ json.Unmarshaler = &NotEqualsTo[Unit]{}
var _ json.Marshaler = NotEqualsTo[Unit]{}

func (d NotEqualsTo[T]) MarshalJSON() ([]byte, error) {
	return json.Marshal(struct {
		NotEqualsTo T
	}{
		NotEqualsTo: d.GetNotEqualsTo(),
	})
}

func (d *NotEqualsTo[T]) UnmarshalJSON(data []byte) error {
	var aux struct {
		NotEqualsTo T
	}
	if err := json.Unmarshal(data, &aux); err != nil {
		return err
	}
	d.notEqualsTo = aux.NotEqualsTo
	return nil
}
