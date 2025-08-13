package ballerina_test

import (
	"testing"
	"time"

	ballerina "ballerina.com/core"
	"github.com/google/uuid"
	"github.com/stretchr/testify/suite"
)

type DeltaPrimitiveSerializationTestSuite struct {
	suite.Suite
}

func (s *DeltaPrimitiveSerializationTestSuite) TestInt() {
	delta := ballerina.NewDeltaIntReplace(42)

	assertBackAndForthFromJsonYieldsSameValue(s.T(), delta)
}

func (s *DeltaPrimitiveSerializationTestSuite) TestInt64() {
	delta := ballerina.NewDeltaInt64Replace(1234567890123456789)

	assertBackAndForthFromJsonYieldsSameValue(s.T(), delta)
}

func (s *DeltaPrimitiveSerializationTestSuite) TestString() {
	delta := ballerina.NewDeltaStringReplace("hello world")

	assertBackAndForthFromJsonYieldsSameValue(s.T(), delta)
}

func (s *DeltaPrimitiveSerializationTestSuite) TestBool() {
	delta := ballerina.NewDeltaBoolReplace(true)

	assertBackAndForthFromJsonYieldsSameValue(s.T(), delta)
}

func (s *DeltaPrimitiveSerializationTestSuite) TestGuid() {
	id := uuid.New()
	delta := ballerina.NewDeltaGuidReplace(id)

	assertBackAndForthFromJsonYieldsSameValue(s.T(), delta)
}

func (s *DeltaPrimitiveSerializationTestSuite) TestTime() {
	fixedTime := time.Date(2023, 12, 25, 10, 30, 0, 0, time.UTC)
	delta := ballerina.NewDeltaTimeReplace(fixedTime)

	assertBackAndForthFromJsonYieldsSameValue(s.T(), delta)
}

func (s *DeltaPrimitiveSerializationTestSuite) TestInt32() {
	delta := ballerina.NewDeltaInt32Replace(12345)

	assertBackAndForthFromJsonYieldsSameValue(s.T(), delta)
}

func (s *DeltaPrimitiveSerializationTestSuite) TestFloat32() {
	delta := ballerina.NewDeltaFloat32Replace(3.14159)

	assertBackAndForthFromJsonYieldsSameValue(s.T(), delta)
}

func (s *DeltaPrimitiveSerializationTestSuite) TestFloat64() {
	delta := ballerina.NewDeltaFloat64Replace(2.718281828459045)

	assertBackAndForthFromJsonYieldsSameValue(s.T(), delta)
}

func TestDeltaPrimitiveSerializationTestSuite(t *testing.T) {
	suite.Run(t, new(DeltaPrimitiveSerializationTestSuite))
}
