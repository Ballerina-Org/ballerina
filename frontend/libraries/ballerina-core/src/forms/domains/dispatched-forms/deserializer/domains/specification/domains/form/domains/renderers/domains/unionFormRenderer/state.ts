// import { ValueOrErrors } from "../../../../../../../../../../../../../main";
// import {
//   DispatchIsObject,
//   DispatchisString,
//   UnionType,
// } from "../../../../../types/state";
// import { NestedRenderer } from "../nestedRenderer/state";
// import { List, Map } from "immutable";
// import { DispatchParsedType } from "../../../../../types/state";
// export type SerializedUnionFormRenderer = {
//   renderer?: unknown;
//   cases?: unknown;
// };

// export type UnionFormRenderer<T> = {
//   kind: "unionForm";
//   renderer?: string;
//   type: DispatchParsedType<T>;
//   cases: Map<string, NestedRenderer<T>>;
// };

// export const UnionFormRenderer = {
//   Default: <T>(
//     type: DispatchParsedType<T>,
//     cases: Map<string, NestedRenderer<T>>,
//     renderer?: string,
//   ): UnionFormRenderer<T> => ({ kind: "unionForm", type, renderer, cases }),
//   Operations: {
//     hasCases: (
//       _: unknown,
//     ): _ is { cases: Record<string, object>; renderer?: unknown } =>
//       DispatchIsObject(_) && "cases" in _ && DispatchIsObject(_.cases),
//     isValidRenderer: (_: unknown): _ is string => DispatchisString(_),
//     tryAsValidUnionForm: <T>(
//       formName: string,
//       serialized: SerializedUnionFormRenderer,
//     ): ValueOrErrors<
//       Omit<SerializedUnionFormRenderer, "cases" | "renderer"> & {
//         renderer?: string;
//         cases: Map<string, object>;
//       },
//       string
//     > => {
//       if (!UnionFormRenderer.Operations.hasCases(serialized))
//         return ValueOrErrors.Default.throwOne(
//           `union form ${formName} is missing the required cases attribute`,
//         );

//       const renderer = serialized.renderer;
//       if (
//         renderer !== undefined &&
//         !UnionFormRenderer.Operations.isValidRenderer(renderer)
//       )
//         return ValueOrErrors.Default.throwOne(
//           `union form ${formName} has an invalid renderer attribute`,
//         );

//       return ValueOrErrors.Default.return({
//         ...serialized,
//         cases: Map(serialized.cases),
//         renderer,
//       });
//     },
//     Deserialize: <T>(
//       type: UnionType<T>,
//       formName: string,
//       serialized: SerializedUnionFormRenderer,
//       fieldViews: any,
//     ): ValueOrErrors<UnionFormRenderer<T>, string> =>
//       UnionFormRenderer.Operations.tryAsValidUnionForm(formName, serialized)
//         .Then((validUnionForm) =>
//           ValueOrErrors.Operations.All(
//             List<ValueOrErrors<[string, NestedRenderer<T>], string>>(
//               validUnionForm.cases
//                 .entrySeq()
//                 .toArray()
//                 .map(([caseName, caseRenderer]) => {
//                   const caseType = type.args.get(caseName);

//                   if (caseType == undefined)
//                     return ValueOrErrors.Default.throwOne(
//                       `case ${caseName} not found in type ${type.typeName}`,
//                     );

//                   return NestedRenderer.Operations.Deserialize(
//                     caseType.fields,
//                     caseName,
//                     caseRenderer,
//                     fieldViews,
//                   ).Then((caseRenderer) =>
//                     ValueOrErrors.Default.return([caseName, caseRenderer]),
//                   );
//                 }),
//             ),
//           ).Then((caseTuples) =>
//             ValueOrErrors.Default.return(
//               UnionFormRenderer.Default(
//                 type,
//                 Map(caseTuples),
//                 validUnionForm.renderer,
//               ),
//             ),
//           ),
//         )
//         .MapErrors((errors) =>
//           errors.map(
//             (error) =>
//               `${error}\n...When parsing union form ${formName}`,
//           ),
//         ),
//   },
// };
