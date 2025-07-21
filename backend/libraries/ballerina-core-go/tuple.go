package ballerina

type Tuple2[a any, b any] struct {
	Item1 a
	Item2 b
}

func NewTuple2[a any, b any](item1 a, item2 b) Tuple2[a, b] {
	var p Tuple2[a, b]
	p.Item1 = item1
	p.Item2 = item2
	return p
}

type Tuple3[a any, b any, c any] struct {
	Item1 a
	Item2 b
	Item3 c
}

func NewTuple3[a any, b any, c any](item1 a, item2 b, item3 c) Tuple3[a, b, c] {
	var p Tuple3[a, b, c]
	p.Item1 = item1
	p.Item2 = item2
	p.Item3 = item3
	return p
}

type Tuple4[a any, b any, c any, d any] struct {
	Item1 a
	Item2 b
	Item3 c
	Item4 d
}

func NewTuple4[a any, b any, c any, d any](item1 a, item2 b, item3 c, item4 d) Tuple4[a, b, c, d] {
	var p Tuple4[a, b, c, d]
	p.Item1 = item1
	p.Item2 = item2
	p.Item3 = item3
	p.Item4 = item4
	return p
}

type Tuple5[a any, b any, c any, d any, e any] struct {
	Item1 a
	Item2 b
	Item3 c
	Item4 d
	Item5 e
}

func NewTuple5[a any, b any, c any, d any, e any](item1 a, item2 b, item3 c, item4 d, item5 e) Tuple5[a, b, c, d, e] {
	var p Tuple5[a, b, c, d, e]
	p.Item1 = item1
	p.Item2 = item2
	p.Item3 = item3
	p.Item4 = item4
	p.Item5 = item5
	return p
}

type Tuple6[a any, b any, c any, d any, e any, f any] struct {
	Item1 a
	Item2 b
	Item3 c
	Item4 d
	Item5 e
	Item6 f
}

func NewTuple6[a any, b any, c any, d any, e any, f any](item1 a, item2 b, item3 c, item4 d, item5 e, item6 f) Tuple6[a, b, c, d, e, f] {
	var p Tuple6[a, b, c, d, e, f]
	p.Item1 = item1
	p.Item2 = item2
	p.Item3 = item3
	p.Item4 = item4
	p.Item5 = item5
	p.Item6 = item6
	return p
}

type Tuple7[a any, b any, c any, d any, e any, f any, g any] struct {
	Item1 a
	Item2 b
	Item3 c
	Item4 d
	Item5 e
	Item6 f
	Item7 g
}

func NewTuple7[a any, b any, c any, d any, e any, f any, g any](item1 a, item2 b, item3 c, item4 d, item5 e, item6 f, item7 g) Tuple7[a, b, c, d, e, f, g] {
	var p Tuple7[a, b, c, d, e, f, g]
	p.Item1 = item1
	p.Item2 = item2
	p.Item3 = item3
	p.Item4 = item4
	p.Item5 = item5
	p.Item6 = item6
	p.Item7 = item7
	return p
}

type Tuple8[a any, b any, c any, d any, e any, f any, g any, h any] struct {
	Item1 a
	Item2 b
	Item3 c
	Item4 d
	Item5 e
	Item6 f
	Item7 g
	Item8 h
}

func NewTuple8[a any, b any, c any, d any, e any, f any, g any, h any](item1 a, item2 b, item3 c, item4 d, item5 e, item6 f, item7 g, item8 h) Tuple8[a, b, c, d, e, f, g, h] {
	var p Tuple8[a, b, c, d, e, f, g, h]
	p.Item1 = item1
	p.Item2 = item2
	p.Item3 = item3
	p.Item4 = item4
	p.Item5 = item5
	p.Item6 = item6
	p.Item7 = item7
	p.Item8 = item8
	return p
}

type Tuple9[a any, b any, c any, d any, e any, f any, g any, h any, i any] struct {
	Item1 a
	Item2 b
	Item3 c
	Item4 d
	Item5 e
	Item6 f
	Item7 g
	Item8 h
	Item9 i
}

func NewTuple9[a any, b any, c any, d any, e any, f any, g any, h any, i any](item1 a, item2 b, item3 c, item4 d, item5 e, item6 f, item7 g, item8 h, item9 i) Tuple9[a, b, c, d, e, f, g, h, i] {
	var p Tuple9[a, b, c, d, e, f, g, h, i]
	p.Item1 = item1
	p.Item2 = item2
	p.Item3 = item3
	p.Item4 = item4
	p.Item5 = item5
	p.Item6 = item6
	p.Item7 = item7
	p.Item8 = item8
	p.Item9 = item9
	return p
}

type DeltaTuple2EffectsEnum string

const (
	Tuple2Item1 DeltaTuple2EffectsEnum = "Tuple2Item1"
	Tuple2Item2 DeltaTuple2EffectsEnum = "Tuple2Item2"
)

var AllDeltaTuple2EffectsEnumCases = [...]DeltaTuple2EffectsEnum{Tuple2Item1, Tuple2Item2}

func DefaultDeltaTuple2EffectsEnum() DeltaTuple2EffectsEnum { return AllDeltaTuple2EffectsEnumCases[0] }

type DeltaTuple2[a any, b any, deltaA any, deltaB any] struct {
	DeltaBase
	Discriminator DeltaTuple2EffectsEnum
	Item1         deltaA
	Item2         deltaB
}

