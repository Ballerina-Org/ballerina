import {VirtualFolderNode, VirtualJsonFile} from "playground-core";
import React from "react";
import {VscFile, VscFolder} from "react-icons/vsc";


export const breadcrumbs = (file: VirtualFolderNode) => {
    if(file.kind == 'file') return <></>

    
    return (<div className="breadcrumbs text-sm">
        <ul>
            {
                file.path.split("/").map((part, i) => 
                    <li><a><VscFolder size={15}/>{part}</a></li>)
                
            }
    
        </ul>
    </div>)}