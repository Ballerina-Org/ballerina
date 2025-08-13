package ballerina_test

import (
	"testing"

	ballerina "ballerina.com/core"
	"github.com/stretchr/testify/suite"
)

type DeltaReadOnlySerializationTestSuite struct {
	suite.Suite
}

func (s *DeltaReadOnlySerializationTestSuite) TestReadOnly() {
	delta := ballerina.DeltaReadOnly{}

	assertBackAndForthFromJsonYieldsSameValue(s.T(), delta)
}

func TestDeltaReadOnlySerializationTestSuite(t *testing.T) {
	suite.Run(t, new(DeltaReadOnlySerializationTestSuite))
}
