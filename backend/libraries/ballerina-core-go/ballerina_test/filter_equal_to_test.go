package ballerina_test

import (
	"testing"

	ballerina "ballerina.com/core"
	"github.com/stretchr/testify/suite"
)

type FilterEqualsToSerializationTestSuite struct {
	suite.Suite
}

func (s *FilterEqualsToSerializationTestSuite) TestString() {
	filter := ballerina.NewEqualsTo("value")

	assertBackAndForthFromJsonYieldsSameValue(s.T(), filter)
}

func (s *FilterEqualsToSerializationTestSuite) TestInt() {
	filter := ballerina.NewEqualsTo(123)

	assertBackAndForthFromJsonYieldsSameValue(s.T(), filter)
}

func TestFilterEqualsToSerializationTestSuite(t *testing.T) {
	suite.Run(t, new(FilterEqualsToSerializationTestSuite))
}
