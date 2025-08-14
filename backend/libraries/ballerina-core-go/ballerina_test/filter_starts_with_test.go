package ballerina_test

import (
	"testing"

	ballerina "ballerina.com/core"
	"github.com/stretchr/testify/suite"
)

type FilterStartsWithSerializationTestSuite struct {
	suite.Suite
}

func (s *FilterStartsWithSerializationTestSuite) TestValue() {
	filter := ballerina.NewStartsWith("value")

	assertBackAndForthFromJsonYieldsSameValue(s.T(), filter)
}

func TestFilterStartsWithSerializationTestSuite(t *testing.T) {
	suite.Run(t, new(FilterStartsWithSerializationTestSuite))
}
