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
} from "ballerina-core";
import { Range, Map } from "immutable";
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
};

const getFriends: DispatchOneSource = {
  get: (id: Guid) => {
    return PromiseRepo.Default.mock(() => ({
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
    }));
  },
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

const lookupSources: DispatchLookupSources = (typeName: string) =>
  typeName == "User"
    ? ValueOrErrors.Default.return({
        one: (apiName: string) =>
          apiName == "BestFriendApi"
            ? ValueOrErrors.Default.return(getFriends)
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
            // Job: {
            //   Discriminator: "Developer",
            //   Developer: {
            //     Name: "Developer",
            //     Salary: Math.floor(Math.random() * 100000),
            //     Language: "TypeScript",
            //   },
            // },
            BestFriend: {
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
              Discriminator: "Designer",
              Designer: {
                Name: "Designer",
                Salary: Math.floor(Math.random() * 100000),
                DesignTool: "Figma",
                Certifications: ["cool stuff"],
              },
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
