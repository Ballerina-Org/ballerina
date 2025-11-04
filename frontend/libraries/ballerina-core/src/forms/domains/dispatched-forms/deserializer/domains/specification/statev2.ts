import Immutable from "immutable";
import {
  CreateLauncher,
  DispatchParsedType,
  DispatchTypeName,
  EditLauncher,
  EntityApi,
  EnumApis,
  LookupApis,
  PassthroughLauncher,
  Renderer,
  SerializedEntityApi,
  SerializedType,
  Specification,
  StreamApis,
  SumNType,
  TableApis,
  Unit,
  Value,
  ValueOrErrors,
} from "../../../../../../../main";
import {
  ConcreteRenderers,
  DispatchApiConverters,
} from "../../../built-ins/state";
import {
  DispatchInjectablesTypes,
  DispatchInjectedPrimitives,
} from "../../../runner/domains/abstract-renderers/injectables/state";

export type SerializedSpecificationV2 = {
  types: object;
  apis: {
    entities?: object;
    enumOptions?: object;
    searchableStreams?: object;
    tables?: object;
    lookups?: object;
  };
  forms: object;
  launchers: object;
};

type RawLauncher = {
  kind: "create" | "edit" | "passthrough";
  form: string;
  api?: string;
  configApi?: string;
  configType?: string;
};

type Launchers = {
  create: Map<string, CreateLauncher>;
  edit: Map<string, EditLauncher>;
  passthrough: Map<string, PassthroughLauncher>;
};

export type ColumnFilters<T> = {
  displayType: DispatchParsedType<T>;
  displayRenderer: Renderer<T>;
  filters: SumNType<T>;
  label?: string;
  tooltip?: string;
  details?: string;
};

export const SpecificationPerformance = {
  Operations: {
    PerformanceDeserialization: <
      T extends DispatchInjectablesTypes<T>,
      Flags = Unit,
      CustomPresentationContext = Unit,
      ExtraContext = Unit,
    >(
      apiConverters: DispatchApiConverters<T>,
      concreteRenderers: ConcreteRenderers<
        T,
        Flags,
        CustomPresentationContext,
        ExtraContext
      >,
      serializedSpecification: SerializedSpecificationV2,
      injectedPrimitives?: DispatchInjectedPrimitives<T>,
    ): ValueOrErrors<Specification<T>, string> => {
      performance.mark("deserialize-specification-v2-start");
      let launchers = {
        create: new Map<string, CreateLauncher>(),
        edit: new Map<string, EditLauncher>(),
        passthrough: new Map<string, PassthroughLauncher>(),
      };
      let forms = new Map<string, Renderer<T>>();
      let types = Immutable.Map<string, DispatchParsedType<T>>();

      const launcherEntries = Object.entries(serializedSpecification.launchers);
      const typeNames = new Set(Object.keys(serializedSpecification.types));

      function parseLauncherForm(name: string): void {
        if (forms.has(name)) {
          return;
        }

        const form = serializedSpecification.forms[name];
        const typeName = form.type;

        // Type Parsing
        const typeResult = DispatchParsedType.Operations.ParseRawType(
          typeName,
          serializedSpecification.types[typeName],
          Immutable.Set(typeNames),
          serializedSpecification.types as Record<string, SerializedType<T>>,
          types.map((type) => ValueOrErrors.Default.return(type)),
          injectedPrimitives,
        );
        if (typeResult.kind == "errors") {
          throw new Error(
            `Error parsing type ${typeName}: ${typeResult.errors.join(", ")}`,
          );
        }
        const [parsedType, alreadyParsedTypes] = typeResult.value;
        types = alreadyParsedTypes
          .map((type) => (type as Value<DispatchParsedType<T>>).value)
          .set(typeName, parsedType);

        // Form Parsing
        const formResult = Renderer.Operations.Deserialize(
          "columns" in form
            ? DispatchParsedType.Default.table(
                DispatchParsedType.Default.lookup(form.type as string),
              )
            : parsedType,
          form,
          concreteRenderers,
          types,
          undefined,
        );
        if (formResult.kind == "errors") {
          throw new Error(
            `Error parsing form ${name}: ${formResult.errors.join(", ")}`,
          );
        }
        forms = forms.set(name, formResult.value);
      }

      for (const [launcherName, launcher] of launcherEntries) {
        if (launcher.kind == "create") {
          parseLauncherForm(launcher.form);
          launchers.create.set(launcherName, launcher);
        } else if (launcher.kind == "edit") {
          parseLauncherForm(launcher.form);
          launchers.edit.set(launcherName, launcher);
        } else if (launcher.kind == "passthrough") {
          parseLauncherForm(launcher.form);
          launchers.passthrough.set(launcherName, launcher);
        }
      }

      const enumsResult = EnumApis.Operations.Deserialize(
        serializedSpecification.apis.enumOptions,
      );
      if (enumsResult.kind == "errors") {
        return ValueOrErrors.Default.throw(enumsResult.errors);
      }
      const enums = enumsResult.value;

      const streamsResult = StreamApis.Operations.Deserialize(
        serializedSpecification.apis.searchableStreams,
      );
      if (streamsResult.kind == "errors") {
        return ValueOrErrors.Default.throw(streamsResult.errors);
      }
      const streams = streamsResult.value;

      const tablesResult = TableApis.Operations.Deserialize(
        concreteRenderers,
        types,
        serializedSpecification.apis.tables,
        injectedPrimitives,
      );
      if (tablesResult.kind == "errors") {
        return ValueOrErrors.Default.throw(tablesResult.errors);
      }
      const tables = tablesResult.value;

      const lookupsResult = LookupApis.Operations.Deserialize(
        serializedSpecification.apis.lookups,
      );
      if (lookupsResult.kind == "errors") {
        return ValueOrErrors.Default.throw(lookupsResult.errors);
      }
      const lookups = lookupsResult.value;

      let entities = Immutable.Map<string, EntityApi>();
      Object.entries(serializedSpecification.apis.entities as object).forEach(
        ([entityApiName, entityApi]: [
          entiyApiName: string,
          entityApi: SerializedEntityApi,
        ]) => {
          entities = entities.set(entityApiName, {
            type: entityApi.type,
            methods: {
              create: entityApi.methods.includes("create"),
              get: entityApi.methods.includes("get"),
              update: entityApi.methods.includes("update"),
              default: entityApi.methods.includes("default"),
            },
          });
        },
      );

      const finalResult = ValueOrErrors.Default.return<Specification<T>, string>({
        types,
        forms: Immutable.Map(forms),
        apis: {
          enums,
          streams,
          entities,
          tables,
          lookups,
        },
        launchers: {
          create: Immutable.Map(launchers.create),
          edit: Immutable.Map(launchers.edit),
          passthrough: Immutable.Map(launchers.passthrough),
        },
      });
      performance.mark("deserialize-specification-v2-done");
      performance.measure("deserialize-specification-v2", "deserialize-specification-v2-start", "deserialize-specification-v2-done");
      console.debug("deserialize-specification-v2", performance.getEntriesByType("measure").find((entry) => entry.name == "deserialize-specification-v2")?.duration);
      return finalResult;
    },
  },
};
