package ballerina

import (
	"encoding/json"
	"github.com/google/uuid"
	"time"
)

type DeltaIntEffectsEnum string

const (
	IntReplace DeltaIntEffectsEnum = "IntReplace"
)

var AllDeltaIntEffectsEnumCases = [...]DeltaIntEffectsEnum{IntReplace}

func DefaultDeltaIntEffectsEnum() DeltaIntEffectsEnum { return AllDeltaIntEffectsEnumCases[0] }

type DeltaInt struct {
	DeltaBase
	discriminator DeltaIntEffectsEnum
	replace       int
}

var _ json.Unmarshaler = &DeltaInt{}
var _ json.Marshaler = DeltaInt{}

func (d DeltaInt) MarshalJSON() ([]byte, error) {
	return json.Marshal(struct {
		DeltaBase
		Discriminator DeltaIntEffectsEnum
		Replace       int
	}{
		DeltaBase:     d.DeltaBase,
		Discriminator: d.discriminator,
		Replace:       d.replace,
	})
}

func (d *DeltaInt) UnmarshalJSON(data []byte) error {
	var aux struct {
		DeltaBase
		Discriminator DeltaIntEffectsEnum
		Replace       int
	}
	if err := json.Unmarshal(data, &aux); err != nil {
		return err
	}
	d.DeltaBase = aux.DeltaBase
	d.discriminator = aux.Discriminator
	d.replace = aux.Replace
	return nil
}

func NewDeltaIntReplace(value int) DeltaInt {
	return DeltaInt{
		discriminator: IntReplace,
		replace:       value,
	}
}
func MatchDeltaInt[Result any](
	onReplace func(int) (Result, error),
) func(DeltaInt) (Result, error) {
	return func(delta DeltaInt) (Result, error) {
		var result Result
		switch delta.discriminator {
		case IntReplace:
			return onReplace(delta.replace)
		}
		return result, NewInvalidDiscriminatorError(string(delta.discriminator), "DeltaInt")
	}
}

type DeltaInt64EffectsEnum string

const (
	Int64Replace DeltaInt64EffectsEnum = "Int64Replace"
)

var AllDeltaInt64EffectsEnumCases = [...]DeltaInt64EffectsEnum{Int64Replace}

func DefaultDeltaInt64EffectsEnum() DeltaInt64EffectsEnum { return AllDeltaInt64EffectsEnumCases[0] }

type DeltaInt64 struct {
	DeltaBase
	discriminator DeltaInt64EffectsEnum
	replace       int64
}

var _ json.Unmarshaler = &DeltaInt64{}
var _ json.Marshaler = DeltaInt64{}

func (d DeltaInt64) MarshalJSON() ([]byte, error) {
	return json.Marshal(struct {
		DeltaBase
		Discriminator DeltaInt64EffectsEnum
		Replace       int64
	}{
		DeltaBase:     d.DeltaBase,
		Discriminator: d.discriminator,
		Replace:       d.replace,
	})
}

func (d *DeltaInt64) UnmarshalJSON(data []byte) error {
	var aux struct {
		DeltaBase
		Discriminator DeltaInt64EffectsEnum
		Replace       int64
	}
	if err := json.Unmarshal(data, &aux); err != nil {
		return err
	}
	d.DeltaBase = aux.DeltaBase
	d.discriminator = aux.Discriminator
	d.replace = aux.Replace
	return nil
}

func NewDeltaInt64Replace(value int64) DeltaInt64 {
	return DeltaInt64{
		discriminator: Int64Replace,
		replace:       value,
	}
}
func MatchDeltaInt64[Result any](
	onReplace func(int64) (Result, error),
) func(DeltaInt64) (Result, error) {
	return func(delta DeltaInt64) (Result, error) {
		var result Result
		switch delta.discriminator {
		case Int64Replace:
			return onReplace(delta.replace)
		}
		return result, NewInvalidDiscriminatorError(string(delta.discriminator), "DeltaInt64")
	}
}

type DeltaStringEffectsEnum string

const (
	StringReplace DeltaStringEffectsEnum = "StringReplace"
)

var AllDeltaStringEffectsEnumCases = [...]DeltaStringEffectsEnum{StringReplace}

func DefaultDeltaStringEffectsEnum() DeltaStringEffectsEnum { return AllDeltaStringEffectsEnumCases[0] }

type DeltaString struct {
	DeltaBase
	discriminator DeltaStringEffectsEnum
	replace       string
}

