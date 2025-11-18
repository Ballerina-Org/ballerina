import {assertUnreachable} from "@blp-private-npm/ui";

export type SketchType =
    | "cube"
    | "tick"
    | "lamp"
    | "rocket"
    | "celebration"
    | "robot"
    | "broken-cog"
    | "box-robot"
    | "geometric-blocks"
    | "happy-woman"
    | "floating-cube"
    | "trophy"
    | "person-cube"
    | "maintenance"
    | "sock"
    | "engineers"
    | "magnifying-glass"
    | "man-on-sailboat"
    | "pen-on-invoice"
    | "moon-flag"
    | "celebrating-woman"
    | "happy-baloons"
    | "waving-robot"
    | "robot-with-cube";
const getSketchUrl = (file: string) =>
    `https://storage.googleapis.com/public.blp-digital.com/ch/docuclerk/sketches/${file}`;

export const SketchType = {
    Operations: {
        getSketchPath: (sketchType: SketchType) => {
            switch (sketchType) {
                case "cube":
                    return getSketchUrl("sketch_cube.png");
                case "tick":
                    return getSketchUrl("sketch_tick.png");
                case "lamp":
                    return getSketchUrl("sketch_lamp.png");
                case "rocket":
                    return getSketchUrl("sketch_rocket.png");
                case "celebration":
                    return getSketchUrl("sketch_celebration.png");
                case "robot":
                    return getSketchUrl("sketch_robot.png");
                case "broken-cog":
                    return getSketchUrl("sketch_broken_cog.png");
                case "box-robot":
                    return getSketchUrl("sketch_box_robot.png");
                case "geometric-blocks":
                    return getSketchUrl("sketch_geometric_blocks.png");
                case "happy-woman":
                    return getSketchUrl("sketch_happy_woman.png");
                case "floating-cube":
                    return getSketchUrl("sketch_floating_cube.png");
                case "trophy":
                    return getSketchUrl("sketch_trophy.png");
                case "person-cube":
                    return getSketchUrl("sketch_person_cube.png");
                case "maintenance":
                    return getSketchUrl("sketch_maintenance.png");
                case "sock":
                    return getSketchUrl("sketch_sock.png");
                case "engineers":
                    return getSketchUrl("sketch_engineers.png");
                case "magnifying-glass":
                    return getSketchUrl("sketch_magnifying_glass.png");
                case "man-on-sailboat":
                    return getSketchUrl("sketch_man_on_sailboat.png");
                case "celebrating-woman":
                    return getSketchUrl("sketch_celebrating_woman.png");
                case "pen-on-invoice":
                    return getSketchUrl("sketch_pen_on_invoice.png");
                case "moon-flag":
                    return getSketchUrl("sketch_moon_flag.png");
                case "happy-baloons":
                    return getSketchUrl("sketch_happy_baloons.png");
                case "waving-robot":
                    return getSketchUrl("sketch_waving_robot.png");
                case "robot-with-cube":
                    return getSketchUrl("sketch_robot_with_cube.png");
                default:
                    assertUnreachable(sketchType);
            }
        },
    },
};