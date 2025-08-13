package ballerina

import "encoding/json"

type DeltaTuple2EffectsEnum string

const (
	Tuple2Item1 DeltaTuple2EffectsEnum = "Tuple2Item1"
	Tuple2Item2 DeltaTuple2EffectsEnum = "Tuple2Item2"
)

var AllDeltaTuple2EffectsEnumCases = [...]DeltaTuple2EffectsEnum{Tuple2Item1, Tuple2Item2}

func DefaultDeltaTuple2EffectsEnum() DeltaTuple2EffectsEnum { return AllDeltaTuple2EffectsEnumCases[0] }

type DeltaTuple2[deltaA any, deltaB any] struct {
	DeltaBase
	discriminator DeltaTuple2EffectsEnum
	item1         *deltaA
	item2         *deltaB
}

var _ json.Unmarshaler = &DeltaTuple2[Unit, Unit]{}
var _ json.Marshaler = DeltaTuple2[Unit, Unit]{}

func (v *DeltaTuple2[deltaA, deltaB]) UnmarshalJSON(data []byte) error {
	var tmp struct {
		DeltaBase
		Discriminator DeltaTuple2EffectsEnum
		Item1         *deltaA
		Item2         *deltaB
	}
	if err := json.Unmarshal(data, &tmp); err != nil {
		return err
	}
	v.DeltaBase = tmp.DeltaBase
	v.discriminator = tmp.Discriminator
	v.item1 = tmp.Item1
	v.item2 = tmp.Item2
	return nil
}

func (v DeltaTuple2[deltaA, deltaB]) MarshalJSON() ([]byte, error) {
	return json.Marshal(struct {
		DeltaBase
		Discriminator DeltaTuple2EffectsEnum
		Item1         *deltaA
		Item2         *deltaB
	}{
		DeltaBase:     v.DeltaBase,
		Discriminator: v.discriminator,
		Item1:         v.item1,
		Item2:         v.item2,
	})
}

