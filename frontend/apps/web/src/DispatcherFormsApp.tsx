import { useEffect, useState } from "react";
import "./App.css";
import {
  unit,
  PromiseRepo,
  Sum,
  PredicateValue,
  replaceWith,
  DeltaTransfer,
  ValueOrErrors,
  DispatchFormsParserTemplate,
  DispatchFormsParserState,
  DispatchFormRunnerTemplate,
  DispatchDeltaTransfer,
  DispatchDeltaCustom,
  DispatchDelta,
  DispatchSpecificationDeserializationResult,
  DispatchFormRunnerState,
  DispatchParsedType,
  IdWrapperProps,
  ErrorRendererProps,
  DispatchInjectedPrimitive,
  DispatchOnChange,
  AggregatedFlags,
} from "ballerina-core";
import { Set, OrderedMap } from "immutable";
import { DispatchPersonFromConfigApis } from "playground-core";
// import SPEC from "../../../../backend/apps/automatic-tests/input-forms/simple-union-example-lookups.json";
import SPEC from "../public/SampleSpecs/dispatch-person-config.json";
import {
  DispatchPersonContainerFormView,
  DispatchPersonLookupTypeRenderer,
  DispatchPersonNestedContainerFormView,
} from "./domains/dispatched-passthrough-form/views/wrappers";
import {
  CategoryAbstractRenderer,
  DispatchCategoryState,
  DispatchPassthroughFormInjectedTypes,
} from "./domains/dispatched-passthrough-form/injected-forms/category";
import {
  DispatchPassthroughFormConcreteRenderers,
  DispatchPassthroughFormCustomPresentationContext,
  DispatchPassthroughFormFlags,
  DispatchPassthroughFormExtraContext,
} from "./domains/dispatched-passthrough-form/views/concrete-renderers";
import { DispatchFieldTypeConverters } from "./domains/dispatched-passthrough-form/apis/field-converters";
import { v4 } from "uuid";
import { DispatchCreateFormLauncherState } from "ballerina-core/src/forms/domains/dispatched-forms/runner/domains/kind/create/state";

const ShowFormsParsingErrors = (
  parsedFormsConfig: DispatchSpecificationDeserializationResult<
    DispatchPassthroughFormInjectedTypes,
    DispatchPassthroughFormFlags,
    DispatchPassthroughFormCustomPresentationContext,
    DispatchPassthroughFormExtraContext
  >,
) => (
  <div style={{ display: "flex", border: "red" }}>
    {parsedFormsConfig.kind == "errors" &&
      JSON.stringify(parsedFormsConfig.errors)}
  </div>
);

const IdWrapper = ({ domNodeId, children }: IdWrapperProps) => (
  <div id={domNodeId}>{children}</div>
);

const ErrorRenderer = ({ message }: ErrorRendererProps) => (
  <div
    style={{
      display: "flex",
      border: "2px dashed red",
      maxWidth: "200px",
      maxHeight: "50px",
      overflowY: "scroll",
      padding: "10px",
    }}
  >
    <pre
      style={{
        whiteSpace: "pre-wrap",
        maxWidth: "200px",
        lineBreak: "anywhere",
      }}
    >{`Error: ${message}`}</pre>
  </div>
);

const InstantiedPersonFormsParserTemplate = DispatchFormsParserTemplate<
  DispatchPassthroughFormInjectedTypes,
  DispatchPassthroughFormFlags,
  DispatchPassthroughFormCustomPresentationContext,
  DispatchPassthroughFormExtraContext
>();

const InstantiedPersonDispatchFormRunnerTemplate = DispatchFormRunnerTemplate<
  DispatchPassthroughFormInjectedTypes,
  DispatchPassthroughFormFlags,
  DispatchPassthroughFormCustomPresentationContext,
  DispatchPassthroughFormExtraContext
>();

