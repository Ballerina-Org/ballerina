import React from "react";
import {CustomEntity, CustomFieldsView} from "playground-core";
import {JobElement} from "./job.tsx";
import {Updater} from "ballerina-core";
import {getDocument} from "playground-core/ide/api/documents.ts";

export const CustomFieldsTracker: CustomFieldsView = (props): React.ReactElement => {

    const jobs = props.context.trace
    return  <div className="card bg-base-100 w-full shadow-sm">
        <div className="card-body">
            <h2 className="card-title">Custom Entity</h2>
            <p>Evaluate custom fields with the code</p>
            <select defaultValue="Pick a color" className="select appearance-none"
                    onChange={async (e) => {
                        const content = await getDocument(e.target.value);
                        if(content.kind == "errors") return CustomEntity.Updaters.Core.fail(content.errors);

                        const u = CustomEntity.Updaters.Template.selectDocument(content.value.content,e.target.value);

                        props.setState(u);
                    }}
            >
                <option disabled={true} value={"Pick a color"}>Select document</option>
                {props.context.documents.available.map(doc =><option key={doc.id} value={doc.id}>{doc.name}</option>)}
            </select>
            <ul className="timeline h-64">
                {jobs.map( (j,i)=> (<JobElement 
                    entity={props.context} 
                    trace={j} 
                    item={i} from={jobs.length} />))}

            </ul>
            {
                props.context.status.kind == 'result' 
                && props.context.status.value.kind == 'errors' 
                && <> <p>Errors:</p>
                <ul>{props.context.status.value.errors.map(e => (<li>{e}</li>))}</ul></> }
        </div>
    </div>
}