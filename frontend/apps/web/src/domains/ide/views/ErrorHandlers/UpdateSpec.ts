import {ideToast} from "../toaster.tsx";
import {toast as sonnerToast} from "sonner";
import {ValueOrErrors} from "ballerina-core";

export const updateSpecErrorHandler = (
    voe: ValueOrErrors<any, string>
): ReturnType<typeof ideToast> =>
    voe.kind === "value"
        ? ideToast({
            title: "success",
            description: "Spec has been saved",
            button: { label: "Ok", onClick: () => {} },
        })
        : ideToast({
            title: "error",
            description: JSON.stringify(
                (voe as Extract<ValueOrErrors<any, string>, { kind: "errors" }>).errors
            ),
            button: { label: "Ok", onClick: () => {} },
        });

export const seedSpecErrorHandler = (
    voe: ValueOrErrors<any, string>
): ReturnType<typeof ideToast> =>
    voe.kind === "value"
        ? ideToast({
            title: "success",
            description: "Spec has been seeded",
            button: { label: "Ok", onClick: () => {} },
        })
        : ideToast({
            title: "error",
            description: JSON.stringify(
                (voe as Extract<ValueOrErrors<any, string>, { kind: "errors" }>).errors
            ),
            button: { label: "Ok", onClick: () => {} },
        });