package ballerina

import (
	"encoding/json"
	"fmt"
)

// The Serializer function should be total (i.e. never return an error).
// However, we use the json.Marshal to serialize the value under the hood (mainly because of the string serialization in json),
// which can return an error. In theory, we could wrap json.Marshal and panic on error. For that to be safe,
// we would have to prove that it can never happen (which we believe is true, but did not formally prove).
// Thus, this partial signature.

type Serializer[T any] func(T) (json.RawMessage, error)

type Deserializer[T any] func(json.RawMessage) (T, error)

type _unitForSerialization struct {
	Kind string `json:"kind"`
}

func UnitSerializer() Serializer[Unit] {
	return func(value Unit) (json.RawMessage, error) {
		return json.Marshal(_unitForSerialization{Kind: "unit"})
	}
}

func UnitDeserializer() Deserializer[Unit] {
	return func(data json.RawMessage) (Unit, error) {
		var unit _unitForSerialization
		err := json.Unmarshal(data, &unit)
		if err != nil {
			return Unit{}, fmt.Errorf("on unit: %w", err)
		}
		if unit.Kind != "unit" {
			return Unit{}, fmt.Errorf("expected kind to be 'unit', got %s", unit.Kind)
		}
		return Unit{}, nil
	}
}

type _sumForSerialization struct {
	Case  string          `json:"case"`
	Value json.RawMessage `json:"value"`
}

func SumSerializer[L any, R any](leftSerializer Serializer[L], rightSerializer Serializer[R]) Serializer[Sum[L, R]] {
	return func(value Sum[L, R]) (json.RawMessage, error) {
		sumForSerialization, err := FoldWithError(value,
			func(left L) (_sumForSerialization, error) {
				value, err := leftSerializer(left)
				if err != nil {
					return _sumForSerialization{}, fmt.Errorf("on case 'Sum.Left': %w", err)
				}
				return _sumForSerialization{Case: "Sum.Left", Value: value}, nil
			},
			func(right R) (_sumForSerialization, error) {
				value, err := rightSerializer(right)
				if err != nil {
					return _sumForSerialization{}, fmt.Errorf("on case 'Sum.Right': %w", err)
				}
				return _sumForSerialization{Case: "Sum.Right", Value: value}, nil
			},
		)
		if err != nil {
			return json.RawMessage{}, fmt.Errorf("on sum: %w", err)
		}
		return json.Marshal(sumForSerialization)
	}
}

func SumDeserializer[L any, R any](leftDeserializer Deserializer[L], rightDeserializer Deserializer[R]) Deserializer[Sum[L, R]] {
	return func(data json.RawMessage) (Sum[L, R], error) {
		var sum _sumForSerialization
		err := json.Unmarshal(data, &sum)
		if err != nil {
			return Sum[L, R]{}, fmt.Errorf("on sum: %w", err)
		}
		switch sum.Case {
		case "Sum.Left":
			left, err := leftDeserializer(sum.Value)
			if err != nil {
				return Sum[L, R]{}, fmt.Errorf("on case 'Sum.Left': %w", err)
			}
			return Left[L, R](left), nil
		case "Sum.Right":
			right, err := rightDeserializer(sum.Value)
			if err != nil {
				return Sum[L, R]{}, fmt.Errorf("on case 'Sum.Right: %w", err)
			}
			return Right[L, R](right), nil
		}
		return Sum[L, R]{}, fmt.Errorf("expected case to be 'Sum.Left' or 'Sum.Right', got %s", sum.Case)
	}
}
