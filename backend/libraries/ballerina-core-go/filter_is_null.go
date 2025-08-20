package ballerina

import (
	"encoding/json"
)

type IsNull[T any] struct {
	isNull Unit
}

func NewIsNull[T any]() IsNull[T] {
	return IsNull[T]{}
}

var _ json.Unmarshaler = &IsNull[Unit]{}
var _ json.Marshaler = IsNull[Unit]{}

func (d IsNull[T]) MarshalJSON() ([]byte, error) {
	return json.Marshal(struct {
		IsNull Unit
	}{
		IsNull: d.isNull,
	})
}

func (d *IsNull[T]) UnmarshalJSON(data []byte) error {
	var aux struct {
		IsNull Unit
	}
	if err := json.Unmarshal(data, &aux); err != nil {
		return err
	}
	return nil
}
