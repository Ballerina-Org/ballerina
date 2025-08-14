package ballerina_test

import (
	"testing"

	ballerina "ballerina.com/core"
	"github.com/stretchr/testify/suite"
)

type FilterSmallerThanSerializationTestSuite struct {
	suite.Suite
}

func (s *FilterSmallerThanSerializationTestSuite) TestString() {
	filter := ballerina.NewSmallerThan("value")

	assertBackAndForthFromJsonYieldsSameValue(s.T(), filter)
}

func (s *FilterSmallerThanSerializationTestSuite) TestInt() {
	filter := ballerina.NewSmallerThan(123)

	assertBackAndForthFromJsonYieldsSameValue(s.T(), filter)
}

func TestFilterSmallerThanSerializationTestSuite(t *testing.T) {
	suite.Run(t, new(FilterSmallerThanSerializationTestSuite))
}
