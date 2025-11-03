package ballerinawriter

import (
	"fmt"

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
			decoratedReader := ballerinareader.DecorateReaderError[Entity, T](func(err error) error {
				return fmt.Errorf("readerWriter error in reader: %w", err)
			})(reader)
			read, err := decoratedReader.Apply(entity)
			if err != nil {
				return entity, err
			}
			decoratedWriter := DecorateWriterError[ballerina.Tuple2[T, Edit], Entity](func(err error) error {
				return fmt.Errorf("readerWriter error in writer: %w", err)
			})(writer)
			return decoratedWriter.Apply(ballerina.NewTuple2(read, edit)).ApplyTo(entity)
		})
	})
}

func DecorateWriterError[Edit, Entity any](errorFn func(err error) error) func(m WriterWithError[Edit, Entity]) WriterWithError[Edit, Entity] {
	return func(m WriterWithError[Edit, Entity]) WriterWithError[Edit, Entity] {
		return WriterWithError[Edit, Entity]{
			Apply: func(edit Edit) ballerinaupdater.UpdaterWithError[Entity] {
				return ballerinaupdater.DecorateUpdaterError[Entity](errorFn)(m.Apply(edit))
			},
		}
	}
}
