package ballerina

import (
	"bytes"
	"encoding/json"
)

type SmallerThanOrEqualsTo[T any] struct {
	smallerThanOrEqualsTo T
}

func NewSmallerThanOrEqualsTo[T any](value T) SmallerThanOrEqualsTo[T] {
	return SmallerThanOrEqualsTo[T]{smallerThanOrEqualsTo: value}
}

func (e SmallerThanOrEqualsTo[T]) GetSmallerThanOrEqualsTo() T {
	return e.smallerThanOrEqualsTo
}

var _ json.Unmarshaler = &SmallerThanOrEqualsTo[Unit]{}
var _ json.Marshaler = SmallerThanOrEqualsTo[Unit]{}

func (d SmallerThanOrEqualsTo[T]) MarshalJSON() ([]byte, error) {
	return json.Marshal(struct {
		SmallerThanOrEqualsTo T
	}{
		SmallerThanOrEqualsTo: d.GetSmallerThanOrEqualsTo(),
	})
}

func (d *SmallerThanOrEqualsTo[T]) UnmarshalJSON(data []byte) error {
	var aux struct {
		SmallerThanOrEqualsTo T
	}
	dec := json.NewDecoder(bytes.NewReader(data))
	dec.DisallowUnknownFields()
	if err := dec.Decode(&aux); err != nil {
		return err
	}
	d.smallerThanOrEqualsTo = aux.SmallerThanOrEqualsTo
	return nil
}
