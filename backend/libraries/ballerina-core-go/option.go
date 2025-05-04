package ballerina

type Option[a any] struct {
	Sum[Unit, a] // NOTE: struct embedding is needed to be able to access Sum's JSON methods
}

// type some[a any] struct { Option[a]; Value a; Kind string }
func Some[a any](value a) Option[a] {
	return Option[a]{Right[Unit](value)}
}

// type none[a any] struct { Option[a]; Kind string }
func None[a any]() Option[a] {
	return Option[a]{Left[Unit, a](DefaultUnit)}
}

func DefaultOption[a any]() Option[a] {
	return None[a]()
}

func MatchOption[a any, c any](self Option[a], onSome func(a) c, onNone func() c) c {
	return Fold(self.Sum, func(_ Unit) c { return onNone() }, onSome)
}

func MapOption[a any, b any](self Option[a], f func(a) b) Option[b] {
	return Option[b]{BiMap(self.Sum, id, f)}
}

type DeltaOptionEffectsEnum string

const (
	OptionReplace DeltaOptionEffectsEnum = "OptionReplace"
	OptionValue   DeltaOptionEffectsEnum = "OptionValue"
)

var AllDeltaOptionEffectsEnumCases = [...]DeltaOptionEffectsEnum{OptionReplace, OptionValue}

func DefaultDeltaOptionEffectsEnum() DeltaOptionEffectsEnum { return AllDeltaOptionEffectsEnumCases[0] }

type DeltaOption[a any, deltaA any] struct {
	DeltaBase
	Discriminator DeltaOptionEffectsEnum
	Replace       a
	Value         deltaA
}

func NewDeltaOptionReplace[a any, deltaA any](value a) DeltaOption[a, deltaA] {
	return DeltaOption[a, deltaA]{
		Discriminator: OptionReplace,
		Replace:       value,
	}
}
func NewDeltaOptionValue[a any, deltaA any](delta deltaA) DeltaOption[a, deltaA] {
	return DeltaOption[a, deltaA]{
		Discriminator: OptionValue,
		Value:         delta,
	}
}
func MatchDeltaOption[a any, deltaA any, Result any](
	onReplace func(a) (Result, error),
	onValue func(deltaA) (Result, error),
) func(DeltaOption[a, deltaA]) (Result, error) {
	return func(delta DeltaOption[a, deltaA]) (Result, error) {
		var result Result
		switch delta.Discriminator {
		case "OptionReplace":
			return onReplace(delta.Replace)
		case "OptionValue":
			return onValue(delta.Value)
		}
		return result, NewInvalidDiscriminatorError(string(delta.Discriminator), "DeltaOption")
	}
}
