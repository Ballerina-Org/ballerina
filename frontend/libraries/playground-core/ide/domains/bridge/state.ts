import {
    ForeignMutationsInput,
    Option,
    replaceWith,
    simpleUpdater,
    Updater,
    Value,
    ValueOrErrors
} from "ballerina-core";

import {FullSpec, IdeReadonlyContext, IdeWritableState, VSpec} from "../../state";
import {Product} from "ballerina-core";
import {V1, V2} from "playground-core";
import {List} from "immutable";

export type SpecRaw = { specBody: Value<string>, dirty: boolean };

export type Bridge = Product<SpecRaw, SpecRaw>

export const SpecRaw = {
    Default: (body?: string): SpecRaw => ({
        specBody: Value.Default( body || "{}"), dirty: false
    }),
    Updaters: {
        Core: {
            specBody: (v: Value<string>): Updater<SpecRaw> =>
                Updater((raw) => ({specBody: v, dirty: true}))
        }
    }
}


export type BridgeErrors = string [] //TODO: use ValidationError
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
            setV1Body: (v: Value<string>) => 
                CoreUpdaters.spec(Product.Updaters.left<SpecRaw, SpecRaw>(SpecRaw.Updaters.Core.specBody(v))),
            setV2Body: (v: Value<string>) =>
                CoreUpdaters.spec(Product.Updaters.right<SpecRaw, SpecRaw>(SpecRaw.Updaters.Core.specBody(v))),
        },
    },

    Operations: {
        toVSpec: (b: Bridge): VSpec => ({ v1: JSON.parse(b.left.specBody.value), v2: JSON.parse(b.right.specBody.value)})
        // getV1: (b: Bridge) : Option<V1> => {
        //     return b.left.right.kind === "value" ? Option.Default.some(b.left.right.value) : Option.Default.none();
        // },
        // load: (data: ValueOrErrors<string[], string>): Updater<BridgeState> => {
        //     const left: SpecState<V1> =
        //         data.kind === "value" ? SpecV.FromBody(JSON.stringify(data.value[0])): SpecV.FromError(data.errors.toArray());
        //     const right: SpecState<V2> =
        //         data.kind === "value" ? SpecV.FromBody(JSON.stringify(data.value[1])): SpecV.FromError(data.errors.toArray());
        //
        //     return Updater<BridgeState>(bs => {
        //         let b =
        //             Product.Updaters.left<SpecState<V1>, SpecState<V2>>(replaceWith(left))
        //                 .then(Product.Updaters.right<SpecState<V1>, SpecState<V2>>(replaceWith(right)))
        //             (bs.spec)
        //
        //         return ({...bs, spec: b});
        //     })
        // },
        // errors: (errors: string []): Updater<BridgeState> => {
        //     return Updater<BridgeState>(bs => {
        //         return ({...bs, errors: errors});
        //     })
        // },
        // undirty: (): Updater<BridgeState> => {
        //     return Updater<BridgeState>(bs => {
        //         const designV1 = bs.spec.left.left.specBody;
        //
        //
        //         const b =
        //             Product.Updaters.left<SpecState<V1>, SpecState<V2>>(
        //                 Product.Updaters.left<SpecRaw,SpecTyped<V1>>(
        //                     replaceWith<SpecRaw>({ specBody:designV1, dirty: false})
        //                     )
        //
        //             )(bs.spec);
        //         return ({...bs, errors: [], spec: b });
        //     })
        // },
        // setV: (): Updater<BridgeState> => {
        //     return Updater<BridgeState>(bs => {
        //         const designV1 = bs.spec.left.left.specBody.value;
        //         const designV2 = bs.spec.right.left.specBody.value;
        //
        //
        //         const b = 
        //             Product.Updaters.left<SpecState<V1>, SpecState<V2>>(
        //                 Product.Updaters.right(
        //                     replaceWith(
        //                         ValueOrErrors.Default.return(JSON.parse(designV1) as V1)
        //                     )
        //                 )
        //             ).then(Product.Updaters.right<SpecState<V1>, SpecState<V2>>(
        //                 Product.Updaters.right(
        //                     replaceWith(
        //                         ValueOrErrors.Default.return(JSON.parse(designV2) as V2)
        //                     )
        //                 )
        //             ))(bs.spec);
        //         return ({...bs, errors: [], spec: b });
        //     })
        // },
    },
    ForeignMutations: (
        _: ForeignMutationsInput<IdeReadonlyContext, IdeWritableState>,
    ) => ({
    }),
};