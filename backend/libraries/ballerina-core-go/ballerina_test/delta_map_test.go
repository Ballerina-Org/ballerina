package ballerina_test

import (
	"fmt"
	"testing"

	ballerina "ballerina.com/core"
	"github.com/stretchr/testify/suite"
)

type DeltaMapSerializationTestSuite struct {
	suite.Suite
}

func (s *DeltaMapSerializationTestSuite) TestKey() {
	delta := ballerina.NewDeltaMapKey[string, int, ballerina.DeltaString, ballerina.DeltaInt32]("0", ballerina.NewDeltaStringReplace("new key"))

	assertBackAndForthFromJsonYieldsSameValue(s.T(), delta)
}

func (s *DeltaMapSerializationTestSuite) TestValue() {
	delta := ballerina.NewDeltaMapValue[string, int, ballerina.DeltaString, ballerina.DeltaInt32]("1", ballerina.NewDeltaInt32Replace(42))

	assertBackAndForthFromJsonYieldsSameValue(s.T(), delta)
}

func (s *DeltaMapSerializationTestSuite) TestAdd() {
	newElement := ballerina.NewTuple2("key", 100)
	delta := ballerina.NewDeltaMapAdd[string, int, ballerina.DeltaString, ballerina.DeltaInt32](newElement)

	assertBackAndForthFromJsonYieldsSameValue(s.T(), delta)
}

func (s *DeltaMapSerializationTestSuite) TestRemove() {
	delta := ballerina.NewDeltaMapRemove[string, int, ballerina.DeltaString, ballerina.DeltaInt32]("2")

	assertBackAndForthFromJsonYieldsSameValue(s.T(), delta)
}

func TestDeltaMapSerializationTestSuite(t *testing.T) {
	suite.Run(t, new(DeltaMapSerializationTestSuite))
}

type deltaMapMatchTestSuite struct {
	suite.Suite
}

func (s *deltaMapMatchTestSuite) TestKeyDeltaShouldReturnKeyIfPresentInCurrentValue() {
	delta := ballerina.NewDeltaMapKey[int, string, ballerina.DeltaInt32, ballerina.DeltaString](0, ballerina.NewDeltaInt32Replace(1))
	currentValue := ballerina.PureReader[ballerina.Unit, ballerina.Sum[error, ballerina.Map[int, string]]](
		ballerina.Right[error, ballerina.Map[int, string]](
			ballerina.Map[int, string]{
				ballerina.KeyValue[int, string]{Key: 0, Value: "1"},
			},
		),
	)

	_, err := ballerina.MatchDeltaMap[int, string, ballerina.DeltaInt32, ballerina.DeltaString, ballerina.Unit](
		func(key ballerina.Tuple2[int, ballerina.DeltaInt32]) func(ballerina.ReaderWithError[ballerina.Unit, int]) (ballerina.Unit, error) {
			return func(currentValueReader ballerina.ReaderWithError[ballerina.Unit, int]) (ballerina.Unit, error) {
				return ballerina.FoldWithError(
					currentValueReader(ballerina.NewUnit()),
					func(err error) (ballerina.Unit, error) {
						return ballerina.NewUnit(), err
					},
					func(value int) (ballerina.Unit, error) {
						return ballerina.NewUnit(), nil
					},
				)
			}
		},
		func(value ballerina.Tuple2[int, ballerina.DeltaString]) func(ballerina.ReaderWithError[ballerina.Unit, string]) (ballerina.Unit, error) {
			return func(currentValueReader ballerina.ReaderWithError[ballerina.Unit, string]) (ballerina.Unit, error) {
				return ballerina.NewUnit(), fmt.Errorf("replace should not be called")
			}
		},
		func(add ballerina.Tuple2[int, string]) (ballerina.Unit, error) {
			return ballerina.NewUnit(), fmt.Errorf("replace should not be called")
		},
		func(remove int) (ballerina.Unit, error) {
			return ballerina.NewUnit(), fmt.Errorf("replace should not be called")
		},
	)(delta)(currentValue)

	s.Require().NoError(err)
}

func (s *deltaMapMatchTestSuite) TestKeyDeltaShouldReturnErrorIfNotInCurrentValue() {
	delta := ballerina.NewDeltaMapKey[int, string, ballerina.DeltaInt32, ballerina.DeltaString](0, ballerina.NewDeltaInt32Replace(1))
	currentValue := ballerina.PureReader[ballerina.Unit, ballerina.Sum[error, ballerina.Map[int, string]]](
		ballerina.Right[error, ballerina.Map[int, string]](
			ballerina.Map[int, string]{},
		),
	)

	_, err := ballerina.MatchDeltaMap[int, string, ballerina.DeltaInt32, ballerina.DeltaString, ballerina.Unit](
		func(key ballerina.Tuple2[int, ballerina.DeltaInt32]) func(ballerina.ReaderWithError[ballerina.Unit, int]) (ballerina.Unit, error) {
			return func(currentValueReader ballerina.ReaderWithError[ballerina.Unit, int]) (ballerina.Unit, error) {
				return ballerina.FoldWithError(
					currentValueReader(ballerina.NewUnit()),
					func(err error) (ballerina.Unit, error) {
						return ballerina.NewUnit(), err
					},
					func(value int) (ballerina.Unit, error) {
						return ballerina.NewUnit(), nil
					},
				)
			}
		},
		func(value ballerina.Tuple2[int, ballerina.DeltaString]) func(ballerina.ReaderWithError[ballerina.Unit, string]) (ballerina.Unit, error) {
			return func(currentValueReader ballerina.ReaderWithError[ballerina.Unit, string]) (ballerina.Unit, error) {
				return ballerina.NewUnit(), nil
			}
		},
		func(add ballerina.Tuple2[int, string]) (ballerina.Unit, error) {
			return ballerina.NewUnit(), fmt.Errorf("replace should not be called")
		},
		func(remove int) (ballerina.Unit, error) {
			return ballerina.NewUnit(), fmt.Errorf("replace should not be called")
		},
	)(delta)(currentValue)

	s.Require().ErrorContains(err, "key 0 not found in current map value")
}

