package ballerina_test

import (
	"testing"

	ballerina "ballerina.com/core"
	"github.com/stretchr/testify/suite"
)

type FilterContainsSerializationTestSuite struct {
	suite.Suite
}

func (s *FilterContainsSerializationTestSuite) TestValue() {
	filter := ballerina.NewContains("value")

	assertBackAndForthFromJsonYieldsSameValue(s.T(), filter)
}

func TestFilterContainsSerializationTestSuite(t *testing.T) {
	suite.Run(t, new(FilterContainsSerializationTestSuite))
}
