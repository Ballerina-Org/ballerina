package ballerinaupdater

import (
	"fmt"
)

func Then[Ctx any](updaterA Updater[Ctx], updaterB Updater[Ctx]) Updater[Ctx] {
	return Updater[Ctx]{
		ApplyTo: func(input Ctx) Ctx {
			return updaterB.ApplyTo(updaterA.ApplyTo(input))
		},
	}
}

func ThenMany[Ctx any](updaters []UpdaterWithError[Ctx]) UpdaterWithError[Ctx] {
	return NewUpdaterWithError(func(v Ctx) (Ctx, error) {
		var err error
		for i := range updaters {
			v, err = updaters[i].ApplyTo(v)
			if err != nil {
				return v, fmt.Errorf("ThenMany error in updater_%d: %w", i, err)
			}
		}
		return v, nil
	})
}

func ThenWithError[Ctx any](updaterA UpdaterWithError[Ctx], updaterB UpdaterWithError[Ctx]) UpdaterWithError[Ctx] {
	return UpdaterWithError[Ctx]{
		ApplyTo: func(input Ctx) (Ctx, error) {
			decoratedUpdaterA := DecorateUpdaterError[Ctx](func(err error) error {
				return fmt.Errorf("thenWithError error in updaterA: %w", err)
			})(updaterA)
			apply, err := decoratedUpdaterA.ApplyTo(input)
			if err != nil {
				return apply, err
			}
			decoratedUpdaterB := DecorateUpdaterError[Ctx](func(err error) error {
				return fmt.Errorf("thenWithError error in updaterB: %w", err)
			})(updaterB)
			return decoratedUpdaterB.ApplyTo(apply)
		},
	}
}

func DecorateUpdaterError[Ctx any](f func(err error) error) func(m UpdaterWithError[Ctx]) UpdaterWithError[Ctx] {
	return func(m UpdaterWithError[Ctx]) UpdaterWithError[Ctx] {
		return UpdaterWithError[Ctx]{
			ApplyTo: func(input Ctx) (Ctx, error) {
				mapped, err := m.ApplyTo(input)
				if err != nil {
					return mapped, f(err)
				}
				return mapped, nil
			},
		}
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
		for i, updater := range updaters {
			decoratedUpdater := DecorateUpdaterError[Entity](func(err error) error {
				return fmt.Errorf("ApplyAll error in updater_%d: %w", i, err)
			})(updater)
			updated, err := decoratedUpdater.ApplyTo(entity)
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
