package ballerinalazy

import (
	"encoding/json"
	"fmt"

	ballerina "ballerina.com/core"
)

type Context = ballerina.Unit

type SomeComplexType struct {
	SomeField      int
	SomeOtherField string
}

// Type definition
type Example = ballerina.Sum[
	Lazy[ballerina.Tuple2[
		Lazy[SomeComplexType],
		string,
	]],
	bool,
]

// Corresponding generated lazy
type LazyExample = LazySum[
	LoadOrRecurse[
		ballerina.Tuple2[
			Lazy[SomeComplexType],
			string,
		],
		LazyTuple2[
			LoadOrRecurse[SomeComplexType, NotLazy],
			NotLazy,
		]],
	NotLazy,
]

// Usage

func loadlazy(lazy LazyExample) ballerina.ReaderWithError[Context, []byte] {
	return MatchLazySum(
		MatchLoadOrRecurse(
			toJson[ballerina.Tuple2[Lazy[SomeComplexType], string]],
			loadTheBigTuple,
			MatchLazyTuple2(
				MatchLoadOrRecurse(
					toJson[SomeComplexType],
					loadSomeComplexType,
					MatchNotLazy[Context, []byte](),
				),
				MatchNotLazy[Context, []byte](),
			),
		),
		MatchNotLazy[Context, []byte](),
	)(lazy)
}

// Helpers
func toJson[T any](value T) ballerina.Sum[error, []byte] {
	asJson, err := json.Marshal(value)
	if err != nil {
		return ballerina.Left[error, []byte](err)
	}
	return ballerina.Right[error, []byte](asJson)
}

var loadTheBigTuple ballerina.ReaderWithError[Context, ballerina.Tuple2[
	Lazy[SomeComplexType],
	string,
]] = ballerina.PureReader[Context, ballerina.Sum[error, ballerina.Tuple2[
	Lazy[SomeComplexType],
	string,
]]](ballerina.Left[error, ballerina.Tuple2[
	Lazy[SomeComplexType],
	string,
]](fmt.Errorf("not implemented")))

var loadSomeComplexType ballerina.ReaderWithError[Context, SomeComplexType] = ballerina.PureReader[Context, ballerina.Sum[error, SomeComplexType]](ballerina.Left[error, SomeComplexType](fmt.Errorf("not implemented")))
