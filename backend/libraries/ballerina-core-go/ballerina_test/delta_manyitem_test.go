package ballerina_test

import (
	"testing"

	ballerina "ballerina.com/core"
	"github.com/stretchr/testify/suite"
)

type DeltaManyItemSerializationTestSuite struct {
	suite.Suite
}

func (s *DeltaManyItemSerializationTestSuite) TestValue() {
	delta := ballerina.NewDeltaManyItemValue[string, ballerina.DeltaString](ballerina.NewDeltaStringReplace("new value"))

	assertBackAndForthFromJsonYieldsSameValue(s.T(), delta)
}

func (s *DeltaManyItemSerializationTestSuite) TestIsLinked() {
	delta := ballerina.NewDeltaManyItemIsLinked[string, ballerina.DeltaString](true)

	assertBackAndForthFromJsonYieldsSameValue(s.T(), delta)
}

func (s *DeltaManyItemSerializationTestSuite) TestCanChangeLink() {
	delta := ballerina.NewDeltaManyItemCanChangeLink[string, ballerina.DeltaString](false)

	assertBackAndForthFromJsonYieldsSameValue(s.T(), delta)
}

func TestDeltaManyItemSerializationTestSuite(t *testing.T) {
	suite.Run(t, new(DeltaManyItemSerializationTestSuite))
}
