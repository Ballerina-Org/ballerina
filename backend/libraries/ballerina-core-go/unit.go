package ballerina

type Unit struct {
}

func NewUnit() Unit {
	var p Unit
	return p
}

var DefaultUnit Unit = NewUnit()

type DeltaUnit interface {
	DeltaBase
}
