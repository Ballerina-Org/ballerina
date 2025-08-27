package ballerina_test

import (
	"testing"

	ballerina "ballerina.com/core"
	"github.com/stretchr/testify/suite"
)

type DeltaOptionSerializationTestSuite struct {
	suite.Suite
}

func (s *DeltaOptionSerializationTestSuite) TestReplace() {
	delta := ballerina.NewDeltaOptionReplace[string, ballerina.DeltaString](ballerina.Some("hello"))

	assertBackAndForthFromJsonYieldsSameValue(s.T(), delta)
}

func (s *DeltaOptionSerializationTestSuite) TestValue() {
	delta := ballerina.NewDeltaOptionValue[string, ballerina.DeltaString](
		ballerina.NewDeltaStringReplace("world"),
	)

	assertBackAndForthFromJsonYieldsSameValue(s.T(), delta)
}

func TestDeltaOptionSerializationTestSuite(t *testing.T) {
	suite.Run(t, new(DeltaOptionSerializationTestSuite))
}
