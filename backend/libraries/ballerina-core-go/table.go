package ballerina

import (
	"fmt"
	"maps"
	"slices"
)

type Table[ID comparable, T any] struct {
	Values    []T
	IdToIndex map[ID]int
	From      int
	To        int
	HasMore   bool
}

func NewTable[ID comparable, T any](values []T, getID func(T) ID, from int, to int, hasMore bool) (Table[ID, T], error) {
	idToIndex := make(map[ID]int)
	for i, value := range values {
		idToIndex[getID(value)] = i
	}
	if len(idToIndex) != len(values) {
		return Table[ID, T]{}, fmt.Errorf("duplicate ids found")
	}
	return Table[ID, T]{
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

func NewTableFromIdToIndex[ID comparable, T any](values []T, idToIndex map[ID]int, from int, to int, hasMore bool) (Table[ID, T], error) {
	n := len(values)
	indices := slices.Sorted(maps.Values(idToIndex))

	if !slices.Equal(indices, makeRange(n)) {
		return Table[ID, T]{}, fmt.Errorf("idToIndex does not cover the exact range [0, %d), got %v", n, indices)
	}
	return Table[ID, T]{
		Values:    values,
		IdToIndex: idToIndex,
		From:      from,
		To:        to,
		HasMore:   hasMore,
	}, nil
}

func MapTable[ID comparable, T, U any](ts Table[ID, T], f func(T) U) Table[ID, U] {
	us := make([]U, len(ts.Values))
	for i := range ts.Values {
		us[i] = f(ts.Values[i])
	}
	return Table[ID, U]{Values: us, IdToIndex: ts.IdToIndex, HasMore: ts.HasMore}
}