func NewDeltaTuple2Item1[deltaA any, deltaB any](delta deltaA) DeltaTuple2[deltaA, deltaB] {
	return DeltaTuple2[deltaA, deltaB]{
		discriminator: Tuple2Item1,
		item1:         &delta,
	}
}
func NewDeltaTuple2Item2[deltaA any, deltaB any](delta deltaB) DeltaTuple2[deltaA, deltaB] {
	return DeltaTuple2[deltaA, deltaB]{
		discriminator: Tuple2Item2,
		item2:         &delta,
	}
}
func MatchDeltaTuple2[deltaA any, deltaB any, Result any](
	onItem1 func(deltaA) (Result, error),
	onItem2 func(deltaB) (Result, error),
) func(DeltaTuple2[deltaA, deltaB]) (Result, error) {
	return func(delta DeltaTuple2[deltaA, deltaB]) (Result, error) {
		var result Result
		switch delta.discriminator {
		case Tuple2Item1:
			return onItem1(*delta.item1)
		case Tuple2Item2:
			return onItem2(*delta.item2)
		}
		return result, NewInvalidDiscriminatorError(string(delta.discriminator), "DeltaTuple2")
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

type DeltaTuple3[deltaA any, deltaB any, deltaC any] struct {
	DeltaBase
	discriminator DeltaTuple3EffectsEnum
	item1         *deltaA
	item2         *deltaB
	item3         *deltaC
}

var _ json.Unmarshaler = &DeltaTuple3[Unit, Unit, Unit]{}
var _ json.Marshaler = DeltaTuple3[Unit, Unit, Unit]{}

func (v *DeltaTuple3[deltaA, deltaB, deltaC]) UnmarshalJSON(data []byte) error {
	var tmp struct {
		DeltaBase
		Discriminator DeltaTuple3EffectsEnum
		Item1         *deltaA
		Item2         *deltaB
		Item3         *deltaC
	}
	if err := json.Unmarshal(data, &tmp); err != nil {
		return err
	}
	v.DeltaBase = tmp.DeltaBase
	v.discriminator = tmp.Discriminator
	v.item1 = tmp.Item1
	v.item2 = tmp.Item2
	v.item3 = tmp.Item3
	return nil
}

func (v DeltaTuple3[deltaA, deltaB, deltaC]) MarshalJSON() ([]byte, error) {
	return json.Marshal(struct {
		DeltaBase
		Discriminator DeltaTuple3EffectsEnum
		Item1         *deltaA
		Item2         *deltaB
		Item3         *deltaC
	}{
		DeltaBase:     v.DeltaBase,
		Discriminator: v.discriminator,
		Item1:         v.item1,
		Item2:         v.item2,
		Item3:         v.item3,
	})
}

func NewDeltaTuple3Item1[deltaA any, deltaB any, deltaC any](delta deltaA) DeltaTuple3[deltaA, deltaB, deltaC] {
	return DeltaTuple3[deltaA, deltaB, deltaC]{
		discriminator: Tuple3Item1,
		item1:         &delta,
	}
}
func NewDeltaTuple3Item2[deltaA any, deltaB any, deltaC any](delta deltaB) DeltaTuple3[deltaA, deltaB, deltaC] {
	return DeltaTuple3[deltaA, deltaB, deltaC]{
		discriminator: Tuple3Item2,
		item2:         &delta,
	}
}
func NewDeltaTuple3Item3[deltaA any, deltaB any, deltaC any](delta deltaC) DeltaTuple3[deltaA, deltaB, deltaC] {
	return DeltaTuple3[deltaA, deltaB, deltaC]{
		discriminator: Tuple3Item3,
		item3:         &delta,
	}
}
func MatchDeltaTuple3[deltaA any, deltaB any, deltaC any, Result any](
	onItem1 func(deltaA) (Result, error),
	onItem2 func(deltaB) (Result, error),
	onItem3 func(deltaC) (Result, error),
) func(DeltaTuple3[deltaA, deltaB, deltaC]) (Result, error) {
	return func(delta DeltaTuple3[deltaA, deltaB, deltaC]) (Result, error) {
		var result Result
		switch delta.discriminator {
		case Tuple3Item1:
			return onItem1(*delta.item1)
		case Tuple3Item2:
			return onItem2(*delta.item2)
		case Tuple3Item3:
			return onItem3(*delta.item3)
		}
		return result, NewInvalidDiscriminatorError(string(delta.discriminator), "DeltaTuple3")
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

type DeltaTuple4[deltaA any, deltaB any, deltaC any, deltaD any] struct {
	DeltaBase
	discriminator DeltaTuple4EffectsEnum
	item1         *deltaA
	item2         *deltaB
	item3         *deltaC
	item4         *deltaD
}

var _ json.Unmarshaler = &DeltaTuple4[Unit, Unit, Unit, Unit]{}
var _ json.Marshaler = DeltaTuple4[Unit, Unit, Unit, Unit]{}

func (v *DeltaTuple4[deltaA, deltaB, deltaC, deltaD]) UnmarshalJSON(data []byte) error {
	var tmp struct {
		DeltaBase
		Discriminator DeltaTuple4EffectsEnum
		Item1         *deltaA
		Item2         *deltaB
		Item3         *deltaC
		Item4         *deltaD
	}
	if err := json.Unmarshal(data, &tmp); err != nil {
		return err
	}
	v.DeltaBase = tmp.DeltaBase
	v.discriminator = tmp.Discriminator
	v.item1 = tmp.Item1
	v.item2 = tmp.Item2
	v.item3 = tmp.Item3
	v.item4 = tmp.Item4
	return nil
}

func (v DeltaTuple4[deltaA, deltaB, deltaC, deltaD]) MarshalJSON() ([]byte, error) {
	return json.Marshal(struct {
		DeltaBase
		Discriminator DeltaTuple4EffectsEnum
		Item1         *deltaA
		Item2         *deltaB
		Item3         *deltaC
		Item4         *deltaD
	}{
		DeltaBase:     v.DeltaBase,
		Discriminator: v.discriminator,
		Item1:         v.item1,
		Item2:         v.item2,
		Item3:         v.item3,
		Item4:         v.item4,
	})
}

func NewDeltaTuple4Item1[deltaA any, deltaB any, deltaC any, deltaD any](delta deltaA) DeltaTuple4[deltaA, deltaB, deltaC, deltaD] {
	return DeltaTuple4[deltaA, deltaB, deltaC, deltaD]{
		discriminator: Tuple4Item1,
		item1:         &delta,
	}
}
func NewDeltaTuple4Item2[deltaA any, deltaB any, deltaC any, deltaD any](delta deltaB) DeltaTuple4[deltaA, deltaB, deltaC, deltaD] {
	return DeltaTuple4[deltaA, deltaB, deltaC, deltaD]{
		discriminator: Tuple4Item2,
		item2:         &delta,
	}
}
func NewDeltaTuple4Item3[deltaA any, deltaB any, deltaC any, deltaD any](delta deltaC) DeltaTuple4[deltaA, deltaB, deltaC, deltaD] {
	return DeltaTuple4[deltaA, deltaB, deltaC, deltaD]{
		discriminator: Tuple4Item3,
		item3:         &delta,
	}
}
func NewDeltaTuple4Item4[deltaA any, deltaB any, deltaC any, deltaD any](delta deltaD) DeltaTuple4[deltaA, deltaB, deltaC, deltaD] {
	return DeltaTuple4[deltaA, deltaB, deltaC, deltaD]{
		discriminator: Tuple4Item4,
		item4:         &delta,
	}
}
func MatchDeltaTuple4[deltaA any, deltaB any, deltaC any, deltaD any, Result any](
	onItem1 func(deltaA) (Result, error),
	onItem2 func(deltaB) (Result, error),
	onItem3 func(deltaC) (Result, error),
	onItem4 func(deltaD) (Result, error),
) func(DeltaTuple4[deltaA, deltaB, deltaC, deltaD]) (Result, error) {
	return func(delta DeltaTuple4[deltaA, deltaB, deltaC, deltaD]) (Result, error) {
		var result Result
		switch delta.discriminator {
		case Tuple4Item1:
			return onItem1(*delta.item1)
		case Tuple4Item2:
			return onItem2(*delta.item2)
		case Tuple4Item3:
			return onItem3(*delta.item3)
		case Tuple4Item4:
			return onItem4(*delta.item4)
		}
		return result, NewInvalidDiscriminatorError(string(delta.discriminator), "DeltaTuple4")
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

type DeltaTuple5[deltaA any, deltaB any, deltaC any, deltaD any, deltaE any] struct {
	DeltaBase
	discriminator DeltaTuple5EffectsEnum
	item1         *deltaA
	item2         *deltaB
	item3         *deltaC
	item4         *deltaD
	item5         *deltaE
}

var _ json.Unmarshaler = &DeltaTuple5[Unit, Unit, Unit, Unit, Unit]{}
var _ json.Marshaler = DeltaTuple5[Unit, Unit, Unit, Unit, Unit]{}

func (v *DeltaTuple5[deltaA, deltaB, deltaC, deltaD, deltaE]) UnmarshalJSON(data []byte) error {
	var tmp struct {
		DeltaBase
		Discriminator DeltaTuple5EffectsEnum
		Item1         *deltaA
		Item2         *deltaB
		Item3         *deltaC
		Item4         *deltaD
		Item5         *deltaE
	}
	if err := json.Unmarshal(data, &tmp); err != nil {
		return err
	}
	v.DeltaBase = tmp.DeltaBase
	v.discriminator = tmp.Discriminator
	v.item1 = tmp.Item1
	v.item2 = tmp.Item2
	v.item3 = tmp.Item3
	v.item4 = tmp.Item4
	v.item5 = tmp.Item5
	return nil
}

func (v DeltaTuple5[deltaA, deltaB, deltaC, deltaD, deltaE]) MarshalJSON() ([]byte, error) {
	return json.Marshal(struct {
		DeltaBase
		Discriminator DeltaTuple5EffectsEnum
		Item1         *deltaA
		Item2         *deltaB
		Item3         *deltaC
		Item4         *deltaD
		Item5         *deltaE
	}{
		DeltaBase:     v.DeltaBase,
		Discriminator: v.discriminator,
		Item1:         v.item1,
		Item2:         v.item2,
		Item3:         v.item3,
		Item4:         v.item4,
		Item5:         v.item5,
	})
}

func NewDeltaTuple5Item1[a any, b any, c any, d any, e any, deltaA any, deltaB any, deltaC any, deltaD any, deltaE any](delta deltaA) DeltaTuple5[deltaA, deltaB, deltaC, deltaD, deltaE] {
	return DeltaTuple5[deltaA, deltaB, deltaC, deltaD, deltaE]{
		discriminator: Tuple5Item1,
		item1:         &delta,
	}
}
func NewDeltaTuple5Item2[a any, b any, c any, d any, e any, deltaA any, deltaB any, deltaC any, deltaD any, deltaE any](delta deltaB) DeltaTuple5[deltaA, deltaB, deltaC, deltaD, deltaE] {
	return DeltaTuple5[deltaA, deltaB, deltaC, deltaD, deltaE]{
		discriminator: Tuple5Item2,
		item2:         &delta,
	}
}
func NewDeltaTuple5Item3[deltaA any, deltaB any, deltaC any, deltaD any, deltaE any](delta deltaC) DeltaTuple5[deltaA, deltaB, deltaC, deltaD, deltaE] {
	return DeltaTuple5[deltaA, deltaB, deltaC, deltaD, deltaE]{
		discriminator: Tuple5Item3,
		item3:         &delta,
	}
}
func NewDeltaTuple5Item4[deltaA any, deltaB any, deltaC any, deltaD any, deltaE any](delta deltaD) DeltaTuple5[deltaA, deltaB, deltaC, deltaD, deltaE] {
	return DeltaTuple5[deltaA, deltaB, deltaC, deltaD, deltaE]{
		discriminator: Tuple5Item4,
		item4:         &delta,
	}
}
func NewDeltaTuple5Item5[deltaA any, deltaB any, deltaC any, deltaD any, deltaE any](delta deltaE) DeltaTuple5[deltaA, deltaB, deltaC, deltaD, deltaE] {
	return DeltaTuple5[deltaA, deltaB, deltaC, deltaD, deltaE]{
		discriminator: Tuple5Item5,
		item5:         &delta,
	}
}
func MatchDeltaTuple5[deltaA any, deltaB any, deltaC any, deltaD any, deltaE any, Result any](
	onItem1 func(deltaA) (Result, error),
	onItem2 func(deltaB) (Result, error),
	onItem3 func(deltaC) (Result, error),
	onItem4 func(deltaD) (Result, error),
	onItem5 func(deltaE) (Result, error),
) func(DeltaTuple5[deltaA, deltaB, deltaC, deltaD, deltaE]) (Result, error) {
	return func(delta DeltaTuple5[deltaA, deltaB, deltaC, deltaD, deltaE]) (Result, error) {
		var result Result
		switch delta.discriminator {
		case Tuple5Item1:
			return onItem1(*delta.item1)
		case Tuple5Item2:
			return onItem2(*delta.item2)
		case Tuple5Item3:
			return onItem3(*delta.item3)
		case Tuple5Item4:
			return onItem4(*delta.item4)
		case Tuple5Item5:
			return onItem5(*delta.item5)
		}
		return result, NewInvalidDiscriminatorError(string(delta.discriminator), "DeltaTuple5")
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

type DeltaTuple6[deltaA any, deltaB any, deltaC any, deltaD any, deltaE any, deltaF any] struct {
	DeltaBase
	discriminator DeltaTuple6EffectsEnum
	item1         *deltaA
	item2         *deltaB
	item3         *deltaC
	item4         *deltaD
	item5         *deltaE
	item6         *deltaF
}

var _ json.Unmarshaler = &DeltaTuple6[Unit, Unit, Unit, Unit, Unit, Unit]{}
var _ json.Marshaler = DeltaTuple6[Unit, Unit, Unit, Unit, Unit, Unit]{}

func (v *DeltaTuple6[deltaA, deltaB, deltaC, deltaD, deltaE, deltaF]) UnmarshalJSON(data []byte) error {
	var tmp struct {
		DeltaBase
		Discriminator DeltaTuple6EffectsEnum
		Item1         *deltaA
		Item2         *deltaB
		Item3         *deltaC
		Item4         *deltaD
		Item5         *deltaE
		Item6         *deltaF
	}
	if err := json.Unmarshal(data, &tmp); err != nil {
		return err
	}
	v.DeltaBase = tmp.DeltaBase
	v.discriminator = tmp.Discriminator
	v.item1 = tmp.Item1
	v.item2 = tmp.Item2
	v.item3 = tmp.Item3
	v.item4 = tmp.Item4
	v.item5 = tmp.Item5
	v.item6 = tmp.Item6
	return nil
}

func (v DeltaTuple6[deltaA, deltaB, deltaC, deltaD, deltaE, deltaF]) MarshalJSON() ([]byte, error) {
	return json.Marshal(struct {
		DeltaBase
		Discriminator DeltaTuple6EffectsEnum
		Item1         *deltaA
		Item2         *deltaB
		Item3         *deltaC
		Item4         *deltaD
		Item5         *deltaE
		Item6         *deltaF
	}{
		DeltaBase:     v.DeltaBase,
		Discriminator: v.discriminator,
		Item1:         v.item1,
		Item2:         v.item2,
		Item3:         v.item3,
		Item4:         v.item4,
		Item5:         v.item5,
		Item6:         v.item6,
	})
}

func NewDeltaTuple6Item1[deltaA any, deltaB any, deltaC any, deltaD any, deltaE any, deltaF any](delta deltaA) DeltaTuple6[deltaA, deltaB, deltaC, deltaD, deltaE, deltaF] {
	return DeltaTuple6[deltaA, deltaB, deltaC, deltaD, deltaE, deltaF]{
		discriminator: Tuple6Item1,
		item1:         &delta,
	}
}
func NewDeltaTuple6Item2[deltaA any, deltaB any, deltaC any, deltaD any, deltaE any, deltaF any](delta deltaB) DeltaTuple6[deltaA, deltaB, deltaC, deltaD, deltaE, deltaF] {
	return DeltaTuple6[deltaA, deltaB, deltaC, deltaD, deltaE, deltaF]{
		discriminator: Tuple6Item2,
		item2:         &delta,
	}
}
func NewDeltaTuple6Item3[deltaA any, deltaB any, deltaC any, deltaD any, deltaE any, deltaF any](delta deltaC) DeltaTuple6[deltaA, deltaB, deltaC, deltaD, deltaE, deltaF] {
	return DeltaTuple6[deltaA, deltaB, deltaC, deltaD, deltaE, deltaF]{
		discriminator: Tuple6Item3,
		item3:         &delta,
	}
}
func NewDeltaTuple6Item4[deltaA any, deltaB any, deltaC any, deltaD any, deltaE any, deltaF any](delta deltaD) DeltaTuple6[deltaA, deltaB, deltaC, deltaD, deltaE, deltaF] {
	return DeltaTuple6[deltaA, deltaB, deltaC, deltaD, deltaE, deltaF]{
		discriminator: Tuple6Item4,
		item4:         &delta,
	}
}
func NewDeltaTuple6Item5[deltaA any, deltaB any, deltaC any, deltaD any, deltaE any, deltaF any](delta deltaE) DeltaTuple6[deltaA, deltaB, deltaC, deltaD, deltaE, deltaF] {
	return DeltaTuple6[deltaA, deltaB, deltaC, deltaD, deltaE, deltaF]{
		discriminator: Tuple6Item5,
		item5:         &delta,
	}
}
func NewDeltaTuple6Item6[deltaA any, deltaB any, deltaC any, deltaD any, deltaE any, deltaF any](delta deltaF) DeltaTuple6[deltaA, deltaB, deltaC, deltaD, deltaE, deltaF] {
	return DeltaTuple6[deltaA, deltaB, deltaC, deltaD, deltaE, deltaF]{
		discriminator: Tuple6Item6,
		item6:         &delta,
	}
}
func MatchDeltaTuple6[deltaA any, deltaB any, deltaC any, deltaD any, deltaE any, deltaF any, Result any](
	onItem1 func(deltaA) (Result, error),
	onItem2 func(deltaB) (Result, error),
	onItem3 func(deltaC) (Result, error),
	onItem4 func(deltaD) (Result, error),
	onItem5 func(deltaE) (Result, error),
	onItem6 func(deltaF) (Result, error),
) func(DeltaTuple6[deltaA, deltaB, deltaC, deltaD, deltaE, deltaF]) (Result, error) {
	return func(delta DeltaTuple6[deltaA, deltaB, deltaC, deltaD, deltaE, deltaF]) (Result, error) {
		var result Result
		switch delta.discriminator {
		case Tuple6Item1:
			return onItem1(*delta.item1)
		case Tuple6Item2:
			return onItem2(*delta.item2)
		case Tuple6Item3:
			return onItem3(*delta.item3)
		case Tuple6Item4:
			return onItem4(*delta.item4)
		case Tuple6Item5:
			return onItem5(*delta.item5)
		case Tuple6Item6:
			return onItem6(*delta.item6)
		}
		return result, NewInvalidDiscriminatorError(string(delta.discriminator), "DeltaTuple6")
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

var AllDeltaTuple7EffectsEnumCases = [...]DeltaTuple7EffectsEnum{Tuple7Item1, Tuple7Item2, Tuple7Item3, Tuple7Item4, Tuple7Item5, Tuple7Item6, Tuple7Item7}

func DefaultDeltaTuple7EffectsEnum() DeltaTuple7EffectsEnum { return AllDeltaTuple7EffectsEnumCases[0] }

type DeltaTuple7[deltaA any, deltaB any, deltaC any, deltaD any, deltaE any, deltaF any, deltaG any] struct {
	DeltaBase
	discriminator DeltaTuple7EffectsEnum
	item1         *deltaA
	item2         *deltaB
	item3         *deltaC
	item4         *deltaD
	item5         *deltaE
	item6         *deltaF
	item7         *deltaG
}

var _ json.Unmarshaler = &DeltaTuple7[Unit, Unit, Unit, Unit, Unit, Unit, Unit]{}
var _ json.Marshaler = DeltaTuple7[Unit, Unit, Unit, Unit, Unit, Unit, Unit]{}

func (v *DeltaTuple7[deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG]) UnmarshalJSON(data []byte) error {
	var tmp struct {
		DeltaBase
		Discriminator DeltaTuple7EffectsEnum
		Item1         *deltaA
		Item2         *deltaB
		Item3         *deltaC
		Item4         *deltaD
		Item5         *deltaE
		Item6         *deltaF
		Item7         *deltaG
	}
	if err := json.Unmarshal(data, &tmp); err != nil {
		return err
	}
	v.DeltaBase = tmp.DeltaBase
	v.discriminator = tmp.Discriminator
	v.item1 = tmp.Item1
	v.item2 = tmp.Item2
	v.item3 = tmp.Item3
	v.item4 = tmp.Item4
	v.item5 = tmp.Item5
	v.item6 = tmp.Item6
	v.item7 = tmp.Item7
	return nil
}

func (v DeltaTuple7[deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG]) MarshalJSON() ([]byte, error) {
	return json.Marshal(struct {
		DeltaBase
		Discriminator DeltaTuple7EffectsEnum
		Item1         *deltaA
		Item2         *deltaB
		Item3         *deltaC
		Item4         *deltaD
		Item5         *deltaE
		Item6         *deltaF
		Item7         *deltaG
	}{
		DeltaBase:     v.DeltaBase,
		Discriminator: v.discriminator,
		Item1:         v.item1,
		Item2:         v.item2,
		Item3:         v.item3,
		Item4:         v.item4,
		Item5:         v.item5,
		Item6:         v.item6,
		Item7:         v.item7,
	})
}

func NewDeltaTuple7Item1[deltaA any, deltaB any, deltaC any, deltaD any, deltaE any, deltaF any, deltaG any](delta deltaA) DeltaTuple7[deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG] {
	return DeltaTuple7[deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG]{
		discriminator: Tuple7Item1,
		item1:         &delta,
	}
}
func NewDeltaTuple7Item2[deltaA any, deltaB any, deltaC any, deltaD any, deltaE any, deltaF any, deltaG any](delta deltaB) DeltaTuple7[deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG] {
	return DeltaTuple7[deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG]{
		discriminator: Tuple7Item2,
		item2:         &delta,
	}
}
func NewDeltaTuple7Item3[deltaA any, deltaB any, deltaC any, deltaD any, deltaE any, deltaF any, deltaG any](delta deltaC) DeltaTuple7[deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG] {
	return DeltaTuple7[deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG]{
		discriminator: Tuple7Item3,
		item3:         &delta,
	}
}
func NewDeltaTuple7Item4[deltaA any, deltaB any, deltaC any, deltaD any, deltaE any, deltaF any, deltaG any](delta deltaD) DeltaTuple7[deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG] {
	return DeltaTuple7[deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG]{
		discriminator: Tuple7Item4,
		item4:         &delta,
	}
}
func NewDeltaTuple7Item5[deltaA any, deltaB any, deltaC any, deltaD any, deltaE any, deltaF any, deltaG any](delta deltaE) DeltaTuple7[deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG] {
	return DeltaTuple7[deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG]{
		discriminator: Tuple7Item5,
		item5:         &delta,
	}
}
func NewDeltaTuple7Item6[deltaA any, deltaB any, deltaC any, deltaD any, deltaE any, deltaF any, deltaG any](delta deltaF) DeltaTuple7[deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG] {
	return DeltaTuple7[deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG]{
		discriminator: Tuple7Item6,
		item6:         &delta,
	}
}
func NewDeltaTuple7Item7[deltaA any, deltaB any, deltaC any, deltaD any, deltaE any, deltaF any, deltaG any](delta deltaG) DeltaTuple7[deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG] {
	return DeltaTuple7[deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG]{
		discriminator: Tuple7Item7,
		item7:         &delta,
	}
}
func MatchDeltaTuple7[deltaA any, deltaB any, deltaC any, deltaD any, deltaE any, deltaF any, deltaG any, Result any](
	onItem1 func(deltaA) (Result, error),
	onItem2 func(deltaB) (Result, error),
	onItem3 func(deltaC) (Result, error),
	onItem4 func(deltaD) (Result, error),
	onItem5 func(deltaE) (Result, error),
	onItem6 func(deltaF) (Result, error),
	onItem7 func(deltaG) (Result, error),
) func(DeltaTuple7[deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG]) (Result, error) {
	return func(delta DeltaTuple7[deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG]) (Result, error) {
		var result Result
		switch delta.discriminator {
		case Tuple7Item1:
			return onItem1(*delta.item1)
		case Tuple7Item2:
			return onItem2(*delta.item2)
		case Tuple7Item3:
			return onItem3(*delta.item3)
		case Tuple7Item4:
			return onItem4(*delta.item4)
		case Tuple7Item5:
			return onItem5(*delta.item5)
		case Tuple7Item6:
			return onItem6(*delta.item6)
		case Tuple7Item7:
			return onItem7(*delta.item7)
		}
		return result, NewInvalidDiscriminatorError(string(delta.discriminator), "DeltaTuple7")
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

var AllDeltaTuple8EffectsEnumCases = [...]DeltaTuple8EffectsEnum{Tuple8Item1, Tuple8Item2, Tuple8Item3, Tuple8Item4, Tuple8Item5, Tuple8Item6, Tuple8Item7, Tuple8Item8}

func DefaultDeltaTuple8EffectsEnum() DeltaTuple8EffectsEnum { return AllDeltaTuple8EffectsEnumCases[0] }

type DeltaTuple8[deltaA any, deltaB any, deltaC any, deltaD any, deltaE any, deltaF any, deltaG any, deltaH any] struct {
	DeltaBase
	discriminator DeltaTuple8EffectsEnum
	item1         *deltaA
	item2         *deltaB
	item3         *deltaC
	item4         *deltaD
	item5         *deltaE
	item6         *deltaF
	item7         *deltaG
	item8         *deltaH
}

var _ json.Unmarshaler = &DeltaTuple8[Unit, Unit, Unit, Unit, Unit, Unit, Unit, Unit]{}
var _ json.Marshaler = DeltaTuple8[Unit, Unit, Unit, Unit, Unit, Unit, Unit, Unit]{}

func (v *DeltaTuple8[deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG, deltaH]) UnmarshalJSON(data []byte) error {
	var tmp struct {
		DeltaBase
		Discriminator DeltaTuple8EffectsEnum
		Item1         *deltaA
		Item2         *deltaB
		Item3         *deltaC
		Item4         *deltaD
		Item5         *deltaE
		Item6         *deltaF
		Item7         *deltaG
		Item8         *deltaH
	}
	if err := json.Unmarshal(data, &tmp); err != nil {
		return err
	}
	v.DeltaBase = tmp.DeltaBase
	v.discriminator = tmp.Discriminator
	v.item1 = tmp.Item1
	v.item2 = tmp.Item2
	v.item3 = tmp.Item3
	v.item4 = tmp.Item4
	v.item5 = tmp.Item5
	v.item6 = tmp.Item6
	v.item7 = tmp.Item7
	v.item8 = tmp.Item8
	return nil
}

func (v DeltaTuple8[deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG, deltaH]) MarshalJSON() ([]byte, error) {
	return json.Marshal(struct {
		DeltaBase
		Discriminator DeltaTuple8EffectsEnum
		Item1         *deltaA
		Item2         *deltaB
		Item3         *deltaC
		Item4         *deltaD
		Item5         *deltaE
		Item6         *deltaF
		Item7         *deltaG
		Item8         *deltaH
	}{
		DeltaBase:     v.DeltaBase,
		Discriminator: v.discriminator,
		Item1:         v.item1,
		Item2:         v.item2,
		Item3:         v.item3,
		Item4:         v.item4,
		Item5:         v.item5,
		Item6:         v.item6,
		Item7:         v.item7,
		Item8:         v.item8,
	})
}

func NewDeltaTuple8Item1[deltaA any, deltaB any, deltaC any, deltaD any, deltaE any, deltaF any, deltaG any, deltaH any](delta deltaA) DeltaTuple8[deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG, deltaH] {
	return DeltaTuple8[deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG, deltaH]{
		discriminator: Tuple8Item1,
		item1:         &delta,
	}
}
func NewDeltaTuple8Item2[deltaA any, deltaB any, deltaC any, deltaD any, deltaE any, deltaF any, deltaG any, deltaH any](delta deltaB) DeltaTuple8[deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG, deltaH] {
	return DeltaTuple8[deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG, deltaH]{
		discriminator: Tuple8Item2,
		item2:         &delta,
	}
}
func NewDeltaTuple8Item3[deltaA any, deltaB any, deltaC any, deltaD any, deltaE any, deltaF any, deltaG any, deltaH any](delta deltaC) DeltaTuple8[deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG, deltaH] {
	return DeltaTuple8[deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG, deltaH]{
		discriminator: Tuple8Item3,
		item3:         &delta,
	}
}
func NewDeltaTuple8Item4[deltaA any, deltaB any, deltaC any, deltaD any, deltaE any, deltaF any, deltaG any, deltaH any](delta deltaD) DeltaTuple8[deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG, deltaH] {
	return DeltaTuple8[deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG, deltaH]{
		discriminator: Tuple8Item4,
		item4:         &delta,
	}
}
func NewDeltaTuple8Item5[deltaA any, deltaB any, deltaC any, deltaD any, deltaE any, deltaF any, deltaG any, deltaH any](delta deltaE) DeltaTuple8[deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG, deltaH] {
	return DeltaTuple8[deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG, deltaH]{
		discriminator: Tuple8Item5,
		item5:         &delta,
	}
}
func NewDeltaTuple8Item6[deltaA any, deltaB any, deltaC any, deltaD any, deltaE any, deltaF any, deltaG any, deltaH any](delta deltaF) DeltaTuple8[deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG, deltaH] {
	return DeltaTuple8[deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG, deltaH]{
		discriminator: Tuple8Item6,
		item6:         &delta,
	}
}
func NewDeltaTuple8Item7[deltaA any, deltaB any, deltaC any, deltaD any, deltaE any, deltaF any, deltaG any, deltaH any](delta deltaG) DeltaTuple8[deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG, deltaH] {
	return DeltaTuple8[deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG, deltaH]{
		discriminator: Tuple8Item7,
		item7:         &delta,
	}
}
func NewDeltaTuple8Item8[deltaA any, deltaB any, deltaC any, deltaD any, deltaE any, deltaF any, deltaG any, deltaH any](delta deltaH) DeltaTuple8[deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG, deltaH] {
	return DeltaTuple8[deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG, deltaH]{
		discriminator: Tuple8Item8,
		item8:         &delta,
	}
}
func MatchDeltaTuple8[deltaA any, deltaB any, deltaC any, deltaD any, deltaE any, deltaF any, deltaG any, deltaH any, Result any](
	onItem1 func(deltaA) (Result, error),
	onItem2 func(deltaB) (Result, error),
	onItem3 func(deltaC) (Result, error),
	onItem4 func(deltaD) (Result, error),
	onItem5 func(deltaE) (Result, error),
	onItem6 func(deltaF) (Result, error),
	onItem7 func(deltaG) (Result, error),
	onItem8 func(deltaH) (Result, error),
) func(DeltaTuple8[deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG, deltaH]) (Result, error) {
	return func(delta DeltaTuple8[deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG, deltaH]) (Result, error) {
		var result Result
		switch delta.discriminator {
		case Tuple8Item1:
			return onItem1(*delta.item1)
		case Tuple8Item2:
			return onItem2(*delta.item2)
		case Tuple8Item3:
			return onItem3(*delta.item3)
		case Tuple8Item4:
			return onItem4(*delta.item4)
		case Tuple8Item5:
			return onItem5(*delta.item5)
		case Tuple8Item6:
			return onItem6(*delta.item6)
		case Tuple8Item7:
			return onItem7(*delta.item7)
		case Tuple8Item8:
			return onItem8(*delta.item8)
		}
		return result, NewInvalidDiscriminatorError(string(delta.discriminator), "DeltaTuple8")
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

var AllDeltaTuple9EffectsEnumCases = [...]DeltaTuple9EffectsEnum{
	Tuple9Item1, Tuple9Item2, Tuple9Item3, Tuple9Item4, Tuple9Item5, Tuple9Item6, Tuple9Item7, Tuple9Item8, Tuple9Item9,
}

func DefaultDeltaTuple9EffectsEnum() DeltaTuple9EffectsEnum { return AllDeltaTuple9EffectsEnumCases[0] }

type DeltaTuple9[deltaA any, deltaB any, deltaC any, deltaD any, deltaE any, deltaF any, deltaG any, deltaH any, deltaI any] struct {
	DeltaBase
	discriminator DeltaTuple9EffectsEnum
	item1         *deltaA
	item2         *deltaB
	item3         *deltaC
	item4         *deltaD
	item5         *deltaE
	item6         *deltaF
	item7         *deltaG
	item8         *deltaH
	item9         *deltaI
}

var _ json.Unmarshaler = &DeltaTuple9[Unit, Unit, Unit, Unit, Unit, Unit, Unit, Unit, Unit]{}
var _ json.Marshaler = DeltaTuple9[Unit, Unit, Unit, Unit, Unit, Unit, Unit, Unit, Unit]{}

func (v *DeltaTuple9[deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG, deltaH, deltaI]) UnmarshalJSON(data []byte) error {
	var tmp struct {
		DeltaBase
		Discriminator DeltaTuple9EffectsEnum
		Item1         *deltaA
		Item2         *deltaB
		Item3         *deltaC
		Item4         *deltaD
		Item5         *deltaE
		Item6         *deltaF
		Item7         *deltaG
		Item8         *deltaH
		Item9         *deltaI
	}
	if err := json.Unmarshal(data, &tmp); err != nil {
		return err
	}
	v.DeltaBase = tmp.DeltaBase
	v.discriminator = tmp.Discriminator
	v.item1 = tmp.Item1
	v.item2 = tmp.Item2
	v.item3 = tmp.Item3
	v.item4 = tmp.Item4
	v.item5 = tmp.Item5
	v.item6 = tmp.Item6
	v.item7 = tmp.Item7
	v.item8 = tmp.Item8
	v.item9 = tmp.Item9
	return nil
}

func (v DeltaTuple9[deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG, deltaH, deltaI]) MarshalJSON() ([]byte, error) {
	return json.Marshal(struct {
		DeltaBase
		Discriminator DeltaTuple9EffectsEnum
		Item1         *deltaA
		Item2         *deltaB
		Item3         *deltaC
		Item4         *deltaD
		Item5         *deltaE
		Item6         *deltaF
		Item7         *deltaG
		Item8         *deltaH
		Item9         *deltaI
	}{
		DeltaBase:     v.DeltaBase,
		Discriminator: v.discriminator,
		Item1:         v.item1,
		Item2:         v.item2,
		Item3:         v.item3,
		Item4:         v.item4,
		Item5:         v.item5,
		Item6:         v.item6,
		Item7:         v.item7,
		Item8:         v.item8,
		Item9:         v.item9,
	})
}

func NewDeltaTuple9Item1[deltaA any, deltaB any, deltaC any, deltaD any, deltaE any, deltaF any, deltaG any, deltaH any, deltaI any](delta deltaA) DeltaTuple9[deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG, deltaH, deltaI] {
	return DeltaTuple9[deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG, deltaH, deltaI]{
		discriminator: Tuple9Item1,
		item1:         &delta,
	}
}
func NewDeltaTuple9Item2[deltaA any, deltaB any, deltaC any, deltaD any, deltaE any, deltaF any, deltaG any, deltaH any, deltaI any](delta deltaB) DeltaTuple9[deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG, deltaH, deltaI] {
	return DeltaTuple9[deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG, deltaH, deltaI]{
		discriminator: Tuple9Item2,
		item2:         &delta,
	}
}
func NewDeltaTuple9Item3[deltaA any, deltaB any, deltaC any, deltaD any, deltaE any, deltaF any, deltaG any, deltaH any, deltaI any](delta deltaC) DeltaTuple9[deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG, deltaH, deltaI] {
	return DeltaTuple9[deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG, deltaH, deltaI]{
		discriminator: Tuple9Item3,
		item3:         &delta,
	}
}
func NewDeltaTuple9Item4[deltaA any, deltaB any, deltaC any, deltaD any, deltaE any, deltaF any, deltaG any, deltaH any, deltaI any](delta deltaD) DeltaTuple9[deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG, deltaH, deltaI] {
	return DeltaTuple9[deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG, deltaH, deltaI]{
		discriminator: Tuple9Item4,
		item4:         &delta,
	}
}
func NewDeltaTuple9Item5[deltaA any, deltaB any, deltaC any, deltaD any, deltaE any, deltaF any, deltaG any, deltaH any, deltaI any](delta deltaE) DeltaTuple9[deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG, deltaH, deltaI] {
	return DeltaTuple9[deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG, deltaH, deltaI]{
		discriminator: Tuple9Item5,
		item5:         &delta,
	}
}
func NewDeltaTuple9Item6[deltaA any, deltaB any, deltaC any, deltaD any, deltaE any, deltaF any, deltaG any, deltaH any, deltaI any](delta deltaF) DeltaTuple9[deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG, deltaH, deltaI] {
	return DeltaTuple9[deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG, deltaH, deltaI]{
		discriminator: Tuple9Item6,
		item6:         &delta,
	}
}
func NewDeltaTuple9Item7[deltaA any, deltaB any, deltaC any, deltaD any, deltaE any, deltaF any, deltaG any, deltaH any, deltaI any](delta deltaG) DeltaTuple9[deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG, deltaH, deltaI] {
	return DeltaTuple9[deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG, deltaH, deltaI]{
		discriminator: Tuple9Item7,
		item7:         &delta,
	}
}
func NewDeltaTuple9Item8[deltaA any, deltaB any, deltaC any, deltaD any, deltaE any, deltaF any, deltaG any, deltaH any, deltaI any](delta deltaH) DeltaTuple9[deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG, deltaH, deltaI] {
	return DeltaTuple9[deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG, deltaH, deltaI]{
		discriminator: Tuple9Item8,
		item8:         &delta,
	}
}
func NewDeltaTuple9Item9[deltaA any, deltaB any, deltaC any, deltaD any, deltaE any, deltaF any, deltaG any, deltaH any, deltaI any](delta deltaI) DeltaTuple9[deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG, deltaH, deltaI] {
	return DeltaTuple9[deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG, deltaH, deltaI]{
		discriminator: Tuple9Item9,
		item9:         &delta,
	}
}
func MatchDeltaTuple9[deltaA any, deltaB any, deltaC any, deltaD any, deltaE any, deltaF any, deltaG any, deltaH any, deltaI any, Result any](
	onItem1 func(deltaA) (Result, error),
	onItem2 func(deltaB) (Result, error),
	onItem3 func(deltaC) (Result, error),
	onItem4 func(deltaD) (Result, error),
	onItem5 func(deltaE) (Result, error),
	onItem6 func(deltaF) (Result, error),
	onItem7 func(deltaG) (Result, error),
	onItem8 func(deltaH) (Result, error),
	onItem9 func(deltaI) (Result, error),
) func(DeltaTuple9[deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG, deltaH, deltaI]) (Result, error) {
	return func(delta DeltaTuple9[deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG, deltaH, deltaI]) (Result, error) {
		var result Result
		switch delta.discriminator {
		case Tuple9Item1:
			return onItem1(*delta.item1)
		case Tuple9Item2:
			return onItem2(*delta.item2)
		case Tuple9Item3:
			return onItem3(*delta.item3)
		case Tuple9Item4:
			return onItem4(*delta.item4)
		case Tuple9Item5:
			return onItem5(*delta.item5)
		case Tuple9Item6:
			return onItem6(*delta.item6)
		case Tuple9Item7:
			return onItem7(*delta.item7)
		case Tuple9Item8:
			return onItem8(*delta.item8)
		case Tuple9Item9:
			return onItem9(*delta.item9)
		}
		return result, NewInvalidDiscriminatorError(string(delta.discriminator), "DeltaTuple9")
	}
}
