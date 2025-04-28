// import { List } from "immutable";
// import { NestedRenderer } from "../../../../../deserializer/domains/specification/domains/form/domains/renderers/domains/nestedRenderer/state";

// import {
//   Bindings,
//   DispatcherContext,
//   DispatchUnionForm,
//   PredicateValue,
//   Template,
//   ValueOrErrors,
// } from "../../../../../../../../../main";

// import { UnionType } from "../../../../../deserializer/domains/specification/domains/types/state";
// import { NestedDispatcher } from "../../state";
// import { RecordFieldRenderer } from "../../../../../deserializer/domains/specification/domains/form/domains/renderers/domains/recordFormRenderer/domains/recordFieldRenderer/state";

// export const NestedUnionDispatcher = {
//   Dispatch: <T extends { [key in keyof T]: { type: any; state: any } }>(
//     type: UnionType<T>,
//     viewKind: string,
//     unionRenderer: RecordFieldRenderer<T> | NestedRenderer<T>,
//     rendererName: string,
//     dispatcherContext: DispatcherContext<T>,
//   ): ValueOrErrors<Template<any, any, any, any>, string> => {
//     const result: ValueOrErrors<Template<any, any, any, any>, string> = (() => {
//       if (viewKind != "union") {
//         return ValueOrErrors.Default.throwOne(
//           `expected viewKind == "union" but got ${viewKind}`,
//         );
//       }

//       return NestedDispatcher.Operations.Dispatch(
//         type,
//         unionRenderer,
//         viewKind,
//         dispatcherContext,
//       ).Then((template) => {
//         return ValueOrErrors.Default.return(
//           DispatchUnionForm(
//             dispatcherContext.defaultState(caseType.fields),
//             dispatcherContext.defaultValue(caseType.fields),
//             template,
//           ),
//         );
//       });
//     })();
//     return result.MapErrors((errors) =>
//       errors.map(
//         (error) =>
//           `${error}\n...When dispatching nested union: ${rendererName}`,
//       ),
//     );
//   },
// };
