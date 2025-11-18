import {useEffect,useState } from "react";
import {IdeTemplate, Ide} from "playground-core";
import "./Ide.css"
import {Unit, unit} from "ballerina-core";
import {IdeLayout} from "./domains/ide/views/domains/layout/layout.tsx";

export const IdeApp = (props: Unit) => {

    const [ide, setIde] = useState(Ide.Default());

    useEffect(() => {
    }, []);
    
    return (
        <div className="IDE">
            <IdeTemplate
                context={ide}
                setState={setIde}
                foreignMutations={unit}
                view={IdeLayout}
            />
        </div>)
}