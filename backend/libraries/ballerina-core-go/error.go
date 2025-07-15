package ballerina

import (
	"fmt"
)

type LookupStreamNotFoundError struct {
	EntityName       string
	LookupStreamName string
}

func (err *LookupStreamNotFoundError) Error() string {
	return fmt.Sprintf("%s/%s is not a valid Entity/LookupStream name", err.EntityName, err.LookupStreamName)
}

func NewLookupStreamNotFoundError(EntityName string, LookupStreamName string) error {
	return &LookupStreamNotFoundError{EntityName: EntityName, LookupStreamName: LookupStreamName}
}

type OneNotFoundError struct {
	EntityName string
	OneName    string
}

func (err *OneNotFoundError) Error() string {
	return fmt.Sprintf("%s/%s is not a valid Entity/One name", err.EntityName, err.OneName)
}

func NewOneNotFoundError(EntityName string, OneName string) error {
	return &OneNotFoundError{EntityName: EntityName, OneName: OneName}
}

type ManyNotFoundError struct {
	EntityName string
	ManyName   string
}

func (err *ManyNotFoundError) Error() string {
	return fmt.Sprintf("%s/%s is not a valid Entity/Many name", err.EntityName, err.ManyName)
}

func NewManyNotFoundError(EntityName string, ManyName string) error {
	return &ManyNotFoundError{EntityName: EntityName, ManyName: ManyName}
}

type TableNotFoundError struct {
	TableName string
}

func (err *TableNotFoundError) Error() string {
	return fmt.Sprintf("%s is not a valid Table name", err.TableName)
}

func NewTableNotFoundError(TableName string) error {
	return &TableNotFoundError{TableName: TableName}
}

type EntityNotFoundError struct {
	EntityName string
}

func (err *EntityNotFoundError) Error() string {
	return fmt.Sprintf("%s is not a valid entity name", err.EntityName)
}

func NewEntityNotFoundError(entityName string) error {
	return &EntityNotFoundError{EntityName: entityName}
}

type StreamNotFoundError struct {
	StreamName string
}

func (err *StreamNotFoundError) Error() string {
	return fmt.Sprintf("%s is not a valid stream name", err.StreamName)
}

func NewStreamNotFoundError(streamName string) error {
	return &StreamNotFoundError{StreamName: streamName}
}

type EnumNotFoundError struct {
	EnumName string
}

func (err *EnumNotFoundError) Error() string {
	return fmt.Sprintf("%s is not a valid enum name", err.EnumName)
}

func NewEnumNotFoundError(enumName string) error {
	return &EnumNotFoundError{EnumName: enumName}
}

type InvalidEnumValueCombinationError struct {
	EnumName  string
	EnumValue string
}

func (err *InvalidEnumValueCombinationError) Error() string {
	return fmt.Sprintf("%s/%s is not a valid enum/value combination", err.EnumName, err.EnumValue)
}

func NewInvalidEnumValueCombinationError(enumName string, enumValue string) error {
	return &InvalidEnumValueCombinationError{EnumName: enumName, EnumValue: enumValue}
}

type EntityNameAndDeltaTypeMismatch struct {
	EntityName string
	Delta      DeltaBase
}

func (err *EntityNameAndDeltaTypeMismatch) Error() string {
	return fmt.Sprintf("%s/%A is not a valid entity name/delta combination", err.EntityName, err.Delta)
}

func NewEntityNameAndDeltaTypeMismatch(entityName string, delta DeltaBase) error {
	return &EntityNameAndDeltaTypeMismatch{EntityName: entityName, Delta: delta}
}

type InvalidDiscriminatorError struct {
	DiscriminatorValue string
	TypeName           string
}

func (err *InvalidDiscriminatorError) Error() string {
	return fmt.Sprintf("%s is not a valid discriminator for type %s", err.DiscriminatorValue, err.TypeName)
}

func NewInvalidDiscriminatorError(discriminatorValue string, typeName string) error {
	return &InvalidDiscriminatorError{DiscriminatorValue: discriminatorValue, TypeName: typeName}
}

type ReadOnlyDeltaCalledError struct {
}

func (err *ReadOnlyDeltaCalledError) Error() string {
	return "read-only, delta calls are not allowed"
}

func NewReadOnlyDeltaCalledError() error {
	return &ReadOnlyDeltaCalledError{}
}
