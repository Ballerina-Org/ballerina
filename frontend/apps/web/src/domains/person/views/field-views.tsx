import {
  FormLabel,
  CollectionReference,
  DateView,
  EnumView,
  EnumMultiselectView,
  StringView,
  NumberView,
  BooleanView,
  SearchableInfiniteStreamView,
  InfiniteStreamMultiselectView,
  CommonFormState,
  AsyncState,
  BaseEnumContext,
  ListFieldView,
  unit,
  Value,
  EnumReference,
  ValueRecord,
  PredicateValue,
} from "ballerina-core";
// import { PersonFormPredicateContext } from "playground-core";

export const MostUglyValidationDebugView = (props: {
  context: { commonFormState: CommonFormState };
}) =>
  props.context.commonFormState.modifiedByUser &&
  props.context.commonFormState.validation.sync &&
  AsyncState.Operations.isLoading(
    props.context.commonFormState.validation.sync,
  ) ? (
    <>🔄</>
  ) : props.context.commonFormState.modifiedByUser &&
    props.context.commonFormState.validation.sync &&
    AsyncState.Operations.hasValue(
      props.context.commonFormState.validation.sync,
    ) &&
    props.context.commonFormState.validation.sync.value.length > 0 ? (
    <table>
      <tr>
        <td>validation errors</td>
        <td>
          {JSON.stringify(props.context.commonFormState.validation.sync.value)}
        </td>
      </tr>
    </table>
  ) : (
    <></>
  );

