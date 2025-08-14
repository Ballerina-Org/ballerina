package ballerina_test

import (
	"fmt"
	"testing"

	ballerina "ballerina.com/core"
)

func TestSum8Serialization(t *testing.T) {
	t.Parallel()
	testCases := []ballerina.Sum8[int, string, float64, string, int16, bool, int, byte]{
		ballerina.Case1Of8[int, string, float64, string, int16, bool, int, byte](10),
		ballerina.Case2Of8[int, string, float64, string, int16, bool, int, byte]("abc"),
		ballerina.Case3Of8[int, string, float64, string, int16, bool, int, byte](3.14),
		ballerina.Case4Of8[int, string, float64, string, int16, bool, int, byte]("def"),
		ballerina.Case5Of8[int, string, float64, string, int16, bool, int, byte](42),
		ballerina.Case6Of8[int, string, float64, string, int16, bool, int, byte](true),
		ballerina.Case7Of8[int, string, float64, string, int16, bool, int, byte](99),
		ballerina.Case8Of8[int, string, float64, string, int16, bool, int, byte](byte(255)),
	}

	for caseIndex, testCase := range testCases {
		t.Run(fmt.Sprintf("case %d of %d", caseIndex+1, len(testCases)), func(t *testing.T) {
			assertBackAndForthFromJsonYieldsSameValue(t, testCase)
		})
	}
}
