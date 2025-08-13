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
