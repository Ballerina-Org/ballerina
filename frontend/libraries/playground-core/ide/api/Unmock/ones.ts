import {faker} from "@faker-js/faker";
import {
    BasicFun,
    DispatchLookupSources,
    DispatchOneSource,
    Guid,
    PredicateValue,
    PromiseRepo,
    TableAbstractRendererState,
    Value,
    ValueOrErrors,
    ValueStreamPosition,
} from "ballerina-core";
import {Map, Range} from "immutable";

import {v4} from "uuid";

import {getSeedEntityId, getLookup} from "../seeds";
// import {getLookup} from "../seeds-lookups";

const permissions = ["Create", "Read", "Update", "Delete"];
const colors = ["Red", "Green", "Blue"];
const genders = ["M", "F", "X"];
const interests = ["Soccer", "Hockey", "BoardGames", "HegelianPhilosophy"];


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

// const lookupSources: DispatchLookupSources = (typeName: string) =>
//     typeName == "User"
//         ? ValueOrErrors.Default.return({
//             one: (apiName: string) =>
//                 apiName == "BestFriendApi"
//                     ? ValueOrErrors.Default.return(getFriends)
//                     : ValueOrErrors.Default.throwOne(
//                         `can't find api ${apiName} when getting lookup api sources`,
//                     ),
//         })
//         : ValueOrErrors.Default.throwOne(
//             `can't find type ${typeName} when getting lookup api source`,
//         );

const lookupSources: DispatchLookupSources = (typeName: string) =>{

    return ValueOrErrors.Default.return({
            one: (apiName: string) =>
                ValueOrErrors.Default.return(
                    {
                        get: (id: Guid) => {
                            debugger
                            const fieldName = apiName.replace(/Api$/, "");
                            return getSeedEntityId("sample", typeName, id).then(valueOrErrors =>
                                valueOrErrors.kind == "value" ? valueOrErrors.value : undefined);
                        },
                        getManyUnlinked:
                            (fromApiRaw: BasicFun<any, ValueOrErrors<PredicateValue, string>>) =>
                                (id: Guid) =>
                                    (streamParams: Map<string, string>) =>
                                        ([streamPosition]: [ValueStreamPosition]) => {
                                            debugger
                                            const fieldName = apiName.replace(/Api$/, "");
                                            //todo: in get many unlinked we dont call for unlinked ones' by design (for now)
                                            const call = 
                                                getLookup("sample",fieldName, id, streamPosition.chunkIndex || 0, streamPosition.chunkSize || 2)
                                                    .then( valueOrErrors => ({
                                                        Values: valueOrErrors.kind == "value" ? valueOrErrors.value : [],
                                                        HasMore: false,//TODO
                                                        From: 1,
                                                        To: 5,
                                                    }))
                                            return call
                                            // return PromiseRepo.Default.mock(() => ({
                                            //     Values: Range(1, 5)
                                            //         .map((_) => ({
                                            //             Id: v4(),
                                            //             Name: faker.person.firstName(),
                                            //             Surname: faker.person.lastName(),
                                            //             Birthday: faker.date.birthdate().toISOString(),
                                            //             Email: faker.internet.email(),
                                            //             SubscribeToNewsletter: faker.datatype.boolean(),
                                            //             FavoriteColor: {
                                            //                 Value: { Value: colors[Math.round(Math.random() * 10) % 3] },
                                            //                 IsSome: true,
                                            //             },
                                            //             Friends: {
                                            //                 From: 0,
                                            //                 To: 0,
                                            //                 HasMore: true,
                                            //                 Values: {},
                                            //             },
                                            //         }))
                                            //         .reduce((acc, curr) => {
                                            //             acc[curr.Id] = curr;
                                            //             return acc;
                                            //         }, {} as any),
                                            //     HasMore: false,
                                            //     From: 1,
                                            //     To: 5,
                                            // })
                                            // )
                                            .then((res) => (
                                            //     {
                                            //     hasMoreValues: false, //res.HasMore,
                                            //     to: 2, //res.To,
                                            //     from: 1, //res.From,
                                            //     data: TableAbstractRendererState.Operations.tableValuesToValueRecord(
                                            //         res.kind == "value" ? res.value : {}, //.Values,
                                            //         fromApiRaw,
                                            //     ),
                                            // }
                                                {
                                                    hasMoreValues: res.HasMore,
                                                    to: res.To,
                                                    from: res.From,
                                                    data: TableAbstractRendererState.Operations.tableValuesToValueRecord(
                                                        res.Values,
                                                        fromApiRaw,
                                                    ),
                                                }
                                            ));
                                        },
                    }
                )
        })};
export const UnmockingApisLookups = {

    lookupSources,
};
//
