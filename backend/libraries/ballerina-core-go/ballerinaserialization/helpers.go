package ballerinaserialization

import (
	"bytes"
	"encoding/json"
	"fmt"

	ballerina "ballerina.com/core"
)

// Used in codegen
func WrappedMarshal[T any](value T) ballerina.Sum[error, json.RawMessage] {
	return ballerina.GoErrorToSum(func(value T) (json.RawMessage, error) {
		return json.Marshal(value)
	})(value)
}

// Used in codegen
func WrappedUnmarshal[T any](data json.RawMessage) ballerina.Sum[error, T] {
	return ballerina.GoErrorToSum(func(data json.RawMessage) (T, error) {
		var value T
		decoder := json.NewDecoder(bytes.NewReader(data))
		decoder.DisallowUnknownFields()
		err := decoder.Decode(&value)
		return value, err
	})(data)
}

// Used in codegen
func WithContext[I any, T any](context string, f func(I) ballerina.Sum[error, T]) func(I) ballerina.Sum[error, T] {
	return func(value I) ballerina.Sum[error, T] {
		return ballerina.MapLeft[error, T](f(value), func(err error) error {
			return fmt.Errorf("%s: %w", context, err)
		})
	}
}

func unmarshalWithContext[T any, U any](context string, f func(T) ballerina.Sum[error, U]) func(json.RawMessage) ballerina.Sum[error, U] {
	return WithContext(context, func(data json.RawMessage) ballerina.Sum[error, U] {
		return ballerina.Bind(WrappedUnmarshal[T](data), f)
	})
}

type _sequentialForSerialization struct {
	Discriminator string            `json:"discriminator"`
	Value         []json.RawMessage `json:"value"`
}

func (s _sequentialForSerialization) getElementsWithDiscriminator(kind string) ballerina.Sum[error, []json.RawMessage] {
	if s.Discriminator != kind {
		return ballerina.Left[error, []json.RawMessage](fmt.Errorf("expected kind to be '%s', got %s", kind, s.Discriminator))
	}
	if s.Value == nil {
		return ballerina.Left[error, []json.RawMessage](fmt.Errorf("missing value field"))
	}
	return ballerina.Right[error, []json.RawMessage](s.Value)
}

// Used in codegen
type RecordForSerialization struct {
	Discriminator string               `json:"discriminator"`
	Value         [][2]json.RawMessage `json:"value"`
}

func NewRecordForSerialization(value [][2]json.RawMessage) RecordForSerialization {
	return RecordForSerialization{
		Discriminator: "record",
		Value:         value,
	}
}

var _ json.Unmarshaler = (*RecordForSerialization)(nil)

func (r *RecordForSerialization) UnmarshalJSON(data []byte) error {
	err := json.Unmarshal(data, r)
	if err != nil {
		return err
	}
	if r.Discriminator != "record" {
		return fmt.Errorf("expected discriminator to be 'record', got %s", r.Discriminator)
	}
	if r.Value == nil {
		return fmt.Errorf("missing value field")
	}
	return nil
}

// Used in codegen
type UnionForSerialization struct {
	Discriminator string             `json:"discriminator"`
	Value         [2]json.RawMessage `json:"value"`
}

func NewUnionForSerialization(caseName string, value json.RawMessage) UnionForSerialization {
	return UnionForSerialization{
		Discriminator: "union",
		Value:         [2]json.RawMessage{json.RawMessage(`{"name": "` + caseName + `"}`), value},
	}
}

var _ json.Unmarshaler = (*UnionForSerialization)(nil)

func (u *UnionForSerialization) UnmarshalJSON(data []byte) error {
	err := json.Unmarshal(data, u)
	if err != nil {
		return err
	}
	if u.Discriminator != "union" {
		return fmt.Errorf("expected discriminator to be 'union', got %s", u.Discriminator)
	}
	return nil
}

func (u UnionForSerialization) GetCaseName() ballerina.Sum[error, string] {
	type caseName struct {
		Name string `json:"name"`
	}
	return ballerina.Bind(
		WrappedUnmarshal[caseName](u.Value[0]),
		func(caseName caseName) ballerina.Sum[error, string] {
			return ballerina.Right[error, string](caseName.Name)
		},
	)
}
