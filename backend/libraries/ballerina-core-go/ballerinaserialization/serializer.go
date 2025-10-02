package ballerinaserialization

import (
	"encoding/json"

	ballerina "ballerina.com/core"
)

// The Serializer function should be total (i.e. never return an error).
// However, we use the json.Marshal to serialize the value under the hood (mainly because of the string serialization in json),
// which can return an error. In theory, we could wrap json.Marshal and panic on error. For that to be safe,
// we would have to prove that we never encounter an error when we serialize. Since we did not prove this for
// the current implementation -- and it is hard to enforce this invariant if we extend the serialization.
// Thus, this partial signature.

type Serializer[T any] func(T) ballerina.Sum[error, json.RawMessage]

type Deserializer[T any] func(json.RawMessage) ballerina.Sum[error, T]
