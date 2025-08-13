package ballerina_test

import (
	"testing"

	ballerina "ballerina.com/core"
	"github.com/stretchr/testify/suite"
)

type DeltaMapSerializationTestSuite struct {
	suite.Suite
}

func (s *DeltaMapSerializationTestSuite) TestKey() {
	delta := ballerina.NewDeltaMapKey[string, int, ballerina.DeltaString, ballerina.DeltaInt32](0, ballerina.NewDeltaStringReplace("new key"))

	assertBackAndForthFromJsonYieldsSameValue(s.T(), delta)
}

func (s *DeltaMapSerializationTestSuite) TestValue() {
	delta := ballerina.NewDeltaMapValue[string, int, ballerina.DeltaString, ballerina.DeltaInt32](1, ballerina.NewDeltaInt32Replace(42))

	assertBackAndForthFromJsonYieldsSameValue(s.T(), delta)
}

func (s *DeltaMapSerializationTestSuite) TestAdd() {
	newElement := ballerina.NewTuple2("key", 100)
	delta := ballerina.NewDeltaMapAdd[string, int, ballerina.DeltaString, ballerina.DeltaInt32](newElement)

	assertBackAndForthFromJsonYieldsSameValue(s.T(), delta)
}

func (s *DeltaMapSerializationTestSuite) TestRemove() {
	delta := ballerina.NewDeltaMapRemove[string, int, ballerina.DeltaString, ballerina.DeltaInt32](2)

	assertBackAndForthFromJsonYieldsSameValue(s.T(), delta)
}

func TestDeltaMapSerializationTestSuite(t *testing.T) {
	suite.Run(t, new(DeltaMapSerializationTestSuite))
}
