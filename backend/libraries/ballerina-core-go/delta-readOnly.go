package ballerina

type DeltaReadOnly struct {
}

func MatchDeltaReadOnly[Result any]() func(DeltaReadOnly) (Result, error) {
	return func(delta DeltaReadOnly) (Result, error) {
		var result Result
		return result, NewReadOnlyDeltaCalledError()
	}
}
