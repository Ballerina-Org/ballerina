package ballerina

import (
	"encoding/json"
)

type StartsWith struct {
	startsWith string
}

func NewStartsWith(value string) StartsWith {
	return StartsWith{startsWith: value}
}

func (c StartsWith) GetStartsWith() string {
	return c.startsWith
}

var _ json.Unmarshaler = &StartsWith{}
var _ json.Marshaler = StartsWith{}

func (d StartsWith) MarshalJSON() ([]byte, error) {
	return json.Marshal(struct {
		StartsWith string
	}{
		StartsWith: d.GetStartsWith(),
	})
}

func (d *StartsWith) UnmarshalJSON(data []byte) error {
	var aux struct {
		StartsWith string
	}
	if err := json.Unmarshal(data, &aux); err != nil {
		return err
	}
	d.startsWith = aux.StartsWith
	return nil
}
