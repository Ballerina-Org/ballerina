import {
    ForeignMutationsInput,
    Launcher,
    Option,
    replaceWith,
    simpleUpdater,
    Updater,
    Value,
    ValueOrErrors
} from "ballerina-core";
import {Ide, IdeReadonlyContext, IdeWritableState} from "../../state";
import {Product} from "ballerina-core";
import {V1, V2} from "../../api/specs";
import {List} from "immutable";
import * as repl from "node:repl";

export type SpecDesign = { specBody: Value<string>, dirty: boolean };
export type SpecOutcome<V> = ValueOrErrors<V, string>

export type SpecV1 = Product<SpecDesign,SpecOutcome<V1>>
export type SpecV2 = Product<SpecDesign,SpecOutcome<V2>>
export type SpecV<V> = Product<SpecDesign,SpecOutcome<V>>
export type Bridge = Product<SpecV1, SpecV2>

export const SpecDesign = {
    Default: (): SpecDesign => ({
        specBody: Value.Default("{}"), dirty: false
    }),
    Updaters: {
        Core: {
            specBody: (v: Value<string>): SpecDesign => ({
                specBody: v, dirty: true
            }),
        }
    }
}

export const SpecV = {
    Default: <V>(): SpecV<V> =>
        Product.Default<SpecDesign,SpecOutcome<V>>(SpecDesign.Default(), ValueOrErrors.Default.throwOne("spec not yet available")),
    FromBody: <V>(body: string): SpecV<V>  =>
        Product.Default<SpecDesign,SpecOutcome<V>>({ specBody: Value.Default(body), dirty: false }, ValueOrErrors.Default.throwOne("V not yet validated")),
    FromError: <V>(error: string[]): SpecV<V>  =>
        Product.Default<SpecDesign,SpecOutcome<V>>({ specBody: Value.Default(""), dirty: false}, ValueOrErrors.Default.throw(List(error))),
    Updaters: {
        Core: {
            specBody: <V>(v: Value<string>) : Updater<SpecV<V>> => 
                Product.Updaters.left<SpecDesign, SpecOutcome<V>>(replaceWith<SpecDesign>({ specBody: v, dirty: true })),
            setOutcome: <V>(v: Value<string>) : Updater<SpecV<V>> =>
                Product.Updaters.right<SpecDesign, SpecOutcome<V>>(replaceWith<SpecOutcome<V2>>(ValueOrErrors.Default.return(JSON.parse(v.value))))
        }
    },
}

// export const SpecV2 = {
//     Default: () =>
//         Product.Default<SpecSource,SpecOutcome<V2>>(SpecDesign.Default(), ValueOrErrors.Default.throwOne("V1 not yet available")),
//     FromBody: (body: string) =>
//         Product.Default<SpecSource,SpecOutcome<V2>>({ specBody: Value.Default(body), dirty: false }, ValueOrErrors.Default.throwOne("V1 not yet validated")),
//     FromError: (error: string[]) =>
//         Product.Default<SpecSource,SpecOutcome<V2>>({ specBody: Value.Default(""), dirty: false }, ValueOrErrors.Default.throw(List(error))),
//     Updaters: {
//         Core: {
//             specBody: (v: Value<string>) : Updater<SpecV2> =>
//                 Product.Updaters.left<SpecSource, SpecOutcome<V2>>(replaceWith<SpecSource>({ specBody: v, dirty: false }))
//         }
//     },
//     Operations: {
//         validate:  async () => ["errors"],
//         // launchers: (v1: SpecV1) => {
//         //     try 
//         //        
//         // }
//     }
// }
//

export type BridgeErrors = string [] //TODO: use ValidationError
export type BridgeState = {
    bridge: Bridge,
    seeds: any,
    errors: BridgeErrors,
}

