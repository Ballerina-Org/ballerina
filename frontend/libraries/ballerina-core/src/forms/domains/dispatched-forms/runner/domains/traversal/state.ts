import {
  Option,
  ValueOrErrors,
  MapRepo,
  PredicateValue,
  DispatchParsedType,
  Renderer,
  BasicFun,
  Expr,
  Updater,
  BasicUpdater,
  FormLayout,
  Unit,
  unit,
  TableLayout,
} from "ballerina-core";
import { List, Map, Set } from "immutable";

export type EvalContext<T, Res> = {
  global: PredicateValue;
  root: PredicateValue;
  local: PredicateValue;
  traversalIterator: PredicateValue;
};

export type TraversalContext<T, Res> = {
  types: Map<string, DispatchParsedType<T>>;
  forms: Map<string, Renderer<T>>;
  primitiveRendererNamesByType: Map<string, Set<string>>;
  joinRes: BasicFun<[Res, Res], Res>;
  zeroRes: BasicFun<Unit, Res>;
  traverseSingleType: Traversal<T, Res>;
};

export type Traversal<T, Res> = BasicFun<
  DispatchParsedType<T>,
  Option<
    // based on the type provided, it can return `None` depending on some predicate such as "there are no children with the desired type T that we are searching". This is an important performance optimization
    ValueTraversal<T, Res>
  >
>;

export type ValueTraversal<T, Res> = BasicFun<
  EvalContext<T, Res>, // the proper dynamic part of the evaluation depends solely on the eval context (root, global, local) and the actual value being traversed
  ValueOrErrors<Res, string>
>;

