package ballerina

type Tuple2[a any, b any] struct {
	Item1 a
	Item2 b
}

func (t Tuple2[a, b]) Unpack() (a, b) {
	return t.Item1, t.Item2
}

func (t Tuple2[a, b]) GetItem1() a {
	return t.Item1
}

func (t Tuple2[a, b]) GetItem2() b {
	return t.Item2
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

func (t Tuple3[a, b, c]) Unpack() (a, b, c) {
	return t.Item1, t.Item2, t.Item3
}

func (t Tuple3[a, b, c]) GetItem1() a {
	return t.Item1
}

func (t Tuple3[a, b, c]) GetItem2() b {
	return t.Item2
}

func (t Tuple3[a, b, c]) GetItem3() c {
	return t.Item3
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

func (t Tuple4[a, b, c, d]) Unpack() (a, b, c, d) {
	return t.Item1, t.Item2, t.Item3, t.Item4
}

func (t Tuple4[a, b, c, d]) GetItem1() a {
	return t.Item1
}

func (t Tuple4[a, b, c, d]) GetItem2() b {
	return t.Item2
}

func (t Tuple4[a, b, c, d]) GetItem3() c {
	return t.Item3
}

func (t Tuple4[a, b, c, d]) GetItem4() d {
	return t.Item4
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

func (t Tuple5[a, b, c, d, e]) Unpack() (a, b, c, d, e) {
	return t.Item1, t.Item2, t.Item3, t.Item4, t.Item5
}

func (t Tuple5[a, b, c, d, e]) GetItem1() a {
	return t.Item1
}

func (t Tuple5[a, b, c, d, e]) GetItem2() b {
	return t.Item2
}

func (t Tuple5[a, b, c, d, e]) GetItem3() c {
	return t.Item3
}

func (t Tuple5[a, b, c, d, e]) GetItem4() d {
	return t.Item4
}

func (t Tuple5[a, b, c, d, e]) GetItem5() e {
	return t.Item5
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

func (t Tuple6[a, b, c, d, e, f]) Unpack() (a, b, c, d, e, f) {
	return t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, t.Item6
}

func (t Tuple6[a, b, c, d, e, f]) GetItem1() a {
	return t.Item1
}

func (t Tuple6[a, b, c, d, e, f]) GetItem2() b {
	return t.Item2
}

func (t Tuple6[a, b, c, d, e, f]) GetItem3() c {
	return t.Item3
}

func (t Tuple6[a, b, c, d, e, f]) GetItem4() d {
	return t.Item4
}

func (t Tuple6[a, b, c, d, e, f]) GetItem5() e {
	return t.Item5
}

func (t Tuple6[a, b, c, d, e, f]) GetItem6() f {
	return t.Item6
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

func (t Tuple7[a, b, c, d, e, f, g]) Unpack() (a, b, c, d, e, f, g) {
	return t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, t.Item6, t.Item7
}

func (t Tuple7[a, b, c, d, e, f, g]) GetItem1() a {
	return t.Item1
}

func (t Tuple7[a, b, c, d, e, f, g]) GetItem2() b {
	return t.Item2
}

func (t Tuple7[a, b, c, d, e, f, g]) GetItem3() c {
	return t.Item3
}

func (t Tuple7[a, b, c, d, e, f, g]) GetItem4() d {
	return t.Item4
}

func (t Tuple7[a, b, c, d, e, f, g]) GetItem5() e {
	return t.Item5
}

func (t Tuple7[a, b, c, d, e, f, g]) GetItem6() f {
	return t.Item6
}

func (t Tuple7[a, b, c, d, e, f, g]) GetItem7() g {
	return t.Item7
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

func (t Tuple8[a, b, c, d, e, f, g, h]) Unpack() (a, b, c, d, e, f, g, h) {
	return t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, t.Item6, t.Item7, t.Item8
}

func (t Tuple8[a, b, c, d, e, f, g, h]) GetItem1() a {
	return t.Item1
}

func (t Tuple8[a, b, c, d, e, f, g, h]) GetItem2() b {
	return t.Item2
}

func (t Tuple8[a, b, c, d, e, f, g, h]) GetItem3() c {
	return t.Item3
}

func (t Tuple8[a, b, c, d, e, f, g, h]) GetItem4() d {
	return t.Item4
}

func (t Tuple8[a, b, c, d, e, f, g, h]) GetItem5() e {
	return t.Item5
}

func (t Tuple8[a, b, c, d, e, f, g, h]) GetItem6() f {
	return t.Item6
}

func (t Tuple8[a, b, c, d, e, f, g, h]) GetItem7() g {
	return t.Item7
}

func (t Tuple8[a, b, c, d, e, f, g, h]) GetItem8() h {
	return t.Item8
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

func (t Tuple9[a, b, c, d, e, f, g, h, i]) Unpack() (a, b, c, d, e, f, g, h, i) {
	return t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, t.Item6, t.Item7, t.Item8, t.Item9
}

func (t Tuple9[a, b, c, d, e, f, g, h, i]) GetItem1() a {
	return t.Item1
}

func (t Tuple9[a, b, c, d, e, f, g, h, i]) GetItem2() b {
	return t.Item2
}

func (t Tuple9[a, b, c, d, e, f, g, h, i]) GetItem3() c {
	return t.Item3
}

func (t Tuple9[a, b, c, d, e, f, g, h, i]) GetItem4() d {
	return t.Item4
}

func (t Tuple9[a, b, c, d, e, f, g, h, i]) GetItem5() e {
	return t.Item5
}

func (t Tuple9[a, b, c, d, e, f, g, h, i]) GetItem6() f {
	return t.Item6
}

func (t Tuple9[a, b, c, d, e, f, g, h, i]) GetItem7() g {
	return t.Item7
}

func (t Tuple9[a, b, c, d, e, f, g, h, i]) GetItem8() h {
	return t.Item8
}

func (t Tuple9[a, b, c, d, e, f, g, h, i]) GetItem9() i {
	return t.Item9
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
