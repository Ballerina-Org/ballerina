package ballerina

import "encoding/json"

type deltaTuple2EffectsEnum string

const (
	tuple2Item1 deltaTuple2EffectsEnum = "Tuple2Item1"
	tuple2Item2 deltaTuple2EffectsEnum = "Tuple2Item2"
)

type DeltaTuple2[deltaA any, deltaB any] struct {
	DeltaBase
	discriminator deltaTuple2EffectsEnum
	item1         *deltaA
	item2         *deltaB
}

var _ json.Unmarshaler = &DeltaTuple2[Unit, Unit]{}
var _ json.Marshaler = DeltaTuple2[Unit, Unit]{}

func (v *DeltaTuple2[deltaA, deltaB]) UnmarshalJSON(data []byte) error {
	var tmp struct {
		DeltaBase
		Discriminator deltaTuple2EffectsEnum
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
		Discriminator deltaTuple2EffectsEnum
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
		discriminator: tuple2Item1,
		item1:         &delta,
	}
}
func NewDeltaTuple2Item2[deltaA any, deltaB any](delta deltaB) DeltaTuple2[deltaA, deltaB] {
	return DeltaTuple2[deltaA, deltaB]{
		discriminator: tuple2Item2,
		item2:         &delta,
	}
}

func MatchDeltaTuple2[A any, B any, deltaA any, deltaB any, Result any](
	onItem1 func(deltaA) func(ReaderWithError[Unit, A]) (Result, error),
	onItem2 func(deltaB) func(ReaderWithError[Unit, B]) (Result, error),
) func(DeltaTuple2[deltaA, deltaB]) func(ReaderWithError[Unit, Tuple2[A, B]]) (Result, error) {
	return func(delta DeltaTuple2[deltaA, deltaB]) func(ReaderWithError[Unit, Tuple2[A, B]]) (Result, error) {
		return func(value ReaderWithError[Unit, Tuple2[A, B]]) (Result, error) {
			var result Result
			switch delta.discriminator {
			case tuple2Item1:
				return onItem1(*delta.item1)(MapReaderWithError[Unit](Tuple2[A, B].GetItem1)(value))
			case tuple2Item2:
				return onItem2(*delta.item2)(MapReaderWithError[Unit](Tuple2[A, B].GetItem2)(value))
			}
			return result, NewInvalidDiscriminatorError(string(delta.discriminator), "DeltaTuple2")
		}
	}
}

type deltaTuple3EffectsEnum string

const (
	tuple3Item1 deltaTuple3EffectsEnum = "Tuple3Item1"
	tuple3Item2 deltaTuple3EffectsEnum = "Tuple3Item2"
	tuple3Item3 deltaTuple3EffectsEnum = "Tuple3Item3"
)

type DeltaTuple3[deltaA any, deltaB any, deltaC any] struct {
	DeltaBase
	discriminator deltaTuple3EffectsEnum
	item1         *deltaA
	item2         *deltaB
	item3         *deltaC
}

var _ json.Unmarshaler = &DeltaTuple3[Unit, Unit, Unit]{}
var _ json.Marshaler = DeltaTuple3[Unit, Unit, Unit]{}

func (v *DeltaTuple3[deltaA, deltaB, deltaC]) UnmarshalJSON(data []byte) error {
	var tmp struct {
		DeltaBase
		Discriminator deltaTuple3EffectsEnum
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
		Discriminator deltaTuple3EffectsEnum
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
		discriminator: tuple3Item1,
		item1:         &delta,
	}
}
func NewDeltaTuple3Item2[deltaA any, deltaB any, deltaC any](delta deltaB) DeltaTuple3[deltaA, deltaB, deltaC] {
	return DeltaTuple3[deltaA, deltaB, deltaC]{
		discriminator: tuple3Item2,
		item2:         &delta,
	}
}
func NewDeltaTuple3Item3[deltaA any, deltaB any, deltaC any](delta deltaC) DeltaTuple3[deltaA, deltaB, deltaC] {
	return DeltaTuple3[deltaA, deltaB, deltaC]{
		discriminator: tuple3Item3,
		item3:         &delta,
	}
}
func MatchDeltaTuple3[A any, B any, C any, deltaA any, deltaB any, deltaC any, Result any](
	onItem1 func(deltaA) func(ReaderWithError[Unit, A]) (Result, error),
	onItem2 func(deltaB) func(ReaderWithError[Unit, B]) (Result, error),
	onItem3 func(deltaC) func(ReaderWithError[Unit, C]) (Result, error),
) func(DeltaTuple3[deltaA, deltaB, deltaC]) func(ReaderWithError[Unit, Tuple3[A, B, C]]) (Result, error) {
	return func(delta DeltaTuple3[deltaA, deltaB, deltaC]) func(ReaderWithError[Unit, Tuple3[A, B, C]]) (Result, error) {
		return func(value ReaderWithError[Unit, Tuple3[A, B, C]]) (Result, error) {
			var result Result
			switch delta.discriminator {
			case tuple3Item1:
				return onItem1(*delta.item1)(MapReaderWithError[Unit](Tuple3[A, B, C].GetItem1)(value))
			case tuple3Item2:
				return onItem2(*delta.item2)(MapReaderWithError[Unit](Tuple3[A, B, C].GetItem2)(value))
			case tuple3Item3:
				return onItem3(*delta.item3)(MapReaderWithError[Unit](Tuple3[A, B, C].GetItem3)(value))
			}
			return result, NewInvalidDiscriminatorError(string(delta.discriminator), "DeltaTuple3")
		}
	}
}

