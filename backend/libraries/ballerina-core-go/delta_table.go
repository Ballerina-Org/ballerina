package ballerina

import (
	"bytes"
	"encoding/json"
)

type deltaTableEffectsEnum string

const (
	tableValue       deltaTableEffectsEnum = "TableValue"
	tableValueAll    deltaTableEffectsEnum = "TableValueAll"
	tableAddAt       deltaTableEffectsEnum = "TableAddAt"
	tableRemoveAt    deltaTableEffectsEnum = "TableRemoveAt"
	tableMoveFromTo  deltaTableEffectsEnum = "TableMoveFromTo"
	tableDuplicateAt deltaTableEffectsEnum = "TableDuplicateAt"
	tableAdd         deltaTableEffectsEnum = "TableAdd"
	tableAddEmpty    deltaTableEffectsEnum = "TableAddEmpty"
)

type DeltaTable[ID any, a any, deltaA any] struct {
	DeltaBase
	discriminator deltaTableEffectsEnum
	value         *Tuple2[ID, deltaA]
	valueAll      *deltaA
	addAt         *Tuple2[ID, a]
	removeAt      *ID
	moveFromTo    *Tuple2[ID, ID]
	duplicateAt   *ID
	add           *a
}

var _ json.Unmarshaler = &DeltaTable[Unit, Unit, Unit]{}
var _ json.Marshaler = DeltaTable[Unit, Unit, Unit]{}

func (d DeltaTable[ID, a, deltaA]) MarshalJSON() ([]byte, error) {
	return json.Marshal(struct {
		DeltaBase
		Discriminator deltaTableEffectsEnum
		Value         *Tuple2[ID, deltaA]
		ValueAll      *deltaA
		AddAt         *Tuple2[ID, a]
		RemoveAt      *ID
		MoveFromTo    *Tuple2[ID, ID]
		DuplicateAt   *ID
		Add           *a
	}{
		DeltaBase:     d.DeltaBase,
		Discriminator: d.discriminator,
		Value:         d.value,
		ValueAll:      d.valueAll,
		AddAt:         d.addAt,
		RemoveAt:      d.removeAt,
		MoveFromTo:    d.moveFromTo,
		DuplicateAt:   d.duplicateAt,
		Add:           d.add,
	})
}

func (d *DeltaTable[ID, a, deltaA]) UnmarshalJSON(data []byte) error {
	var aux struct {
		DeltaBase
		Discriminator deltaTableEffectsEnum
		Value         *Tuple2[ID, deltaA]
		ValueAll      *deltaA
		AddAt         *Tuple2[ID, a]
		RemoveAt      *ID
		MoveFromTo    *Tuple2[ID, ID]
		DuplicateAt   *ID
		Add           *a
	}
	dec := json.NewDecoder(bytes.NewReader(data))
	dec.DisallowUnknownFields()
	if err := dec.Decode(&aux); err != nil {
		return err
	}
	d.DeltaBase = aux.DeltaBase
	d.discriminator = aux.Discriminator
	d.value = aux.Value
	d.valueAll = aux.ValueAll
	d.addAt = aux.AddAt
	d.removeAt = aux.RemoveAt
	d.moveFromTo = aux.MoveFromTo
	d.duplicateAt = aux.DuplicateAt
	d.add = aux.Add
	return nil
}

func NewDeltaTableValue[ID any, a any, deltaA any](index ID, delta deltaA) DeltaTable[ID, a, deltaA] {
	val := NewTuple2(index, delta)
	return DeltaTable[ID, a, deltaA]{
		discriminator: tableValue,
		value:         &val,
	}
}
func NewDeltaTableValueAll[ID any, a any, deltaA any](delta deltaA) DeltaTable[ID, a, deltaA] {
	return DeltaTable[ID, a, deltaA]{
		discriminator: tableValueAll,
		valueAll:      &delta,
	}
}
func NewDeltaTableAddAt[ID any, a any, deltaA any](index ID, newElement a) DeltaTable[ID, a, deltaA] {
	addAt := NewTuple2(index, newElement)
	return DeltaTable[ID, a, deltaA]{
		discriminator: tableAddAt,
		addAt:         &addAt,
	}
}
func NewDeltaTableRemoveAt[ID any, a any, deltaA any](index ID) DeltaTable[ID, a, deltaA] {
	return DeltaTable[ID, a, deltaA]{
		discriminator: tableRemoveAt,
		removeAt:      &index,
	}
}
func NewDeltaTableMoveFromTo[ID any, a any, deltaA any](from ID, to ID) DeltaTable[ID, a, deltaA] {
	move := NewTuple2(from, to)
	return DeltaTable[ID, a, deltaA]{
		discriminator: tableMoveFromTo,
		moveFromTo:    &move,
	}
}
func NewDeltaTableDuplicateAt[ID any, a any, deltaA any](index ID) DeltaTable[ID, a, deltaA] {
	return DeltaTable[ID, a, deltaA]{
		discriminator: tableDuplicateAt,
		duplicateAt:   &index,
	}
}
func NewDeltaTableAdd[ID any, a any, deltaA any](newElement a) DeltaTable[ID, a, deltaA] {
	return DeltaTable[ID, a, deltaA]{
		discriminator: tableAdd,
		add:           &newElement,
	}
}
func NewDeltaTableAddEmpty[ID any, a any, deltaA any]() DeltaTable[ID, a, deltaA] {
	return DeltaTable[ID, a, deltaA]{
		discriminator: tableAddEmpty,
	}
}

func MatchDeltaTable[ID comparable, a any, deltaA any, Result any](
	onValue func(Tuple2[ID, deltaA]) func(ReaderWithError[Unit, a]) (Result, error),
	onValueAll func(deltaA) (Result, error),
	onAddAt func(Tuple2[ID, a]) (Result, error),
	onRemoveAt func(ID) (Result, error),
	onMoveFromTo func(Tuple2[ID, ID]) (Result, error),
	onDuplicateAt func(ID) (Result, error),
	onAdd func(a) (Result, error),
	onAddEmpty func() (Result, error),
) func(DeltaTable[ID, a, deltaA]) func(ReaderWithError[Unit, Table[ID, a]]) (Result, error) {
	return func(delta DeltaTable[ID, a, deltaA]) func(ReaderWithError[Unit, Table[ID, a]]) (Result, error) {
		return func(table ReaderWithError[Unit, Table[ID, a]]) (Result, error) {
			var result Result
			switch delta.discriminator {
			case tableValue:
				value := MapReaderWithError[Unit, Table[ID, a], a](
					func(table Table[ID, a]) a {
						return table.Values[table.IdToIndex[delta.value.Item1]]
					},
				)(table)
				return onValue(*delta.value)(value)
			case tableValueAll:
				return onValueAll(*delta.valueAll)
			case tableAddAt:
				return onAddAt(*delta.addAt)
			case tableRemoveAt:
				return onRemoveAt(*delta.removeAt)
			case tableMoveFromTo:
				return onMoveFromTo(*delta.moveFromTo)
			case tableDuplicateAt:
				return onDuplicateAt(*delta.duplicateAt)
			case tableAdd:
				return onAdd(*delta.add)
			case tableAddEmpty:
				return onAddEmpty()
			}
			return result, NewInvalidDiscriminatorError(string(delta.discriminator), "DeltaTable")
		}
	}
}
