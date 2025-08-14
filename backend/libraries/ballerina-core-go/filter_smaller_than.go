package ballerina

import (
	"encoding/json"
)

type SmallerThan[T any] struct {
	smallerThan T
}

func NewSmallerThan[T any](value T) SmallerThan[T] {
	return SmallerThan[T]{smallerThan: value}
}

func (e SmallerThan[T]) GetSmallerThan() T {
	return e.smallerThan
}

var _ json.Unmarshaler = &SmallerThan[Unit]{}
var _ json.Marshaler = SmallerThan[Unit]{}

func (d SmallerThan[T]) MarshalJSON() ([]byte, error) {
	return json.Marshal(struct {
		SmallerThan T
	}{
		SmallerThan: d.GetSmallerThan(),
	})
}

func (d *SmallerThan[T]) UnmarshalJSON(data []byte) error {
	var aux struct {
		SmallerThan T
	}
	if err := json.Unmarshal(data, &aux); err != nil {
		return err
	}
	d.smallerThan = aux.SmallerThan
	return nil
}
