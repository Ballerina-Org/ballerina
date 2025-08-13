package ballerina_test

import (
	"encoding/json"
	"testing"

	"github.com/stretchr/testify/require"
)

func backAndForthFromJson[T any](t *testing.T, value T) T {
	t.Helper()
	asJson, err := json.Marshal(value)
	require.NoError(t, err)

	var unmarshalled T
	require.NoError(t, json.Unmarshal(asJson, &unmarshalled))
	return unmarshalled
}
