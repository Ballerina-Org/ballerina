package ballerina

import (
	"github.com/google/uuid"
)

type Table[T any] struct {
	Values  []T
	From    int
	To      int
	HasMore bool
}

func NewTable[T any](values []T, from int, to int, hasMore bool) Table[T] {
	return Table[T]{
		Values:  values,
		From:    from,
		To:      to,
		HasMore: hasMore,
	}
}

func MapTable[T, U any](ts Table[T], f func(T) U) Table[U] {
	us := make([]U, len(ts.Values))
	for i := range ts.Values {
		us[i] = f(ts.Values[i])
	}
	return Table[U]{Values: us, HasMore: ts.HasMore}
}
func DefaultTable[T any]() Table[T] {
	return Table[T]{Values: make([]T, 0), From: 0, To: 0, HasMore: false}
}

type DeltaTableEffectsEnum string

const (
	TableReplace     DeltaTableEffectsEnum = "TableReplace"
	TableValue       DeltaTableEffectsEnum = "TableValue"
	TableAddAt       DeltaTableEffectsEnum = "TableAddAt"
	TableRemoveAt    DeltaTableEffectsEnum = "TableRemoveAt"
	TableMoveFromTo  DeltaTableEffectsEnum = "TableMoveFromTo"
	TableDuplicateAt DeltaTableEffectsEnum = "TableDuplicateAt"
	TableAdd         DeltaTableEffectsEnum = "TableAdd"
	TableAddEmpty    DeltaTableEffectsEnum = "TableAddEmpty"
)

var AllDeltaTableEffectsEnumCases = [...]DeltaTableEffectsEnum{TableReplace, TableValue, TableAddAt, TableRemoveAt, TableMoveFromTo, TableDuplicateAt, TableAdd, TableAddEmpty}

func DefaultDeltaTableEffectsEnum() DeltaTableEffectsEnum { return AllDeltaTableEffectsEnumCases[0] }

type DeltaTable[a any, deltaA any] struct {
	DeltaBase
	Discriminator DeltaTableEffectsEnum
	Replace       Table[a]
	Value         Tuple2[uuid.UUID, deltaA]
	AddAt         Tuple2[uuid.UUID, a]
	RemoveAt      uuid.UUID
	MoveFromTo    Tuple2[uuid.UUID, uuid.UUID]
	DuplicateAt   uuid.UUID
	Add           a
}

func NewDeltaTableReplace[a any, deltaA any](value Table[a]) DeltaTable[a, deltaA] {
	return DeltaTable[a, deltaA]{
		Discriminator: TableReplace,
		Replace:       value,
	}
}
func NewDeltaTableValue[a any, deltaA any](index uuid.UUID, delta deltaA) DeltaTable[a, deltaA] {
	return DeltaTable[a, deltaA]{
		Discriminator: TableValue,
		Value:         NewTuple2(index, delta),
	}
}
func NewDeltaTableAddAt[a any, deltaA any](index uuid.UUID, newElement a) DeltaTable[a, deltaA] {
	return DeltaTable[a, deltaA]{
		Discriminator: TableAddAt,
		AddAt:         NewTuple2(index, newElement),
	}
}
func NewDeltaTableRemoveAt[a any, deltaA any](index uuid.UUID) DeltaTable[a, deltaA] {
	return DeltaTable[a, deltaA]{
		Discriminator: TableRemoveAt,
		RemoveAt:      index,
	}
}
func NewDeltaTableMoveFromTo[a any, deltaA any](from uuid.UUID, to uuid.UUID) DeltaTable[a, deltaA] {
	return DeltaTable[a, deltaA]{
		Discriminator: TableMoveFromTo,
		MoveFromTo:    NewTuple2(from, to),
	}
}
func NewDeltaTableDuplicateAt[a any, deltaA any](index uuid.UUID) DeltaTable[a, deltaA] {
	return DeltaTable[a, deltaA]{
		Discriminator: TableDuplicateAt,
		DuplicateAt:   index,
	}
}
func NewDeltaTableAdd[a any, deltaA any](newElement a) DeltaTable[a, deltaA] {
	return DeltaTable[a, deltaA]{
		Discriminator: TableAdd,
		Add:           newElement,
	}
}
func NewDeltaTableAddEmpty[a any, deltaA any]() DeltaTable[a, deltaA] {
	return DeltaTable[a, deltaA]{
		Discriminator: TableAddEmpty,
	}
}

func MatchDeltaTable[a any, deltaA any, Result any](
	onReplace func(Table[a]) (Result, error),
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
		switch delta.Discriminator {
		case "TableReplace":
			return onReplace(delta.Replace)
		case "TableValue":
			return onValue(delta.Value)
		case "TableAddAt":
			return onAddAt(delta.AddAt)
		case "TableRemoveAt":
			return onRemoveAt(delta.RemoveAt)
		case "TableMoveFromTo":
			return onMoveFromTo(delta.MoveFromTo)
		case "TableDuplicateAt":
			return onDuplicateAt(delta.DuplicateAt)
		case "TableAdd":
			return onAdd(delta.Add)
		case "TableAddEmpty":
			return onAddEmpty()
		}
		return result, NewInvalidDiscriminatorError(string(delta.Discriminator), "DeltaTable")
	}
}