var _ json.Unmarshaler = &DeltaString{}
var _ json.Marshaler = DeltaString{}

func (d DeltaString) MarshalJSON() ([]byte, error) {
	return json.Marshal(struct {
		DeltaBase
		Discriminator DeltaStringEffectsEnum
		Replace       string
	}{
		DeltaBase:     d.DeltaBase,
		Discriminator: d.discriminator,
		Replace:       d.replace,
	})
}

func (d *DeltaString) UnmarshalJSON(data []byte) error {
	var aux struct {
		DeltaBase
		Discriminator DeltaStringEffectsEnum
		Replace       string
	}
	if err := json.Unmarshal(data, &aux); err != nil {
		return err
	}
	d.DeltaBase = aux.DeltaBase
	d.discriminator = aux.Discriminator
	d.replace = aux.Replace
	return nil
}

func NewDeltaStringReplace(value string) DeltaString {
	return DeltaString{
		discriminator: StringReplace,
		replace:       value,
	}
}
func MatchDeltaString[Result any](
	onReplace func(string) (Result, error),
) func(DeltaString) (Result, error) {
	return func(delta DeltaString) (Result, error) {
		var result Result
		switch delta.discriminator {
		case StringReplace:
			return onReplace(delta.replace)
		}
		return result, NewInvalidDiscriminatorError(string(delta.discriminator), "DeltaString")
	}
}

type DeltaBoolEffectsEnum string

const (
	BoolReplace DeltaBoolEffectsEnum = "BoolReplace"
)

var AllDeltaBoolEffectsEnumCases = [...]DeltaBoolEffectsEnum{BoolReplace}

func DefaultDeltaBoolEffectsEnum() DeltaBoolEffectsEnum { return AllDeltaBoolEffectsEnumCases[0] }

type DeltaBool struct {
	DeltaBase
	discriminator DeltaBoolEffectsEnum
	replace       bool
}

var _ json.Unmarshaler = &DeltaBool{}
var _ json.Marshaler = DeltaBool{}

func (d DeltaBool) MarshalJSON() ([]byte, error) {
	return json.Marshal(struct {
		DeltaBase
		Discriminator DeltaBoolEffectsEnum
		Replace       bool
	}{
		DeltaBase:     d.DeltaBase,
		Discriminator: d.discriminator,
		Replace:       d.replace,
	})
}

func (d *DeltaBool) UnmarshalJSON(data []byte) error {
	var aux struct {
		DeltaBase
		Discriminator DeltaBoolEffectsEnum
		Replace       bool
	}
	if err := json.Unmarshal(data, &aux); err != nil {
		return err
	}
	d.DeltaBase = aux.DeltaBase
	d.discriminator = aux.Discriminator
	d.replace = aux.Replace
	return nil
}

func NewDeltaBoolReplace(value bool) DeltaBool {
	return DeltaBool{
		discriminator: BoolReplace,
		replace:       value,
	}
}
func MatchDeltaBool[Result any](
	onReplace func(bool) (Result, error),
) func(DeltaBool) (Result, error) {
	return func(delta DeltaBool) (Result, error) {
		var result Result
		switch delta.discriminator {
		case BoolReplace:
			return onReplace(delta.replace)
		}
		return result, NewInvalidDiscriminatorError(string(delta.discriminator), "DeltaBool")
	}
}

type DeltaGuidEffectsEnum string

const (
	GuidReplace DeltaGuidEffectsEnum = "GuidReplace"
)

var AllDeltaGuidEffectsEnumCases = [...]DeltaGuidEffectsEnum{GuidReplace}

func DefaultDeltaGuidEffectsEnum() DeltaGuidEffectsEnum { return AllDeltaGuidEffectsEnumCases[0] }

type DeltaGuid struct {
	DeltaBase
	discriminator DeltaGuidEffectsEnum
	replace       uuid.UUID
}

var _ json.Unmarshaler = &DeltaGuid{}
var _ json.Marshaler = DeltaGuid{}

func (d DeltaGuid) MarshalJSON() ([]byte, error) {
	return json.Marshal(struct {
		DeltaBase
		Discriminator DeltaGuidEffectsEnum
		Replace       uuid.UUID
	}{
		DeltaBase:     d.DeltaBase,
		Discriminator: d.discriminator,
		Replace:       d.replace,
	})
}

