import {
    assertUnreachable,
    BaseDropdownMultiple,
    BaseDropdownV3,
    BaseFieldWithoutMetadata,
    BaseInputV3,
    BaseLabelV3,
    BaseStatusIcon,
    DropdownItem,
    TaskSolverStatusIconWrapper,
} from "@blp-private-npm/ui";
import { Map, Set } from "immutable";
import {
    BasicFun,
    ConcreteRenderers,
    EnumAbstractRendererView,
    EnumMultiselectAbstractRendererView,
    PredicateValue, SimpleCallback, Synchronized, Unit,
    VoidCallbackWithOptionalFlags,
} from "ballerina-core";
import i18n from "../../../../../../../i18n.ts"
import React, {createContext, ReactNode, useContext, useState} from "react";
import {
    DispatchPassthroughFormExtraContext
} from "../../../../../dispatched-passthrough-form/views/concrete-renderers.tsx";
import {
    ListElementCustomPresentationContext
} from "../../../../../dispatched-passthrough-form/views/tailwind-renderers.tsx";
import {
    DispatchPassthroughFormInjectedTypes
} from "../../../../../dispatched-passthrough-form/injected-forms/category.tsx";

import {Country} from "@blp-private-npm/ui/build/types/country";
import {Language} from "@blp-private-npm/ui/build/utils/languages";
import {TFunction} from "i18next";


// export function useStateWithEffectAndCondition<T>(
//     init: T,
//     condition: boolean
// ): [T, React.Dispatch<React.SetStateAction<T>>] {
//     const [value, setValue] = React.useState(init);
//     React.useEffect(() => {
//         if (condition) setValue(init);
//     }, [init, condition]);
//     return [value, setValue];
// }


// export const TABLE_COLUMN_MIN_WIDTH = 200;
// export const TABLE_COLUMN_MAX_WIDTH = 400;
// export const TABLE_COLUMN_INIT_WIDTH = 200;
// export const TABLE_COLUMN_ACTION_WIDTH = 40;
//
//
//


// import {
//     CustomPresentationContexts,
//     DashboardConcreteRenderers,
//     FieldExtraContext,
//     Flags,
//     mapTypeToFlags,
//     translateForCustomDataDrivenTranslations,
//     translateForDataDrivenTranslationsWithContext,
// } from "@/components2/organisms/forms/dashboard-field-views/common";






// const EnumInputComponent = (
//     props: React.ComponentProps<
//         EnumAbstractRendererView<
//             CustomPresentationContexts,
//             IdeFlags,
//             FieldExtraContext
//         >
//     >
// ) => {
//     const ddTranslations = translateForCustomDataDrivenTranslations(
//         props.context.extraContext.locale,
//         props.context.extraContext.namespace
//     );
//     // Enums are a special case, we use the translatorFor function to translate the value as they have no prefix
//     const ddTranslationsWithCtx = translateForCustomDataDrivenTranslations(
//         props.context.extraContext.locale,
//         props.context.extraContext.namespace
//     );
//     const flags =
//         props.context.type.kind === "singleSelection"
//             ? mapTypeToFlags({
//                 typeStr: props.context.type.args[0].asString(),
//                 ancestors: props.context.lookupTypeAncestorNames,
//                 documentType: "FIXME: Document Type",
//             })
//             : undefined;
//
//     // TODO: improve search
//     const [searchText, setSearchText] = useState("");
//
//     const selectedValue =
//         !PredicateValue.Operations.IsUnit(props.context.value) &&
//         props.context.value.isSome &&
//         PredicateValue.Operations.IsRecord(props.context.value.value)
//             ? props.context.value.value.fields.get("Value")
//             : undefined;
//
//     return (
//         <BaseDropdownV3
//             variant="outlined"
//             shape={
//                 props.context.customPresentationContext?.isCell
//                     ? "rectangular"
//                     : "square"
//             }
//             size="m"
//             readOnly={props.context.readOnly}
//             disabled={props.context.disabled}
//             tooltip={props.context.customPresentationContext?.tooltip}
//             handleClear={
//                 !props.context.customPresentationContext?.disableClearable &&
//                 props.context.customPresentationContext?.sum?.clearCallback
//                     ? props.context.customPresentationContext.sum.clearCallback
//                     : // not in a sum -> clear the value by setting it to an empty string
//                     () => props.foreignMutations.setNewValue("", undefined)
//             }
//             onMouseClick={() => props.foreignMutations.loadOptions()}
//             valueProps={{
//                 value:
//                     selectedValue && PredicateValue.Operations.IsString(selectedValue)
//                         ? ddTranslationsWithCtx(selectedValue)
//                         : undefined,
//                 valueId:
//                     selectedValue && PredicateValue.Operations.IsString(selectedValue)
//                         ? selectedValue
//                         : undefined,
//                 invalid: false,
//                 placeholder: ddTranslations("selectValue"),
//                 copyable: props.context.customPresentationContext?.disableCopyable
//                     ? false
//                     : true,
//                 customAiApplied: Boolean(
//                     props.context?.customPresentationContext?.customAiApplied
//                 ),
//                 startAdornment:
//                 props.context?.customPresentationContext?.customAiApplied,
//                 isWrapped: props.context?.customPresentationContext?.isWrapped,
//                 endAdornment: props.context?.customPresentationContext?.endAdornment,
//             }}
//             searchProps={{
//                 kind: "searchable",
//                 searchText,
//                 setSearchText,
//                 placeholder: ddTranslations("typeAndFilterMin3Characters"),
//             }}
//             listProps={{
//                 isLoading:
//                     props.context.activeOptions === "loading" ||
//                     props.context.activeOptions === "unloaded",
//                 options:
//                     props.context.activeOptions !== "loading" &&
//                     props.context.activeOptions !== "unloaded"
//                         ? props.context.activeOptions
//                             .map((o) => {
//                                 const optionValue = o.fields.get("Value");
//                                 return optionValue &&
//                                 PredicateValue.Operations.IsString(optionValue)
//                                     ? {
//                                         id: optionValue,
//                                         label: ddTranslationsWithCtx(optionValue),
//                                     }
//                                     : undefined;
//                             })
//                             .filter((o) => o !== undefined)
//                             .filter(
//                                 (o) =>
//                                     searchText.length === 0 ||
//                                     o.label.toLowerCase().includes(searchText.toLowerCase())
//                             )
//                         : [],
//                 select: (v) => props.foreignMutations.setNewValue(v.id, flags),
//             }}
//         />
//     );
// };

