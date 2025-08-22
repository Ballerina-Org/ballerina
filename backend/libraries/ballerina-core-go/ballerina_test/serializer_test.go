package ballerina_test

import (
	"encoding/json"
	"testing"

	ballerina "ballerina.com/core"
	"github.com/stretchr/testify/require"
)

func TestUnitSerialization(t *testing.T) {
	t.Parallel()
	serializer := ballerina.UnitSerializer()
	unit := ballerina.Unit{}
	serialized := serializer(unit)
	require.Equal(t, ballerina.Right[error, json.RawMessage](json.RawMessage(`{"kind":"unit"}`)), serialized)
}

func TestUnitDeserialization(t *testing.T) {
	t.Parallel()
	deserializer := ballerina.UnitDeserializer()
	serialized := json.RawMessage(`{"kind":"unit"}`)
	deserialized := deserializer(serialized)
	require.Equal(t, ballerina.Right[error, ballerina.Unit](ballerina.Unit{}), deserialized)
}

func TestSumSerialization(t *testing.T) {
	t.Parallel()
	serializer := ballerina.SumSerializer(ballerina.UnitSerializer(), ballerina.UnitSerializer())

	sumLeft := ballerina.Left[ballerina.Unit, ballerina.Unit](ballerina.Unit{})
	serializedLeft := serializer(sumLeft)
	require.Equal(t, ballerina.Right[error, json.RawMessage](json.RawMessage(`{"case":"Sum.Left","value":{"kind":"unit"}}`)), serializedLeft)

	sumRight := ballerina.Right[ballerina.Unit, ballerina.Unit](ballerina.Unit{})
	serializedRight := serializer(sumRight)
	require.Equal(t, ballerina.Right[error, json.RawMessage](json.RawMessage(`{"case":"Sum.Right","value":{"kind":"unit"}}`)), serializedRight)
}

func TestSumDeserialization(t *testing.T) {
	t.Parallel()
	deserializer := ballerina.SumDeserializer(ballerina.UnitDeserializer(), ballerina.UnitDeserializer())

	serializedLeft := json.RawMessage(`{"case":"Sum.Left","value":{"kind":"unit"}}`)
	deserializedLeft := deserializer(serializedLeft)
	require.Equal(t, ballerina.Right[error, ballerina.Sum[ballerina.Unit, ballerina.Unit]](ballerina.Left[ballerina.Unit, ballerina.Unit](ballerina.Unit{})), deserializedLeft)

	serializedRight := json.RawMessage(`{"case":"Sum.Right","value":{"kind":"unit"}}`)
	deserializedRight := deserializer(serializedRight)
	require.Equal(t, ballerina.Right[error, ballerina.Sum[ballerina.Unit, ballerina.Unit]](ballerina.Right[ballerina.Unit, ballerina.Unit](ballerina.Unit{})), deserializedRight)
}

func TestStringSerialization(t *testing.T) {
	t.Parallel()
	serializer := ballerina.StringSerializer()
	string := `he\nllo`
	serialized := serializer(string)
	require.Equal(t, ballerina.Right[error, json.RawMessage](json.RawMessage(`"he\\nllo"`)), serialized)
}

func TestStringDeserialization(t *testing.T) {
	t.Parallel()
	deserializer := ballerina.StringDeserializer()
	serialized := json.RawMessage(`"he\\nllo"`)
	deserialized := deserializer(serialized)
	require.Equal(t, ballerina.Right[error, string](`he\nllo`), deserialized)
}

func TestBoolSerialization(t *testing.T) {
	t.Parallel()
	serializer := ballerina.BoolSerializer()
	bool := true
	serialized := serializer(bool)
	require.Equal(t, ballerina.Right[error, json.RawMessage](json.RawMessage(`true`)), serialized)
}

func TestBoolDeserialization(t *testing.T) {
	t.Parallel()
	deserializer := ballerina.BoolDeserializer()
	serialized := json.RawMessage(`false`)
	deserialized := deserializer(serialized)
	require.Equal(t, ballerina.Right[error, bool](false), deserialized)
}

func TestInt64Serialization(t *testing.T) {
	t.Parallel()
	serializer := ballerina.IntSerializer()
	serialized := serializer(int64(123))
	require.Equal(t, ballerina.Right[error, json.RawMessage](json.RawMessage(`{"kind":"int","value":"123"}`)), serialized)
}

func TestInt64Deserialization(t *testing.T) {
	t.Parallel()
	deserializer := ballerina.IntDeserializer()
	serialized := json.RawMessage(`{"kind":"int","value":"123"}`)
	deserialized := deserializer(serialized)
	require.Equal(t, ballerina.Right[error, int64](123), deserialized)
}

func TestFloat64Serialization(t *testing.T) {
	t.Parallel()
	serializer := ballerina.FloatSerializer()
	serialized := serializer(float64(1.75))
	require.Equal(t, ballerina.Right[error, json.RawMessage](json.RawMessage(`{"kind":"float","value":"1.75"}`)), serialized)
}

func TestFloat64Deserialization(t *testing.T) {
	t.Parallel()
	deserializer := ballerina.FloatDeserializer()
	serialized := json.RawMessage(`{"kind":"float","value":"1.75"}`)
	deserialized := deserializer(serialized)
	require.Equal(t, ballerina.Right[error, float64](1.75), deserialized)
}
