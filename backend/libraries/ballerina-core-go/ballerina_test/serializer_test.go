package ballerina_test

import (
	"encoding/json"
	"testing"
	"time"

	ballerina "ballerina.com/core"
	"github.com/stretchr/testify/assert"
	"github.com/stretchr/testify/require"
)

func assertErrorContains[T any](t *testing.T, expected string, actual ballerina.Sum[error, T]) {
	ballerina.Fold(actual, func(err error) ballerina.Unit {
		assert.Contains(t, err.Error(), expected)
		return ballerina.NewUnit()
	}, func(value T) ballerina.Unit {
		t.Errorf("expected error to contain '%s', but got value %v", expected, value)
		return ballerina.NewUnit()
	})
}

func TestUnitSerialization(t *testing.T) {
	t.Parallel()
	serializer := ballerina.UnitSerializer
	unit := ballerina.Unit{}
	serialized := serializer(unit)
	require.Equal(t, ballerina.Right[error, json.RawMessage](json.RawMessage(`{"kind":"unit"}`)), serialized)
}

func TestUnitDeserialization(t *testing.T) {
	t.Parallel()
	deserializer := ballerina.UnitDeserializer
	serialized := json.RawMessage(`{"kind":"unit"}`)
	deserialized := deserializer(serialized)
	require.Equal(t, ballerina.Right[error, ballerina.Unit](ballerina.Unit{}), deserialized)
}

func TestUnitDeserializationError(t *testing.T) {
	t.Parallel()
	deserializer := ballerina.UnitDeserializer
	testCases := []struct {
		name          string
		serialized    json.RawMessage
		expectedError string
	}{
		{name: "not-unit-kind", serialized: json.RawMessage(`{"kind":"not-unit"}`), expectedError: "on unit: expected kind to be 'unit', got 'not-unit'"},
		{name: "empty", serialized: json.RawMessage(`{}`), expectedError: "on unit"},
		{name: "other-key", serialized: json.RawMessage(`{"other-key":"something"}`), expectedError: "on unit"},
	}

	for _, testCase := range testCases {
		t.Run(testCase.name, func(t *testing.T) {
			deserialized := deserializer(testCase.serialized)
			assertErrorContains(t, testCase.expectedError, deserialized)
		})
	}
}

func TestSumSerialization(t *testing.T) {
	t.Parallel()
	serializer := ballerina.SumSerializer(ballerina.UnitSerializer, ballerina.UnitSerializer)
	testCases := []struct {
		name     string
		sum      ballerina.Sum[ballerina.Unit, ballerina.Unit]
		expected json.RawMessage
	}{
		{name: "left", sum: ballerina.Left[ballerina.Unit, ballerina.Unit](ballerina.Unit{}), expected: json.RawMessage(`{"case":"Sum.Left","value":{"kind":"unit"}}`)},
		{name: "right", sum: ballerina.Right[ballerina.Unit, ballerina.Unit](ballerina.Unit{}), expected: json.RawMessage(`{"case":"Sum.Right","value":{"kind":"unit"}}`)},
	}

	for _, testCase := range testCases {
		t.Run(testCase.name, func(t *testing.T) {
			serialized := serializer(testCase.sum)
			require.Equal(t, ballerina.Right[error, json.RawMessage](testCase.expected), serialized)
		})
	}
}

func TestSumDeserialization(t *testing.T) {
	t.Parallel()
	deserializer := ballerina.SumDeserializer(ballerina.UnitDeserializer, ballerina.UnitDeserializer)
	testCases := []struct {
		name       string
		serialized json.RawMessage
		expected   ballerina.Sum[ballerina.Unit, ballerina.Unit]
	}{
		{name: "left", serialized: json.RawMessage(`{"case":"Sum.Left","value":{"kind":"unit"}}`), expected: ballerina.Left[ballerina.Unit, ballerina.Unit](ballerina.Unit{})},
		{name: "right", serialized: json.RawMessage(`{"case":"Sum.Right","value":{"kind":"unit"}}`), expected: ballerina.Right[ballerina.Unit, ballerina.Unit](ballerina.Unit{})},
	}

	for _, testCase := range testCases {
		t.Run(testCase.name, func(t *testing.T) {
			deserialized := deserializer(testCase.serialized)
			require.Equal(t, ballerina.Right[error, ballerina.Sum[ballerina.Unit, ballerina.Unit]](testCase.expected), deserialized)
		})
	}
}

