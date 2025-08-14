package ballerina_test

import (
	"testing"

	ballerina "ballerina.com/core"
	"github.com/stretchr/testify/suite"
)

type FilterGreaterThanOrEqualsToSerializationTestSuite struct {
	suite.Suite
}

func (s *FilterGreaterThanOrEqualsToSerializationTestSuite) TestString() {
	filter := ballerina.NewGreaterThanOrEqualsTo("value")

	assertBackAndForthFromJsonYieldsSameValue(s.T(), filter)
}

func (s *FilterGreaterThanOrEqualsToSerializationTestSuite) TestInt() {
	filter := ballerina.NewGreaterThanOrEqualsTo(123)

	assertBackAndForthFromJsonYieldsSameValue(s.T(), filter)
}

func TestFilterGreaterThanOrEqualsToSerializationTestSuite(t *testing.T) {
	suite.Run(t, new(FilterGreaterThanOrEqualsToSerializationTestSuite))
}
