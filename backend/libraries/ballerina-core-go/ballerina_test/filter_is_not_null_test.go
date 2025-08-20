package ballerina_test

import (
	"testing"

	ballerina "ballerina.com/core"
	"github.com/stretchr/testify/suite"
)

type FilterIsNotNullSerializationTestSuite struct {
	suite.Suite
}

func (s *FilterIsNotNullSerializationTestSuite) Test() {
	filter := ballerina.NewIsNotNull[ballerina.Unit]()

	assertBackAndForthFromJsonYieldsSameValue(s.T(), filter)
}

func TestFilterIsNotNullSerializationTestSuite(t *testing.T) {
	suite.Run(t, new(FilterIsNotNullSerializationTestSuite))
}
