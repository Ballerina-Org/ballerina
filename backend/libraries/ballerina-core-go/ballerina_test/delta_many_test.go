package ballerina_test

import (
	"testing"

	ballerina "ballerina.com/core"
	"github.com/stretchr/testify/suite"
)

type DeltaManySerializationTestSuite struct {
	suite.Suite
}

func (s *DeltaManySerializationTestSuite) TestLinkedItems() {
	delta := ballerina.NewDeltaManyLinkedItems(
		ballerina.NewDeltaChunkAdd[string, ballerina.DeltaString]("linked item"),
	)

	assertBackAndForthFromJsonYieldsSameValue(s.T(), delta)
}

func (s *DeltaManySerializationTestSuite) TestUnlinkedItems() {
	delta := ballerina.NewDeltaManyUnlinkedItems(
		ballerina.NewDeltaChunkAdd[string, ballerina.DeltaString]("unlinked item"),
	)

	assertBackAndForthFromJsonYieldsSameValue(s.T(), delta)
}

func (s *DeltaManySerializationTestSuite) TestAllItems() {
	delta := ballerina.NewDeltaManyAllItems(
		ballerina.NewDeltaChunkAdd[ballerina.ManyItem[string], ballerina.DeltaManyItem[string, ballerina.DeltaString]](
			ballerina.NewManyItem("all item", true, true),
		),
	)

	assertBackAndForthFromJsonYieldsSameValue(s.T(), delta)
}

func TestDeltaManySerializationTestSuite(t *testing.T) {
	suite.Run(t, new(DeltaManySerializationTestSuite))
}
