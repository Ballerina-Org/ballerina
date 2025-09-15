package ballerina

type KeyValue[k comparable, v any] struct {
	Key   k
	Value v
}

type Map[k comparable, v any] []KeyValue[k, v]
