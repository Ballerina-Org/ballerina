package ballerina

type One[a any] struct {
	Option[a] // NOTE: struct embedding is needed to be able to access Sum's JSON methods
}

// type some[a any] struct { One[a]; Value a; Kind string }
func Loaded[a any](value a) One[a] {
	return One[a]{Some[a](value)}
}

// type none[a any] struct { One[a]; Kind string }
func Lazy[a any]() One[a] {
	return One[a]{None[a]()}
}

func DefaultOne[a any]() One[a] {
	return Lazy[a]()
}

// func MatchOne[a any, c any](self One[a], onSome func(a) c, onNone func() c) c {
// 	return Fold(self.Sum, func(_ Unit) c { return onNone() }, onSome)
// }

func MapOne[a any, b any](self One[a], f func(a) b) One[b] {
	return Fold(self.Sum, func(_ Unit) One[b] { return Lazy[b]() }, func(value a) One[b] { return Loaded[b](f(value)) })
}

type DeltaOneEffectsEnum string

const (
	OneReplace     DeltaOneEffectsEnum = "OneReplace"
	OneValue       DeltaOneEffectsEnum = "OneValue"
	OneCreateValue DeltaOneEffectsEnum = "OneCreateValue"
	OneDeleteValue DeltaOneEffectsEnum = "OneDeleteValue"
)

var AllDeltaOneEffectsEnumCases = [...]DeltaOneEffectsEnum{OneReplace, OneValue, OneCreateValue, OneDeleteValue}

func DefaultDeltaOneEffectsEnum() DeltaOneEffectsEnum { return AllDeltaOneEffectsEnumCases[0] }

type DeltaOne[a any, deltaA any] struct {
	DeltaBase
	Discriminator DeltaOneEffectsEnum
	Replace       a
	Value         deltaA
	CreateValue   a
	DeleteValue   Unit
}

func NewDeltaOneReplace[a any, deltaA any](value a) DeltaOne[a, deltaA] {
	return DeltaOne[a, deltaA]{
		Discriminator: OneReplace,
		Replace:       value,
	}
}
func NewDeltaOneValue[a any, deltaA any](delta deltaA) DeltaOne[a, deltaA] {
	return DeltaOne[a, deltaA]{
		Discriminator: OneValue,
		Value:         delta,
	}
}
func NewDeltaOneCreateValue[a any, deltaA any](value a) DeltaOne[a, deltaA] {
	return DeltaOne[a, deltaA]{
		Discriminator: OneCreateValue,
		CreateValue:   value,
	}
}
func NewDeltaOneDeleteValue[a any, deltaA any]() DeltaOne[a, deltaA] {
	return DeltaOne[a, deltaA]{
		Discriminator: OneCreateValue,
		DeleteValue:   NewUnit(),
	}
}
func MatchDeltaOne[a any, deltaA any, Result any](
	onReplace func(a) (Result, error),
	onValue func(deltaA) (Result, error),
	onCreateValue func(a) (Result, error),
	onDeleteValue func() (Result, error),
) func(DeltaOne[a, deltaA]) (Result, error) {
	return func(delta DeltaOne[a, deltaA]) (Result, error) {
		var result Result
		switch delta.Discriminator {
		case "OneReplace":
			return onReplace(delta.Replace)
		case "OneValue":
			return onValue(delta.Value)
		case "OneCreateValue":
			return onCreateValue(delta.CreateValue)
		case "OneDeleteValue":
			return onDeleteValue()
		}
		return result, NewInvalidDiscriminatorError(string(delta.Discriminator), "DeltaOne")
	}
}
