package ballerina

import (
	"errors"
)

type Serializer[T any] interface {
	Serialize(T) Serialized
	Deserialize(Serialized) (T, error)
}

const _KIND_KEY = "kind"

type UnitSerializer struct{}

func (UnitSerializer) Serialize(value Unit) Serialized {
	return Serialized{Value: Case6Of7[Null, string, int64, bool, float64, map[string]Serialized, []Serialized](map[string]Serialized{_KIND_KEY: Serialized{Value: Case2Of7[Null, string, int64, bool, float64, map[string]Serialized, []Serialized]("unit")}})}
}

func (s UnitSerializer) Deserialize(value Serialized) (Unit, error) {
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