func TestSumDeserializationError(t *testing.T) {
	t.Parallel()
	deserializer := ballerina.SumDeserializer(ballerina.UnitDeserializer, ballerina.UnitDeserializer)
	testCases := []struct {
		name          string
		serialized    json.RawMessage
		expectedError string
	}{
		{name: "on-sum-on-left", serialized: json.RawMessage(`{"case":"Sum.Left","value":{"kind":"not-unit"}}`), expectedError: "on sum: on left:"},
		{name: "on-sum-on-right", serialized: json.RawMessage(`{"case":"Sum.Right","value":{"kind":"not-unit"}}`), expectedError: "on sum: on right:"},
		{name: "not-sum-case", serialized: json.RawMessage(`{"case":"not-sum","value":{"kind":"unit"}}`), expectedError: "on sum: expected case to be 'Sum.Left' or 'Sum.Right', got not-sum"},
		{name: "empty", serialized: json.RawMessage(`{}`), expectedError: "on sum"},
		{name: "other-key", serialized: json.RawMessage(`{"other-key":"something"}`), expectedError: "on sum"},
	}

	for _, testCase := range testCases {
		t.Run(testCase.name, func(t *testing.T) {
			deserialized := deserializer(testCase.serialized)
			assertErrorContains(t, testCase.expectedError, deserialized)
		})
	}
}
func TestOptionSerialization(t *testing.T) {
	t.Parallel()

	serializer := ballerina.OptionSerializer(ballerina.UnitSerializer)
	testCases := []struct {
		name     string
		option   ballerina.Option[ballerina.Unit]
		expected json.RawMessage
	}{
		{name: "some", option: ballerina.Some(ballerina.Unit{}), expected: json.RawMessage(`{"case":"some","value":{"kind":"unit"}}`)},
		{name: "none", option: ballerina.None[ballerina.Unit](), expected: json.RawMessage(`{"case":"none","value":{"kind":"unit"}}`)},
	}

	for _, testCase := range testCases {
		t.Run(testCase.name, func(t *testing.T) {
			serialized := serializer(testCase.option)
			require.Equal(t, ballerina.Right[error, json.RawMessage](testCase.expected), serialized)
		})
	}
}

func TestOptionDeserialization(t *testing.T) {
	t.Parallel()
	deserializer := ballerina.OptionDeserializer(ballerina.UnitDeserializer)
	testCases := []struct {
		name       string
		serialized json.RawMessage
		expected   ballerina.Option[ballerina.Unit]
	}{
		{name: "some", serialized: json.RawMessage(`{"case":"some","value":{"kind":"unit"}}`), expected: ballerina.Some(ballerina.Unit{})},
		{name: "none", serialized: json.RawMessage(`{"case":"none","value":{"kind":"unit"}}`), expected: ballerina.None[ballerina.Unit]()},
	}

	for _, testCase := range testCases {
		t.Run(testCase.name, func(t *testing.T) {
			deserialized := deserializer(testCase.serialized)
			require.Equal(t, ballerina.Right[error, ballerina.Option[ballerina.Unit]](testCase.expected), deserialized)
		})
	}
}

func TestOptionDeserializationError(t *testing.T) {
	t.Parallel()
	deserializer := ballerina.OptionDeserializer(ballerina.UnitDeserializer)
	testCases := []struct {
		name          string
		serialized    json.RawMessage
		expectedError string
	}{
		{name: "on-some", serialized: json.RawMessage(`{"case":"some","value":{"kind":"not-unit"}}`), expectedError: "on option: on some:"},
		{name: "on-none", serialized: json.RawMessage(`{"case":"none","value":{"kind":"not-unit"}}`), expectedError: "on option: on none:"},
		{name: "not-option-case", serialized: json.RawMessage(`{"case":"not-option","value":{"kind":"unit"}}`), expectedError: "on option: expected case to be 'none' or 'some', got not-option"},
		{name: "empty", serialized: json.RawMessage(`{}`), expectedError: "on option"},
		{name: "other-key", serialized: json.RawMessage(`{"other-key":"something"}`), expectedError: "on option"},
	}

	for _, testCase := range testCases {
		t.Run(testCase.name, func(t *testing.T) {
			deserialized := deserializer(testCase.serialized)
			assertErrorContains(t, testCase.expectedError, deserialized)
		})
	}
}

