import {Option, Unit, View} from "ballerina-core";
import {SpecEditor} from "../../../spec-editor/state";

// type Label = { label: string } //TODO: probably exists elsewhere in our codebase with a internationalization support, check
//
// export type LayoutSpecEditorActionType =
//     | { kind: "format" }
//     | { kind: "valid" }
//     & Label
//
// export type LayoutSpecRunnerActionType =
//     | { kind: "run" }
//     | { kind: "save" }
//     | { kind: "deploy" }
//     & Label
//
// export type LayoutActions =
//     {
//         specEditor: Option<LayoutSpecEditorActionType>,
//         specRunner: Option<LayoutSpecRunnerActionType>
//     }
//
// export const LayoutActions = {
//     Default: (): LayoutActions => ({
//         specEditor: Option.Default.none(),
//         specRunner: Option.Default.none() 
//     })
// }
//
// export type ActionsReadonlyContext = Unit;
// export type ActionsWritableState = SpecEditor;
//
// export type ActionsForeignMutationsExpected = Unit
//
// export type ActionsView = View<
//     ActionsReadonlyContext & ActionsWritableState,
//     ActionsWritableState,
//     ActionsForeignMutationsExpected,
//     {
//     }
// >;