export const PersonFieldViews = {
  BooleanView:
    <
      Context extends FormLabel & { showAllErrors: boolean },
      ForeignMutationsExpected,
    >(): BooleanView<Context, ForeignMutationsExpected> =>
    (props) => (
      <>
        {props.context.label && <h3>{props.context.label}</h3>}
        <input
          type="checkbox"
          checked={props.context.value}
          onChange={(e) =>
            props.foreignMutations.setNewValue(e.currentTarget.checked)
          }
        />
        <MostUglyValidationDebugView {...props} />
      </>
    ),
  NumberView:
    <
      Context extends FormLabel & { showAllErrors: boolean },
      ForeignMutationsExpected,
    >(): NumberView<Context, ForeignMutationsExpected> =>
    (props) => (
      <>
        {props.context.label && <h3>{props.context.label}</h3>}
        <input
          type="number"
          value={props.context.value}
          onChange={(e) =>
            props.foreignMutations.setNewValue(
              ~~parseInt(e.currentTarget.value),
            )
          }
        />
        <MostUglyValidationDebugView {...props} />
      </>
    ),
  StringView:
    <
      Context extends FormLabel & { showAllErrors: boolean },
      ForeignMutationsExpected,
    >(): StringView<Context, ForeignMutationsExpected> =>
    (props) => (
      <>
        {props.context.label && <h3>{props.context.label}</h3>}
        <input
          value={props.context.value}
          onChange={(e) =>
            props.foreignMutations.setNewValue(e.currentTarget.value)
          }
        />
        <MostUglyValidationDebugView {...props} />
      </>
    ),
  DateView:
    <
      Context extends FormLabel & { showAllErrors: boolean },
      ForeignMutationsExpected,
    >(): DateView<Context, ForeignMutationsExpected> =>
    (props) => (
      <>
        {props.context.label && <h3>{props.context.label}</h3>}
        <input
          value={props.context.customFormState.possiblyInvalidInput}
          onChange={(e) =>
            props.foreignMutations.setNewValue(e.currentTarget.value)
          }
        />
        <MostUglyValidationDebugView {...props} />
      </>
    ),
  EnumView:
    <
      Context extends FormLabel & BaseEnumContext,
      ForeignMutationsExpected,
    >(): EnumMultiselectView<Context, ForeignMutationsExpected> =>
    (props) => (
      <>
        {props.context.label && <h3>{props.context.label}</h3>}
        {props.context.details && (
          <p>
            <em>{props.context.details}</em>
          </p>
        )}
        {props.context.activeOptions == "loading" ? (
          "loading options"
        ) : (
          <select
            multiple
            value={props.context.selectedIds}
            disabled={props.context.disabled}
            onChange={(e) =>
              props.foreignMutations.setNewValue(
                Array.from(e.currentTarget.options)
                  .filter((_) => _.selected)
                  .map((_) => _.value),
              )
            }
          >
            <>
              {props.context.activeOptions.map((o) => (
                <option value={o.fields.get("Value")! as string}>
                  {o.fields.get("Value") as string}
                </option>
              ))}
            </>
          </select>
        )}
        <MostUglyValidationDebugView {...props} />
      </>
    ),
  EnumMultiselectView:
    <
      Context extends FormLabel & BaseEnumContext,
      ForeignMutationsExpected,
    >(): EnumMultiselectView<Context, ForeignMutationsExpected> =>
    (props) => (
      <>
        {props.context.label && <h3>{props.context.label}</h3>}
        {props.context.details && (
          <p>
            <em>{props.context.details}</em>
          </p>
        )}
        {props.context.activeOptions == "loading" ? (
          "loading options"
        ) : (
          <select
            multiple
            value={props.context.selectedIds}
            disabled={props.context.disabled}
            onChange={(e) =>
              props.foreignMutations.setNewValue(
                Array.from(e.currentTarget.options)
                  .filter((_) => _.selected)
                  .map((_) => _.value),
              )
            }
          >
            <>
              {props.context.activeOptions.map((o) => (
                <option value={o.fields.get("Value")! as string}>
                  {o.fields.get("Value") as string}
                </option>
              ))}
            </>
          </select>
        )}
        <MostUglyValidationDebugView {...props} />
      </>
    ),
  Interests:
    <
      Context extends FormLabel & BaseEnumContext,
      ForeignMutationsExpected,
    >(): EnumMultiselectView<Context, ForeignMutationsExpected> =>
    (props) => (
      <>
        {props.context.label && <h3>{props.context.label}</h3>}
        {props.context.details && (
          <p>
            <em>{props.context.details}</em>
          </p>
        )}
        {props.context.activeOptions == "loading" ? (
          "loading options"
        ) : (
          <select
            multiple
            value={props.context.selectedIds}
            disabled={props.context.disabled}
            onChange={(e) =>
              props.foreignMutations.setNewValue(
                Array.from(e.currentTarget.options)
                  .filter((_) => _.selected)
                  .map((_) => _.value),
              )
            }
          >
            <>
              {props.context.activeOptions.map((o) => (
                <option value={o.fields.get("Value")! as string}>
                  {o.fields.get("Value") as string}
                </option>
              ))}
            </>
          </select>
        )}
        <MostUglyValidationDebugView {...props} />
      </>
    ),
  InfiniteStreamView:
    <
      Context extends FormLabel,
      ForeignMutationsExpected,
    >(): SearchableInfiniteStreamView<Context, ForeignMutationsExpected> =>
    (props) => (
      <>
        {props.context.label && <h3>{props.context.label}</h3>}
        {props.context.tooltip && <p>{props.context.tooltip}</p>}
        {props.context.details && (
          <p>
            <em>{props.context.details}</em>
          </p>
        )}
        <button
          disabled={props.context.disabled}
          onClick={() => props.foreignMutations.toggleOpen()}
        >
          {props.context.value.isSome &&
            ((props.context.value.value as ValueRecord).fields.get(
              "DisplayValue",
            ) as string)}{" "}
          {props.context.customFormState.status == "open" ? "➖" : "➕"}
        </button>
        <button
          disabled={props.context.disabled}
          onClick={() => props.foreignMutations.clearSelection()}
        >
          ❌
        </button>
        {props.context.customFormState.status == "closed" ? (
          <></>
        ) : (
          <>
            <input
              disabled={props.context.disabled}
              value={props.context.customFormState.searchText.value}
              onChange={(e) =>
                props.foreignMutations.setSearchText(e.currentTarget.value)
              }
            />
            <ul>
              {props.context.customFormState.stream.loadedElements
                .valueSeq()
                .map((chunk) =>
                  chunk.data.valueSeq().map((element) => (
                    <li>
                      <button
                        disabled={props.context.disabled}
                        onClick={() =>
                          props.foreignMutations.select(
                            PredicateValue.Default.option(
                              true,
                              ValueRecord.Default.fromJSON(element),
                            ),
                          )
                        }
                      >
                        {element.DisplayValue}{" "}
                        {props.context.value.isSome &&
                        (props.context.value.value as ValueRecord).fields.get(
                          "Id",
                        ) == element.Id
                          ? "✅"
                          : ""}
                      </button>
                    </li>
                  )),
                )}
            </ul>
          </>
        )}
        <MostUglyValidationDebugView {...props} />
        <button
          disabled={props.context.hasMoreValues == false}
          onClick={() => props.foreignMutations.loadMore()}
        >
          ⋯
        </button>
        <button onClick={() => props.foreignMutations.reload()}>🔄</button>
      </>
    ),
  InfiniteStreamMultiselectView:
    <
      Context extends FormLabel & { showAllErrors: boolean },
      ForeignMutationsExpected,
    >(): InfiniteStreamMultiselectView<Context, ForeignMutationsExpected> =>
    (props) => (
      <>
        {props.context.label && <h3>{props.context.label}</h3>}
        {props.context.details && (
          <p>
            <em>{props.context.details}</em>
          </p>
        )}
        <button
          disabled={props.context.disabled}
          onClick={() => props.foreignMutations.toggleOpen()}
        >
          {props.context.value.fields
            .map((_) => (_ as ValueRecord).fields.get("DisplayValue") as string)
            .join(", ")}{" "}
          {props.context.customFormState.status == "open" ? "➖" : "➕"}
        </button>
        <button
          disabled={props.context.disabled}
          onClick={() => props.foreignMutations.clearSelection()}
        >
          ❌
        </button>
        {props.context.customFormState.status == "closed" ? (
          <></>
        ) : (
          <>
            <input
              disabled={props.context.disabled}
              value={props.context.customFormState.searchText.value}
              onChange={(e) =>
                props.foreignMutations.setSearchText(e.currentTarget.value)
              }
            />
            <ul>
              {props.context.availableOptions.map((element) => {
                return (
                  <li>
                    <button
                      disabled={props.context.disabled}
                      onClick={() =>
                        props.foreignMutations.toggleSelection(
                          ValueRecord.Default.fromJSON(element),
                        )
                      }
                    >
                      {element.DisplayValue}{" "}
                      {props.context.value.fields.has(element.Id) ? "✅" : ""}
                    </button>
                  </li>
                );
              })}
            </ul>
          </>
        )}
        <MostUglyValidationDebugView {...props} />
        <button
          disabled={
            props.context.disabled || props.context.hasMoreValues == false
          }
          onClick={() => props.foreignMutations.loadMore()}
        >
          ⋯
        </button>
        <button
          disabled={props.context.disabled}
          onClick={() => props.foreignMutations.reload()}
        >
          🔄
        </button>
      </>
    ),
  ListViews: {
    defaultList:
      <
        ElementFormState,
        Context extends FormLabel,
        ForeignMutationsExpected,
      >(): ListFieldView<ElementFormState, Context, ForeignMutationsExpected> =>
      (props) => (
        <>
          {props.context.label && <h3>{props.context.label}</h3>}
          {props.context.tooltip && <p>{props.context.tooltip}</p>}
          {props.context.details && (
            <p>
              <em>{props.context.details}</em>
            </p>
          )}
          <ul>
            {props.context.value.values.map((_, elementIndex) => {
              return (
                <li
                  style={{
                    display: "flex",
                    flexDirection: "column",
                    gap: "10px",
                  }}
                >
                  {props.embeddedElementTemplate(elementIndex)({
                    ...props,
                    view: unit,
                  })}
                  <div style={{ display: "flex" }}>
                    <button
                      onClick={() =>
                        props.foreignMutations.remove(elementIndex)
                      }
                    >
                      ❌
                    </button>
                    <button
                      onClick={() =>
                        props.foreignMutations.move(
                          elementIndex,
                          elementIndex - 1,
                        )
                      }
                    >
                      ⬆️
                    </button>
                    <button
                      onClick={() =>
                        props.foreignMutations.move(
                          elementIndex,
                          elementIndex + 1,
                        )
                      }
                    >
                      ⬇️
                    </button>
                    <button
                      onClick={() =>
                        props.foreignMutations.duplicate(elementIndex)
                      }
                    >
                      📑
                    </button>
                    <button
                      onClick={() =>
                        props.foreignMutations.insert(elementIndex + 1)
                      }
                    >
                      ➕
                    </button>
                  </div>
                </li>
              );
            })}
          </ul>
          <button
            onClick={() => {
              props.foreignMutations.add(unit);
            }}
          >
            ➕
          </button>
        </>
      ),
  },
};
