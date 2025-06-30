import { useEffect, useState } from "react";
import {IDETemplate, IDE, SpecEditor} from "playground-core";
import {IDELayout} from "./domains/ide/views/ide";
import "./IDE.css"
import SPEC from "../public/SampleSpecs/dispatch-person-config.json";
import {replaceWith} from "ballerina-core";

export const IDEApp = (props: {}) => {
    
    const [ide, setIDE] = useState(IDE.Default());
    
    useEffect(() => {
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