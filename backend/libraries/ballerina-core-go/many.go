package ballerina

import (
	"github.com/google/uuid"
)

type Many[T any] struct {
	Values  []T
	From    int
	To      int
	HasMore bool
}

func NewMany[T any](values []T, from int, to int, hasMore bool) Many[T] {
	return Many[T]{
		Values:  values,
		From:    from,
		To:      to,
		HasMore: hasMore,
	}
}

func MapMany[T, U any](ts Many[T], f func(T) U) Many[U] {
	us := make([]U, len(ts.Values))
	for i := range ts.Values {
		us[i] = f(ts.Values[i])
	}
	return Many[U]{Values: us, HasMore: ts.HasMore}
}
func DefaultMany[T any]() Many[T] {
	return Many[T]{Values: make([]T, 0), From: 0, To: 0, HasMore: false}
}

type DeltaManyEffectsEnum string

const (
	ManyReplace     DeltaManyEffectsEnum = "ManyReplace"
	ManyValue       DeltaManyEffectsEnum = "ManyValue"
	ManyAddAt       DeltaManyEffectsEnum = "ManyAddAt"
	ManyRemoveAt    DeltaManyEffectsEnum = "ManyRemoveAt"
	ManyMoveFromTo  DeltaManyEffectsEnum = "ManyMoveFromTo"
	ManyDuplicateAt DeltaManyEffectsEnum = "ManyDuplicateAt"
	ManyAdd         DeltaManyEffectsEnum = "ManyAdd"
)

var AllDeltaManyEffectsEnumCases = [...]DeltaManyEffectsEnum{ManyReplace, ManyValue, ManyAddAt, ManyRemoveAt, ManyMoveFromTo, ManyDuplicateAt, ManyAdd}

func DefaultDeltaManyEffectsEnum() DeltaManyEffectsEnum { return AllDeltaManyEffectsEnumCases[0] }

type DeltaMany[a any, deltaA any] struct {
	DeltaBase
	Discriminator DeltaManyEffectsEnum
	Replace       Many[a]
	Value         Tuple2[uuid.UUID, deltaA]
	AddAt         Tuple2[uuid.UUID, a]
	RemoveAt      uuid.UUID
	MoveFromTo    Tuple2[uuid.UUID, uuid.UUID]
	DuplicateAt   uuid.UUID
	Add           a
}

func NewDeltaManyReplace[a any, deltaA any](value Many[a]) DeltaMany[a, deltaA] {
	return DeltaMany[a, deltaA]{
		Discriminator: ManyReplace,
		Replace:       value,
	}
}
func NewDeltaManyValue[a any, deltaA any](index uuid.UUID, delta deltaA) DeltaMany[a, deltaA] {
	return DeltaMany[a, deltaA]{
		Discriminator: ManyValue,
		Value:         NewTuple2(index, delta),
	}
}
func NewDeltaManyAddAt[a any, deltaA any](index uuid.UUID, newElement a) DeltaMany[a, deltaA] {
	return DeltaMany[a, deltaA]{
		Discriminator: ManyAddAt,
		AddAt:         NewTuple2(index, newElement),
	}
}
func NewDeltaManyRemoveAt[a any, deltaA any](index uuid.UUID) DeltaMany[a, deltaA] {
	return DeltaMany[a, deltaA]{
		Discriminator: ManyRemoveAt,
		RemoveAt:      index,
	}
}
func NewDeltaManyMoveFromTo[a any, deltaA any](from uuid.UUID, to uuid.UUID) DeltaMany[a, deltaA] {
	return DeltaMany[a, deltaA]{
		Discriminator: ManyRemoveAt,
		MoveFromTo:    NewTuple2(from, to),
	}
}
func NewDeltaManyDuplicateAt[a any, deltaA any](index uuid.UUID) DeltaMany[a, deltaA] {
	return DeltaMany[a, deltaA]{
		Discriminator: ManyDuplicateAt,
		DuplicateAt:   index,
	}
}
func NewDeltaManyAdd[a any, deltaA any](newElement a) DeltaMany[a, deltaA] {
	return DeltaMany[a, deltaA]{
		Discriminator: ManyAdd,
		Add:           newElement,
	}
}

func MatchDeltaMany[a any, deltaA any, Result any](
	onReplace func(Many[a]) (Result, error),
	onValue func(Tuple2[uuid.UUID, deltaA]) (Result, error),
	onAddAt func(Tuple2[uuid.UUID, a]) (Result, error),
	onRemoveAt func(uuid.UUID) (Result, error),
	onMoveFromTo func(Tuple2[uuid.UUID, uuid.UUID]) (Result, error),
	onDuplicateAt func(uuid.UUID) (Result, error),
	onAdd func(a) (Result, error),
) func(DeltaMany[a, deltaA]) (Result, error) {
	return func(delta DeltaMany[a, deltaA]) (Result, error) {
		var result Result
		switch delta.Discriminator {
		case "ManyReplace":
			return onReplace(delta.Replace)
		case "ManyValue":
			return onValue(delta.Value)
		case "ManyAddAt":
			return onAddAt(delta.AddAt)
		case "ManyRemoveAt":
			return onRemoveAt(delta.RemoveAt)
		case "ManyMoveFromTo":
			return onMoveFromTo(delta.MoveFromTo)
		case "ManyDuplicateAt":
			return onDuplicateAt(delta.DuplicateAt)
		case "ManyAdd":
			return onAdd(delta.Add)
		}
		return result, NewInvalidDiscriminatorError(string(delta.Discriminator), "DeltaMany")
	}
}
