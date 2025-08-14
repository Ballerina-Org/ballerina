package ballerina_test

import (
	"fmt"
	"testing"

	ballerina "ballerina.com/core"
)

func TestSum4Serialization(t *testing.T) {
	t.Parallel()
	testCases := []ballerina.Sum4[int, string, float64, string]{
		ballerina.Case1Of4[int, string, float64, string](10),
		ballerina.Case2Of4[int, string, float64, string]("abc"),
		ballerina.Case3Of4[int, string, float64, string](3.14),
		ballerina.Case4Of4[int, string, float64, string]("def"),
	}

	for caseIndex, testCase := range testCases {
		t.Run(fmt.Sprintf("case %d of %d", caseIndex+1, len(testCases)), func(t *testing.T) {
			assertBackAndForthFromJsonYieldsSameValue(t, testCase)
		})
	}
}
