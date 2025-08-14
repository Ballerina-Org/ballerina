package ballerina

import (
	"encoding/json"
)

type IsNull struct {
	isNull interface{}
}

func NewIsNull() IsNull {
	return IsNull{}
}

var _ json.Unmarshaler = &IsNull{}
var _ json.Marshaler = IsNull{}

func (d IsNull) MarshalJSON() ([]byte, error) {
	return json.Marshal(struct {
		IsNull interface{}
	}{
		IsNull: d.isNull,
	})
}

func (d *IsNull) UnmarshalJSON(data []byte) error {
	var aux struct {
		IsNull interface{}
	}
	if err := json.Unmarshal(data, &aux); err != nil {
		return err
	}
	return nil
}
