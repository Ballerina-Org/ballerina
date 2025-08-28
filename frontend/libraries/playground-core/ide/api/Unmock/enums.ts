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


const permissions = ["Create", "Read", "Update", "Delete"];
const colors = ["Red", "Green", "Blue"];
const genders = ["M", "F", "X"];
const interests = ["Soccer", "Hockey", "BoardGames", "HegelianPhilosophy"];



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

export const UnmockingApisEnums = {

    enumApis,

};
//
