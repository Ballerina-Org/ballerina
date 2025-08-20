package ballerina

type Sorting string

const (
	Ascending  Sorting = "Ascending"
	Descending Sorting = "Descending"
)

var AllSortingCases = [...]Sorting{Ascending, Descending}

func DefaultSorting() Sorting { return AllSortingCases[0] }
