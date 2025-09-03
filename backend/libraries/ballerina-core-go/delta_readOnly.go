package ballerina

type DeltaReadOnly struct {
	DeltaBase
}

func MatchDeltaReadOnly[Value, Result any]() func(DeltaReadOnly) func(ReaderWithError[Unit, ReadOnly[Value]]) (Result, error) {
	return func(delta DeltaReadOnly) func(ReaderWithError[Unit, ReadOnly[Value]]) (Result, error) {
		return func(readOnly ReaderWithError[Unit, ReadOnly[Value]]) (Result, error) {
			var result Result
			return result, NewReadOnlyDeltaCalledError()
		}
	}
}
