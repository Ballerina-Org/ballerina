import {simpleUpdater, ValueOrErrors} from "ballerina-core";
import {List} from "immutable";
import JSZip, { JSZipObject } from "jszip";
import {getSpec} from "../../api/specs";
import {VirtualFolders} from "../locked/vfs/state";
import {Ide} from "../../state";

export type ChooseStep = 'default' | 'upload-started' | 'upload-in-progress' | 'upload-finished';

export const ChooseState = {
    Operations: {
        tryParseJsonObject: (str: string): Record<string, unknown> | null => {
            try {
                const parsed = JSON.parse(str);
                if (parsed !== null && typeof parsed === "object" && !Array.isArray(parsed)) {
                    return parsed as Record<string, unknown>;
                }
            } catch {
                return null;
            }
            return null;
        },
        handleZip: async (source: File | null): Promise<ValueOrErrors<{ path: string[]; content: Record<string, unknown> }[], string>> => {
            if(!source) return ValueOrErrors.Default.throwOne("UploadZip source must be defined");
            if (!source.name.toLowerCase().endsWith(".zip")) 
                return ValueOrErrors.Default.throwOne("Please select a zip file");
            const buffer = await source.arrayBuffer();
            const zip: JSZip = await JSZip.loadAsync(buffer);
            const entries = Object.entries(zip.files) as [string, JSZipObject][];

            const tasks = 
                entries
                .filter(([path, entry]) => !entry.dir && path.endsWith(".json"))
                    
                .map(async ([path, entry]) => {
                    const content = await entry.async("string");
                    return { path, content };
                })

            const all = await Promise.all(tasks);
            const parsed = 
                all.map(file =>
                    ({...file, content: ChooseState.Operations.tryParseJsonObject(file.content)})
            );

            const valid = parsed.every(file => file.content != null)
            return valid ? 
                ValueOrErrors.Default.return(parsed.map( file => ({path: file.path.split("/"), content: file.content!}))) 
                : ValueOrErrors.Default.throw(
                    List(["Uploading zipped file failed. The following files are not in expected format", 
                        ...parsed.filter(file => file.content == null).map(file => file.path)]));
        }
    }
}