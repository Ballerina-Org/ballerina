import {Debounced, Option, replaceWith, simpleUpdater, Synchronized, Updater, Value} from "ballerina-core";
import { Map } from "immutable";
import {FlatNode} from "./upload/model";
import {Ide} from "../../../state";
import {LockedSpec} from "../state";
export type TopLevelKey = "types" | "forms" | "apis" | "launchers" | "typesV2" | "schema" | "config";

export type JsonPrimitive = string | number | boolean | null;
export type JsonValue = JsonPrimitive | JsonValue[] | { [k: string]: JsonValue };
export type JsonSection = Record<string, JsonValue>;
export type KnownSections = Partial<Record<TopLevelKey, JsonSection>>;


export type VfsWorkspace = {
    //root: FlatNode;                                   
    
    //files: VirtualJsonFile[];      
    nodes: FlatNode;
    merged: Option<KnownSections>;    
    selectedFolder: Option<FlatNode>;
    selectedFile: Option<FlatNode>;
};

export const VirtualFolders = {
    Updaters: {
        Template: {
            selectedFileContent: (value: string) => Updater<Ide>(ide => {
                if(ide.phase == 'locked' && ide.locked.virtualFolders.selectedFile.kind == "r"){
                    const vfs = ide.locked.virtualFolders
                    const node = ide.locked.virtualFolders.selectedFile.value
                    const next: VfsWorkspace = {...vfs, selectedFile: Option.Default.some({...node, metadata: {...node.metadata, content: value}})};
                    return LockedSpec.Updaters.Core.vfs(replaceWith(next))(ide)
                }
                return ide
            })
        }
    },
    Operations: {
        buildWorkspaceFromRoot(origin: 'existing' | 'create', nodes: FlatNode): VfsWorkspace {
         
            return {
                nodes, 
                merged: Option.Default.none(),
                selectedFile: nodes.metadata?.isLeaf && nodes.children?.length || 0 > 0 ? Option.Default.some(nodes.children![0]) : Option.Default.none(),
                selectedFolder: nodes.metadata?.isLeaf ? Option.Default.some(nodes) : Option.Default.none(),
            }
        },
        formatBytes: (bytes?: number) => {
            if (bytes == null || !Number.isFinite(bytes)) return "";
            const k = 1024;
            const sizes = ["B", "KB", "MB", "GB", "TB"];
            const i = Math.max(0, Math.floor(Math.log(bytes) / Math.log(k)));
            const n = bytes / Math.pow(k, i);
            return `${n.toFixed(n >= 10 || i === 0 ? 0 : 1)} ${sizes[i]}`;
        },
    }
}

export const VfsWorkspace = {
    Updaters: {
        Core: {
            ...simpleUpdater<VfsWorkspace>()("selectedFile"),
            ...simpleUpdater<VfsWorkspace>()("merged"),
            ...simpleUpdater<VfsWorkspace>()("selectedFolder"),
                    
        }
    }
}