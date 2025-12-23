package ballerina

import (
	"bytes"
	"encoding/json"

	"github.com/google/uuid"
)

type deltaTableEffectsEnum string

const (
	tableValue       deltaTableEffectsEnum = "TableValue"
	tableValueAll    deltaTableEffectsEnum = "TableValueAll"
	tableAddAt       deltaTableEffectsEnum = "TableAddAt"
	tableRemoveAt    deltaTableEffectsEnum = "TableRemoveAt"
	tableRemoveAll   deltaTableEffectsEnum = "TableRemoveAll"
	tableMoveFromTo  deltaTableEffectsEnum = "TableMoveFromTo"
	tableDuplicateAt deltaTableEffectsEnum = "TableDuplicateAt"
	tableAdd         deltaTableEffectsEnum = "TableAdd"
	tableAddEmpty    deltaTableEffectsEnum = "TableAddEmpty"
	tableActionOnAll deltaTableEffectsEnum = "TableActionOnAll"
)

type DeltaTable[a any, deltaA any] struct {
	DeltaBase
	discriminator deltaTableEffectsEnum
	value         *Tuple2[uuid.UUID, deltaA]
	valueAll      *deltaA
	addAt         *Tuple2[uuid.UUID, a]
	removeAt      *uuid.UUID
	removeAll     Unit
	moveFromTo    *Tuple2[uuid.UUID, uuid.UUID]
	duplicateAt   *uuid.UUID
	add           *a
	actionOnAll   *string
}

var _ json.Unmarshaler = &DeltaTable[Unit, Unit]{}
var _ json.Marshaler = DeltaTable[Unit, Unit]{}

func (d DeltaTable[a, deltaA]) MarshalJSON() ([]byte, error) {
	return json.Marshal(struct {
		DeltaBase
		Discriminator deltaTableEffectsEnum
		Value         *Tuple2[uuid.UUID, deltaA]
		ValueAll      *deltaA
		AddAt         *Tuple2[uuid.UUID, a]
		RemoveAt      *uuid.UUID
		RemoveAll     Unit
		MoveFromTo    *Tuple2[uuid.UUID, uuid.UUID]
		DuplicateAt   *uuid.UUID
		Add           *a
		ActionOnAll   *string
	}{
		DeltaBase:     d.DeltaBase,
		Discriminator: d.discriminator,
		Value:         d.value,
		ValueAll:      d.valueAll,
		AddAt:         d.addAt,
		RemoveAt:      d.removeAt,
		RemoveAll:     d.removeAll,
		MoveFromTo:    d.moveFromTo,
		DuplicateAt:   d.duplicateAt,
		Add:           d.add,
		ActionOnAll:   d.actionOnAll,
	})
}

