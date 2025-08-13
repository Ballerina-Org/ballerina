package ballerina

import (
	"encoding/json"
	"github.com/google/uuid"
)

type DeltaTableEffectsEnum string

const (
	TableValue       DeltaTableEffectsEnum = "TableValue"
	TableAddAt       DeltaTableEffectsEnum = "TableAddAt"
	TableRemoveAt    DeltaTableEffectsEnum = "TableRemoveAt"
	TableMoveFromTo  DeltaTableEffectsEnum = "TableMoveFromTo"
	TableDuplicateAt DeltaTableEffectsEnum = "TableDuplicateAt"
	TableAdd         DeltaTableEffectsEnum = "TableAdd"
	TableAddEmpty    DeltaTableEffectsEnum = "TableAddEmpty"
)

var AllDeltaTableEffectsEnumCases = [...]DeltaTableEffectsEnum{TableValue, TableAddAt, TableRemoveAt, TableMoveFromTo, TableDuplicateAt, TableAdd, TableAddEmpty}

func DefaultDeltaTableEffectsEnum() DeltaTableEffectsEnum { return AllDeltaTableEffectsEnumCases[0] }

type DeltaTable[a any, deltaA any] struct {
	DeltaBase
	discriminator DeltaTableEffectsEnum
	value         *Tuple2[uuid.UUID, deltaA]
	addAt         *Tuple2[uuid.UUID, a]
	removeAt      *uuid.UUID
	moveFromTo    *Tuple2[uuid.UUID, uuid.UUID]
	duplicateAt   *uuid.UUID
	add           *a
}

var _ json.Unmarshaler = &DeltaTable[Unit, Unit]{}
var _ json.Marshaler = DeltaTable[Unit, Unit]{}

func (d DeltaTable[a, deltaA]) MarshalJSON() ([]byte, error) {
	return json.Marshal(struct {
		DeltaBase
		Discriminator DeltaTableEffectsEnum
		Value         *Tuple2[uuid.UUID, deltaA]
		AddAt         *Tuple2[uuid.UUID, a]
		RemoveAt      *uuid.UUID
		MoveFromTo    *Tuple2[uuid.UUID, uuid.UUID]
		DuplicateAt   *uuid.UUID
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

func (d *DeltaTable[a, deltaA]) UnmarshalJSON(data []byte) error {
	var aux struct {
		DeltaBase
		Discriminator DeltaTableEffectsEnum
		Value         *Tuple2[uuid.UUID, deltaA]
		AddAt         *Tuple2[uuid.UUID, a]
		RemoveAt      *uuid.UUID
		MoveFromTo    *Tuple2[uuid.UUID, uuid.UUID]
		DuplicateAt   *uuid.UUID
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

func NewDeltaTableValue[a any, deltaA any](index uuid.UUID, delta deltaA) DeltaTable[a, deltaA] {
	val := NewTuple2(index, delta)
	return DeltaTable[a, deltaA]{
		discriminator: TableValue,
		value:         &val,
	}
}
func NewDeltaTableAddAt[a any, deltaA any](index uuid.UUID, newElement a) DeltaTable[a, deltaA] {
	addAt := NewTuple2(index, newElement)
	return DeltaTable[a, deltaA]{
		discriminator: TableAddAt,
		addAt:         &addAt,
	}
}
func NewDeltaTableRemoveAt[a any, deltaA any](index uuid.UUID) DeltaTable[a, deltaA] {
	return DeltaTable[a, deltaA]{
		discriminator: TableRemoveAt,
		removeAt:      &index,
	}
}
func NewDeltaTableMoveFromTo[a any, deltaA any](from uuid.UUID, to uuid.UUID) DeltaTable[a, deltaA] {
	move := NewTuple2(from, to)
	return DeltaTable[a, deltaA]{
		discriminator: TableMoveFromTo,
		moveFromTo:    &move,
	}
}
func NewDeltaTableDuplicateAt[a any, deltaA any](index uuid.UUID) DeltaTable[a, deltaA] {
	return DeltaTable[a, deltaA]{
		discriminator: TableDuplicateAt,
		duplicateAt:   &index,
	}
}
func NewDeltaTableAdd[a any, deltaA any](newElement a) DeltaTable[a, deltaA] {
	return DeltaTable[a, deltaA]{
		discriminator: TableAdd,
		add:           &newElement,
	}
}
func NewDeltaTableAddEmpty[a any, deltaA any]() DeltaTable[a, deltaA] {
	return DeltaTable[a, deltaA]{
		discriminator: TableAddEmpty,
	}
}

func MatchDeltaTable[a any, deltaA any, Result any](
	onValue func(Tuple2[uuid.UUID, deltaA]) (Result, error),
	onAddAt func(Tuple2[uuid.UUID, a]) (Result, error),
	onRemoveAt func(uuid.UUID) (Result, error),
	onMoveFromTo func(Tuple2[uuid.UUID, uuid.UUID]) (Result, error),
	onDuplicateAt func(uuid.UUID) (Result, error),
	onAdd func(a) (Result, error),
	onAddEmpty func() (Result, error),
) func(DeltaTable[a, deltaA]) (Result, error) {
	return func(delta DeltaTable[a, deltaA]) (Result, error) {
		var result Result
		switch delta.discriminator {
		case TableValue:
			return onValue(*delta.value)
		case TableAddAt:
			return onAddAt(*delta.addAt)
		case TableRemoveAt:
			return onRemoveAt(*delta.removeAt)
		case TableMoveFromTo:
			return onMoveFromTo(*delta.moveFromTo)
		case TableDuplicateAt:
			return onDuplicateAt(*delta.duplicateAt)
		case TableAdd:
			return onAdd(*delta.add)
		case TableAddEmpty:
			return onAddEmpty()
		}
		return result, NewInvalidDiscriminatorError(string(delta.discriminator), "DeltaTable")
	}
}