func NewDeltaTuple2Item1[a any, b any, deltaA any, deltaB any](delta deltaA) DeltaTuple2[a, b, deltaA, deltaB] {
	return DeltaTuple2[a, b, deltaA, deltaB]{
		Discriminator: Tuple2Item1,
		Item1:         delta,
	}
}
func NewDeltaTuple2Item2[a any, b any, deltaA any, deltaB any](delta deltaB) DeltaTuple2[a, b, deltaA, deltaB] {
	return DeltaTuple2[a, b, deltaA, deltaB]{
		Discriminator: Tuple2Item2,
		Item2:         delta,
	}
}
func MatchDeltaTuple2[a any, b any, deltaA any, deltaB any, Result any](
	onItem1 func(deltaA) (Result, error),
	onItem2 func(deltaB) (Result, error),
) func(DeltaTuple2[a, b, deltaA, deltaB]) (Result, error) {
	return func(delta DeltaTuple2[a, b, deltaA, deltaB]) (Result, error) {
		var result Result
		switch delta.Discriminator {
		case "Tuple2Item1":
			return onItem1(delta.Item1)
		case "Tuple2Item2":
			return onItem2(delta.Item2)
		}
		return result, NewInvalidDiscriminatorError(string(delta.Discriminator), "DeltaTuple2")
	}
}

type DeltaTuple3EffectsEnum string

const (
	Tuple3Item1 DeltaTuple3EffectsEnum = "Tuple3Item1"
	Tuple3Item2 DeltaTuple3EffectsEnum = "Tuple3Item2"
	Tuple3Item3 DeltaTuple3EffectsEnum = "Tuple3Item3"
)

var AllDeltaTuple3EffectsEnumCases = [...]DeltaTuple3EffectsEnum{Tuple3Item1, Tuple3Item2, Tuple3Item3}

func DefaultDeltaTuple3EffectsEnum() DeltaTuple3EffectsEnum { return AllDeltaTuple3EffectsEnumCases[0] }

type DeltaTuple3[a any, b any, c any, deltaA any, deltaB any, deltaC any] struct {
	DeltaBase
	Discriminator DeltaTuple3EffectsEnum
	Item1         deltaA
	Item2         deltaB
	Item3         deltaC
}

func NewDeltaTuple3Item1[a any, b any, c any, deltaA any, deltaB any, deltaC any](delta deltaA) DeltaTuple3[a, b, c, deltaA, deltaB, deltaC] {
	return DeltaTuple3[a, b, c, deltaA, deltaB, deltaC]{
		Discriminator: Tuple3Item1,
		Item1:         delta,
	}
}
func NewDeltaTuple3Item2[a any, b any, c any, deltaA any, deltaB any, deltaC any](delta deltaB) DeltaTuple3[a, b, c, deltaA, deltaB, deltaC] {
	return DeltaTuple3[a, b, c, deltaA, deltaB, deltaC]{
		Discriminator: Tuple3Item2,
		Item2:         delta,
	}
}
func NewDeltaTuple3Item3[a any, b any, c any, deltaA any, deltaB any, deltaC any](delta deltaC) DeltaTuple3[a, b, c, deltaA, deltaB, deltaC] {
	return DeltaTuple3[a, b, c, deltaA, deltaB, deltaC]{
		Discriminator: Tuple3Item3,
		Item3:         delta,
	}
}
func MatchDeltaTuple3[a any, b any, c any, deltaA any, deltaB any, deltaC any, Result any](
	onItem1 func(deltaA) (Result, error),
	onItem2 func(deltaB) (Result, error),
	onItem3 func(deltaC) (Result, error),
) func(DeltaTuple3[a, b, c, deltaA, deltaB, deltaC]) (Result, error) {
	return func(delta DeltaTuple3[a, b, c, deltaA, deltaB, deltaC]) (Result, error) {
		var result Result
		switch delta.Discriminator {
		case "Tuple3Item1":
			return onItem1(delta.Item1)
		case "Tuple3Item2":
			return onItem2(delta.Item2)
		case "Tuple3Item3":
			return onItem3(delta.Item3)
		}
		return result, NewInvalidDiscriminatorError(string(delta.Discriminator), "DeltaTuple3")
	}
}

type DeltaTuple4EffectsEnum string

const (
	Tuple4Item1 DeltaTuple4EffectsEnum = "Tuple4Item1"
	Tuple4Item2 DeltaTuple4EffectsEnum = "Tuple4Item2"
	Tuple4Item3 DeltaTuple4EffectsEnum = "Tuple4Item3"
	Tuple4Item4 DeltaTuple4EffectsEnum = "Tuple4Item4"
)

var AllDeltaTuple4EffectsEnumCases = [...]DeltaTuple4EffectsEnum{Tuple4Item1, Tuple4Item2, Tuple4Item3, Tuple4Item4}

func DefaultDeltaTuple4EffectsEnum() DeltaTuple4EffectsEnum { return AllDeltaTuple4EffectsEnumCases[0] }

type DeltaTuple4[a any, b any, c any, d any, deltaA any, deltaB any, deltaC any, deltaD any] struct {
	DeltaBase
	Discriminator DeltaTuple4EffectsEnum
	Item1         deltaA
	Item2         deltaB
	Item3         deltaC
	Item4         deltaD
}

