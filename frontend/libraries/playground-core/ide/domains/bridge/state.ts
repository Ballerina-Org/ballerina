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

export type SpecSource = {
    specBody: Value<string>
};

export type SpecOutcome<V> = ValueOrErrors<V, string>

export type SpecV1 = Product<SpecSource,SpecOutcome<V1>>
export type SpecV2 = Product<SpecSource,SpecOutcome<V2>>

export type Bridge = Product<SpecV1, SpecV2>

export const SpecDesign = {
    Default: (): SpecSource => ({
        specBody: Value.Default("{}")
    }),
    Updaters: {
        Core: {
            specBody: (v: Value<string>) => ({
                specBody: v
            }),
        }
    }
}


export const SpecV1 = {
    Default: () =>
        Product.Default<SpecSource,SpecOutcome<V1>>(SpecDesign.Default(), ValueOrErrors.Default.throwOne("V1 not yet available")),
    FromBody: (body: string) =>
        Product.Default<SpecSource,SpecOutcome<V1>>({ specBody: Value.Default(body) }, ValueOrErrors.Default.throwOne("V1 not yet validated")),
    FromError: (error: string[]) =>
        Product.Default<SpecSource,SpecOutcome<V1>>({ specBody: Value.Default("") }, ValueOrErrors.Default.throw(List(error))),
    Updaters: {
        Core: {
            specBody: (v: Value<string>) : Updater<SpecV1> => 
                Product.Updaters.left<SpecSource, SpecOutcome<V1>>(replaceWith({ specBody: v }))
        }
    },
    Operations: {
        validate:  async () => ["errors"],
        // launchers: (v1: SpecV1) => {
        //     try 
        //        
        // }
    }
}

export const SpecV2 = {
    Default: () =>
        Product.Default<SpecSource,SpecOutcome<V2>>(SpecDesign.Default(), ValueOrErrors.Default.throwOne("V1 not yet available")),
    FromBody: (body: string) =>
        Product.Default<SpecSource,SpecOutcome<V2>>({ specBody: Value.Default(body) }, ValueOrErrors.Default.throwOne("V1 not yet validated")),
    FromError: (error: string[]) =>
        Product.Default<SpecSource,SpecOutcome<V2>>({ specBody: Value.Default("") }, ValueOrErrors.Default.throw(List(error))),
    Updaters: {
        Core: {
            specBody: (v: Value<string>) : Updater<SpecV2> =>
                Product.Updaters.left<SpecSource, SpecOutcome<V2>>(replaceWith({ specBody: v }))
        }
    },
    Operations: {
        validate:  async () => ["errors"],
        // launchers: (v1: SpecV1) => {
        //     try 
        //        
        // }
    }
}



// export type Cardinality = "One" | "Many";
// export type Errors = Record<string, string[]>;
//
//
// export type ValidationError =
//     | { kind: "V1Invalid"; errors: Errors }
//     | { kind: "V2Invalid"; errors: Errors }
//     | { kind: "MissingTypeInV2"; name: string }
//     | { kind: "MissingTable"; name: string }
//     | { kind: "ExtraTable"; name: string }
//     | {
//     kind: "OneRelationMismatch";
//     v1: string;
//     v2: string;
//     expected: Cardinality;
//     actual: Cardinality;
// }
//     | {
//     kind: "ManyRelationMismatch";
//     v1: string;
//     v2: string;
//     expected: Cardinality;
//     actual: Cardinality;
// }
//     | { kind: "FormNotSpanningTree"; launcher: string; reason: string }
//     | { kind: "UnionCaseMissingInV1"; typeName: string; case: string }
//     | { kind: "TypeFieldsMismatch"; path: string[]; field: string }
//     | { kind: "Other"; message: string };

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
    V1: (b: Bridge) : Option<V1> => {
        return b.left.right.kind === "value" ? Option.Default.some(b.left.right.value) : Option.Default.none();
    },
    SpecV1: (b: Bridge) : Option<SpecV1> => {
        return b.left.right.kind === "value" ? Option.Default.some(b.left) : Option.Default.none();
    },
    Default: (): BridgeState =>
        ({
            bridge: Product.Default(SpecV1.Default(), SpecV2.Default()),
            seeds: {},
            errors: [] }),
    Updaters: {
        Core: CoreUpdaters,
        Template: {
            setV1Body: (v: Value<string>) => Updater<BridgeState>(bs => {
                let b =
                    Product.Updaters.left<SpecV1, SpecV2>(replaceWith(SpecV1.FromBody(v.value)))
                    (bs.bridge)
                return ({...bs, bridge: b});
            }),
            setV2Body: (v: Value<string>) => Updater<BridgeState>(bs => {
                let b =
                    Product.Updaters.right<SpecV1, SpecV2>(replaceWith(SpecV2.FromBody(v.value)))
                    (bs.bridge)
                return ({...bs, bridge: b});
            }),
        },
    },

    Operations: {
        load: (data: ValueOrErrors<string[], string>): Updater<BridgeState> => {
            const left =
                data.kind === "value" ? SpecV1.FromBody(JSON.stringify(data.value[0])): SpecV1.FromError(data.errors.toArray());
            const right =
                data.kind === "value" ? SpecV2.FromBody(JSON.stringify(data.value[1])): SpecV1.FromError(data.errors.toArray());
            debugger
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