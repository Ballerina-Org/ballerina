package ballerina

import (
	"encoding/json"
)

type IsNull struct {
	isNull Unit
}

func NewIsNull() IsNull {
	return IsNull{}
}

var _ json.Unmarshaler = &IsNull{}
var _ json.Marshaler = IsNull{}

func (d IsNull) MarshalJSON() ([]byte, error) {
	return json.Marshal(struct {
		IsNull Unit
	}{
		IsNull: d.isNull,
	})
}

func (d *IsNull) UnmarshalJSON(data []byte) error {
	var aux struct {
		IsNull Unit
	}
	if err := json.Unmarshal(data, &aux); err != nil {
		return err
	}
	return nil
}
