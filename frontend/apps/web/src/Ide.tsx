import {useEffect,useState } from "react";
import {IdeTemplate, Ide} from "playground-core";
import {IdeLayout} from "./domains/ide/views/layout.tsx";
import "./Ide.css"
import SPEC from "../public/SampleSpecs/dispatch-person-config.json";
import {replaceWith} from "ballerina-core";

export const IdeApp = (props: {}) => {

    const [ide, setIde] = useState(Ide.Default());

    useEffect(() => {
    }, []);

    return (
        <div className="IDE">
            <IdeTemplate
                context={ide}
                setState={setIde}
                foreignMutations={{}}
                view={IdeLayout}
            />
        </div>)
}