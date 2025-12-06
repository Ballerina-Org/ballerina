import React from "react";
import {CustomFieldsView} from "playground-core";
import {JobElement} from "./job.tsx";

export const CustomFieldsTracker: CustomFieldsView = (props): React.ReactElement => {
    const jobs = props.context.trace
    return  <div className="card bg-base-100 w-full shadow-sm">
        <div className="card-body">
            <h2 className="card-title">Custom Entity</h2>
            <p>Evaluate custom fields with the code</p>
            <ul className="timeline mb-7 ">
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