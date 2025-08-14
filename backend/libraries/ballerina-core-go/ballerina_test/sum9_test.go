package ballerina_test

import (
	"fmt"
	"testing"

	ballerina "ballerina.com/core"
)

func TestSum9Serialization(t *testing.T) {
	t.Parallel()
	testCases := []ballerina.Sum9[int, string, float64, string, int16, bool, int, byte, rune]{
		ballerina.Case1Of9[int, string, float64, string, int16, bool, int, byte, rune](10),
		ballerina.Case2Of9[int, string, float64, string, int16, bool, int, byte, rune]("abc"),
		ballerina.Case3Of9[int, string, float64, string, int16, bool, int, byte, rune](3.14),
		ballerina.Case4Of9[int, string, float64, string, int16, bool, int, byte, rune]("def"),
		ballerina.Case5Of9[int, string, float64, string, int16, bool, int, byte, rune](42),
		ballerina.Case6Of9[int, string, float64, string, int16, bool, int, byte, rune](true),
		ballerina.Case7Of9[int, string, float64, string, int16, bool, int, byte, rune](99),
		ballerina.Case8Of9[int, string, float64, string, int16, bool, int, byte, rune](byte(255)),
		ballerina.Case9Of9[int, string, float64, string, int16, bool, int, byte, rune](rune('Ω')),
	}

	for caseIndex, testCase := range testCases {
		t.Run(fmt.Sprintf("case %d of %d", caseIndex+1, len(testCases)), func(t *testing.T) {
			assertBackAndForthFromJsonYieldsSameValue(t, testCase)
		})
	}
}