func NewDeltaTuple4Item1[a any, b any, c any, d any, deltaA any, deltaB any, deltaC any, deltaD any](delta deltaA) DeltaTuple4[a, b, c, d, deltaA, deltaB, deltaC, deltaD] {
	return DeltaTuple4[a, b, c, d, deltaA, deltaB, deltaC, deltaD]{
		Discriminator: Tuple4Item1,
		Item1:         delta,
	}
}
func NewDeltaTuple4Item2[a any, b any, c any, d any, deltaA any, deltaB any, deltaC any, deltaD any](delta deltaB) DeltaTuple4[a, b, c, d, deltaA, deltaB, deltaC, deltaD] {
	return DeltaTuple4[a, b, c, d, deltaA, deltaB, deltaC, deltaD]{
		Discriminator: Tuple4Item2,
		Item2:         delta,
	}
}
func NewDeltaTuple4Item3[a any, b any, c any, d any, deltaA any, deltaB any, deltaC any, deltaD any](delta deltaC) DeltaTuple4[a, b, c, d, deltaA, deltaB, deltaC, deltaD] {
	return DeltaTuple4[a, b, c, d, deltaA, deltaB, deltaC, deltaD]{
		Discriminator: Tuple4Item3,
		Item3:         delta,
	}
}
func NewDeltaTuple4Item4[a any, b any, c any, d any, deltaA any, deltaB any, deltaC any, deltaD any](delta deltaD) DeltaTuple4[a, b, c, d, deltaA, deltaB, deltaC, deltaD] {
	return DeltaTuple4[a, b, c, d, deltaA, deltaB, deltaC, deltaD]{
		Discriminator: Tuple4Item4,
		Item4:         delta,
	}
}
func MatchDeltaTuple4[a any, b any, c any, d any, deltaA any, deltaB any, deltaC any, deltaD any, Result any](
	onItem1 func(deltaA) (Result, error),
	onItem2 func(deltaB) (Result, error),
	onItem3 func(deltaC) (Result, error),
	onItem4 func(deltaD) (Result, error),
) func(DeltaTuple4[a, b, c, d, deltaA, deltaB, deltaC, deltaD]) (Result, error) {
	return func(delta DeltaTuple4[a, b, c, d, deltaA, deltaB, deltaC, deltaD]) (Result, error) {
		var result Result
		switch delta.Discriminator {
		case "Tuple4Item1":
			return onItem1(delta.Item1)
		case "Tuple4Item2":
			return onItem2(delta.Item2)
		case "Tuple4Item3":
			return onItem3(delta.Item3)
		case "Tuple4Item4":
			return onItem4(delta.Item4)
		}
		return result, NewInvalidDiscriminatorError(string(delta.Discriminator), "DeltaTuple4")
	}
}

type DeltaTuple5EffectsEnum string

const (
	Tuple5Item1 DeltaTuple5EffectsEnum = "Tuple5Item1"
	Tuple5Item2 DeltaTuple5EffectsEnum = "Tuple5Item2"
	Tuple5Item3 DeltaTuple5EffectsEnum = "Tuple5Item3"
	Tuple5Item4 DeltaTuple5EffectsEnum = "Tuple5Item4"
	Tuple5Item5 DeltaTuple5EffectsEnum = "Tuple5Item5"
)

var AllDeltaTuple5EffectsEnumCases = [...]DeltaTuple5EffectsEnum{Tuple5Item1, Tuple5Item2, Tuple5Item3, Tuple5Item4, Tuple5Item5}

func DefaultDeltaTuple5EffectsEnum() DeltaTuple5EffectsEnum { return AllDeltaTuple5EffectsEnumCases[0] }

type DeltaTuple5[a any, b any, c any, d any, e any, deltaA any, deltaB any, deltaC any, deltaD any, deltaE any] struct {
	DeltaBase
	Discriminator DeltaTuple5EffectsEnum
	Item1         deltaA
	Item2         deltaB
	Item3         deltaC
	Item4         deltaD
	Item5         deltaE
}

func NewDeltaTuple5Item1[a any, b any, c any, d any, e any, deltaA any, deltaB any, deltaC any, deltaD any, deltaE any](delta deltaA) DeltaTuple5[a, b, c, d, e, deltaA, deltaB, deltaC, deltaD, deltaE] {
	return DeltaTuple5[a, b, c, d, e, deltaA, deltaB, deltaC, deltaD, deltaE]{
		Discriminator: Tuple5Item1,
		Item1:         delta,
	}
}
func NewDeltaTuple5Item2[a any, b any, c any, d any, e any, deltaA any, deltaB any, deltaC any, deltaD any, deltaE any](delta deltaB) DeltaTuple5[a, b, c, d, e, deltaA, deltaB, deltaC, deltaD, deltaE] {
	return DeltaTuple5[a, b, c, d, e, deltaA, deltaB, deltaC, deltaD, deltaE]{
		Discriminator: Tuple5Item2,
		Item2:         delta,
	}
}
func NewDeltaTuple5Item3[a any, b any, c any, d any, e any, deltaA any, deltaB any, deltaC any, deltaD any, deltaE any](delta deltaC) DeltaTuple5[a, b, c, d, e, deltaA, deltaB, deltaC, deltaD, deltaE] {
	return DeltaTuple5[a, b, c, d, e, deltaA, deltaB, deltaC, deltaD, deltaE]{
		Discriminator: Tuple5Item3,
		Item3:         delta,
	}
}
func NewDeltaTuple5Item4[a any, b any, c any, d any, e any, deltaA any, deltaB any, deltaC any, deltaD any, deltaE any](delta deltaD) DeltaTuple5[a, b, c, d, e, deltaA, deltaB, deltaC, deltaD, deltaE] {
	return DeltaTuple5[a, b, c, d, e, deltaA, deltaB, deltaC, deltaD, deltaE]{
		Discriminator: Tuple5Item4,
		Item4:         delta,
	}
}
func NewDeltaTuple5Item5[a any, b any, c any, d any, e any, deltaA any, deltaB any, deltaC any, deltaD any, deltaE any](delta deltaE) DeltaTuple5[a, b, c, d, e, deltaA, deltaB, deltaC, deltaD, deltaE] {
	return DeltaTuple5[a, b, c, d, e, deltaA, deltaB, deltaC, deltaD, deltaE]{
		Discriminator: Tuple5Item5,
		Item5:         delta,
	}
}
func MatchDeltaTuple5[a any, b any, c any, d any, e any, deltaA any, deltaB any, deltaC any, deltaD any, deltaE any, Result any](
	onItem1 func(deltaA) (Result, error),
	onItem2 func(deltaB) (Result, error),
	onItem3 func(deltaC) (Result, error),
	onItem4 func(deltaD) (Result, error),
	onItem5 func(deltaE) (Result, error),
) func(DeltaTuple5[a, b, c, d, e, deltaA, deltaB, deltaC, deltaD, deltaE]) (Result, error) {
	return func(delta DeltaTuple5[a, b, c, d, e, deltaA, deltaB, deltaC, deltaD, deltaE]) (Result, error) {
		var result Result
		switch delta.Discriminator {
		case "Tuple5Item1":
			return onItem1(delta.Item1)
		case "Tuple5Item2":
			return onItem2(delta.Item2)
		case "Tuple5Item3":
			return onItem3(delta.Item3)
		case "Tuple5Item4":
			return onItem4(delta.Item4)
		case "Tuple5Item5":
			return onItem5(delta.Item5)
		}
		return result, NewInvalidDiscriminatorError(string(delta.Discriminator), "DeltaTuple5")
	}
}

