package ballerinaupdater

type Updater[Ctx any] struct {
	ApplyTo func(Ctx) Ctx
}

type UpdaterWithError[Ctx any] struct {
	ApplyTo func(Ctx) (Ctx, error)
}
