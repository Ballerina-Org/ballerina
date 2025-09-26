package ballerina_test

import (
	"testing"

	ballerina "ballerina.com/core"
)

func TestDeltaLazySerialization(t *testing.T) {
	delta := ballerina.NewDeltaLazyValue[string, ballerina.DeltaString](ballerina.NewDeltaStringReplace("delta value"))
	assertBackAndForthFromJsonYieldsSameValue(t, delta)
}
