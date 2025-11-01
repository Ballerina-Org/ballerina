package ballerinawriter

import "ballerina.com/core/ballerinareader"

type WritableField[FieldType, Record any] struct {
	Reader ballerinareader.Reader[Record, FieldType]
	Writer Writer[FieldType, Record]
}

type WritableFieldWithError[FieldType, Record any] struct {
	Reader ballerinareader.Reader[Record, FieldType]
	Writer WriterWithError[FieldType, Record]
}
