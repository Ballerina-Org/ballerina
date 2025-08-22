package ballerina

import (
	"fmt"
	"maps"
	"slices"

	"github.com/google/uuid"
)

type Chunk[T any] struct {
	Values    []T
	IdToIndex map[uuid.UUID]int
	From      int
	To        int
	HasMore   bool
}

func NewChunk[T any](values []T, getID func(T) uuid.UUID, from, to int, hasMore bool) (Chunk[T], error) {
	idToIndex := make(map[uuid.UUID]int)
	for i, value := range values {
		idToIndex[getID(value)] = i
	}
	if len(idToIndex) != len(values) {
		return Chunk[T]{}, fmt.Errorf("duplicate ids found")
	}
	return Chunk[T]{
		Values:    values,
		IdToIndex: idToIndex,
		From:      from,
		To:        to,
		HasMore:   hasMore,
	}, nil
}

func NewChunkFromIdToIndex[T any](values []T, idToIndex map[uuid.UUID]int, from int, to int, hasMore bool) (Chunk[T], error) {
	n := len(values)
	indices := slices.Sorted(maps.Values(idToIndex))

	if !slices.Equal(indices, makeRange(n)) {
		return Chunk[T]{}, fmt.Errorf("idToIndex does not cover the exact range [0, %d), got %v", n, indices)
	}
	return Chunk[T]{
		Values:    values,
		IdToIndex: idToIndex,
		From:      from,
		To:        to,
		HasMore:   hasMore,
	}, nil
}

func MapChunk[T, U any](ts Chunk[T], f func(T) U) Chunk[U] {
	us := make([]U, len(ts.Values))
	for i := range ts.Values {
		us[i] = f(ts.Values[i])
	}
	return Chunk[U]{Values: us, IdToIndex: ts.IdToIndex, HasMore: ts.HasMore}
}
func DefaultChunk[T any]() Chunk[T] {
	return Chunk[T]{Values: make([]T, 0), IdToIndex: make(map[uuid.UUID]int), HasMore: false}
}
