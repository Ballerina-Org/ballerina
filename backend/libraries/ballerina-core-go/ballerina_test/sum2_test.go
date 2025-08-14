package ballerina_test

import (
	"testing"

	ballerina "ballerina.com/core"
	"github.com/stretchr/testify/suite"
)

type Sum2TestSuite struct {
	suite.Suite
}

func (s *Sum2TestSuite) TestSum2Case1() {
	value := ballerina.Case1Of2[int, string](10)

	assertBackAndForthFromJsonYieldsSameValue(s.T(), value)
}

func (s *Sum2TestSuite) TestSum2Case2() {
	value := ballerina.Case2Of2[int, string]("abc")

	assertBackAndForthFromJsonYieldsSameValue(s.T(), value)
}

func TestSum2SerializationTestSuite(t *testing.T) {
	suite.Run(t, new(Sum2TestSuite))
}
