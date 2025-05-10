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
  AbstractTableRendererState,
  DispatchTableApiSources,
  DispatchOneSource,
  DispatchLookupSources,
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
      InactiveUsers: {
        Values: Range(1, 2)
          .map((_) => ({
            Id: v4(),
            Name: faker.person.firstName(),
            Surname: faker.person.lastName(),
            Birthday: faker.date.birthdate().toISOString(),
            Email: faker.internet.email(),
            SubscribeToNewsletter: faker.datatype.boolean(),
          }))
          .reduce((acc, curr) => {
            acc[curr.Id] = curr;
            return acc;
          }, {} as any),
        HasMore: true,
        From: 0,
        To: 10,
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
            Name: "Jane",
            Surname: "Doe",
            Birthday: "1990-01-01",
            Email: "jane.doe@example.com",
            SubscribeToNewsletter: true,
            InactiveUsers: {
              Values: Range(1, 11)
                .map((_) => ({
                  Id: v4(),
                  Name: faker.person.firstName(),
                  Surname: faker.person.lastName(),
                  Birthday: faker.date.birthdate().toISOString(),
                  Email: faker.internet.email(),
                  SubscribeToNewsletter: faker.datatype.boolean(),
                  InactiveUsers: {
                    Values: Range(1, 2)
                      .map((_) => ({
                        Id: v4(),
                        Name: faker.person.firstName(),
                        Surname: faker.person.lastName(),
                        Birthday: faker.date.birthdate().toISOString(),
                        Email: faker.internet.email(),
                        SubscribeToNewsletter: faker.datatype.boolean(),
                      }))
                      .reduce((acc, curr) => {
                        acc[curr.Id] = curr;
                        return acc;
                      }, {} as any),
                    HasMore: true,
                    From: 0,
                    To: 10,
                  },
                }))
                .reduce((acc, curr) => {
                  acc[curr.Id] = curr;
                  return acc;
                }, {} as any),
              HasMore: true,
              From: 0,
              To: 10,
            },
          },
          [v4()]: {
            Id: v4(),
            Name: "John",
            Surname: "Doe",
            Birthday: "1990-01-01",
            Email: "john.doe@example.com",
            SubscribeToNewsletter: true,
            InactiveUsers: {
              Values: Range(1, 2)
                .map((_) => ({
                  Id: v4(),
                  Name: faker.person.firstName(),
                  Surname: faker.person.lastName(),
                  Birthday: faker.date.birthdate().toISOString(),
                  Email: faker.internet.email(),
                  SubscribeToNewsletter: faker.datatype.boolean(),
                }))
                .reduce((acc, curr) => {
                  acc[curr.Id] = curr;
                  return acc;
                }, {} as any),
              HasMore: true,
              From: 0,
              To: 10,
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
        data: AbstractTableRendererState.Operations.tableValuesToValueRecord(
          res.Values,
          fromApiRaw,
        ),
      }));
    },
};

