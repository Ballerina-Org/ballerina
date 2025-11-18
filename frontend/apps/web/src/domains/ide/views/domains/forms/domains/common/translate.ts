import i18n from "../../../../../../../../i18n.ts";
import { LocalizationState } from "./localization-state.ts";
import {Namespace} from "./namespace.ts";

export function translateForCustomDataDrivenTranslations(
    locale: LocalizationState,
    namespace: Namespace
) {
    const t = i18n.t.bind(i18n);
    return (key: string | undefined, replace: Record<string, string> = {}) => {
        if (key) {
            return t(key, {
                ns: namespace,
                ...replace,
            });
        }

        return "!!Err:NoKey";
    };
}

export function translateForDataDrivenTranslationsWithContext(
    locale: LocalizationState,
    namespace: Namespace
) {
    // "!@£$IgnoreSeparator" is needed to ignore the separator in the key for the data driven translations
    // because we are using : as separator in the key for the data driven translations which
    // is the default namespace separator and confuses react-i18next
    return (
        key: string | undefined,
        context: string, // where the label is used
        identifier: string, // identifier for error, usually the field or column name
        replace: Record<string, string> = {}
    ) => {
        if (namespace === Namespace.TranslationNamespaceSetupGuide) {
            return translateForCustomDataDrivenTranslations(locale, namespace)(
                key,
                replace
            );
        }
        if (key) {
            const result = locale.t(`${context}:${key}`, {
                ns: namespace,
                nsSeparator: "!@£$IgnoreSeparator",
                ...replace,
                defaultValue: "!!Err:NoKey:" + identifier,
            });
            if (result === "!!Err:NoKey:" + identifier) {
                // TODO: find a better way to warn about this without cluttering the console
                // console.warn(`No key is defined for ${context}:${identifier}`);
                return "!!Err:NoKey:" + identifier;
            }
            return result;
        }
        // TODO: find a better way to warn about this without cluttering the console
        // console.warn(`No label is defined for ${context}:${identifier}`);
        return "!!Err:NoLabel:" + identifier;
    };
}