const CoreUpdaters = {
    ...simpleUpdater<BridgeState>()("seeds"),
}
export const Bridge = {
    
    Default: (): BridgeState =>
        ({
            bridge: Product.Default(SpecV.Default<V1>(), SpecV.Default<V2>()),
            seeds: {},
            errors: [] }),
    Updaters: {
        Core: CoreUpdaters,
        Template: {
            setV1Body: (v: Value<string>) => Updater<BridgeState>(bs => {
                let b =
                    Product.Updaters.left<SpecV1, SpecV2>(SpecV.Updaters.Core.specBody<V1>(v))
                    (bs.bridge)
                return ({...bs, bridge: b});
            }),
            // setV1Design: (v: Value<string>) => Updater<BridgeState>(bs => {
            //     const l = Product.Updaters.left<SpecSource, SpecOutcome<V1>>(replaceWith<SpecSource>({specBody: v, dirty: true}))
            //     let b =
            //         Product.Updaters.left<SpecV1, SpecV2>(l)
            //         (bs.bridge)
            //     return ({...bs, bridge: b});
            // }),
            setV2Body: (v: Value<string>) => Updater<BridgeState>(bs => {
                let b =
                    Product.Updaters.right<SpecV1, SpecV2>(SpecV.Updaters.Core.specBody<V2>(v))
                    (bs.bridge)
                return ({...bs, bridge: b});
            }),
            setV2: (v: Value<string>) => Updater<BridgeState>(bs => {
                let b =
                    Product.Updaters.right<SpecV1, SpecV2>(SpecV.Updaters.Core.setOutcome<V2>(v))
                    (bs.bridge)
                return ({...bs, bridge: b});
            }),
        },
    },

    Operations: {
        getV1: (b: Bridge) : Option<V1> => {
            return b.left.right.kind === "value" ? Option.Default.some(b.left.right.value) : Option.Default.none();
        },
        load: (data: ValueOrErrors<string[], string>): Updater<BridgeState> => {
            const left: SpecV<V1> =
                data.kind === "value" ? SpecV.FromBody(JSON.stringify(data.value[0])): SpecV.FromError(data.errors.toArray());
            const right: SpecV<V2> =
                data.kind === "value" ? SpecV.FromBody(JSON.stringify(data.value[1])): SpecV.FromError(data.errors.toArray());
    
            return Updater<BridgeState>(bs => {
                let b =
                    Product.Updaters.left<SpecV1, SpecV2>(replaceWith(left))
                        .then(Product.Updaters.right<SpecV1, SpecV2>(replaceWith(right)))
                    (bs.bridge)

                return ({...bs, bridge: b});
            })
        },
        errors: (errors: string []): Updater<BridgeState> => {
            return Updater<BridgeState>(bs => {
                return ({...bs, errors: errors});
            })
        },
        undirty: (): Updater<BridgeState> => {
            return Updater<BridgeState>(bs => {
                const designV1 = bs.bridge.left.left.specBody;


                const b =
                    Product.Updaters.left<SpecV1, SpecV2>(
                        Product.Updaters.left<SpecDesign,SpecOutcome<V1>>(
                            replaceWith<SpecDesign>({ specBody:designV1, dirty: false})
                            )

                    )(bs.bridge);
                return ({...bs, errors: [], bridge: b });
            })
        },
        setV: (): Updater<BridgeState> => {
            return Updater<BridgeState>(bs => {
                const designV1 = bs.bridge.left.left.specBody.value;
                const designV2 = bs.bridge.right.left.specBody.value;

        
                const b = 
                    Product.Updaters.left<SpecV1, SpecV2>(
                        Product.Updaters.right(
                            replaceWith(
                                ValueOrErrors.Default.return(JSON.parse(designV1) as V1)
                            )
                        )
                    ).then(Product.Updaters.right<SpecV1, SpecV2>(
                        Product.Updaters.right(
                            replaceWith(
                                ValueOrErrors.Default.return(JSON.parse(designV2) as V2)
                            )
                        )
                    ))(bs.bridge);
                return ({...bs, errors: [], bridge: b });
            })
        },
    },
    ForeignMutations: (
        _: ForeignMutationsInput<IdeReadonlyContext, IdeWritableState>,
    ) => ({
    }),
};