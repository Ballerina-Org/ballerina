package ballerina_test

import (
	"encoding/json"
	"fmt"
	"testing"

	ballerina "ballerina.com/core"
	"ballerina.com/core/ballerinaserialization"
	"github.com/stretchr/testify/require"
)

func TestDeserializeUnion_Success(t *testing.T) {
	t.Parallel()

	data := json.RawMessage(`{"discriminator":"union","value":[{"name": "MyCase"},{"discriminator":"int","value":"42"}]}`)

	expected := ballerinaserialization.UnionForSerialization{
		Discriminator: "union",
		Value:         [2]json.RawMessage{json.RawMessage(`{"name": "MyCase"}`), json.RawMessage(`{"discriminator":"int","value":"42"}`)},
	}

	result := ballerinaserialization.DeserializeUnion(data)
	require.Equal(t, ballerina.Right[error, ballerinaserialization.UnionForSerialization](expected), result)
}

func TestUnionGetCaseName_Success(t *testing.T) {
	t.Parallel()
	data := json.RawMessage(`{"discriminator":"union","value":[{"name": "MyCase"},{"discriminator":"unit"}]}`)
	deserialized := ballerinaserialization.DeserializeUnion(data)

	// Ensure it deserialized and then extract case name
	err := ballerina.Fold(deserialized, func(err error) error {
		return fmt.Errorf("unexpected error: %v", err)
	}, func(u ballerinaserialization.UnionForSerialization) error {
		require.Equal(t, ballerina.Right[error, string]("MyCase"), u.GetCaseName())
		return nil
	})
	require.NoError(t, err)
}

func TestDeserializeUnion_Error_NotUnionDiscriminator(t *testing.T) {
	t.Parallel()
	data := json.RawMessage(`{"discriminator":"not-union","value":[{"name": "Case"},{"discriminator":"unit"}]}`)
	result := ballerinaserialization.DeserializeUnion(data)
	assertErrorContains(t, "expected discriminator to be 'union'", result)
}

func TestUnionGetCaseName_Error_BadHeader(t *testing.T) {
	t.Parallel()
	// First element is not an object with name; use a number to force unmarshal error
	data := json.RawMessage(`{"discriminator":"union","value":[123,{"discriminator":"unit"}]}`)
	deserialized := ballerinaserialization.DeserializeUnion(data)

	err := ballerina.Fold(deserialized, func(err error) error {
		// if overall deserialization fails, still acceptable as error
		return fmt.Errorf("unexpected error: %v", err)
	}, func(u ballerinaserialization.UnionForSerialization) error {
		res := u.GetCaseName()
		assertErrorContains(t, "cannot unmarshal", res)
		return nil
	})
	require.NoError(t, err)
}
