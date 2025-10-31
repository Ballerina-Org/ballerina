package ballerinawriter

import (
	ballerina "ballerina.com/core"
	"ballerina.com/core/ballerinareader"
	"ballerina.com/core/ballerinaupdater"
)

func NewWriter[Edit, Entity any](apply func(Edit) ballerinaupdater.Updater[Entity]) Writer[Edit, Entity] {
	return Writer[Edit, Entity]{
		Apply: apply,
	}
}

func NewWriterWithError[Edit, Entity any](apply func(Edit) ballerinaupdater.UpdaterWithError[Entity]) WriterWithError[Edit, Entity] {
	return WriterWithError[Edit, Entity]{
		Apply: apply,
	}
}

func ReaderWriter[T, Edit, Entity any](reader ballerinareader.ReaderWithError[Entity, T], writer WriterWithError[ballerina.Tuple2[T, Edit], Entity]) WriterWithError[Edit, Entity] {
	return NewWriterWithError[Edit, Entity](func(edit Edit) ballerinaupdater.UpdaterWithError[Entity] {
		return ballerinaupdater.NewUpdaterWithError(func(entity Entity) (Entity, error) {
			read, err := reader.Apply(entity)
			if err != nil {
				return entity, err
			}
			return writer.Apply(ballerina.NewTuple2(read, edit)).ApplyTo(entity)
		})
	})
}