export const RendererTraversal = {
  Operations: {
    Run: <T, Res>(
      type: DispatchParsedType<T>,
      renderer: Renderer<T>,
      traversalContext: TraversalContext<T, Res>,
    ): ValueOrErrors<Option<ValueTraversal<T, Res>>, string> => {
      const rec = RendererTraversal.Operations.Run<T, Res>;

      const mapEvalContext = (
        f: BasicUpdater<EvalContext<T, Res>>,
      ): Updater<Option<ValueTraversal<T, Res>>> =>
        Updater(
          Option.Updaters.some<ValueTraversal<T, Res>>(
            (v) => (ctx) => v(f(ctx)),
          ),
        );

      const traverseNode = traversalContext.traverseSingleType(type);

      if (
        type.kind == "primitive" ||
        type.kind == "singleSelection" ||
        type.kind == "multiSelection"
      ) {
        return ValueOrErrors.Default.return(traverseNode);
      }

      // TODO -- later when we only resolve lookups at the last moment, we can remove the other type checks
      if (
        (type.kind == "lookup" ||
          type.kind == "record" ||
          type.kind == "union" ||
          type.kind == "table") &&
        renderer.kind == "lookupRenderer"
      ) {
        if (traversalContext.primitiveRendererNamesByType.has(type.name)) {
          if (
            traversalContext.primitiveRendererNamesByType
              .get(type.name)!
              .has(renderer.renderer)
          ) {
            return ValueOrErrors.Default.return(traverseNode);
          }
        }
        if (traversalContext.forms.has(renderer.renderer)) {
          // renderer.renderer is the form name
          // this is a form lookup, so "local" changes here to the traversed value
          return (
            type.kind == "lookup"
              ? MapRepo.Operations.tryFindWithError(
                  type.name,
                  traversalContext.types,
                  () => `Error: cannot find type ${type.name} in types`,
                )
              : ValueOrErrors.Default.return<DispatchParsedType<T>, string>(
                  type,
                )
          ).Then((resolvedType) => {
            return rec(
              resolvedType,
              traversalContext.forms.get(renderer.renderer)!,
              traversalContext,
            ).Then((valueTraversal: Option<ValueTraversal<T, Res>>) => {
              return ValueOrErrors.Default.return(
                mapEvalContext((ctx) => ({
                  ...ctx,
                  local: ctx.traversalIterator,
                }))(valueTraversal),
              );
            });
          });
        }
        return ValueOrErrors.Default.throwOne(
          `Error: cannot resolve lookup renderer ${renderer.renderer} for type ${type.name}.`,
        );
      }
      if (type.kind == "record" && renderer.kind == "recordRenderer") {
        return ValueOrErrors.Operations.All(
          List(
            renderer.fields
              .map((fieldRenderer, fieldName) =>
                rec(
                  fieldRenderer.renderer.type,
                  fieldRenderer.renderer,
                  traversalContext,
                ).Then((fieldTraversal) => {
                  return ValueOrErrors.Default.return({
                    fieldName: fieldName,
                    visibility: fieldRenderer.visible,
                    fieldTraversal: fieldTraversal,
                  });
                }),
              )
              .valueSeq(),
          ),
        ).Then((fieldTraversals) => {
          if (
            fieldTraversals.every((f) => f.fieldTraversal.kind == "l") &&
            traverseNode.kind == "l"
          ) {
            return ValueOrErrors.Default.return(Option.Default.none());
          }
          return ValueOrErrors.Default.return(
            Option.Default.some((evalContext: EvalContext<T, Res>) => {
              if (
                !PredicateValue.Operations.IsRecord(
                  evalContext.traversalIterator,
                )
              )
                return ValueOrErrors.Default.throwOne(
                  `Error: traversal iterator is not a record, got ${JSON.stringify(
                    evalContext.traversalIterator,
                    undefined,
                    2,
                  )}`,
                );
              const visibleFieldsRes =
                FormLayout.Operations.ComputeVisibleFieldsForRecord(
                  Map([
                    ["global", evalContext.global],
                    ["local", evalContext.local],
                    ["root", evalContext.root],
                  ]),
                  renderer.tabs,
                );
              // TODO later, make this monadic
              if (visibleFieldsRes.kind == "errors") {
                return visibleFieldsRes;
              }
              const visibleFields = visibleFieldsRes.value;
              const traversalIteratorFields =
                evalContext.traversalIterator.fields;
              return ValueOrErrors.Operations.All(
                fieldTraversals.flatMap((f) => {
                  // should be a map and instead of flatmap and [] a VoE.default.return([]) then an All on this and then a flatmap, everything is returned in a VoE
                  if (
                    f.fieldTraversal.kind == "l" ||
                    !visibleFields.includes(f.fieldName)
                  )
                    return [];
                  if (f.visibility != undefined) {
                    const visible = Expr.Operations.Evaluate(
                      Map([
                        ["global", evalContext.global],
                        ["local", evalContext.local],
                        ["root", evalContext.root],
                      ]),
                    )(f.visibility);
                    if (visible.kind == "value" && !visible.value) {
                      return [];
                    }
                  }

                  return [
                    f.fieldTraversal.value({
                      ...evalContext,
                      traversalIterator: traversalIteratorFields.get(
                        f.fieldName,
                      )!,
                    }),
                  ];
                }),
              ).Then((fieldResults: List<Res>) => {
                return traverseNode.kind == "r"
                  ? traverseNode
                      .value(evalContext)
                      .Then((nodeResult: Res) =>
                        ValueOrErrors.Default.return(
                          fieldResults.reduce(
                            (acc, res) => traversalContext.joinRes([acc, res]),
                            nodeResult,
                          ),
                        ),
                      )
                  : ValueOrErrors.Default.return(
                      fieldResults.reduce(
                        (acc, res) => traversalContext.joinRes([acc, res]),
                        traversalContext.zeroRes(unit),
                      ),
                    );
              });
            }),
          );
        });
      }
      if (
        type.kind == "sum" &&
        (renderer.kind == "sumRenderer" ||
          renderer.kind == "sumUnitDateRenderer")
      ) {
        // this will be removed when sums become proper partials
        if (renderer.kind == "sumUnitDateRenderer") {
          if (traverseNode.kind == "l") {
            return ValueOrErrors.Default.return(Option.Default.none());
          }
          return ValueOrErrors.Default.return(
            Option.Default.some((evalContext: EvalContext<T, Res>) =>
              traverseNode.value(evalContext),
            ),
          );
        }
        if (renderer.kind == "sumRenderer") {
          return rec(
            type.args[0],
            renderer.leftRenderer.renderer,
            traversalContext,
          ).Then((leftTraversal) =>
            rec(
              type.args[1],
              renderer.rightRenderer.renderer,
              traversalContext,
            ).Then((rightTraversal) => {
              if (
                leftTraversal.kind == "l" &&
                rightTraversal.kind == "l" &&
                traverseNode.kind == "l"
              ) {
                return ValueOrErrors.Default.return(Option.Default.none());
              }
              return ValueOrErrors.Default.return(
                Option.Default.some<ValueTraversal<T, Res>>(
                  (evalContext: EvalContext<T, Res>) => {
                    const iterator = evalContext.traversalIterator;
                    if (!PredicateValue.Operations.IsSum(iterator)) {
                      return ValueOrErrors.Default.throwOne(
                        `Error: traversal iterator is not a sum, got ${evalContext.traversalIterator}`,
                      );
                    }

                    const isRight = iterator.value.kind == "r";

                    if (isRight && rightTraversal.kind == "r") {
                      if (traverseNode.kind == "r") {
                        return traverseNode
                          .value(evalContext)
                          .Then((nodeResult: Res) => {
                            return rightTraversal
                              .value({
                                ...evalContext,
                                traversalIterator: iterator.value.value,
                              })
                              .Then((rightRes) => {
                                return ValueOrErrors.Default.return<
                                  Res,
                                  string
                                >(
                                  traversalContext.joinRes([
                                    nodeResult,
                                    rightRes,
                                  ]),
                                );
                              });
                          });
                      }
                      return rightTraversal
                        .value({
                          ...evalContext,
                          traversalIterator: iterator.value.value,
                        })
                        .Then((rightRes) => {
                          return ValueOrErrors.Default.return<Res, string>(
                            rightRes,
                          );
                        });
                    }

                    if (!isRight && leftTraversal.kind == "r") {
                      if (traverseNode.kind == "r") {
                        return traverseNode
                          .value(evalContext)
                          .Then((nodeResult: Res) => {
                            return leftTraversal
                              .value({
                                ...evalContext,
                                traversalIterator: iterator.value.value,
                              })
                              .Then((leftRes) => {
                                return ValueOrErrors.Default.return<
                                  Res,
                                  string
                                >(
                                  traversalContext.joinRes([
                                    nodeResult,
                                    leftRes,
                                  ]),
                                );
                              });
                          });
                      }
                      return leftTraversal
                        .value({
                          ...evalContext,
                          traversalIterator: iterator.value.value,
                        })
                        .Then((leftRes) => {
                          return ValueOrErrors.Default.return<Res, string>(
                            leftRes,
                          );
                        });
                    }

                    return ValueOrErrors.Default.return<Res, string>(
                      traversalContext.zeroRes(unit),
                    );
                  },
                ),
              );
            }),
          );
        }
      }

      if (type.kind == "tuple" && renderer.kind == "tupleRenderer") {
        return ValueOrErrors.Operations.All(
          List(
            renderer.itemRenderers.map((itemRenderer, index) => {
              return rec(
                type.args[index],
                itemRenderer.renderer,
                traversalContext,
              ).Then((itemTraversal) => {
                return ValueOrErrors.Default.return({
                  index: index,
                  itemTraversal: itemTraversal,
                });
              });
            }),
          ),
        ).Then((itemTraversals) => {
          if (
            itemTraversals.every((i) => i.itemTraversal.kind == "l") &&
            traverseNode.kind == "l"
          ) {
            return ValueOrErrors.Default.return(Option.Default.none());
          }

          return ValueOrErrors.Default.return(
            Option.Default.some((evalContext: EvalContext<T, Res>) => {
              const iterator = evalContext.traversalIterator;
              if (!PredicateValue.Operations.IsTuple(iterator)) {
                return ValueOrErrors.Default.throwOne(
                  `Error: traversal iterator is not a tuple, got ${evalContext.traversalIterator}`,
                );
              }
              return ValueOrErrors.Operations.All(
                itemTraversals.flatMap((i) =>
                  i.itemTraversal.kind == "r"
                    ? [
                        i.itemTraversal.value({
                          ...evalContext,
                          traversalIterator: iterator.values.get(i.index)!,
                        }),
                      ]
                    : [],
                ),
              ).Then((itemResults) => {
                return traverseNode.kind == "r"
                  ? traverseNode
                      .value(evalContext)
                      .Then((nodeResult: Res) =>
                        ValueOrErrors.Default.return(
                          itemResults.reduce(
                            (acc, res) => traversalContext.joinRes([acc, res]),
                            nodeResult,
                          ),
                        ),
                      )
                  : ValueOrErrors.Default.return(
                      itemResults.reduce(
                        (acc, res) => traversalContext.joinRes([acc, res]),
                        traversalContext.zeroRes(unit),
                      ),
                    );
              });
            }),
          );
        });
      }

      if (type.kind == "union" && renderer.kind == "unionRenderer") {
        return ValueOrErrors.Operations.All(
          List(
            renderer.cases
              .map((caseRenderer, caseName) =>
                rec(caseRenderer.type, caseRenderer, traversalContext).Then(
                  (caseTraversal) =>
                    ValueOrErrors.Default.return({
                      caseName: caseName,
                      caseTraversal: caseTraversal,
                    }),
                ),
              )
              .valueSeq(),
          ),
        ).Then((caseTraversals) => {
          if (
            caseTraversals.every((c) => c.caseTraversal.kind == "l") &&
            traverseNode.kind == "l"
          ) {
            return ValueOrErrors.Default.return(Option.Default.none());
          }
          return ValueOrErrors.Default.return(
            Option.Default.some((evalContext: EvalContext<T, Res>) => {
              const iterator = evalContext.traversalIterator;
              if (!PredicateValue.Operations.IsUnionCase(iterator)) {
                return ValueOrErrors.Default.throwOne(
                  `Error: traversal iterator is not a union case, got ${evalContext.traversalIterator}`,
                );
              }
              const caseName = iterator.caseName;
              const caseTraversal = caseTraversals.find(
                (c) => c.caseName == caseName,
              )?.caseTraversal;
              if (!caseTraversal) {
                return ValueOrErrors.Default.throwOne(
                  `Error: cannot find case traversal for case ${caseName}`,
                );
              }
              return caseTraversal.kind == "r"
                ? caseTraversal
                    .value({
                      ...evalContext,
                      traversalIterator: iterator.fields,
                    })
                    .Then((caseRes) => {
                      return traverseNode.kind == "r"
                        ? traverseNode
                            .value(evalContext)
                            .Then((nodeResult: Res) => {
                              return ValueOrErrors.Default.return(
                                traversalContext.joinRes([nodeResult, caseRes]),
                              );
                            })
                        : ValueOrErrors.Default.return(caseRes);
                    })
                : traverseNode.kind == "r"
                  ? traverseNode.value(evalContext).Then((nodeResult: Res) => {
                      return ValueOrErrors.Default.return(nodeResult);
                    })
                  : ValueOrErrors.Default.return(
                      traversalContext.zeroRes(unit),
                    );
            }),
          );
        });
      }
      // TODO -- add recursion for the detailsRenderer, but requires state
      if (type.kind == "table" && renderer.kind == "tableRenderer") {
        return ValueOrErrors.Operations.All(
          List(
            renderer.columns
              .map((column, columnName) => {
                return rec(
                  column.renderer.type,
                  column.renderer,
                  traversalContext,
                ).Then((columnTraversal) => {
                  return ValueOrErrors.Default.return({
                    columnName: columnName,
                    columnTraversal: columnTraversal,
                  });
                });
              })
              .valueSeq(),
          ),
        ).Then((columnTraversals) => {
          if (
            columnTraversals.every((c) => c.columnTraversal.kind == "l") &&
            traverseNode.kind == "l"
          ) {
            return ValueOrErrors.Default.return(Option.Default.none());
          }
          return ValueOrErrors.Default.return(
            Option.Default.some((evalContext: EvalContext<T, Res>) => {
              const iterator = evalContext.traversalIterator;
              if (!PredicateValue.Operations.IsTable(iterator)) {
                return ValueOrErrors.Default.throwOne(
                  `Error: traversal iterator is not a table, got ${evalContext.traversalIterator}`,
                );
              }
              return TableLayout.Operations.ComputeLayout(
                Map([
                  ["global", evalContext.global],
                  ["local", evalContext.local],
                  ["root", evalContext.root],
                ]),
                renderer.visibleColumns,
              ).Then((visibleColumns) => {
                // Note: we do not allow visiblity predicates on individual column cells
                return ValueOrErrors.Operations.All<Res, string>(
                  columnTraversals.flatMap((c) => {
                    const colTraversal = c.columnTraversal;
                    if (
                      colTraversal.kind == "l" ||
                      !visibleColumns.columns.includes(c.columnName)
                    ) {
                      return [];
                    }
                    return iterator.data.valueSeq().flatMap((row) => {
                      // TODO make this monadic
                      const columnValue = row.fields.get(c.columnName);
                      if (!columnValue) {
                        return [
                          ValueOrErrors.Default.throwOne(
                            `Error: cannot find column ${
                              c.columnName
                            } in row ${JSON.stringify(row)}`,
                          ),
                        ];
                      }
                      return [
                        colTraversal.value({
                          ...evalContext,
                          traversalIterator: columnValue,
                        }),
                      ];
                    });
                  }),
                ).Then((columnResults) => {
                  return traverseNode.kind == "r"
                    ? traverseNode
                        .value(evalContext)
                        .Then((nodeResult: Res) =>
                          ValueOrErrors.Default.return(
                            columnResults.reduce(
                              (acc, res) =>
                                traversalContext.joinRes([acc, res]),
                              nodeResult,
                            ),
                          ),
                        )
                    : ValueOrErrors.Default.return(
                        columnResults.reduce(
                          (acc, res) => traversalContext.joinRes([acc, res]),
                          traversalContext.zeroRes(unit),
                        ),
                      );
                });
              });
            }),
          );
        });
      }
      // TODO -- should we also look at the previewRenderer? Woud also requite state
      if (type.kind == "one" && renderer.kind == "oneRenderer") {
        return rec(
          renderer.detailsRenderer.renderer.type,
          renderer.detailsRenderer.renderer,
          traversalContext,
        ).Then((itemTraversal) => {
          if (itemTraversal.kind == "l" && traverseNode.kind == "l") {
            return ValueOrErrors.Default.return(Option.Default.none());
          }
          return ValueOrErrors.Default.return(
            Option.Default.some<ValueTraversal<T, Res>>(
              (evalContext: EvalContext<T, Res>) => {
                const iterator = evalContext.traversalIterator;

                // Handle partial ones
                if (PredicateValue.Operations.IsUnit(iterator)) {
                  return traverseNode.kind == "r"
                    ? traverseNode.value(evalContext)
                    : ValueOrErrors.Default.return(
                        traversalContext.zeroRes(unit),
                      );
                }
                if (!PredicateValue.Operations.IsOption(iterator)) {
                  return ValueOrErrors.Default.throwOne<Res, string>(
                    `Error: traversal iterator for one is not an option, got ${iterator}`,
                  );
                }
                const isSome =
                  PredicateValue.Operations.IsOption(iterator) &&
                  iterator.isSome;

                if (!isSome) {
                  return traverseNode.kind == "r"
                    ? traverseNode.value(evalContext)
                    : ValueOrErrors.Default.return(
                        traversalContext.zeroRes(unit),
                      );
                }

                return traverseNode.kind == "r"
                  ? traverseNode.value(evalContext).Then((nodeResult: Res) => {
                      return itemTraversal.kind == "r"
                        ? itemTraversal
                            .value({
                              ...evalContext,
                              traversalIterator: iterator.value,
                            })
                            .Then((itemResult: Res) => {
                              return ValueOrErrors.Default.return(
                                traversalContext.joinRes([
                                  nodeResult,
                                  itemResult,
                                ]),
                              );
                            })
                        : ValueOrErrors.Default.return(nodeResult);
                    })
                  : itemTraversal.kind == "r"
                    ? itemTraversal.value({
                        ...evalContext,
                        traversalIterator: iterator.value,
                      })
                    : ValueOrErrors.Default.return(
                        traversalContext.zeroRes(unit),
                      );
              },
            ),
          );
        });
      }

      if (type.kind == "list" && renderer.kind == "listRenderer") {
        return rec(
          type.args[0],
          renderer.elementRenderer.renderer,
          traversalContext,
        ).Then((elementTraversal) => {
          if (elementTraversal.kind == "l" && traverseNode.kind == "l") {
            return ValueOrErrors.Default.return(Option.Default.none());
          }
          return ValueOrErrors.Default.return(
            Option.Default.some((evalContext: EvalContext<T, Res>) => {
              const iterator = evalContext.traversalIterator;
              if (!PredicateValue.Operations.IsTuple(iterator)) {
                return ValueOrErrors.Default.throwOne<Res, string>(
                  `Error: traversal iterator for list is not a tuple, got ${JSON.stringify(
                    iterator,
                    null,
                    2,
                  )}`,
                );
              }
              return ValueOrErrors.Operations.All<Res, string>(
                iterator.values.map((value) =>
                  elementTraversal.kind == "r"
                    ? elementTraversal.value({
                        ...evalContext,
                        traversalIterator: value,
                      })
                    : ValueOrErrors.Default.return(
                        traversalContext.zeroRes(unit),
                      ),
                ),
              ).Then((elementResults) =>
                traverseNode.kind == "r"
                  ? traverseNode
                      .value(evalContext)
                      .Then((nodeResult: Res) =>
                        ValueOrErrors.Default.return(
                          elementResults.reduce(
                            (acc, res) => traversalContext.joinRes([acc, res]),
                            nodeResult,
                          ),
                        ),
                      )
                  : ValueOrErrors.Default.return(
                      elementResults.reduce(
                        (acc, res) => traversalContext.joinRes([acc, res]),
                        traversalContext.zeroRes(unit),
                      ),
                    ),
              );
            }),
          );
        });
      }

      if (type.kind == "map" && renderer.kind == "mapRenderer") {
        return rec(
          type.args[0],
          renderer.keyRenderer.renderer,
          traversalContext,
        ).Then((keyTraversal) => {
          return rec(
            type.args[1],
            renderer.valueRenderer.renderer,
            traversalContext,
          ).Then((valueTraversal) => {
            if (
              keyTraversal.kind == "l" &&
              valueTraversal.kind == "l" &&
              traverseNode.kind == "l"
            ) {
              return ValueOrErrors.Default.return(Option.Default.none());
            }
            return ValueOrErrors.Default.return(
              Option.Default.some<ValueTraversal<T, Res>>(
                (evalContext: EvalContext<T, Res>) => {
                  if (keyTraversal.kind == "l" && valueTraversal.kind == "l") {
                    return traverseNode.kind == "r"
                      ? traverseNode.value(evalContext)
                      : ValueOrErrors.Default.return<Res, string>(
                          traversalContext.zeroRes(unit),
                        );
                  }

                  const iterator = evalContext.traversalIterator;
                  if (!PredicateValue.Operations.IsTuple(iterator)) {
                    return ValueOrErrors.Default.throwOne<Res, string>(
                      `Error: traversal iterator for map is not a tuple, got ${JSON.stringify(
                        iterator,
                        null,
                        2,
                      )}`,
                    );
                  }

                  return ValueOrErrors.Operations.All<Res, string>(
                    iterator.values.map((value) => {
                      if (!PredicateValue.Operations.IsTuple(value)) {
                        return ValueOrErrors.Default.throwOne<Res, string>(
                          `Error: traversal iterator for map keyValue is not a tuple, got ${JSON.stringify(
                            value,
                            null,
                            2,
                          )}`,
                        );
                      }
                      const keyRes =
                        keyTraversal.kind == "r"
                          ? keyTraversal.value({
                              ...evalContext,
                              traversalIterator: value.values.get(0)!,
                            })
                          : ValueOrErrors.Default.return(
                              traversalContext.zeroRes(unit),
                            );

                      const valueRes =
                        valueTraversal.kind == "r"
                          ? valueTraversal.value({
                              ...evalContext,
                              traversalIterator: value.values.get(1)!,
                            })
                          : ValueOrErrors.Default.return(
                              traversalContext.zeroRes(unit),
                            );

                      if (keyRes.kind == "errors") {
                        return keyRes as ValueOrErrors<Res, string>;
                      }
                      if (valueRes.kind == "errors") {
                        return valueRes as ValueOrErrors<Res, string>;
                      }

                      return ValueOrErrors.Default.return<Res, string>(
                        traversalContext.joinRes([
                          keyRes.value,
                          valueRes.value,
                        ]),
                      );
                    }),
                  ).Then((keyValueResults) => {
                    return traverseNode.kind == "r"
                      ? traverseNode
                          .value(evalContext)
                          .Then((nodeResult: Res) => {
                            return ValueOrErrors.Default.return<Res, string>(
                              keyValueResults.reduce(
                                (acc, res) =>
                                  traversalContext.joinRes([acc, res]),
                                nodeResult,
                              ),
                            );
                          })
                      : ValueOrErrors.Default.return<Res, string>(
                          keyValueResults.reduce(
                            (acc, res) => traversalContext.joinRes([acc, res]),
                            traversalContext.zeroRes(unit),
                          ),
                        );
                  });
                },
              ),
            );
          });
        });
      }

      return ValueOrErrors.Default.return(Option.Default.none());
    },
  },
};
