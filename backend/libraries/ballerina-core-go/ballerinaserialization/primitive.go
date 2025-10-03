package ballerinaserialization

import (
	"encoding/json"
	"fmt"
	"strconv"
	"time"

	ballerina "ballerina.com/core"
)

const unitDiscriminator = "unit"

type _unitForSerialization struct {
	Kind string `json:"kind"`
}

var UnitSerializer Serializer[ballerina.Unit] = withContext(fmt.Sprintf("on %s", unitDiscriminator), func(value ballerina.Unit) ballerina.Sum[error, json.RawMessage] {
	return wrappedMarshal(_unitForSerialization{Kind: unitDiscriminator})
})

var UnitDeserializer Deserializer[ballerina.Unit] = unmarshalWithContext(fmt.Sprintf("on %s", unitDiscriminator), func(unitForSerialization _unitForSerialization) ballerina.Sum[error, ballerina.Unit] {
	if unitForSerialization.Kind != unitDiscriminator {
		return ballerina.Left[error, ballerina.Unit](fmt.Errorf("expected kind to be '%s', got '%s'", unitDiscriminator, unitForSerialization.Kind))
	}
	return ballerina.Right[error, ballerina.Unit](ballerina.Unit{})
})

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

const stringDiscriminator = "string"

var StringSerializer Serializer[string] = serializePrimitiveTypeFrom(stringDiscriminator, func(value string) string {
	return value
})

var StringDeserializer Deserializer[string] = deserializePrimitiveTypeTo(stringDiscriminator, func(value string) (string, error) {
	return value, nil
})

const boolDiscriminator = "bool"

var BoolSerializer Serializer[bool] = serializePrimitiveTypeFrom(boolDiscriminator, func(value bool) string {
	return strconv.FormatBool(value)
})

var BoolDeserializer Deserializer[bool] = deserializePrimitiveTypeTo(boolDiscriminator, func(value string) (bool, error) {
	return strconv.ParseBool(value)
})

const intDiscriminator = "int"

var IntSerializer Serializer[int64] = serializePrimitiveTypeFrom(intDiscriminator, func(value int64) string {
	return strconv.FormatInt(value, 10)
})

var IntDeserializer Deserializer[int64] = deserializePrimitiveTypeTo(intDiscriminator, func(value string) (int64, error) {
	return strconv.ParseInt(value, 10, 64)
})

const floatDiscriminator = "float"

var FloatSerializer Serializer[float64] = serializePrimitiveTypeFrom(floatDiscriminator, func(value float64) string {
	return strconv.FormatFloat(value, 'f', -1, 64)
})

var FloatDeserializer Deserializer[float64] = deserializePrimitiveTypeTo(floatDiscriminator, func(value string) (float64, error) {
	return strconv.ParseFloat(value, 64)
})

const dateDiscriminator = "date"

var DateSerializer Serializer[time.Time] = serializePrimitiveTypeFrom(dateDiscriminator, func(value time.Time) string {
	return value.Format(time.DateOnly)
})

var DateDeserializer Deserializer[time.Time] = deserializePrimitiveTypeTo(dateDiscriminator, func(value string) (time.Time, error) {
	return time.Parse(time.DateOnly, value)
})
