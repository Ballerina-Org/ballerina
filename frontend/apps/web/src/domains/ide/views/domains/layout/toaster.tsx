// source: https://sonner.emilkowal.ski/styling
import React from 'react';
import {toast, Toaster} from 'sonner';
import {List} from "immutable";

type ToastPosition =
    | "top-left"
    | "top-center"
    | "top-right"
    | "bottom-left"
    | "bottom-center"
    | "bottom-right";

interface ToastProps {
    id: string | number;
    title: string;
    description: List<string> | string;
    position?: string,
    duration?: number,
    dismissible?: boolean,
    button?: {
        label: string;
        onClick: () => void;
    };
}
import type { ExternalToast } from "sonner";
import {ValueOrErrors} from "ballerina-core";

const getPosition = (type: "success" | "warning" | "error" | "info"): ToastPosition => {
    switch (type) {
        case "success":
            return "top-center";
        case "warning":
            return "top-center";
        case "error":
            return "top-center";
        case "info":
            return "bottom-right";
    }
};

export function AppToaster() {
    return (
        <Toaster
            richColors
            position="top-center"
            closeButton
            // optional defaults:
            toastOptions={{ duration: 4000 }}
        />
    );
}

export const notify = {
    success: (title: string, description?: string, duration?: number) =>
        toast.success(title, { description, duration, position: getPosition("success") }),
    warning: (title: string, description?: string, duration?: number) =>
        toast.warning(title, { description, duration, position: getPosition("warning") }),
    error: (title: string, description?: string, duration?: number) =>
        toast.error(title, { description, duration, position: getPosition("error") }),
    info: (title: string, description?: string, duration?: number) =>
        toast.info(title, { description, duration, position: getPosition("info") }),
};

export const fromVoe = (voe: ValueOrErrors<any, string>, title: string ) =>
    voe.kind == 'errors' ? 
        notify.error(title,JSON.stringify(
            (voe as Extract<ValueOrErrors<any, string>, { kind: "errors" }>).errors
        )) : notify.success(title)

export const errorFromList = (voe: List<string> ): string =>
    JSON.stringify(
        voe
    )