type DeltaTuple6EffectsEnum string

const (
	Tuple6Item1 DeltaTuple6EffectsEnum = "Tuple6Item1"
	Tuple6Item2 DeltaTuple6EffectsEnum = "Tuple6Item2"
	Tuple6Item3 DeltaTuple6EffectsEnum = "Tuple6Item3"
	Tuple6Item4 DeltaTuple6EffectsEnum = "Tuple6Item4"
	Tuple6Item5 DeltaTuple6EffectsEnum = "Tuple6Item5"
	Tuple6Item6 DeltaTuple6EffectsEnum = "Tuple6Item6"
)

var AllDeltaTuple6EffectsEnumCases = [...]DeltaTuple6EffectsEnum{Tuple6Item1, Tuple6Item2, Tuple6Item3, Tuple6Item4, Tuple6Item5, Tuple6Item6}

func DefaultDeltaTuple6EffectsEnum() DeltaTuple6EffectsEnum { return AllDeltaTuple6EffectsEnumCases[0] }

type DeltaTuple6[a any, b any, c any, d any, e any, f any, deltaA any, deltaB any, deltaC any, deltaD any, deltaE any, deltaF any] struct {
	DeltaBase
	Discriminator DeltaTuple6EffectsEnum
	Item1         deltaA
	Item2         deltaB
	Item3         deltaC
	Item4         deltaD
	Item5         deltaE
	Item6         deltaF
}

func NewDeltaTuple6Item1[a any, b any, c any, d any, e any, f any, deltaA any, deltaB any, deltaC any, deltaD any, deltaE any, deltaF any](delta deltaA) DeltaTuple6[a, b, c, d, e, f, deltaA, deltaB, deltaC, deltaD, deltaE, deltaF] {
	return DeltaTuple6[a, b, c, d, e, f, deltaA, deltaB, deltaC, deltaD, deltaE, deltaF]{
		Discriminator: Tuple6Item1,
		Item1:         delta,
	}
}
func NewDeltaTuple6Item2[a any, b any, c any, d any, e any, f any, deltaA any, deltaB any, deltaC any, deltaD any, deltaE any, deltaF any](delta deltaB) DeltaTuple6[a, b, c, d, e, f, deltaA, deltaB, deltaC, deltaD, deltaE, deltaF] {
	return DeltaTuple6[a, b, c, d, e, f, deltaA, deltaB, deltaC, deltaD, deltaE, deltaF]{
		Discriminator: Tuple6Item2,
		Item2:         delta,
	}
}
func NewDeltaTuple6Item3[a any, b any, c any, d any, e any, f any, deltaA any, deltaB any, deltaC any, deltaD any, deltaE any, deltaF any](delta deltaC) DeltaTuple6[a, b, c, d, e, f, deltaA, deltaB, deltaC, deltaD, deltaE, deltaF] {
	return DeltaTuple6[a, b, c, d, e, f, deltaA, deltaB, deltaC, deltaD, deltaE, deltaF]{
		Discriminator: Tuple6Item3,
		Item3:         delta,
	}
}
func NewDeltaTuple6Item4[a any, b any, c any, d any, e any, f any, deltaA any, deltaB any, deltaC any, deltaD any, deltaE any, deltaF any](delta deltaD) DeltaTuple6[a, b, c, d, e, f, deltaA, deltaB, deltaC, deltaD, deltaE, deltaF] {
	return DeltaTuple6[a, b, c, d, e, f, deltaA, deltaB, deltaC, deltaD, deltaE, deltaF]{
		Discriminator: Tuple6Item4,
		Item4:         delta,
	}
}
func NewDeltaTuple6Item5[a any, b any, c any, d any, e any, f any, deltaA any, deltaB any, deltaC any, deltaD any, deltaE any, deltaF any](delta deltaE) DeltaTuple6[a, b, c, d, e, f, deltaA, deltaB, deltaC, deltaD, deltaE, deltaF] {
	return DeltaTuple6[a, b, c, d, e, f, deltaA, deltaB, deltaC, deltaD, deltaE, deltaF]{
		Discriminator: Tuple6Item5,
		Item5:         delta,
	}
}
func NewDeltaTuple6Item6[a any, b any, c any, d any, e any, f any, deltaA any, deltaB any, deltaC any, deltaD any, deltaE any, deltaF any](delta deltaF) DeltaTuple6[a, b, c, d, e, f, deltaA, deltaB, deltaC, deltaD, deltaE, deltaF] {
	return DeltaTuple6[a, b, c, d, e, f, deltaA, deltaB, deltaC, deltaD, deltaE, deltaF]{
		Discriminator: Tuple6Item6,
		Item6:         delta,
	}
}
func MatchDeltaTuple6[a any, b any, c any, d any, e any, f any, deltaA any, deltaB any, deltaC any, deltaD any, deltaE any, deltaF any, Result any](
	onItem1 func(deltaA) (Result, error),
	onItem2 func(deltaB) (Result, error),
	onItem3 func(deltaC) (Result, error),
	onItem4 func(deltaD) (Result, error),
	onItem5 func(deltaE) (Result, error),
	onItem6 func(deltaF) (Result, error),
) func(DeltaTuple6[a, b, c, d, e, f, deltaA, deltaB, deltaC, deltaD, deltaE, deltaF]) (Result, error) {
	return func(delta DeltaTuple6[a, b, c, d, e, f, deltaA, deltaB, deltaC, deltaD, deltaE, deltaF]) (Result, error) {
		var result Result
		switch delta.Discriminator {
		case "Tuple6Item1":
			return onItem1(delta.Item1)
		case "Tuple6Item2":
			return onItem2(delta.Item2)
		case "Tuple6Item3":
			return onItem3(delta.Item3)
		case "Tuple6Item4":
			return onItem4(delta.Item4)
		case "Tuple6Item5":
			return onItem5(delta.Item5)
		case "Tuple6Item6":
			return onItem6(delta.Item6)
		}
		return result, NewInvalidDiscriminatorError(string(delta.Discriminator), "DeltaTuple6")
	}
}

