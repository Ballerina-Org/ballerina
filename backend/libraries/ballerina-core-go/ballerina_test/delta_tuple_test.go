package ballerina_test

import (
	"testing"

	ballerina "ballerina.com/core"
	"github.com/stretchr/testify/suite"
)

type DeltaTupleSerializationTestSuite struct {
	suite.Suite
}

func (s *DeltaTupleSerializationTestSuite) TestTuple2Item1() {
	delta := ballerina.NewDeltaTuple2Item1[ballerina.DeltaString, ballerina.DeltaInt32](ballerina.NewDeltaStringReplace("first item"))

	assertBackAndForthFromJsonYieldsSameValue(s.T(), delta)
}

func (s *DeltaTupleSerializationTestSuite) TestTuple2Item2() {
	delta := ballerina.NewDeltaTuple2Item2[ballerina.DeltaString, ballerina.DeltaInt32](ballerina.NewDeltaInt32Replace(42))

	assertBackAndForthFromJsonYieldsSameValue(s.T(), delta)
}

func (s *DeltaTupleSerializationTestSuite) TestTuple3Item1() {
	delta := ballerina.NewDeltaTuple3Item1[ballerina.DeltaString, ballerina.DeltaInt32, ballerina.DeltaBool](ballerina.NewDeltaStringReplace("first item"))

	assertBackAndForthFromJsonYieldsSameValue(s.T(), delta)
}

func (s *DeltaTupleSerializationTestSuite) TestTuple3Item2() {
	delta := ballerina.NewDeltaTuple3Item2[ballerina.DeltaString, ballerina.DeltaInt32, ballerina.DeltaBool](ballerina.NewDeltaInt32Replace(42))

	assertBackAndForthFromJsonYieldsSameValue(s.T(), delta)
}

func (s *DeltaTupleSerializationTestSuite) TestTuple3Item3() {
	delta := ballerina.NewDeltaTuple3Item3[ballerina.DeltaString, ballerina.DeltaInt32, ballerina.DeltaBool](ballerina.NewDeltaBoolReplace(true))

	assertBackAndForthFromJsonYieldsSameValue(s.T(), delta)
}

func (s *DeltaTupleSerializationTestSuite) TestTuple4Item1() {
	delta := ballerina.NewDeltaTuple4Item1[ballerina.DeltaString, ballerina.DeltaInt32, ballerina.DeltaBool, ballerina.DeltaFloat32](ballerina.NewDeltaStringReplace("first item"))

	assertBackAndForthFromJsonYieldsSameValue(s.T(), delta)
}

func (s *DeltaTupleSerializationTestSuite) TestTuple4Item2() {
	delta := ballerina.NewDeltaTuple4Item2[ballerina.DeltaString, ballerina.DeltaInt32, ballerina.DeltaBool, ballerina.DeltaFloat32](ballerina.NewDeltaInt32Replace(42))

	assertBackAndForthFromJsonYieldsSameValue(s.T(), delta)
}

func (s *DeltaTupleSerializationTestSuite) TestTuple4Item3() {
	delta := ballerina.NewDeltaTuple4Item3[ballerina.DeltaString, ballerina.DeltaInt32, ballerina.DeltaBool, ballerina.DeltaFloat32](ballerina.NewDeltaBoolReplace(true))

	assertBackAndForthFromJsonYieldsSameValue(s.T(), delta)
}

func (s *DeltaTupleSerializationTestSuite) TestTuple4Item4() {
	delta := ballerina.NewDeltaTuple4Item4[ballerina.DeltaString, ballerina.DeltaInt32, ballerina.DeltaBool, ballerina.DeltaFloat32](ballerina.NewDeltaFloat32Replace(3.14159))

	assertBackAndForthFromJsonYieldsSameValue(s.T(), delta)
}

func TestDeltaTupleSerializationTestSuite(t *testing.T) {
	suite.Run(t, new(DeltaTupleSerializationTestSuite))
}
