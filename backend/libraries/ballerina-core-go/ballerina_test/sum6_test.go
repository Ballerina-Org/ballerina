package ballerina_test

import (
	"fmt"
	"testing"

	ballerina "ballerina.com/core"
)

func TestSum6Serialization(t *testing.T) {
	t.Parallel()
	testCases := []ballerina.Sum6[int, string, float64, string, int16, bool]{
		ballerina.Case1Of6[int, string, float64, string, int16, bool](10),
		ballerina.Case2Of6[int, string, float64, string, int16, bool]("abc"),
		ballerina.Case3Of6[int, string, float64, string, int16, bool](3.14),
		ballerina.Case4Of6[int, string, float64, string, int16, bool]("def"),
		ballerina.Case5Of6[int, string, float64, string, int16, bool](42),
		ballerina.Case6Of6[int, string, float64, string, int16, bool](true),
	}

	for caseIndex, testCase := range testCases {
		t.Run(fmt.Sprintf("case %d of %d", caseIndex+1, len(testCases)), func(t *testing.T) {
			assertBackAndForthFromJsonYieldsSameValue(t, testCase)
		})
	}
}