type deltaTuple4EffectsEnum string

const (
	tuple4Item1 deltaTuple4EffectsEnum = "Tuple4Item1"
	tuple4Item2 deltaTuple4EffectsEnum = "Tuple4Item2"
	tuple4Item3 deltaTuple4EffectsEnum = "Tuple4Item3"
	tuple4Item4 deltaTuple4EffectsEnum = "Tuple4Item4"
)

type DeltaTuple4[deltaA any, deltaB any, deltaC any, deltaD any] struct {
	DeltaBase
	discriminator deltaTuple4EffectsEnum
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
		Discriminator deltaTuple4EffectsEnum
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
		Discriminator deltaTuple4EffectsEnum
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
		discriminator: tuple4Item1,
		item1:         &delta,
	}
}
func NewDeltaTuple4Item2[deltaA any, deltaB any, deltaC any, deltaD any](delta deltaB) DeltaTuple4[deltaA, deltaB, deltaC, deltaD] {
	return DeltaTuple4[deltaA, deltaB, deltaC, deltaD]{
		discriminator: tuple4Item2,
		item2:         &delta,
	}
}
func NewDeltaTuple4Item3[deltaA any, deltaB any, deltaC any, deltaD any](delta deltaC) DeltaTuple4[deltaA, deltaB, deltaC, deltaD] {
	return DeltaTuple4[deltaA, deltaB, deltaC, deltaD]{
		discriminator: tuple4Item3,
		item3:         &delta,
	}
}
func NewDeltaTuple4Item4[deltaA any, deltaB any, deltaC any, deltaD any](delta deltaD) DeltaTuple4[deltaA, deltaB, deltaC, deltaD] {
	return DeltaTuple4[deltaA, deltaB, deltaC, deltaD]{
		discriminator: tuple4Item4,
		item4:         &delta,
	}
}
func MatchDeltaTuple4[A any, B any, C any, D any, deltaA any, deltaB any, deltaC any, deltaD any, Result any](
	onItem1 func(deltaA) func(ReaderWithError[Unit, A]) (Result, error),
	onItem2 func(deltaB) func(ReaderWithError[Unit, B]) (Result, error),
	onItem3 func(deltaC) func(ReaderWithError[Unit, C]) (Result, error),
	onItem4 func(deltaD) func(ReaderWithError[Unit, D]) (Result, error),
) func(DeltaTuple4[deltaA, deltaB, deltaC, deltaD]) func(ReaderWithError[Unit, Tuple4[A, B, C, D]]) (Result, error) {
	return func(delta DeltaTuple4[deltaA, deltaB, deltaC, deltaD]) func(ReaderWithError[Unit, Tuple4[A, B, C, D]]) (Result, error) {
		return func(value ReaderWithError[Unit, Tuple4[A, B, C, D]]) (Result, error) {
			var result Result
			switch delta.discriminator {
			case tuple4Item1:
				return onItem1(*delta.item1)(MapReaderWithError[Unit](Tuple4[A, B, C, D].GetItem1)(value))
			case tuple4Item2:
				return onItem2(*delta.item2)(MapReaderWithError[Unit](Tuple4[A, B, C, D].GetItem2)(value))
			case tuple4Item3:
				return onItem3(*delta.item3)(MapReaderWithError[Unit](Tuple4[A, B, C, D].GetItem3)(value))
			case tuple4Item4:
				return onItem4(*delta.item4)(MapReaderWithError[Unit](Tuple4[A, B, C, D].GetItem4)(value))
			}
			return result, NewInvalidDiscriminatorError(string(delta.discriminator), "DeltaTuple4")
		}
	}
}

type deltaTuple5EffectsEnum string

const (
	tuple5Item1 deltaTuple5EffectsEnum = "Tuple5Item1"
	tuple5Item2 deltaTuple5EffectsEnum = "Tuple5Item2"
	tuple5Item3 deltaTuple5EffectsEnum = "Tuple5Item3"
	tuple5Item4 deltaTuple5EffectsEnum = "Tuple5Item4"
	tuple5Item5 deltaTuple5EffectsEnum = "Tuple5Item5"
)

