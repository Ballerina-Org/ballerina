import { useEffect, useState } from "react";
import {IDETemplate, IDE, RawJsonEditor} from "playground-core";
import {IDELayout} from "./domains/ide/views/ide-layout.tsx";
import "./IDE.css"
import SPEC from "../public/SampleSpecs/dispatch-person-config.json";
import {replaceWith} from "ballerina-core";

export const IDEApp = (props: {}) => {
    
    const [ide, setIDE] = useState(IDE.Default([]));
    
    useEffect(() => {
        setIDE(IDE.Updaters.Core.rawEditor(RawJsonEditor.Updaters.Core.inputString(replaceWith({value: JSON.stringify(SPEC)}))))
    }, []);
    
    return (
        <div className="IDE">
            <IDETemplate
                context={ide}
                setState={setIDE}
                foreignMutations={{}}
                view={IDELayout}
            />
        </div>)
}