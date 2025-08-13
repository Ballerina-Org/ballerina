package ballerina_test

import (
	"testing"

	ballerina "ballerina.com/core"
	"github.com/stretchr/testify/suite"
)

type DeltaSetSerializationTestSuite struct {
	suite.Suite
}

func (s *DeltaSetSerializationTestSuite) TestValue() {
	delta := ballerina.NewDeltaSetValue[string, ballerina.DeltaString]("key", ballerina.NewDeltaStringReplace("new value"))

	assertBackAndForthFromJsonYieldsSameValue(s.T(), delta)
}

func (s *DeltaSetSerializationTestSuite) TestAdd() {
	delta := ballerina.NewDeltaSetAdd[string, ballerina.DeltaString]("new element")

	assertBackAndForthFromJsonYieldsSameValue(s.T(), delta)
}

func (s *DeltaSetSerializationTestSuite) TestRemove() {
	delta := ballerina.NewDeltaSetRemove[string, ballerina.DeltaString]("element to remove")

	assertBackAndForthFromJsonYieldsSameValue(s.T(), delta)
}

func TestDeltaSetSerializationTestSuite(t *testing.T) {
	suite.Run(t, new(DeltaSetSerializationTestSuite))
}