type DeltaTuple5[deltaA any, deltaB any, deltaC any, deltaD any, deltaE any] struct {
	DeltaBase
	discriminator deltaTuple5EffectsEnum
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
		Discriminator deltaTuple5EffectsEnum
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
		Discriminator deltaTuple5EffectsEnum
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
		discriminator: tuple5Item1,
		item1:         &delta,
	}
}
func NewDeltaTuple5Item2[a any, b any, c any, d any, e any, deltaA any, deltaB any, deltaC any, deltaD any, deltaE any](delta deltaB) DeltaTuple5[deltaA, deltaB, deltaC, deltaD, deltaE] {
	return DeltaTuple5[deltaA, deltaB, deltaC, deltaD, deltaE]{
		discriminator: tuple5Item2,
		item2:         &delta,
	}
}
func NewDeltaTuple5Item3[deltaA any, deltaB any, deltaC any, deltaD any, deltaE any](delta deltaC) DeltaTuple5[deltaA, deltaB, deltaC, deltaD, deltaE] {
	return DeltaTuple5[deltaA, deltaB, deltaC, deltaD, deltaE]{
		discriminator: tuple5Item3,
		item3:         &delta,
	}
}
func NewDeltaTuple5Item4[deltaA any, deltaB any, deltaC any, deltaD any, deltaE any](delta deltaD) DeltaTuple5[deltaA, deltaB, deltaC, deltaD, deltaE] {
	return DeltaTuple5[deltaA, deltaB, deltaC, deltaD, deltaE]{
		discriminator: tuple5Item4,
		item4:         &delta,
	}
}
func NewDeltaTuple5Item5[deltaA any, deltaB any, deltaC any, deltaD any, deltaE any](delta deltaE) DeltaTuple5[deltaA, deltaB, deltaC, deltaD, deltaE] {
	return DeltaTuple5[deltaA, deltaB, deltaC, deltaD, deltaE]{
		discriminator: tuple5Item5,
		item5:         &delta,
	}
}
func MatchDeltaTuple5[A any, B any, C any, D any, E any, deltaA any, deltaB any, deltaC any, deltaD any, deltaE any, Result any](
	onItem1 func(deltaA) func(ReaderWithError[Unit, A]) (Result, error),
	onItem2 func(deltaB) func(ReaderWithError[Unit, B]) (Result, error),
	onItem3 func(deltaC) func(ReaderWithError[Unit, C]) (Result, error),
	onItem4 func(deltaD) func(ReaderWithError[Unit, D]) (Result, error),
	onItem5 func(deltaE) func(ReaderWithError[Unit, E]) (Result, error),
) func(DeltaTuple5[deltaA, deltaB, deltaC, deltaD, deltaE]) func(ReaderWithError[Unit, Tuple5[A, B, C, D, E]]) (Result, error) {
	return func(delta DeltaTuple5[deltaA, deltaB, deltaC, deltaD, deltaE]) func(ReaderWithError[Unit, Tuple5[A, B, C, D, E]]) (Result, error) {
		return func(value ReaderWithError[Unit, Tuple5[A, B, C, D, E]]) (Result, error) {
			var result Result
			switch delta.discriminator {
			case tuple5Item1:
				return onItem1(*delta.item1)(MapReaderWithError[Unit](Tuple5[A, B, C, D, E].GetItem1)(value))
			case tuple5Item2:
				return onItem2(*delta.item2)(MapReaderWithError[Unit](Tuple5[A, B, C, D, E].GetItem2)(value))
			case tuple5Item3:
				return onItem3(*delta.item3)(MapReaderWithError[Unit](Tuple5[A, B, C, D, E].GetItem3)(value))
			case tuple5Item4:
				return onItem4(*delta.item4)(MapReaderWithError[Unit](Tuple5[A, B, C, D, E].GetItem4)(value))
			case tuple5Item5:
				return onItem5(*delta.item5)(MapReaderWithError[Unit](Tuple5[A, B, C, D, E].GetItem5)(value))
			}
			return result, NewInvalidDiscriminatorError(string(delta.discriminator), "DeltaTuple5")
		}
	}
}

type deltaTuple6EffectsEnum string

const (
	tuple6Item1 deltaTuple6EffectsEnum = "Tuple6Item1"
	tuple6Item2 deltaTuple6EffectsEnum = "Tuple6Item2"
	tuple6Item3 deltaTuple6EffectsEnum = "Tuple6Item3"
	tuple6Item4 deltaTuple6EffectsEnum = "Tuple6Item4"
	tuple6Item5 deltaTuple6EffectsEnum = "Tuple6Item5"
	tuple6Item6 deltaTuple6EffectsEnum = "Tuple6Item6"
)