func TestTuple2Serialization(t *testing.T) {
	t.Parallel()
	serializer := ballerina.Tuple2Serializer(ballerina.UnitSerializer, ballerina.UnitSerializer)
	serialized := serializer(ballerina.Tuple2[ballerina.Unit, ballerina.Unit]{Item1: ballerina.Unit{}, Item2: ballerina.Unit{}})
	require.Equal(t, ballerina.Right[error, json.RawMessage](json.RawMessage(`{"kind":"tuple","elements":[{"kind":"unit"},{"kind":"unit"}]}`)), serialized)
}

func TestTuple2Deserialization(t *testing.T) {
	t.Parallel()
	deserializer := ballerina.Tuple2Deserializer(ballerina.UnitDeserializer, ballerina.UnitDeserializer)
	serialized := json.RawMessage(`{"kind":"tuple","elements":[{"kind":"unit"},{"kind":"unit"}]}`)
	deserialized := deserializer(serialized)
	require.Equal(t, ballerina.Right[error, ballerina.Tuple2[ballerina.Unit, ballerina.Unit]](ballerina.Tuple2[ballerina.Unit, ballerina.Unit]{Item1: ballerina.Unit{}, Item2: ballerina.Unit{}}), deserialized)
}

func TestTuple2DeserializationError(t *testing.T) {
	t.Parallel()
	deserializer := ballerina.Tuple2Deserializer(ballerina.UnitDeserializer, ballerina.UnitDeserializer)
	testCases := []struct {
		name          string
		serialized    json.RawMessage
		expectedError string
	}{
		{name: "on-item1", serialized: json.RawMessage(`{"kind":"tuple","elements":[{"kind":"not-unit"},{"kind":"unit"}]}`), expectedError: "on tuple2: on item1:"},
		{name: "on-item2", serialized: json.RawMessage(`{"kind":"tuple","elements":[{"kind":"unit"},{"kind":"not-unit"}]}`), expectedError: "on tuple2: on item2:"},
		{name: "different-length", serialized: json.RawMessage(`{"kind":"tuple","elements":[{"kind":"unit"},{"kind":"unit"},{"kind":"unit"}]}`), expectedError: "on tuple2"},
		{name: "on-element", serialized: json.RawMessage(`{"kind":"tuple","elements":[{"kind":"not-unit"},{"kind":"unit"}]}`), expectedError: "on tuple2"},
		{name: "empty", serialized: json.RawMessage(`{}`), expectedError: "on tuple2"},
		{name: "other-key", serialized: json.RawMessage(`{"other-key":"something"}`), expectedError: "on tuple2"},
		{name: "non-tuple-kind", serialized: json.RawMessage(`{"kind":"list","elements":[{"kind":"unit"},{"kind":"unit"}]}`), expectedError: "on tuple2"},
	}

	for _, testCase := range testCases {
		t.Run(testCase.name, func(t *testing.T) {
			deserialized := deserializer(testCase.serialized)
			assertErrorContains(t, testCase.expectedError, deserialized)
		})
	}
}

func TestListSerialization(t *testing.T) {
	t.Parallel()
	serializer := ballerina.ListSerializer(ballerina.BoolSerializer)
	serialized := serializer([]bool{true, false, true})
	require.Equal(t, ballerina.Right[error, json.RawMessage](json.RawMessage(`{"kind":"list","elements":[true,false,true]}`)), serialized)
}

func TestListDeserialization(t *testing.T) {
	t.Parallel()
	deserializer := ballerina.ListDeserializer(ballerina.BoolDeserializer)
	serialized := json.RawMessage(`{"kind":"list","elements":[true,false,true]}`)
	deserialized := deserializer(serialized)
	require.Equal(t, ballerina.Right[error, []bool]([]bool{true, false, true}), deserialized)
}

