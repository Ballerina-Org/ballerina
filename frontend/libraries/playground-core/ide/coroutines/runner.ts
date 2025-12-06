import { Unit } from "ballerina-core";
import { Co } from "./builder";
import { bootstrap } from "./ide-bootstrap";
import { CustomEntity } from "ide/domains/phases/custom-fields/state";

export const Bootstrap =
    Co.Template<Unit>(bootstrap, {
        runFilter: (props) => props.context.phase.kind === 'bootstrap' },
    );
