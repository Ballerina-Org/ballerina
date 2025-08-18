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
		{Serialized: ballerina.Serialized{Value: ballerina.Case1Of7[ballerina.Null, string, int64, bool, float64, map[string]ballerina.Serialized, []ballerina.Serialized](ballerina.Null{})}, Expected: "null"},
		{Serialized: ballerina.Serialized{Value: ballerina.Case2Of7[ballerina.Null, string, int64, bool, float64, map[string]ballerina.Serialized, []ballerina.Serialized]("hello")}, Expected: `"hello"`},
		{Serialized: ballerina.Serialized{Value: ballerina.Case3Of7[ballerina.Null, string, int64, bool, float64, map[string]ballerina.Serialized, []ballerina.Serialized](123)}, Expected: `123`},
		{Serialized: ballerina.Serialized{Value: ballerina.Case4Of7[ballerina.Null, string, int64, bool, float64, map[string]ballerina.Serialized, []ballerina.Serialized](true)}, Expected: `true`},
		{Serialized: ballerina.Serialized{Value: ballerina.Case5Of7[ballerina.Null, string, int64, bool, float64, map[string]ballerina.Serialized, []ballerina.Serialized](123.456)}, Expected: `123.456`},
		{Serialized: ballerina.Serialized{Value: ballerina.Case6Of7[ballerina.Null, string, int64, bool, float64, map[string]ballerina.Serialized, []ballerina.Serialized](map[string]ballerina.Serialized{"hello": ballerina.Serialized{Value: ballerina.Case1Of7[ballerina.Null, string, int64, bool, float64, map[string]ballerina.Serialized, []ballerina.Serialized](ballerina.Null{})}})}, Expected: `{"hello":null}`},
		{Serialized: ballerina.Serialized{Value: ballerina.Case7Of7[ballerina.Null, string, int64, bool, float64, map[string]ballerina.Serialized, []ballerina.Serialized]([]ballerina.Serialized{ballerina.Serialized{Value: ballerina.Case1Of7[ballerina.Null, string, int64, bool, float64, map[string]ballerina.Serialized, []ballerina.Serialized](ballerina.Null{})}})}, Expected: `[null]`},
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
		{Json: "null", Expected: ballerina.Serialized{Value: ballerina.Case1Of7[ballerina.Null, string, int64, bool, float64, map[string]ballerina.Serialized, []ballerina.Serialized](ballerina.Null{})}},
		{Json: `"hello"`, Expected: ballerina.Serialized{Value: ballerina.Case2Of7[ballerina.Null, string, int64, bool, float64, map[string]ballerina.Serialized, []ballerina.Serialized]("hello")}},
		{Json: `123`, Expected: ballerina.Serialized{Value: ballerina.Case3Of7[ballerina.Null, string, int64, bool, float64, map[string]ballerina.Serialized, []ballerina.Serialized](123)}},
		{Json: `true`, Expected: ballerina.Serialized{Value: ballerina.Case4Of7[ballerina.Null, string, int64, bool, float64, map[string]ballerina.Serialized, []ballerina.Serialized](true)}},
		{Json: `123.456`, Expected: ballerina.Serialized{Value: ballerina.Case5Of7[ballerina.Null, string, int64, bool, float64, map[string]ballerina.Serialized, []ballerina.Serialized](123.456)}},
		{Json: `{"hello":null}`, Expected: ballerina.Serialized{Value: ballerina.Case6Of7[ballerina.Null, string, int64, bool, float64, map[string]ballerina.Serialized, []ballerina.Serialized](map[string]ballerina.Serialized{"hello": ballerina.Serialized{Value: ballerina.Case1Of7[ballerina.Null, string, int64, bool, float64, map[string]ballerina.Serialized, []ballerina.Serialized](ballerina.Null{})}})}},
		{Json: `[null]`, Expected: ballerina.Serialized{Value: ballerina.Case7Of7[ballerina.Null, string, int64, bool, float64, map[string]ballerina.Serialized, []ballerina.Serialized]([]ballerina.Serialized{ballerina.Serialized{Value: ballerina.Case1Of7[ballerina.Null, string, int64, bool, float64, map[string]ballerina.Serialized, []ballerina.Serialized](ballerina.Null{})}})}},
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
		ballerina.Serialized{Value: ballerina.Case1Of7[ballerina.Null, string, int64, bool, float64, map[string]ballerina.Serialized, []ballerina.Serialized](ballerina.Null{})},
		ballerina.Serialized{Value: ballerina.Case2Of7[ballerina.Null, string, int64, bool, float64, map[string]ballerina.Serialized, []ballerina.Serialized]("hello")},
		ballerina.Serialized{Value: ballerina.Case3Of7[ballerina.Null, string, int64, bool, float64, map[string]ballerina.Serialized, []ballerina.Serialized](123)},
		ballerina.Serialized{Value: ballerina.Case4Of7[ballerina.Null, string, int64, bool, float64, map[string]ballerina.Serialized, []ballerina.Serialized](true)},
		ballerina.Serialized{Value: ballerina.Case5Of7[ballerina.Null, string, int64, bool, float64, map[string]ballerina.Serialized, []ballerina.Serialized](123.456)},
		ballerina.Serialized{Value: ballerina.Case6Of7[ballerina.Null, string, int64, bool, float64, map[string]ballerina.Serialized, []ballerina.Serialized](map[string]ballerina.Serialized{"hello": ballerina.Serialized{Value: ballerina.Case1Of7[ballerina.Null, string, int64, bool, float64, map[string]ballerina.Serialized, []ballerina.Serialized](ballerina.Null{})}})},
		ballerina.Serialized{Value: ballerina.Case7Of7[ballerina.Null, string, int64, bool, float64, map[string]ballerina.Serialized, []ballerina.Serialized]([]ballerina.Serialized{ballerina.Serialized{Value: ballerina.Case1Of7[ballerina.Null, string, int64, bool, float64, map[string]ballerina.Serialized, []ballerina.Serialized](ballerina.Null{})}})},
	}

	for caseIndex, testCase := range testCases {
		t.Run(fmt.Sprintf("case %d of %d", caseIndex+1, len(testCases)), func(t *testing.T) {
			assertBackAndForthFromJsonYieldsSameValue(t, testCase)
		})
	}
}
