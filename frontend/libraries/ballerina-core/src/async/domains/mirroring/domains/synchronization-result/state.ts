export type SynchronizationResult =
  | "completed"
  | "should be enqueued again"
  | "permanent error";
