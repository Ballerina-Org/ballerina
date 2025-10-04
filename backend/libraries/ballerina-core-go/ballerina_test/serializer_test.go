package ballerina_test

import (
	"encoding/json"
	"fmt"
	"testing"
	"time"

	ballerina "ballerina.com/core"
	"ballerina.com/core/ballerinaserialization"
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
		{name: "not-unit-kind", serialized: json.RawMessage(`{"discriminator":"not-unit"}`), expectedError: "on unit: expected kind to be 'unit', got 'not-unit'"},
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
		{name: "left", sum: ballerina.Left[ballerina.Unit, ballerina.Unit](ballerina.Unit{}), expected: json.RawMessage(`{"discriminator":"sum","value":[0,{"discriminator":"unit"}]}`)},
		{name: "right", sum: ballerina.Right[ballerina.Unit, ballerina.Unit](ballerina.Unit{}), expected: json.RawMessage(`{"discriminator":"sum","value":[1,{"discriminator":"unit"}]}`)},
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
		{name: "left", serialized: json.RawMessage(`{"discriminator":"sum","value":[0,{"discriminator":"unit"}]}`), expected: ballerina.Left[ballerina.Unit, ballerina.Unit](ballerina.Unit{})},
		{name: "right", serialized: json.RawMessage(`{"discriminator":"sum","value":[1,{"discriminator":"unit"}]}`), expected: ballerina.Right[ballerina.Unit, ballerina.Unit](ballerina.Unit{})},
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
		{name: "on-sum-on-left", serialized: json.RawMessage(`{"discriminator":"sum","value":[0,{"discriminator":"not-unit"}]}`), expectedError: "on sum: on left: on unit: expected kind to be 'unit', got 'not-unit'"},
		{name: "on-sum-on-right", serialized: json.RawMessage(`{"discriminator":"sum","value":[1,{"discriminator":"not-unit"}]}`), expectedError: "on sum: on right: on unit: expected kind to be 'unit', got 'not-unit'"},
		{name: "not-sum-case", serialized: json.RawMessage(`{"discriminator":"not-sum","value":[0,{"discriminator":"unit"}]}`), expectedError: "on sum: expected discriminator to be 'sum', got 'not-sum'"},
		{name: "no-value-field", serialized: json.RawMessage(`{"discriminator":"sum"}`), expectedError: "on sum: expected 2 elements in sum, got 0"},
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
		{name: "none", option: ballerina.None[ballerina.Unit](), expected: json.RawMessage(`{"discriminator":"sum","value":[0,{"discriminator":"unit"}]}`)},
		{name: "some", option: ballerina.Some(ballerina.Unit{}), expected: json.RawMessage(`{"discriminator":"sum","value":[1,{"discriminator":"unit"}]}`)},
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
		{name: "none", serialized: json.RawMessage(`{"discriminator":"sum","value":[0,{"discriminator":"unit"}]}`), expected: ballerina.None[ballerina.Unit]()},
		{name: "some", serialized: json.RawMessage(`{"discriminator":"sum","value":[1,{"discriminator":"unit"}]}`), expected: ballerina.Some(ballerina.Unit{})},
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
	require.Equal(t, ballerina.Right[error, []bool]([]bool{true, false, true}), deserialized)
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
	serialized := serializer(int64(123))
	require.Equal(t, ballerina.Right[error, json.RawMessage](json.RawMessage(`{"discriminator":"int","value":"123"}`)), serialized)
}

func TestIntDeserialization(t *testing.T) {
	t.Parallel()
	deserializer := ballerinaserialization.IntDeserializer
	serialized := json.RawMessage(`{"discriminator":"int","value":"123"}`)
	deserialized := deserializer(serialized)
	require.Equal(t, ballerina.Right[error, int64](123), deserialized)
}

func TestFloatSerialization(t *testing.T) {
	t.Parallel()
	serializer := ballerinaserialization.FloatSerializer
	serialized := serializer(float64(1.75))
	require.Equal(t, ballerina.Right[error, json.RawMessage](json.RawMessage(`{"discriminator":"float","value":"1.75"}`)), serialized)
}

