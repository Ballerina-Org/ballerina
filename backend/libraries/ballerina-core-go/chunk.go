package ballerina

import (
	"github.com/google/uuid"
)

type Chunk[T any] struct {
	Values  []T
	HasMore bool
}

func NewChunk[T any](values []T, from int, to int, hasMore bool) Chunk[T] {
	return Chunk[T]{
		Values:  values,
		HasMore: hasMore,
	}
}

func MapChunk[T, U any](ts Chunk[T], f func(T) U) Chunk[U] {
	us := make([]U, len(ts.Values))
	for i := range ts.Values {
		us[i] = f(ts.Values[i])
	}
	return Chunk[U]{Values: us, HasMore: ts.HasMore}
}
func DefaultChunk[T any]() Chunk[T] {
	return Chunk[T]{Values: make([]T, 0), HasMore: false}
}

type DeltaChunkEffectsEnum string

const (
	ChunkValue       DeltaChunkEffectsEnum = "ChunkValue"
	ChunkAddAt       DeltaChunkEffectsEnum = "ChunkAddAt"
	ChunkRemoveAt    DeltaChunkEffectsEnum = "ChunkRemoveAt"
	ChunkMoveFromTo  DeltaChunkEffectsEnum = "ChunkMoveFromTo"
	ChunkDuplicateAt DeltaChunkEffectsEnum = "ChunkDuplicateAt"
	ChunkAdd         DeltaChunkEffectsEnum = "ChunkAdd"
)

var AllDeltaChunkEffectsEnumCases = [...]DeltaChunkEffectsEnum{ChunkValue, ChunkAddAt, ChunkRemoveAt, ChunkMoveFromTo, ChunkDuplicateAt, ChunkAdd}

func DefaultDeltaChunkEffectsEnum() DeltaChunkEffectsEnum { return AllDeltaChunkEffectsEnumCases[0] }

type DeltaChunk[a any, deltaA any] struct {
	DeltaBase
	Discriminator DeltaChunkEffectsEnum
	Value         Tuple2[uuid.UUID, deltaA]
	AddAt         Tuple2[uuid.UUID, a]
	RemoveAt      uuid.UUID
	MoveFromTo    Tuple2[uuid.UUID, uuid.UUID]
	DuplicateAt   uuid.UUID
	Add           a
}

func NewDeltaChunkValue[a any, deltaA any](index uuid.UUID, delta deltaA) DeltaChunk[a, deltaA] {
	return DeltaChunk[a, deltaA]{
		Discriminator: ChunkValue,
		Value:         NewTuple2(index, delta),
	}
}
func NewDeltaChunkAddAt[a any, deltaA any](index uuid.UUID, newElement a) DeltaChunk[a, deltaA] {
	return DeltaChunk[a, deltaA]{
		Discriminator: ChunkAddAt,
		AddAt:         NewTuple2(index, newElement),
	}
}
func NewDeltaChunkRemoveAt[a any, deltaA any](index uuid.UUID) DeltaChunk[a, deltaA] {
	return DeltaChunk[a, deltaA]{
		Discriminator: ChunkRemoveAt,
		RemoveAt:      index,
	}
}
func NewDeltaChunkMoveFromTo[a any, deltaA any](from uuid.UUID, to uuid.UUID) DeltaChunk[a, deltaA] {
	return DeltaChunk[a, deltaA]{
		Discriminator: ChunkRemoveAt,
		MoveFromTo:    NewTuple2(from, to),
	}
}
func NewDeltaChunkDuplicateAt[a any, deltaA any](index uuid.UUID) DeltaChunk[a, deltaA] {
	return DeltaChunk[a, deltaA]{
		Discriminator: ChunkDuplicateAt,
		DuplicateAt:   index,
	}
}
func NewDeltaChunkAdd[a any, deltaA any](newElement a) DeltaChunk[a, deltaA] {
	return DeltaChunk[a, deltaA]{
		Discriminator: ChunkAdd,
		Add:           newElement,
	}
}

func MatchDeltaChunk[a any, deltaA any, Result any](
	onValue func(Tuple2[uuid.UUID, deltaA]) (Result, error),
	onAddAt func(Tuple2[uuid.UUID, a]) (Result, error),
	onRemoveAt func(uuid.UUID) (Result, error),
	onMoveFromTo func(Tuple2[uuid.UUID, uuid.UUID]) (Result, error),
	onDuplicateAt func(uuid.UUID) (Result, error),
	onAdd func(a) (Result, error),
) func(DeltaChunk[a, deltaA]) (Result, error) {
	return func(delta DeltaChunk[a, deltaA]) (Result, error) {
		var result Result
		switch delta.Discriminator {
		case "ChunkValue":
			return onValue(delta.Value)
		case "ChunkAddAt":
			return onAddAt(delta.AddAt)
		case "ChunkRemoveAt":
			return onRemoveAt(delta.RemoveAt)
		case "ChunkMoveFromTo":
			return onMoveFromTo(delta.MoveFromTo)
		case "ChunkDuplicateAt":
			return onDuplicateAt(delta.DuplicateAt)
		case "ChunkAdd":
			return onAdd(delta.Add)
		}
		return result, NewInvalidDiscriminatorError(string(delta.Discriminator), "DeltaChunk")
	}
}
