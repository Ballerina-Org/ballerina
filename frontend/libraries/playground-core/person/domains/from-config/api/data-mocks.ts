import { faker } from "@faker-js/faker";
import {
  CollectionReference,
  InfiniteStreamSources,
  PromiseRepo,
  EnumOptionsSources,
  EntityApis,
  unit,
  Guid,
  GlobalConfigurationSources,
} from "ballerina-core";
import { OrderedMap, List } from "immutable";
import { City } from "../../address/state";
import { AddressApi } from "../../address/apis/mocks";
import { v4 } from "uuid";
import { PersonApi } from "../../../apis/mocks";

const permissions = ["Create", "Read", "Update", "Delete"];
const colors = ["Red", "Green", "Blue"];
const genders = ["M", "F", "X"];
const interests = ["Soccer", "Hockey", "BoardGames", "HegelianPhilosophy"];

const streamApis: InfiniteStreamSources = (streamName: string) =>
  streamName == "departments"
    ? PersonApi.getDepartments()
    : streamName == "cities"
      ? AddressApi.getCities()
      : (_: any) => (_: any) => {
          alert(`Cannot find stream API ${streamName}`);
          return Promise.resolve({
            hasMoreValues: false,
            data: OrderedMap(),
          });
        };
const enumApis: EnumOptionsSources = (enumName: string) =>
  enumName == "colors"
    ? () =>
        PromiseRepo.Default.mock(
          () => colors.map((_) => ({ Value: _ })),
          undefined,
          1,
          0,
        )
    : enumName == "permissions"
      ? () =>
          PromiseRepo.Default.mock(
            () => permissions.map((_) => ({ Value: _ })),
            undefined,
            1,
            0,
          )
      : enumName == "genders"
        ? () =>
            PromiseRepo.Default.mock(
              () => genders.map((_) => ({ Value: _ })),
              undefined,
              1,
              0,
            )
        : enumName == "interests"
          ? () =>
              PromiseRepo.Default.mock(
                () => interests.map((_) => ({ Value: _ })),
                undefined,
                1,
                0,
              )
          : () =>
              PromiseRepo.Default.mock(() => {
                alert(`Cannot find enum API ${enumName}`);
                return [];
              });
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
            category: ["child", "adult", "senior"][
              Math.round(Math.random() * 10) % 3
            ],
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
              { Key: "Steve", Value: "adult" },
              { Key: "Alice", Value: "senior" },
            ],
            friendsByCategory: [],
            relatives: [
              ["child", "adult", "senior"][Math.round(Math.random() * 10) % 3],
              ["child", "adult", "senior"][Math.round(Math.random() * 10) % 3],
              ["child", "adult", "senior"][Math.round(Math.random() * 10) % 3],
            ],
            interests: [{ Value: interests[1] }, { Value: interests[2] }],
            departments: [
              { Id: v4(), DisplayValue: "Department 1" },
              { Id: v4(), DisplayValue: "Department 2" },
            ],
            emails: ["john@doe.it", "johnthedon@doe.com"],
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
          });
        };
      case "globalConfiguration":
        return (_: Guid) => {
          return Promise.resolve({
            IsAdmin: false,
            ERP: {
              caseName: "ERP:SAP",
              fields: {
                Value: {
                  caseName: "SAP:S2",
                  fields: {
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
              category: "",
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
            };
          })
      : (_) => {
          alert(`Cannot find entity API ${apiName} for 'default'`);
          return Promise.reject();
        },
};

export const PersonFromConfigApis = {
  streamApis,
  enumApis,
  entityApis,
};
