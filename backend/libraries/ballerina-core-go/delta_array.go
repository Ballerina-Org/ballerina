package ballerina

import (
	"bytes"
	"encoding/json"
)

type deltaArrayEffectsEnum string

const (
	arrayValue       deltaArrayEffectsEnum = "ArrayValue"
	arrayAddAt       deltaArrayEffectsEnum = "ArrayAddAt"
	arrayRemoveAt    deltaArrayEffectsEnum = "ArrayRemoveAt"
	arrayMoveFromTo  deltaArrayEffectsEnum = "ArrayMoveFromTo"
	arrayDuplicateAt deltaArrayEffectsEnum = "ArrayDuplicateAt"
	arrayAdd         deltaArrayEffectsEnum = "ArrayAdd"
	arrayReplace     deltaArrayEffectsEnum = "ArrayReplace"
)

var allDeltaArrayEffectsEnumCases = [...]deltaArrayEffectsEnum{arrayValue, arrayAddAt, arrayRemoveAt, arrayMoveFromTo, arrayDuplicateAt, arrayAdd, arrayReplace}

func DefaultDeltaArrayEffectsEnum() deltaArrayEffectsEnum { return allDeltaArrayEffectsEnumCases[0] }

type DeltaArray[a any, deltaA any] struct {
	DeltaBase
	discriminator deltaArrayEffectsEnum
	value         *Tuple2[int, deltaA]
	addAt         *Tuple2[int, a]
	removeAt      *int
	moveFromTo    *Tuple2[int, int]
	duplicateAt   *int
	add           *a
	replace       *[]a
}

var _ json.Unmarshaler = &DeltaArray[Unit, Unit]{}
var _ json.Marshaler = DeltaArray[Unit, Unit]{}

func (d DeltaArray[a, deltaA]) MarshalJSON() ([]byte, error) {
	return json.Marshal(struct {
		DeltaBase
		Discriminator deltaArrayEffectsEnum
		Value         *Tuple2[int, deltaA]
		AddAt         *Tuple2[int, a]
		RemoveAt      *int
		MoveFromTo    *Tuple2[int, int]
		DuplicateAt   *int
		Add           *a
		Replace       *[]a
	}{
		DeltaBase:     d.DeltaBase,
		Discriminator: d.discriminator,
		Value:         d.value,
		AddAt:         d.addAt,
		RemoveAt:      d.removeAt,
		MoveFromTo:    d.moveFromTo,
		DuplicateAt:   d.duplicateAt,
		Add:           d.add,
		Replace:       d.replace,
	})
}

func (d *DeltaArray[a, deltaA]) UnmarshalJSON(data []byte) error {
	var aux struct {
		DeltaBase
		Discriminator deltaArrayEffectsEnum
		Value         *Tuple2[int, deltaA]
		AddAt         *Tuple2[int, a]
		RemoveAt      *int
		MoveFromTo    *Tuple2[int, int]
		DuplicateAt   *int
		Add           *a
		Replace       *[]a
	}
	dec := json.NewDecoder(bytes.NewReader(data))
	dec.DisallowUnknownFields()
	if err := dec.Decode(&aux); err != nil {
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
	d.replace = aux.Replace
	return nil
}

func NewDeltaArrayReplace[a any, deltaA any](value []a) DeltaArray[a, deltaA] {
	return DeltaArray[a, deltaA]{
		discriminator: arrayReplace,
		replace:       &value,
	}
}
func NewDeltaArrayValue[a any, deltaA any](index int, delta deltaA) DeltaArray[a, deltaA] {
	tmp := NewTuple2(index, delta)
	return DeltaArray[a, deltaA]{
		discriminator: arrayValue,
		value:         &tmp,
	}
}
func NewDeltaArrayAddAt[a any, deltaA any](index int, newElement a) DeltaArray[a, deltaA] {
	tmp := NewTuple2(index, newElement)
	return DeltaArray[a, deltaA]{
		discriminator: arrayAddAt,
		addAt:         &tmp,
	}
}
func NewDeltaArrayRemoveAt[a any, deltaA any](index int) DeltaArray[a, deltaA] {
	return DeltaArray[a, deltaA]{
		discriminator: arrayRemoveAt,
		removeAt:      &index,
	}
}
func NewDeltaArrayMoveFromTo[a any, deltaA any](from int, to int) DeltaArray[a, deltaA] {
	tmp := NewTuple2(from, to)
	return DeltaArray[a, deltaA]{
		discriminator: arrayMoveFromTo,
		moveFromTo:    &tmp,
	}
}
func NewDeltaArrayDuplicateAt[a any, deltaA any](index int) DeltaArray[a, deltaA] {
	return DeltaArray[a, deltaA]{
		discriminator: arrayDuplicateAt,
		duplicateAt:   &index,
	}
}
func NewDeltaArrayAdd[a any, deltaA any](newElement a) DeltaArray[a, deltaA] {
	return DeltaArray[a, deltaA]{
		discriminator: arrayAdd,
		add:           &newElement,
	}
}

func MatchDeltaArray[a any, deltaA any, Result any](
	onValue func(Tuple2[int, deltaA]) func(ReaderWithError[Unit, a]) (Result, error),
	onAddAt func(Tuple2[int, a]) (Result, error),
	onRemoveAt func(int) (Result, error),
	onMoveFromTo func(Tuple2[int, int]) (Result, error),
	onDuplicateAt func(int) (Result, error),
	onAdd func(a) (Result, error),
	onReplace func([]a) (Result, error),
) func(DeltaArray[a, deltaA]) func(ReaderWithError[Unit, Array[a]]) (Result, error) {
	return func(delta DeltaArray[a, deltaA]) func(ReaderWithError[Unit, Array[a]]) (Result, error) {
		return func(value ReaderWithError[Unit, Array[a]]) (Result, error) {
			var result Result
			switch delta.discriminator {
			case arrayValue:
				arrayElement := MapReaderWithError[Unit](
					func(value Array[a]) a {
						return value[delta.value.Item1]
					},
				)(value)
				return onValue(*delta.value)(arrayElement)
			case arrayAddAt:
				return onAddAt(*delta.addAt)
			case arrayRemoveAt:
				return onRemoveAt(*delta.removeAt)
			case arrayMoveFromTo:
				return onMoveFromTo(*delta.moveFromTo)
			case arrayDuplicateAt:
				return onDuplicateAt(*delta.duplicateAt)
			case arrayAdd:
				return onAdd(*delta.add)
			case arrayReplace:
				return onReplace(*delta.replace)
			}
			return result, NewInvalidDiscriminatorError(string(delta.discriminator), "DeltaArray")
		}
	}
}
