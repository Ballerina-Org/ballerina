package ballerina_test

import (
	"testing"

	ballerina "ballerina.com/core"
	"github.com/google/uuid"
	"github.com/stretchr/testify/suite"
)

type DeltaTableSerializationTestSuite struct {
	suite.Suite
}

func (s *DeltaTableSerializationTestSuite) TestValue() {
	id := uuid.New()
	delta := ballerina.NewDeltaTableValue[string, ballerina.DeltaString](id, ballerina.NewDeltaStringReplace("table value"))

	assertBackAndForthFromJsonYieldsSameValue(s.T(), delta)
}

func (s *DeltaTableSerializationTestSuite) TestAddAt() {
	id := uuid.New()
	delta := ballerina.NewDeltaTableAddAt[string, ballerina.DeltaString](id, "new table item")

	assertBackAndForthFromJsonYieldsSameValue(s.T(), delta)
}

func (s *DeltaTableSerializationTestSuite) TestRemoveAt() {
	id := uuid.New()
	delta := ballerina.NewDeltaTableRemoveAt[string, ballerina.DeltaString](id)

	assertBackAndForthFromJsonYieldsSameValue(s.T(), delta)
}

func (s *DeltaTableSerializationTestSuite) TestMoveFromTo() {
	fromID := uuid.New()
	toID := uuid.New()
	delta := ballerina.NewDeltaTableMoveFromTo[string, ballerina.DeltaString](fromID, toID)

	assertBackAndForthFromJsonYieldsSameValue(s.T(), delta)
}

func (s *DeltaTableSerializationTestSuite) TestDuplicateAt() {
	id := uuid.New()
	delta := ballerina.NewDeltaTableDuplicateAt[string, ballerina.DeltaString](id)

	assertBackAndForthFromJsonYieldsSameValue(s.T(), delta)
}

func (s *DeltaTableSerializationTestSuite) TestAdd() {
	delta := ballerina.NewDeltaTableAdd[string, ballerina.DeltaString]("new table element")

	assertBackAndForthFromJsonYieldsSameValue(s.T(), delta)
}

func (s *DeltaTableSerializationTestSuite) TestAddBatch() {
	delta := ballerina.NewDeltaTableAddBatch[string, ballerina.DeltaString]([]string{"new table element 1", "new table element 2"})

	assertBackAndForthFromJsonYieldsSameValue(s.T(), delta)
}

func (s *DeltaTableSerializationTestSuite) TestAddEmpty() {
	delta := ballerina.NewDeltaTableAddEmpty[string, ballerina.DeltaString]()

	assertBackAndForthFromJsonYieldsSameValue(s.T(), delta)
}

func (s *DeltaTableSerializationTestSuite) TestRemoveAll() {
	delta := ballerina.NewDeltaTableRemoveAll[string, ballerina.DeltaString]()

	assertBackAndForthFromJsonYieldsSameValue(s.T(), delta)
}

func (s *DeltaTableSerializationTestSuite) TestAddBatch() {
	delta := ballerina.NewDeltaTableAddBatch[string, ballerina.DeltaString](3)

	assertBackAndForthFromJsonYieldsSameValue(s.T(), delta)
}

func (s *DeltaTableSerializationTestSuite) TestRemoveBatch() {
	id1 := uuid.New()
	id2 := uuid.New()
	delta := ballerina.NewDeltaTableRemoveBatch[string, ballerina.DeltaString]([]uuid.UUID{id1, id2})

	assertBackAndForthFromJsonYieldsSameValue(s.T(), delta)
}

func TestDeltaTableSerializationTestSuite(t *testing.T) {
	suite.Run(t, new(DeltaTableSerializationTestSuite))
}
