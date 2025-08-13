package ballerina_test

import (
	"testing"

	ballerina "ballerina.com/core"
	"github.com/stretchr/testify/suite"
)

type DeltaArraySerializationTestSuite struct {
	suite.Suite
}

func (s *DeltaArraySerializationTestSuite) TestValue() {
	delta := ballerina.NewDeltaArrayValue[string, ballerina.DeltaString](0, ballerina.NewDeltaStringReplace("hello"))

	assertBackAndForthFromJsonYieldsSameValue(s.T(), delta)
}

func (s *DeltaArraySerializationTestSuite) TestAddAt() {
	delta := ballerina.NewDeltaArrayAddAt[string, ballerina.DeltaString](1, "world")

	assertBackAndForthFromJsonYieldsSameValue(s.T(), delta)
}

func (s *DeltaArraySerializationTestSuite) TestRemoveAt() {
	delta := ballerina.NewDeltaArrayRemoveAt[string, ballerina.DeltaString](2)

	assertBackAndForthFromJsonYieldsSameValue(s.T(), delta)
}

func (s *DeltaArraySerializationTestSuite) TestMoveFromTo() {
	delta := ballerina.NewDeltaArrayMoveFromTo[string, ballerina.DeltaString](0, 3)

	assertBackAndForthFromJsonYieldsSameValue(s.T(), delta)
}

func (s *DeltaArraySerializationTestSuite) TestDuplicateAt() {
	delta := ballerina.NewDeltaArrayDuplicateAt[string, ballerina.DeltaString](1)

	assertBackAndForthFromJsonYieldsSameValue(s.T(), delta)
}

func (s *DeltaArraySerializationTestSuite) TestAdd() {
	delta := ballerina.NewDeltaArrayAdd[string, ballerina.DeltaString]("new item")

	assertBackAndForthFromJsonYieldsSameValue(s.T(), delta)
}

func TestDeltaArraySerializationTestSuite(t *testing.T) {
	suite.Run(t, new(DeltaArraySerializationTestSuite))
}
