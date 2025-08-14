package ballerina_test

import (
	"testing"

	ballerina "ballerina.com/core"
	"github.com/stretchr/testify/suite"
)

type FilterGreaterThanSerializationTestSuite struct {
	suite.Suite
}

func (s *FilterGreaterThanSerializationTestSuite) TestString() {
	filter := ballerina.NewGreaterThan("value")

	assertBackAndForthFromJsonYieldsSameValue(s.T(), filter)
}

func (s *FilterGreaterThanSerializationTestSuite) TestInt() {
	filter := ballerina.NewGreaterThan(123)

	assertBackAndForthFromJsonYieldsSameValue(s.T(), filter)
}

func TestFilterGreaterThanSerializationTestSuite(t *testing.T) {
	suite.Run(t, new(FilterGreaterThanSerializationTestSuite))
}
