import { DispatchParsedType } from "../../../deserializer/domains/specification/domains/types/state";
import { DispatchDeltaFromDTO } from "./delta-to-dto/state";
import { DispatchDeltaToUpdater } from "./delta-to-updater/state";
import { PredicateValue } from "src/forms/domains/parser/domains/predicates/state";

export const DispatchDeltaTransfer = {
  Default: {
    FromDelta: DispatchDeltaFromDTO,
    ToUpdater: DispatchDeltaToUpdater,
    ToDelta: DispatchDeltaFromDTO,
  },
};
