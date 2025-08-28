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
import { City } from "../../../person/domains/address/state";
import { v4 } from "uuid";

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



const tableApiSources: DispatchTableApiSources = (streamName: string) =>
    streamName == "ActiveUsersApi"
        ? ValueOrErrors.Default.return(getActiveUsers)
        : streamName == "ActiveFriendsApi"
            ? ValueOrErrors.Default.return(getActiveFriends)
            : streamName == "ChildrenApi"
                ? ValueOrErrors.Default.return(getChildren)
                : ValueOrErrors.Default.throwOne(`Cannot find table API ${streamName}`);

export const UnmockingApisTables = {

    tableApiSources,

};
//
