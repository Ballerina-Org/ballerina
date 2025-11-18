export type IdeFlags =
    | { kind: "localOnly" }
    | {
        kind: "localAndRemote";
        customLocks: string[];
        lockedCards:  any; //DashboardCardType[];
    };

export type TypeMapping = {
    typeStr?: string;
    ancestors?: string[];
    documentType?: string;
};
