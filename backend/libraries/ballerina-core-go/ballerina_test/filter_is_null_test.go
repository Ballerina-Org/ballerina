package ballerina_test

import (
	"testing"

	ballerina "ballerina.com/core"
	"github.com/stretchr/testify/suite"
)

type FilterIsNullSerializationTestSuite struct {
	suite.Suite
}

func (s *FilterIsNullSerializationTestSuite) Test() {
	filter := ballerina.NewIsNull[ballerina.Unit]()

	assertBackAndForthFromJsonYieldsSameValue(s.T(), filter)
}

func TestFilterIsNullSerializationTestSuite(t *testing.T) {
	suite.Run(t, new(FilterIsNullSerializationTestSuite))
}
