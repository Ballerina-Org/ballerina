
import {
    ForeignMutationsInput,
    Option, ParsedType,
    replaceWith,
    simpleUpdater, TypeName,
    Updater,
    Value,
    ValueOrErrors
} from "ballerina-core";

import {FullSpec, IdeReadonlyContext, IdeWritableState, VSpec} from "../../state";
import {Product} from "ballerina-core";


export type V1Types<T> = Map<TypeName, ParsedType<T>>
export type V2Types = Map<TypeName, any>

export type TypesBridge<T> = Product<V1Types<T>, V2Types>

export type SpecRaw = { specBody: Value<string> };

export type Bridge = Product<SpecRaw, SpecRaw>

export const SpecRaw = {
    Default: (body?: string): SpecRaw => ({
        specBody: Value.Default( body || "{}")
    }),
    Updaters: {
        Core: {
            specBody: (v: Value<string>): Updater<SpecRaw> =>
                Updater((raw) => ({specBody: v}))
        }
    }
}

export type BridgeErrors = string [] 
export type BridgeState = {
    spec: Bridge,
    seeds: any,
    errors: BridgeErrors,
}

const CoreUpdaters = {
    ...simpleUpdater<BridgeState>()("seeds"),
    ...simpleUpdater<BridgeState>()("spec"),
}
export const Bridge = {
    
    Empty: (): BridgeState =>
        ({
            spec: Product.Default(SpecRaw.Default(), SpecRaw.Default()),
            seeds: {},
            errors: [] }),
    Default: (fullSpec: FullSpec): BridgeState =>
        ({
            spec: Product.Default(SpecRaw.Default(JSON.stringify(fullSpec.v1)), SpecRaw.Default(JSON.stringify(fullSpec.v2))),
            seeds: fullSpec.seeds,
            errors: [] }),
    Updaters: {
        Core: CoreUpdaters,
        Template: {
            setV1Body: (v: Value<string>) => {
                debugger
                return CoreUpdaters.spec(Product.Updaters.left<SpecRaw, SpecRaw>(SpecRaw.Updaters.Core.specBody(v)));
            },
            setV2Body: (v: Value<string>) =>
                CoreUpdaters.spec(Product.Updaters.right<SpecRaw, SpecRaw>(SpecRaw.Updaters.Core.specBody(v))),
        },
    },

    Operations: {
        toVSpec: (b: Bridge): VSpec => ({ v1: JSON.parse(b.left.specBody.value), v2: JSON.parse(b.right.specBody.value)})
    },
    ForeignMutations: (
        _: ForeignMutationsInput<IdeReadonlyContext, IdeWritableState>,
    ) => ({
    }),
};