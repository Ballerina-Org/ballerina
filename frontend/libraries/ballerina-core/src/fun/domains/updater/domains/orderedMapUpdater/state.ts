import { BasicFun, Fun } from "../../../../state";
import { unit, Unit } from "../../../unit/state";
import { BasicUpdater, Updater } from "../../state";
import { OrderedMap } from "immutable";

export type OrderedMapUpdater<
  Entity,
  Field extends keyof Entity,
  DisplayName extends string,
> = Entity extends {
  [_ in Field]: OrderedMap<infer key, infer value>;
}
  ? {
      [f in DisplayName]: {
        add: Fun<[key, value], Updater<Entity>>;
        remove: Fun<key, Updater<Entity>>;
        concat: Fun<OrderedMap<key, value>, Updater<Entity>>;
        set: Fun<key, Fun<BasicUpdater<value>, Updater<Entity>>>;
        upsert: Fun<
          [key, BasicFun<Unit, value>, BasicUpdater<value>],
          Updater<Entity>
        >;
      };
    }
  : "Error: orderedMapUpdater has been invoked on a field which is not an OrderedMap";

export const orderedMapUpdater =
  <Entity>() =>
  <Field extends keyof Entity, DisplayName extends string>(
    field: Field,
    displayName: DisplayName,
  ): OrderedMapUpdater<Entity, Field, DisplayName> =>
    ({
      [displayName]: {
        add: Fun(
          ([key, value]: [unknown, unknown]): Updater<Entity> =>
            Updater(
              (entity) =>
                ({
                  ...entity,
                  [field]: (entity[field] as OrderedMap<unknown, unknown>).set(
                    key,
                    value,
                  ),
                }) as Entity,
            ),
        ),
        remove: Fun(
          (key: unknown): Updater<Entity> =>
            Updater(
              (entity) =>
                ({
                  ...entity,
                  [field]: (
                    entity[field] as OrderedMap<unknown, unknown>
                  ).remove(key),
                }) as Entity,
            ),
        ),
        concat: Fun(
          (other: OrderedMap<unknown, unknown>): Updater<Entity> =>
            Updater(
              (entity) =>
                ({
                  ...entity,
                  [field]: (
                    entity[field] as OrderedMap<unknown, unknown>
                  ).concat(other),
                }) as Entity,
            ),
        ),
        set: Fun((key: unknown) =>
          Fun(
            (fieldUpdater: BasicUpdater<unknown>): Updater<Entity> =>
              Updater(
                (entity) =>
                  ({
                    ...entity,
                    [field]:
                      (entity[field] as OrderedMap<unknown, unknown>).has(
                        key,
                      ) == false
                        ? entity[field]
                        : (entity[field] as OrderedMap<unknown, unknown>).set(
                            key,
                            fieldUpdater(
                              (
                                entity[field] as OrderedMap<unknown, unknown>
                              ).get(key)!,
                            ),
                          ),
                  }) as Entity,
              ),
          ),
        ),
        upsert: Fun(
          ([key, fallback, fieldUpdater]: [
            unknown,
            BasicFun<Unit, unknown>,
            BasicUpdater<unknown>,
          ]): Updater<Entity> =>
            Updater(
              (entity) =>
                ({
                  ...entity,
                  [field]: (entity[field] as OrderedMap<unknown, unknown>).set(
                    key,
                    (entity[field] as OrderedMap<unknown, unknown>).has(key)
                      ? fieldUpdater(
                          (entity[field] as OrderedMap<unknown, unknown>).get(
                            key,
                          )!,
                        )
                      : fieldUpdater(fallback(unit)),
                  ),
                }) as Entity,
            ),
        ),
      },
    }) as OrderedMapUpdater<Entity, Field, DisplayName>;
