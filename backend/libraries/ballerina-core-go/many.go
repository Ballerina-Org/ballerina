package ballerina

type Many[T any, ID comparable] struct {
	LinkedItems   Chunk[T, ID]
	UnlinkedItems Chunk[T, ID]
	AllItems      Chunk[ManyItem[T], ID]
}

func NewMany[T any, ID comparable](linkedItems Chunk[T, ID], unlinkedItems Chunk[T, ID], allItems Chunk[ManyItem[T], ID]) Many[T, ID] {
	return Many[T, ID]{
		LinkedItems:   linkedItems,
		UnlinkedItems: unlinkedItems,
		AllItems:      allItems,
	}
}

func MapMany[T, U any, ID comparable](m Many[T, ID], f func(T) U) Many[U, ID] {
	return Many[U, ID]{
		LinkedItems:   MapChunk(m.LinkedItems, f),
		UnlinkedItems: MapChunk(m.UnlinkedItems, f),
		AllItems: MapChunk(m.AllItems, func(item ManyItem[T]) ManyItem[U] {
			return MapManyItem(item, f)
		}),
	}
}
