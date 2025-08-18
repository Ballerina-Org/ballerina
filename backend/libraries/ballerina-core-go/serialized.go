package ballerina

import (
	"encoding/json"
	"errors"
)

type Null struct{}

func (Null) MarshalJSON() ([]byte, error) {
	return []byte("null"), nil
}

func (Null) UnmarshalJSON(data []byte) error {
	if string(data) == "null" {
		return nil
	}
	return errors.New("expected null")
}

type Serialized struct {
	Value Sum7[Null, string, int64, bool, float64, map[string]Serialized, []Serialized]
}

func (Serialized) Null() Serialized {
	return Serialized{Value: Case1Of7[Null, string, int64, bool, float64, map[string]Serialized, []Serialized](Null{})}
}

func (Serialized) String(s string) Serialized {
	return Serialized{Value: Case2Of7[Null, string, int64, bool, float64, map[string]Serialized, []Serialized](s)}
}

func (Serialized) Int64(i int64) Serialized {
	return Serialized{Value: Case3Of7[Null, string, int64, bool, float64, map[string]Serialized, []Serialized](i)}
}

func (Serialized) Bool(b bool) Serialized {
	return Serialized{Value: Case4Of7[Null, string, int64, bool, float64, map[string]Serialized, []Serialized](b)}
}

func (Serialized) Float64(f float64) Serialized {
	return Serialized{Value: Case5Of7[Null, string, int64, bool, float64, map[string]Serialized, []Serialized](f)}
}

func (Serialized) Map(m map[string]Serialized) Serialized {
	return Serialized{Value: Case6Of7[Null, string, int64, bool, float64, map[string]Serialized, []Serialized](m)}
}

func (Serialized) Array(a []Serialized) Serialized {
	return Serialized{Value: Case7Of7[Null, string, int64, bool, float64, map[string]Serialized, []Serialized](a)}
}

func FoldSerialized[Result any](onNull func(Null) (Result, error),
	onString func(string) (Result, error),
	onInt64 func(int64) (Result, error),
	onBool func(bool) (Result, error),
	onFloat64 func(float64) (Result, error),
	onMap func(map[string]Serialized) (Result, error),
	onArray func([]Serialized) (Result, error)) func(Serialized) (Result, error) {
	return func(s Serialized) (Result, error) {
		return FoldSum7(
			onNull,
			onString,
			onInt64,
			onBool,
			onFloat64,
			onMap,
			onArray,
		)(s.Value)
	}
}

func (s Serialized) MarshalJSON() ([]byte, error) {
	serializeNull := func(data Null) ([]byte, error) {
		return json.Marshal(data)
	}
	serializeString := func(data string) ([]byte, error) {
		return json.Marshal(data)
	}
	serializeInt64 := func(data int64) ([]byte, error) {
		return json.Marshal(data)
	}
	serializeBool := func(data bool) ([]byte, error) {
		return json.Marshal(data)
	}
	serializeFloat64 := func(data float64) ([]byte, error) {
		return json.Marshal(data)
	}
	serializeMap := func(data map[string]Serialized) ([]byte, error) {
		return json.Marshal(data)
	}
	serializeArray := func(data []Serialized) ([]byte, error) {
		return json.Marshal(data)
	}
	output, err := FoldSum7(
		serializeNull,
		serializeString,
		serializeInt64,
		serializeBool,
		serializeFloat64,
		serializeMap,
		serializeArray,
	)(s.Value)
	return output, err
}

func (s *Serialized) UnmarshalJSON(data []byte) error {
	deserialized, err := firstSuccessful(
		func() (Serialized, error) { return tryUnmarshalAs[Null](data, Case1Of7) },
		func() (Serialized, error) { return tryUnmarshalAs[string](data, Case2Of7) },
		func() (Serialized, error) { return tryUnmarshalAs[int64](data, Case3Of7) },
		func() (Serialized, error) { return tryUnmarshalAs[bool](data, Case4Of7) },
		func() (Serialized, error) { return tryUnmarshalAs[float64](data, Case5Of7) },
		func() (Serialized, error) { return tryUnmarshalAs[map[string]Serialized](data, Case6Of7) },
		func() (Serialized, error) { return tryUnmarshalAs[[]Serialized](data, Case7Of7) },
	)
	if err != nil {
		return err
	}
	*s = deserialized
	return nil
}
func firstSuccessful[T any](fns ...func() (T, error)) (T, error) {
	var errs []error
	for _, fn := range fns {
		if out, err := fn(); err == nil {
			return out, nil
		} else {
			errs = append(errs, err)
		}
	}
	var zero T
	return zero, errors.Join(errs...)
}

func tryUnmarshalAs[T any](data []byte, f func(T) Sum7[Null, string, int64, bool, float64, map[string]Serialized, []Serialized]) (Serialized, error) {
	var t T
	err := json.Unmarshal(data, &t)
	if err != nil {
		return Serialized{}, err
	}
	return Serialized{Value: f(t)}, nil
}
