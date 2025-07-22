package ballerina

type ManyItem[T any] struct {
	Value         T
	IsLinked      bool
	CanChangeLink bool
}

func NewManyItem[T any](value T, isLinked bool, canChangeLink bool) ManyItem[T] {
	return ManyItem[T]{
		Value:         value,
		IsLinked:      isLinked,
		CanChangeLink: canChangeLink,
	}
}

func MapManyItem[T, U any](item ManyItem[T], f func(T) U) ManyItem[U] {
	return ManyItem[U]{
		Value:         f(item.Value),
		IsLinked:      item.IsLinked,
		CanChangeLink: item.CanChangeLink,
	}
}

func DefaultManyItem[T any](value T) ManyItem[T] {
	return ManyItem[T]{Value: value, IsLinked: false, CanChangeLink: true}
}

type DeltaManyItemEffectsEnum string

const (
	ManyItemValue         DeltaManyItemEffectsEnum = "ManyItemValue"
	ManyItemIsLinked      DeltaManyItemEffectsEnum = "ManyItemIsLinked"
	ManyItemCanChangeLink DeltaManyItemEffectsEnum = "ManyItemCanChangeLink"
)

var AllDeltaManyItemEffectsEnumCases = [...]DeltaManyItemEffectsEnum{ManyItemValue, ManyItemIsLinked, ManyItemCanChangeLink}

func DefaultDeltaManyItemEffectsEnum() DeltaManyItemEffectsEnum {
	return AllDeltaManyItemEffectsEnumCases[0]
}

type DeltaManyItem[T any, deltaT any] struct {
	DeltaBase
	Discriminator DeltaManyItemEffectsEnum
	Value         deltaT
	IsLinked      bool
	CanChangeLink bool
}

func NewDeltaManyItemValue[T any, deltaT any](delta deltaT) DeltaManyItem[T, deltaT] {
	return DeltaManyItem[T, deltaT]{
		Discriminator: ManyItemValue,
		Value:         delta,
	}
}

func NewDeltaManyItemIsLinked[T any, deltaT any](isLinked bool) DeltaManyItem[T, deltaT] {
	return DeltaManyItem[T, deltaT]{
		Discriminator: ManyItemIsLinked,
		IsLinked:      isLinked,
	}
}

func NewDeltaManyItemCanChangeLink[T any, deltaT any](canChangeLink bool) DeltaManyItem[T, deltaT] {
	return DeltaManyItem[T, deltaT]{
		Discriminator: ManyItemCanChangeLink,
		CanChangeLink: canChangeLink,
	}
}

func MatchDeltaManyItem[T any, deltaT any, Result any](
	onValue func(deltaT) (Result, error),
	onIsLinked func(bool) (Result, error),
	onCanChangeLink func(bool) (Result, error),
) func(DeltaManyItem[T, deltaT]) (Result, error) {
	return func(delta DeltaManyItem[T, deltaT]) (Result, error) {
		var result Result
		switch delta.Discriminator {
		case "ManyItemValue":
			return onValue(delta.Value)
		case "ManyItemIsLinked":
			return onIsLinked(delta.IsLinked)
		case "ManyItemCanChangeLink":
			return onCanChangeLink(delta.CanChangeLink)
		}
		return result, NewInvalidDiscriminatorError(string(delta.Discriminator), "DeltaManyItem")
	}
}
