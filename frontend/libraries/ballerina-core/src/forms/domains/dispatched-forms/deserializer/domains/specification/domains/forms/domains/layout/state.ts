import { OrderedMap } from "immutable";

import { ValueOrErrors } from "../../../../../../../../../../../main";

type FieldName = string;

export type RawFormLayout = {
  [key: string]: RawTabLayout;
};

export const RawFormLayout = {
  isFormLayout: (rawFormLayout: unknown): rawFormLayout is RawFormLayout =>
    typeof rawFormLayout == "object" &&
    rawFormLayout != null &&
    Object.keys(rawFormLayout).length > 0 &&
    Object.values(rawFormLayout).every((tab) => RawTabLayout.isTabLayout(tab)),
};

export type RawTabLayout = {
  [key: string]: { columns: RawColumnLayout };
};

export const RawTabLayout = {
  isTabLayout: (rawTabLayout: unknown): rawTabLayout is RawTabLayout =>
    typeof rawTabLayout == "object" &&
    rawTabLayout != null &&
    Object.keys(rawTabLayout).length > 0 &&
    Object.entries(rawTabLayout).every(
      ([key, column]) =>
        key == "columns" && RawColumnLayout.isColumnLayout(column),
    ),
};

export type RawColumnLayout = {
  [key: string]: { groups: RawGroupLayout };
};

export const RawColumnLayout = {
  isColumnLayout: (
    rawColumnLayout: unknown,
  ): rawColumnLayout is RawColumnLayout =>
    typeof rawColumnLayout == "object" &&
    rawColumnLayout != null &&
    Object.keys(rawColumnLayout).length > 0 &&
    Object.values(rawColumnLayout).every(
      (group) =>
        RawGroupLayout.isLookup(group) || RawGroupLayout.isInlined(group),
    ),
};

export type RawGroupLayout = Array<FieldName> | object;

export const RawGroupLayout = {
  isLookup: (rawGroupLayout: unknown): rawGroupLayout is object =>
    typeof rawGroupLayout == "object",
  isInlined: (rawGroupLayout: unknown): rawGroupLayout is Array<FieldName> =>
    Array.isArray(rawGroupLayout) &&
    rawGroupLayout.every((field) => typeof field == "string"),
};

export type RecordFormLayout =
  | { kind: "Inlined"; tabs: OrderedMap<string, TabLayout> }
  | { kind: "Lookup" };

export type TabLayout = {
  columns: OrderedMap<string, ColumnLayout>;
};

export type ColumnLayout = {
  groups: OrderedMap<string, GroupLayout>;
};

export type GroupLayout = Array<FieldName>;

// We currently still support parsing of table layout with inlined groups here because some existing forms still use it
export const RecordFormLayout = {
  Default: (): RecordFormLayout => ({ kind: "Inlined", tabs: OrderedMap() }),
  Operations: {
    Deserialize: (
      rawLayout: unknown,
    ): ValueOrErrors<RecordFormLayout, string> => {
      if (!RawFormLayout.isFormLayout(rawLayout)) {
        return ValueOrErrors.Default.throwOne(
          `Invalid layout, expected object, got ${JSON.stringify(rawLayout)}`,
        );
      }

      if (
        Object.values(rawLayout).some((tab) =>
          Object.values(tab.columns).some((column) =>
            Object.values(column.groups).some((group) =>
              RawGroupLayout.isLookup(group),
            ),
          ),
        )
      ) {
        return ValueOrErrors.Default.return({ kind: "Lookup" });
      }

      return ValueOrErrors.Default.return({
        kind: "Inlined",
        tabs: OrderedMap(
          Object.entries(rawLayout).map(([tabName, tab]) => [
            tabName,
            {
              columns: OrderedMap(
                Object.entries(tab.columns).map(([columnName, column]) => [
                  columnName,
                  {
                    groups: OrderedMap(
                      Object.entries(column.groups).map(
                        ([groupName, group]) => [
                          groupName,
                          Array.isArray(group) ? group : [],
                        ],
                      ),
                    ),
                  },
                ]),
              ),
            },
          ]),
        ),
      });
    },
  },
};
