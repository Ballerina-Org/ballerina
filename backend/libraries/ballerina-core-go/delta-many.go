package ballerina

import (
	"encoding/json"
)

type DeltaManyEffectsEnum string

const (
	ManyLinkedItems   DeltaManyEffectsEnum = "ManyLinkedItems"
	ManyUnlinkedItems DeltaManyEffectsEnum = "ManyUnlinkedItems"
	ManyAllItems      DeltaManyEffectsEnum = "ManyAllItems"
)

var AllDeltaManyEffectsEnumCases = [...]DeltaManyEffectsEnum{ManyLinkedItems, ManyUnlinkedItems, ManyAllItems}

func DefaultDeltaManyEffectsEnum() DeltaManyEffectsEnum {
	return AllDeltaManyEffectsEnumCases[0]
}

type DeltaMany[T any, deltaT any] struct {
	discriminator DeltaManyEffectsEnum
	linkedItems   *DeltaChunk[T, deltaT]
	unlinkedItems *DeltaChunk[T, deltaT]
	allItems      *DeltaChunk[ManyItem[T], DeltaManyItem[T, deltaT]]
}

var _ json.Unmarshaler = &DeltaMany[Unit, Unit]{}
var _ json.Marshaler = DeltaMany[Unit, Unit]{}

// MarshalJSON implements json.Marshaler.
func (d DeltaMany[T, deltaT]) MarshalJSON() ([]byte, error) {
	return json.Marshal(&struct {
		Discriminator DeltaManyEffectsEnum
		LinkedItems   *DeltaChunk[T, deltaT]
		UnlinkedItems *DeltaChunk[T, deltaT]
		AllItems      *DeltaChunk[ManyItem[T], DeltaManyItem[T, deltaT]]
	}{
		Discriminator: d.discriminator,
		LinkedItems:   d.linkedItems,
		UnlinkedItems: d.unlinkedItems,
		AllItems:      d.allItems,
	})
}

// UnmarshalJSON implements json.Unmarshaler.
func (d *DeltaMany[T, deltaT]) UnmarshalJSON(data []byte) error {
	var aux struct {
		Discriminator DeltaManyEffectsEnum
		LinkedItems   *DeltaChunk[T, deltaT]
		UnlinkedItems *DeltaChunk[T, deltaT]
		AllItems      *DeltaChunk[ManyItem[T], DeltaManyItem[T, deltaT]]
	}
	if err := json.Unmarshal(data, &aux); err != nil {
		return err
	}
	d.discriminator = aux.Discriminator
	d.linkedItems = aux.LinkedItems
	d.unlinkedItems = aux.UnlinkedItems
	d.allItems = aux.AllItems
	return nil
}

func NewDeltaManyLinkedItems[T any, deltaT any](delta DeltaChunk[T, deltaT]) DeltaMany[T, deltaT] {
	return DeltaMany[T, deltaT]{
		discriminator: ManyLinkedItems,
		linkedItems:   &delta,
	}
}

func NewDeltaManyUnlinkedItems[T any, deltaT any](delta DeltaChunk[T, deltaT]) DeltaMany[T, deltaT] {
	return DeltaMany[T, deltaT]{
		discriminator: ManyUnlinkedItems,
		unlinkedItems: &delta,
	}
}

func NewDeltaManyAllItems[T any, deltaT any](delta DeltaChunk[ManyItem[T], DeltaManyItem[T, deltaT]]) DeltaMany[T, deltaT] {
	return DeltaMany[T, deltaT]{
		discriminator: ManyAllItems,
		allItems:      &delta,
	}
}

func MatchDeltaMany[T any, deltaT any, Result any](
	onLinkedItems func(*DeltaChunk[T, deltaT]) (Result, error),
	onUnlinkedItems func(*DeltaChunk[T, deltaT]) (Result, error),
	onAllItems func(*DeltaChunk[ManyItem[T], DeltaManyItem[T, deltaT]]) (Result, error),
) func(DeltaMany[T, deltaT]) (Result, error) {
	return func(delta DeltaMany[T, deltaT]) (Result, error) {
		var result Result
		switch delta.discriminator {
		case "ManyLinkedItems":
			return onLinkedItems(delta.linkedItems)
		case "ManyUnlinkedItems":
			return onUnlinkedItems(delta.unlinkedItems)
		case "ManyAllItems":
			return onAllItems(delta.allItems)
		}
		return result, NewInvalidDiscriminatorError(string(delta.discriminator), "DeltaMany")
	}
}
