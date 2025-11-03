package ballerinaupdater

func Then[Ctx any](updaterA Updater[Ctx], updaterB Updater[Ctx]) Updater[Ctx] {
	return Updater[Ctx]{
		ApplyTo: func(input Ctx) Ctx {
			return updaterB.ApplyTo(updaterA.ApplyTo(input))
		},
	}
}

func ThenWithError[Ctx any](updaterA UpdaterWithError[Ctx], updaterB UpdaterWithError[Ctx]) UpdaterWithError[Ctx] {
	return UpdaterWithError[Ctx]{
		ApplyTo: func(input Ctx) (Ctx, error) {
			apply, err := updaterA.ApplyTo(input)
			if err != nil {
				return apply, err
			}
			return updaterB.ApplyTo(apply)
		},
	}
}

func ToUpdaterWithError[Ctx any](reader Updater[Ctx]) UpdaterWithError[Ctx] {
	return UpdaterWithError[Ctx]{
		ApplyTo: func(input Ctx) (Ctx, error) { return reader.ApplyTo(input), nil },
	}
}

func NewUpdater[T any](convert func(T) T) Updater[T] {
	return Updater[T]{
		ApplyTo: func(input T) T {
			return convert(input)
		},
	}
}

func NewUpdaterWithError[T any](convert func(T) (T, error)) UpdaterWithError[T] {
	return UpdaterWithError[T]{
		ApplyTo: func(input T) (T, error) {
			return convert(input)
		},
	}
}

func ApplyAll[Entity any](updaters []UpdaterWithError[Entity]) UpdaterWithError[Entity] {
	return NewUpdaterWithError(func(entity Entity) (Entity, error) {
		for _, updater := range updaters {
			updated, err := updater.ApplyTo(entity)
			if err != nil {
				return entity, err
			}
			entity = updated
		}
		return entity, nil
	})
}

func ReplaceWith[T any](value T) Updater[T] {
	return NewUpdater(func(_ T) T {
		return value
	})
}

func ReplaceWithWithError[T any](value T) UpdaterWithError[T] {
	return NewUpdaterWithError(func(_ T) (T, error) {
		return value, nil
	})
}
