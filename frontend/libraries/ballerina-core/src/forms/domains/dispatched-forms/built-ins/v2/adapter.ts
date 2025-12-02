import {PredicateValue} from "../../../parser/domains/predicates/state";
import {DispatchParsedType} from "../../deserializer/domains/specification/domains/types/state";
import {Option} from "../../../../../collections/domains/sum/state";
import {Maybe} from "../../../../../collections/domains/maybe/state";
import {Guid} from "playground-core/ide/domains/types/Guid";

export type SimpleArity = {
    min: Maybe<number>,
    max: Maybe<number>
};

export type LookupMethod = 
    | { kind: 'create' }
    | { kind: 'delete' }
    | { kind: 'link' }
    | { kind: 'unlink' }
    | { kind: 'clear' }
    | { kind: 'replace'}

export type Var = { name: string };

export type UpdaterPathStep =
    | { kind: "Field"; field: string }
    | { kind: "TupleItem"; index: number | Var }
    | { kind: "ListItem"; item: Var }
    | { kind: "UnionCase"; case: string; var: Var }
    | { kind: "SumCase"; index: number; var: Var };

export function isRelationType(
    t: DispatchParsedType<any>
): t is DispatchParsedType<any> & {
    kind: "singleSelection" | "multiSelection" | "one";
} {
    return (
        t.kind === "singleSelection" ||
        t.kind === "multiSelection" ||
        t.kind === "one"
    );
}

export type Delta<VExt, DExt> =
    | { kind: "Multiple"; items: Delta<VExt, DExt>[] }
    | { kind: "Replace"; value: any }
    | { kind: "Record"; field: string; delta: Delta<VExt, DExt> }
    | { kind: "Union"; case: string; delta: Delta<VExt, DExt> }
    | { kind: "Tuple"; index: number; delta: Delta<VExt, DExt> }
    | { kind: "Sum"; index: number; delta: Delta<VExt, DExt> }
    | { kind: "Ext"; ext: DExt };

export type Relation = {
        arity: SimpleArity;
        path: UpdaterPathStep []
        method: LookupMethod
        targets: Guid []
        value: Option<any>
    }

export type ToApiValue =
    | { kind: 'Structural'; item: any }
    | { kind: 'Relation'; relation: Relation }

export type ToApiResult = ToApiValue []

export function extractSelectionItems(
    pv: PredicateValue,
    t: DispatchParsedType<any>
): PredicateValue[] {
    switch (t.kind) {
        case "one":
            return extractSelectionItems(pv, t.arg);

        case "singleSelection":
            return pv == null ? [] : [pv];

        case "multiSelection":
            return Array.isArray(pv) ? pv : [];

        default:
            return [];
    }
}