type DeltaTuple6[deltaA any, deltaB any, deltaC any, deltaD any, deltaE any, deltaF any] struct {
	DeltaBase
	discriminator deltaTuple6EffectsEnum
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
		Discriminator deltaTuple6EffectsEnum
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
		Discriminator deltaTuple6EffectsEnum
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
		discriminator: tuple6Item1,
		item1:         &delta,
	}
}
func NewDeltaTuple6Item2[deltaA any, deltaB any, deltaC any, deltaD any, deltaE any, deltaF any](delta deltaB) DeltaTuple6[deltaA, deltaB, deltaC, deltaD, deltaE, deltaF] {
	return DeltaTuple6[deltaA, deltaB, deltaC, deltaD, deltaE, deltaF]{
		discriminator: tuple6Item2,
		item2:         &delta,
	}
}
func NewDeltaTuple6Item3[deltaA any, deltaB any, deltaC any, deltaD any, deltaE any, deltaF any](delta deltaC) DeltaTuple6[deltaA, deltaB, deltaC, deltaD, deltaE, deltaF] {
	return DeltaTuple6[deltaA, deltaB, deltaC, deltaD, deltaE, deltaF]{
		discriminator: tuple6Item3,
		item3:         &delta,
	}
}
func NewDeltaTuple6Item4[deltaA any, deltaB any, deltaC any, deltaD any, deltaE any, deltaF any](delta deltaD) DeltaTuple6[deltaA, deltaB, deltaC, deltaD, deltaE, deltaF] {
	return DeltaTuple6[deltaA, deltaB, deltaC, deltaD, deltaE, deltaF]{
		discriminator: tuple6Item4,
		item4:         &delta,
	}
}
func NewDeltaTuple6Item5[deltaA any, deltaB any, deltaC any, deltaD any, deltaE any, deltaF any](delta deltaE) DeltaTuple6[deltaA, deltaB, deltaC, deltaD, deltaE, deltaF] {
	return DeltaTuple6[deltaA, deltaB, deltaC, deltaD, deltaE, deltaF]{
		discriminator: tuple6Item5,
		item5:         &delta,
	}
}
func NewDeltaTuple6Item6[deltaA any, deltaB any, deltaC any, deltaD any, deltaE any, deltaF any](delta deltaF) DeltaTuple6[deltaA, deltaB, deltaC, deltaD, deltaE, deltaF] {
	return DeltaTuple6[deltaA, deltaB, deltaC, deltaD, deltaE, deltaF]{
		discriminator: tuple6Item6,
		item6:         &delta,
	}
}
func MatchDeltaTuple6[A any, B any, C any, D any, E any, F any, deltaA any, deltaB any, deltaC any, deltaD any, deltaE any, deltaF any, Result any](
	onItem1 func(deltaA) func(ReaderWithError[Unit, A]) (Result, error),
	onItem2 func(deltaB) func(ReaderWithError[Unit, B]) (Result, error),
	onItem3 func(deltaC) func(ReaderWithError[Unit, C]) (Result, error),
	onItem4 func(deltaD) func(ReaderWithError[Unit, D]) (Result, error),
	onItem5 func(deltaE) func(ReaderWithError[Unit, E]) (Result, error),
	onItem6 func(deltaF) func(ReaderWithError[Unit, F]) (Result, error),
) func(DeltaTuple6[deltaA, deltaB, deltaC, deltaD, deltaE, deltaF]) func(ReaderWithError[Unit, Tuple6[A, B, C, D, E, F]]) (Result, error) {
	return func(delta DeltaTuple6[deltaA, deltaB, deltaC, deltaD, deltaE, deltaF]) func(ReaderWithError[Unit, Tuple6[A, B, C, D, E, F]]) (Result, error) {
		return func(value ReaderWithError[Unit, Tuple6[A, B, C, D, E, F]]) (Result, error) {
			var result Result
			switch delta.discriminator {
			case tuple6Item1:
				return onItem1(*delta.item1)(MapReaderWithError[Unit](Tuple6[A, B, C, D, E, F].GetItem1)(value))
			case tuple6Item2:
				return onItem2(*delta.item2)(MapReaderWithError[Unit](Tuple6[A, B, C, D, E, F].GetItem2)(value))
			case tuple6Item3:
				return onItem3(*delta.item3)(MapReaderWithError[Unit](Tuple6[A, B, C, D, E, F].GetItem3)(value))
			case tuple6Item4:
				return onItem4(*delta.item4)(MapReaderWithError[Unit](Tuple6[A, B, C, D, E, F].GetItem4)(value))
			case tuple6Item5:
				return onItem5(*delta.item5)(MapReaderWithError[Unit](Tuple6[A, B, C, D, E, F].GetItem5)(value))
			case tuple6Item6:
				return onItem6(*delta.item6)(MapReaderWithError[Unit](Tuple6[A, B, C, D, E, F].GetItem6)(value))
			}
			return result, NewInvalidDiscriminatorError(string(delta.discriminator), "DeltaTuple6")
		}
	}
}

type deltaTuple7EffectsEnum string

const (
	tuple7Item1 deltaTuple7EffectsEnum = "Tuple7Item1"
	tuple7Item2 deltaTuple7EffectsEnum = "Tuple7Item2"
	tuple7Item3 deltaTuple7EffectsEnum = "Tuple7Item3"
	tuple7Item4 deltaTuple7EffectsEnum = "Tuple7Item4"
	tuple7Item5 deltaTuple7EffectsEnum = "Tuple7Item5"
	tuple7Item6 deltaTuple7EffectsEnum = "Tuple7Item6"
	tuple7Item7 deltaTuple7EffectsEnum = "Tuple7Item7"
)

