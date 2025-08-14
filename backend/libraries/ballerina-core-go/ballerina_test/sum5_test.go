package ballerina_test

import (
	"fmt"
	"testing"

	ballerina "ballerina.com/core"
)

func TestSum5Serialization(t *testing.T) {
	t.Parallel()
	testCases := []ballerina.Sum5[int, string, float64, string, int16]{
		ballerina.Case1Of5[int, string, float64, string, int16](10),
		ballerina.Case2Of5[int, string, float64, string, int16]("abc"),
		ballerina.Case3Of5[int, string, float64, string, int16](3.14),
		ballerina.Case4Of5[int, string, float64, string, int16]("def"),
		ballerina.Case5Of5[int, string, float64, string, int16](42),
	}

	for caseIndex, testCase := range testCases {
		t.Run(fmt.Sprintf("case %d of %d", caseIndex+1, len(testCases)), func(t *testing.T) {
			assertBackAndForthFromJsonYieldsSameValue(t, testCase)
		})
	}
}
