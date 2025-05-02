import { List, Map } from "immutable";
import {
  ValueOrErrors,
  Errors,
  DispatchParsedType,
} from "../../../../../../../../main";
import { PredicateValue } from "../../state";

export const PredicateValueExtractor = {
  Operations: {
    ExtractPredicateValue: (
      lookupName: string,
      typesMap: Map<string, DispatchParsedType<any>>,
      t: DispatchParsedType<any>,
    ): ((
      v: PredicateValue,
    ) => ValueOrErrors<Array<PredicateValue>, Errors<any>>) => {
      const self = PredicateValueExtractor.Operations.ExtractPredicateValue;
      switch (t.kind) {
        case "lookup": {
          const lookupType = typesMap.get(t.name);
          if (!lookupType) {
            return (_) =>
              ValueOrErrors.Default.throwOne(
                Errors.Default.singleton([
                  t.name,
                  "cannot find lookup type name",
                ]),
              );
          }

          const traverseLookupValue = self(lookupName, typesMap, lookupType);
          return (v) =>
            t.name === lookupName
              ? ValueOrErrors.Default.return([v])
              : traverseLookupValue(v);
        }
        case "primitive":
          return (_) => ValueOrErrors.Default.return([]);
        // case "option": {
        //   const traverseOptionValue = self(lookupName, typesMap, t.value);
        //   return (v: PredicateValue) =>
        //     !PredicateValue.Operations.IsOption(v)
        //       ? ValueOrErrors.Default.throwOne(
        //           Errors.Default.singleton(["not a ValueOption", v]),
        //         )
        //       : !v.isSome
        //       ? ValueOrErrors.Default.return([])
        //       : traverseOptionValue(v.value);
        // }
        case "record": {
          const traverseRecordFields = t.fields.map((f) =>
            self(lookupName, typesMap, f),
          );
          return (v: PredicateValue) =>
            !PredicateValue.Operations.IsRecord(v)
              ? ValueOrErrors.Default.throwOne(
                  Errors.Default.singleton(["not a ValueRecord", v]),
                )
              : ValueOrErrors.Operations.All(
                  List(
                    traverseRecordFields
                      .entrySeq()
                      .map(([k, traverseField]) =>
                        traverseField(v.fields.get(k)!),
                      ),
                  ),
                ).Map((listFailingChecks) =>
                  listFailingChecks.reduce(
                    (acc, curr) => [...acc, ...curr],
                    [] as Array<PredicateValue>,
                  ),
                );
        }
        case "singleSelection": {
          const traverseSingleSelection = self(lookupName, typesMap, t.args[0]);
          return (v) =>
            !PredicateValue.Operations.IsOption(v)
              ? ValueOrErrors.Default.throwOne(
                  Errors.Default.singleton([
                    "not a ValueOption (from SingleSelection)",
                    v,
                  ]),
                )
              : !v.isSome
                ? ValueOrErrors.Default.return([])
                : traverseSingleSelection(v.value);
        }
        case "multiSelection": {
          // multi selection only has 1 arg type, which is the same for all the selcted elements
          const traverseMultiSelectionField = self(
            lookupName,
            typesMap,
            t.args[0],
          );
          return (v: PredicateValue) =>
            !PredicateValue.Operations.IsRecord(v)
              ? ValueOrErrors.Default.throwOne(
                  Errors.Default.singleton([
                    "not a ValueRecord (from MultiSelection)",
                    v,
                  ]),
                )
              : ValueOrErrors.Operations.All(
                  List(
                    v.fields
                      .entrySeq()
                      .map(([_, field]) => traverseMultiSelectionField(field)),
                  ),
                ).Map((listFailingChecks) =>
                  listFailingChecks.reduce(
                    (acc, curr) => [...acc, ...curr],
                    [] as Array<PredicateValue>,
                  ),
                );
        }
        case "map": {
          const traverseKey = self(lookupName, typesMap, t.args[0]);
          const traverseValue = self(lookupName, typesMap, t.args[1]);
          return (v: PredicateValue) =>
            !PredicateValue.Operations.IsRecord(v)
              ? ValueOrErrors.Default.throwOne(
                  Errors.Default.singleton(["not a ValueRecord (from Map)", v]),
                )
              : ValueOrErrors.Operations.All(
                  List(
                    v.fields
                      .entrySeq()
                      .map(([_k, field]) =>
                        ValueOrErrors.Operations.All(
                          List(
                            [traverseKey, traverseValue].map((traverseField) =>
                              traverseField(field),
                            ),
                          ),
                        ).Map((listFailingChecks) =>
                          listFailingChecks.reduce(
                            (acc, curr) => [...acc, ...curr],
                            [] as Array<PredicateValue>,
                          ),
                        ),
                      ),
                  ),
                ).Map((listFailingChecks) =>
                  listFailingChecks.reduce(
                    (acc, curr) => [...acc, ...curr],
                    [] as Array<PredicateValue>,
                  ),
                );
        }
        case "sum": {
          const traverseLeftValue = self(lookupName, typesMap, t.args[0]);
          const traverseRightValue = self(lookupName, typesMap, t.args[1]);
          return (v) =>
            !PredicateValue.Operations.IsSum(v)
              ? ValueOrErrors.Default.throwOne(
                  Errors.Default.singleton(["not a ValueSum", v]),
                )
              : ValueOrErrors.Operations.All(
                  List(
                    [traverseLeftValue, traverseRightValue].map(
                      (traverseField) => traverseField(v),
                    ),
                  ),
                ).Map((listFailingChecks) =>
                  listFailingChecks.reduce(
                    (acc, curr) => [...acc, ...curr],
                    [] as Array<PredicateValue>,
                  ),
                );
        }
        case "tuple": {
          const traverseTupleFields = t.args.map((f) =>
            self(lookupName, typesMap, f),
          );
          return (v) =>
            !PredicateValue.Operations.IsTuple(v)
              ? ValueOrErrors.Default.throwOne(
                  Errors.Default.singleton(["not a ValueTuple", v]),
                )
              : ValueOrErrors.Operations.All(
                  List(
                    traverseTupleFields.flatMap((traverseField, idx) =>
                      traverseField(v.values.get(idx)!),
                    ),
                  ),
                ).Map((listFailingChecks) =>
                  listFailingChecks.reduce(
                    (acc, curr) => [...acc, ...curr],
                    [] as Array<PredicateValue>,
                  ),
                );
        }
        // case "union": {
        //   const traverseUnionFields = t.args.map((f) =>
        //     self(lookupName, typesMap, f.fields),
        //   );
        //   return (v) =>
        //     !PredicateValue.Operations.IsTuple(v)
        //       ? ValueOrErrors.Default.throwOne(
        //           Errors.Default.singleton([
        //             "not a ValueRecord (from union)",
        //             v,
        //           ]),
        //         )
        //       : (ValueOrErrors.Operations.All(
        //           traverseUnionFields
        //             .filter((_, k) => v.fields.has(k))
        //             .map<ValueOrErrors<Array<PredicateValue>, Errors<any>>>(
        //               (traverseField, k) => traverseField(v.values.get(k)!),
        //             )
        //             .valueSeq()
        //             .toList(),
        //         ).Map((listFailingChecks) =>
        //           listFailingChecks.reduce(
        //             (acc, curr) => [...acc, ...curr],
        //             [] as Array<PredicateValue>,
        //           ),
        //         ));
        // }
        case "list": {
          const traverseListField = self(lookupName, typesMap, t.args[0]);
          return (v) =>
            !PredicateValue.Operations.IsTuple(v)
              ? ValueOrErrors.Default.throwOne(
                  Errors.Default.singleton(["not a ValueTuple (from List)", v]),
                )
              : ValueOrErrors.Operations.All(
                  List(v.values.map((v) => traverseListField(v))),
                ).Map((listFailingChecks) =>
                  listFailingChecks.reduce(
                    (acc, curr) => [...acc, ...curr],
                    [] as Array<PredicateValue>,
                  ),
                );
        }
        case "table": {
          return (_) => ValueOrErrors.Default.return([]);
        }
        default:
          return (_) =>
            ValueOrErrors.Default.throwOne(
              Errors.Default.singleton(["unknown type", t]),
            );
      }
    },
  },
};
