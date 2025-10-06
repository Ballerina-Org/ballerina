package ballerina

import (
	"fmt"
	"maps"
	"slices"
)

type Chunk[T any, ID comparable] struct {
	Values    []T
	IdToIndex map[ID]int
	From      int
	To        int
	HasMore   bool
}

func NewChunk[T any, ID comparable](values []T, getID func(T) ID, from, to int, hasMore bool) (Chunk[T, ID], error) {
	idToIndex := make(map[ID]int)
	for i, value := range values {
		idToIndex[getID(value)] = i
	}
	if len(idToIndex) != len(values) {
		return Chunk[T, ID]{}, fmt.Errorf("duplicate ids found")
	}
	return Chunk[T, ID]{
		Values:    values,
		IdToIndex: idToIndex,
		From:      from,
		To:        to,
		HasMore:   hasMore,
	}, nil
}

func NewChunkFromIdToIndex[T any, ID comparable](values []T, idToIndex map[ID]int, from int, to int, hasMore bool) (Chunk[T, ID], error) {
	n := len(values)
	indices := slices.Sorted(maps.Values(idToIndex))

	if !slices.Equal(indices, makeRange(n)) {
		return Chunk[T, ID]{}, fmt.Errorf("idToIndex does not cover the exact range [0, %d), got %v", n, indices)
	}
	return Chunk[T, ID]{
		Values:    values,
		IdToIndex: idToIndex,
		From:      from,
		To:        to,
		HasMore:   hasMore,
	}, nil
}

func MapChunk[T, U any, ID comparable](ts Chunk[T, ID], f func(T) U) Chunk[U, ID] {
	us := make([]U, len(ts.Values))
	for i := range ts.Values {
		us[i] = f(ts.Values[i])
	}
	return Chunk[U, ID]{Values: us, IdToIndex: ts.IdToIndex, HasMore: ts.HasMore}
}
