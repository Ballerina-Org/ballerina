package ballerina

import (
	"encoding/json"

	"github.com/google/uuid"
)

type deltaChunkEffectsEnum string

const (
	chunkValue       deltaChunkEffectsEnum = "ChunkValue"
	chunkAddAt       deltaChunkEffectsEnum = "ChunkAddAt"
	chunkRemoveAt    deltaChunkEffectsEnum = "ChunkRemoveAt"
	chunkMoveFromTo  deltaChunkEffectsEnum = "ChunkMoveFromTo"
	chunkDuplicateAt deltaChunkEffectsEnum = "ChunkDuplicateAt"
	chunkAdd         deltaChunkEffectsEnum = "ChunkAdd"
)

var allDeltaChunkEffectsEnumCases = [...]deltaChunkEffectsEnum{chunkValue, chunkAddAt, chunkRemoveAt, chunkMoveFromTo, chunkDuplicateAt, chunkAdd}

func DefaultDeltaChunkEffectsEnum() deltaChunkEffectsEnum { return allDeltaChunkEffectsEnumCases[0] }

type DeltaChunk[a any, deltaA any] struct {
	DeltaBase
	discriminator deltaChunkEffectsEnum
	value         *Tuple2[uuid.UUID, deltaA]
	addAt         *Tuple2[uuid.UUID, a]
	removeAt      *uuid.UUID
	moveFromTo    *Tuple2[uuid.UUID, uuid.UUID]
	duplicateAt   *uuid.UUID
	add           *a
}

var _ json.Unmarshaler = &DeltaChunk[Unit, Unit]{}
var _ json.Marshaler = DeltaChunk[Unit, Unit]{}

func (d DeltaChunk[a, deltaA]) MarshalJSON() ([]byte, error) {
	return json.Marshal(&struct {
		DeltaBase
		Discriminator deltaChunkEffectsEnum
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

func (d *DeltaChunk[a, deltaA]) UnmarshalJSON(data []byte) error {
	type chunkAlias struct {
		DeltaBase
		Discriminator deltaChunkEffectsEnum
		Value         *Tuple2[uuid.UUID, deltaA]
		AddAt         *Tuple2[uuid.UUID, a]
		RemoveAt      *uuid.UUID
		MoveFromTo    *Tuple2[uuid.UUID, uuid.UUID]
		DuplicateAt   *uuid.UUID
		Add           *a
	}
	var aux chunkAlias
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

func NewDeltaChunkValue[a any, deltaA any](index uuid.UUID, delta deltaA) DeltaChunk[a, deltaA] {
	tmp := NewTuple2[uuid.UUID, deltaA](index, delta)
	return DeltaChunk[a, deltaA]{
		discriminator: chunkValue,
		value:         &tmp,
	}
}
func NewDeltaChunkAddAt[a any, deltaA any](index uuid.UUID, newElement a) DeltaChunk[a, deltaA] {
	tmp := NewTuple2[uuid.UUID, a](index, newElement)
	return DeltaChunk[a, deltaA]{
		discriminator: chunkAddAt,
		addAt:         &tmp,
	}
}
func NewDeltaChunkRemoveAt[a any, deltaA any](index uuid.UUID) DeltaChunk[a, deltaA] {
	return DeltaChunk[a, deltaA]{
		discriminator: chunkRemoveAt,
		removeAt:      &index,
	}
}
func NewDeltaChunkMoveFromTo[a any, deltaA any](from uuid.UUID, to uuid.UUID) DeltaChunk[a, deltaA] {
	tmp := NewTuple2[uuid.UUID, uuid.UUID](from, to)
	return DeltaChunk[a, deltaA]{
		discriminator: chunkMoveFromTo,
		moveFromTo:    &tmp,
	}
}
func NewDeltaChunkDuplicateAt[a any, deltaA any](index uuid.UUID) DeltaChunk[a, deltaA] {
	return DeltaChunk[a, deltaA]{
		discriminator: chunkDuplicateAt,
		duplicateAt:   &index,
	}
}
func NewDeltaChunkAdd[a any, deltaA any](newElement a) DeltaChunk[a, deltaA] {
	return DeltaChunk[a, deltaA]{
		discriminator: chunkAdd,
		add:           &newElement,
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
		switch delta.discriminator {
		case chunkValue:
			return onValue(*delta.value)
		case chunkAddAt:
			return onAddAt(*delta.addAt)
		case chunkRemoveAt:
			return onRemoveAt(*delta.removeAt)
		case chunkMoveFromTo:
			return onMoveFromTo(*delta.moveFromTo)
		case chunkDuplicateAt:
			return onDuplicateAt(*delta.duplicateAt)
		case chunkAdd:
			return onAdd(*delta.add)
		}
		return result, NewInvalidDiscriminatorError(string(delta.discriminator), "DeltaChunk")
	}
}