func (d *DeltaTable[a, deltaA]) UnmarshalJSON(data []byte) error {
	var aux struct {
		DeltaBase
		Discriminator deltaTableEffectsEnum
		Value         *Tuple2[uuid.UUID, deltaA]
		ValueAll      *deltaA
		AddAt         *Tuple2[uuid.UUID, a]
		RemoveAt      *uuid.UUID
		RemoveAll     Unit
		MoveFromTo    *Tuple2[uuid.UUID, uuid.UUID]
		DuplicateAt   *uuid.UUID
		Add           *a
		ActionOnAll   *string
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
	d.removeAll = aux.RemoveAll
	d.moveFromTo = aux.MoveFromTo
	d.duplicateAt = aux.DuplicateAt
	d.add = aux.Add
	d.actionOnAll = aux.ActionOnAll
	return nil
}

func NewDeltaTableValue[a any, deltaA any](index uuid.UUID, delta deltaA) DeltaTable[a, deltaA] {
	val := NewTuple2(index, delta)
	return DeltaTable[a, deltaA]{
		discriminator: tableValue,
		value:         &val,
	}
}
func NewDeltaTableValueAll[a any, deltaA any](delta deltaA) DeltaTable[a, deltaA] {
	return DeltaTable[a, deltaA]{
		discriminator: tableValueAll,
		valueAll:      &delta,
	}
}
func NewDeltaTableAddAt[a any, deltaA any](index uuid.UUID, newElement a) DeltaTable[a, deltaA] {
	addAt := NewTuple2(index, newElement)
	return DeltaTable[a, deltaA]{
		discriminator: tableAddAt,
		addAt:         &addAt,
	}
}
func NewDeltaTableRemoveAt[a any, deltaA any](index uuid.UUID) DeltaTable[a, deltaA] {
	return DeltaTable[a, deltaA]{
		discriminator: tableRemoveAt,
		removeAt:      &index,
	}
}
func NewDeltaTableRemoveAll[a any, deltaA any]() DeltaTable[a, deltaA] {
	return DeltaTable[a, deltaA]{
		discriminator: tableRemoveAll,
		removeAll:     NewUnit(),
	}
}
func NewDeltaTableMoveFromTo[a any, deltaA any](from uuid.UUID, to uuid.UUID) DeltaTable[a, deltaA] {
	move := NewTuple2(from, to)
	return DeltaTable[a, deltaA]{
		discriminator: tableMoveFromTo,
		moveFromTo:    &move,
	}
}
func NewDeltaTableDuplicateAt[a any, deltaA any](index uuid.UUID) DeltaTable[a, deltaA] {
	return DeltaTable[a, deltaA]{
		discriminator: tableDuplicateAt,
		duplicateAt:   &index,
	}
}
func NewDeltaTableAdd[a any, deltaA any](newElement a) DeltaTable[a, deltaA] {
	return DeltaTable[a, deltaA]{
		discriminator: tableAdd,
		add:           &newElement,
	}
}
func NewDeltaTableAddEmpty[a any, deltaA any]() DeltaTable[a, deltaA] {
	return DeltaTable[a, deltaA]{
		discriminator: tableAddEmpty,
	}
}
func NewDeltaTableActionOnAll[a any, deltaA any](actionOnAll string) DeltaTable[a, deltaA] {
	return DeltaTable[a, deltaA]{
		discriminator: tableActionOnAll,
		actionOnAll:   &actionOnAll,
	}
}

func MatchDeltaTable[a any, deltaA any, Result any](
	onValue func(Tuple2[uuid.UUID, deltaA]) func(ReaderWithError[Unit, a]) (Result, error),
	onValueAll func(deltaA) (Result, error),
	onAddAt func(Tuple2[uuid.UUID, a]) (Result, error),
	onRemoveAt func(uuid.UUID) (Result, error),
	onRemoveAll func() (Result, error),
	onMoveFromTo func(Tuple2[uuid.UUID, uuid.UUID]) (Result, error),
	onDuplicateAt func(uuid.UUID) (Result, error),
	onAdd func(a) (Result, error),
	onAddEmpty func() (Result, error),
	onActionOnAll func(string) (Result, error),
) func(DeltaTable[a, deltaA]) func(ReaderWithError[Unit, Table[a]]) (Result, error) {
	return func(delta DeltaTable[a, deltaA]) func(ReaderWithError[Unit, Table[a]]) (Result, error) {
		return func(table ReaderWithError[Unit, Table[a]]) (Result, error) {
			var result Result
			switch delta.discriminator {
			case tableValue:
				value := MapReaderWithError[Unit, Table[a], a](
					func(table Table[a]) a {
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
			case tableRemoveAll:
				return onRemoveAll()
			case tableMoveFromTo:
				return onMoveFromTo(*delta.moveFromTo)
			case tableDuplicateAt:
				return onDuplicateAt(*delta.duplicateAt)
			case tableAdd:
				return onAdd(*delta.add)
			case tableAddEmpty:
				return onAddEmpty()
			case tableActionOnAll:
				return onActionOnAll(*delta.actionOnAll)
			}
			return result, NewInvalidDiscriminatorError(string(delta.discriminator), "DeltaTable")
		}
	}
}
