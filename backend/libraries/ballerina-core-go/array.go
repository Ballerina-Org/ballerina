package ballerina

type Array[T any] []T

func MapArray[T, U any](ts []T, f func(T) U) []U {
	us := make([]U, len(ts))
	for i := range ts {
		us[i] = f(ts[i])
	}
	return us
}
func DefaultArray[T any]() Array[T] {
	var res Array[T] = make([]T, 0)
	return res
}

type DeltaArrayEffectsEnum string

const (
	ArrayReplace     DeltaArrayEffectsEnum = "ArrayReplace"
	ArrayValue       DeltaArrayEffectsEnum = "ArrayValue"
	ArrayAddAt       DeltaArrayEffectsEnum = "ArrayAddAt"
	ArrayRemoveAt    DeltaArrayEffectsEnum = "ArrayRemoveAt"
	ArrayMoveFromTo  DeltaArrayEffectsEnum = "ArrayMoveFromTo"
	ArrayDuplicateAt DeltaArrayEffectsEnum = "ArrayDuplicateAt"
	ArrayAdd         DeltaArrayEffectsEnum = "ArrayAdd"
)

var AllDeltaArrayEffectsEnumCases = [...]DeltaArrayEffectsEnum{ArrayReplace, ArrayValue, ArrayAddAt, ArrayRemoveAt, ArrayMoveFromTo, ArrayDuplicateAt, ArrayAdd}

func DefaultDeltaArrayEffectsEnum() DeltaArrayEffectsEnum { return AllDeltaArrayEffectsEnumCases[0] }

type DeltaArray[a any, deltaA any] struct {
	DeltaBase
	Discriminator DeltaArrayEffectsEnum
	Replace       Array[a]
	Value         Tuple2[int, deltaA]
	AddAt         Tuple2[int, a]
	RemoveAt      int
	MoveFromTo    Tuple2[int, int]
	DuplicateAt   int
	Add           a
}

func NewDeltaArrayReplace[a any, deltaA any](value Array[a]) DeltaArray[a, deltaA] {
	return DeltaArray[a, deltaA]{
		Discriminator: ArrayReplace,
		Replace:       value,
	}
}
func NewDeltaArrayValue[a any, deltaA any](index int, delta deltaA) DeltaArray[a, deltaA] {
	return DeltaArray[a, deltaA]{
		Discriminator: ArrayValue,
		Value:         NewTuple2(index, delta),
	}
}
func NewDeltaArrayAddAt[a any, deltaA any](index int, newElement a) DeltaArray[a, deltaA] {
	return DeltaArray[a, deltaA]{
		Discriminator: ArrayAddAt,
		AddAt:         NewTuple2(index, newElement),
	}
}
func NewDeltaArrayRemoveAt[a any, deltaA any](index int) DeltaArray[a, deltaA] {
	return DeltaArray[a, deltaA]{
		Discriminator: ArrayRemoveAt,
		RemoveAt:      index,
	}
}
func NewDeltaArrayMoveFromTo[a any, deltaA any](from int, to int) DeltaArray[a, deltaA] {
	return DeltaArray[a, deltaA]{
		Discriminator: ArrayRemoveAt,
		MoveFromTo:    NewTuple2(from, to),
	}
}
func NewDeltaArrayDuplicateAt[a any, deltaA any](index int) DeltaArray[a, deltaA] {
	return DeltaArray[a, deltaA]{
		Discriminator: ArrayDuplicateAt,
		DuplicateAt:   index,
	}
}
func NewDeltaArrayAdd[a any, deltaA any](newElement a) DeltaArray[a, deltaA] {
	return DeltaArray[a, deltaA]{
		Discriminator: ArrayAdd,
		Add:           newElement,
	}
}

func MatchDeltaArray[a any, deltaA any, Result any](
	onReplace func(Array[a]) (Result, error),
	onValue func(Tuple2[int, deltaA]) (Result, error),
	onAddAt func(Tuple2[int, a]) (Result, error),
	onRemoveAt func(int) (Result, error),
	onMoveFromTo func(Tuple2[int, int]) (Result, error),
	onDuplicateAt func(int) (Result, error),
	onAdd func(a) (Result, error),
) func(DeltaArray[a, deltaA]) (Result, error) {
	return func(delta DeltaArray[a, deltaA]) (Result, error) {
		var result Result
		switch delta.Discriminator {
		case "ArrayReplace":
			return onReplace(delta.Replace)
		case "ArrayValue":
			return onValue(delta.Value)
		case "ArrayAddAt":
			return onAddAt(delta.AddAt)
		case "ArrayRemoveAt":
			return onRemoveAt(delta.RemoveAt)
		case "ArrayMoveFromTo":
			return onMoveFromTo(delta.MoveFromTo)
		case "ArrayDuplicateAt":
			return onDuplicateAt(delta.DuplicateAt)
		case "ArrayAdd":
			return onAdd(delta.Add)
		}
		return result, NewInvalidDiscriminatorError(string(delta.Discriminator), "DeltaArray")
	}
}
