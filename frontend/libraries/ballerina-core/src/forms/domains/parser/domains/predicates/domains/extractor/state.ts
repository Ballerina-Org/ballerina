import { List, Map } from "immutable";
import {
  ValueOrErrors,
  Errors,
  DispatchParsedType,
  BasicFun,
  MapRepo,
} from "../../../../../../../../main";
import { PredicateValue } from "../../state";

type ExtractedTypeInstances = ValueOrErrors<
  Array<PredicateValue>,
  Errors<string[]>
>;
type TypeInstancesExtractor = BasicFun<PredicateValue, ExtractedTypeInstances>;

export const PredicateValueExtractor = {
  Operations: {
    ExtractPredicateValue: (
      lookupName: string,
      typesMap: Map<string, DispatchParsedType<any>>,
      t: DispatchParsedType<any>,
      debugPath: string[],
    ): TypeInstancesExtractor => {
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

          return t.name === lookupName
            ? (v) => ValueOrErrors.Default.return([v])
            : self(lookupName, typesMap, lookupType, debugPath);
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
            self(lookupName, typesMap, f, debugPath.concat(f.name.toString())),
          );
          return (v: PredicateValue): ExtractedTypeInstances =>
            !PredicateValue.Operations.IsRecord(v)
              ? ValueOrErrors.Default.throwOne(
                  Errors.Default.singleton([
                    "not a ValueRecord: " + debugPath.join("."),
                    JSON.stringify(v),
                  ]),
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
          const traverseSingleSelection = self(
            lookupName,
            typesMap,
            t.args[0],
            debugPath.concat(t.args[0].name.toString()),
          );
          return (v) =>
            !PredicateValue.Operations.IsOption(v)
              ? ValueOrErrors.Default.throwOne(
                  Errors.Default.singleton([
                    "not a ValueOption (from SingleSelection)",
                    JSON.stringify(v),
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
            debugPath.concat(t.args[0].name.toString()),
          );
          return (v: PredicateValue) =>
            !PredicateValue.Operations.IsRecord(v)
              ? ValueOrErrors.Default.throwOne(
                  Errors.Default.singleton([
                    "not a ValueRecord (from MultiSelection)",
                    JSON.stringify(v),
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
          const traverseKey = self(
            lookupName,
            typesMap,
            t.args[0],
            debugPath.concat(t.args[0].name.toString()),
          );
          const traverseValue = self(
            lookupName,
            typesMap,
            t.args[1],
            debugPath.concat(t.args[1].name.toString()),
          );
          return (v: PredicateValue) =>
            !PredicateValue.Operations.IsTuple(v)
              ? ValueOrErrors.Default.throwOne(
                  Errors.Default.singleton([
                    "not a ValueRecord (from Map)",
                    JSON.stringify(v),
                  ]),
                )
              : (
                  ValueOrErrors.Operations.All(
                    List(
                      v.values.map((entry) =>
                        !PredicateValue.Operations.IsTuple(entry)
                          ? ValueOrErrors.Default.throwOne(
                              Errors.Default.singleton([
                                "not a ValueRecord (from inner Map)",
                                JSON.stringify(entry),
                              ]),
                            )
                          : ValueOrErrors.Operations.All(
                              List(
                                [traverseKey, traverseValue].map(
                                  (traverseField, kvIdx) =>
                                    traverseField(entry.values.get(kvIdx)!),
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
                  ) as ValueOrErrors<List<PredicateValue[]>, Errors<any>>
                ).Map((listFailingChecks) =>
                  listFailingChecks.reduce(
                    (acc, curr) => [...acc, ...curr],
                    [] as Array<PredicateValue>,
                  ),
                );
        }
        case "sum": {
          const traverseLeftValue = self(
            lookupName,
            typesMap,
            t.args[0],
            debugPath.concat(t.args[0].name.toString()),
          );
          const traverseRightValue = self(
            lookupName,
            typesMap,
            t.args[1],
            debugPath.concat(t.args[1].name.toString()),
          );
          return (v) =>
            !PredicateValue.Operations.IsSum(v)
              ? ValueOrErrors.Default.throwOne(
                  Errors.Default.singleton([
                    "not a ValueSum",
                    JSON.stringify(v),
                  ]),
                )
              : (v.value.kind === "l" ? traverseLeftValue : traverseRightValue)(
                  v.value.value,
                );
          //  .Map((listFailingChecks) =>
          //   listFailingChecks.reduce(
          //     (acc, curr) => [...acc, ...curr],
          //     [] as Array<PredicateValue>,
          //   ),
          // );
        }
        case "tuple": {
          const traverseTupleFields = t.args.map((f) =>
            self(lookupName, typesMap, f, debugPath.concat(f.name.toString())),
          );
          return (v) =>
            !PredicateValue.Operations.IsTuple(v)
              ? ValueOrErrors.Default.throwOne(
                  Errors.Default.singleton([
                    "not a ValueTuple",
                    JSON.stringify(v),
                  ]),
                )
              : ValueOrErrors.Operations.All(
                  List(
                    traverseTupleFields.map((traverseField, idx) =>
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
        case "union": {
          const traverseCases: Map<string, TypeInstancesExtractor> = t.args
            .map((f) =>
              self(
                lookupName,
                typesMap,
                f,
                debugPath.concat(f.name.toString()),
              ),
            )
            .toMap();
          return (v): ExtractedTypeInstances =>
            PredicateValue.Operations.IsPrimitive(v)
              ? ValueOrErrors.Default.return([])
              : !PredicateValue.Operations.IsUnionCase(v)
                ? ValueOrErrors.Default.throwOne(
                    Errors.Default.singleton([
                      "not a ValueUnion (from union)" + debugPath.join("."),
                      JSON.stringify(v),
                    ]),
                  )
                : MapRepo.Operations.tryFindWithError<
                    string,
                    TypeInstancesExtractor,
                    Errors<string[]>
                  >(v.caseName, traverseCases, () =>
                    Errors.Default.singleton([
                      `unexpected union case ${v.caseName}`,
                      JSON.stringify(v),
                    ]),
                  ).Then((traverseCase: TypeInstancesExtractor) =>
                    traverseCase(v.fields),
                  );
        }
        case "one": {
          const traverseValue: TypeInstancesExtractor = self(
            lookupName,
            typesMap,
            t.args,
            debugPath.concat(t.args.name.toString()),
          );
          return (v): ExtractedTypeInstances =>
            PredicateValue.Operations.IsOption(v)
              ? v.isSome
                ? traverseValue(v.value)
                : ValueOrErrors.Default.return([])
              : PredicateValue.Operations.IsSum(v)
                ? v.value.kind == "r"
                  ? traverseValue(v.value.value)
                  : ValueOrErrors.Default.return([])
                : ValueOrErrors.Default.throwOne(
                    Errors.Default.singleton([
                      "not a One/Option or One/Sum (from one)",
                      JSON.stringify(v),
                    ]),
                  );
        }
        case "list": {
          const traverseListField = self(
            lookupName,
            typesMap,
            t.args[0],
            debugPath.concat(t.args[0].name.toString()),
          );
          return (v) =>
            !PredicateValue.Operations.IsTuple(v)
              ? ValueOrErrors.Default.throwOne(
                  Errors.Default.singleton([
                    "not a ValueTuple (from List)",
                    JSON.stringify(v),
                  ]),
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
              Errors.Default.singleton(["unknown type", JSON.stringify(t)]),
            );
      }
    },
  },
};
