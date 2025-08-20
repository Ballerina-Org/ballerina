package ballerina

import (
	"encoding/json"
)

type IsNotNull[T any] struct {
	isNotNull Unit
}

func NewIsNotNull[T any]() IsNotNull[T] {
	return IsNotNull[T]{}
}

var _ json.Unmarshaler = &IsNotNull[Unit]{}
var _ json.Marshaler = IsNotNull[Unit]{}

func (d IsNotNull[T]) MarshalJSON() ([]byte, error) {
	return json.Marshal(struct {
		IsNotNull Unit
	}{
		IsNotNull: d.isNotNull,
	})
}

func (d *IsNotNull[T]) UnmarshalJSON(data []byte) error {
	var aux struct {
		IsNotNull Unit
	}
	if err := json.Unmarshal(data, &aux); err != nil {
		return err
	}
	return nil
}
