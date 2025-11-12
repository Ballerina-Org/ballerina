import { BasicFun } from "../../../../state";
import { BasicUpdater, Updater } from "../../state";

export type Kind<T> = T extends { kind: infer k } ? k : never;

export type CaseUpdater<
  Entity,
  Field extends keyof Entity,
  CaseName extends Kind<Entity[Field]> & string,
> = {
  [f in CaseName]: BasicFun<
    BasicUpdater<Entity[Field] & { kind: CaseName }>,
    Updater<Entity>
  >;
};

export const caseUpdater =
  <Entity>() =>
  <Field extends keyof Entity>(field: Field) =>
  <CaseName extends Kind<Entity[Field]> & string>(
    caseName: CaseName,
  ): CaseUpdater<Entity, Field, CaseName> =>
    ({
      [caseName]: (
        caseUpdater: BasicUpdater<Entity[Field]>,
      ): Updater<Entity & { [_ in Field]: { kind: CaseName } }> => {
        return Updater<Entity & { [_ in Field]: { kind: CaseName } }>(
          (currentEntity) =>
            currentEntity[field].kind == caseName
              ? {
                  ...currentEntity,
                  [field]: caseUpdater(
                    currentEntity[field] as Entity[Field] & { kind: CaseName },
                  ),
                }
              : currentEntity,
        );
      },
    }) as CaseUpdater<Entity, Field, CaseName>;

import {
  LeftValue,
  RightValue,
  Sum,
} from "../../../../../collections/domains/sum/state";
type Y = Sum<number, boolean>;
const Y = Sum<number, boolean>();
type X = {
  y: Y;
};
const X = {
  Default: (): X => ({ y: Y.Default.left(10) }),
  Updaters: {
    y: {
      ...caseUpdater<X>()("y")("l"),
      ...caseUpdater<X>()("y")("r"),
    },
  },
};

// const visitor =
//   X.Updaters.y.l(LeftValue.Updaters.value(_ => _ + 1)).then(
//     X.Updaters.y.r(RightValue.Updaters.value(_ => !_))
//   )

// console.log(visitor)
