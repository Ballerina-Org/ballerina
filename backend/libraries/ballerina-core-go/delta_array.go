package ballerina

import "encoding/json"

type DeltaArrayEffectsEnum string

const (
	ArrayValue       DeltaArrayEffectsEnum = "ArrayValue"
	ArrayAddAt       DeltaArrayEffectsEnum = "ArrayAddAt"
	ArrayRemoveAt    DeltaArrayEffectsEnum = "ArrayRemoveAt"
	ArrayMoveFromTo  DeltaArrayEffectsEnum = "ArrayMoveFromTo"
	ArrayDuplicateAt DeltaArrayEffectsEnum = "ArrayDuplicateAt"
	ArrayAdd         DeltaArrayEffectsEnum = "ArrayAdd"
)

var AllDeltaArrayEffectsEnumCases = [...]DeltaArrayEffectsEnum{ArrayValue, ArrayAddAt, ArrayRemoveAt, ArrayMoveFromTo, ArrayDuplicateAt, ArrayAdd}

func DefaultDeltaArrayEffectsEnum() DeltaArrayEffectsEnum { return AllDeltaArrayEffectsEnumCases[0] }

type DeltaArray[a any, deltaA any] struct {
	DeltaBase
	discriminator DeltaArrayEffectsEnum
	value         *Tuple2[int, deltaA]
	addAt         *Tuple2[int, a]
	removeAt      *int
	moveFromTo    *Tuple2[int, int]
	duplicateAt   *int
	add           *a
}

var _ json.Unmarshaler = &DeltaArray[Unit, Unit]{}
var _ json.Marshaler = DeltaArray[Unit, Unit]{}

func (d DeltaArray[a, deltaA]) MarshalJSON() ([]byte, error) {
	return json.Marshal(struct {
		DeltaBase
		Discriminator DeltaArrayEffectsEnum
		Value         *Tuple2[int, deltaA]
		AddAt         *Tuple2[int, a]
		RemoveAt      *int
		MoveFromTo    *Tuple2[int, int]
		DuplicateAt   *int
		Add           *a
	}{
		DeltaBase:     d.DeltaBase,
		Discriminator: d.discriminator,
		Value:         d.value,
		AddAt:         d.addAt,
		RemoveAt:      d.removeAt,
		MoveFromTo:    d.moveFromTo,
		DuplicateAt:   d.duplicateAt,
		Add:           d.add,
	})
}

func (d *DeltaArray[a, deltaA]) UnmarshalJSON(data []byte) error {
	var aux struct {
		DeltaBase
		Discriminator DeltaArrayEffectsEnum
		Value         *Tuple2[int, deltaA]
		AddAt         *Tuple2[int, a]
		RemoveAt      *int
		MoveFromTo    *Tuple2[int, int]
		DuplicateAt   *int
		Add           *a
	}
	if err := json.Unmarshal(data, &aux); err != nil {
		return err
	}
	d.DeltaBase = aux.DeltaBase
	d.discriminator = aux.Discriminator
	d.value = aux.Value
	d.addAt = aux.AddAt
	d.removeAt = aux.RemoveAt
	d.moveFromTo = aux.MoveFromTo
	d.duplicateAt = aux.DuplicateAt
	d.add = aux.Add
	return nil
}

func NewDeltaArrayValue[a any, deltaA any](index int, delta deltaA) DeltaArray[a, deltaA] {
	tmp := NewTuple2(index, delta)
	return DeltaArray[a, deltaA]{
		discriminator: ArrayValue,
		value:         &tmp,
	}
}
func NewDeltaArrayAddAt[a any, deltaA any](index int, newElement a) DeltaArray[a, deltaA] {
	tmp := NewTuple2(index, newElement)
	return DeltaArray[a, deltaA]{
		discriminator: ArrayAddAt,
		addAt:         &tmp,
	}
}
func NewDeltaArrayRemoveAt[a any, deltaA any](index int) DeltaArray[a, deltaA] {
	return DeltaArray[a, deltaA]{
		discriminator: ArrayRemoveAt,
		removeAt:      &index,
	}
}
func NewDeltaArrayMoveFromTo[a any, deltaA any](from int, to int) DeltaArray[a, deltaA] {
	tmp := NewTuple2(from, to)
	return DeltaArray[a, deltaA]{
		discriminator: ArrayMoveFromTo,
		moveFromTo:    &tmp,
	}
}
func NewDeltaArrayDuplicateAt[a any, deltaA any](index int) DeltaArray[a, deltaA] {
	return DeltaArray[a, deltaA]{
		discriminator: ArrayDuplicateAt,
		duplicateAt:   &index,
	}
}
func NewDeltaArrayAdd[a any, deltaA any](newElement a) DeltaArray[a, deltaA] {
	return DeltaArray[a, deltaA]{
		discriminator: ArrayAdd,
		add:           &newElement,
	}
}

func MatchDeltaArray[a any, deltaA any, Result any](
	onValue func(Tuple2[int, deltaA]) (Result, error),
	onAddAt func(Tuple2[int, a]) (Result, error),
	onRemoveAt func(int) (Result, error),
	onMoveFromTo func(Tuple2[int, int]) (Result, error),
	onDuplicateAt func(int) (Result, error),
	onAdd func(a) (Result, error),
) func(DeltaArray[a, deltaA]) (Result, error) {
	return func(delta DeltaArray[a, deltaA]) (Result, error) {
		var result Result
		switch delta.discriminator {
		case ArrayValue:
			return onValue(*delta.value)
		case ArrayAddAt:
			return onAddAt(*delta.addAt)
		case ArrayRemoveAt:
			return onRemoveAt(*delta.removeAt)
		case ArrayMoveFromTo:
			return onMoveFromTo(*delta.moveFromTo)
		case ArrayDuplicateAt:
			return onDuplicateAt(*delta.duplicateAt)
		case ArrayAdd:
			return onAdd(*delta.add)
		}
		return result, NewInvalidDiscriminatorError(string(delta.discriminator), "DeltaArray")
	}
}
