package ballerina_test

import (
	"encoding/json"
	"testing"
	"time"

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

func TestOptionSerialization(t *testing.T) {
	t.Parallel()

	serializer := ballerina.OptionSerializer(ballerina.UnitSerializer())
	some := ballerina.Some(ballerina.Unit{})
	serialized := serializer(some)
	require.Equal(t, ballerina.Right[error, json.RawMessage](json.RawMessage(`{"case":"some","value":{"kind":"unit"}}`)), serialized)

	none := ballerina.None[ballerina.Unit]()
	serializedNone := serializer(none)
	require.Equal(t, ballerina.Right[error, json.RawMessage](json.RawMessage(`{"case":"none","value":{"kind":"unit"}}`)), serializedNone)
}

func TestOptionDeserialization(t *testing.T) {
	t.Parallel()
	deserializer := ballerina.OptionDeserializer(ballerina.UnitDeserializer())

	serializedSome := json.RawMessage(`{"case":"some","value":{"kind":"unit"}}`)
	deserializedSome := deserializer(serializedSome)
	require.Equal(t, ballerina.Right[error, ballerina.Option[ballerina.Unit]](ballerina.Some(ballerina.Unit{})), deserializedSome)

	serializedNone := json.RawMessage(`{"case":"none","value":{"kind":"unit"}}`)
	deserializedNone := deserializer(serializedNone)
	require.Equal(t, ballerina.Right[error, ballerina.Option[ballerina.Unit]](ballerina.None[ballerina.Unit]()), deserializedNone)
}

func TestTuple2Serialization(t *testing.T) {
	t.Parallel()
	serializer := ballerina.Tuple2Serializer(ballerina.UnitSerializer(), ballerina.UnitSerializer())
	serialized := serializer(ballerina.Tuple2[ballerina.Unit, ballerina.Unit]{Item1: ballerina.Unit{}, Item2: ballerina.Unit{}})
	require.Equal(t, ballerina.Right[error, json.RawMessage](json.RawMessage(`{"kind":"tuple","elements":[{"kind":"unit"},{"kind":"unit"}]}`)), serialized)
}

func TestTuple2Deserialization(t *testing.T) {
	t.Parallel()
	deserializer := ballerina.Tuple2Deserializer(ballerina.UnitDeserializer(), ballerina.UnitDeserializer())
	serialized := json.RawMessage(`{"kind":"tuple","elements":[{"kind":"unit"},{"kind":"unit"}]}`)
	deserialized := deserializer(serialized)
	require.Equal(t, ballerina.Right[error, ballerina.Tuple2[ballerina.Unit, ballerina.Unit]](ballerina.Tuple2[ballerina.Unit, ballerina.Unit]{Item1: ballerina.Unit{}, Item2: ballerina.Unit{}}), deserialized)
}

func TestListSerialization(t *testing.T) {
	t.Parallel()
	serializer := ballerina.ListSerializer(ballerina.BoolSerializer())
	serialized := serializer([]bool{true, false, true})
	require.Equal(t, ballerina.Right[error, json.RawMessage](json.RawMessage(`{"kind":"list","elements":[true,false,true]}`)), serialized)
}

func TestListDeserialization(t *testing.T) {
	t.Parallel()
	deserializer := ballerina.ListDeserializer(ballerina.BoolDeserializer())
	serialized := json.RawMessage(`{"kind":"list","elements":[true,false,true]}`)
	deserialized := deserializer(serialized)
	require.Equal(t, ballerina.Right[error, []bool]([]bool{true, false, true}), deserialized)
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

func TestIntSerialization(t *testing.T) {
	t.Parallel()
	serializer := ballerina.IntSerializer()
	serialized := serializer(int64(123))
	require.Equal(t, ballerina.Right[error, json.RawMessage](json.RawMessage(`{"kind":"int","value":"123"}`)), serialized)
}

func TestIntDeserialization(t *testing.T) {
	t.Parallel()
	deserializer := ballerina.IntDeserializer()
	serialized := json.RawMessage(`{"kind":"int","value":"123"}`)
	deserialized := deserializer(serialized)
	require.Equal(t, ballerina.Right[error, int64](123), deserialized)
}

func TestFloatSerialization(t *testing.T) {
	t.Parallel()
	serializer := ballerina.FloatSerializer()
	serialized := serializer(float64(1.75))
	require.Equal(t, ballerina.Right[error, json.RawMessage](json.RawMessage(`{"kind":"float","value":"1.75"}`)), serialized)
}

func TestFloatDeserialization(t *testing.T) {
	t.Parallel()
	deserializer := ballerina.FloatDeserializer()
	serialized := json.RawMessage(`{"kind":"float","value":"1.75"}`)
	deserialized := deserializer(serialized)
	require.Equal(t, ballerina.Right[error, float64](1.75), deserialized)
}

func TestDateSerialization(t *testing.T) {
	t.Parallel()
	serializer := ballerina.DateSerializer()
	date := time.Date(2025, 1, 1, 0, 0, 0, 0, time.UTC)
	serialized := serializer(date)
	require.Equal(t, ballerina.Right[error, json.RawMessage](json.RawMessage(`{"kind":"date","value":"2025-01-01"}`)), serialized)
}

func TestDateDeserialization(t *testing.T) {
	t.Parallel()
	deserializer := ballerina.DateDeserializer()
	serialized := json.RawMessage(`{"kind":"date","value":"2025-01-01"}`)
	deserialized := deserializer(serialized)
	require.Equal(t, ballerina.Right[error, time.Time](time.Date(2025, 1, 1, 0, 0, 0, 0, time.UTC)), deserialized)
}