func (d *DeltaGuid) UnmarshalJSON(data []byte) error {
	var aux struct {
		DeltaBase
		Discriminator DeltaGuidEffectsEnum
		Replace       uuid.UUID
	}
	if err := json.Unmarshal(data, &aux); err != nil {
		return err
	}
	d.DeltaBase = aux.DeltaBase
	d.discriminator = aux.Discriminator
	d.replace = aux.Replace
	return nil
}

func NewDeltaGuidReplace(value uuid.UUID) DeltaGuid {
	return DeltaGuid{
		discriminator: GuidReplace,
		replace:       value,
	}
}
func MatchDeltaGuid[Result any](
	onReplace func(uuid.UUID) (Result, error),
) func(DeltaGuid) (Result, error) {
	return func(delta DeltaGuid) (Result, error) {
		var result Result
		switch delta.discriminator {
		case GuidReplace:
			return onReplace(delta.replace)
		}
		return result, NewInvalidDiscriminatorError(string(delta.discriminator), "DeltaGuid")
	}
}

func DefaultGuid() uuid.UUID {
	return uuid.Nil
}

type DeltaTimeEffectsEnum string

const (
	TimeReplace DeltaTimeEffectsEnum = "TimeReplace"
)

var AllDeltaTimeEffectsEnumCases = [...]DeltaTimeEffectsEnum{TimeReplace}

func DefaultDeltaTimeEffectsEnum() DeltaTimeEffectsEnum { return AllDeltaTimeEffectsEnumCases[0] }

type DeltaTime struct {
	DeltaBase
	discriminator DeltaTimeEffectsEnum
	replace       time.Time
}

var _ json.Unmarshaler = &DeltaTime{}
var _ json.Marshaler = DeltaTime{}

func (d DeltaTime) MarshalJSON() ([]byte, error) {
	return json.Marshal(struct {
		DeltaBase
		Discriminator DeltaTimeEffectsEnum
		Replace       time.Time
	}{
		DeltaBase:     d.DeltaBase,
		Discriminator: d.discriminator,
		Replace:       d.replace,
	})
}

func (d *DeltaTime) UnmarshalJSON(data []byte) error {
	var aux struct {
		DeltaBase
		Discriminator DeltaTimeEffectsEnum
		Replace       time.Time
	}
	if err := json.Unmarshal(data, &aux); err != nil {
		return err
	}
	d.DeltaBase = aux.DeltaBase
	d.discriminator = aux.Discriminator
	d.replace = aux.Replace
	return nil
}

func NewDeltaTimeReplace(value time.Time) DeltaTime {
	return DeltaTime{
		discriminator: TimeReplace,
		replace:       value,
	}
}
func MatchDeltaTime[Result any](
	onReplace func(time.Time) (Result, error),
) func(DeltaTime) (Result, error) {
	return func(delta DeltaTime) (Result, error) {
		var result Result
		switch delta.discriminator {
		case TimeReplace:
			return onReplace(delta.replace)
		}
		return result, NewInvalidDiscriminatorError(string(delta.discriminator), "DeltaTime")
	}
}

type DeltaInt32EffectsEnum string

const (
	Int32Replace DeltaInt32EffectsEnum = "Int32Replace"
)

var AllDeltaInt32EffectsEnumCases = [...]DeltaInt32EffectsEnum{Int32Replace}

func DefaultDeltaInt32EffectsEnum() DeltaInt32EffectsEnum { return AllDeltaInt32EffectsEnumCases[0] }

type DeltaInt32 struct {
	DeltaBase
	discriminator DeltaInt32EffectsEnum
	replace       int32
}

var _ json.Unmarshaler = &DeltaInt32{}
var _ json.Marshaler = DeltaInt32{}

func (d DeltaInt32) MarshalJSON() ([]byte, error) {
	return json.Marshal(struct {
		DeltaBase
		Discriminator DeltaInt32EffectsEnum
		Replace       int32
	}{
		DeltaBase:     d.DeltaBase,
		Discriminator: d.discriminator,
		Replace:       d.replace,
	})
}

func (d *DeltaInt32) UnmarshalJSON(data []byte) error {
	var aux struct {
		DeltaBase
		Discriminator DeltaInt32EffectsEnum
		Replace       int32
	}
	if err := json.Unmarshal(data, &aux); err != nil {
		return err
	}
	d.DeltaBase = aux.DeltaBase
	d.discriminator = aux.Discriminator
	d.replace = aux.Replace
	return nil
}

