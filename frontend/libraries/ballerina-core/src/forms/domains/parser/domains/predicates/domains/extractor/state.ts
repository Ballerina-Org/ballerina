import { List, Map } from "immutable";
import { ValueOrErrors, Errors } from "../../../../../../../../main";
import { ParsedType } from "../../../types/state";
import { PredicateValue } from "../../state";

export const extractPredicateValue = (
  lookupName: string,
  typesMap: Map<string, ParsedType<any>>,
  t: ParsedType<any>,
): ((
  v: PredicateValue,
) => ValueOrErrors<Array<PredicateValue>, Errors<any>>) => {
  switch (t.kind) {
    case "lookup": {
      const lookupType = typesMap.get(t.name);
      if (!lookupType) {
        return (_) =>
          ValueOrErrors.Default.throwOne(
            Errors.Default.singleton([t.name, "cannot find lookup type name"]),
          );
      }

      const traverseLookupValue = extractPredicateValue(
        lookupName,
        typesMap,
        lookupType,
      );
      return (v) =>
        t.name === lookupName
          ? ValueOrErrors.Default.return([v])
          : traverseLookupValue(v);
    }
    case "primitive":
      return (_) => ValueOrErrors.Default.return([]);
    case "option": {
      const traverseOptionValue = extractPredicateValue(
        lookupName,
        typesMap,
        t.value,
      );
      return (v: PredicateValue) =>
        !PredicateValue.Operations.IsOption(v)
          ? ValueOrErrors.Default.throwOne(
              Errors.Default.singleton(["not a ValueOption", v]),
            )
          : !v.isSome
            ? ValueOrErrors.Default.return([])
            : traverseOptionValue(v.value);
    }
    case "record": {
      const traverseRecordFields = t.fields.map((f) =>
        extractPredicateValue(lookupName, typesMap, f),
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
                  .map(([k, traverseField]) => traverseField(v.fields.get(k)!)),
              ),
            ).Map((listFailingChecks) =>
              listFailingChecks.reduce(
                (acc, curr) => [...acc, ...curr],
                [] as Array<PredicateValue>,
              ),
            );
    }
    case "application": {
      switch (t.value) {
        case "SingleSelection": {
          const traverseSingleSelection = extractPredicateValue(
            lookupName,
            typesMap,
            t.args[0],
          );
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
        case "MultiSelection": {
          // multi selection only has 1 arg type, which is the same for all the selcted elements
          const traverseMultiSelectionField = extractPredicateValue(
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
        case "Map": {
          const traverseKey = extractPredicateValue(
            lookupName,
            typesMap,
            t.args[0],
          );
          const traverseValue = extractPredicateValue(
            lookupName,
            typesMap,
            t.args[1],
          );
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
        case "Sum": {
          const traverseLeftValue = extractPredicateValue(
            lookupName,
            typesMap,
            t.args[0],
          );
          const traverseRightValue = extractPredicateValue(
            lookupName,
            typesMap,
            t.args[1],
          );
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
        case "Option": {
          const traverseOptionValue = extractPredicateValue(
            lookupName,
            typesMap,
            t.args[0],
          ); // TODO: check this
          return (v: PredicateValue) =>
            !PredicateValue.Operations.IsOption(v)
              ? ValueOrErrors.Default.throwOne(
                  Errors.Default.singleton(["not a ValueOption", v]),
                )
              : !v.isSome
                ? ValueOrErrors.Default.return([])
                : traverseOptionValue(v.value);
        }
        case "Tuple": {
          const traverseTupleFields = t.args.flatMap((f) =>
            extractPredicateValue(lookupName, typesMap, f),
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
        case "Union": {
          const traverseUnionFields = t.args.map((f) =>
            extractPredicateValue(lookupName, typesMap, f),
          );
          return (v) =>
            !PredicateValue.Operations.IsTuple(v)
              ? ValueOrErrors.Default.throwOne(
                  Errors.Default.singleton([
                    "not a ValueRecord (from union)",
                    v,
                  ]),
                )
              : ValueOrErrors.Operations.All(
                  List(
                    traverseUnionFields.flatMap((traverseField, idx) =>
                      !v.values.has(idx)
                        ? []
                        : [traverseField(v.values.get(idx)!)],
                    ),
                  ),
                ).Map((listFailingChecks) =>
                  listFailingChecks.reduce(
                    (acc, curr) => [...acc, ...curr],
                    [] as Array<PredicateValue>,
                  ),
                );
        }
        case "KeyOf":
          return (_) => ValueOrErrors.Default.return([]);
        case "List": {
          const traverseListField = extractPredicateValue(
            lookupName,
            typesMap,
            t.args[0],
          );
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
        case "Table":
        default:
          return (_) => ValueOrErrors.Default.return([]);
      }
    }
    case "union": {
      // const traverseUnionFields = t.args.map((f) =>
      //   extractPredicateValue(lookupName, typesMap, f.fields),
      // );
      return (_) => ValueOrErrors.Default.return([]);
    }
    case "table":
    default:
      return (_) =>
        ValueOrErrors.Default.throwOne(
          Errors.Default.singleton(["unknown type", t]),
        );
  }
};
