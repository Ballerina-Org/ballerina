package ballerina

import (
	"encoding/json"
)

type deltaManyEffectsEnum string

const (
	manyLinkedItems   deltaManyEffectsEnum = "ManyLinkedItems"
	manyUnlinkedItems deltaManyEffectsEnum = "ManyUnlinkedItems"
	manyAllItems      deltaManyEffectsEnum = "ManyAllItems"
)

var allDeltaManyEffectsEnumCases = [...]deltaManyEffectsEnum{manyLinkedItems, manyUnlinkedItems, manyAllItems}

func DefaultDeltaManyEffectsEnum() deltaManyEffectsEnum {
	return allDeltaManyEffectsEnumCases[0]
}

type DeltaMany[T any, deltaT any] struct {
	DeltaBase
	discriminator deltaManyEffectsEnum
	linkedItems   *DeltaChunk[T, deltaT]
	unlinkedItems *DeltaChunk[T, deltaT]
	allItems      *DeltaChunk[ManyItem[T], DeltaManyItem[T, deltaT]]
}

var _ json.Unmarshaler = &DeltaMany[Unit, Unit]{}
var _ json.Marshaler = DeltaMany[Unit, Unit]{}

func (d DeltaMany[T, deltaT]) MarshalJSON() ([]byte, error) {
	return json.Marshal(&struct {
		DeltaBase
		Discriminator deltaManyEffectsEnum
		LinkedItems   *DeltaChunk[T, deltaT]
		UnlinkedItems *DeltaChunk[T, deltaT]
		AllItems      *DeltaChunk[ManyItem[T], DeltaManyItem[T, deltaT]]
	}{
		DeltaBase:     d.DeltaBase,
		Discriminator: d.discriminator,
		LinkedItems:   d.linkedItems,
		UnlinkedItems: d.unlinkedItems,
		AllItems:      d.allItems,
	})
}

// UnmarshalJSON implements json.Unmarshaler.
func (d *DeltaMany[T, deltaT]) UnmarshalJSON(data []byte) error {
	var aux struct {
		DeltaBase
		Discriminator deltaManyEffectsEnum
		LinkedItems   *DeltaChunk[T, deltaT]
		UnlinkedItems *DeltaChunk[T, deltaT]
		AllItems      *DeltaChunk[ManyItem[T], DeltaManyItem[T, deltaT]]
	}
	if err := json.Unmarshal(data, &aux); err != nil {
		return err
	}
	d.DeltaBase = aux.DeltaBase
	d.discriminator = aux.Discriminator
	d.linkedItems = aux.LinkedItems
	d.unlinkedItems = aux.UnlinkedItems
	d.allItems = aux.AllItems
	return nil
}

func NewDeltaManyLinkedItems[T any, deltaT any](delta DeltaChunk[T, deltaT]) DeltaMany[T, deltaT] {
	return DeltaMany[T, deltaT]{
		discriminator: manyLinkedItems,
		linkedItems:   &delta,
	}
}

func NewDeltaManyUnlinkedItems[T any, deltaT any](delta DeltaChunk[T, deltaT]) DeltaMany[T, deltaT] {
	return DeltaMany[T, deltaT]{
		discriminator: manyUnlinkedItems,
		unlinkedItems: &delta,
	}
}

func NewDeltaManyAllItems[T any, deltaT any](delta DeltaChunk[ManyItem[T], DeltaManyItem[T, deltaT]]) DeltaMany[T, deltaT] {
	return DeltaMany[T, deltaT]{
		discriminator: manyAllItems,
		allItems:      &delta,
	}
}

func MatchDeltaMany[context any, T any, deltaT any, Result any](
	onLinkedItems func(DeltaChunk[T, deltaT]) func(ReaderWithError[context, Chunk[T]]) (Result, error),
	onUnlinkedItems func(DeltaChunk[T, deltaT]) func(ReaderWithError[context, Chunk[T]]) (Result, error),
	onAllItems func(DeltaChunk[ManyItem[T], DeltaManyItem[T, deltaT]]) func(ReaderWithError[context, Chunk[ManyItem[T]]]) (Result, error),
) func(DeltaMany[T, deltaT]) func(ReaderWithError[context, Many[T]]) (Result, error) {
	return func(delta DeltaMany[T, deltaT]) func(ReaderWithError[context, Many[T]]) (Result, error) {
		return func(many ReaderWithError[context, Many[T]]) (Result, error) {
			var result Result
			switch delta.discriminator {
			case manyLinkedItems:
				linkedItems := MapReaderWithError[context, Many[T], Chunk[T]](
					func(many Many[T]) Chunk[T] {
						return many.LinkedItems
					},
				)(many)
				return onLinkedItems(*delta.linkedItems)(linkedItems)
			case manyUnlinkedItems:
				unlinkedItems := MapReaderWithError[context, Many[T], Chunk[T]](
					func(many Many[T]) Chunk[T] {
						return many.UnlinkedItems
					},
				)(many)
				return onUnlinkedItems(*delta.unlinkedItems)(unlinkedItems)
			case manyAllItems:
				allItems := MapReaderWithError[context, Many[T], Chunk[ManyItem[T]]](
					func(many Many[T]) Chunk[ManyItem[T]] {
						return many.AllItems
					},
				)(many)
				return onAllItems(*delta.allItems)(allItems)
			}
			return result, NewInvalidDiscriminatorError(string(delta.discriminator), "DeltaMany")
		}
	}
}
