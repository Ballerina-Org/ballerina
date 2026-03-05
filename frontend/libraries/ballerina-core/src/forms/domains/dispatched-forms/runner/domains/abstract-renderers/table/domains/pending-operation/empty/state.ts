export type TableAbstractRendererNoPendingOps = {
  kind: "empty";
};

export const TableAbstractRendererNoPendingOps = {
  Default: (): TableAbstractRendererNoPendingOps => ({ kind: "empty" }),
};
