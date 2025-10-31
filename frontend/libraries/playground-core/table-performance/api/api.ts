import { faker } from "@faker-js/faker";
import {
  PromiseRepo,
  EntityApis,
  Guid,
  PredicateValue,
  ValueOrErrors,
  BasicFun,
  DispatchEnumOptionsSources,
  DispatchTableApiSources,
  DispatchTableApiSource,
  DispatchOneSource,
  DispatchLookupSources,
  TableAbstractRendererState,
  DispatchParsedType,
  SumNType,
  TableGetManyParams,
} from "ballerina-core";
import { Map, Range } from "immutable";
import { ValueStreamPosition } from "ballerina-core";
import { v4 } from "uuid";

const userFieldsEnum = [
  "Name",
  "Surname",
  "Birthday",
  "Email",
  "SubscribeToNewsletter",
];
const userGroupFieldsEnum = ["Name", "Description"];
const activityFieldsEnum = ["Description", "Timestamp"];
const usersSetupTabsEnum = [
  "ActiveFields",
  "InactiveFields",
  "UserGroupsFields",
  "ActivityFields",
];

const getUsers: DispatchTableApiSource = {
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
    (streamParams: TableGetManyParams) => {
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
      })).then((res) =>
        PredicateValue.Default.table(
          res.From,
          res.To,
          TableAbstractRendererState.Operations.tableValuesToValueRecord(
            res.Values,
            fromApiRaw,
          ),
          res.HasMore,
        ),
      );
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

const tableApiSources: DispatchTableApiSources = (streamName: string) =>
  streamName == "TablePerformanceUsersApi"
    ? ValueOrErrors.Default.return(getUsers)
    : ValueOrErrors.Default.throwOne(`Cannot find table API ${streamName}`);

const createTuple4 = () => ({
  Item1: faker.person.firstName(),
  Item2: faker.number.int(),
  Item3: faker.datatype.boolean(),
  Item4: {
    Street: faker.location.street(),
    Number: faker.number.int(),
    City: faker.location.city(),
    State: faker.location.state(),
    Zip: faker.location.zipCode(),
    Country: faker.location.country(),
  },
});

const entityApis: EntityApis = {
  create: (apiName: string) => (e: any) => {
    alert(`Cannot find entity API ${apiName} for 'create'`);
    return Promise.reject();
  },
  get: (apiName: string) => {
    switch (apiName) {
      case "App":
        return (_: Guid) => {
          console.log(`get app api`);
          return Promise.resolve({
            Users: {
              Values: Range(1, 300)
                .map((_) => ({
                  [1]: createTuple4(),
                  [2]: createTuple4(),
                  [3]: createTuple4(),
                  [4]: createTuple4(),
                  [5]: createTuple4(),
                  [6]: createTuple4(),
                  [7]: createTuple4(),
                  [8]: createTuple4(),
                  [9]: createTuple4(),
                  [10]: createTuple4(),
                  [11]: createTuple4(),
                  [12]: createTuple4(),
                  [13]: createTuple4(),
                  [14]: createTuple4(),
                  [15]: createTuple4(),
                  [16]: createTuple4(),
                  [17]: createTuple4(),
                  [18]: createTuple4(),
                  [19]: createTuple4(),
                  [20]: createTuple4(),
                  [21]: createTuple4(),
                  [22]: createTuple4(),
                  [23]: createTuple4(),
                  [24]: createTuple4(),
                  [25]: createTuple4(),
                  [26]: createTuple4(),
                  [27]: createTuple4(),
                  [28]: createTuple4(),
                  [29]: createTuple4(),
                  [30]: createTuple4(),
                }))
                .toArray(),
              HasMore: true,
              From: 0,
              To: 10,
            },
            Admin: {
              isRight: false,
              // isRight: true,
              // right: {
              //   Name: "Spiffy",
              //   Surname: "User",
              //   Birthday: "1990-01-01",
              //   Email: "admin.user@example.com",
              //   SubscribeToNewsletter: true,
              // },
            },
            SuperAdmin: {
              isRight: false,
              Value: {},
              // isRight: true,
              // right: {
              //   Name: "Spiffy",
              //   Surname: "User",
              // },
            },
            Inactive: {
              Values: {},
              HasMore: true,
              From: 0,
              To: 1,
            },
            Groups: {
              Values: {},
              HasMore: false,
              From: 0,
              To: 0,
            },
            Activities: {
              Values: {},
              HasMore: false,
              From: 0,
              To: 0,
            },
          });
        };
      case "globalConfiguration":
        return (_: Guid) => {
          return Promise.resolve({});
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
      case "App":
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
  default: (apiName: string) => (_) => {
    alert(`Cannot find entity API ${apiName} for 'default'`);
    return Promise.reject();
  },
};

export const UsersSetupFromConfigApis = {
  entityApis,
  tableApiSources,
};
