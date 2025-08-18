package ballerina_test

import (
	"encoding/json"
	"fmt"
	"testing"

	ballerina "ballerina.com/core"
	"github.com/stretchr/testify/require"
)

func TestSerializedSerialization(t *testing.T) {
	t.Parallel()
	testCases := []struct {
		Serialized ballerina.Serialized
		Expected   string
	}{
		{Serialized: ballerina.Serialized{}.Null(), Expected: "null"},
		{Serialized: ballerina.Serialized{}.String("hello"), Expected: `"hello"`},
		{Serialized: ballerina.Serialized{}.Int64(123), Expected: `123`},
		{Serialized: ballerina.Serialized{}.Bool(true), Expected: `true`},
		{Serialized: ballerina.Serialized{}.Float64(123.456), Expected: `123.456`},
		{Serialized: ballerina.Serialized{}.Map(map[string]ballerina.Serialized{"hello": ballerina.Serialized{}.Null()}), Expected: `{"hello":null}`},
		{Serialized: ballerina.Serialized{}.Array([]ballerina.Serialized{ballerina.Serialized{}.Null()}), Expected: `[null]`},
	}

	for caseIndex, testCase := range testCases {
		t.Run(fmt.Sprintf("case %d of %d", caseIndex+1, len(testCases)), func(t *testing.T) {
			json, err := json.Marshal(testCase.Serialized)
			require.NoError(t, err)
			require.Equal(t, testCase.Expected, string(json))
		})
	}
}

func TestSerializedDeserialization(t *testing.T) {
	t.Parallel()
	testCases := []struct {
		Json     string
		Expected ballerina.Serialized
	}{
		{Json: "null", Expected: ballerina.Serialized{}.Null()},
		{Json: `"hello"`, Expected: ballerina.Serialized{}.String("hello")},
		{Json: `123`, Expected: ballerina.Serialized{}.Int64(123)},
		{Json: `true`, Expected: ballerina.Serialized{}.Bool(true)},
		{Json: `123.456`, Expected: ballerina.Serialized{}.Float64(123.456)},
		{Json: `{"hello":null}`, Expected: ballerina.Serialized{}.Map(map[string]ballerina.Serialized{"hello": ballerina.Serialized{}.Null()})},
		{Json: `[null]`, Expected: ballerina.Serialized{}.Array([]ballerina.Serialized{ballerina.Serialized{}.Null()})},
	}

	for caseIndex, testCase := range testCases {
		t.Run(fmt.Sprintf("case %d of %d", caseIndex+1, len(testCases)), func(t *testing.T) {
			var deserialized ballerina.Serialized
			err := json.Unmarshal([]byte(testCase.Json), &deserialized)
			require.NoError(t, err)
			require.Equal(t, testCase.Expected, deserialized)
		})
	}
}

func TestSerializedSerializationBackAndForth(t *testing.T) {
	t.Parallel()
	testCases := []ballerina.Serialized{
		ballerina.Serialized{}.Null(),
		ballerina.Serialized{}.String("hello"),
		ballerina.Serialized{}.Int64(123),
		ballerina.Serialized{}.Bool(true),
		ballerina.Serialized{}.Float64(123.456),
		ballerina.Serialized{}.Map(map[string]ballerina.Serialized{"hello": ballerina.Serialized{}.Null()}),
		ballerina.Serialized{}.Array([]ballerina.Serialized{ballerina.Serialized{}.Null()}),
	}

	for caseIndex, testCase := range testCases {
		t.Run(fmt.Sprintf("case %d of %d", caseIndex+1, len(testCases)), func(t *testing.T) {
			assertBackAndForthFromJsonYieldsSameValue(t, testCase)
		})
	}
}
