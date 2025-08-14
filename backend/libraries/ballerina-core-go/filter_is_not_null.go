package ballerina

import (
	"encoding/json"
)

type IsNotNull struct {
	isNotNull interface{}
}

func NewIsNotNull() IsNotNull {
	return IsNotNull{}
}

var _ json.Unmarshaler = &IsNotNull{}
var _ json.Marshaler = IsNotNull{}

func (d IsNotNull) MarshalJSON() ([]byte, error) {
	return json.Marshal(struct {
		IsNotNull interface{}
	}{
		IsNotNull: d.isNotNull,
	})
}

func (d *IsNotNull) UnmarshalJSON(data []byte) error {
	var aux struct {
		IsNotNull interface{}
	}
	if err := json.Unmarshal(data, &aux); err != nil {
		return err
	}
	return nil
}