export const DispatcherFormsApp = (props: {}) => {
  const [specificationDeserializer, setSpecificationDeserializer] = useState(
    DispatchFormsParserState<
      DispatchPassthroughFormInjectedTypes,
      DispatchPassthroughFormFlags,
      DispatchPassthroughFormCustomPresentationContext,
      DispatchPassthroughFormExtraContext
    >().Default(),
  );

  const [personPassthroughFormState, setPersonPassthroughFormState] = useState(
    DispatchFormRunnerState<
      DispatchPassthroughFormInjectedTypes,
      DispatchPassthroughFormFlags,
      DispatchPassthroughFormCustomPresentationContext,
      DispatchPassthroughFormExtraContext
    >().Default.passthrough(),
  );
  const [personConfigState, setPersonConfigState] = useState(
    DispatchFormRunnerState<
      DispatchPassthroughFormInjectedTypes,
      DispatchPassthroughFormFlags,
      DispatchPassthroughFormCustomPresentationContext,
      DispatchPassthroughFormExtraContext
    >().Default.passthrough(),
  );

  const [personCreateState, setPersonCreateState] = useState(
    DispatchFormRunnerState<
      DispatchPassthroughFormInjectedTypes,
      DispatchPassthroughFormFlags,
      DispatchPassthroughFormCustomPresentationContext,
      DispatchPassthroughFormExtraContext
    >().Default.create(),
  );

  const [personEntity, setPersonEntity] = useState<
    Sum<ValueOrErrors<PredicateValue, string>, "not initialized">
  >(Sum.Default.right("not initialized"));
  const [config, setConfig] = useState<
    Sum<ValueOrErrors<PredicateValue, string>, "not initialized">
  >(Sum.Default.right("not initialized"));
  const [createConfig, setCreateConfig] = useState<
    Sum<ValueOrErrors<PredicateValue, string>, "not initialized">
  >(Sum.Default.right("not initialized"));

  // TODO replace with delta transfer
  const [entityPath, setEntityPath] = useState<any>(null);

  const [remoteEntityVersionIdentifier, setRemoteEntityVersionIdentifier] =
    useState(v4());
  const [
    remoteConfigEntityVersionIdentifier,
    setRemoteConfigEntityVersionIdentifier,
  ] = useState(v4());

  const parseCustomDelta =
    <T,>(
      toRawObject: (
        value: PredicateValue,
        type: DispatchParsedType<T>,
        state: any,
      ) => ValueOrErrors<any, string>,
      fromDelta: (
        delta: DispatchDelta<DispatchPassthroughFormFlags>,
      ) => ValueOrErrors<DeltaTransfer<T>, string>,
    ) =>
    (
      deltaCustom: DispatchDeltaCustom<DispatchPassthroughFormFlags>,
    ): ValueOrErrors<
      [T, string, AggregatedFlags<DispatchPassthroughFormFlags>],
      string
    > => {
      if (deltaCustom.value.kind == "CategoryReplace") {
        return toRawObject(
          deltaCustom.value.replace,
          deltaCustom.value.type,
          deltaCustom.value.state,
        ).Then((value) => {
          return ValueOrErrors.Default.return([
            {
              kind: "CategoryReplace",
              replace: value,
            },
            "[CategoryReplace]",
            deltaCustom.flags ? [[deltaCustom.flags, "[CategoryReplace]"]] : [],
          ] as [T, string, AggregatedFlags<DispatchPassthroughFormFlags>]);
        });
      }
      return ValueOrErrors.Default.throwOne(
        `Unsupported delta kind: ${deltaCustom.value.kind}`,
      );
    };

  const onPersonConfigChange: DispatchOnChange<
    PredicateValue,
    DispatchPassthroughFormFlags
  > = (updater, delta) => {
    if (config.kind == "r" || config.value.kind == "errors") {
      return;
    }

    const newConfig =
      updater.kind == "r"
        ? updater.value(config.value.value)
        : config.value.value;
    console.log("patching config", newConfig);
    setConfig(
      replaceWith(Sum.Default.left(ValueOrErrors.Default.return(newConfig))),
    );
    if (
      specificationDeserializer.deserializedSpecification.sync.kind ==
        "loaded" &&
      specificationDeserializer.deserializedSpecification.sync.value.kind ==
        "value"
    ) {
      const toApiRawParser =
        specificationDeserializer.deserializedSpecification.sync.value.value.launchers.passthrough.get(
          "person-config",
        )!.parseValueToApi;
      setEntityPath(
        DispatchDeltaTransfer.Default.FromDelta(
          toApiRawParser as any, //TODO - fix type issue if worth it
          parseCustomDelta,
        )(delta),
      );
      setRemoteConfigEntityVersionIdentifier(v4());
    }
  };

  const onPersonEntityChange: DispatchOnChange<
    PredicateValue,
    DispatchPassthroughFormFlags
  > = (updater, delta) => {
    setPersonEntity((prev) => {
      if (prev.kind == "r" || prev.value.kind == "errors") {
        return prev;
      }
      const newEntity =
        updater.kind == "r"
          ? updater.value(prev.value.value)
          : prev.value.value;
      return replaceWith(
        Sum.Default.left<
          ValueOrErrors<PredicateValue, string>,
          "not initialized"
        >(ValueOrErrors.Default.return(newEntity)),
      )(prev);
    });
    if (
      specificationDeserializer.deserializedSpecification.sync.kind ==
        "loaded" &&
      specificationDeserializer.deserializedSpecification.sync.value.kind ==
        "value"
    ) {
      const toApiRawParser =
        specificationDeserializer.deserializedSpecification.sync.value.value.launchers.passthrough.get(
          "person-transparent",
        )!.parseValueToApi;
      const dispatchDeltaTransfer = DispatchDeltaTransfer.Default.FromDelta(
        toApiRawParser as any, //TODO - fix type issue if worth it
        parseCustomDelta,
      )(delta);

      console.debug("dispatchDeltaTransfer", dispatchDeltaTransfer);

      setEntityPath(dispatchDeltaTransfer);
      setRemoteEntityVersionIdentifier(v4());
    }
  };

  // const onAddressFieldsChange = (
  //   updater: Updater<any>,
  //   delta: DispatchDelta,
  // ): void => {
  //   if (globalConfiguration.kind == "r" || globalConfiguration.value.kind == "errors") {
  //     return;
  //   }
  //   const newEntity = updater(globalConfiguration.value);
  //   console.log("patching entity", newEntity);
  //   setGlobalConfiguration(replaceWith(Sum.Default.left(newEntity)));
  //   if (
  //     specificationDeserializer.deserializedSpecification.sync.kind ==
  //       "loaded" &&
  //     specificationDeserializer.deserializedSpecification.sync.value.kind ==
  //       "value"
  //   ) {
  //     const toApiRawParser =
  //       specificationDeserializer.deserializedSpecification.sync.value.value.launchers.passthrough.get(
  //         "addresses-config",
  //       )!.parseEntityFromApi;
  //     setEntityPath(
  //       DispatchDeltaTransfer.Default.FromDelta(
  //         toApiRawParser,
  //         parseCustomDelta,
  //       )(delta),
  //     );
  //   }
  // };

  const p1 = {
    Id: "8f0c943b-2435-42b6-b3b2-de3b2dc68bd3",
    BestFriend: {
      isRight: false,
      right: {},
    },
    EagerEditableOne: {
      isRight: true,
      right: {
        Id: "be02f0c8-6781-4ff6-81bd-aa40a2bb2b2b",
        Name: "John",
        Surname: "Doe",
        Birthday: "1990-01-01",
        Email: "john.doe@example.com",
        SubscribeToNewsletter: true,
      },
    },
    LazyReadonlyOne: {
      isRight: false,
      right: {},
    },
    EagerReadonlyOne: {
      isRight: true,
      right: {
        Id: "91744769-b807-4cc4-a6e3-ffeaec69eeb6",
        Name: "John",
        Surname: "Doe",
        Birthday: "1990-01-01",
        Email: "john.doe@example.com",
        SubscribeToNewsletter: true,
      },
    },
    Friends: {
      Values: {
        "aff339ea-5fc3-4ca8-bb24-b3b3e411f865": {
          Id: "9df0e65a-9f5b-4e49-b640-202576ce7f89",
          Name: "Edyth",
          Surname: "Abbott",
          Birthday: "1996-05-04T17:52:25.872Z",
          Email: "Santino91@gmail.com",
          SubscribeToNewsletter: false,
          FavoriteColor: {
            Value: {
              Value: "Red",
            },
            IsSome: true,
          },
          City: {
            IsSome: true,
            Value: {
              Id: "82f71154-9c75-4c34-bf18-609197098d3c",
              DisplayValue: "Lake Cyril",
            },
          },
          StreetNumberAndCity: {
            Item1: "Wolf Manors",
            Item2: 100,
            Item3: {
              IsSome: true,
              Value: {
                Id: "3f0b760a-57f3-412a-b1ca-5b19b945abb8",
                DisplayValue: "Hirtheworth",
              },
            },
          },
          Friends: {
            From: 0,
            To: 0,
            HasMore: true,
            Values: {},
          },
        },
        "98bb04b3-031b-4758-9fd1-32fe6a0d37da": {
          Id: "3c6e63a2-0168-468c-be61-b732967e7d09",
          Name: "Cornelius",
          Surname: "Wisoky",
          Birthday: "1960-05-08T20:10:33.230Z",
          Email: "Everett.Yost@gmail.com",
          SubscribeToNewsletter: true,
          FavoriteColor: {
            Value: {
              Value: "Red",
            },
            IsSome: true,
          },
          City: {
            IsSome: true,
            Value: {
              Id: "25cb0c51-fdf8-45fd-8257-5cd7929c6147",
              DisplayValue: "Lake Kimside",
            },
          },
          StreetNumberAndCity: {
            Item1: "Maggie Junctions",
            Item2: 100,
            Item3: {
              IsSome: true,
              Value: {
                Id: "cd0651e2-5cc2-4a5a-aa4c-9a2da4efc792",
                DisplayValue: "East Ilatown",
              },
            },
          },
          Friends: {
            From: 0,
            To: 0,
            HasMore: true,
            Values: {},
          },
        },
      },
      HasMore: true,
      From: 0,
      To: 0,
    },
    Children: {
      From: 0,
      To: 0,
      HasMore: true,
      Values: {},
    },
    Job: {
      Discriminator: "Owners",
      Owners: ["Ines", "Brandon"],
    },
    Category: {
      kind: "adult",
      extraSpecial: false,
    },
    FullName: {
      Item1: "Sylvia",
      Item2: "Weissnat",
    },
    Birthday: "2024-12-06T10:56:10.104Z",
    SuperSecretNumber: {
      ReadOnly: 123123,
    },
    MoreSecretNumbers: [
      {
        ReadOnly: 15651,
      },
      {
        ReadOnly: 15651,
      },
      {
        ReadOnly: 15651,
      },
    ],
    SubscribeToNewsletter: true,
    FavoriteColor: {
      Value: {
        Value: "Green",
      },
      IsSome: true,
    },
    Gender: {
      IsRight: true,
      Value: {
        IsSome: true,
        Value: {
          Value: "M",
        },
      },
    },
    Dependants: [
      {
        Key: "Steve",
        Value: {
          kind: "senior",
          extraSpecial: false,
        },
      },
      {
        Key: "Alice",
        Value: {
          kind: "adult",
          extraSpecial: false,
        },
      },
    ],
    FriendsByCategory: [],
    Relatives: [
      {
        kind: "adult",
        extraSpecial: false,
      },
      {
        kind: "adult",
        extraSpecial: false,
      },
      {
        kind: "adult",
        extraSpecial: false,
      },
    ],
    Interests: [
      {
        Value: "Hockey",
      },
      {
        Value: "BoardGames",
      },
    ],
    Departments: [
      {
        Id: "04c8d240-3b89-480c-be78-94b6415bb7b2",
        DisplayValue: "Department 1",
      },
      {
        Id: "f82de5d8-d622-4ae2-9647-0d0eda78533c",
        DisplayValue: "Department 2",
      },
    ],
    Emails: ["john@doe.it", "johnthedon@doe.com"],
    SchoolAddress: {
      StreetNumberAndCity: {
        Item1: "Renner Brooks",
        Item2: 100,
        Item3: {
          IsSome: true,
          Value: {
            Id: "873fa8db-b2fc-4ce5-84ed-0b23433f79a0",
            DisplayValue: "Kingland",
          },
        },
      },
    },
    MainAddress: {
      IsRight: true,
      Value: {
        Item1: {
          StreetNumberAndCity: {
            Item1: "Turcotte Centers",
            Item2: 361,
            Item3: {
              IsSome: false,
              Value: {
                Value: "",
              },
            },
          },
        },
        Item2: {
          LandArea: {
            X: 34,
            Y: 36,
          },
        },
      },
    },
    AddressesAndAddressesWithLabel: {
      Item1: [
        {
          StreetNumberAndCity: {
            Item1: "N Walnut Street",
            Item2: 378,
            Item3: {
              IsSome: true,
              Value: {
                Id: "f7fc0928-79ec-4ecc-9f80-c2ed7f4882a7",
                DisplayValue: "Fort Kali",
              },
            },
          },
        },
        {
          StreetNumberAndCity: {
            Item1: "Ethyl Parkway",
            Item2: 174,
            Item3: {
              IsSome: false,
              Value: {
                Value: "",
              },
            },
          },
        },
      ],
      Item2: [
        {
          Key: "my house",
          Value: {
            StreetNumberAndCity: {
              Item1: "Chelsey Pines",
              Item2: 26,
              Item3: {
                IsSome: true,
                Value: {
                  Id: "8eb15a90-ff83-4955-a849-f1298309c6e9",
                  DisplayValue: "Fort Neal",
                },
              },
            },
          },
        },
      ],
    },
    AddressesByCity: [
      {
        Key: {
          IsSome: true,
          Value: {
            Id: "bf15e5e9-e6b8-482a-9846-094ed2c33c8f",
            DisplayValue: "East Orange",
          },
        },
        Value: {
          StreetNumberAndCity: {
            Item1: "Russell Street",
            Item2: 95,
            Item3: {
              IsSome: false,
              Value: {
                Value: "",
              },
            },
          },
        },
      },
      {
        Key: {
          IsSome: true,
          Value: {
            Id: "06041b55-54a6-4b7b-a88d-18ff081af7e9",
            DisplayValue: "Port Emersonstead",
          },
        },
        Value: {
          StreetNumberAndCity: {
            Item1: "Main Street N",
            Item2: 134,
            Item3: {
              IsSome: false,
              Value: {
                Value: "",
              },
            },
          },
        },
      },
    ],
    ImportantDate: {
      IsRight: true,
      Value: "2011-05-08T16:55:15.416Z",
    },
    CutOffDates: [
      {
        IsRight: true,
        Value: "2006-06-24T04:45:41.758Z",
      },
      {
        IsRight: true,
        Value: "2012-12-01T23:34:38.806Z",
      },
    ],
    AddressesBy: {
      IsRight: true,
      Value: [
        {
          Key: "home",
          Value: {
            StreetNumberAndCity: {
              Item1: "West Street",
              Item2: 15,
              Item3: {
                IsSome: false,
                Value: {
                  Value: "",
                },
              },
            },
          },
        },
      ],
    },
    AddressesWithColorLabel: [
      {
        Key: {
          IsSome: true,
          Value: {
            Value: "Green",
          },
        },
        Value: {
          StreetNumberAndCity: {
            Item1: "E 11th Street",
            Item2: 410,
            Item3: {
              IsSome: false,
              Value: {
                Value: "",
              },
            },
          },
        },
      },
      {
        Key: {
          IsSome: true,
          Value: {
            Value: "Green",
          },
        },
        Value: {
          StreetNumberAndCity: {
            Item1: "Giuseppe Ford",
            Item2: 17,
            Item3: {
              IsSome: true,
              Value: {
                Id: "0a3fe2b9-9191-4304-8046-afdc93eaae09",
                DisplayValue: "Fort Trevionfort",
              },
            },
          },
        },
      },
    ],
    Permissions: [],
    CityByDepartment: [],
    ShoeColours: [
      {
        Value: "Red",
      },
    ],
    FriendsBirthdays: [],
    Holidays: [],
    FriendsAddresses: [
      {
        Key: "Chad Gottlieb",
        Value: [
          {
            StreetNumberAndCity: {
              Item1: "W Lake Street",
              Item2: 366,
              Item3: {
                IsSome: true,
                Value: {
                  Id: "fdd6ab97-5a3e-490f-9d8a-e66eb6963fb3",
                  DisplayValue: "Port Leonestad",
                },
              },
            },
          },
          {
            StreetNumberAndCity: {
              Item1: "Nienow Valley",
              Item2: 150,
              Item3: {
                IsSome: true,
                Value: {
                  Id: "97ded63c-7542-4823-93b5-93c70fe8886a",
                  DisplayValue: "Somerville",
                },
              },
            },
          },
        ],
      },
      {
        Key: "Nona Boehm",
        Value: [
          {
            StreetNumberAndCity: {
              Item1: "Daniel Pass",
              Item2: 34,
              Item3: {
                IsSome: true,
                Value: {
                  Id: "42be1f98-f75b-474e-aa54-b51fdad4356d",
                  DisplayValue: "Noblesville",
                },
              },
            },
          },
          {
            StreetNumberAndCity: {
              Item1: "Franklin Avenue",
              Item2: 474,
              Item3: {
                IsSome: true,
                Value: {
                  Id: "e5fe031a-af98-49ee-93b9-4a7e5f4f26e0",
                  DisplayValue: "Mesquite",
                },
              },
            },
          },
        ],
      },
    ],
    IncomeTaxBrackets: [
      [
        {
          Amount: 100000,
          TaxRate: 0.1,
          TaxAmount: 10000,
        },
      ],
    ],
  };
  const p2 = {
    Id: "8f0c943b-2435-42b6-b3b2-de3b2dc68bd3",
    BestFriend: {
      isRight: false,
      right: {},
    },
    EagerEditableOne: {
      isRight: true,
      right: {
        Id: "be02f0c8-6781-4ff6-81bd-aa40a2bb2b2b",
        Name: "John",
        Surname: "Doe",
        Birthday: "1990-01-01",
        Email: "john.doe@example.com",
        SubscribeToNewsletter: true,
      },
    },
    LazyReadonlyOne: {
      isRight: false,
      right: {},
    },
    EagerReadonlyOne: {
      isRight: true,
      right: {
        Id: "91744769-b807-4cc4-a6e3-ffeaec69eeb6",
        Name: "John",
        Surname: "Doe",
        Birthday: "1990-01-01",
        Email: "john.doe@example.co",
        SubscribeToNewsletter: true,
      },
    },
    Friends: {
      Values: {
        "aff339ea-5fc3-4ca8-bb24-b3b3e411f865": {
          Id: "9df0e65a-9f5b-4e49-b640-202576ce7f89",
          Name: "Edyth",
          Surname: "Abbott",
          Birthday: "1996-05-04T17:52:25.872Z",
          Email: "Santino91@gmail.com",
          SubscribeToNewsletter: false,
          FavoriteColor: {
            Value: {
              Value: "Red",
            },
            IsSome: true,
          },
          City: {
            IsSome: true,
            Value: {
              Id: "82f71154-9c75-4c34-bf18-609197098d3c",
              DisplayValue: "Lake Cyril",
            },
          },
          StreetNumberAndCity: {
            Item1: "Wolf Manors",
            Item2: 100,
            Item3: {
              IsSome: true,
              Value: {
                Id: "3f0b760a-57f3-412a-b1ca-5b19b945abb8",
                DisplayValue: "Hirtheworth",
              },
            },
          },
          Friends: {
            From: 0,
            To: 0,
            HasMore: true,
            Values: {},
          },
        },
        "98bb04b3-031b-4758-9fd1-32fe6a0d37da": {
          Id: "3c6e63a2-0168-468c-be61-b732967e7d09",
          Name: "Cornelius",
          Surname: "Wisoky",
          Birthday: "1960-05-08T20:10:33.230Z",
          Email: "Everett.Yost@gmail.com",
          SubscribeToNewsletter: true,
          FavoriteColor: {
            Value: {
              Value: "Red",
            },
            IsSome: true,
          },
          City: {
            IsSome: true,
            Value: {
              Id: "25cb0c51-fdf8-45fd-8257-5cd7929c6147",
              DisplayValue: "Lake Kimside",
            },
          },
          StreetNumberAndCity: {
            Item1: "Maggie Junctions",
            Item2: 100,
            Item3: {
              IsSome: true,
              Value: {
                Id: "cd0651e2-5cc2-4a5a-aa4c-9a2da4efc792",
                DisplayValue: "East Ilatown",
              },
            },
          },
          Friends: {
            From: 0,
            To: 0,
            HasMore: true,
            Values: {},
          },
        },
      },
      HasMore: true,
      From: 0,
      To: 0,
    },
    Children: {
      From: 0,
      To: 0,
      HasMore: true,
      Values: {},
    },
    Job: {
      Discriminator: "Owners",
      Owners: ["Ines", "Brandon"],
    },
    Category: {
      kind: "adult",
      extraSpecial: false,
    },
    FullName: {
      Item1: "Jo",
      Item2: "Weissnat",
    },
    Birthday: "2024-12-06T10:56:10.104Z",
    SuperSecretNumber: {
      ReadOnly: 123123,
    },
    MoreSecretNumbers: [
      {
        ReadOnly: 15651,
      },
      {
        ReadOnly: 15651,
      },
      {
        ReadOnly: 15651,
      },
    ],
    SubscribeToNewsletter: true,
    FavoriteColor: {
      Value: {
        Value: "Green",
      },
      IsSome: true,
    },
    Gender: {
      IsRight: true,
      Value: {
        IsSome: true,
        Value: {
          Value: "M",
        },
      },
    },
    Dependants: [
      {
        Key: "Steve",
        Value: {
          kind: "senior",
          extraSpecial: false,
        },
      },
      {
        Key: "Alice",
        Value: {
          kind: "adult",
          extraSpecial: false,
        },
      },
    ],
    FriendsByCategory: [],
    Relatives: [
      {
        kind: "adult",
        extraSpecial: false,
      },
      {
        kind: "adult",
        extraSpecial: false,
      },
      {
        kind: "adult",
        extraSpecial: false,
      },
    ],
    Interests: [
      {
        Value: "Hockey",
      },
      {
        Value: "BoardGames",
      },
    ],
    Departments: [
      {
        Id: "04c8d240-3b89-480c-be78-94b6415bb7b2",
        DisplayValue: "Department 1",
      },
      {
        Id: "f82de5d8-d622-4ae2-9647-0d0eda78533c",
        DisplayValue: "Department 2",
      },
    ],
    Emails: ["john@doe.it", "johnthedon@doe.com"],
    SchoolAddress: {
      StreetNumberAndCity: {
        Item1: "Renner Brooks",
        Item2: 100,
        Item3: {
          IsSome: true,
          Value: {
            Id: "873fa8db-b2fc-4ce5-84ed-0b23433f79a0",
            DisplayValue: "Kingland",
          },
        },
      },
    },
    MainAddress: {
      IsRight: true,
      Value: {
        Item1: {
          StreetNumberAndCity: {
            Item1: "Turcotte Centers",
            Item2: 361,
            Item3: {
              IsSome: false,
              Value: {
                Value: "",
              },
            },
          },
        },
        Item2: {
          LandArea: {
            X: 34,
            Y: 36,
          },
        },
      },
    },
    AddressesAndAddressesWithLabel: {
      Item1: [
        {
          StreetNumberAndCity: {
            Item1: "N Walnut Street",
            Item2: 378,
            Item3: {
              IsSome: true,
              Value: {
                Id: "f7fc0928-79ec-4ecc-9f80-c2ed7f4882a7",
                DisplayValue: "Fort Kali",
              },
            },
          },
        },
        {
          StreetNumberAndCity: {
            Item1: "Ethyl Parkway",
            Item2: 174,
            Item3: {
              IsSome: false,
              Value: {
                Value: "",
              },
            },
          },
        },
      ],
      Item2: [
        {
          Key: "my house",
          Value: {
            StreetNumberAndCity: {
              Item1: "Chelsey Pines",
              Item2: 26,
              Item3: {
                IsSome: true,
                Value: {
                  Id: "8eb15a90-ff83-4955-a849-f1298309c6e9",
                  DisplayValue: "Fort Neal",
                },
              },
            },
          },
        },
      ],
    },
    AddressesByCity: [
      {
        Key: {
          IsSome: true,
          Value: {
            Id: "bf15e5e9-e6b8-482a-9846-094ed2c33c8f",
            DisplayValue: "East Orange",
          },
        },
        Value: {
          StreetNumberAndCity: {
            Item1: "Russell Street",
            Item2: 95,
            Item3: {
              IsSome: false,
              Value: {
                Value: "",
              },
            },
          },
        },
      },
      {
        Key: {
          IsSome: true,
          Value: {
            Id: "06041b55-54a6-4b7b-a88d-18ff081af7e9",
            DisplayValue: "Port Emersonstead",
          },
        },
        Value: {
          StreetNumberAndCity: {
            Item1: "Main Street N",
            Item2: 134,
            Item3: {
              IsSome: false,
              Value: {
                Value: "",
              },
            },
          },
        },
      },
    ],
    ImportantDate: {
      IsRight: true,
      Value: "2011-05-08T16:55:15.416Z",
    },
    CutOffDates: [
      {
        IsRight: true,
        Value: "2006-06-24T04:45:41.758Z",
      },
      {
        IsRight: true,
        Value: "2012-12-01T23:34:38.806Z",
      },
    ],
    AddressesBy: {
      IsRight: true,
      Value: [
        {
          Key: "home",
          Value: {
            StreetNumberAndCity: {
              Item1: "West Street",
              Item2: 15,
              Item3: {
                IsSome: false,
                Value: {
                  Value: "",
                },
              },
            },
          },
        },
      ],
    },
    AddressesWithColorLabel: [
      {
        Key: {
          IsSome: true,
          Value: {
            Value: "Green",
          },
        },
        Value: {
          StreetNumberAndCity: {
            Item1: "E 11th Street",
            Item2: 410,
            Item3: {
              IsSome: false,
              Value: {
                Value: "",
              },
            },
          },
        },
      },
      {
        Key: {
          IsSome: true,
          Value: {
            Value: "Green",
          },
        },
        Value: {
          StreetNumberAndCity: {
            Item1: "Giuseppe Ford",
            Item2: 17,
            Item3: {
              IsSome: true,
              Value: {
                Id: "0a3fe2b9-9191-4304-8046-afdc93eaae09",
                DisplayValue: "Fort Trevionfort",
              },
            },
          },
        },
      },
    ],
    Permissions: [],
    CityByDepartment: [],
    ShoeColours: [
      {
        Value: "Red",
      },
    ],
    FriendsBirthdays: [],
    Holidays: [],
    FriendsAddresses: [
      {
        Key: "Chad Gottlieb",
        Value: [
          {
            StreetNumberAndCity: {
              Item1: "W Lake Street",
              Item2: 366,
              Item3: {
                IsSome: true,
                Value: {
                  Id: "fdd6ab97-5a3e-490f-9d8a-e66eb6963fb3",
                  DisplayValue: "Port Leonestad",
                },
              },
            },
          },
          {
            StreetNumberAndCity: {
              Item1: "Nienow Valley",
              Item2: 150,
              Item3: {
                IsSome: true,
                Value: {
                  Id: "97ded63c-7542-4823-93b5-93c70fe8886a",
                  DisplayValue: "Somerville",
                },
              },
            },
          },
        ],
      },
      {
        Key: "Nona Boehm",
        Value: [
          {
            StreetNumberAndCity: {
              Item1: "Daniel Pass",
              Item2: 34,
              Item3: {
                IsSome: true,
                Value: {
                  Id: "42be1f98-f75b-474e-aa54-b51fdad4356d",
                  DisplayValue: "Noblesville",
                },
              },
            },
          },
          {
            StreetNumberAndCity: {
              Item1: "Franklin Avenue",
              Item2: 474,
              Item3: {
                IsSome: true,
                Value: {
                  Id: "e5fe031a-af98-49ee-93b9-4a7e5f4f26e0",
                  DisplayValue: "Mesquite",
                },
              },
            },
          },
        ],
      },
    ],
    IncomeTaxBrackets: [
      [
        {
          Amount: 100000,
          TaxRate: 0.1,
          TaxAmount: 10000,
        },
      ],
    ],
  };
  useEffect(() => {
    DispatchPersonFromConfigApis.entityApis
      .get("person")("")
      .then((raw) => {
        if (
          specificationDeserializer.deserializedSpecification.sync.kind ==
            "loaded" &&
          specificationDeserializer.deserializedSpecification.sync.value.kind ==
            "value"
        ) {
          const parsed =
            specificationDeserializer.deserializedSpecification.sync.value.value.launchers.passthrough
              .get("person-transparent")!
              .parseEntityFromApi(raw);
          const parsedWithRegistry =
            specificationDeserializer.deserializedSpecification.sync.value.value.launchers.passthrough
              .get("person-transparent")!
              .parseFromApiByTypeWithRegistry(OrderedMap(), "person", p1);
          if (parsedWithRegistry.kind == "errors") {
            console.debug("parsedWithRegistry failed");
            console.debug(
              "parsedWithRegistry errors",
              parsedWithRegistry.errors,
            );
            console.error("parsed entity errors", parsedWithRegistry.errors);
          } else {
            console.debug("parsedWithRegistry success");
            console.debug(
              "parsedWithRegistry",
              parsedWithRegistry.value[2].toJS(),
            );
            const parsedWithRegistry2 =
              specificationDeserializer.deserializedSpecification.sync.value.value.launchers.passthrough
                .get("person-transparent")!
                .parseFromApiByTypeWithRegistry(
                  parsedWithRegistry.value[2],
                  "person",
                  p2,
                );
            if (parsedWithRegistry2.kind == "errors") {
              console.debug("parsedWithRegistry2 failed");
              console.debug(
                "parsedWithRegistry2 errors",
                parsedWithRegistry2.errors,
              );
              console.error("parsed entity errors", parsedWithRegistry2.errors);
            } else {
              console.debug("parsedWithRegistry2 success");
              console.debug(
                "parsedWithRegistry2",
                parsedWithRegistry2.value[2].toJS(),
              );
            }
          }
          if (parsed.kind == "errors") {
            console.error("parsed entity errors", parsed.errors);
          } else {
            console.debug("parsed success");
            setPersonEntity(Sum.Default.left(parsed));
          }
        }
      });
    DispatchPersonFromConfigApis.entityApis
      .get("person-config")("")
      .then((raw) => {
        if (
          specificationDeserializer.deserializedSpecification.sync.kind ==
            "loaded" &&
          specificationDeserializer.deserializedSpecification.sync.value.kind ==
            "value"
        ) {
          const parsed =
            specificationDeserializer.deserializedSpecification.sync.value.value.launchers.passthrough
              .get("person-config")!
              .parseEntityFromApi(raw);
          if (parsed.kind == "errors") {
            console.error("parsed person config errors", parsed.errors);
          } else {
            setConfig(Sum.Default.left(parsed));
          }
        }
      });
  }, [specificationDeserializer.deserializedSpecification.sync.kind]);

  // console.debug(
  //   "personPassthroughFormState",
  //   JSON.stringify(
  //     personPassthroughFormState?.formState?.fieldStates?.get("dependants"),
  //     null,
  //     2,
  //   ),
  // );
  // console.debug(
  //   "personPassthroughFormState common",
  //   JSON.stringify(
  //     personPassthroughFormState?.formState?.commonFormState,
  //     null,
  //     2,
  //   ),
  // );
  // console.debug("personConfig", JSON.stringify(config, null, 2));

  if (
    specificationDeserializer.deserializedSpecification.sync.kind == "loaded" &&
    specificationDeserializer.deserializedSpecification.sync.value.kind ==
      "errors"
  ) {
    return (
      <ol>
        <pre>
          {specificationDeserializer.deserializedSpecification.sync.value.errors.map(
            (_: string, index: number) => (
              <li key={index}>{_}</li>
            ),
          )}
        </pre>
      </ol>
    );
  }

  // console.debug("personEntity", JSON.stringify(personEntity, null, 2));

  return (
    <div className="App">
      <h1>Ballerina 🩰</h1>
      <div className="card">
        <table>
          <tbody>
            <tr>
              <td>
                <InstantiedPersonFormsParserTemplate
                  context={{
                    ...specificationDeserializer,
                    lookupTypeRenderer: DispatchPersonLookupTypeRenderer,
                    defaultRecordConcreteRenderer:
                      DispatchPersonContainerFormView,
                    fieldTypeConverters: DispatchFieldTypeConverters,
                    defaultNestedRecordConcreteRenderer:
                      DispatchPersonNestedContainerFormView,
                    concreteRenderers: DispatchPassthroughFormConcreteRenderers,
                    getFormsConfig: () => PromiseRepo.Default.mock(() => SPEC),
                    IdWrapper,
                    ErrorRenderer,
                    injectedPrimitives: [
                      DispatchInjectedPrimitive.Default(
                        "injectedCategory",
                        CategoryAbstractRenderer,
                        {
                          kind: "custom",
                          value: {
                            kind: "adult",
                            extraSpecial: false,
                          },
                        },
                        DispatchCategoryState.Default(),
                      ),
                    ],
                  }}
                  setState={setSpecificationDeserializer}
                  view={unit}
                  foreignMutations={unit}
                />
                <h3> Dispatcher Passthrough form</h3>

                <h4>Config</h4>
                <div style={{ border: "2px dashed lightblue" }}>
                  <InstantiedPersonDispatchFormRunnerTemplate
                    context={{
                      ...specificationDeserializer,
                      ...personConfigState,
                      launcherRef: {
                        name: "person-config",
                        kind: "passthrough",
                        entity: config,
                        config: Sum.Default.left(
                          ValueOrErrors.Default.return(
                            PredicateValue.Default.record(OrderedMap()),
                          ),
                        ),
                        onEntityChange: onPersonConfigChange,
                        apiSources: {
                          infiniteStreamSources:
                            DispatchPersonFromConfigApis.streamApis,
                          enumOptionsSources:
                            DispatchPersonFromConfigApis.enumApis,
                          tableApiSources:
                            DispatchPersonFromConfigApis.tableApiSources,
                          lookupSources:
                            DispatchPersonFromConfigApis.lookupSources,
                        },
                      },
                      remoteEntityVersionIdentifier:
                        remoteConfigEntityVersionIdentifier,
                      showFormParsingErrors: ShowFormsParsingErrors,
                      extraContext: {
                        flags: Set(["BC", "X"]),
                      },
                      globallyDisabled: false,
                      globallyReadOnly: false,
                    }}
                    setState={setPersonConfigState}
                    view={unit}
                    foreignMutations={unit}
                  />
                </div>
                <h3>Person</h3>
                {/* {entityPath && entityPath.kind == "value" && (
                  <pre
                    style={{
                      display: "inline-block",
                      verticalAlign: "top",
                      textAlign: "left",
                    }}
                  >
                    {JSON.stringify(entityPath.value, null, 2)}
                  </pre>
                )} */}
                {entityPath && entityPath.kind == "errors" && (
                  <pre>
                    DeltaErrors: {JSON.stringify(entityPath.errors, null, 2)}
                  </pre>
                )}
                <InstantiedPersonDispatchFormRunnerTemplate
                  context={{
                    ...specificationDeserializer,
                    ...personPassthroughFormState,
                    launcherRef: {
                      name: "person-transparent",
                      kind: "passthrough",
                      entity: personEntity,
                      config,
                      onEntityChange: onPersonEntityChange,
                      apiSources: {
                        infiniteStreamSources:
                          DispatchPersonFromConfigApis.streamApis,
                        enumOptionsSources:
                          DispatchPersonFromConfigApis.enumApis,
                        tableApiSources:
                          DispatchPersonFromConfigApis.tableApiSources,
                        lookupSources:
                          DispatchPersonFromConfigApis.lookupSources,
                      },
                    },
                    remoteEntityVersionIdentifier,
                    showFormParsingErrors: ShowFormsParsingErrors,
                    extraContext: {
                      flags: Set(["BC", "X"]),
                    },
                    globallyDisabled: false,
                    globallyReadOnly: false,
                  }}
                  setState={setPersonPassthroughFormState}
                  view={unit}
                  foreignMutations={unit}
                />

                <h3>Create Person</h3>
                <InstantiedPersonDispatchFormRunnerTemplate
                  context={{
                    ...specificationDeserializer,
                    ...personCreateState,
                    launcherRef: {
                      name: "create-person",
                      kind: "create",
                      apiSources: {
                        infiniteStreamSources:
                          DispatchPersonFromConfigApis.streamApis,
                        enumOptionsSources:
                          DispatchPersonFromConfigApis.enumApis,
                        entityApis: DispatchPersonFromConfigApis.entityApis,
                        tableApiSources:
                          DispatchPersonFromConfigApis.tableApiSources,
                        lookupSources:
                          DispatchPersonFromConfigApis.lookupSources,
                      },
                      // config: {
                      //   source: "api",
                      //   getGlobalConfig: () =>
                      //     DispatchPersonFromConfigApis.entityApis.get(
                      //       "person-config",
                      //     )(""),
                      // },
                      config: {
                        source: "entity",
                        value: config,
                      },
                    },
                    remoteEntityVersionIdentifier,
                    showFormParsingErrors: ShowFormsParsingErrors,
                    extraContext: {
                      flags: Set(["BC", "X"]),
                    },
                    globallyDisabled: false,
                    globallyReadOnly: false,
                  }}
                  setState={setPersonCreateState}
                  view={unit}
                  foreignMutations={unit}
                />
                <button
                  onClick={() => {
                    setPersonCreateState((_) =>
                      _.innerFormState.kind == "create"
                        ? {
                            ..._,
                            ...DispatchFormRunnerState<
                              DispatchPassthroughFormInjectedTypes,
                              DispatchPassthroughFormFlags,
                              DispatchPassthroughFormCustomPresentationContext,
                              DispatchPassthroughFormExtraContext
                            >().Updaters.Template.create(
                              DispatchCreateFormLauncherState<
                                DispatchPassthroughFormInjectedTypes,
                                DispatchPassthroughFormFlags,
                                DispatchPassthroughFormCustomPresentationContext,
                                DispatchPassthroughFormExtraContext
                              >().Updaters.Template.submit(),
                            )(_),
                          }
                        : _,
                    );
                  }}
                >
                  Create Person
                </button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  );
};