func TestFloatDeserialization(t *testing.T) {
	t.Parallel()
	deserializer := ballerinaserialization.FloatDeserializer
	serialized := json.RawMessage(`{"discriminator":"float","value":"1.75"}`)
	deserialized := deserializer(serialized)
	require.Equal(t, ballerina.Right[error, float64](1.75), deserialized)
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

type SomeRecord struct {
	A int64
	B string
	C bool
}

func SomeRecordSerializer() ballerinaserialization.Serializer[SomeRecord] {
	aSerializer := ballerinaserialization.IntSerializer
	bSerializer := ballerinaserialization.StringSerializer
	cSerializer := ballerinaserialization.BoolSerializer
	return func(value SomeRecord) ballerina.Sum[error, json.RawMessage] {
		return ballerina.Bind(
			ballerinaserialization.WithContext(
				"on item1",
				aSerializer,
			)(value.A),
			func(item1 json.RawMessage) ballerina.Sum[error, json.RawMessage] {
				return ballerina.Bind(
					ballerinaserialization.WithContext(
						"on item2",
						bSerializer,
					)(value.B),
					func(item2 json.RawMessage) ballerina.Sum[error, json.RawMessage] {
						return ballerina.Bind(
							ballerinaserialization.WithContext(
								"on item3",
								cSerializer,
							)(value.C),
							func(item3 json.RawMessage) ballerina.Sum[error, json.RawMessage] {
								return ballerinaserialization.WrappedMarshal(
									ballerinaserialization.NewRecordForSerialization(
										[][2]json.RawMessage{
											{
												json.RawMessage(`{"name": "A"}`), item1,
											},
											{
												json.RawMessage(`{"name": "B"}`), item2,
											},
											{
												json.RawMessage(`{"name": "C"}`), item3,
											},
										},
									),
								)
							},
						)
					},
				)
			},
		)
	}
}

func SomeRecordDeserializer() ballerinaserialization.Deserializer[SomeRecord] {
	aDeserializer := ballerinaserialization.IntDeserializer
	bDeserializer := ballerinaserialization.StringDeserializer
	cDeserializer := ballerinaserialization.BoolDeserializer
	return ballerinaserialization.WithContext(
		"on record",
		func(value json.RawMessage) ballerina.Sum[error, SomeRecord] {
			return ballerina.Bind(
				ballerinaserialization.WrappedUnmarshal[ballerinaserialization.RecordForSerialization](value),
				func(recordForSerialization ballerinaserialization.RecordForSerialization) ballerina.Sum[error, SomeRecord] {
					if len(recordForSerialization.Value) != 3 {
						return ballerina.Left[error, SomeRecord](fmt.Errorf("expected 3 fields in record, got %d", len(recordForSerialization.Value)))
					}
					fields := recordForSerialization.Value
					return ballerina.Bind(
						ballerinaserialization.WithContext("on field A", aDeserializer)(fields[0][1]),
						func(item1 int64) ballerina.Sum[error, SomeRecord] {
							return ballerina.Bind(
								ballerinaserialization.WithContext("on field B", bDeserializer)(fields[1][1]),
								func(item2 string) ballerina.Sum[error, SomeRecord] {
									return ballerina.MapRight(
										ballerinaserialization.WithContext("on field C", cDeserializer)(fields[2][1]),
										func(item3 bool) SomeRecord {
											return SomeRecord{A: item1, B: item2, C: item3}
										},
									)
								},
							)
						},
					)
				},
			)
		},
	)
}

type _SomeUnionCases string

const (
	_A _SomeUnionCases = "A"
	_B _SomeUnionCases = "B"
	_C _SomeUnionCases = "C"
)

type SomeUnion struct {
	discriminator _SomeUnionCases
	_A            *int64
	_B            *string
	_C            *bool
}

func NewSomeUnionA(value int64) SomeUnion {
	return SomeUnion{
		discriminator: _A,
		_A:            &value,
	}
}
func NewSomeUnionB(value string) SomeUnion {
	return SomeUnion{
		discriminator: _B,
		_B:            &value,
	}
}
func NewSomeUnionC(value bool) SomeUnion {
	return SomeUnion{
		discriminator: _C,
		_C:            &value,
	}
}

func MatchSomeUnion[Result any](
	value SomeUnion,
	onA func(int64) (Result, error),
	onB func(string) (Result, error),
	onC func(bool) (Result, error),
) (Result, error) {
	switch value.discriminator {
	case _A:
		result, err := onA(*value._A)
		if err != nil {
			return *new(Result), fmt.Errorf("onA:%w", err)
		}
		return result, nil
	case _B:
		result, err := onB(*value._B)
		if err != nil {
			return *new(Result), fmt.Errorf("onB:%w", err)
		}
		return result, nil
	case _C:
		result, err := onC(*value._C)
		if err != nil {
			return *new(Result), fmt.Errorf("onC:%w", err)
		}
		return result, nil
	}
	return *new(Result), fmt.Errorf("%s is not a valid discriminator value", value.discriminator)
}

func SomeUnionSerializer() ballerinaserialization.Serializer[SomeUnion] {
	aSerializer := ballerinaserialization.IntSerializer
	bSerializer := ballerinaserialization.StringSerializer
	cSerializer := ballerinaserialization.BoolSerializer
	return func(value SomeUnion) ballerina.Sum[error, json.RawMessage] {
		serialized, err := MatchSomeUnion(
			value,
			func(item1 int64) (ballerina.Sum[error, json.RawMessage], error) {
				return ballerina.Bind(
					ballerinaserialization.WithContext("on A", aSerializer)(item1),
					func(item1 json.RawMessage) ballerina.Sum[error, json.RawMessage] {
						return ballerinaserialization.WrappedMarshal(
							ballerinaserialization.NewUnionForSerialization("A", item1),
						)
					},
				), nil
			},
			func(item2 string) (ballerina.Sum[error, json.RawMessage], error) {
				return ballerina.Bind(
					ballerinaserialization.WithContext("on B", bSerializer)(item2),
					func(item2 json.RawMessage) ballerina.Sum[error, json.RawMessage] {
						return ballerinaserialization.WrappedMarshal(
							ballerinaserialization.NewUnionForSerialization("B", item2),
						)
					},
				), nil
			},
			func(item3 bool) (ballerina.Sum[error, json.RawMessage], error) {
				return ballerina.Bind(
					ballerinaserialization.WithContext("on C", cSerializer)(item3),
					func(item3 json.RawMessage) ballerina.Sum[error, json.RawMessage] {
						return ballerinaserialization.WrappedMarshal(
							ballerinaserialization.NewUnionForSerialization("C", item3),
						)
					},
				), nil
			},
		)
		if err != nil {
			return ballerina.Left[error, json.RawMessage](err)
		}
		return serialized
	}
}

func SomeUnionDeserializer() ballerinaserialization.Deserializer[SomeUnion] {
	aDeserializer := ballerinaserialization.IntDeserializer
	bDeserializer := ballerinaserialization.StringDeserializer
	cDeserializer := ballerinaserialization.BoolDeserializer
	return ballerinaserialization.WithContext(
		"on SomeUnion",
		func(value json.RawMessage) ballerina.Sum[error, SomeUnion] {
			return ballerina.Bind(
				ballerinaserialization.WrappedUnmarshal[ballerinaserialization.UnionForSerialization](value),
				func(unionForSerialization ballerinaserialization.UnionForSerialization) ballerina.Sum[error, SomeUnion] {
					return ballerina.Bind(
						unionForSerialization.GetCaseName(),
						func(caseName string) ballerina.Sum[error, SomeUnion] {
							switch caseName {
							case string(_A):
								return ballerina.MapRight(
									ballerinaserialization.WithContext("on case A", aDeserializer)(unionForSerialization.Value[1]),
									NewSomeUnionA,
								)
							case string(_B):
								return ballerina.MapRight(
									ballerinaserialization.WithContext("on case B", bDeserializer)(unionForSerialization.Value[1]),
									NewSomeUnionB,
								)
							case string(_C):
								return ballerina.MapRight(
									ballerinaserialization.WithContext("on case C", cDeserializer)(unionForSerialization.Value[1]),
									NewSomeUnionC,
								)
							default:
								return ballerina.Left[error, SomeUnion](fmt.Errorf("unknown union case: %s", caseName))
							}
						},
					)
				},
			)
		},
	)
}
