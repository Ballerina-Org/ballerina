import {
  ValueOrErrors,
  Unit,
  PredicateValue,
  OneAbstractRendererReadonlyContext,
} from "../../../../../../../../../main";

const getIdFromContext = <
  CustomPresentationContext = Unit,
  ExtraContext = Unit,
>(
  ctx: OneAbstractRendererReadonlyContext<
    CustomPresentationContext,
    ExtraContext
  >,
): ValueOrErrors<string, string | undefined> => {
  if (ctx.value == undefined) {
    return ValueOrErrors.Default.throwOne(undefined);
  }

  /// When initailising, in both stages, inject the id to the get chunk

  const local = ctx.bindings.get("local");
  if (local == undefined) {
    return ValueOrErrors.Default.throwOne(
      `local binding is undefined when intialising one`,
    );
  }

  if (!PredicateValue.Operations.IsRecord(local)) {
    return ValueOrErrors.Default.throwOne(
      `local binding is not a record when intialising one\n... in couroutine for\n...${ctx.domNodeAncestorPath + "[one]"}`,
    );
  }

  if (!local.fields.has("Id")) {
    return ValueOrErrors.Default.throwOne(
      `local binding is missing Id (check casing) when intialising one\n... in couroutine for\n...${ctx.domNodeAncestorPath + "[one]"}`,
    );
  }

  const id = local.fields.get("Id")!; // safe because of above check;
  if (!PredicateValue.Operations.IsString(id)) {
    return ValueOrErrors.Default.throwOne(
      `local Id is not a string when intialising one\n... in couroutine for\n...${ctx.domNodeAncestorPath + "[one]"}`,
    );
  }

  return ValueOrErrors.Default.return(id);
};

export default getIdFromContext;
