package ballerinaserialization

import (
	"encoding/json"
	"fmt"
	"strconv"
	"time"

	ballerina "ballerina.com/core"
)

type _unitForSerialization struct {
	Kind string `json:"kind"`
}

var UnitSerializer Serializer[ballerina.Unit] = withContext("on unit", func(value ballerina.Unit) ballerina.Sum[error, json.RawMessage] {
	return wrappedMarshal(_unitForSerialization{Kind: "unit"})
})

var UnitDeserializer Deserializer[ballerina.Unit] = unmarshalWithContext("on unit", func(unitForSerialization _unitForSerialization) ballerina.Sum[error, ballerina.Unit] {
	if unitForSerialization.Kind != "unit" {
		return ballerina.Left[error, ballerina.Unit](fmt.Errorf("expected kind to be 'unit', got '%s'", unitForSerialization.Kind))
	}
	return ballerina.Right[error, ballerina.Unit](ballerina.Unit{})
})

type _sequentialForSerialization struct {
	Kind     string            `json:"kind"`
	Elements []json.RawMessage `json:"elements"`
}

func (s _sequentialForSerialization) getElementsWithKind(kind string) ballerina.Sum[error, []json.RawMessage] {
	if s.Kind != kind {
		return ballerina.Left[error, []json.RawMessage](fmt.Errorf("expected kind to be '%s', got %s", kind, s.Kind))
	}
	return ballerina.Right[error, []json.RawMessage](s.Elements)
}

var StringSerializer Serializer[string] = withContext("on string", wrappedMarshal[string])

var StringDeserializer Deserializer[string] = withContext("on string", wrappedUnmarshal[string])

var BoolSerializer Serializer[bool] = withContext("on bool", wrappedMarshal[bool])

var BoolDeserializer Deserializer[bool] = withContext("on bool", wrappedUnmarshal[bool])

type _primitiveTypeForSerialization struct {
	Kind  string `json:"kind"`
	Value string `json:"value"`
}

func (s _primitiveTypeForSerialization) getValueWithKind(kind string) ballerina.Sum[error, string] {
	if s.Kind != kind {
		return ballerina.Left[error, string](fmt.Errorf("expected kind to be '%s', got %s", kind, s.Kind))
	}
	return ballerina.Right[error, string](s.Value)
}

func serializePrimitiveTypeFrom[T any](kind string, serialize func(T) string) func(T) ballerina.Sum[error, json.RawMessage] {
	return withContext("on "+kind, func(value T) ballerina.Sum[error, json.RawMessage] {
		return wrappedMarshal(_primitiveTypeForSerialization{Kind: kind, Value: serialize(value)})
	})
}

func deserializePrimitiveTypeTo[T any](kind string, parse func(string) (T, error)) func(json.RawMessage) ballerina.Sum[error, T] {
	return unmarshalWithContext("on "+kind, func(primitiveTypeForSerialization _primitiveTypeForSerialization) ballerina.Sum[error, T] {
		return ballerina.Bind(primitiveTypeForSerialization.getValueWithKind(kind), ballerina.GoErrorToSum(parse))
	})
}

var IntSerializer Serializer[int64] = serializePrimitiveTypeFrom("int", func(value int64) string {
	return strconv.FormatInt(value, 10)
})

var IntDeserializer Deserializer[int64] = deserializePrimitiveTypeTo("int", func(value string) (int64, error) {
	return strconv.ParseInt(value, 10, 64)
})

var FloatSerializer Serializer[float64] = serializePrimitiveTypeFrom("float", func(value float64) string {
	return strconv.FormatFloat(value, 'f', -1, 64)
})

var FloatDeserializer Deserializer[float64] = deserializePrimitiveTypeTo("float", func(value string) (float64, error) {
	return strconv.ParseFloat(value, 64)
})

var DateSerializer Serializer[time.Time] = serializePrimitiveTypeFrom("date", func(value time.Time) string {
	return value.Format(time.DateOnly)
})

var DateDeserializer Deserializer[time.Time] = deserializePrimitiveTypeTo("date", func(value string) (time.Time, error) {
	return time.Parse(time.DateOnly, value)
})