type DeltaTuple7EffectsEnum string

const (
	Tuple7Item1 DeltaTuple7EffectsEnum = "Tuple7Item1"
	Tuple7Item2 DeltaTuple7EffectsEnum = "Tuple7Item2"
	Tuple7Item3 DeltaTuple7EffectsEnum = "Tuple7Item3"
	Tuple7Item4 DeltaTuple7EffectsEnum = "Tuple7Item4"
	Tuple7Item5 DeltaTuple7EffectsEnum = "Tuple7Item5"
	Tuple7Item6 DeltaTuple7EffectsEnum = "Tuple7Item6"
	Tuple7Item7 DeltaTuple7EffectsEnum = "Tuple7Item7"
)

var AllDeltaTuple7EffectsEnumCases = [...]DeltaTuple7EffectsEnum{Tuple7Item1, Tuple7Item2, Tuple7Item3, Tuple7Item4, Tuple7Item5, Tuple7Item6}

func DefaultDeltaTuple7EffectsEnum() DeltaTuple7EffectsEnum { return AllDeltaTuple7EffectsEnumCases[0] }

type DeltaTuple7[a any, b any, c any, d any, e any, f any, g any, deltaA any, deltaB any, deltaC any, deltaD any, deltaE any, deltaF any, deltaG any] struct {
	DeltaBase
	Discriminator DeltaTuple7EffectsEnum
	Item1         deltaA
	Item2         deltaB
	Item3         deltaC
	Item4         deltaD
	Item5         deltaE
	Item6         deltaF
	Item7         deltaG
}

func NewDeltaTuple7Item1[a any, b any, c any, d any, e any, f any, g any, deltaA any, deltaB any, deltaC any, deltaD any, deltaE any, deltaF any, deltaG any](delta deltaA) DeltaTuple7[a, b, c, d, e, f, g, deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG] {
	return DeltaTuple7[a, b, c, d, e, f, g, deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG]{
		Discriminator: Tuple7Item1,
		Item1:         delta,
	}
}
func NewDeltaTuple7Item2[a any, b any, c any, d any, e any, f any, g any, deltaA any, deltaB any, deltaC any, deltaD any, deltaE any, deltaF any, deltaG any](delta deltaB) DeltaTuple7[a, b, c, d, e, f, g, deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG] {
	return DeltaTuple7[a, b, c, d, e, f, g, deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG]{
		Discriminator: Tuple7Item2,
		Item2:         delta,
	}
}
func NewDeltaTuple7Item3[a any, b any, c any, d any, e any, f any, g any, deltaA any, deltaB any, deltaC any, deltaD any, deltaE any, deltaF any, deltaG any](delta deltaC) DeltaTuple7[a, b, c, d, e, f, g, deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG] {
	return DeltaTuple7[a, b, c, d, e, f, g, deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG]{
		Discriminator: Tuple7Item3,
		Item3:         delta,
	}
}
func NewDeltaTuple7Item4[a any, b any, c any, d any, e any, f any, g any, deltaA any, deltaB any, deltaC any, deltaD any, deltaE any, deltaF any, deltaG any](delta deltaD) DeltaTuple7[a, b, c, d, e, f, g, deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG] {
	return DeltaTuple7[a, b, c, d, e, f, g, deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG]{
		Discriminator: Tuple7Item4,
		Item4:         delta,
	}
}
func NewDeltaTuple7Item5[a any, b any, c any, d any, e any, f any, g any, deltaA any, deltaB any, deltaC any, deltaD any, deltaE any, deltaF any, deltaG any](delta deltaE) DeltaTuple7[a, b, c, d, e, f, g, deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG] {
	return DeltaTuple7[a, b, c, d, e, f, g, deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG]{
		Discriminator: Tuple7Item5,
		Item5:         delta,
	}
}
func NewDeltaTuple7Item6[a any, b any, c any, d any, e any, f any, g any, deltaA any, deltaB any, deltaC any, deltaD any, deltaE any, deltaF any, deltaG any](delta deltaF) DeltaTuple7[a, b, c, d, e, f, g, deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG] {
	return DeltaTuple7[a, b, c, d, e, f, g, deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG]{
		Discriminator: Tuple7Item6,
		Item6:         delta,
	}
}
func NewDeltaTuple7Item7[a any, b any, c any, d any, e any, f any, g any, deltaA any, deltaB any, deltaC any, deltaD any, deltaE any, deltaF any, deltaG any](delta deltaG) DeltaTuple7[a, b, c, d, e, f, g, deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG] {
	return DeltaTuple7[a, b, c, d, e, f, g, deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG]{
		Discriminator: Tuple7Item7,
		Item7:         delta,
	}
}
func MatchDeltaTuple7[a any, b any, c any, d any, e any, f any, g any, deltaA any, deltaB any, deltaC any, deltaD any, deltaE any, deltaF any, deltaG any, Result any](
	onItem1 func(deltaA) (Result, error),
	onItem2 func(deltaB) (Result, error),
	onItem3 func(deltaC) (Result, error),
	onItem4 func(deltaD) (Result, error),
	onItem5 func(deltaE) (Result, error),
	onItem6 func(deltaF) (Result, error),
	onItem7 func(deltaG) (Result, error),
) func(DeltaTuple7[a, b, c, d, e, f, g, deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG]) (Result, error) {
	return func(delta DeltaTuple7[a, b, c, d, e, f, g, deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG]) (Result, error) {
		var result Result
		switch delta.Discriminator {
		case "Tuple7Item1":
			return onItem1(delta.Item1)
		case "Tuple7Item2":
			return onItem2(delta.Item2)
		case "Tuple7Item3":
			return onItem3(delta.Item3)
		case "Tuple7Item4":
			return onItem4(delta.Item4)
		case "Tuple7Item5":
			return onItem5(delta.Item5)
		case "Tuple7Item6":
			return onItem6(delta.Item6)
		case "Tuple7Item7":
			return onItem7(delta.Item7)
		}
		return result, NewInvalidDiscriminatorError(string(delta.Discriminator), "DeltaTuple7")
	}
}

