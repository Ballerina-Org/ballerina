package ballerina_test

import (
	"testing"

	ballerina "ballerina.com/core"
	"github.com/google/uuid"
	"github.com/stretchr/testify/suite"
)

type DeltaChunkSerializationTestSuite struct {
	suite.Suite
}

func (s *DeltaChunkSerializationTestSuite) TestValue() {
	id := uuid.New()
	delta := ballerina.NewDeltaChunkValue[string, ballerina.DeltaString](id, ballerina.NewDeltaStringReplace("hello"))

	assertBackAndForthFromJsonYieldsSameValue(s.T(), delta)
}

func (s *DeltaChunkSerializationTestSuite) TestAddAt() {
	id := uuid.New()
	delta := ballerina.NewDeltaChunkAddAt[string, ballerina.DeltaString](id, "world")

	assertBackAndForthFromJsonYieldsSameValue(s.T(), delta)
}

func (s *DeltaChunkSerializationTestSuite) TestRemoveAt() {
	id := uuid.New()
	delta := ballerina.NewDeltaChunkRemoveAt[string, ballerina.DeltaString](id)

	assertBackAndForthFromJsonYieldsSameValue(s.T(), delta)
}

func (s *DeltaChunkSerializationTestSuite) TestMoveFromTo() {
	fromID := uuid.New()
	toID := uuid.New()
	delta := ballerina.NewDeltaChunkMoveFromTo[string, ballerina.DeltaString](fromID, toID)

	assertBackAndForthFromJsonYieldsSameValue(s.T(), delta)
}

func (s *DeltaChunkSerializationTestSuite) TestDuplicateAt() {
	id := uuid.New()
	delta := ballerina.NewDeltaChunkDuplicateAt[string, ballerina.DeltaString](id)

	assertBackAndForthFromJsonYieldsSameValue(s.T(), delta)
}

func (s *DeltaChunkSerializationTestSuite) TestAdd() {
	delta := ballerina.NewDeltaChunkAdd[string, ballerina.DeltaString]("new item")

	assertBackAndForthFromJsonYieldsSameValue(s.T(), delta)
}

func TestDeltaChunkSerializationTestSuite(t *testing.T) {
	suite.Run(t, new(DeltaChunkSerializationTestSuite))
}