func TestListDeserializationError(t *testing.T) {
	t.Parallel()
	deserializer := ballerina.ListDeserializer(ballerina.BoolDeserializer)
	testCases := []struct {
		name          string
		serialized    json.RawMessage
		expectedError string
	}{
		{name: "on-element", serialized: json.RawMessage(`{"kind":"list","elements":["not-bool"]}`), expectedError: "on list"},
		{name: "not-list-elements", serialized: json.RawMessage(`{"kind":"list","not-elements":[true,false,true]}`), expectedError: "on list"},
		{name: "other-key", serialized: json.RawMessage(`{"other-key":"something"}`), expectedError: "on list"},
		{name: "non-list-kind", serialized: json.RawMessage(`{"kind":"list","elements":[{"kind":"unit"},{"kind":"unit"}]}`), expectedError: "on list"},
	}

	for _, testCase := range testCases {
		t.Run(testCase.name, func(t *testing.T) {
			deserialized := deserializer(testCase.serialized)
			assertErrorContains(t, testCase.expectedError, deserialized)
		})
	}
}

func TestStringSerialization(t *testing.T) {
	t.Parallel()
	serializer := ballerina.StringSerializer
	string := `he\nllo`
	serialized := serializer(string)
	require.Equal(t, ballerina.Right[error, json.RawMessage](json.RawMessage(`"he\\nllo"`)), serialized)
}

func TestStringDeserialization(t *testing.T) {
	t.Parallel()
	deserializer := ballerina.StringDeserializer
	serialized := json.RawMessage(`"he\\nllo"`)
	deserialized := deserializer(serialized)
	require.Equal(t, ballerina.Right[error, string](`he\nllo`), deserialized)
}

func TestBoolSerialization(t *testing.T) {
	t.Parallel()
	serializer := ballerina.BoolSerializer
	bool := true
	serialized := serializer(bool)
	require.Equal(t, ballerina.Right[error, json.RawMessage](json.RawMessage(`true`)), serialized)
}

func TestBoolDeserialization(t *testing.T) {
	t.Parallel()
	deserializer := ballerina.BoolDeserializer
	serialized := json.RawMessage(`false`)
	deserialized := deserializer(serialized)
	require.Equal(t, ballerina.Right[error, bool](false), deserialized)
}

func TestIntSerialization(t *testing.T) {
	t.Parallel()
	serializer := ballerina.IntSerializer
	serialized := serializer(int64(123))
	require.Equal(t, ballerina.Right[error, json.RawMessage](json.RawMessage(`{"kind":"int","value":"123"}`)), serialized)
}

func TestIntDeserialization(t *testing.T) {
	t.Parallel()
	deserializer := ballerina.IntDeserializer
	serialized := json.RawMessage(`{"kind":"int","value":"123"}`)
	deserialized := deserializer(serialized)
	require.Equal(t, ballerina.Right[error, int64](123), deserialized)
}

func TestFloatSerialization(t *testing.T) {
	t.Parallel()
	serializer := ballerina.FloatSerializer
	serialized := serializer(float64(1.75))
	require.Equal(t, ballerina.Right[error, json.RawMessage](json.RawMessage(`{"kind":"float","value":"1.75"}`)), serialized)
}

func TestFloatDeserialization(t *testing.T) {
	t.Parallel()
	deserializer := ballerina.FloatDeserializer
	serialized := json.RawMessage(`{"kind":"float","value":"1.75"}`)
	deserialized := deserializer(serialized)
	require.Equal(t, ballerina.Right[error, float64](1.75), deserialized)
}

func TestDateSerialization(t *testing.T) {
	t.Parallel()
	serializer := ballerina.DateSerializer
	date := time.Date(2025, 1, 1, 0, 0, 0, 0, time.UTC)
	serialized := serializer(date)
	require.Equal(t, ballerina.Right[error, json.RawMessage](json.RawMessage(`{"kind":"date","value":"2025-01-01"}`)), serialized)
}

func TestDateDeserialization(t *testing.T) {
	t.Parallel()
	deserializer := ballerina.DateDeserializer
	serialized := json.RawMessage(`{"kind":"date","value":"2025-01-01"}`)
	deserialized := deserializer(serialized)
	require.Equal(t, ballerina.Right[error, time.Time](time.Date(2025, 1, 1, 0, 0, 0, 0, time.UTC)), deserialized)
}
