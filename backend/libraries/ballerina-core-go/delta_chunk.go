package ballerina

import (
	"bytes"
	"encoding/json"
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

type DeltaChunk[a any, deltaA any, ID comparable] struct {
	DeltaBase
	discriminator deltaChunkEffectsEnum
	value         *Tuple2[ID, deltaA]
	addAt         *Tuple2[ID, a]
	removeAt      *ID
	moveFromTo    *Tuple2[ID, ID]
	duplicateAt   *ID
	add           *a
}

var _ json.Unmarshaler = &DeltaChunk[Unit, Unit, Unit]{}
var _ json.Marshaler = DeltaChunk[Unit, Unit, Unit]{}

func (d DeltaChunk[a, deltaA, ID]) MarshalJSON() ([]byte, error) {
	return json.Marshal(&struct {
		DeltaBase
		Discriminator deltaChunkEffectsEnum
		Value         *Tuple2[ID, deltaA]
		AddAt         *Tuple2[ID, a]
		RemoveAt      *ID
		MoveFromTo    *Tuple2[ID, ID]
		DuplicateAt   *ID
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

func (d *DeltaChunk[a, deltaA, ID]) UnmarshalJSON(data []byte) error {
	type chunkAlias struct {
		DeltaBase
		Discriminator deltaChunkEffectsEnum
		Value         *Tuple2[ID, deltaA]
		AddAt         *Tuple2[ID, a]
		RemoveAt      *ID
		MoveFromTo    *Tuple2[ID, ID]
		DuplicateAt   *ID
		Add           *a
	}
	var aux chunkAlias
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
	return nil
}

func NewDeltaChunkValue[a any, deltaA any, ID comparable](index ID, delta deltaA) DeltaChunk[a, deltaA, ID] {
	tmp := NewTuple2[ID, deltaA](index, delta)
	return DeltaChunk[a, deltaA, ID]{
		discriminator: chunkValue,
		value:         &tmp,
	}
}
func NewDeltaChunkAddAt[a any, deltaA any, ID comparable](index ID, newElement a) DeltaChunk[a, deltaA, ID] {
	tmp := NewTuple2[ID, a](index, newElement)
	return DeltaChunk[a, deltaA, ID]{
		discriminator: chunkAddAt,
		addAt:         &tmp,
	}
}
func NewDeltaChunkRemoveAt[a any, deltaA any, ID comparable](index ID) DeltaChunk[a, deltaA, ID] {
	return DeltaChunk[a, deltaA, ID]{
		discriminator: chunkRemoveAt,
		removeAt:      &index,
	}
}
func NewDeltaChunkMoveFromTo[a any, deltaA any, ID comparable](from ID, to ID) DeltaChunk[a, deltaA, ID] {
	tmp := NewTuple2[ID, ID](from, to)
	return DeltaChunk[a, deltaA, ID]{
		discriminator: chunkMoveFromTo,
		moveFromTo:    &tmp,
	}
}
func NewDeltaChunkDuplicateAt[a any, deltaA any, ID comparable](index ID) DeltaChunk[a, deltaA, ID] {
	return DeltaChunk[a, deltaA, ID]{
		discriminator: chunkDuplicateAt,
		duplicateAt:   &index,
	}
}
func NewDeltaChunkAdd[a any, deltaA any, ID comparable](newElement a) DeltaChunk[a, deltaA, ID] {
	return DeltaChunk[a, deltaA, ID]{
		discriminator: chunkAdd,
		add:           &newElement,
	}
}

func MatchDeltaChunk[a any, deltaA any, Result any, ID comparable](
	onValue func(Tuple2[ID, deltaA]) func(ReaderWithError[Unit, a]) (Result, error),
	onAddAt func(Tuple2[ID, a]) (Result, error),
	onRemoveAt func(ID) (Result, error),
	onMoveFromTo func(Tuple2[ID, ID]) (Result, error),
	onDuplicateAt func(ID) (Result, error),
	onAdd func(a) (Result, error),
) func(DeltaChunk[a, deltaA, ID]) func(ReaderWithError[Unit, Chunk[a, ID]]) (Result, error) {
	return func(delta DeltaChunk[a, deltaA, ID]) func(ReaderWithError[Unit, Chunk[a, ID]]) (Result, error) {
		return func(chunk ReaderWithError[Unit, Chunk[a, ID]]) (Result, error) {
			var result Result
			switch delta.discriminator {
			case chunkValue:
				value := MapReaderWithError[Unit, Chunk[a, ID], a](
					func(chunk Chunk[a, ID]) a {
						return chunk.Values[chunk.IdToIndex[delta.value.Item1]]
					},
				)(chunk)
				return onValue(*delta.value)(value)
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
}
