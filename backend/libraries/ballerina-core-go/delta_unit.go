package ballerina

type DeltaUnit Unit

func NewDeltaUnit() DeltaUnit {
	return DeltaUnit(NewUnit())
}
