package ballerina

import (
	"encoding/json"
	"fmt"
)

type Serializer[T any] interface {
	Serialize(T) json.RawMessage
}

type Deserializer[T any] interface {
	Deserialize(json.RawMessage) (T, error)
}

type UnitSerializer struct{}

type unitForSerialization struct {
	Kind string `json:"kind"`
}

func (UnitSerializer) Serialize(Unit) (json.RawMessage, error) {
	return json.Marshal(unitForSerialization{Kind: "unit"})
}

type UnitDeserializer struct{}

func (UnitDeserializer) Deserialize(data json.RawMessage) (Unit, error) {
	var unit unitForSerialization
	err := json.Unmarshal(data, &unit)
	if err != nil {
		return Unit{}, err
	}
	if unit.Kind != "unit" {
		return Unit{}, fmt.Errorf("expected kind to be 'unit', got %s", unit.Kind)
	}
	return Unit{}, nil
}

type SumSerializer[Left any, Right any] struct {
	SerializerLeft  Serializer[Left]
	SerializerRight Serializer[Right]
}

// TODO: rename, current name is only because of nameclash
type sumForSerializationNEW struct {
	Case  string          `json:"case"`
	Value json.RawMessage `json:"value"`
}

func (s SumSerializer[Left, Right]) Serialize(value Sum[Left, Right]) (json.RawMessage, error) {
	return json.Marshal(Fold(value, func(left Left) sumForSerializationNEW {
		return sumForSerializationNEW{Case: "Sum.Left", Value: s.SerializerLeft.Serialize(left)}
	}, func(right Right) sumForSerializationNEW {
		return sumForSerializationNEW{Case: "Sum.Right", Value: s.SerializerRight.Serialize(right)}
	}))
}

type SumDeserializer[a any, b any] struct {
	DeserializerLeft  Deserializer[a]
	DeserializerRight Deserializer[b]
}

func (s SumDeserializer[L, R]) Deserialize(data json.RawMessage) (Sum[L, R], error) {
	var sum sumForSerializationNEW
	err := json.Unmarshal(data, &sum)
	if err != nil {
		return Sum[L, R]{}, err
	}
	switch sum.Case {
	case "Sum.Left":
		left, err := s.DeserializerLeft.Deserialize(sum.Value)
		if err != nil {
			return Sum[L, R]{}, err
		}
		return Left[L, R](left), nil
	case "Sum.Right":
		right, err := s.DeserializerRight.Deserialize(sum.Value)
		if err != nil {
			return Sum[L, R]{}, err
		}
		return Right[L, R](right), nil
	}
	return Sum[L, R]{}, fmt.Errorf("expected case to be 'Sum.Left' or 'Sum.Right', got %s", sum.Case)
}
