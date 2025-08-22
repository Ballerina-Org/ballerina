package ballerina

import (
	"fmt"
	"maps"
	"slices"

	"github.com/google/uuid"
)

type Table[T any] struct {
	Values    []T
	IdToIndex map[uuid.UUID]int
	From      int
	To        int
	HasMore   bool
}

func NewTable[T any](values []T, getID func(T) uuid.UUID, from int, to int, hasMore bool) (Table[T], error) {
	idToIndex := make(map[uuid.UUID]int)
	for i, value := range values {
		idToIndex[getID(value)] = i
	}
	if len(idToIndex) != len(values) {
		return Table[T]{}, fmt.Errorf("duplicate ids found")
	}
	return Table[T]{
		Values:    values,
		IdToIndex: idToIndex,
		From:      from,
		To:        to,
		HasMore:   hasMore,
	}, nil
}

func makeRange(n int) []int {
	expected := make([]int, n)
	for i := range n {
		expected[i] = i
	}
	return expected
}

func NewTableFromIdToIndex[T any](values []T, idToIndex map[uuid.UUID]int, from int, to int, hasMore bool) (Table[T], error) {
	n := len(values)
	indices := slices.Sorted(maps.Values(idToIndex))

	if !slices.Equal(indices, makeRange(n)) {
		return Table[T]{}, fmt.Errorf("idToIndex does not cover the exact range [0, %d), got %v", n, indices)
	}
	return Table[T]{
		Values:    values,
		IdToIndex: idToIndex,
		From:      from,
		To:        to,
		HasMore:   hasMore,
	}, nil
}

func MapTable[T, U any](ts Table[T], f func(T) U) Table[U] {
	us := make([]U, len(ts.Values))
	for i := range ts.Values {
		us[i] = f(ts.Values[i])
	}
	return Table[U]{Values: us, IdToIndex: ts.IdToIndex, HasMore: ts.HasMore}
}
func DefaultTable[T any]() Table[T] {
	return Table[T]{Values: make([]T, 0), IdToIndex: make(map[uuid.UUID]int), From: 0, To: 0, HasMore: false}
}
