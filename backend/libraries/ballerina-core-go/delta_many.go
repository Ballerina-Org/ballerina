package ballerina

import (
	"bytes"
	"encoding/json"
)

type deltaManyEffectsEnum string

const (
	manyLinkedItems   deltaManyEffectsEnum = "ManyLinkedItems"
	manyUnlinkedItems deltaManyEffectsEnum = "ManyUnlinkedItems"
	manyAllItems      deltaManyEffectsEnum = "ManyAllItems"
)

type DeltaMany[T any, deltaT any, ID comparable] struct {
	DeltaBase
	discriminator deltaManyEffectsEnum
	linkedItems   *DeltaChunk[T, deltaT, ID]
	unlinkedItems *DeltaChunk[T, deltaT, ID]
	allItems      *DeltaChunk[ManyItem[T], DeltaManyItem[T, deltaT], ID]
}

var _ json.Unmarshaler = &DeltaMany[Unit, Unit, Unit]{}
var _ json.Marshaler = DeltaMany[Unit, Unit, Unit]{}

func (d DeltaMany[T, deltaT, ID]) MarshalJSON() ([]byte, error) {
	return json.Marshal(&struct {
		DeltaBase
		Discriminator deltaManyEffectsEnum
		LinkedItems   *DeltaChunk[T, deltaT, ID]
		UnlinkedItems *DeltaChunk[T, deltaT, ID]
		AllItems      *DeltaChunk[ManyItem[T], DeltaManyItem[T, deltaT], ID]
	}{
		DeltaBase:     d.DeltaBase,
		Discriminator: d.discriminator,
		LinkedItems:   d.linkedItems,
		UnlinkedItems: d.unlinkedItems,
		AllItems:      d.allItems,
	})
}

// UnmarshalJSON implements json.Unmarshaler.
func (d *DeltaMany[T, deltaT, ID]) UnmarshalJSON(data []byte) error {
	var aux struct {
		DeltaBase
		Discriminator deltaManyEffectsEnum
		LinkedItems   *DeltaChunk[T, deltaT, ID]
		UnlinkedItems *DeltaChunk[T, deltaT, ID]
		AllItems      *DeltaChunk[ManyItem[T], DeltaManyItem[T, deltaT], ID]
	}
	dec := json.NewDecoder(bytes.NewReader(data))
	dec.DisallowUnknownFields()
	if err := dec.Decode(&aux); err != nil {
		return err
	}
	d.DeltaBase = aux.DeltaBase
	d.discriminator = aux.Discriminator
	d.linkedItems = aux.LinkedItems
	d.unlinkedItems = aux.UnlinkedItems
	d.allItems = aux.AllItems
	return nil
}

func NewDeltaManyLinkedItems[T any, deltaT any, ID comparable](delta DeltaChunk[T, deltaT, ID]) DeltaMany[T, deltaT, ID] {
	return DeltaMany[T, deltaT, ID]{
		discriminator: manyLinkedItems,
		linkedItems:   &delta,
	}
}

func NewDeltaManyUnlinkedItems[T any, deltaT any, ID comparable](delta DeltaChunk[T, deltaT, ID]) DeltaMany[T, deltaT, ID] {
	return DeltaMany[T, deltaT, ID]{
		discriminator: manyUnlinkedItems,
		unlinkedItems: &delta,
	}
}

func NewDeltaManyAllItems[T any, deltaT any, ID comparable](delta DeltaChunk[ManyItem[T], DeltaManyItem[T, deltaT], ID]) DeltaMany[T, deltaT, ID] {
	return DeltaMany[T, deltaT, ID]{
		discriminator: manyAllItems,
		allItems:      &delta,
	}
}

func MatchDeltaMany[T any, deltaT any, Result any, ID comparable](
	onLinkedItems func(DeltaChunk[T, deltaT, ID]) func(ReaderWithError[Unit, Chunk[T, ID]]) (Result, error),
	onUnlinkedItems func(DeltaChunk[T, deltaT, ID]) func(ReaderWithError[Unit, Chunk[T, ID]]) (Result, error),
	onAllItems func(DeltaChunk[ManyItem[T], DeltaManyItem[T, deltaT], ID]) func(ReaderWithError[Unit, Chunk[ManyItem[T], ID]]) (Result, error),
) func(DeltaMany[T, deltaT, ID]) func(ReaderWithError[Unit, Many[T, ID]]) (Result, error) {
	return func(delta DeltaMany[T, deltaT, ID]) func(ReaderWithError[Unit, Many[T, ID]]) (Result, error) {
		return func(many ReaderWithError[Unit, Many[T, ID]]) (Result, error) {
			var result Result
			switch delta.discriminator {
			case manyLinkedItems:
				linkedItems := MapReaderWithError[Unit, Many[T, ID], Chunk[T, ID]](
					func(many Many[T, ID]) Chunk[T, ID] {
						return many.LinkedItems
					},
				)(many)
				return onLinkedItems(*delta.linkedItems)(linkedItems)
			case manyUnlinkedItems:
				unlinkedItems := MapReaderWithError[Unit, Many[T, ID], Chunk[T, ID]](
					func(many Many[T, ID]) Chunk[T, ID] {
						return many.UnlinkedItems
					},
				)(many)
				return onUnlinkedItems(*delta.unlinkedItems)(unlinkedItems)
			case manyAllItems:
				allItems := MapReaderWithError[Unit, Many[T, ID], Chunk[ManyItem[T], ID]](
					func(many Many[T, ID]) Chunk[ManyItem[T], ID] {
						return many.AllItems
					},
				)(many)
				return onAllItems(*delta.allItems)(allItems)
			}
			return result, NewInvalidDiscriminatorError(string(delta.discriminator), "DeltaMany")
		}
	}
}
