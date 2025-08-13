package ballerina_test

import (
	"encoding/json"
	"testing"

	"github.com/stretchr/testify/require"
)

func assertBackAndForthFromJsonYieldsSameValue[T any](t *testing.T, value T) {
	t.Helper()
	asJson, err := json.Marshal(value)
	require.NoError(t, err)

	var unmarshalled T
	require.NoError(t, json.Unmarshal(asJson, &unmarshalled))
	require.Equal(t, value, unmarshalled)
}
