package ballerina_test

import (
	"fmt"
	"testing"

	ballerina "ballerina.com/core"
)

func TestSum7Serialization(t *testing.T) {
	t.Parallel()
	testCases := []ballerina.Sum7[int, string, float64, string, int16, bool, int]{
		ballerina.Case1Of7[int, string, float64, string, int16, bool, int](10),
		ballerina.Case2Of7[int, string, float64, string, int16, bool, int]("abc"),
		ballerina.Case3Of7[int, string, float64, string, int16, bool, int](3.14),
		ballerina.Case4Of7[int, string, float64, string, int16, bool, int]("def"),
		ballerina.Case5Of7[int, string, float64, string, int16, bool, int](42),
		ballerina.Case6Of7[int, string, float64, string, int16, bool, int](true),
		ballerina.Case7Of7[int, string, float64, string, int16, bool, int](99),
	}

	for caseIndex, testCase := range testCases {
		t.Run(fmt.Sprintf("case %d of %d", caseIndex+1, len(testCases)), func(t *testing.T) {
			assertBackAndForthFromJsonYieldsSameValue(t, testCase)
		})
	}
}
