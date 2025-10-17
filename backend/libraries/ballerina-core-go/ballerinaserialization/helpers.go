package ballerinaserialization

import (
	"bytes"
	"encoding/json"
	"fmt"

	ballerina "ballerina.com/core"
)

func goErrorToSum[T, U any](f func(T) (U, error)) func(T) ballerina.Sum[error, U] {
	return func(value T) ballerina.Sum[error, U] {
		result, err := f(value)
		if err != nil {
			return ballerina.Left[error, U](err)
		}
		return ballerina.Right[error, U](result)
	}
}

func sumToGoError[T any](sum ballerina.Sum[error, T]) (T, error) {
	return ballerina.FoldWithError(
		sum,
		func(err error) (T, error) {
			return *new(T), err
		}, func(result T) (T, error) {
			return result, nil
		},
	)
}

// Used in codegen
func WrappedMarshal[T any](value T) ballerina.Sum[error, json.RawMessage] {
	return goErrorToSum(func(value T) (json.RawMessage, error) {
		return json.Marshal(value)
	})(value)
}

// Used in codegen
func WrappedUnmarshal[T any](data json.RawMessage) ballerina.Sum[error, T] {
	return goErrorToSum(func(data json.RawMessage) (T, error) {
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

func (s _sequentialForSerialization) getElementsWithDiscriminator(discriminator string) ballerina.Sum[error, []json.RawMessage] {
	if s.Discriminator != discriminator {
		return ballerina.Left[error, []json.RawMessage](fmt.Errorf("expected discriminator to be '%s', got %s", discriminator, s.Discriminator))
	}
	if s.Value == nil {
		return ballerina.Left[error, []json.RawMessage](fmt.Errorf("missing value field"))
	}
	return ballerina.Right[error, []json.RawMessage](s.Value)
}

// Used in codegen
type recordForSerialization struct {
	Discriminator string               `json:"discriminator"`
	Value         [][2]json.RawMessage `json:"value"`
}

func NewRecordForSerialization(value [][2]json.RawMessage) recordForSerialization {
	return recordForSerialization{
		Discriminator: "record",
		Value:         value,
	}
}

func DeserializeRecord(data json.RawMessage) ballerina.Sum[error, map[string]json.RawMessage] {
	var r recordForSerialization
	err := json.Unmarshal(data, &r)
	if err != nil {
		return ballerina.Left[error, map[string]json.RawMessage](err)
	}
	if r.Discriminator != "record" {
		return ballerina.Left[error, map[string]json.RawMessage](fmt.Errorf("expected discriminator to be 'record', got %s", r.Discriminator))
	}
	if r.Value == nil {
		return ballerina.Left[error, map[string]json.RawMessage](fmt.Errorf("missing value field"))
	}
	fields := make(map[string]json.RawMessage, len(r.Value))
	for i, field := range r.Value {
		var fieldName string
		err := json.Unmarshal(field[0], &fieldName)
		if err != nil {
			return ballerina.Left[error, map[string]json.RawMessage](fmt.Errorf("failed to unmarshal record field name %d: %w", i, err))
		}
		fields[fieldName] = field[1]
	}
	return ballerina.Right[error, map[string]json.RawMessage](fields)
}

func GetRecordFieldByName(fields map[string]json.RawMessage) func(string) ballerina.Sum[error, json.RawMessage] {
	return func(name string) ballerina.Sum[error, json.RawMessage] {
		field, ok := fields[name]
		if !ok {
			return ballerina.Left[error, json.RawMessage](fmt.Errorf("field %s not found", name))
		}
		return ballerina.Right[error, json.RawMessage](field)
	}
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

func DeserializeUnion(data json.RawMessage) ballerina.Sum[error, UnionForSerialization] {
	var u UnionForSerialization
	err := json.Unmarshal(data, &u)
	if err != nil {
		return ballerina.Left[error, UnionForSerialization](err)
	}
	if u.Discriminator != "union" {
		return ballerina.Left[error, UnionForSerialization](fmt.Errorf("expected discriminator to be 'union', got %s", u.Discriminator))
	}
	return ballerina.Right[error, UnionForSerialization](u)
}

func (u UnionForSerialization) GetCaseName() ballerina.Sum[error, string] {
	type caseName struct {
		Name string `json:"name"`
	}
	return unmarshalWithContext(
		"on union case name",
		func(caseName caseName) ballerina.Sum[error, string] {
			return ballerina.Right[error, string](caseName.Name)
		},
	)(u.Value[0])
}