type DeltaTuple7[deltaA any, deltaB any, deltaC any, deltaD any, deltaE any, deltaF any, deltaG any] struct {
	DeltaBase
	discriminator deltaTuple7EffectsEnum
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
		Discriminator deltaTuple7EffectsEnum
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
		Discriminator deltaTuple7EffectsEnum
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
		discriminator: tuple7Item1,
		item1:         &delta,
	}
}
func NewDeltaTuple7Item2[deltaA any, deltaB any, deltaC any, deltaD any, deltaE any, deltaF any, deltaG any](delta deltaB) DeltaTuple7[deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG] {
	return DeltaTuple7[deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG]{
		discriminator: tuple7Item2,
		item2:         &delta,
	}
}
func NewDeltaTuple7Item3[deltaA any, deltaB any, deltaC any, deltaD any, deltaE any, deltaF any, deltaG any](delta deltaC) DeltaTuple7[deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG] {
	return DeltaTuple7[deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG]{
		discriminator: tuple7Item3,
		item3:         &delta,
	}
}
func NewDeltaTuple7Item4[deltaA any, deltaB any, deltaC any, deltaD any, deltaE any, deltaF any, deltaG any](delta deltaD) DeltaTuple7[deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG] {
	return DeltaTuple7[deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG]{
		discriminator: tuple7Item4,
		item4:         &delta,
	}
}
func NewDeltaTuple7Item5[deltaA any, deltaB any, deltaC any, deltaD any, deltaE any, deltaF any, deltaG any](delta deltaE) DeltaTuple7[deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG] {
	return DeltaTuple7[deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG]{
		discriminator: tuple7Item5,
		item5:         &delta,
	}
}
func NewDeltaTuple7Item6[deltaA any, deltaB any, deltaC any, deltaD any, deltaE any, deltaF any, deltaG any](delta deltaF) DeltaTuple7[deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG] {
	return DeltaTuple7[deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG]{
		discriminator: tuple7Item6,
		item6:         &delta,
	}
}
func NewDeltaTuple7Item7[deltaA any, deltaB any, deltaC any, deltaD any, deltaE any, deltaF any, deltaG any](delta deltaG) DeltaTuple7[deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG] {
	return DeltaTuple7[deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG]{
		discriminator: tuple7Item7,
		item7:         &delta,
	}
}
func MatchDeltaTuple7[A any, B any, C any, D any, E any, F any, G any, deltaA any, deltaB any, deltaC any, deltaD any, deltaE any, deltaF any, deltaG any, Result any](
	onItem1 func(deltaA) func(ReaderWithError[Unit, A]) (Result, error),
	onItem2 func(deltaB) func(ReaderWithError[Unit, B]) (Result, error),
	onItem3 func(deltaC) func(ReaderWithError[Unit, C]) (Result, error),
	onItem4 func(deltaD) func(ReaderWithError[Unit, D]) (Result, error),
	onItem5 func(deltaE) func(ReaderWithError[Unit, E]) (Result, error),
	onItem6 func(deltaF) func(ReaderWithError[Unit, F]) (Result, error),
	onItem7 func(deltaG) func(ReaderWithError[Unit, G]) (Result, error),
) func(DeltaTuple7[deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG]) func(ReaderWithError[Unit, Tuple7[A, B, C, D, E, F, G]]) (Result, error) {
	return func(delta DeltaTuple7[deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG]) func(ReaderWithError[Unit, Tuple7[A, B, C, D, E, F, G]]) (Result, error) {
		return func(value ReaderWithError[Unit, Tuple7[A, B, C, D, E, F, G]]) (Result, error) {
			var result Result
			switch delta.discriminator {
			case tuple7Item1:
				return onItem1(*delta.item1)(MapReaderWithError[Unit](Tuple7[A, B, C, D, E, F, G].GetItem1)(value))
			case tuple7Item2:
				return onItem2(*delta.item2)(MapReaderWithError[Unit](Tuple7[A, B, C, D, E, F, G].GetItem2)(value))
			case tuple7Item3:
				return onItem3(*delta.item3)(MapReaderWithError[Unit](Tuple7[A, B, C, D, E, F, G].GetItem3)(value))
			case tuple7Item4:
				return onItem4(*delta.item4)(MapReaderWithError[Unit](Tuple7[A, B, C, D, E, F, G].GetItem4)(value))
			case tuple7Item5:
				return onItem5(*delta.item5)(MapReaderWithError[Unit](Tuple7[A, B, C, D, E, F, G].GetItem5)(value))
			case tuple7Item6:
				return onItem6(*delta.item6)(MapReaderWithError[Unit](Tuple7[A, B, C, D, E, F, G].GetItem6)(value))
			case tuple7Item7:
				return onItem7(*delta.item7)(MapReaderWithError[Unit](Tuple7[A, B, C, D, E, F, G].GetItem7)(value))
			}
			return result, NewInvalidDiscriminatorError(string(delta.discriminator), "DeltaTuple7")
		}
	}
}

