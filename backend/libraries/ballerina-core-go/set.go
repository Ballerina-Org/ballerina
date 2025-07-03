package ballerina

type Set[a comparable] []a

func DefaultSet[a comparable]() Set[a] {
	return make([]a, 0)
}

type DeltaSetEffectsEnum string

const (
	SetValue  DeltaSetEffectsEnum = "SetValue"
	SetAdd    DeltaSetEffectsEnum = "SetAdd"
	SetRemove DeltaSetEffectsEnum = "SetRemove"
)

var AllDeltaSetEffectsEnumCases = [...]DeltaSetEffectsEnum{SetValue, SetAdd, SetRemove}

func DefaultDeltaSetEffectsEnum() DeltaSetEffectsEnum { return AllDeltaSetEffectsEnumCases[0] }

type DeltaSet[a comparable, deltaA any] struct {
	DeltaBase
	Discriminator DeltaSetEffectsEnum
	Value         Tuple2[a, deltaA]
	Add           a
	Remove        a
}

func NewDeltaSetValue[a comparable, deltaA any](index a, delta deltaA) DeltaSet[a, deltaA] {
	return DeltaSet[a, deltaA]{
		Discriminator: SetValue,
		Value:         NewTuple2(index, delta),
	}
}
func NewDeltaSetAdd[a comparable, deltaA any](newElement a) DeltaSet[a, deltaA] {
	return DeltaSet[a, deltaA]{
		Discriminator: SetAdd,
		Add:           newElement,
	}
}
func NewDeltaSetRemove[a comparable, deltaA any](element a) DeltaSet[a, deltaA] {
	return DeltaSet[a, deltaA]{
		Discriminator: SetRemove,
		Remove:        element,
	}
}

func MatchDeltaSet[a comparable, deltaA any, Result any](
	onValue func(Tuple2[a, deltaA]) (Result, error),
	onAdd func(a) (Result, error),
	onRemove func(a) (Result, error),
) func(DeltaSet[a, deltaA]) (Result, error) {
	return func(delta DeltaSet[a, deltaA]) (Result, error) {
		var result Result
		switch delta.Discriminator {
		case "SetValue":
			return onValue(delta.Value)
		case "SetAdd":
			return onAdd(delta.Add)
		case "SetRemove":
			return onRemove(delta.Remove)
		}
		return result, NewInvalidDiscriminatorError(string(delta.Discriminator), "DeltaSet")
	}
}
