package ballerina_test

import (
	"encoding/json"
	"testing"
	"time"

	ballerina "ballerina.com/core"
	"ballerina.com/core/ballerinaserialization"
	"github.com/shopspring/decimal"
	"github.com/stretchr/testify/assert"
	"github.com/stretchr/testify/require"
)

func assertErrorContains[T any](t *testing.T, expected string, actual ballerina.Sum[error, T]) {
	t.Helper()
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
	serializer := ballerinaserialization.UnitSerializer
	unit := ballerina.Unit{}
	serialized := serializer(unit)
	require.Equal(t, ballerina.Right[error, json.RawMessage](json.RawMessage(`{"discriminator":"unit"}`)), serialized)
}

func TestUnitDeserialization(t *testing.T) {
	t.Parallel()
	deserializer := ballerinaserialization.UnitDeserializer
	serialized := json.RawMessage(`{"discriminator":"unit"}`)
	deserialized := deserializer(serialized)
	require.Equal(t, ballerina.Right[error, ballerina.Unit](ballerina.Unit{}), deserialized)
}

func TestUnitDeserializationError(t *testing.T) {
	t.Parallel()
	deserializer := ballerinaserialization.UnitDeserializer
	testCases := []struct {
		name          string
		serialized    json.RawMessage
		expectedError string
	}{
		{name: "not-unit-kind", serialized: json.RawMessage(`{"discriminator":"not-unit"}`), expectedError: "on unit: expected discriminator to be 'unit', got 'not-unit'"},
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
	serializer := ballerinaserialization.SumSerializer(ballerinaserialization.UnitSerializer, ballerinaserialization.UnitSerializer)
	testCases := []struct {
		name     string
		sum      ballerina.Sum[ballerina.Unit, ballerina.Unit]
		expected json.RawMessage
	}{
		{name: "left", sum: ballerina.Left[ballerina.Unit, ballerina.Unit](ballerina.Unit{}), expected: json.RawMessage(`{"discriminator":"sum","value":[1,{"discriminator":"unit"}]}`)},
		{name: "right", sum: ballerina.Right[ballerina.Unit, ballerina.Unit](ballerina.Unit{}), expected: json.RawMessage(`{"discriminator":"sum","value":[2,{"discriminator":"unit"}]}`)},
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
	deserializer := ballerinaserialization.SumDeserializer(ballerinaserialization.UnitDeserializer, ballerinaserialization.UnitDeserializer)
	testCases := []struct {
		name       string
		serialized json.RawMessage
		expected   ballerina.Sum[ballerina.Unit, ballerina.Unit]
	}{
		{name: "left", serialized: json.RawMessage(`{"discriminator":"sum","value":[1,{"discriminator":"unit"}]}`), expected: ballerina.Left[ballerina.Unit, ballerina.Unit](ballerina.Unit{})},
		{name: "right", serialized: json.RawMessage(`{"discriminator":"sum","value":[2,{"discriminator":"unit"}]}`), expected: ballerina.Right[ballerina.Unit, ballerina.Unit](ballerina.Unit{})},
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
	deserializer := ballerinaserialization.SumDeserializer(ballerinaserialization.UnitDeserializer, ballerinaserialization.UnitDeserializer)
	testCases := []struct {
		name          string
		serialized    json.RawMessage
		expectedError string
	}{
		{name: "on-sum-on-left", serialized: json.RawMessage(`{"discriminator":"sum","value":[1,{"discriminator":"not-unit"}]}`), expectedError: "on sum: on left: on unit: expected discriminator to be 'unit', got 'not-unit'"},
		{name: "on-sum-on-right", serialized: json.RawMessage(`{"discriminator":"sum","value":[2,{"discriminator":"not-unit"}]}`), expectedError: "on sum: on right: on unit: expected discriminator to be 'unit', got 'not-unit'"},
		{name: "not-sum-case", serialized: json.RawMessage(`{"discriminator":"not-sum","value":[1,2,{"discriminator":"unit"}]}`), expectedError: "on sum: expected discriminator to be 'sum', got 'not-sum'"},
		{name: "no-value-field", serialized: json.RawMessage(`{"discriminator":"sum"}`), expectedError: "on sum"},
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

	serializer := ballerinaserialization.OptionSerializer(ballerinaserialization.UnitSerializer)
	testCases := []struct {
		name     string
		option   ballerina.Option[ballerina.Unit]
		expected json.RawMessage
	}{
		{name: "none", option: ballerina.None[ballerina.Unit](), expected: json.RawMessage(`{"discriminator":"sum","value":[1,{"discriminator":"unit"}]}`)},
		{name: "some", option: ballerina.Some(ballerina.Unit{}), expected: json.RawMessage(`{"discriminator":"sum","value":[2,{"discriminator":"unit"}]}`)},
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
	deserializer := ballerinaserialization.OptionDeserializer(ballerinaserialization.UnitDeserializer)
	testCases := []struct {
		name       string
		serialized json.RawMessage
		expected   ballerina.Option[ballerina.Unit]
	}{
		{name: "none", serialized: json.RawMessage(`{"discriminator":"sum","value":[1,{"discriminator":"unit"}]}`), expected: ballerina.None[ballerina.Unit]()},
		{name: "some", serialized: json.RawMessage(`{"discriminator":"sum","value":[2,{"discriminator":"unit"}]}`), expected: ballerina.Some(ballerina.Unit{})},
	}

	for _, testCase := range testCases {
		t.Run(testCase.name, func(t *testing.T) {
			deserialized := deserializer(testCase.serialized)
			require.Equal(t, ballerina.Right[error, ballerina.Option[ballerina.Unit]](testCase.expected), deserialized)
		})
	}
}

func TestTuple2Serialization(t *testing.T) {
	t.Parallel()
	serializer := ballerinaserialization.Tuple2Serializer(ballerinaserialization.UnitSerializer, ballerinaserialization.UnitSerializer)
	serialized := serializer(ballerina.Tuple2[ballerina.Unit, ballerina.Unit]{Item1: ballerina.Unit{}, Item2: ballerina.Unit{}})
	require.Equal(t, ballerina.Right[error, json.RawMessage](json.RawMessage(`{"discriminator":"tuple","value":[{"discriminator":"unit"},{"discriminator":"unit"}]}`)), serialized)
}

func TestTuple2Deserialization(t *testing.T) {
	t.Parallel()
	deserializer := ballerinaserialization.Tuple2Deserializer(ballerinaserialization.UnitDeserializer, ballerinaserialization.UnitDeserializer)
	serialized := json.RawMessage(`{"discriminator":"tuple","value":[{"discriminator":"unit"},{"discriminator":"unit"}]}`)
	deserialized := deserializer(serialized)
	require.Equal(t, ballerina.Right[error, ballerina.Tuple2[ballerina.Unit, ballerina.Unit]](ballerina.Tuple2[ballerina.Unit, ballerina.Unit]{Item1: ballerina.Unit{}, Item2: ballerina.Unit{}}), deserialized)
}

func TestTuple2DeserializationError(t *testing.T) {
	t.Parallel()
	deserializer := ballerinaserialization.Tuple2Deserializer(ballerinaserialization.UnitDeserializer, ballerinaserialization.UnitDeserializer)
	testCases := []struct {
		name          string
		serialized    json.RawMessage
		expectedError string
	}{
		{name: "on-item1", serialized: json.RawMessage(`{"discriminator":"tuple","value":[{"discriminator":"not-unit"},{"discriminator":"unit"}]}`), expectedError: "on tuple2: on item1:"},
		{name: "on-item2", serialized: json.RawMessage(`{"discriminator":"tuple","value":[{"discriminator":"unit"},{"discriminator":"not-unit"}]}`), expectedError: "on tuple2: on item2:"},
		{name: "different-length", serialized: json.RawMessage(`{"discriminator":"tuple","value":[{"discriminator":"unit"},{"discriminator":"unit"},{"discriminator":"unit"}]}`), expectedError: "on tuple2"},
		{name: "on-element", serialized: json.RawMessage(`{"discriminator":"tuple","value":[{"discriminator":"not-unit"},{"discriminator":"unit"}]}`), expectedError: "on tuple2"},
		{name: "empty", serialized: json.RawMessage(`{}`), expectedError: "on tuple2"},
		{name: "other-key", serialized: json.RawMessage(`{"other-key":"something"}`), expectedError: "on tuple2"},
		{name: "non-tuple-discriminator", serialized: json.RawMessage(`{"discriminator":"list","value":[{"discriminator":"unit"},{"discriminator":"unit"}]}`), expectedError: "on tuple2"},
		{name: "no-value-field", serialized: json.RawMessage(`{"discriminator":"tuple"}`), expectedError: "on tuple2: missing value field"},
	}

	for _, testCase := range testCases {
		t.Run(testCase.name, func(t *testing.T) {
			deserialized := deserializer(testCase.serialized)
			assertErrorContains(t, testCase.expectedError, deserialized)
		})
	}
}

func TestTuple3Serialization(t *testing.T) {
	t.Parallel()

	tuple := ballerina.Tuple3[string, bool, int]{
		Item1: "hello",
		Item2: true,
		Item3: int(42),
	}

	serializer := ballerinaserialization.Tuple3Serializer(
		ballerinaserialization.StringSerializer,
		ballerinaserialization.BoolSerializer,
		ballerinaserialization.IntSerializer,
	)
	deserializer := ballerinaserialization.Tuple3Deserializer(
		ballerinaserialization.StringDeserializer,
		ballerinaserialization.BoolDeserializer,
		ballerinaserialization.IntDeserializer,
	)

	mappedTupleWithError := ballerina.Bind(serializer(tuple), deserializer)

	require.Equal(t, ballerina.Right[error, ballerina.Tuple3[string, bool, int]](tuple), mappedTupleWithError)
}

func TestTuple4Serialization(t *testing.T) {
	t.Parallel()

	tuple := ballerina.Tuple4[string, bool, int, float64]{
		Item1: "hello",
		Item2: true,
		Item3: int(42),
		Item4: float64(3.14),
	}

	serializer := ballerinaserialization.Tuple4Serializer(
		ballerinaserialization.StringSerializer,
		ballerinaserialization.BoolSerializer,
		ballerinaserialization.IntSerializer,
		ballerinaserialization.FloatSerializer,
	)
	deserializer := ballerinaserialization.Tuple4Deserializer(
		ballerinaserialization.StringDeserializer,
		ballerinaserialization.BoolDeserializer,
		ballerinaserialization.IntDeserializer,
		ballerinaserialization.FloatDeserializer,
	)

	mappedTupleWithError := ballerina.Bind(serializer(tuple), deserializer)

	require.Equal(t, ballerina.Right[error, ballerina.Tuple4[string, bool, int, float64]](tuple), mappedTupleWithError)
}

func TestSum3SerializationAndDeserialization(t *testing.T) {
	t.Parallel()

	serializer := ballerinaserialization.Sum3Serializer(
		ballerinaserialization.StringSerializer,
		ballerinaserialization.BoolSerializer,
		ballerinaserialization.IntSerializer,
	)
	deserializer := ballerinaserialization.Sum3Deserializer(
		ballerinaserialization.StringDeserializer,
		ballerinaserialization.BoolDeserializer,
		ballerinaserialization.IntDeserializer,
	)

	t.Run("case 1/3", func(t *testing.T) {
		sum := ballerina.Case1Of3[string, bool, int]("abc")
		mapped := ballerina.Bind(serializer(sum), deserializer)
		require.Equal(t, ballerina.Right[error, ballerina.Sum3[string, bool, int]](sum), mapped)
	})

	t.Run("case 2/3", func(t *testing.T) {
		sum := ballerina.Case2Of3[string, bool, int](true)
		mapped := ballerina.Bind(serializer(sum), deserializer)
		require.Equal(t, ballerina.Right[error, ballerina.Sum3[string, bool, int]](sum), mapped)
	})

	t.Run("case 3/3", func(t *testing.T) {
		sum := ballerina.Case3Of3[string, bool, int](int(42))
		mapped := ballerina.Bind(serializer(sum), deserializer)
		require.Equal(t, ballerina.Right[error, ballerina.Sum3[string, bool, int]](sum), mapped)
	})
}

func TestSum4SerializationAndDeserialization(t *testing.T) {
	t.Parallel()

	serializer := ballerinaserialization.Sum4Serializer(
		ballerinaserialization.StringSerializer,
		ballerinaserialization.BoolSerializer,
		ballerinaserialization.IntSerializer,
		ballerinaserialization.FloatSerializer,
	)
	deserializer := ballerinaserialization.Sum4Deserializer(
		ballerinaserialization.StringDeserializer,
		ballerinaserialization.BoolDeserializer,
		ballerinaserialization.IntDeserializer,
		ballerinaserialization.FloatDeserializer,
	)

	t.Run("case 1/4", func(t *testing.T) {
		sum := ballerina.Case1Of4[string, bool, int, float64]("abc")
		mapped := ballerina.Bind(serializer(sum), deserializer)
		require.Equal(t, ballerina.Right[error, ballerina.Sum4[string, bool, int, float64]](sum), mapped)
	})

	t.Run("case 2/4", func(t *testing.T) {
		sum := ballerina.Case2Of4[string, bool, int, float64](true)
		mapped := ballerina.Bind(serializer(sum), deserializer)
		require.Equal(t, ballerina.Right[error, ballerina.Sum4[string, bool, int, float64]](sum), mapped)
	})

	t.Run("case 3/4", func(t *testing.T) {
		sum := ballerina.Case3Of4[string, bool, int, float64](int(42))
		mapped := ballerina.Bind(serializer(sum), deserializer)
		require.Equal(t, ballerina.Right[error, ballerina.Sum4[string, bool, int, float64]](sum), mapped)
	})

	t.Run("case 4/4", func(t *testing.T) {
		sum := ballerina.Case4Of4[string, bool, int, float64](3.14)
		mapped := ballerina.Bind(serializer(sum), deserializer)
		require.Equal(t, ballerina.Right[error, ballerina.Sum4[string, bool, int, float64]](sum), mapped)
	})
}

func TestListSerialization(t *testing.T) {
	t.Parallel()
	serializer := ballerinaserialization.ListSerializer(ballerinaserialization.BoolSerializer)
	serialized := serializer([]bool{true, false, true})
	require.Equal(t, ballerina.Right[error, json.RawMessage](json.RawMessage(`{"discriminator":"list","value":[{"discriminator":"bool","value":"true"},{"discriminator":"bool","value":"false"},{"discriminator":"bool","value":"true"}]}`)), serialized)
}

func TestListDeserialization(t *testing.T) {
	t.Parallel()
	deserializer := ballerinaserialization.ListDeserializer(ballerinaserialization.BoolDeserializer)
	serialized := json.RawMessage(`
{
  "discriminator": "list",
  "value": [
    {"discriminator": "bool", "value": "true"},
    {"discriminator": "bool", "value": "false"},
    {"discriminator": "bool", "value": "true"}
  ]
}`)
	deserialized := deserializer(serialized)
	require.Equal(t, ballerina.Right[error, ballerina.Array[bool]]([]bool{true, false, true}), deserialized)
}

func TestListDeserializationError(t *testing.T) {
	t.Parallel()
	deserializer := ballerinaserialization.ListDeserializer(ballerinaserialization.BoolDeserializer)
	testCases := []struct {
		name          string
		serialized    json.RawMessage
		expectedError string
	}{
		{name: "on-element", serialized: json.RawMessage(`{"discriminator":"list","elements":["not-bool"]}`), expectedError: "on list"},
		{name: "not-list-value", serialized: json.RawMessage(`{"discriminator":"list","not-elements":[true,false,true]}`), expectedError: "on list"},
		{name: "other-key", serialized: json.RawMessage(`{"other-key":"something"}`), expectedError: "on list"},
		{name: "non-list-discriminator", serialized: json.RawMessage(`{"discriminator":"list","elements":[{"discriminator":"unit"},{"discriminator":"unit"}]}`), expectedError: "on list"},
		{name: "no-value-field", serialized: json.RawMessage(`{"discriminator":"list"}`), expectedError: "on list: missing value field"},
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
	serializer := ballerinaserialization.StringSerializer
	string := `he\nllo`
	serialized := serializer(string)
	require.Equal(t, ballerina.Right[error, json.RawMessage](json.RawMessage(`{"discriminator":"string","value":"he\\nllo"}`)), serialized)
}

func TestStringDeserialization(t *testing.T) {
	t.Parallel()
	deserializer := ballerinaserialization.StringDeserializer
	serialized := json.RawMessage(`{"discriminator":"string","value":"he\\nllo"}`)
	deserialized := deserializer(serialized)
	require.Equal(t, ballerina.Right[error, string](`he\nllo`), deserialized)
}

func TestBoolSerialization(t *testing.T) {
	t.Parallel()
	serializer := ballerinaserialization.BoolSerializer
	bool := true
	serialized := serializer(bool)
	require.Equal(t, ballerina.Right[error, json.RawMessage](json.RawMessage(`{"discriminator":"bool","value":"true"}`)), serialized)
}

func TestBoolDeserialization(t *testing.T) {
	t.Parallel()
	deserializer := ballerinaserialization.BoolDeserializer
	serialized := json.RawMessage(`{"discriminator":"bool","value":"false"}`)
	deserialized := deserializer(serialized)
	require.Equal(t, ballerina.Right[error, bool](false), deserialized)
}

func TestIntSerialization(t *testing.T) {
	t.Parallel()
	serializer := ballerinaserialization.IntSerializer
	serialized := serializer(int(123))
	require.Equal(t, ballerina.Right[error, json.RawMessage](json.RawMessage(`{"discriminator":"int32","value":"123"}`)), serialized)
}

func TestIntDeserialization(t *testing.T) {
	t.Parallel()
	deserializer := ballerinaserialization.IntDeserializer
	serialized := json.RawMessage(`{"discriminator":"int32","value":"123"}`)
	deserialized := deserializer(serialized)
	require.Equal(t, ballerina.Right[error, int](123), deserialized)
}

func TestFloatSerialization(t *testing.T) {
	t.Parallel()
	serializer := ballerinaserialization.FloatSerializer
	serialized := serializer(float64(1.75))
	require.Equal(t, ballerina.Right[error, json.RawMessage](json.RawMessage(`{"discriminator":"float64","value":"1.75"}`)), serialized)
}

func TestFloatDeserialization(t *testing.T) {
	t.Parallel()
	deserializer := ballerinaserialization.FloatDeserializer
	serialized := json.RawMessage(`{"discriminator":"float64","value":"1.75"}`)
	deserialized := deserializer(serialized)
	require.Equal(t, ballerina.Right[error, float64](1.75), deserialized)
}

func TestDecimalSerialization(t *testing.T) {
	t.Parallel()
	value, err := decimal.NewFromString("123.45")
	require.NoError(t, err)
	serialized := ballerinaserialization.DecimalSerializer(value)
	require.Equal(t, ballerina.Right[error, json.RawMessage](json.RawMessage(`{"discriminator":"decimal","value":"123.45"}`)), serialized)
}

func TestDecimalDeserialization(t *testing.T) {
	t.Parallel()
	serialized := json.RawMessage(`{"discriminator":"decimal","value":"123.45"}`)
	deserialized := ballerinaserialization.DecimalDeserializer(serialized)
	expected, err := decimal.NewFromString("123.45")
	require.NoError(t, err)
	require.Equal(t, ballerina.Right[error, decimal.Decimal](expected), deserialized)
}

func TestDateSerialization(t *testing.T) {
	t.Parallel()
	serializer := ballerinaserialization.DateSerializer
	date := time.Date(2025, 1, 1, 0, 0, 0, 0, time.UTC)
	serialized := serializer(date)
	require.Equal(t, ballerina.Right[error, json.RawMessage](json.RawMessage(`{"discriminator":"date","value":"2025-01-01"}`)), serialized)
}

func TestDateDeserialization(t *testing.T) {
	t.Parallel()
	deserializer := ballerinaserialization.DateDeserializer
	serialized := json.RawMessage(`{"discriminator":"date","value":"2025-01-01"}`)
	deserialized := deserializer(serialized)
	require.Equal(t, ballerina.Right[error, time.Time](time.Date(2025, 1, 1, 0, 0, 0, 0, time.UTC)), deserialized)
}

func TestTuple5Serialization(t *testing.T) {
	t.Parallel()

	tuple := ballerina.Tuple5[string, bool, int, float64, string]{
		Item1: "hello",
		Item2: true,
		Item3: int(42),
		Item4: float64(3.14),
		Item5: "world",
	}

	serializer := ballerinaserialization.Tuple5Serializer(
		ballerinaserialization.StringSerializer,
		ballerinaserialization.BoolSerializer,
		ballerinaserialization.IntSerializer,
		ballerinaserialization.FloatSerializer,
		ballerinaserialization.StringSerializer,
	)
	deserializer := ballerinaserialization.Tuple5Deserializer(
		ballerinaserialization.StringDeserializer,
		ballerinaserialization.BoolDeserializer,
		ballerinaserialization.IntDeserializer,
		ballerinaserialization.FloatDeserializer,
		ballerinaserialization.StringDeserializer,
	)

	mappedTupleWithError := ballerina.Bind(serializer(tuple), deserializer)

	require.Equal(t, ballerina.Right[error, ballerina.Tuple5[string, bool, int, float64, string]](tuple), mappedTupleWithError)
}

func TestTuple6Serialization(t *testing.T) {
	t.Parallel()

	tuple := ballerina.Tuple6[string, bool, int, float64, string, bool]{
		Item1: "hello",
		Item2: true,
		Item3: int(42),
		Item4: float64(3.14),
		Item5: "world",
		Item6: false,
	}

	serializer := ballerinaserialization.Tuple6Serializer(
		ballerinaserialization.StringSerializer,
		ballerinaserialization.BoolSerializer,
		ballerinaserialization.IntSerializer,
		ballerinaserialization.FloatSerializer,
		ballerinaserialization.StringSerializer,
		ballerinaserialization.BoolSerializer,
	)
	deserializer := ballerinaserialization.Tuple6Deserializer(
		ballerinaserialization.StringDeserializer,
		ballerinaserialization.BoolDeserializer,
		ballerinaserialization.IntDeserializer,
		ballerinaserialization.FloatDeserializer,
		ballerinaserialization.StringDeserializer,
		ballerinaserialization.BoolDeserializer,
	)

	mappedTupleWithError := ballerina.Bind(serializer(tuple), deserializer)

	require.Equal(t, ballerina.Right[error, ballerina.Tuple6[string, bool, int, float64, string, bool]](tuple), mappedTupleWithError)
}

func TestTuple7Serialization(t *testing.T) {
	t.Parallel()

	tuple := ballerina.Tuple7[string, bool, int, float64, string, bool, int]{
		Item1: "hello",
		Item2: true,
		Item3: int(42),
		Item4: float64(3.14),
		Item5: "world",
		Item6: false,
		Item7: int(100),
	}

	serializer := ballerinaserialization.Tuple7Serializer(
		ballerinaserialization.StringSerializer,
		ballerinaserialization.BoolSerializer,
		ballerinaserialization.IntSerializer,
		ballerinaserialization.FloatSerializer,
		ballerinaserialization.StringSerializer,
		ballerinaserialization.BoolSerializer,
		ballerinaserialization.IntSerializer,
	)
	deserializer := ballerinaserialization.Tuple7Deserializer(
		ballerinaserialization.StringDeserializer,
		ballerinaserialization.BoolDeserializer,
		ballerinaserialization.IntDeserializer,
		ballerinaserialization.FloatDeserializer,
		ballerinaserialization.StringDeserializer,
		ballerinaserialization.BoolDeserializer,
		ballerinaserialization.IntDeserializer,
	)

	mappedTupleWithError := ballerina.Bind(serializer(tuple), deserializer)

	require.Equal(t, ballerina.Right[error, ballerina.Tuple7[string, bool, int, float64, string, bool, int]](tuple), mappedTupleWithError)
}

func TestTuple8Serialization(t *testing.T) {
	t.Parallel()

	tuple := ballerina.Tuple8[string, bool, int, float64, string, bool, int, float64]{
		Item1: "hello",
		Item2: true,
		Item3: int(42),
		Item4: float64(3.14),
		Item5: "world",
		Item6: false,
		Item7: int(100),
		Item8: float64(2.71),
	}

	serializer := ballerinaserialization.Tuple8Serializer(
		ballerinaserialization.StringSerializer,
		ballerinaserialization.BoolSerializer,
		ballerinaserialization.IntSerializer,
		ballerinaserialization.FloatSerializer,
		ballerinaserialization.StringSerializer,
		ballerinaserialization.BoolSerializer,
		ballerinaserialization.IntSerializer,
		ballerinaserialization.FloatSerializer,
	)
	deserializer := ballerinaserialization.Tuple8Deserializer(
		ballerinaserialization.StringDeserializer,
		ballerinaserialization.BoolDeserializer,
		ballerinaserialization.IntDeserializer,
		ballerinaserialization.FloatDeserializer,
		ballerinaserialization.StringDeserializer,
		ballerinaserialization.BoolDeserializer,
		ballerinaserialization.IntDeserializer,
		ballerinaserialization.FloatDeserializer,
	)

	mappedTupleWithError := ballerina.Bind(serializer(tuple), deserializer)

	require.Equal(t, ballerina.Right[error, ballerina.Tuple8[string, bool, int, float64, string, bool, int, float64]](tuple), mappedTupleWithError)
}

func TestTuple9Serialization(t *testing.T) {
	t.Parallel()

	tuple := ballerina.Tuple9[string, bool, int, float64, string, bool, int, float64, string]{
		Item1: "hello",
		Item2: true,
		Item3: int(42),
		Item4: float64(3.14),
		Item5: "world",
		Item6: false,
		Item7: int(100),
		Item8: float64(2.71),
		Item9: "final",
	}

	serializer := ballerinaserialization.Tuple9Serializer(
		ballerinaserialization.StringSerializer,
		ballerinaserialization.BoolSerializer,
		ballerinaserialization.IntSerializer,
		ballerinaserialization.FloatSerializer,
		ballerinaserialization.StringSerializer,
		ballerinaserialization.BoolSerializer,
		ballerinaserialization.IntSerializer,
		ballerinaserialization.FloatSerializer,
		ballerinaserialization.StringSerializer,
	)
	deserializer := ballerinaserialization.Tuple9Deserializer(
		ballerinaserialization.StringDeserializer,
		ballerinaserialization.BoolDeserializer,
		ballerinaserialization.IntDeserializer,
		ballerinaserialization.FloatDeserializer,
		ballerinaserialization.StringDeserializer,
		ballerinaserialization.BoolDeserializer,
		ballerinaserialization.IntDeserializer,
		ballerinaserialization.FloatDeserializer,
		ballerinaserialization.StringDeserializer,
	)

	mappedTupleWithError := ballerina.Bind(serializer(tuple), deserializer)

	require.Equal(t, ballerina.Right[error, ballerina.Tuple9[string, bool, int, float64, string, bool, int, float64, string]](tuple), mappedTupleWithError)
}

func TestTuple10Serialization(t *testing.T) {
	t.Parallel()

	tuple := ballerina.Tuple10[string, bool, int, float64, string, bool, int, float64, string, bool]{
		Item1:  "hello",
		Item2:  true,
		Item3:  int(42),
		Item4:  float64(3.14),
		Item5:  "world",
		Item6:  false,
		Item7:  int(100),
		Item8:  float64(2.71),
		Item9:  "final",
		Item10: true,
	}

	serializer := ballerinaserialization.Tuple10Serializer(
		ballerinaserialization.StringSerializer,
		ballerinaserialization.BoolSerializer,
		ballerinaserialization.IntSerializer,
		ballerinaserialization.FloatSerializer,
		ballerinaserialization.StringSerializer,
		ballerinaserialization.BoolSerializer,
		ballerinaserialization.IntSerializer,
		ballerinaserialization.FloatSerializer,
		ballerinaserialization.StringSerializer,
		ballerinaserialization.BoolSerializer,
	)
	deserializer := ballerinaserialization.Tuple10Deserializer(
		ballerinaserialization.StringDeserializer,
		ballerinaserialization.BoolDeserializer,
		ballerinaserialization.IntDeserializer,
		ballerinaserialization.FloatDeserializer,
		ballerinaserialization.StringDeserializer,
		ballerinaserialization.BoolDeserializer,
		ballerinaserialization.IntDeserializer,
		ballerinaserialization.FloatDeserializer,
		ballerinaserialization.StringDeserializer,
		ballerinaserialization.BoolDeserializer,
	)

	mappedTupleWithError := ballerina.Bind(serializer(tuple), deserializer)

	require.Equal(t, ballerina.Right[error, ballerina.Tuple10[string, bool, int, float64, string, bool, int, float64, string, bool]](tuple), mappedTupleWithError)
}

func TestTuple11Serialization(t *testing.T) {
	t.Parallel()

	tuple := ballerina.Tuple11[string, bool, int, float64, string, bool, int, float64, string, bool, int]{
		Item1:  "hello",
		Item2:  true,
		Item3:  int(42),
		Item4:  float64(3.14),
		Item5:  "world",
		Item6:  false,
		Item7:  int(100),
		Item8:  float64(2.71),
		Item9:  "final",
		Item10: true,
		Item11: int(100),
	}

	serializer := ballerinaserialization.Tuple11Serializer(
		ballerinaserialization.StringSerializer,
		ballerinaserialization.BoolSerializer,
		ballerinaserialization.IntSerializer,
		ballerinaserialization.FloatSerializer,
		ballerinaserialization.StringSerializer,
		ballerinaserialization.BoolSerializer,
		ballerinaserialization.IntSerializer,
		ballerinaserialization.FloatSerializer,
		ballerinaserialization.StringSerializer,
		ballerinaserialization.BoolSerializer,
		ballerinaserialization.IntSerializer,
	)
	deserializer := ballerinaserialization.Tuple11Deserializer(
		ballerinaserialization.StringDeserializer,
		ballerinaserialization.BoolDeserializer,
		ballerinaserialization.IntDeserializer,
		ballerinaserialization.FloatDeserializer,
		ballerinaserialization.StringDeserializer,
		ballerinaserialization.BoolDeserializer,
		ballerinaserialization.IntDeserializer,
		ballerinaserialization.FloatDeserializer,
		ballerinaserialization.StringDeserializer,
		ballerinaserialization.BoolDeserializer,
		ballerinaserialization.IntDeserializer,
	)

	mappedTupleWithError := ballerina.Bind(serializer(tuple), deserializer)

	require.Equal(t, ballerina.Right[error, ballerina.Tuple11[string, bool, int, float64, string, bool, int, float64, string, bool, int]](tuple), mappedTupleWithError)
}
