package ballerinawriter

import (
	"ballerina.com/core/ballerinareader"
	"ballerina.com/core/ballerinaupdater"
)

func NewWritableField[FieldType, Record any](
	get func(Record) FieldType,
	set func(Record, FieldType) Record,
) WritableField[FieldType, Record] {
	return WritableField[FieldType, Record]{
		Reader: ballerinareader.NewReader(get),
		Writer: NewWriter[FieldType, Record](
			func(edit FieldType) ballerinaupdater.Updater[Record] {
				return ballerinaupdater.NewUpdater(func(r Record) Record {
					return set(r, edit)
				})
			},
		),
	}
}

func NewWritableFieldWithError[FieldType, Record any](
	get func(Record) FieldType,
	set func(Record, FieldType) (Record, error),
) WritableFieldWithError[FieldType, Record] {
	return WritableFieldWithError[FieldType, Record]{
		Reader: ballerinareader.NewReader(get),
		Writer: NewWriterWithError[FieldType, Record](
			func(edit FieldType) ballerinaupdater.UpdaterWithError[Record] {
				return ballerinaupdater.NewUpdaterWithError(func(r Record) (Record, error) {
					return set(r, edit)
				})
			},
		),
	}
}
