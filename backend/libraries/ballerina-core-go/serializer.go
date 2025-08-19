package ballerina

import (
	"errors"
)

type Serializer[T any] interface {
	Serialize(T) Serialized
}

type Deserializer[T any] interface {
	Deserialize(Serialized) (T, error)
}

const _KIND_KEY = "kind"

type UnitSerializer struct{}

func (UnitSerializer) Serialize(value Unit) Serialized {
	return Serialized{}.Map(map[string]Serialized{_KIND_KEY: Serialized{}.String("unit")})
}

type UnitDeserializer struct{}

func (UnitDeserializer) Deserialize(value Serialized) (Unit, error) {
	err := errors.New("not a unit")
	return FoldSerialized(
		func(Null) (Unit, error) { return Unit{}, err },
		func(string) (Unit, error) { return Unit{}, err },
		func(int64) (Unit, error) { return Unit{}, err },
		func(bool) (Unit, error) { return Unit{}, err },
		func(float64) (Unit, error) { return Unit{}, err },
		func(m map[string]Serialized) (Unit, error) {
			if value, ok := m[_KIND_KEY]; ok {
				isUnit, err := FoldSerialized(
					func(Null) (bool, error) { return false, nil },
					func(s string) (bool, error) { return s == "unit", nil },
					func(int64) (bool, error) { return false, nil },
					func(bool) (bool, error) { return false, nil },
					func(float64) (bool, error) { return false, nil },
					func(map[string]Serialized) (bool, error) { return false, nil },
					func([]Serialized) (bool, error) { return false, nil },
				)(value)
				if err != nil {
					return Unit{}, err
				}
				if isUnit {
					return Unit{}, nil
				}
			}
			return Unit{}, err
		},
		func([]Serialized) (Unit, error) { return Unit{}, err },
	)(value)
}

type SumSerializer[a any, b any] struct {
	SerializerLeft  Serializer[a]
	SerializerRight Serializer[b]
}

func (s SumSerializer[a, b]) Serialize(value Sum[a, b]) Serialized {
	return Fold(value,
		func(left a) Serialized {
			return Serialized{}.Map(map[string]Serialized{"case": Serialized{}.String("Sum.Left"), "value": s.SerializerLeft.Serialize(left)})
		},
		func(right b) Serialized {
			return Serialized{}.Map(map[string]Serialized{"case": Serialized{}.String("Sum.Right"), "value": s.SerializerRight.Serialize(right)})
		},
	)
}

type SumDeserializer[a any, b any] struct {
	DeserializerLeft  Deserializer[a]
	DeserializerRight Deserializer[b]
}

func (s SumDeserializer[a, b]) Deserialize(value Serialized) (Sum[a, b], error) {
	err := errors.New("not a sum")
	return FoldSerialized(
		func(Null) (Sum[a, b], error) { return Sum[a, b]{}, err },
		func(string) (Sum[a, b], error) { return Sum[a, b]{}, err },
		func(int64) (Sum[a, b], error) { return Sum[a, b]{}, err },
		func(bool) (Sum[a, b], error) { return Sum[a, b]{}, err },
		func(float64) (Sum[a, b], error) { return Sum[a, b]{}, err },
		func(m map[string]Serialized) (Sum[a, b], error) {
			if caseValue, ok := m["case"]; ok {
				isLeft, err := FoldSerialized(
					func(Null) (bool, error) { return false, err },
					func(s string) (bool, error) { return s == "Sum.Left", nil },
					func(int64) (bool, error) { return false, err },
					func(bool) (bool, error) { return false, err },
					func(float64) (bool, error) { return false, err },
					func(map[string]Serialized) (bool, error) { return false, err },
					func([]Serialized) (bool, error) { return false, err },
				)(caseValue)
				if err != nil {
					return Sum[a, b]{}, err
				}
				valueValue, ok := m["value"]
				if !ok {
					return Sum[a, b]{}, err
				}
				if isLeft {
					left, err := s.DeserializerLeft.Deserialize(valueValue)
					if err != nil {
						return Sum[a, b]{}, err
					}
					return Left[a, b](left), nil
				} else {
					right, err := s.DeserializerRight.Deserialize(valueValue)
					if err != nil {
						return Sum[a, b]{}, err
					}
					return Right[a, b](right), nil
				}
			}
			return Sum[a, b]{}, err
		},
		func([]Serialized) (Sum[a, b], error) { return Sum[a, b]{}, err },
	)(value)
}
