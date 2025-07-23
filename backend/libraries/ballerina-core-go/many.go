package ballerina

type Many[T any] struct {
	LinkedItems   Chunk[T]
	UnlinkedItems Chunk[T]
	AllItems      Chunk[ManyItem[T]]
}

func NewMany[T any](linkedItems Chunk[T], unlinkedItems Chunk[T], allItems Chunk[ManyItem[T]]) Many[T] {
	return Many[T]{
		LinkedItems:   linkedItems,
		UnlinkedItems: unlinkedItems,
		AllItems:      allItems,
	}
}

func MapMany[T, U any](m Many[T], f func(T) U) Many[U] {
	return Many[U]{
		LinkedItems:   MapChunk(m.LinkedItems, f),
		UnlinkedItems: MapChunk(m.UnlinkedItems, f),
		AllItems: MapChunk(m.AllItems, func(item ManyItem[T]) ManyItem[U] {
			return MapManyItem(item, f)
		}),
	}
}

func DefaultMany[T any]() Many[T] {
	return Many[T]{
		LinkedItems:   DefaultChunk[T](),
		UnlinkedItems: DefaultChunk[T](),
		AllItems:      DefaultChunk[ManyItem[T]](),
	}
}

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
	DeltaBase
	Discriminator DeltaManyEffectsEnum
	LinkedItems   DeltaChunk[T, deltaT]
	UnlinkedItems DeltaChunk[T, deltaT]
	AllItems      DeltaChunk[ManyItem[T], DeltaManyItem[T, deltaT]]
}

func NewDeltaManyLinkedItems[T any, deltaT any](delta DeltaChunk[T, deltaT]) DeltaMany[T, deltaT] {
	return DeltaMany[T, deltaT]{
		Discriminator: ManyLinkedItems,
		LinkedItems:   delta,
	}
}

func NewDeltaManyUnlinkedItems[T any, deltaT any](delta DeltaChunk[T, deltaT]) DeltaMany[T, deltaT] {
	return DeltaMany[T, deltaT]{
		Discriminator: ManyUnlinkedItems,
		UnlinkedItems: delta,
	}
}

func NewDeltaManyAllItems[T any, deltaT any](delta DeltaChunk[ManyItem[T], DeltaManyItem[T, deltaT]]) DeltaMany[T, deltaT] {
	return DeltaMany[T, deltaT]{
		Discriminator: ManyAllItems,
		AllItems:      delta,
	}
}

func MatchDeltaMany[T any, deltaT any, Result any](
	onLinkedItems func(DeltaChunk[T, deltaT]) (Result, error),
	onUnlinkedItems func(DeltaChunk[T, deltaT]) (Result, error),
	onAllItems func(DeltaChunk[ManyItem[T], DeltaManyItem[T, deltaT]]) (Result, error),
) func(DeltaMany[T, deltaT]) (Result, error) {
	return func(delta DeltaMany[T, deltaT]) (Result, error) {
		var result Result
		switch delta.Discriminator {
		case "ManyLinkedItems":
			return onLinkedItems(delta.LinkedItems)
		case "ManyUnlinkedItems":
			return onUnlinkedItems(delta.UnlinkedItems)
		case "ManyAllItems":
			return onAllItems(delta.AllItems)
		}
		return result, NewInvalidDiscriminatorError(string(delta.Discriminator), "DeltaMany")
	}
}
