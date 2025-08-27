package ballerina

import (
	"encoding/json"
	"time"

	"github.com/google/uuid"
)

type deltaIntEffectsEnum string

const (
	intReplace deltaIntEffectsEnum = "IntReplace"
)

var allDeltaIntEffectsEnumCases = [...]deltaIntEffectsEnum{intReplace}

func DefaultDeltaIntEffectsEnum() deltaIntEffectsEnum { return allDeltaIntEffectsEnumCases[0] }

type DeltaInt struct {
	DeltaBase
	discriminator deltaIntEffectsEnum
	replace       int
}

var _ json.Unmarshaler = &DeltaInt{}
var _ json.Marshaler = DeltaInt{}

func (d DeltaInt) MarshalJSON() ([]byte, error) {
	return json.Marshal(struct {
		DeltaBase
		Discriminator deltaIntEffectsEnum
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
		Discriminator deltaIntEffectsEnum
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
		discriminator: intReplace,
		replace:       value,
	}
}
func MatchDeltaInt[context any, Result any](
	onReplace func(int) func(ReaderWithError[context, int]) (Result, error),
) func(DeltaInt) func(ReaderWithError[context, int]) (Result, error) {
	return func(delta DeltaInt) func(ReaderWithError[context, int]) (Result, error) {
		return func(value ReaderWithError[context, int]) (Result, error) {
			var result Result
			switch delta.discriminator {
			case intReplace:
				return onReplace(delta.replace)(value)
			}
			return result, NewInvalidDiscriminatorError(string(delta.discriminator), "DeltaInt")
		}
	}
}

type deltaInt64EffectsEnum string

const (
	int64Replace deltaInt64EffectsEnum = "Int64Replace"
)

var allDeltaInt64EffectsEnumCases = [...]deltaInt64EffectsEnum{int64Replace}

func DefaultDeltaInt64EffectsEnum() deltaInt64EffectsEnum { return allDeltaInt64EffectsEnumCases[0] }

type DeltaInt64 struct {
	DeltaBase
	discriminator deltaInt64EffectsEnum
	replace       int64
}

var _ json.Unmarshaler = &DeltaInt64{}
var _ json.Marshaler = DeltaInt64{}

func (d DeltaInt64) MarshalJSON() ([]byte, error) {
	return json.Marshal(struct {
		DeltaBase
		Discriminator deltaInt64EffectsEnum
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
		Discriminator deltaInt64EffectsEnum
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
		discriminator: int64Replace,
		replace:       value,
	}
}
func MatchDeltaInt64[context any, Result any](
	onReplace func(int64) func(ReaderWithError[context, int64]) (Result, error),
) func(DeltaInt64) func(ReaderWithError[context, int64]) (Result, error) {
	return func(delta DeltaInt64) func(ReaderWithError[context, int64]) (Result, error) {
		return func(value ReaderWithError[context, int64]) (Result, error) {
			var result Result
			switch delta.discriminator {
			case int64Replace:
				return onReplace(delta.replace)(value)
			}
			return result, NewInvalidDiscriminatorError(string(delta.discriminator), "DeltaInt64")
		}
	}
}

type deltaStringEffectsEnum string

const (
	stringReplace deltaStringEffectsEnum = "StringReplace"
)

var allDeltaStringEffectsEnumCases = [...]deltaStringEffectsEnum{stringReplace}

func DefaultDeltaStringEffectsEnum() deltaStringEffectsEnum { return allDeltaStringEffectsEnumCases[0] }

type DeltaString struct {
	DeltaBase
	discriminator deltaStringEffectsEnum
	replace       string
}

var _ json.Unmarshaler = &DeltaString{}
var _ json.Marshaler = DeltaString{}

func (d DeltaString) MarshalJSON() ([]byte, error) {
	return json.Marshal(struct {
		DeltaBase
		Discriminator deltaStringEffectsEnum
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
		Discriminator deltaStringEffectsEnum
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
		discriminator: stringReplace,
		replace:       value,
	}
}
func MatchDeltaString[context any, Result any](
	onReplace func(string) func(ReaderWithError[context, string]) (Result, error),
) func(DeltaString) func(ReaderWithError[context, string]) (Result, error) {
	return func(delta DeltaString) func(ReaderWithError[context, string]) (Result, error) {
		return func(value ReaderWithError[context, string]) (Result, error) {
			var result Result
			switch delta.discriminator {
			case stringReplace:
				return onReplace(delta.replace)(value)
			}
			return result, NewInvalidDiscriminatorError(string(delta.discriminator), "DeltaString")
		}
	}
}

type deltaBoolEffectsEnum string

const (
	boolReplace deltaBoolEffectsEnum = "BoolReplace"
)

var allDeltaBoolEffectsEnumCases = [...]deltaBoolEffectsEnum{boolReplace}

func DefaultDeltaBoolEffectsEnum() deltaBoolEffectsEnum { return allDeltaBoolEffectsEnumCases[0] }

type DeltaBool struct {
	DeltaBase
	discriminator deltaBoolEffectsEnum
	replace       bool
}

var _ json.Unmarshaler = &DeltaBool{}
var _ json.Marshaler = DeltaBool{}

func (d DeltaBool) MarshalJSON() ([]byte, error) {
	return json.Marshal(struct {
		DeltaBase
		Discriminator deltaBoolEffectsEnum
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
		Discriminator deltaBoolEffectsEnum
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
		discriminator: boolReplace,
		replace:       value,
	}
}
func MatchDeltaBool[context any, Result any](
	onReplace func(bool) func(ReaderWithError[context, bool]) (Result, error),
) func(DeltaBool) func(ReaderWithError[context, bool]) (Result, error) {
	return func(delta DeltaBool) func(ReaderWithError[context, bool]) (Result, error) {
		return func(value ReaderWithError[context, bool]) (Result, error) {
			var result Result
			switch delta.discriminator {
			case boolReplace:
				return onReplace(delta.replace)(value)
			}
			return result, NewInvalidDiscriminatorError(string(delta.discriminator), "DeltaBool")
		}
	}
}

type deltaGuidEffectsEnum string

const (
	guidReplace deltaGuidEffectsEnum = "GuidReplace"
)

var allDeltaGuidEffectsEnumCases = [...]deltaGuidEffectsEnum{guidReplace}

func DefaultDeltaGuidEffectsEnum() deltaGuidEffectsEnum { return allDeltaGuidEffectsEnumCases[0] }

type DeltaGuid struct {
	DeltaBase
	discriminator deltaGuidEffectsEnum
	replace       uuid.UUID
}

var _ json.Unmarshaler = &DeltaGuid{}
var _ json.Marshaler = DeltaGuid{}

func (d DeltaGuid) MarshalJSON() ([]byte, error) {
	return json.Marshal(struct {
		DeltaBase
		Discriminator deltaGuidEffectsEnum
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
		Discriminator deltaGuidEffectsEnum
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
		discriminator: guidReplace,
		replace:       value,
	}
}
func MatchDeltaGuid[context any, Result any](
	onReplace func(uuid.UUID) func(ReaderWithError[context, uuid.UUID]) (Result, error),
) func(DeltaGuid) func(ReaderWithError[context, uuid.UUID]) (Result, error) {
	return func(delta DeltaGuid) func(ReaderWithError[context, uuid.UUID]) (Result, error) {
		return func(value ReaderWithError[context, uuid.UUID]) (Result, error) {
			var result Result
			switch delta.discriminator {
			case guidReplace:
				return onReplace(delta.replace)(value)
			}
			return result, NewInvalidDiscriminatorError(string(delta.discriminator), "DeltaGuid")
		}
	}
}

func DefaultGuid() uuid.UUID {
	return uuid.Nil
}

type deltaTimeEffectsEnum string

const (
	timeReplace deltaTimeEffectsEnum = "TimeReplace"
)

var allDeltaTimeEffectsEnumCases = [...]deltaTimeEffectsEnum{timeReplace}

func DefaultDeltaTimeEffectsEnum() deltaTimeEffectsEnum { return allDeltaTimeEffectsEnumCases[0] }

type DeltaTime struct {
	DeltaBase
	discriminator deltaTimeEffectsEnum
	replace       time.Time
}

var _ json.Unmarshaler = &DeltaTime{}
var _ json.Marshaler = DeltaTime{}

func (d DeltaTime) MarshalJSON() ([]byte, error) {
	return json.Marshal(struct {
		DeltaBase
		Discriminator deltaTimeEffectsEnum
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
		Discriminator deltaTimeEffectsEnum
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
		discriminator: timeReplace,
		replace:       value,
	}
}
func MatchDeltaTime[context any, Result any](
	onReplace func(time.Time) func(ReaderWithError[context, time.Time]) (Result, error),
) func(DeltaTime) func(ReaderWithError[context, time.Time]) (Result, error) {
	return func(delta DeltaTime) func(ReaderWithError[context, time.Time]) (Result, error) {
		return func(value ReaderWithError[context, time.Time]) (Result, error) {
			var result Result
			switch delta.discriminator {
			case timeReplace:
				return onReplace(delta.replace)(value)
			}
			return result, NewInvalidDiscriminatorError(string(delta.discriminator), "DeltaTime")
		}
	}
}

type deltaInt32EffectsEnum string

const (
	int32Replace deltaInt32EffectsEnum = "Int32Replace"
)

var allDeltaInt32EffectsEnumCases = [...]deltaInt32EffectsEnum{int32Replace}

func DefaultDeltaInt32EffectsEnum() deltaInt32EffectsEnum { return allDeltaInt32EffectsEnumCases[0] }

type DeltaInt32 struct {
	DeltaBase
	discriminator deltaInt32EffectsEnum
	replace       int32
}

var _ json.Unmarshaler = &DeltaInt32{}
var _ json.Marshaler = DeltaInt32{}

func (d DeltaInt32) MarshalJSON() ([]byte, error) {
	return json.Marshal(struct {
		DeltaBase
		Discriminator deltaInt32EffectsEnum
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
		Discriminator deltaInt32EffectsEnum
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
		discriminator: int32Replace,
		replace:       value,
	}
}
func MatchDeltaInt32[context any, Result any](
	onReplace func(int32) func(ReaderWithError[context, int32]) (Result, error),
) func(DeltaInt32) func(ReaderWithError[context, int32]) (Result, error) {
	return func(delta DeltaInt32) func(ReaderWithError[context, int32]) (Result, error) {
		return func(value ReaderWithError[context, int32]) (Result, error) {
			var result Result
			switch delta.discriminator {
			case int32Replace:
				return onReplace(delta.replace)(value)
			}
			return result, NewInvalidDiscriminatorError(string(delta.discriminator), "DeltaInt32")
		}
	}
}

type deltaFloat32EffectsEnum string

const (
	float32Replace deltaFloat32EffectsEnum = "Float32Replace"
)

var allDeltaFloat32EffectsEnumCases = [...]deltaFloat32EffectsEnum{float32Replace}

func DefaultDeltaFloat32EffectsEnum() deltaFloat32EffectsEnum {
	return allDeltaFloat32EffectsEnumCases[0]
}

type DeltaFloat32 struct {
	DeltaBase
	discriminator deltaFloat32EffectsEnum
	replace       float32
}

var _ json.Unmarshaler = &DeltaFloat32{}
var _ json.Marshaler = DeltaFloat32{}

func (d DeltaFloat32) MarshalJSON() ([]byte, error) {
	return json.Marshal(struct {
		DeltaBase
		Discriminator deltaFloat32EffectsEnum
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
		Discriminator deltaFloat32EffectsEnum
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
		discriminator: float32Replace,
		replace:       value,
	}
}
func MatchDeltaFloat32[context any, Result any](
	onReplace func(float32) func(ReaderWithError[context, float32]) (Result, error),
) func(DeltaFloat32) func(ReaderWithError[context, float32]) (Result, error) {
	return func(delta DeltaFloat32) func(ReaderWithError[context, float32]) (Result, error) {
		return func(value ReaderWithError[context, float32]) (Result, error) {
			var result Result
			switch delta.discriminator {
			case float32Replace:
				return onReplace(delta.replace)(value)
			}
			return result, NewInvalidDiscriminatorError(string(delta.discriminator), "DeltaFloat32")
		}
	}
}

type deltaFloat64EffectsEnum string

const (
	float64Replace deltaFloat64EffectsEnum = "Float64Replace"
)

var allDeltaFloat64EffectsEnumCases = [...]deltaFloat64EffectsEnum{float64Replace}

func DefaultDeltaFloat64EffectsEnum() deltaFloat64EffectsEnum {
	return allDeltaFloat64EffectsEnumCases[0]
}

type DeltaFloat64 struct {
	DeltaBase
	discriminator deltaFloat64EffectsEnum
	replace       float64
}

var _ json.Unmarshaler = &DeltaFloat64{}
var _ json.Marshaler = DeltaFloat64{}

func (d DeltaFloat64) MarshalJSON() ([]byte, error) {
	return json.Marshal(struct {
		DeltaBase
		Discriminator deltaFloat64EffectsEnum
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
		Discriminator deltaFloat64EffectsEnum
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
		discriminator: float64Replace,
		replace:       value,
	}
}
func MatchDeltaFloat64[context any, Result any](
	onReplace func(float64) func(ReaderWithError[context, float64]) (Result, error),
) func(DeltaFloat64) func(ReaderWithError[context, float64]) (Result, error) {
	return func(delta DeltaFloat64) func(ReaderWithError[context, float64]) (Result, error) {
		return func(value ReaderWithError[context, float64]) (Result, error) {
			var result Result
			switch delta.discriminator {
			case float64Replace:
				return onReplace(delta.replace)(value)
			}
			return result, NewInvalidDiscriminatorError(string(delta.discriminator), "DeltaFloat64")
		}
	}
}