// const EnumReadOnlyComponent = (
//     props: React.ComponentProps<
//         EnumAbstractRendererView<
//             CustomPresentationContexts,
//             IdeFlags,
//             FieldExtraContext
//         >
//     >
// ) => {
//     const ddTranslationsWithCtx = translateForCustomDataDrivenTranslations(
//         props.context.extraContext.locale,
//         props.context.extraContext.namespace
//     );
//
//     const selectedValue =
//         !PredicateValue.Operations.IsUnit(props.context.value) &&
//         props.context.value.isSome &&
//         PredicateValue.Operations.IsRecord(props.context.value.value)
//             ? props.context.value.value.fields.get("Value")
//             : undefined;
//
//     return (
//         <BaseInputV3
//             variant={
//                 props.context.customPresentationContext?.isCell ? "cell" : "outlined"
//             }
//             shape={"square"}
//             kind="readOnly"
//             fullWidth
//             copyable={
//                 props.context.customPresentationContext?.disableCopyable ? false : true
//             }
//             value={
//                 selectedValue && PredicateValue.Operations.IsString(selectedValue)
//                     ? ddTranslationsWithCtx(selectedValue)
//                     : ""
//             }
//             onClick={() => {}}
//             disabled={props.context.disabled}
//         />
//     );
// };
//
// const EnumMultiselectComponent = (
//     props: React.ComponentProps<
//         EnumMultiselectAbstractRendererView<
//             CustomPresentationContexts,
//             IdeFlags,
//             FieldExtraContext
//         >
//     >
// ) => {
//     const [searchText, setSearchText] = useState("");
//     const ddTranslationsWithCtx = translateForCustomDataDrivenTranslations(
//         props.context.extraContext.locale,
//         props.context.extraContext.namespace
//     );
//
//     const loadOptions = () =>
//         props.context.activeOptions === "unloaded" &&
//         props.foreignMutations.loadOptions();
//
//     return (
//         <BaseDropdownMultiple
//             onOpenChange={loadOptions}
//             readOnly={props.context.readOnly}
//             listProps={{
//                 isLoading:
//                     props.context.activeOptions === "loading" ||
//                     props.context.activeOptions === "unloaded",
//                 allOptions:
//                     props.context.activeOptions !== "loading" &&
//                     props.context.activeOptions !== "unloaded"
//                         ? props.context.activeOptions
//                             .map((o) => {
//                                 const optionValue = o.fields.get("Value");
//                                 return optionValue &&
//                                 PredicateValue.Operations.IsString(optionValue)
//                                     ? {
//                                         id: optionValue,
//                                         label: ddTranslationsWithCtx(optionValue),
//                                     }
//                                     : undefined;
//                             })
//                             .filter((o) => o !== undefined)
//                             .filter(
//                                 (o) =>
//                                     searchText.length === 0 ||
//                                     o.label.toLowerCase().includes(searchText.toLowerCase())
//                             )
//                         : [],
//                 submitSelectedOptions: (items: DropdownItem[]) => {
//                     const selectedIds = items.map((item) => item.id);
//                     props.foreignMutations.setNewValue(selectedIds, undefined);
//                 },
//             }}
//             valueProps={{
//                 initSelectedOptions: props.context.selectedIds.map((id) => {
//                     return {
//                         id: id,
//                         label: ddTranslationsWithCtx(id),
//                     };
//                 }),
//             }}
//             searchProps={{
//                 searchText,
//                 setSearchText,
//             }}
//         />
//     );
// };
//
// export const EnumFieldViewsSingleSelection = {
//     enumWithMenuContext: () => (props) => {
//         return (
//             <BaseFieldWithoutMetadata
//                 disabled={props.context.disabled}
//                 domNodeId={props.context.domNodeId}
//             >
//                 {({ showMenu, endAdornment }) => (
//                     <EnumInputComponent
//                         {...props}
//                         context={{
//                             ...props.context,
//                             customPresentationContext: {
//                                 ...props.context.customPresentationContext,
//                                 isWrapped: showMenu,
//                                 endAdornment,
//                             },
//                         }}
//                     />
//                 )}
//             </BaseFieldWithoutMetadata>
//         );
//     },
//     enum: () => (props) => {
//         return <EnumInputComponent {...props} />;
//     },
//     enumWithLabel: () => (props) => {
//         const ddTranslationsWithCtx = translateForDataDrivenTranslationsWithContext(
//             props.context.extraContext.locale,
//             props.context.extraContext.namespace
//         );
//
//         return (
//             <BaseLabelV3
//                 dense
//                 label={ddTranslationsWithCtx(
//                     props.context.label,
//                     props.context.labelContext,
//                     props.context.domNodeId
//                 )}
//                 highlightedValue={
//                     props.context?.customPresentationContext?.highlightLabelValue
//                 }
//             >
//                 <EnumInputComponent {...props} />
//             </BaseLabelV3>
//         );
//     },
//     enumLabelInTooltip: () => (props) => {
//         const ddTranslationsWithCtx = translateForDataDrivenTranslationsWithContext(
//             props.context.extraContext.locale,
//             props.context.extraContext.namespace
//         );
//
//         return (
//             <EnumInputComponent
//                 {...props}
//                 context={{
//                     ...props.context,
//                     customPresentationContext: {
//                         ...props.context.customPresentationContext,
//                         tooltip: ddTranslationsWithCtx(
//                             props.context.label,
//                             props.context.labelContext,
//                             props.context.domNodeId
//                         ),
//                     },
//                 }}
//             />
//         );
//     },
//     enumCell: () => (props) => {
//         return (
//             <EnumInputComponent
//                 {...props}
//                 context={{
//                     ...props.context,
//                     customPresentationContext: {
//                         ...props.context.customPresentationContext,
//                         isCell: true,
//                     },
//                 }}
//             />
//         );
//     },
//     enumAsFailingChecksStatus: () => (props) => {
//         const status =
//             PredicateValue.Operations.IsOption(props.context.value) &&
//             PredicateValue.Operations.IsRecord(props.context.value.value)
//                 ? props.context.value.value.fields.get("Value")
//                 : undefined;
//
//         if (!status || status === "NO_STATUS") return <></>;
//
//         return (
//             <TaskSolverStatusIconWrapper size={"m"}>
//                 <BaseStatusIcon
//                     kind="empty"
//                     colorVariant={
//                         status === "UNCERTAIN"
//                             ? "warning"
//                             : status === "OK"
//                                 ? "success"
//                                 : "info"
//                     }
//                     size="xs"
//                     outlined="solid"
//                 />
//             </TaskSolverStatusIconWrapper>
//         );
//     },
//     customAIModelState: () => (_props) => {
//         return <>customAIModelState</>;
//     },
//     customAIModelVisibility: () => (_props) => {
//         return <>customAIModelVisibility</>;
//     },
//     enumNotEditable: () => (props) => {
//         return <EnumReadOnlyComponent {...props} />;
//     },
// } satisfies IdeConcreteRenderers["enumSingleSelection"];
//
// const EnumMultiselectWithContextMenuComponent = (
//     props: React.ComponentProps<
//         EnumMultiselectAbstractRendererView<
//             CustomPresentationContexts,
//             IdeFlags,
//             FieldExtraContext
//         >
//     >
// ) => {
//     return (
//         <BaseFieldWithoutMetadata
//             disabled={props.context.disabled}
//             domNodeId={props.context.domNodeId}
//         >
//             {({ showMenu, endAdornment }) => (
//                 <EnumMultiselectComponent
//                     {...props}
//                     context={{
//                         ...props.context,
//                         customPresentationContext: {
//                             ...props.context.customPresentationContext,
//                             isWrapped: showMenu,
//                             endAdornment,
//                         },
//                     }}
//                 />
//             )}
//         </BaseFieldWithoutMetadata>
//     );
// };
