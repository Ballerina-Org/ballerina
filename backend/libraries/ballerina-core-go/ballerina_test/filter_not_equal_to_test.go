package ballerina_test

import (
	"testing"

	ballerina "ballerina.com/core"
	"github.com/stretchr/testify/suite"
)

type FilterNotEqualsToSerializationTestSuite struct {
	suite.Suite
}

func (s *FilterNotEqualsToSerializationTestSuite) TestString() {
	filter := ballerina.NewNotEqualsTo("value")

	assertBackAndForthFromJsonYieldsSameValue(s.T(), filter)
}

func (s *FilterNotEqualsToSerializationTestSuite) TestInt() {
	filter := ballerina.NewNotEqualsTo(123)

	assertBackAndForthFromJsonYieldsSameValue(s.T(), filter)
}

func TestFilterNotEqualsToSerializationTestSuite(t *testing.T) {
	suite.Run(t, new(FilterNotEqualsToSerializationTestSuite))
}
