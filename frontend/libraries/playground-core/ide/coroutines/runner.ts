import { Unit } from "ballerina-core";
import { Co } from "./builder";
import { bootstrap } from "./ide-bootstrap";

export const Bootstrap =
    Co.Template<Unit>(bootstrap, {
        runFilter: (props) => true,
    });