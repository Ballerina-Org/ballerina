import { List, Map, OrderedMap } from "immutable";
import { FieldName } from "../types/state";
import { Bindings, Expr, PredicateValue } from "../predicates/state";
import { Predicate, ValueOrErrors } from "../../../../../../main";

export type PredicateFormLayout = OrderedMap<string, PredicateTabLayout>;

export type PredicateTabLayout = {
  columns: OrderedMap<string, PredicateColumnLayout>;
};

export const RawTabLayout = {
  isTabLayout: (rawTabLayout: unknown): rawTabLayout is object =>
    typeof rawTabLayout == "object" &&
    rawTabLayout != null &&
    Object.keys(rawTabLayout).length > 0,
};

export type PredicateColumnLayout = {
  groups: OrderedMap<string, PredicateGroupLayout>;
};

export const RawColumnLayout = {
  isColumnLayout: (rawColumnLayout: unknown): rawColumnLayout is object =>
    typeof rawColumnLayout == "object" &&
    rawColumnLayout != null &&
    Object.keys(rawColumnLayout).length > 0,
};
export type PredicateGroupLayout =
  | { kind: "Inlined"; fields: Array<FieldName> }
  | { kind: "Computed"; fields: Expr };

export const GroupLayout = {
  Default: {
    Inlined: (fields: Array<FieldName>): PredicateGroupLayout => ({
      kind: "Inlined",
      fields,
    }),
    Computed: (fields: Expr): PredicateGroupLayout => ({
      kind: "Computed",
      fields,
    }),
  },
};

export type FormLayout = Map<string, CalculatedTabLayout>;
export type CalculatedTabLayout = {
  columns: OrderedMap<string, CalculatedColumnLayout>;
};
export type CalculatedColumnLayout = {
  groups: OrderedMap<string, CalculatedGroupLayout>;
};
export type CalculatedGroupLayout = Array<FieldName>;

export const RawGroupLayout = {
  isInlined: (rawGroupLayout: unknown): rawGroupLayout is Array<FieldName> =>
    Array.isArray(rawGroupLayout) &&
    rawGroupLayout.every((field) => typeof field == "string"),
};

