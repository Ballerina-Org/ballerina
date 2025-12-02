import React, {useEffect} from "react";
import {VscCircle, VscCircleFilled} from "react-icons/vsc";
import {JobTrace} from "playground-core/ide/domains/phases/custom-fields/domains/job/state.ts";
import {SimpleCallback} from "ballerina-core";

type Props = {
    content: string,
    enabled: boolean,
    getContent: SimpleCallback<string>,
}

export const Document = (props: Props): React.ReactElement => {
    const [content, setContent] = React.useState(props.content);
    const [enabled, setEnabled] = React.useState(props.enabled);
    
    return <div className="w-full">
            <fieldset className="fieldset bg-base-100 border-base-300 rounded-box w-64 border p-4">
                <legend className="fieldset-legend">Simulate document</legend>
                <label className="label">
                    <input type="checkbox" defaultChecked={enabled} className="toggle" onChange={(_) => setEnabled(!enabled)} />
                    {enabled ? "enabled" : "disabled"}
                </label>
            </fieldset>
            <textarea 
                disabled={!enabled} 
                className="textarea h-[80vh] w-full" 
                placeholder="raw document"
                onChange={(e) => setContent(e.target.value)}
            >{content}</textarea>
            <button className="btn btn-wide" onClick={(_) => props.getContent(content)}>Send</button>
        </div>

}