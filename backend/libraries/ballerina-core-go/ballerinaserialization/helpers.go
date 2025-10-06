package ballerinaserialization

import (
	"bytes"
	"encoding/json"
	"fmt"

	ballerina "ballerina.com/core"
)

func wrappedMarshal[T any](value T) ballerina.Sum[error, json.RawMessage] {
	return ballerina.GoErrorToSum(func(value T) (json.RawMessage, error) {
		return json.Marshal(value)
	})(value)
}

func wrappedUnmarshal[T any](data json.RawMessage) ballerina.Sum[error, T] {
	return ballerina.GoErrorToSum(func(data json.RawMessage) (T, error) {
		var value T
		decoder := json.NewDecoder(bytes.NewReader(data))
		decoder.DisallowUnknownFields()
		err := decoder.Decode(&value)
		return value, err
	})(data)
}

func withContext[I any, T any](context string, f func(I) ballerina.Sum[error, T]) func(I) ballerina.Sum[error, T] {
	return func(value I) ballerina.Sum[error, T] {
		return ballerina.MapLeft[error, T](f(value), func(err error) error {
			return fmt.Errorf("%s: %w", context, err)
		})
	}
}

func unmarshalWithContext[T any, U any](context string, f func(T) ballerina.Sum[error, U]) func(json.RawMessage) ballerina.Sum[error, U] {
	return withContext(context, func(data json.RawMessage) ballerina.Sum[error, U] {
		return ballerina.Bind(wrappedUnmarshal[T](data), f)
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