type deltaTuple8EffectsEnum string

const (
	tuple8Item1 deltaTuple8EffectsEnum = "Tuple8Item1"
	tuple8Item2 deltaTuple8EffectsEnum = "Tuple8Item2"
	tuple8Item3 deltaTuple8EffectsEnum = "Tuple8Item3"
	tuple8Item4 deltaTuple8EffectsEnum = "Tuple8Item4"
	tuple8Item5 deltaTuple8EffectsEnum = "Tuple8Item5"
	tuple8Item6 deltaTuple8EffectsEnum = "Tuple8Item6"
	tuple8Item7 deltaTuple8EffectsEnum = "Tuple8Item7"
	tuple8Item8 deltaTuple8EffectsEnum = "Tuple8Item8"
)

type DeltaTuple8[deltaA any, deltaB any, deltaC any, deltaD any, deltaE any, deltaF any, deltaG any, deltaH any] struct {
	DeltaBase
	discriminator deltaTuple8EffectsEnum
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
		Discriminator deltaTuple8EffectsEnum
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
		Discriminator deltaTuple8EffectsEnum
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
		discriminator: tuple8Item1,
		item1:         &delta,
	}
}
func NewDeltaTuple8Item2[deltaA any, deltaB any, deltaC any, deltaD any, deltaE any, deltaF any, deltaG any, deltaH any](delta deltaB) DeltaTuple8[deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG, deltaH] {
	return DeltaTuple8[deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG, deltaH]{
		discriminator: tuple8Item2,
		item2:         &delta,
	}
}
func NewDeltaTuple8Item3[deltaA any, deltaB any, deltaC any, deltaD any, deltaE any, deltaF any, deltaG any, deltaH any](delta deltaC) DeltaTuple8[deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG, deltaH] {
	return DeltaTuple8[deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG, deltaH]{
		discriminator: tuple8Item3,
		item3:         &delta,
	}
}
func NewDeltaTuple8Item4[deltaA any, deltaB any, deltaC any, deltaD any, deltaE any, deltaF any, deltaG any, deltaH any](delta deltaD) DeltaTuple8[deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG, deltaH] {
	return DeltaTuple8[deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG, deltaH]{
		discriminator: tuple8Item4,
		item4:         &delta,
	}
}
func NewDeltaTuple8Item5[deltaA any, deltaB any, deltaC any, deltaD any, deltaE any, deltaF any, deltaG any, deltaH any](delta deltaE) DeltaTuple8[deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG, deltaH] {
	return DeltaTuple8[deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG, deltaH]{
		discriminator: tuple8Item5,
		item5:         &delta,
	}
}
func NewDeltaTuple8Item6[deltaA any, deltaB any, deltaC any, deltaD any, deltaE any, deltaF any, deltaG any, deltaH any](delta deltaF) DeltaTuple8[deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG, deltaH] {
	return DeltaTuple8[deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG, deltaH]{
		discriminator: tuple8Item6,
		item6:         &delta,
	}
}
func NewDeltaTuple8Item7[deltaA any, deltaB any, deltaC any, deltaD any, deltaE any, deltaF any, deltaG any, deltaH any](delta deltaG) DeltaTuple8[deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG, deltaH] {
	return DeltaTuple8[deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG, deltaH]{
		discriminator: tuple8Item7,
		item7:         &delta,
	}
}
func NewDeltaTuple8Item8[deltaA any, deltaB any, deltaC any, deltaD any, deltaE any, deltaF any, deltaG any, deltaH any](delta deltaH) DeltaTuple8[deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG, deltaH] {
	return DeltaTuple8[deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG, deltaH]{
		discriminator: tuple8Item8,
		item8:         &delta,
	}
}
func MatchDeltaTuple8[A any, B any, C any, D any, E any, F any, G any, H any, deltaA any, deltaB any, deltaC any, deltaD any, deltaE any, deltaF any, deltaG any, deltaH any, Result any](
	onItem1 func(deltaA) func(ReaderWithError[Unit, A]) (Result, error),
	onItem2 func(deltaB) func(ReaderWithError[Unit, B]) (Result, error),
	onItem3 func(deltaC) func(ReaderWithError[Unit, C]) (Result, error),
	onItem4 func(deltaD) func(ReaderWithError[Unit, D]) (Result, error),
	onItem5 func(deltaE) func(ReaderWithError[Unit, E]) (Result, error),
	onItem6 func(deltaF) func(ReaderWithError[Unit, F]) (Result, error),
	onItem7 func(deltaG) func(ReaderWithError[Unit, G]) (Result, error),
	onItem8 func(deltaH) func(ReaderWithError[Unit, H]) (Result, error),
) func(DeltaTuple8[deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG, deltaH]) func(ReaderWithError[Unit, Tuple8[A, B, C, D, E, F, G, H]]) (Result, error) {
	return func(delta DeltaTuple8[deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG, deltaH]) func(ReaderWithError[Unit, Tuple8[A, B, C, D, E, F, G, H]]) (Result, error) {
		return func(value ReaderWithError[Unit, Tuple8[A, B, C, D, E, F, G, H]]) (Result, error) {
			var result Result
			switch delta.discriminator {
			case tuple8Item1:
				return onItem1(*delta.item1)(MapReaderWithError[Unit](Tuple8[A, B, C, D, E, F, G, H].GetItem1)(value))
			case tuple8Item2:
				return onItem2(*delta.item2)(MapReaderWithError[Unit](Tuple8[A, B, C, D, E, F, G, H].GetItem2)(value))
			case tuple8Item3:
				return onItem3(*delta.item3)(MapReaderWithError[Unit](Tuple8[A, B, C, D, E, F, G, H].GetItem3)(value))
			case tuple8Item4:
				return onItem4(*delta.item4)(MapReaderWithError[Unit](Tuple8[A, B, C, D, E, F, G, H].GetItem4)(value))
			case tuple8Item5:
				return onItem5(*delta.item5)(MapReaderWithError[Unit](Tuple8[A, B, C, D, E, F, G, H].GetItem5)(value))
			case tuple8Item6:
				return onItem6(*delta.item6)(MapReaderWithError[Unit](Tuple8[A, B, C, D, E, F, G, H].GetItem6)(value))
			case tuple8Item7:
				return onItem7(*delta.item7)(MapReaderWithError[Unit](Tuple8[A, B, C, D, E, F, G, H].GetItem7)(value))
			case tuple8Item8:
				return onItem8(*delta.item8)(MapReaderWithError[Unit](Tuple8[A, B, C, D, E, F, G, H].GetItem8)(value))
			}
			return result, NewInvalidDiscriminatorError(string(delta.discriminator), "DeltaTuple8")
		}
	}
}