const getAdminLookup: DispatchOneSource = {
  get: (id: Guid) => {
    return PromiseRepo.Default.mock(() => ({
      Id: v4(),
      Name: "Admin",
      Surname: "User",
      Birthday: "1990-01-01",
      Email: "admin.user@example.com",
      SubscribeToNewsletter: true,
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
        data: AbstractTableRendererState.Operations.tableValuesToValueRecord(
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
          apiName == "AdminApi"
            ? ValueOrErrors.Default.return(getAdminLookup)
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
              "addressesByCity",
              "departments",
              "schoolAddress",
              "mainAddress",
              "addressesAndAddressesWithLabel",
              "addressesWithColorLabel",
              "addressesBy",
              "permissions",
              "cityByDepartment",
              "holidays",
              "friendsAddresses",
            ].map((_) => ({ Value: _ })),
          undefined,
          1,
          0,
        ),
      )
    : ValueOrErrors.Default.throwOne(`Cannot find enum API ${enumName}`);
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
            job: {
              Discriminator: "Developer",
              Developer: {
                name: "Developer",
                salary: Math.floor(Math.random() * 100000),
                language: "TypeScript",
              },
            },
            category: {
              kind: ["child", "adult", "senior"][
                Math.round(Math.random() * 10) % 3
              ],
              extraSpecial: false,
            },
            fullName: {
              Item1: faker.person.firstName(),
              Item2: faker.person.lastName(),
            },
            birthday: new Date(
              Date.now() - Math.random() * 1000 * 60 * 60 * 24 * 365 * 45,
            ).toISOString(),
            subscribeToNewsletter: Math.random() > 0.5,
            favoriteColor: {
              Value: { Value: colors[Math.round(Math.random() * 10) % 3] },
              IsSome: true,
            },
            gender: {
              IsRight: true,
              Value: { IsSome: true, Value: { Value: "M" } },
            },
            dependants: [
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
            friendsByCategory: [],
            relatives: [
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
            interests: [{ Value: interests[1] }, { Value: interests[2] }],
            departments: [
              { Id: v4(), DisplayValue: "Department 1" },
              { Id: v4(), DisplayValue: "Department 2" },
            ],
            emails: ["john@doe.it", "johnthedon@doe.com"],
            schoolAddress: {
              streetNumberAndCity: {
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
            mainAddress: {
              IsRight: true,
              Value: {
                Item1: {
                  streetNumberAndCity: {
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
                  landArea: {
                    x: Math.floor(Math.random() * 100),
                    y: Math.floor(Math.random() * 100),
                  },
                },
              },
            },
            addressesAndAddressesWithLabel: {
              Item1: [
                {
                  streetNumberAndCity: {
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
                  streetNumberAndCity: {
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
                    streetNumberAndCity: {
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
            addressesByCity: [
              {
                Key: {
                  IsSome: true,
                  Value: { ...City.Default(v4(), faker.location.city()) },
                },
                Value: {
                  streetNumberAndCity: {
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
                  streetNumberAndCity: {
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
            importantDate: {
              IsRight: false,
              Value: {},
            },
            cutOffDates: [
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
            addressesBy: {
              IsRight: true,
              Value: [
                {
                  Key: "home",
                  Value: {
                    streetNumberAndCity: {
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
            addressesWithColorLabel: [
              {
                Key: {
                  IsSome: true,
                  Value: { Value: colors[Math.round(Math.random() * 10) % 3] },
                },
                Value: {
                  streetNumberAndCity: {
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
                  streetNumberAndCity: {
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
            permissions: [],
            cityByDepartment: [],
            shoeColours: [],
            friendsBirthdays: [],
            holidays: [],
            friendsAddresses: [
              {
                Key: `${faker.person.firstName()} ${faker.person.lastName()}`,
                Value: [
                  {
                    streetNumberAndCity: {
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
                    streetNumberAndCity: {
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
                    streetNumberAndCity: {
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
                    streetNumberAndCity: {
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
              { Value: "departments" },
              { Value: "schoolAddress" },
              { Value: "mainAddress" },
              { Value: "addressesAndAddressesWithLabel" },
              { Value: "addressesWithColorLabel" },
              { Value: "addressesBy" },
              { Value: "permissions" },
              { Value: "cityByDepartment" },
              { Value: "holidays" },
              { Value: "addressesByCity" },
              { Value: "friendsAddresses" },
            ],
            ERP: {
              Discriminator: "ERPSAP",
              ERPSAP: {
                Discriminator: "SAPS2",
                SAPS2: {
                  S2OnlyField: true,
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
              category: {
                kind: "adult",
                extraSpecial: false,
              },
              fullName: {
                Item1: "",
                Item2: "",
              },
              birthday: "01/01/2000",
              subscribeToNewsletter: false,
              favoriteColor: { Value: { Value: null }, IsSome: false },
              gender: {
                IsRight: false,
                Value: {},
              },
              dependants: [],
              friendsByCategory: [],
              relatives: [],
              interests: [],
              departments: [],
              emails: [],
              schoolAddress: {
                streetNumberAndCity: {
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
              mainAddress: {
                IsRight: false,
                Value: "",
              },
              addressesAndAddressesWithLabel: {
                Item1: [],
                Item2: [],
              },
              addressesByCity: [],
              importantDate: {
                IsRight: false,
                Value: "",
              },
              cutOffDates: [],
              addressesBy: {
                IsRight: false,
                Value: [],
              },
              addressesWithColorLabel: [],
              permissions: [],
              cityByDepartment: [],
              shoeColours: [],
              friendsBirthdays: [],
              holidays: [],
              friendsAddresses: [],
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
