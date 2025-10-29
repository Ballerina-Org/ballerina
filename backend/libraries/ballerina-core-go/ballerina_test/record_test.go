package ballerina_test

import (
	"encoding/json"
	"testing"

	ballerina "ballerina.com/core"
	"ballerina.com/core/ballerinaserialization"
	"github.com/stretchr/testify/require"
)

func TestDeserializeRecord_Success(t *testing.T) {
	t.Parallel()

	data := json.RawMessage(`{
        "discriminator": "record",
        "value": [
            ["a", {"discriminator":"string","value":"hello"}],
            ["b", {"discriminator":"int","value":"42"}]
        ]
    }`)

	result := ballerinaserialization.DeserializeRecord(data)
	require.Equal(t, ballerina.Right[error, map[string]json.RawMessage](map[string]json.RawMessage{
		"a": json.RawMessage(`{"discriminator":"string","value":"hello"}`),
		"b": json.RawMessage(`{"discriminator":"int","value":"42"}`),
	}), result)
}

func TestDeserializeRecord_Error_NotRecordDiscriminator(t *testing.T) {
	t.Parallel()
	data := json.RawMessage(`{"discriminator":"not-record","value":[]}`)
	result := ballerinaserialization.DeserializeRecord(data)
	assertErrorContains(t, "expected discriminator to be 'record'", result)
}

func TestDeserializeRecord_Error_MissingValue(t *testing.T) {
	t.Parallel()
	data := json.RawMessage(`{"discriminator":"record"}`)
	result := ballerinaserialization.DeserializeRecord(data)
	assertErrorContains(t, "missing value field", result)
}

func TestDeserializeRecord_Error_BadFieldName(t *testing.T) {
	t.Parallel()
	data := json.RawMessage(`{
        "discriminator": "record",
        "value": [
            [4, {"discriminator":"unit"}]
        ]
    }`)
	result := ballerinaserialization.DeserializeRecord(data)
	assertErrorContains(t, "failed to unmarshal record field name 0", result)
}
