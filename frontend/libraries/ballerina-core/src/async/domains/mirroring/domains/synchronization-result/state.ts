export type SynchronizationResult =
  | "completed"
  | "should be enqueued again"
  | "permanent error";

export type SynchronizationOperationResult =
  | { kind: "completed" }
  | { kind: "should be enqueued again" }
  | { kind: "permanent error" };
