import {Maybe, simpleUpdater, Value, ValueOrErrors, View} from "ballerina-core";
import {List} from "immutable";
import JSZip, { JSZipObject } from "jszip";
import {WorkspaceVariant} from "../locked/domains/folders/state";
import {BootstrapPhase} from "../bootstrap/state";

export type SelectionStep = 
    | 'idle'
    | 'upload-started' 
    | 'upload-finished'

export type Spec = { name: string }

export type SelectionPhase = {
    kind: SelectionStep,
    name: Value<string>,
    specs: Spec[],
    variant: WorkspaceVariant,
    errors: List<string>,
}

export const SelectionPhase = {
    Default: (specs: Spec [], variant: WorkspaceVariant) : SelectionPhase => ({ 
        specs: specs, 
        kind: 'idle', 
        variant: variant,
        errors: List<string>(),
        name: Value.Default("Spec Name") 
    }),
    Updaters: {
        Core: {
            ...simpleUpdater<SelectionPhase>()("name"),
            ...simpleUpdater<SelectionPhase>()("errors")
        }
    },
    Operations: {
        tryParseJsonObject: (str: string): Record<string, unknown> | null => {
            try {
                if (str.charCodeAt(0) === 0xFEFF) {
                    str = str.slice(1);
                }
                const parsed = JSON.parse(str);
                if (parsed !== null && typeof parsed === "object" && !Array.isArray(parsed)) {
                    return parsed as Record<string, unknown>;
                }
            } catch(e: any) {
                return null;
            }
            return null;
        },
        handleZip: async (source: File | null): Promise<ValueOrErrors<{ path: string[]; content: string}[], string>> => {
            if(!source) 
                return ValueOrErrors.Default.throwOne("UploadZip source must be defined");
            if(!source.name.toLowerCase().endsWith(".zip")) 
                return ValueOrErrors.Default.throwOne("Please select a zip file");
            
            try {
                const buffer = await source.arrayBuffer();
                const zip: JSZip = await JSZip.loadAsync(buffer);
                const entries = Object.entries(zip.files) as [string, JSZipObject][];

                const files =
                    entries
                        .filter(([path, entry]) => !entry.dir) 
                
                console.log(entries.map(([path, e]) => ({ path, dir: e.dir })));
                
                const tasks = 
                    files.map(async ([path, entry]) => {
                        const content = await entry.async("string");
                        return {path, content};
                    })

                const all = await Promise.all(tasks);
                // const parsed =
                //     all.map(file =>
                //         ({...file, content: SelectionPhase.Operations.tryParseJsonObject(file.content)})
                //     );
                const valid = true //parsed.every(file => file.content != null)
                return ValueOrErrors.Default.return(all.map(file => ({
                        path: file.path.split("/"),
                        content: file.content
                    })))
                    // : ValueOrErrors.Default.throw(
                    //     List(["Uploading zipped file failed. The following files are not in expected format",
                    //         ...parsed.filter(file => file.content == null).map(file => file.path)]));
            }
            catch (err: unknown) {
                if (err instanceof Error) {
                    console.error("Real error:", err.message);
                }
                return ValueOrErrors.Default.throw(
                    List(["Uploading zipped file failed. The following files are not in expected format"])
                );
            }
        }
    }
}

export type SelectionPhaseForeignMutationsExpected = {
}

export type SelectionPhaseView = View<
    Maybe<SelectionPhase>,
    Maybe<SelectionPhase>,
    SelectionPhaseForeignMutationsExpected,
    {
    }
>;
