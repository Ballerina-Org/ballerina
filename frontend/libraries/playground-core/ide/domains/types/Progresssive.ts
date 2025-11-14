export type ProgressiveAB<A, B> =
    | { kind: "selectA"; options: A[] }
    | { kind: "selectB"; a: A; options: B[] }
    | { kind: "done"; a: A; b: B };

export type ProgressiveABC<A, B, C> =
    | { kind: "selectA"; options: A[] }
    | { kind: "selectB"; a: A; options: B[] }
    | { kind: "selectC"; a: A; b: B; options: C[] }
    | { kind: "done"; a: A; b: B; c: C };