func NewDeltaInt32Replace(value int32) DeltaInt32 {
	return DeltaInt32{
		discriminator: Int32Replace,
		replace:       value,
	}
}
func MatchDeltaInt32[Result any](
	onReplace func(int32) (Result, error),
) func(DeltaInt32) (Result, error) {
	return func(delta DeltaInt32) (Result, error) {
		var result Result
		switch delta.discriminator {
		case Int32Replace:
			return onReplace(delta.replace)
		}
		return result, NewInvalidDiscriminatorError(string(delta.discriminator), "DeltaInt32")
	}
}

type DeltaFloat32EffectsEnum string

const (
	Float32Replace DeltaFloat32EffectsEnum = "Float32Replace"
)

var AllDeltaFloat32EffectsEnumCases = [...]DeltaFloat32EffectsEnum{Float32Replace}

func DefaultDeltaFloat32EffectsEnum() DeltaFloat32EffectsEnum {
	return AllDeltaFloat32EffectsEnumCases[0]
}

type DeltaFloat32 struct {
	DeltaBase
	discriminator DeltaFloat32EffectsEnum
	replace       float32
}

var _ json.Unmarshaler = &DeltaFloat32{}
var _ json.Marshaler = DeltaFloat32{}

func (d DeltaFloat32) MarshalJSON() ([]byte, error) {
	return json.Marshal(struct {
		DeltaBase
		Discriminator DeltaFloat32EffectsEnum
		Replace       float32
	}{
		DeltaBase:     d.DeltaBase,
		Discriminator: d.discriminator,
		Replace:       d.replace,
	})
}

func (d *DeltaFloat32) UnmarshalJSON(data []byte) error {
	var aux struct {
		DeltaBase
		Discriminator DeltaFloat32EffectsEnum
		Replace       float32
	}
	if err := json.Unmarshal(data, &aux); err != nil {
		return err
	}
	d.DeltaBase = aux.DeltaBase
	d.discriminator = aux.Discriminator
	d.replace = aux.Replace
	return nil
}

func NewDeltaFloat32Replace(value float32) DeltaFloat32 {
	return DeltaFloat32{
		discriminator: Float32Replace,
		replace:       value,
	}
}
func MatchDeltaFloat32[Result any](
	onReplace func(float32) (Result, error),
) func(DeltaFloat32) (Result, error) {
	return func(delta DeltaFloat32) (Result, error) {
		var result Result
		switch delta.discriminator {
		case Float32Replace:
			return onReplace(delta.replace)
		}
		return result, NewInvalidDiscriminatorError(string(delta.discriminator), "DeltaFloat32")
	}
}

type DeltaFloat64EffectsEnum string

const (
	Float64Replace DeltaFloat64EffectsEnum = "Float64Replace"
)

var AllDeltaFloat64EffectsEnumCases = [...]DeltaFloat64EffectsEnum{Float64Replace}

func DefaultDeltaFloat64EffectsEnum() DeltaFloat64EffectsEnum {
	return AllDeltaFloat64EffectsEnumCases[0]
}

type DeltaFloat64 struct {
	DeltaBase
	discriminator DeltaFloat64EffectsEnum
	replace       float64
}

var _ json.Unmarshaler = &DeltaFloat64{}
var _ json.Marshaler = DeltaFloat64{}

func (d DeltaFloat64) MarshalJSON() ([]byte, error) {
	return json.Marshal(struct {
		DeltaBase
		Discriminator DeltaFloat64EffectsEnum
		Replace       float64
	}{
		DeltaBase:     d.DeltaBase,
		Discriminator: d.discriminator,
		Replace:       d.replace,
	})
}

func (d *DeltaFloat64) UnmarshalJSON(data []byte) error {
	var aux struct {
		DeltaBase
		Discriminator DeltaFloat64EffectsEnum
		Replace       float64
	}
	if err := json.Unmarshal(data, &aux); err != nil {
		return err
	}
	d.DeltaBase = aux.DeltaBase
	d.discriminator = aux.Discriminator
	d.replace = aux.Replace
	return nil
}

func NewDeltaFloat64Replace(value float64) DeltaFloat64 {
	return DeltaFloat64{
		discriminator: Float64Replace,
		replace:       value,
	}
}
func MatchDeltaFloat64[Result any](
	onReplace func(float64) (Result, error),
) func(DeltaFloat64) (Result, error) {
	return func(delta DeltaFloat64) (Result, error) {
		var result Result
		switch delta.discriminator {
		case Float64Replace:
			return onReplace(delta.replace)
		}
		return result, NewInvalidDiscriminatorError(string(delta.discriminator), "DeltaFloat64")
	}
}
