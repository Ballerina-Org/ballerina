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
