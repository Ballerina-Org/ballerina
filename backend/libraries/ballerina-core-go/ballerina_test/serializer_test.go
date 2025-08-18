package ballerina_test

import (
	"testing"

	ballerina "ballerina.com/core"
	"github.com/stretchr/testify/require"
)

func assertBackAndForthSerializationYieldsSameValue[T any](t *testing.T, value T, serializer ballerina.Serializer[T]) {
	t.Helper()
	serialized := serializer.Serialize(value)
	deserialized, err := serializer.Deserialize(serialized)
	require.NoError(t, err)
	require.Equal(t, value, deserialized)
}

func TestUnitSerializer(t *testing.T) {
	t.Parallel()
	serializer := ballerina.UnitSerializer{}

	t.Run("serialize and deserialize Unit", func(t *testing.T) {
		serialized := serializer.Serialize(ballerina.Unit{})
		deserialized, err := serializer.Deserialize(serialized)
		require.NoError(t, err)
		require.Equal(t, ballerina.Unit{}, deserialized)
	})
}
