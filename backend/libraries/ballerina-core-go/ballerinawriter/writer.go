package ballerinawriter

import "ballerina.com/core/ballerinaupdater"

type Writer[Edit, Entity any] struct {
	Apply func(Edit) ballerinaupdater.Updater[Entity]
}

type WriterWithError[Edit, Entity any] struct {
	Apply func(Edit) ballerinaupdater.UpdaterWithError[Entity]
}
