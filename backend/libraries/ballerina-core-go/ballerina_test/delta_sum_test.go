package ballerina_test

import (
	"fmt"
	"testing"

	ballerina "ballerina.com/core"
	"github.com/stretchr/testify/assert"
	"github.com/stretchr/testify/require"
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

func TestMatchDeltaSumShouldCotraverseValue(t *testing.T) {
	t.Parallel()
	delta := ballerina.NewDeltaSumRight[string, int32, ballerina.DeltaString, ballerina.DeltaInt32](
		ballerina.NewDeltaInt32Replace(42),
	)
	var currentValue ballerina.ReaderWithError[ballerina.Unit, ballerina.Sum[string, int32]] = func(ballerina.Unit) ballerina.Sum[error, ballerina.Sum[string, int32]] {
		return ballerina.Right[error, ballerina.Sum[string, int32]](ballerina.Right[string, int32](1))
	}

	_, err := ballerina.MatchDeltaSum(
		func(value ballerina.Sum[string, int32]) func(ballerina.ReaderWithError[ballerina.Unit, ballerina.Sum[string, int32]]) (ballerina.Unit, error) {
			return func(reader ballerina.ReaderWithError[ballerina.Unit, ballerina.Sum[string, int32]]) (ballerina.Unit, error) {
				return ballerina.NewUnit(), fmt.Errorf("replace should not be called")
			}
		},
		ballerina.MatchDeltaString(
			func(value string) func(ballerina.ReaderWithError[ballerina.Unit, string]) (ballerina.Unit, error) {
				return func(reader ballerina.ReaderWithError[ballerina.Unit, string]) (ballerina.Unit, error) {
					return ballerina.NewUnit(), nil
				}
			},
		),
		ballerina.MatchDeltaInt32(
			func(value int32) func(ballerina.ReaderWithError[ballerina.Unit, int32]) (ballerina.Unit, error) {
				return func(reader ballerina.ReaderWithError[ballerina.Unit, int32]) (ballerina.Unit, error) {
					assert.Equal(t, int32(42), value)
					return ballerina.NewUnit(), nil
				}
			},
		),
	)(delta)(currentValue)

	require.NoError(t, err)
}
func TestMatchDeltaSumShouldFailIfDeltaDoesNotMatchValue(t *testing.T) {
	t.Parallel()
	delta := ballerina.NewDeltaSumLeft[string, int32, ballerina.DeltaString, ballerina.DeltaInt32](
		ballerina.NewDeltaStringReplace("hello"),
	)
	var currentValue ballerina.ReaderWithError[ballerina.Unit, ballerina.Sum[string, int32]] = func(ballerina.Unit) ballerina.Sum[error, ballerina.Sum[string, int32]] {
		return ballerina.Right[error, ballerina.Sum[string, int32]](ballerina.Right[string, int32](1))
	}

	_, err := ballerina.MatchDeltaSum(
		func(ballerina.Sum[string, int32]) func(ballerina.ReaderWithError[ballerina.Unit, ballerina.Sum[string, int32]]) (ballerina.Unit, error) {
			return func(ballerina.ReaderWithError[ballerina.Unit, ballerina.Sum[string, int32]]) (ballerina.Unit, error) {
				return ballerina.NewUnit(), fmt.Errorf("replace should not be called")
			}
		},
		ballerina.MatchDeltaString(
			func(string) func(ballerina.ReaderWithError[ballerina.Unit, string]) (ballerina.Unit, error) {
				return func(currentValue ballerina.ReaderWithError[ballerina.Unit, string]) (ballerina.Unit, error) {
					return ballerina.FoldWithError(
						currentValue(ballerina.NewUnit()),
						func(err error) (ballerina.Unit, error) {
							return ballerina.NewUnit(), err
						},
						func(string) (ballerina.Unit, error) {
							return ballerina.NewUnit(), nil
						},
					)
				}
			},
		),
		ballerina.MatchDeltaInt32(
			func(int32) func(ballerina.ReaderWithError[ballerina.Unit, int32]) (ballerina.Unit, error) {
				return func(ballerina.ReaderWithError[ballerina.Unit, int32]) (ballerina.Unit, error) {
					return ballerina.NewUnit(), fmt.Errorf("right should not be called")
				}
			},
		),
	)(delta)(currentValue)

	require.Error(t, err)
	require.Equal(t, "sum is right", err.Error())
}
