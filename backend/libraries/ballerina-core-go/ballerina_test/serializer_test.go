package ballerina_test

import (
	"testing"

	ballerina "ballerina.com/core"
	"github.com/stretchr/testify/require"
)

func assertBackAndForthSerializationYieldsSameValue[T any](t *testing.T, value T, serializer ballerina.Serializer[T], deserializer ballerina.Deserializer[T]) {
	t.Helper()
	serialized := serializer.Serialize(value)
	deserialized, err := deserializer.Deserialize(serialized)
	require.NoError(t, err)
	require.Equal(t, value, deserialized)
}

func TestUnitSerialization(t *testing.T) {
	t.Parallel()
	serializer := ballerina.UnitSerializer{}
	deserializer := ballerina.UnitDeserializer{}

	t.Run("serialize and deserialize Unit", func(t *testing.T) {
		assertBackAndForthSerializationYieldsSameValue(t, ballerina.Unit{}, serializer, deserializer)
	})
}

func TestSumSerialization(t *testing.T) {
	t.Parallel()
	serializer := ballerina.SumSerializer[ballerina.Unit, ballerina.Unit]{
		SerializerLeft:  ballerina.UnitSerializer{},
		SerializerRight: ballerina.UnitSerializer{},
	}
	deserializer := ballerina.SumDeserializer[ballerina.Unit, ballerina.Unit]{
		DeserializerLeft:  ballerina.UnitDeserializer{},
		DeserializerRight: ballerina.UnitDeserializer{},
	}

	t.Run("serialize and deserialize Sum.Left", func(t *testing.T) {
		assertBackAndForthSerializationYieldsSameValue(t, ballerina.Left[ballerina.Unit, ballerina.Unit](ballerina.Unit{}), serializer, deserializer)
	})

	t.Run("serialize and deserialize Sum.Right", func(t *testing.T) {
		assertBackAndForthSerializationYieldsSameValue(t, ballerina.Right[ballerina.Unit, ballerina.Unit](ballerina.Unit{}), serializer, deserializer)
	})
}
