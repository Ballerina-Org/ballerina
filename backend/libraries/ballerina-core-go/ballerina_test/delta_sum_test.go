package ballerina_test

import (
	"testing"

	ballerina "ballerina.com/core"
	"github.com/stretchr/testify/suite"
)

type DeltaSumSerializationTestSuite struct {
	suite.Suite
}

func (s *DeltaSumSerializationTestSuite) TestReplace() {
	delta := ballerina.NewDeltaSumReplace[string, int, ballerina.DeltaString, ballerina.DeltaInt32](
		ballerina.Left[string, int]("hello"),
	)

	assertBackAndForthFromJsonYieldsSameValue(s.T(), delta)
}

func (s *DeltaSumSerializationTestSuite) TestLeft() {
	delta := ballerina.NewDeltaSumLeft[string, int, ballerina.DeltaString, ballerina.DeltaInt32](
		ballerina.NewDeltaStringReplace("hello"),
	)

	assertBackAndForthFromJsonYieldsSameValue(s.T(), delta)
}

func (s *DeltaSumSerializationTestSuite) TestRight() {
	delta := ballerina.NewDeltaSumRight[string, int, ballerina.DeltaString, ballerina.DeltaInt32](
		ballerina.NewDeltaInt32Replace(42),
	)

	assertBackAndForthFromJsonYieldsSameValue(s.T(), delta)
}

func TestDeltaSumSerializationTestSuite(t *testing.T) {
	suite.Run(t, new(DeltaSumSerializationTestSuite))
}
