package ballerina_test

import (
	"fmt"
	"testing"

	ballerina "ballerina.com/core"
)

func TestSum10Serialization(t *testing.T) {
	t.Parallel()
	testCases := []ballerina.Sum10[int, string, float64, string, int16, bool, int, byte, rune, string]{
		ballerina.Case1Of10[int, string, float64, string, int16, bool, int, byte, rune, string](10),
		ballerina.Case2Of10[int, string, float64, string, int16, bool, int, byte, rune, string]("abc"),
		ballerina.Case3Of10[int, string, float64, string, int16, bool, int, byte, rune, string](3.14),
		ballerina.Case4Of10[int, string, float64, string, int16, bool, int, byte, rune, string]("def"),
		ballerina.Case5Of10[int, string, float64, string, int16, bool, int, byte, rune, string](42),
		ballerina.Case6Of10[int, string, float64, string, int16, bool, int, byte, rune, string](true),
		ballerina.Case7Of10[int, string, float64, string, int16, bool, int, byte, rune, string](99),
		ballerina.Case8Of10[int, string, float64, string, int16, bool, int, byte, rune, string](byte(255)),
		ballerina.Case9Of10[int, string, float64, string, int16, bool, int, byte, rune, string](rune('Ω')),
		ballerina.Case10Of10[int, string, float64, string, int16, bool, int, byte, rune, string]("last"),
	}

	for caseIndex, testCase := range testCases {
		t.Run(fmt.Sprintf("case %d of %d", caseIndex+1, len(testCases)), func(t *testing.T) {
			assertBackAndForthFromJsonYieldsSameValue(t, testCase)
		})
	}
}
