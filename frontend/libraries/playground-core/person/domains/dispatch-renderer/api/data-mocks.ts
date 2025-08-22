import { faker } from "@faker-js/faker";
import {
  PromiseRepo,
  EntityApis,
  unit,
  Guid,
  DispatchInfiniteStreamSources,
  ValueOrErrors,
  DispatchEnumOptionsSources,
  DispatchTableApiSource,
  BasicFun,
  PredicateValue,
  ValueStreamPosition,
  DispatchTableApiSources,
  DispatchOneSource,
  DispatchLookupSources,
  TableAbstractRendererState,
  DispatchTableFiltersAndSorting,
  SumNType,
  DispatchParsedType,
  Value,
  ValueFilter,
} from "ballerina-core";
import { Range, Map, List } from "immutable";
import { City } from "../../address/state";
import { AddressApi } from "../../address/apis/mocks";
import { v4 } from "uuid";
import { PersonApi } from "../../../apis/mocks";

const permissions = ["Create", "Read", "Update", "Delete"];
const colors = ["Red", "Green", "Blue"];
const genders = ["M", "F", "X"];
const interests = ["Soccer", "Hockey", "BoardGames", "HegelianPhilosophy"];

const getActiveUsers: DispatchTableApiSource = {
  get: (id: Guid) => {
    return PromiseRepo.Default.mock(() => ({
      Id: id,
      Name: "Jane",
      Surname: "Doe",
      Birthday: "1990-01-01",
      Email: "jane.doe@example.com",
      SubscribeToNewsletter: true,
      FavoriteColor: {
        Value: { Value: colors[Math.round(Math.random() * 10) % 3] },
        IsSome: true,
      },
      City: {
        IsSome: true,
        Value: {
          ...City.Default(v4(), faker.location.city()),
        },
      },
      StreetNumberAndCity: {
        Item1: faker.location.street(),
        Item2: 100,
        Item3: {
          IsSome: true,
          Value: {
            ...City.Default(v4(), faker.location.city()),
          },
        },
      },
      Friends: {
        From: 0,
        To: 0,
        HasMore: true,
        Values: {},
      },
    }));
  },
  getMany:
    (fromApiRaw: BasicFun<any, ValueOrErrors<PredicateValue, string>>) =>
    (streamParams: Map<string, string>) =>
    ([streamPosition]: [ValueStreamPosition]) => {
      console.debug("streamParams - getMany ActiveUsers", streamParams.toJS());
      return PromiseRepo.Default.mock(() => ({
        Values: {
          [v4()]: {
            Id: v4(),
            Name: faker.person.firstName(),
            Surname: faker.person.lastName(),
            Birthday: faker.date.birthdate().toISOString(),
            Email: faker.internet.email(),
            SubscribeToNewsletter: true,
            FavoriteColor: {
              Value: { Value: colors[Math.round(Math.random() * 10) % 3] },
              IsSome: true,
            },
            City: {
              IsSome: true,
              Value: {
                ...City.Default(v4(), faker.location.city()),
              },
            },
            StreetNumberAndCity: {
              Item1: faker.location.street(),
              Item2: 100,
              Item3: {
                IsSome: true,
                Value: {
                  ...City.Default(v4(), faker.location.city()),
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
          [v4()]: {
            Id: v4(),
            Name: "John",
            Surname: "Doe",
            Birthday: "1990-01-01",
            Email: "john.doe@example.com",
            SubscribeToNewsletter: true,
            FavoriteColor: {
              Value: {},
              IsSome: false,
            },
            City: {
              IsSome: true,
              Value: {
                ...City.Default(v4(), faker.location.city()),
              },
            },
            StreetNumberAndCity: {
              Item1: faker.location.street(),
              Item2: 100,
              Item3: {
                IsSome: true,
                Value: {
                  ...City.Default(v4(), faker.location.city()),
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
        From: 1,
        To: 2,
      })).then((res) => ({
        from: res.From,
        to: res.To,
        hasMoreValues: res.HasMore,
        data: TableAbstractRendererState.Operations.tableValuesToValueRecord(
          res.Values,
          fromApiRaw,
        ),
      }));
    },
  getDefaultFiltersAndSorting:
    (filterTypes: Map<string, SumNType<any>>) =>
    (
      parseFromApiByType: (
        type: DispatchParsedType<any>,
      ) => (raw: any) => ValueOrErrors<PredicateValue, string>,
    ) =>
    () =>
      PromiseRepo.Default.mock(() => ({
        Filters: {
          Name: [
            {
              Discriminator: "case1of2",
              Case1: {
                EqualsTo: "John",
              },
              Case2: null,
            },
          ],
        },
        Sorting: [["Name", "Ascending"]],
      })).then((res) => {
        const parsedFilters: [string, ValueOrErrors<ValueFilter, string>[]][] =
          Object.entries(res.Filters).map(
            ([columnName, filters]) =>
              [
                columnName,
                filters.map((filter) => {
                  const filterType = filterTypes.get(columnName);
                  if (!filterType) {
                    console.error(
                      `filter type not found for column ${columnName}`,
                    );
                    return ValueOrErrors.Default.throwOne<ValueFilter, string>(
                      `filter type not found for column ${columnName}`,
                    );
                  }
                  return parseFromApiByType(filterType)(
                    filter,
                  ) as ValueOrErrors<ValueFilter, string>;
                }),
              ] as const,
          );
        console.debug("parsedFiltersz", parsedFilters);
        const parsedFiltersMap = Map(parsedFilters);
        if (
          parsedFiltersMap.some((filters) =>
            filters.some((f) => f.kind == "errors"),
          )
        ) {
          console.error(
            "error parsing filters to api",
            parsedFiltersMap.filter((filters) =>
              filters.some((f) => f.kind == "errors"),
            ),
          );
          return {
            filters: Map(),
            sorting: Map(),
          };
        }

        // TODO: Deal with this monadically
        const parsedFiltersValues = parsedFiltersMap.map((filters) =>
          List(filters.map((f) => (f as Value<ValueFilter>).value)),
        );

        return {
          filters: parsedFiltersValues,
          sorting: Map<string, "Ascending" | "Descending" | undefined>(
            res.Sorting.map(
              (s) =>
                [s[0], s[1]] as [
                  string,
                  "Ascending" | "Descending" | undefined,
                ],
            ),
          ),
        };
      }),
  // (filterTypes: Map<string, SumNType<any>>) =>
  //   (fromApiRaw: BasicFun<any, ValueOrErrors<PredicateValue, string>>) =>
  //   () =>
  //     PromiseRepo.Default.mock(() => ({
  //       filters: Map(),
  //       sorting: Map(),
  //     })),
};

const getActiveFriends: DispatchTableApiSource = {
  get: (id: Guid) => {
    return PromiseRepo.Default.mock(() => ({
      Id: v4(),
      Name: faker.person.firstName(),
      Surname: faker.person.lastName(),
      Birthday: faker.date.birthdate().toISOString(),
      Email: faker.internet.email(),
      SubscribeToNewsletter: faker.datatype.boolean(),
      FavoriteColor: {
        Value: { Value: colors[Math.round(Math.random() * 10) % 3] },
        IsSome: true,
      },
      Friends: {
        From: 0,
        To: 0,
        HasMore: true,
        Values: {},
      },
    }));
  },
  getMany:
    (fromApiRaw: BasicFun<any, ValueOrErrors<PredicateValue, string>>) =>
    (streamParams: Map<string, string>) =>
    ([streamPosition]: [ValueStreamPosition]) => {
      console.debug(
        "streamParams - getMany ActiveFriends",
        streamParams.toJS(),
      );
      return PromiseRepo.Default.mock(() => ({
        Values: {
          [v4()]: {
            Id: v4(),
            Name: faker.person.firstName(),
            Surname: faker.person.lastName(),
            Birthday: faker.date.birthdate().toISOString(),
            Email: faker.internet.email(),
            SubscribeToNewsletter: faker.datatype.boolean(),
            FavoriteColor: {
              Value: { Value: colors[Math.round(Math.random() * 10) % 3] },
              IsSome: true,
            },
            Friends: {
              From: 0,
              To: 0,
              HasMore: true,
              Values: {},
            },
          },
          [v4()]: {
            Id: v4(),
            Name: faker.person.firstName(),
            Surname: faker.person.lastName(),
            Birthday: faker.date.birthdate().toISOString(),
            Email: faker.internet.email(),
            SubscribeToNewsletter: faker.datatype.boolean(),
            FavoriteColor: {
              Value: { Value: colors[Math.round(Math.random() * 10) % 3] },
              IsSome: true,
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
        From: 1,
        To: 2,
      })).then((res) => ({
        from: res.From,
        to: res.To,
        hasMoreValues: res.HasMore,
        data: TableAbstractRendererState.Operations.tableValuesToValueRecord(
          res.Values,
          fromApiRaw,
        ),
      }));
    },
  getDefaultFiltersAndSorting:
    (filterTypes: Map<string, SumNType<any>>) =>
    (
      parseFromApiByType: (
        type: DispatchParsedType<any>,
      ) => (raw: any) => ValueOrErrors<PredicateValue, string>,
    ) =>
    () =>
      PromiseRepo.Default.mock(() => ({
        filters: Map(),
        sorting: Map(),
      })),
};

const getChildren: DispatchTableApiSource = {
  get: (id: Guid) => {
    return PromiseRepo.Default.mock(() => ({
      Id: id,
      Name: "Jane",
      Surname: "Doe",
      Birthday: "1990-01-01",
      Email: "jane.doe@example.com",
      SubscribeToNewsletter: true,
      FavoriteColor: {
        Value: { Value: colors[Math.round(Math.random() * 10) % 3] },
        IsSome: true,
      },
      City: {
        IsSome: true,
        Value: {
          ...City.Default(v4(), faker.location.city()),
        },
      },
      StreetNumberAndCity: {
        Item1: faker.location.street(),
        Item2: 100,
        Item3: {
          IsSome: true,
          Value: {
            ...City.Default(v4(), faker.location.city()),
          },
        },
      },
      Friends: {
        From: 0,
        To: 0,
        HasMore: true,
        Values: {},
      },
    }));
  },
  getMany:
    (fromApiRaw: BasicFun<any, ValueOrErrors<PredicateValue, string>>) =>
    (streamParams: Map<string, string>) =>
    ([streamPosition]: [ValueStreamPosition]) => {
      console.debug("streamParams - getMany Children", streamParams.toJS());
      return PromiseRepo.Default.mock(() => ({
        Values: {
          [v4()]: {
            Id: v4(),
            Name: faker.person.firstName(),
            Surname: faker.person.lastName(),
            Birthday: faker.date.birthdate().toISOString(),
            Email: faker.internet.email(),
            SubscribeToNewsletter: true,
            FavoriteColor: {
              Value: { Value: colors[Math.round(Math.random() * 10) % 3] },
              IsSome: true,
            },
            City: {
              IsSome: true,
              Value: {
                ...City.Default(v4(), faker.location.city()),
              },
            },
            StreetNumberAndCity: {
              Item1: faker.location.street(),
              Item2: 100,
              Item3: {
                IsSome: true,
                Value: {
                  ...City.Default(v4(), faker.location.city()),
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
          [v4()]: {
            Id: v4(),
            Name: "John",
            Surname: "Doe",
            Birthday: "1990-01-01",
            Email: "john.doe@example.com",
            SubscribeToNewsletter: true,
            FavoriteColor: {
              Value: {},
              IsSome: false,
            },
            City: {
              IsSome: true,
              Value: {
                ...City.Default(v4(), faker.location.city()),
              },
            },
            StreetNumberAndCity: {
              Item1: faker.location.street(),
              Item2: 100,
              Item3: {
                IsSome: true,
                Value: {
                  ...City.Default(v4(), faker.location.city()),
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
        From: 1,
        To: 2,
      })).then((res) => ({
        from: res.From,
        to: res.To,
        hasMoreValues: res.HasMore,
        data: TableAbstractRendererState.Operations.tableValuesToValueRecord(
          res.Values,
          fromApiRaw,
        ),
      }));
    },
  getDefaultFiltersAndSorting:
    (filterTypes: Map<string, SumNType<any>>) =>
    (
      parseFromApiByType: (
        type: DispatchParsedType<any>,
      ) => (raw: any) => ValueOrErrors<PredicateValue, string>,
    ) =>
    () =>
      PromiseRepo.Default.mock(() => ({
        filters: Map(),
        sorting: Map(),
      })),
};

const getFriends: DispatchOneSource = {
  get: (id: Guid) => {
    return PromiseRepo.Default.mock(
      () => ({
        Id: v4(),
        Name: "Tim",
        Surname: "Pool",
        Birthday: "1990-01-01",
        Email: "tim.pool@example.com",
        SubscribeToNewsletter: true,
        FavoriteColor: {
          Value: { Value: colors[Math.round(Math.random() * 10) % 3] },
          IsSome: true,
        },
        Friends: {
          From: 0,
          To: 0,
          HasMore: true,
          Values: {},
        },
      }),
      undefined,
      undefined,
      2,
    );
  },
  getManyUnlinked:
    (fromApiRaw: BasicFun<any, ValueOrErrors<PredicateValue, string>>) =>
    (id: Guid) =>
    (streamParams: Map<string, string>) =>
    ([streamPosition]: [ValueStreamPosition]) => {
      console.debug("streamParams - getMany Friends", streamParams.toJS());
      return PromiseRepo.Default.mock(() => ({
        Values: Range(1, 5)
          .map((_) => ({
            Id: v4(),
            Name: faker.person.firstName(),
            Surname: faker.person.lastName(),
            Birthday: faker.date.birthdate().toISOString(),
            Email: faker.internet.email(),
            SubscribeToNewsletter: faker.datatype.boolean(),
            FavoriteColor: {
              Value: { Value: colors[Math.round(Math.random() * 10) % 3] },
              IsSome: true,
            },
            Friends: {
              From: 0,
              To: 0,
              HasMore: true,
              Values: {},
            },
          }))
          .reduce((acc, curr) => {
            acc[curr.Id] = curr;
            return acc;
          }, {} as any),
        HasMore: false,
        From: 1,
        To: 5,
      })).then((res) => ({
        hasMoreValues: res.HasMore,
        to: res.To,
        from: res.From,
        data: TableAbstractRendererState.Operations.tableValuesToValueRecord(
          res.Values,
          fromApiRaw,
        ),
      }));
    },
};

const eagerEditableOne: DispatchOneSource = {
  get: undefined,
  getManyUnlinked:
    (fromApiRaw: BasicFun<any, ValueOrErrors<PredicateValue, string>>) =>
    (id: Guid) =>
    (streamParams: Map<string, string>) =>
    ([streamPosition]: [ValueStreamPosition]) => {
      return PromiseRepo.Default.mock(() => ({
        Values: Range(1, 5)
          .map((_) => ({
            Id: v4(),
            Name: faker.person.firstName(),
            Surname: faker.person.lastName(),
            Birthday: faker.date.birthdate().toISOString(),
            Email: faker.internet.email(),
            SubscribeToNewsletter: faker.datatype.boolean(),
            FavoriteColor: {
              Value: { Value: colors[Math.round(Math.random() * 10) % 3] },
              IsSome: true,
            },
            Friends: {
              From: 0,
              To: 0,
              HasMore: true,
              Values: {},
            },
          }))
          .reduce((acc, curr) => {
            acc[curr.Id] = curr;
            return acc;
          }, {} as any),
        HasMore: false,
        From: 1,
        To: 5,
      })).then((res) => ({
        hasMoreValues: res.HasMore,
        to: res.To,
        from: res.From,
        data: TableAbstractRendererState.Operations.tableValuesToValueRecord(
          res.Values,
          fromApiRaw,
        ),
      }));
    },
};

const lazyReadonlyOne: DispatchOneSource = {
  get: (id: Guid) => {
    return PromiseRepo.Default.mock(
      () => ({
        Id: v4(),
        Name: "Tim",
        Surname: "Pool",
        Birthday: "1990-01-01",
        Email: "tim.pool@example.com",
        SubscribeToNewsletter: true,
        FavoriteColor: {
          Value: { Value: colors[Math.round(Math.random() * 10) % 3] },
          IsSome: true,
        },
        Friends: {
          From: 0,
          To: 0,
          HasMore: true,
          Values: {},
        },
      }),
      undefined,
      undefined,
      1,
    );
  },
  getManyUnlinked: undefined,
};

const lookupSources: DispatchLookupSources = (typeName: string) =>
  typeName == "User"
    ? ValueOrErrors.Default.return({
        one: (apiName: string) =>
          apiName == "BestFriendApi"
            ? ValueOrErrors.Default.return(getFriends)
            : apiName == "EagerEditableOneApi"
              ? ValueOrErrors.Default.return(eagerEditableOne)
              : apiName == "LazyReadonlyOneApi"
                ? ValueOrErrors.Default.return(lazyReadonlyOne)
                : ValueOrErrors.Default.throwOne(
                    `can't find api ${apiName} when getting lookup api sources`,
                  ),
      })
    : ValueOrErrors.Default.throwOne(
        `can't find type ${typeName} when getting lookup api source`,
      );

const tableApiSources: DispatchTableApiSources = (streamName: string) =>
  streamName == "ActiveUsersApi"
    ? ValueOrErrors.Default.return(getActiveUsers)
    : streamName == "ActiveFriendsApi"
      ? ValueOrErrors.Default.return(getActiveFriends)
      : streamName == "ChildrenApi"
        ? ValueOrErrors.Default.return(getChildren)
        : ValueOrErrors.Default.throwOne(`Cannot find table API ${streamName}`);

const streamApis: DispatchInfiniteStreamSources = (streamName: string) =>
  streamName == "departments"
    ? ValueOrErrors.Default.return(PersonApi.getDepartments())
    : streamName == "cities"
      ? ValueOrErrors.Default.return(AddressApi.getCities())
      : ValueOrErrors.Default.throwOne(`Cannot find stream API ${streamName}`);

const enumApis: DispatchEnumOptionsSources = (enumName: string) =>
  enumName == "colors"
    ? ValueOrErrors.Default.return(() =>
        PromiseRepo.Default.mock(
          () => colors.map((_) => ({ Value: _ })),
          undefined,
          1,
          0,
        ),
      )
    : enumName == "permissions"
      ? ValueOrErrors.Default.return(() =>
          PromiseRepo.Default.mock(
            () => permissions.map((_) => ({ Value: _ })),
            undefined,
            1,
            0,
          ),
        )
      : enumName == "genders"
        ? ValueOrErrors.Default.return(() =>
            PromiseRepo.Default.mock(
              () => genders.map((_) => ({ Value: _ })),
              undefined,
              1,
              0,
            ),
          )
        : enumName == "interests"
          ? ValueOrErrors.Default.return(() =>
              PromiseRepo.Default.mock(
                () => interests.map((_) => ({ Value: _ })),
                undefined,
                1,
                0,
              ),
            )
          : enumName == "addressesFields"
            ? ValueOrErrors.Default.return(() =>
                PromiseRepo.Default.mock(
                  () =>
                    [
                      "AddressesByCity",
                      "Departments",
                      "SchoolAddress",
                      "MainAddress",
                      "AddressesAndAddressesWithLabel",
                      "AddressesWithColorLabel",
                      "AddressesBy",
                      "Permissions",
                      "CityByDepartment",
                      "Holidays",
                      "FriendsAddresses",
                    ].map((_) => ({ Value: _ })),
                  undefined,
                  1,
                  0,
                ),
              )
            : ValueOrErrors.Default.throwOne(
                `Cannot find enum API ${enumName}`,
              );
const entityApis: EntityApis = {
  create: (apiName: string) =>
    apiName == "person"
      ? (e: any) =>
          PromiseRepo.Default.mock(() => {
            console.log(
              "person create api post body",
              JSON.stringify(e, undefined, 2),
            );
            return unit;
          })
      : (e: any) => {
          alert(`Cannot find entity API ${apiName} for 'create'`);
          return Promise.reject();
        },
  get: (apiName: string) => {
    switch (apiName) {
      case "person":
        return (id: Guid) => {
          console.log(`get person ${id}`);
          return Promise.resolve({
            Id: v4(),
            BestFriend: {
              isRight: false,
              right: {},
            },
            EagerEditableOne: {
              isRight: true,
              right: {
                Id: v4(),
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
                Id: v4(),
                Name: "John",
                Surname: "Doe",
                Birthday: "1990-01-01",
                Email: "john.doe@example.com",
                SubscribeToNewsletter: true,
              },
            },
            Friends: {
              From: 0,
              To: 0,
              HasMore: true,
              Values: {},
            },
            Children: {
              From: 0,
              To: 0,
              HasMore: true,
              Values: {},
            },
            Job: {
              // Discriminator: "Designer",
              // Designer: {
              //   Name: "Designer",
              //   Salary: Math.floor(Math.random() * 100000),
              //   DesignTool: "Figma",
              //   Certifications: ["cool stuff"],
              // },
              // Discriminator: "Manager",
              // Manager: {
              //   ReadOnly: "I'm a Manager!",
              // },
              Discriminator: "Owners",
              Owners: [faker.person.firstName(), faker.person.firstName()],
            },
            Category: {
              kind: ["child", "adult", "senior"][
                Math.round(Math.random() * 10) % 3
              ],
              extraSpecial: false,
            },
            FullName: {
              Item1: faker.person.firstName(),
              Item2: faker.person.lastName(),
            },
            Birthday: new Date(
              Date.now() - Math.random() * 1000 * 60 * 60 * 24 * 365 * 45,
            ).toISOString(),
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
            SubscribeToNewsletter: Math.random() > 0.5,
            FavoriteColor: {
              Value: { Value: colors[Math.round(Math.random() * 10) % 3] },
              IsSome: true,
            },
            Gender: {
              IsRight: true,
              Value: { IsSome: true, Value: { Value: "M" } },
            },
            Dependants: [
              {
                Key: "Steve",
                Value: {
                  kind: ["child", "adult", "senior"][
                    Math.round(Math.random() * 10) % 3
                  ],
                  extraSpecial: false,
                },
              },
              {
                Key: "Alice",
                Value: {
                  kind: ["child", "adult", "senior"][
                    Math.round(Math.random() * 10) % 3
                  ],
                  extraSpecial: false,
                },
              },
            ],
            FriendsByCategory: [],
            Relatives: [
              {
                kind: ["child", "adult", "senior"][
                  Math.round(Math.random() * 10) % 3
                ],
                extraSpecial: false,
              },
              {
                kind: ["child", "adult", "senior"][
                  Math.round(Math.random() * 10) % 3
                ],
                extraSpecial: false,
              },
              {
                kind: ["child", "adult", "senior"][
                  Math.round(Math.random() * 10) % 3
                ],
                extraSpecial: false,
              },
            ],
            Interests: [{ Value: interests[1] }, { Value: interests[2] }],
            Departments: [
              { Id: v4(), DisplayValue: "Department 1" },
              { Id: v4(), DisplayValue: "Department 2" },
            ],
            Emails: ["john@doe.it", "johnthedon@doe.com"],
            SchoolAddress: {
              StreetNumberAndCity: {
                Item1: faker.location.street(),
                Item2: 100,
                Item3: {
                  IsSome: true,
                  Value: {
                    ...City.Default(v4(), faker.location.city()),
                  },
                },
              },
            },
            MainAddress: {
              IsRight: true,
              Value: {
                Item1: {
                  StreetNumberAndCity: {
                    Item1: faker.location.street(),
                    Item2: Math.floor(Math.random() * 500),
                    Item3:
                      Math.random() > 0.5
                        ? { IsSome: false, Value: { Value: "" } }
                        : {
                            IsSome: true,
                            Value: {
                              ...City.Default(v4(), faker.location.city()),
                            },
                          },
                  },
                },
                Item2: {
                  LandArea: {
                    X: Math.floor(Math.random() * 100),
                    Y: Math.floor(Math.random() * 100),
                  },
                },
              },
            },
            AddressesAndAddressesWithLabel: {
              Item1: [
                {
                  StreetNumberAndCity: {
                    Item1: faker.location.street(),
                    Item2: Math.floor(Math.random() * 500),
                    Item3:
                      Math.random() > 0.5
                        ? { IsSome: false, Value: { Value: "" } }
                        : {
                            IsSome: true,
                            Value: {
                              ...City.Default(v4(), faker.location.city()),
                            },
                          },
                  },
                },
                {
                  StreetNumberAndCity: {
                    Item1: faker.location.street(),
                    Item2: Math.floor(Math.random() * 500),
                    Item3:
                      Math.random() > 0.5
                        ? { IsSome: false, Value: { Value: "" } }
                        : {
                            IsSome: true,
                            Value: {
                              ...City.Default(v4(), faker.location.city()),
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
                      Item1: faker.location.street(),
                      Item2: Math.floor(Math.random() * 500),
                      Item3:
                        Math.random() > 0.5
                          ? { IsSome: false, Value: { Value: "" } }
                          : {
                              IsSome: true,
                              Value: {
                                ...City.Default(v4(), faker.location.city()),
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
                  Value: { ...City.Default(v4(), faker.location.city()) },
                },
                Value: {
                  StreetNumberAndCity: {
                    Item1: faker.location.street(),
                    Item2: Math.floor(Math.random() * 500),
                    Item3:
                      Math.random() > 0.5
                        ? { IsSome: false, Value: { Value: "" } }
                        : {
                            IsSome: true,
                            Value: {
                              ...City.Default(v4(), faker.location.city()),
                            },
                          },
                  },
                },
              },
              {
                Key: {
                  IsSome: true,
                  Value: { ...City.Default(v4(), faker.location.city()) },
                },
                Value: {
                  StreetNumberAndCity: {
                    Item1: faker.location.street(),
                    Item2: Math.floor(Math.random() * 500),
                    Item3:
                      Math.random() > 0.5
                        ? { IsSome: false, Value: { Value: "" } }
                        : {
                            IsSome: true,
                            Value: {
                              ...City.Default(v4(), faker.location.city()),
                            },
                          },
                  },
                },
              },
            ],
            ImportantDate: {
              IsRight: true,
              Value: new Date(
                Date.now() - Math.random() * 1000 * 60 * 60 * 24 * 365 * 45,
              ).toISOString(),
            },
            CutOffDates: [
              {
                IsRight: true,
                Value: new Date(
                  Date.now() - Math.random() * 1000 * 60 * 60 * 24 * 365 * 45,
                ).toISOString(),
              },
              {
                IsRight: true,
                Value: new Date(
                  Date.now() - Math.random() * 1000 * 60 * 60 * 24 * 365 * 45,
                ).toISOString(),
              },
            ],
            AddressesBy: {
              IsRight: true,
              Value: [
                {
                  Key: "home",
                  Value: {
                    StreetNumberAndCity: {
                      Item1: faker.location.street(),
                      Item2: Math.floor(Math.random() * 500),
                      Item3:
                        Math.random() > 0.5
                          ? { IsSome: false, Value: { Value: "" } }
                          : {
                              IsSome: true,
                              Value: {
                                ...City.Default(v4(), faker.location.city()),
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
                  Value: { Value: colors[Math.round(Math.random() * 10) % 3] },
                },
                Value: {
                  StreetNumberAndCity: {
                    Item1: faker.location.street(),
                    Item2: Math.floor(Math.random() * 500),
                    Item3:
                      Math.random() > 0.5
                        ? { IsSome: false, Value: { Value: "" } }
                        : {
                            IsSome: true,
                            Value: {
                              ...City.Default(v4(), faker.location.city()),
                            },
                          },
                  },
                },
              },
              {
                Key: {
                  IsSome: true,
                  Value: { Value: colors[Math.round(Math.random() * 10) % 3] },
                },
                Value: {
                  StreetNumberAndCity: {
                    Item1: faker.location.street(),
                    Item2: Math.floor(Math.random() * 500),
                    Item3:
                      Math.random() > 0.5
                        ? { IsSome: false, Value: { Value: "" } }
                        : {
                            IsSome: true,
                            Value: {
                              ...City.Default(v4(), faker.location.city()),
                            },
                          },
                  },
                },
              },
            ],
            Permissions: [],
            CityByDepartment: [],
            ShoeColours: [{ Value: "Red" }],
            FriendsBirthdays: [],
            Holidays: [],
            FriendsAddresses: [
              {
                Key: `${faker.person.firstName()} ${faker.person.lastName()}`,
                Value: [
                  {
                    StreetNumberAndCity: {
                      Item1: faker.location.street(),
                      Item2: Math.floor(Math.random() * 500),
                      Item3: {
                        IsSome: true,
                        Value: {
                          ...City.Default(v4(), faker.location.city()),
                        },
                      },
                    },
                  },
                  {
                    StreetNumberAndCity: {
                      Item1: faker.location.street(),
                      Item2: Math.floor(Math.random() * 500),
                      Item3: {
                        IsSome: true,
                        Value: {
                          ...City.Default(v4(), faker.location.city()),
                        },
                      },
                    },
                  },
                ],
              },
              {
                Key: `${faker.person.firstName()} ${faker.person.lastName()}`,
                Value: [
                  {
                    StreetNumberAndCity: {
                      Item1: faker.location.street(),
                      Item2: Math.floor(Math.random() * 500),
                      Item3: {
                        IsSome: true,
                        Value: {
                          ...City.Default(v4(), faker.location.city()),
                        },
                      },
                    },
                  },
                  {
                    StreetNumberAndCity: {
                      Item1: faker.location.street(),
                      Item2: Math.floor(Math.random() * 500),
                      Item3: {
                        IsSome: true,
                        Value: {
                          ...City.Default(v4(), faker.location.city()),
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
          });
        };
      case "person-config":
        return (_: Guid) => {
          return Promise.resolve({
            IsAdmin: false,
            ActiveAddressFields: [
              { Value: "Departments" },
              { Value: "SchoolAddress" },
              { Value: "MainAddress" },
              { Value: "AddressesAndAddressesWithLabel" },
              { Value: "AddressesWithColorLabel" },
              { Value: "AddressesBy" },
              { Value: "Permissions" },
              { Value: "CityByDepartment" },
              { Value: "Holidays" },
              { Value: "AddressesByCity" },
              { Value: "FriendsAddresses" },
            ],
            ERP: {
              Discriminator: "ERPSAP",
              ERPSAP: {
                Value: {
                  Discriminator: "SAPS2",
                  SAPS2: {
                    S2OnlyField: true,
                  },
                },
              },
            },
          });
        };
      default:
        return (id: Guid) => {
          alert(`Cannot find entity API ${apiName} for 'get' ${id}`);
          return Promise.reject();
        };
    }
  },
  update: (apiName: string) => (_id: Guid, _e: any) => {
    console.log(`update ${apiName} ${_id}`, JSON.stringify(_e, undefined, 2));
    switch (apiName) {
      case "person":
        return PromiseRepo.Default.mock(() => []);
      case "errorPerson":
        return Promise.reject({
          status: 400,
          message: "Bad Request: Invalid person data provided",
        });
      default:
        alert(`Cannot find entity API ${apiName} for 'update'`);
        return Promise.resolve([]);
    }
  },
  default: (apiName: string) =>
    apiName == "person"
      ? (_) =>
          PromiseRepo.Default.mock(() => {
            return {
              Friends: {
                From: 0,
                To: 0,
                HasMore: false,
                Values: {},
              },
              Category: {
                kind: "adult",
                extraSpecial: false,
              },
              FullName: {
                Item1: "",
                Item2: "",
              },
              Birthday: "01/01/2000",
              SubscribeToNewsletter: false,
              FavoriteColor: { Value: { Value: null }, IsSome: false },
              Gender: {
                IsRight: false,
                Value: {},
              },
              Dependants: [],
              FriendsByCategory: [],
              Relatives: [],
              Interests: [],
              Departments: [],
              Emails: [],
              SchoolAddress: {
                StreetNumberAndCity: {
                  Item1: faker.location.street(),
                  Item2: Math.floor(Math.random() * 500),
                  Item3:
                    Math.random() > 0.5
                      ? { IsSome: false, Value: { Value: "" } }
                      : {
                          IsSome: true,
                          Value: {
                            ...City.Default(v4(), faker.location.city()),
                          },
                        },
                },
              },
              MainAddress: {
                IsRight: false,
                Value: "",
              },
              AddressesAndAddressesWithLabel: {
                Item1: [],
                Item2: [],
              },
              AddressesByCity: [],
              ImportantDate: {
                IsRight: false,
                Value: "",
              },
              CutOffDates: [],
              AddressesBy: {
                IsRight: false,
                Value: [],
              },
              AddressesWithColorLabel: [],
              Permissions: [],
              CityByDepartment: [],
              ShoeColours: [],
              FriendsBirthdays: [],
              Holidays: [],
              FriendsAddresses: [],
            };
          })
      : (_) => {
          alert(`Cannot find entity API ${apiName} for 'default'`);
          return Promise.reject();
        },
};

export const DispatchPersonFromConfigApis = {
  streamApis,
  enumApis,
  entityApis,
  tableApiSources,
  lookupSources,
};
//