export const FormLayout = {
  Default: (): PredicateFormLayout => OrderedMap(),
  Operations: {
    ParseGroupLayout: (
      rawGroupLayout: unknown,
    ): ValueOrErrors<PredicateGroupLayout, string> => {
      if (RawGroupLayout.isInlined(rawGroupLayout)) {
        return ValueOrErrors.Default.return({
          kind: "Inlined",
          fields: rawGroupLayout,
        });
      }
      return Expr.Operations.parse(rawGroupLayout).Then((expr) =>
        ValueOrErrors.Default.return({
          kind: "Computed",
          fields: expr,
        }),
      );
    },
    ParseLayout: (
      rawLayout: unknown,
    ): ValueOrErrors<PredicateFormLayout, string> => {
      if (!rawLayout || typeof rawLayout != "object") {
        return ValueOrErrors.Default.throwOne(
          `Invalid layout, expected object, got ${JSON.stringify(rawLayout)}`,
        );
      }
      if (
        !("tabs" in rawLayout) ||
        typeof rawLayout.tabs != "object" ||
        rawLayout.tabs == null
      ) {
        return ValueOrErrors.Default.throwOne(
          `Invalid layout, expected object with "tabs" property, got ${JSON.stringify(
            rawLayout,
          )}`,
        );
      }
      let tabs: PredicateFormLayout = OrderedMap();
      Object.entries(rawLayout.tabs).forEach(
        ([tabName, tab]: [tabName: string, tab: object]) => {
          let cols: PredicateTabLayout = { columns: OrderedMap() };
          tabs = tabs.set(tabName, cols);
          if (
            !("columns" in tab) ||
            typeof tab.columns != "object" ||
            tab.columns == null
          ) {
            return ValueOrErrors.Default.throwOne(
              `Invalid layout, expected object with "columns" property, got ${JSON.stringify(
                tab,
              )}`,
            );
          }
          Object.entries(tab.columns).forEach(
            ([colName, col]: [colName: string, col: object]) => {
              let column: PredicateColumnLayout = { groups: OrderedMap() };
              cols.columns = cols.columns.set(colName, column);
              if (
                !("groups" in col) ||
                typeof col.groups != "object" ||
                col.groups == null
              ) {
                return ValueOrErrors.Default.throwOne(
                  `Invalid layout, expected object with "groups" property, got ${JSON.stringify(
                    col,
                  )}`,
                );
              }
              Object.keys(col.groups).forEach((groupName) => {
                const groupConfig = (col.groups as any)[groupName];
                const parsedGroupLayout =
                  FormLayout.Operations.ParseGroupLayout(groupConfig);
                if (parsedGroupLayout.kind == "errors") {
                  return parsedGroupLayout;
                }
                column.groups = column.groups.set(
                  groupName,
                  parsedGroupLayout.value,
                );
              });
            },
          );
        },
      );
      return ValueOrErrors.Default.return(tabs);
    },
    EvaluateGroup: (
      bindings: Bindings,
      group: PredicateGroupLayout,
    ): ValueOrErrors<Array<FieldName>, string> => {
      if (group.kind == "Inlined") {
        return ValueOrErrors.Default.return(group.fields);
      }

      return Expr.Operations.Evaluate(bindings)(group.fields).Then((result) => {
        if (!PredicateValue.Operations.IsRecord(result)) {
          return ValueOrErrors.Default.throwOne(
            `Invalid group layout: ${JSON.stringify(result)}`,
          );
        }
        return ValueOrErrors.Default.return(Array.from(result.fields.keys()));
      });
    },
    ComputeLayout: (
      bindings: Bindings,
      formLayout: PredicateFormLayout,
    ): ValueOrErrors<FormLayout, string> =>
      ValueOrErrors.Operations.All(
        List(
          formLayout
            .map((tab) =>
              ValueOrErrors.Operations.All(
                List(
                  tab.columns
                    .map((column) =>
                      ValueOrErrors.Operations.All(
                        List<ValueOrErrors<[string, Array<FieldName>], string>>(
                          column.groups
                            .entrySeq()
                            .map(([groupName, group]) =>
                              FormLayout.Operations.EvaluateGroup(
                                bindings,
                                group,
                              ).Then((fields) =>
                                ValueOrErrors.Default.return([
                                  groupName,
                                  fields,
                                ]),
                              ),
                            ),
                        ),
                      ).Then((calculatedGroups) =>
                        ValueOrErrors.Default.return({
                          groups: OrderedMap(calculatedGroups),
                        }),
                      ),
                    )
                    .entrySeq()
                    .toArray()
                    .map(([columnName, column]) =>
                      column.Then((column) =>
                        ValueOrErrors.Default.return<
                          [string, CalculatedColumnLayout],
                          string
                        >([columnName, column]),
                      ),
                    ),
                ),
              ).Then((calculatedColumns) =>
                ValueOrErrors.Default.return({
                  columns: OrderedMap(calculatedColumns),
                }),
              ),
            )
            .entrySeq()
            .toArray()
            .map(([tabName, tab]) =>
              tab.Then((tab) =>
                ValueOrErrors.Default.return<
                  [string, CalculatedTabLayout],
                  string
                >([tabName, tab]),
              ),
            ),
        ),
      ).Then((calculatedTabs) =>
        ValueOrErrors.Default.return(OrderedMap(calculatedTabs)),
      ),
  },
};

export type PredicateVisibleColumns =
  | {
      kind: "Computed";
      columns: Expr;
    }
  | {
      kind: "Inlined";
      columns: Array<string>;
    };

export const RawVisibleColumns = {
  isInlined: (rawVisibleColumns: unknown): rawVisibleColumns is Array<string> =>
    Array.isArray(rawVisibleColumns) &&
    rawVisibleColumns.every((column) => typeof column == "string"),
};

export type CalculatedTableLayout = {
  columns: Array<string>;
};

export const TableLayout = {
  Default: (): CalculatedTableLayout => ({
    columns: [],
  }),
  Operations: {
    ParseLayout: (
      rawVisibleColumns: unknown,
    ): ValueOrErrors<PredicateVisibleColumns, string> => {
      if (RawVisibleColumns.isInlined(rawVisibleColumns)) {
        return ValueOrErrors.Default.return({
          kind: "Inlined",
          columns: rawVisibleColumns,
        });
      }
      return Expr.Operations.parse(rawVisibleColumns).Then((expr) =>
        ValueOrErrors.Default.return({
          kind: "Computed",
          columns: expr,
        }),
      );
    },
    ComputeLayout: (
      bindings: Bindings,
      visibleColumns: PredicateVisibleColumns,
    ): ValueOrErrors<CalculatedTableLayout, string> => {
      if (visibleColumns.kind == "Inlined") {
        return ValueOrErrors.Default.return({
          columns: visibleColumns.columns,
        });
      }
      console.debug(`Computing visible columns`, visibleColumns.columns);
      return Expr.Operations.Evaluate(bindings)(visibleColumns.columns).Then(
        (result) => {
          if (!PredicateValue.Operations.IsRecord(result)) {
            return ValueOrErrors.Default.throwOne(
              `Invalid visible columns: ${JSON.stringify(result)}`,
            );
          }
          console.debug(
            `Computed visible columns ${JSON.stringify(Array.from(result.fields.keys()))}`,
            result,
          );
          return ValueOrErrors.Default.return({
            columns: Array.from(result.fields.keys()),
          });
        },
      );
    },
  },
};
