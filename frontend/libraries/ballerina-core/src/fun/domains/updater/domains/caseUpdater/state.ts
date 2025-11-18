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

// export const caseUpdater =
//     <Entity>() =>
//         <Field extends keyof Entity>(field: Field) =>
//             <CaseName extends Kind<Entity[Field]> & string>(
//                 caseName: CaseName,
//             ): CaseUpdater<Entity, Field, CaseName> =>
//                 ({
//                     [caseName]: (
//                         caseUpdater: BasicUpdater<Entity[Field]>,
//                     ): Updater<Entity & { [_ in Field]: { kind: CaseName } }> => {
//                         return Updater<Entity & { [_ in Field]: { kind: CaseName } }>(
//                             (currentEntity) =>
//                                 currentEntity[field].kind === caseName
//                                     ? {
//                                         ...currentEntity,
//                                         [field]: caseUpdater(
//                                             currentEntity[field] as Entity[Field] & { kind: CaseName },
//                                         ),
//                                     }
//                                     : currentEntity,
//                         );
//                     },
//                 }) as CaseUpdater<Entity, Field, CaseName>;
// export const caseUpdater =
//     <Entity>() =>
//         <
//             Field extends keyof Entity,
//             FieldType extends Entity[Field] & { kind: string } // ensure `kind`
//         >(field: Field) =>
//             <CaseName extends Kind<FieldType> & string>(caseName: CaseName) => {
//                 type Case = Extract<FieldType, { kind: CaseName }>;
//                 type PayloadKey = Exclude<keyof Case, 'kind'>;
//                 type Payload = Case[PayloadKey];
//
//                 // 🔒 get the (only) non-'kind' key at runtime
//                 const payloadKey = (
//                     Object.keys({} as Case).find((k) => k !== 'kind') as PayloadKey
//                 )!;
//
//                 // Define makeUpdater using arrow functions and overloads
//                 const makeUpdater = (
//                     ((updater: ((payload: Payload) => Payload) | ((fullCase: Case) => Case)) =>
//                         Updater<Entity>((currentEntity) => {
//                             const fieldValue = currentEntity[field] as FieldType;
//                             if (fieldValue.kind !== caseName) return currentEntity;
//
//                             const currentCase = fieldValue as Case;
//                             const payload = (currentCase as any)[payloadKey];
//                             const updated = (updater as any)(payload ?? currentCase);
//
//                             const nextValue =
//                                 updated && (updated as any).kind === caseName
//                                     ? updated
//                                     : { kind: caseName, [payloadKey]: updated };
//
//                             return { ...currentEntity, [field]: nextValue } as Entity;
//                         })) as unknown) as {
//                     (updater: (payload: Payload) => Payload): Updater<Entity>;
//                     (updater: (fullCase: Case) => Case): Updater<Entity>;
//                 };
//
//                 return { [caseName]: makeUpdater } as Record<CaseName, typeof makeUpdater>;
//             };

export const caseUpdater =
    <Entity>() =>
        <
            Field extends keyof Entity,
            FieldType extends Entity[Field] & { kind: string }
        >(field: Field) =>
            <CaseName extends Kind<FieldType> & string>(caseName: CaseName) => {
                type Case = Extract<FieldType, { kind: CaseName }>;
                type PayloadKey = Exclude<keyof Case, "kind">;
                type Payload = Case[PayloadKey];

                const makeUpdater = (updater: BasicUpdater<Payload>): Updater<Entity> =>
                    Updater((currentEntity) => {
                        const fieldValue = currentEntity[field] as FieldType;
                        if (fieldValue.kind !== caseName) return currentEntity;

                        const currentCase = fieldValue as Case;
                        
                        const payloadKey = Object.keys(currentCase).find(
                            (k) => k !== "kind"
                        ) as PayloadKey;

                        const payload = (currentCase as any)[payloadKey] as Payload;
                        const updatedPayload = updater(payload);

                        const nextValue = { kind: caseName, [payloadKey]: updatedPayload } as Case;

                        return { ...currentEntity, [field]: nextValue } as Entity;
                    });

                return { [caseName]: makeUpdater } as Record<CaseName, typeof makeUpdater>;
            };
// export const caseUpdater =
//     <Entity>() =>
//         <
//             Field extends keyof Entity,
//             FieldType extends Entity[Field] & { kind: string } // ensure `kind`
//         >(field: Field) =>
//             <CaseName extends Kind<FieldType> & string>(caseName: CaseName) => {
//                 type Case = Extract<FieldType, { kind: CaseName }>;
//                 type PayloadKey = Exclude<keyof Case, 'kind'>;
//                 type Payload = Case[PayloadKey];
//
//                 // 🔒 get the (only) non-'kind' key at runtime
//                 const payloadKey = (
//                     Object.keys({} as Case).find((k) => k !== 'kind') as PayloadKey
//                 )!;
//
//                 const makeUpdater = (updater: BasicUpdater<Payload>): Updater<Entity> =>
//                     Updater<Entity>((currentEntity) => {
//                         const fieldValue = currentEntity[field] as FieldType;
//                         if (fieldValue.kind !== caseName) return currentEntity;
//
//                         const currentCase = fieldValue as Case;
//                         const currentPayload = (currentCase as any)[payloadKey] as Payload;
//                         const updatedPayload = updater(currentPayload);
//
//                         const nextValue = {
//                             kind: caseName,
//                             [payloadKey]: updatedPayload,
//                         } as Case;
//
//                         return { ...currentEntity, [field]: nextValue } as Entity;
//                     });
//
//                 return { [caseName]: makeUpdater } as Record<CaseName, typeof makeUpdater>;
//             };

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
