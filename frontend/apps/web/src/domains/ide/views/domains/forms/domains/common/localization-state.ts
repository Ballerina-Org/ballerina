import {TFunction} from "i18next";
import i18n from "../../../../../../../../i18n.ts";
import {Country} from "@blp-private-npm/ui/build/types/country";
import {Language} from "@blp-private-npm/ui/build/utils/languages";
import {BasicFun, simpleUpdater, Updater} from "ballerina-core";
import {DATE_FORMAT} from "./dates.ts";
import {numberToDisplayString} from "@blp-private-npm/ui/build/utils/parser";
import { formatIsoDate } from "node_modules/@blp-private-npm/ui/build/utils/dateHelper.js";
import { assertUnreachable } from "node_modules/@blp-private-npm/ui/build/index.js";

const IntlNumberFormatMap: Record<Country, string> = {
    DE: "de-DE",
    CH: "de-CH",
    AU: "de-AT",
    US: "en-US",
};
export function getCurrentDateTimeFilenameString() {
    return DateTime.now().setZone("local").toFormat("'_'yyyyMMdd'_'HHmmss");
}

export const languageMap: Record<Language, string> = {
    [Language.cs]: "🇨🇿 Čeština",
    [Language.de]: "🇩🇪 Deutsch",
    [Language.en]: "🇬🇧 English",
    [Language["en-US"]]: "🇺🇸 English (US)",
    [Language.es]: "🇪🇸 Español",
    [Language.fr]: "🇫🇷 Français",
    [Language.it]: "🇮🇹 Italiano",
    [Language.nl]: "🇳🇱 Nederlands",
    [Language.pl]: "🇵🇱 Polski",
    [Language.sv]: "🇸🇪 Svenska",
};

export function getLocaleFromLanguage(language: Language | undefined) {
    let locale: string | undefined;
    switch (language) {
        case Language.cs:
            locale = "cs-CZ";
            break;
        case Language.de:
        case undefined:
            locale = "de-DE";
            break;
        case Language.en:
            locale = "en-GB";
            break;
        case Language["en-US"]:
            locale = "en-US";
            break;
        case Language.es:
            locale = "es-ES";
            break;
        case Language.fr:
            locale = "fr-FR";
            break;
        case Language.it:
            locale = "it-IT";
            break;
        case Language.nl:
            locale = "nl-NL";
            break;
        case Language.pl:
            locale = "pl-PL";
            break;
        case Language.sv:
            locale = "sv-SE";
            break;
        default:
            assertUnreachable(language);
    }
    return locale;
}
export type LocalizationState = {
    t: TFunction;
    i18n: typeof i18n;
    numDecimals: number;
    country: Country | undefined;
    language: Language;
    numberToDisplayString: BasicFun<number | undefined, string>;
    formatISODate: (
        originalDate: string | undefined,
        dateFormat: DATE_FORMAT
    ) => string;
    formatISOLocalDate: (
        originalDate: string | undefined,
        dateFormat: DATE_FORMAT
    ) => string;
    formatISORelativeDate: (originalDate: string | undefined) => string;
    intFormatter: Intl.NumberFormat;
    floatFormatter: Intl.NumberFormat;
};

export const LocalizationState = {
    Default: (merged: UserConfig, i18n: typeof i18n): LocalizationState => {
        const numDecimals =
            typeof merged.numDecimals == "number" ? merged.numDecimals : 2;
        const country = merged.floatParseCountryFormat
            ? merged.floatParseCountryFormat
            : "DE";

        const language = merged.language || "de";

        const intFormatter = new Intl.NumberFormat(IntlNumberFormatMap[country], {
            minimumFractionDigits: 0,
            maximumFractionDigits: 0,
        });
        const floatFormatter = new Intl.NumberFormat(IntlNumberFormatMap[country], {
            minimumFractionDigits: numDecimals,
            maximumFractionDigits: numDecimals,
        });

        return {
            t: i18n.t,
            i18n,
            numDecimals,
            country,
            language: language,
            intFormatter,
            floatFormatter,
            numberToDisplayString: (v) =>
                numberToDisplayString(v, country, numDecimals),
            formatISODate: (originalDate, dateFormat) =>
                originalDate ? formatIsoDate(originalDate, dateFormat, language) : "",
            formatISOLocalDate: (originalDate, dateFormat) =>
                originalDate
                    ? formatIsoDate(originalDate, dateFormat, language, "system")
                    : "",
            formatISORelativeDate: (originalDate) => {
                if (!originalDate) return "";

                const locale = getLocaleFromLanguage(language);
                const dt = DateTime.fromISO(originalDate, { zone: "utc" });
                const now = DateTime.now();

                const diffInDays = Math.floor(now.diff(dt, "days").days);

                const rtf = new Intl.RelativeTimeFormat(locale, { numeric: "auto" });

                if (diffInDays === 0) {
                    return rtf.format(0, "day"); // “today”
                }
                if (diffInDays === 1) {
                    return rtf.format(-1, "day"); // “yesterday”
                }

                if (diffInDays < 14) {
                    return rtf.format(-diffInDays, "day"); // → “5 days ago”
                }

                const diffInWeeks = Math.floor(diffInDays / 7);
                if (diffInWeeks < 4) {
                    return rtf.format(-diffInWeeks, "week"); // → “3 weeks ago”
                }

                const diffInMonths = Math.floor(now.diff(dt, "months").months);
                if (diffInMonths < 12) {
                    return rtf.format(-diffInMonths, "month"); // → “2 months ago”
                }

                const diffInYears = Math.floor(now.diff(dt, "years").years);
                return rtf.format(-diffInYears, "year"); // → “1 year ago”
                // if (!originalDate) return "";
                // const locale = getLocaleFromLanguage(language);
                // const dt = DateTime.fromISO(originalDate, { zone: "utc" }).setLocale(
                //   locale
                // );
                // const now = DateTime.now().setLocale(locale);
                // const diff = now
                //   .diff(dt, ["years", "months", "weeks", "days"])
                //   .toObject();
                // let unit: "years" | "months" | "weeks" | "days" = "days";

                // if (diff.years && diff.years >= 1) unit = "years";
                // else if (diff.months && diff.months >= 1) unit = "months";
                // else if (diff.weeks && diff.weeks >= 3) unit = "weeks";
                // const relative = dt.setLocale(locale).toRelative({
                //   base: now,
                //   style: "long",
                //   round: true,
                //   unit: unit,
                // });
                // if (!relative) return "";
                // return relative;
            },
        };
    },
    Updaters: {
        Core: {
            ...simpleUpdater<LocalizationState>()("language"),
            ...simpleUpdater<LocalizationState>()("country"),
            ...simpleUpdater<LocalizationState>()("numDecimals"),
        },
        Template: {
            changeLanguage: (language: Language): Updater<LocalizationState> =>
                LocalizationState.Updaters.Core.language((_) => {
                    return language;
                }),
        },
    },
};