type DeltaTuple8EffectsEnum string

const (
	Tuple8Item1 DeltaTuple8EffectsEnum = "Tuple8Item1"
	Tuple8Item2 DeltaTuple8EffectsEnum = "Tuple8Item2"
	Tuple8Item3 DeltaTuple8EffectsEnum = "Tuple8Item3"
	Tuple8Item4 DeltaTuple8EffectsEnum = "Tuple8Item4"
	Tuple8Item5 DeltaTuple8EffectsEnum = "Tuple8Item5"
	Tuple8Item6 DeltaTuple8EffectsEnum = "Tuple8Item6"
	Tuple8Item7 DeltaTuple8EffectsEnum = "Tuple8Item7"
	Tuple8Item8 DeltaTuple8EffectsEnum = "Tuple8Item8"
)

var AllDeltaTuple8EffectsEnumCases = [...]DeltaTuple8EffectsEnum{Tuple8Item1, Tuple8Item2, Tuple8Item3, Tuple8Item4, Tuple8Item5, Tuple8Item6}

func DefaultDeltaTuple8EffectsEnum() DeltaTuple8EffectsEnum { return AllDeltaTuple8EffectsEnumCases[0] }

type DeltaTuple8[a any, b any, c any, d any, e any, f any, g any, h any, deltaA any, deltaB any, deltaC any, deltaD any, deltaE any, deltaF any, deltaG any, deltaH any] struct {
	DeltaBase
	Discriminator DeltaTuple8EffectsEnum
	Item1         deltaA
	Item2         deltaB
	Item3         deltaC
	Item4         deltaD
	Item5         deltaE
	Item6         deltaF
	Item7         deltaG
	Item8         deltaH
}

