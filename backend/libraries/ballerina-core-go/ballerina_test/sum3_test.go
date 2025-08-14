package ballerina_test

import (
	"fmt"
	"testing"

	ballerina "ballerina.com/core"
)

func TestSum3Serialization(t *testing.T) {
	t.Parallel()
	testCases := []ballerina.Sum3[int, string, float64]{
		ballerina.Case1Of3[int, string, float64](10),
		ballerina.Case2Of3[int, string, float64]("abc"),
		ballerina.Case3Of3[int, string, float64](3.14),
	}

	for caseIndex, testCase := range testCases {
		t.Run(fmt.Sprintf("case %d of %d", caseIndex+1, len(testCases)), func(t *testing.T) {
			assertBackAndForthFromJsonYieldsSameValue(t, testCase)
		})
	}
}
