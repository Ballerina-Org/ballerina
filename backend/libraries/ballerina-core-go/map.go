package ballerina

type KeyValue[k comparable, v any] struct {
	Key   k
	Value v
}

type Map[k comparable, v any] []KeyValue[k, v]

func DefaultMap[k comparable, v any]() Map[k, v] {
	return make([]KeyValue[k, v], 0)
}

func (m Map[k, v]) Get(key k) Option[v] {
	for _, kv := range m {
		if kv.Key == key {
			return Some(kv.Value)
		}
	}
	return None[v]()
}