func NewDeltaTuple8Item1[a any, b any, c any, d any, e any, f any, g any, h any, deltaA any, deltaB any, deltaC any, deltaD any, deltaE any, deltaF any, deltaG any, deltaH any](delta deltaA) DeltaTuple8[a, b, c, d, e, f, g, h, deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG, deltaH] {
	return DeltaTuple8[a, b, c, d, e, f, g, h, deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG, deltaH]{
		Discriminator: Tuple8Item1,
		Item1:         delta,
	}
}
func NewDeltaTuple8Item2[a any, b any, c any, d any, e any, f any, g any, h any, deltaA any, deltaB any, deltaC any, deltaD any, deltaE any, deltaF any, deltaG any, deltaH any](delta deltaB) DeltaTuple8[a, b, c, d, e, f, g, h, deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG, deltaH] {
	return DeltaTuple8[a, b, c, d, e, f, g, h, deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG, deltaH]{
		Discriminator: Tuple8Item2,
		Item2:         delta,
	}
}
func NewDeltaTuple8Item3[a any, b any, c any, d any, e any, f any, g any, h any, deltaA any, deltaB any, deltaC any, deltaD any, deltaE any, deltaF any, deltaG any, deltaH any](delta deltaC) DeltaTuple8[a, b, c, d, e, f, g, h, deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG, deltaH] {
	return DeltaTuple8[a, b, c, d, e, f, g, h, deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG, deltaH]{
		Discriminator: Tuple8Item3,
		Item3:         delta,
	}
}
func NewDeltaTuple8Item4[a any, b any, c any, d any, e any, f any, g any, h any, deltaA any, deltaB any, deltaC any, deltaD any, deltaE any, deltaF any, deltaG any, deltaH any](delta deltaD) DeltaTuple8[a, b, c, d, e, f, g, h, deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG, deltaH] {
	return DeltaTuple8[a, b, c, d, e, f, g, h, deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG, deltaH]{
		Discriminator: Tuple8Item4,
		Item4:         delta,
	}
}
func NewDeltaTuple8Item5[a any, b any, c any, d any, e any, f any, g any, h any, deltaA any, deltaB any, deltaC any, deltaD any, deltaE any, deltaF any, deltaG any, deltaH any](delta deltaE) DeltaTuple8[a, b, c, d, e, f, g, h, deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG, deltaH] {
	return DeltaTuple8[a, b, c, d, e, f, g, h, deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG, deltaH]{
		Discriminator: Tuple8Item5,
		Item5:         delta,
	}
}
func NewDeltaTuple8Item6[a any, b any, c any, d any, e any, f any, g any, h any, deltaA any, deltaB any, deltaC any, deltaD any, deltaE any, deltaF any, deltaG any, deltaH any](delta deltaF) DeltaTuple8[a, b, c, d, e, f, g, h, deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG, deltaH] {
	return DeltaTuple8[a, b, c, d, e, f, g, h, deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG, deltaH]{
		Discriminator: Tuple8Item6,
		Item6:         delta,
	}
}
func NewDeltaTuple8Item7[a any, b any, c any, d any, e any, f any, g any, h any, deltaA any, deltaB any, deltaC any, deltaD any, deltaE any, deltaF any, deltaG any, deltaH any](delta deltaG) DeltaTuple8[a, b, c, d, e, f, g, h, deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG, deltaH] {
	return DeltaTuple8[a, b, c, d, e, f, g, h, deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG, deltaH]{
		Discriminator: Tuple8Item7,
		Item7:         delta,
	}
}
func NewDeltaTuple8Item8[a any, b any, c any, d any, e any, f any, g any, h any, deltaA any, deltaB any, deltaC any, deltaD any, deltaE any, deltaF any, deltaG any, deltaH any](delta deltaH) DeltaTuple8[a, b, c, d, e, f, g, h, deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG, deltaH] {
	return DeltaTuple8[a, b, c, d, e, f, g, h, deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG, deltaH]{
		Discriminator: Tuple8Item8,
		Item8:         delta,
	}
}
func MatchDeltaTuple8[a any, b any, c any, d any, e any, f any, g any, h any, deltaA any, deltaB any, deltaC any, deltaD any, deltaE any, deltaF any, deltaG any, deltaH any, Result any](
	onItem1 func(deltaA) (Result, error),
	onItem2 func(deltaB) (Result, error),
	onItem3 func(deltaC) (Result, error),
	onItem4 func(deltaD) (Result, error),
	onItem5 func(deltaE) (Result, error),
	onItem6 func(deltaF) (Result, error),
	onItem7 func(deltaG) (Result, error),
	onItem8 func(deltaH) (Result, error),
) func(DeltaTuple8[a, b, c, d, e, f, g, h, deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG, deltaH]) (Result, error) {
	return func(delta DeltaTuple8[a, b, c, d, e, f, g, h, deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG, deltaH]) (Result, error) {
		var result Result
		switch delta.Discriminator {
		case "Tuple8Item1":
			return onItem1(delta.Item1)
		case "Tuple8Item2":
			return onItem2(delta.Item2)
		case "Tuple8Item3":
			return onItem3(delta.Item3)
		case "Tuple8Item4":
			return onItem4(delta.Item4)
		case "Tuple8Item5":
			return onItem5(delta.Item5)
		case "Tuple8Item6":
			return onItem6(delta.Item6)
		case "Tuple8Item7":
			return onItem7(delta.Item7)
		case "Tuple8Item8":
			return onItem8(delta.Item8)
		}
		return result, NewInvalidDiscriminatorError(string(delta.Discriminator), "DeltaTuple8")
	}
}

type DeltaTuple9EffectsEnum string

const (
	Tuple9Item1 DeltaTuple9EffectsEnum = "Tuple9Item1"
	Tuple9Item2 DeltaTuple9EffectsEnum = "Tuple9Item2"
	Tuple9Item3 DeltaTuple9EffectsEnum = "Tuple9Item3"
	Tuple9Item4 DeltaTuple9EffectsEnum = "Tuple9Item4"
	Tuple9Item5 DeltaTuple9EffectsEnum = "Tuple9Item5"
	Tuple9Item6 DeltaTuple9EffectsEnum = "Tuple9Item6"
	Tuple9Item7 DeltaTuple9EffectsEnum = "Tuple9Item7"
	Tuple9Item8 DeltaTuple9EffectsEnum = "Tuple9Item8"
	Tuple9Item9 DeltaTuple9EffectsEnum = "Tuple9Item9"
)

var AllDeltaTuple9EffectsEnumCases = [...]DeltaTuple9EffectsEnum{Tuple9Item1, Tuple9Item2, Tuple9Item3, Tuple9Item4, Tuple9Item5, Tuple9Item6}

func DefaultDeltaTuple9EffectsEnum() DeltaTuple9EffectsEnum { return AllDeltaTuple9EffectsEnumCases[0] }

type DeltaTuple9[a any, b any, c any, d any, e any, f any, g any, h any, i any, deltaA any, deltaB any, deltaC any, deltaD any, deltaE any, deltaF any, deltaG any, deltaH any, deltaI any] struct {
	DeltaBase
	Discriminator DeltaTuple9EffectsEnum
	Item1         deltaA
	Item2         deltaB
	Item3         deltaC
	Item4         deltaD
	Item5         deltaE
	Item6         deltaF
	Item7         deltaG
	Item8         deltaH
}

