package ballerina

import (
	"encoding/json"
)

type Contains struct {
	contains string
}

func NewContains(value string) Contains {
	return Contains{contains: value}
}

func (c Contains) GetContains() string {
	return c.contains
}

var _ json.Unmarshaler = &Contains{}
var _ json.Marshaler = Contains{}

func (d Contains) MarshalJSON() ([]byte, error) {
	return json.Marshal(struct {
		Contains string
	}{
		Contains: d.GetContains(),
	})
}

func (d *Contains) UnmarshalJSON(data []byte) error {
	var aux struct {
		Contains string
	}
	if err := json.Unmarshal(data, &aux); err != nil {
		return err
	}
	d.contains = aux.Contains
	return nil
}