func (s *deltaMapMatchTestSuite) TestValueDeltaShouldReturnErrorIfNotPresentInCurrentValue() {
	delta := ballerina.NewDeltaMapValue[int, string, ballerina.DeltaInt32, ballerina.DeltaString](0, ballerina.NewDeltaStringReplace("new value"))
	currentValue := ballerina.PureReader[ballerina.Unit, ballerina.Sum[error, ballerina.Map[int, string]]](
		ballerina.Right[error, ballerina.Map[int, string]](
			ballerina.Map[int, string]{},
		),
	)

	_, err := ballerina.MatchDeltaMap[int, string, ballerina.DeltaInt32, ballerina.DeltaString, ballerina.Unit](
		func(key ballerina.Tuple2[int, ballerina.DeltaInt32]) func(ballerina.ReaderWithError[ballerina.Unit, int]) (ballerina.Unit, error) {
			return func(currentValueReader ballerina.ReaderWithError[ballerina.Unit, int]) (ballerina.Unit, error) {
				return ballerina.NewUnit(), fmt.Errorf("replace should not be called")
			}
		},
		func(value ballerina.Tuple2[int, ballerina.DeltaString]) func(ballerina.ReaderWithError[ballerina.Unit, string]) (ballerina.Unit, error) {
			return func(currentValueReader ballerina.ReaderWithError[ballerina.Unit, string]) (ballerina.Unit, error) {
				return ballerina.FoldWithError(
					currentValueReader(ballerina.NewUnit()),
					func(err error) (ballerina.Unit, error) {
						return ballerina.NewUnit(), err
					},
					func(value string) (ballerina.Unit, error) {
						return ballerina.NewUnit(), nil
					},
				)
			}
		},
		func(add ballerina.Tuple2[int, string]) (ballerina.Unit, error) {
			return ballerina.NewUnit(), fmt.Errorf("replace should not be called")
		},
		func(remove int) (ballerina.Unit, error) {
			return ballerina.NewUnit(), fmt.Errorf("replace should not be called")
		},
	)(delta)(currentValue)

	s.Require().ErrorContains(err, "key 0 not found in current map value")
}

func (s *deltaMapMatchTestSuite) TestValueDeltaShouldReturnValueIfPresentInCurrentValue() {
	delta := ballerina.NewDeltaMapValue[int, string, ballerina.DeltaInt32, ballerina.DeltaString](0, ballerina.NewDeltaStringReplace("new value"))
	currentValue := ballerina.PureReader[ballerina.Unit, ballerina.Sum[error, ballerina.Map[int, string]]](
		ballerina.Right[error, ballerina.Map[int, string]](
			ballerina.Map[int, string]{
				ballerina.KeyValue[int, string]{Key: 0, Value: "1"},
			},
		),
	)

	_, err := ballerina.MatchDeltaMap[int, string, ballerina.DeltaInt32, ballerina.DeltaString, ballerina.Unit](
		func(key ballerina.Tuple2[int, ballerina.DeltaInt32]) func(ballerina.ReaderWithError[ballerina.Unit, int]) (ballerina.Unit, error) {
			return func(currentValueReader ballerina.ReaderWithError[ballerina.Unit, int]) (ballerina.Unit, error) {
				return ballerina.NewUnit(), fmt.Errorf("replace should not be called")
			}
		},
		func(value ballerina.Tuple2[int, ballerina.DeltaString]) func(ballerina.ReaderWithError[ballerina.Unit, string]) (ballerina.Unit, error) {
			return func(currentValueReader ballerina.ReaderWithError[ballerina.Unit, string]) (ballerina.Unit, error) {
				return ballerina.FoldWithError(
					currentValueReader(ballerina.NewUnit()),
					func(err error) (ballerina.Unit, error) {
						return ballerina.NewUnit(), err
					},
					func(value string) (ballerina.Unit, error) {
						return ballerina.NewUnit(), nil
					},
				)
			}
		},
		func(add ballerina.Tuple2[int, string]) (ballerina.Unit, error) {
			return ballerina.NewUnit(), fmt.Errorf("replace should not be called")
		},
		func(remove int) (ballerina.Unit, error) {
			return ballerina.NewUnit(), fmt.Errorf("replace should not be called")
		},
	)(delta)(currentValue)

	s.Require().NoError(err)
}

func TestMatchDeltaMapTestSuite(t *testing.T) {
	suite.Run(t, new(deltaMapMatchTestSuite))
}
