package ballerina

type KeyValue[k comparable, v any] struct {
	Key   k
	Value v
}

type Map[k comparable, v any] []KeyValue[k, v]

func DefaultMap[k comparable, v any]() Map[k, v] {
	return make([]KeyValue[k, v], 0)
}

type DeltaMapEffectsEnum string

const (
	MapKey    DeltaMapEffectsEnum = "MapKey"
	MapValue  DeltaMapEffectsEnum = "MapValue"
	MapAdd    DeltaMapEffectsEnum = "MapAdd"
	MapRemove DeltaMapEffectsEnum = "MapRemove"
)

var AllDeltaMapEffectsEnumCases = [...]DeltaMapEffectsEnum{MapKey, MapValue, MapAdd, MapRemove}

func DefaultDeltaMapEffectsEnum() DeltaMapEffectsEnum { return AllDeltaMapEffectsEnumCases[0] }

type DeltaMap[k comparable, v any, deltaK any, deltaV any] struct {
	DeltaBase
	Discriminator DeltaMapEffectsEnum
	Key           Tuple2[int, deltaK]
	Value         Tuple2[int, deltaV]
	Add           Tuple2[k, v]
	Remove        int
}

func NewDeltaMapKey[k comparable, v any, deltaK any, deltaV any](index int, delta deltaK) DeltaMap[k, v, deltaK, deltaV] {
	return DeltaMap[k, v, deltaK, deltaV]{
		Discriminator: MapKey,
		Key:           NewTuple2(index, delta),
	}
}
func NewDeltaMapValue[k comparable, v any, deltaK any, deltaV any](index int, delta deltaV) DeltaMap[k, v, deltaK, deltaV] {
	return DeltaMap[k, v, deltaK, deltaV]{
		Discriminator: MapValue,
		Value:         NewTuple2(index, delta),
	}
}
func NewDeltaMapAdd[k comparable, v any, deltaK any, deltaV any](newElement Tuple2[k, v]) DeltaMap[k, v, deltaK, deltaV] {
	return DeltaMap[k, v, deltaK, deltaV]{
		Discriminator: MapAdd,
		Add:           newElement,
	}
}
func NewDeltaMapRemove[k comparable, v any, deltaK any, deltaV any](index int) DeltaMap[k, v, deltaK, deltaV] {
	return DeltaMap[k, v, deltaK, deltaV]{
		Discriminator: MapRemove,
		Remove:        index,
	}
}

func MatchDeltaMap[k comparable, v any, deltaK any, deltaV any, Result any](
	onKey func(Tuple2[int, deltaK]) (Result, error),
	onValue func(Tuple2[int, deltaV]) (Result, error),
	onAdd func(Tuple2[k, v]) (Result, error),
	onRemove func(int) (Result, error),
) func(DeltaMap[k, v, deltaK, deltaV]) (Result, error) {
	return func(delta DeltaMap[k, v, deltaK, deltaV]) (Result, error) {
		var result Result
		switch delta.Discriminator {
		case "MapKey":
			return onKey(delta.Key)
		case "MapValue":
			return onValue(delta.Value)
		case "MapAdd":
			return onAdd(delta.Add)
		case "MapRemove":
			return onRemove(delta.Remove)
		}
		return result, NewInvalidDiscriminatorError(string(delta.Discriminator), "DeltaMap")
	}
}