func NewDeltaTuple9Item1[a any, b any, c any, d any, e any, f any, g any, h any, i any, deltaA any, deltaB any, deltaC any, deltaD any, deltaE any, deltaF any, deltaG any, deltaH any, deltaI any](delta deltaA) DeltaTuple9[a, b, c, d, e, f, g, h, i, deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG, deltaH, deltaI] {
	return DeltaTuple9[a, b, c, d, e, f, g, h, i, deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG, deltaH, deltaI]{
		Discriminator: Tuple9Item1,
		Item1:         delta,
	}
}
func NewDeltaTuple9Item2[a any, b any, c any, d any, e any, f any, g any, h any, i any, deltaA any, deltaB any, deltaC any, deltaD any, deltaE any, deltaF any, deltaG any, deltaH any, deltaI any](delta deltaB) DeltaTuple9[a, b, c, d, e, f, g, h, i, deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG, deltaH, deltaI] {
	return DeltaTuple9[a, b, c, d, e, f, g, h, i, deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG, deltaH, deltaI]{
		Discriminator: Tuple9Item2,
		Item2:         delta,
	}
}
func NewDeltaTuple9Item3[a any, b any, c any, d any, e any, f any, g any, h any, i any, deltaA any, deltaB any, deltaC any, deltaD any, deltaE any, deltaF any, deltaG any, deltaH any, deltaI any](delta deltaC) DeltaTuple9[a, b, c, d, e, f, g, h, i, deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG, deltaH, deltaI] {
	return DeltaTuple9[a, b, c, d, e, f, g, h, i, deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG, deltaH, deltaI]{
		Discriminator: Tuple9Item3,
		Item3:         delta,
	}
}
func NewDeltaTuple9Item4[a any, b any, c any, d any, e any, f any, g any, h any, i any, deltaA any, deltaB any, deltaC any, deltaD any, deltaE any, deltaF any, deltaG any, deltaH any, deltaI any](delta deltaD) DeltaTuple9[a, b, c, d, e, f, g, h, i, deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG, deltaH, deltaI] {
	return DeltaTuple9[a, b, c, d, e, f, g, h, i, deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG, deltaH, deltaI]{
		Discriminator: Tuple9Item4,
		Item4:         delta,
	}
}
func NewDeltaTuple9Item5[a any, b any, c any, d any, e any, f any, g any, h any, i any, deltaA any, deltaB any, deltaC any, deltaD any, deltaE any, deltaF any, deltaG any, deltaH any, deltaI any](delta deltaE) DeltaTuple9[a, b, c, d, e, f, g, h, i, deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG, deltaH, deltaI] {
	return DeltaTuple9[a, b, c, d, e, f, g, h, i, deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG, deltaH, deltaI]{
		Discriminator: Tuple9Item5,
		Item5:         delta,
	}
}
func NewDeltaTuple9Item6[a any, b any, c any, d any, e any, f any, g any, h any, i any, deltaA any, deltaB any, deltaC any, deltaD any, deltaE any, deltaF any, deltaG any, deltaH any, deltaI any](delta deltaF) DeltaTuple9[a, b, c, d, e, f, g, h, i, deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG, deltaH, deltaI] {
	return DeltaTuple9[a, b, c, d, e, f, g, h, i, deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG, deltaH, deltaI]{
		Discriminator: Tuple9Item6,
		Item6:         delta,
	}
}
func NewDeltaTuple9Item7[a any, b any, c any, d any, e any, f any, g any, h any, i any, deltaA any, deltaB any, deltaC any, deltaD any, deltaE any, deltaF any, deltaG any, deltaH any, deltaI any](delta deltaG) DeltaTuple9[a, b, c, d, e, f, g, h, i, deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG, deltaH, deltaI] {
	return DeltaTuple9[a, b, c, d, e, f, g, h, i, deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG, deltaH, deltaI]{
		Discriminator: Tuple9Item7,
		Item7:         delta,
	}
}
func NewDeltaTuple9Item8[a any, b any, c any, d any, e any, f any, g any, h any, i any, deltaA any, deltaB any, deltaC any, deltaD any, deltaE any, deltaF any, deltaG any, deltaH any, deltaI any](delta deltaH) DeltaTuple9[a, b, c, d, e, f, g, h, i, deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG, deltaH, deltaI] {
	return DeltaTuple9[a, b, c, d, e, f, g, h, i, deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG, deltaH, deltaI]{
		Discriminator: Tuple9Item8,
		Item8:         delta,
	}
}
func NewDeltaTuple9Item9[a any, b any, c any, d any, e any, f any, g any, h any, i any, deltaA any, deltaB any, deltaC any, deltaD any, deltaE any, deltaF any, deltaG any, deltaH any, deltaI any](delta deltaH) DeltaTuple9[a, b, c, d, e, f, g, h, i, deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG, deltaH, deltaI] {
	return DeltaTuple9[a, b, c, d, e, f, g, h, i, deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG, deltaH, deltaI]{
		Discriminator: Tuple9Item9,
		Item8:         delta,
	}
}
func MatchDeltaTuple9[a any, b any, c any, d any, e any, f any, g any, h any, i any, deltaA any, deltaB any, deltaC any, deltaD any, deltaE any, deltaF any, deltaG any, deltaH any, deltaI any, Result any](
	onItem1 func(deltaA) (Result, error),
	onItem2 func(deltaB) (Result, error),
	onItem3 func(deltaC) (Result, error),
	onItem4 func(deltaD) (Result, error),
	onItem5 func(deltaE) (Result, error),
	onItem6 func(deltaF) (Result, error),
	onItem7 func(deltaG) (Result, error),
	onItem8 func(deltaH) (Result, error),
	onItem9 func(deltaH) (Result, error),
) func(DeltaTuple9[a, b, c, d, e, f, g, h, i, deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG, deltaH, deltaI]) (Result, error) {
	return func(delta DeltaTuple9[a, b, c, d, e, f, g, h, i, deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG, deltaH, deltaI]) (Result, error) {
		var result Result
		switch delta.Discriminator {
		case "Tuple9Item1":
			return onItem1(delta.Item1)
		case "Tuple9Item2":
			return onItem2(delta.Item2)
		case "Tuple9Item3":
			return onItem3(delta.Item3)
		case "Tuple9Item4":
			return onItem4(delta.Item4)
		case "Tuple9Item5":
			return onItem5(delta.Item5)
		case "Tuple9Item6":
			return onItem6(delta.Item6)
		case "Tuple9Item7":
			return onItem7(delta.Item7)
		case "Tuple9Item8":
			return onItem8(delta.Item8)
		case "Tuple9Item9":
			return onItem9(delta.Item8)
		}
		return result, NewInvalidDiscriminatorError(string(delta.Discriminator), "DeltaTuple9")
	}
}
