package ballerina_test

import (
	"testing"

	ballerina "ballerina.com/core"
	"github.com/stretchr/testify/suite"
)

type DeltaOneSerializationTestSuite struct {
	suite.Suite
}

func (s *DeltaOneSerializationTestSuite) TestReplace() {
	delta := ballerina.NewDeltaOneReplace[string, ballerina.DeltaString]("replacement value")

	assertBackAndForthFromJsonYieldsSameValue(s.T(), delta)
}

func (s *DeltaOneSerializationTestSuite) TestValue() {
	delta := ballerina.NewDeltaOneValue[string, ballerina.DeltaString](ballerina.NewDeltaStringReplace("delta value"))

	assertBackAndForthFromJsonYieldsSameValue(s.T(), delta)
}

func TestDeltaOneSerializationTestSuite(t *testing.T) {
	suite.Run(t, new(DeltaOneSerializationTestSuite))
}