type deltaTuple9EffectsEnum string

const (
	tuple9Item1 deltaTuple9EffectsEnum = "Tuple9Item1"
	tuple9Item2 deltaTuple9EffectsEnum = "Tuple9Item2"
	tuple9Item3 deltaTuple9EffectsEnum = "Tuple9Item3"
	tuple9Item4 deltaTuple9EffectsEnum = "Tuple9Item4"
	tuple9Item5 deltaTuple9EffectsEnum = "Tuple9Item5"
	tuple9Item6 deltaTuple9EffectsEnum = "Tuple9Item6"
	tuple9Item7 deltaTuple9EffectsEnum = "Tuple9Item7"
	tuple9Item8 deltaTuple9EffectsEnum = "Tuple9Item8"
	tuple9Item9 deltaTuple9EffectsEnum = "Tuple9Item9"
)

type DeltaTuple9[deltaA any, deltaB any, deltaC any, deltaD any, deltaE any, deltaF any, deltaG any, deltaH any, deltaI any] struct {
	DeltaBase
	discriminator deltaTuple9EffectsEnum
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
		Discriminator deltaTuple9EffectsEnum
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
		Discriminator deltaTuple9EffectsEnum
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
		discriminator: tuple9Item1,
		item1:         &delta,
	}
}
func NewDeltaTuple9Item2[deltaA any, deltaB any, deltaC any, deltaD any, deltaE any, deltaF any, deltaG any, deltaH any, deltaI any](delta deltaB) DeltaTuple9[deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG, deltaH, deltaI] {
	return DeltaTuple9[deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG, deltaH, deltaI]{
		discriminator: tuple9Item2,
		item2:         &delta,
	}
}
func NewDeltaTuple9Item3[deltaA any, deltaB any, deltaC any, deltaD any, deltaE any, deltaF any, deltaG any, deltaH any, deltaI any](delta deltaC) DeltaTuple9[deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG, deltaH, deltaI] {
	return DeltaTuple9[deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG, deltaH, deltaI]{
		discriminator: tuple9Item3,
		item3:         &delta,
	}
}
func NewDeltaTuple9Item4[deltaA any, deltaB any, deltaC any, deltaD any, deltaE any, deltaF any, deltaG any, deltaH any, deltaI any](delta deltaD) DeltaTuple9[deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG, deltaH, deltaI] {
	return DeltaTuple9[deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG, deltaH, deltaI]{
		discriminator: tuple9Item4,
		item4:         &delta,
	}
}
func NewDeltaTuple9Item5[deltaA any, deltaB any, deltaC any, deltaD any, deltaE any, deltaF any, deltaG any, deltaH any, deltaI any](delta deltaE) DeltaTuple9[deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG, deltaH, deltaI] {
	return DeltaTuple9[deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG, deltaH, deltaI]{
		discriminator: tuple9Item5,
		item5:         &delta,
	}
}
func NewDeltaTuple9Item6[deltaA any, deltaB any, deltaC any, deltaD any, deltaE any, deltaF any, deltaG any, deltaH any, deltaI any](delta deltaF) DeltaTuple9[deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG, deltaH, deltaI] {
	return DeltaTuple9[deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG, deltaH, deltaI]{
		discriminator: tuple9Item6,
		item6:         &delta,
	}
}
func NewDeltaTuple9Item7[deltaA any, deltaB any, deltaC any, deltaD any, deltaE any, deltaF any, deltaG any, deltaH any, deltaI any](delta deltaG) DeltaTuple9[deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG, deltaH, deltaI] {
	return DeltaTuple9[deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG, deltaH, deltaI]{
		discriminator: tuple9Item7,
		item7:         &delta,
	}
}
func NewDeltaTuple9Item8[deltaA any, deltaB any, deltaC any, deltaD any, deltaE any, deltaF any, deltaG any, deltaH any, deltaI any](delta deltaH) DeltaTuple9[deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG, deltaH, deltaI] {
	return DeltaTuple9[deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG, deltaH, deltaI]{
		discriminator: tuple9Item8,
		item8:         &delta,
	}
}
func NewDeltaTuple9Item9[deltaA any, deltaB any, deltaC any, deltaD any, deltaE any, deltaF any, deltaG any, deltaH any, deltaI any](delta deltaI) DeltaTuple9[deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG, deltaH, deltaI] {
	return DeltaTuple9[deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG, deltaH, deltaI]{
		discriminator: tuple9Item9,
		item9:         &delta,
	}
}
func MatchDeltaTuple9[A any, B any, C any, D any, E any, F any, G any, H any, I any, deltaA any, deltaB any, deltaC any, deltaD any, deltaE any, deltaF any, deltaG any, deltaH any, deltaI any, Result any](
	onItem1 func(deltaA) func(ReaderWithError[Unit, A]) (Result, error),
	onItem2 func(deltaB) func(ReaderWithError[Unit, B]) (Result, error),
	onItem3 func(deltaC) func(ReaderWithError[Unit, C]) (Result, error),
	onItem4 func(deltaD) func(ReaderWithError[Unit, D]) (Result, error),
	onItem5 func(deltaE) func(ReaderWithError[Unit, E]) (Result, error),
	onItem6 func(deltaF) func(ReaderWithError[Unit, F]) (Result, error),
	onItem7 func(deltaG) func(ReaderWithError[Unit, G]) (Result, error),
	onItem8 func(deltaH) func(ReaderWithError[Unit, H]) (Result, error),
	onItem9 func(deltaI) func(ReaderWithError[Unit, I]) (Result, error),
) func(DeltaTuple9[deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG, deltaH, deltaI]) func(ReaderWithError[Unit, Tuple9[A, B, C, D, E, F, G, H, I]]) (Result, error) {
	return func(delta DeltaTuple9[deltaA, deltaB, deltaC, deltaD, deltaE, deltaF, deltaG, deltaH, deltaI]) func(ReaderWithError[Unit, Tuple9[A, B, C, D, E, F, G, H, I]]) (Result, error) {
		return func(value ReaderWithError[Unit, Tuple9[A, B, C, D, E, F, G, H, I]]) (Result, error) {
			var result Result
			switch delta.discriminator {
			case tuple9Item1:
				return onItem1(*delta.item1)(MapReaderWithError[Unit](Tuple9[A, B, C, D, E, F, G, H, I].GetItem1)(value))
			case tuple9Item2:
				return onItem2(*delta.item2)(MapReaderWithError[Unit](Tuple9[A, B, C, D, E, F, G, H, I].GetItem2)(value))
			case tuple9Item3:
				return onItem3(*delta.item3)(MapReaderWithError[Unit](Tuple9[A, B, C, D, E, F, G, H, I].GetItem3)(value))
			case tuple9Item4:
				return onItem4(*delta.item4)(MapReaderWithError[Unit](Tuple9[A, B, C, D, E, F, G, H, I].GetItem4)(value))
			case tuple9Item5:
				return onItem5(*delta.item5)(MapReaderWithError[Unit](Tuple9[A, B, C, D, E, F, G, H, I].GetItem5)(value))
			case tuple9Item6:
				return onItem6(*delta.item6)(MapReaderWithError[Unit](Tuple9[A, B, C, D, E, F, G, H, I].GetItem6)(value))
			case tuple9Item7:
				return onItem7(*delta.item7)(MapReaderWithError[Unit](Tuple9[A, B, C, D, E, F, G, H, I].GetItem7)(value))
			case tuple9Item8:
				return onItem8(*delta.item8)(MapReaderWithError[Unit](Tuple9[A, B, C, D, E, F, G, H, I].GetItem8)(value))
			case tuple9Item9:
				return onItem9(*delta.item9)(MapReaderWithError[Unit](Tuple9[A, B, C, D, E, F, G, H, I].GetItem9)(value))
			}
			return result, NewInvalidDiscriminatorError(string(delta.discriminator), "DeltaTuple9")
		}
	}
}
