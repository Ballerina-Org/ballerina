import React, {useEffect} from "react";
import {Dropdown} from "../layout/dropdown.tsx"
import {
    FlatNode,
    getSpec,
    Ide,
    WorkspaceState, CustomFields, Meta, INode, CustomFieldsView
} from "playground-core";
import {BasicFun, replaceWith, Updater, Option, Value, Visibility} from "ballerina-core";
import {SelectionPhase} from "playground-core/ide/domains/phases/selection/state.ts";
import {VscCircle, VscCircleFilled} from "react-icons/vsc";
import {Job} from "./job.tsx";

export const CustomFieldsTracker: CustomFieldsView = (props): React.ReactElement => {
    const [counter, setCounter] = React.useState(5);
    useEffect(() => {
        if (counter <= -1) return;

        const id = setTimeout(() => {
            setCounter(v => v - 1);
        }, 1000);

        return () => clearTimeout(id);
    }, [counter]);
    const jobs = props.context.jobFlow.traces
    return props.context.visibility == ('fully-visible' as Visibility)
        && props.context.provider.hasNode()? <div className="card bg-base-100 w-full shadow-sm">
        <div className="card-body">
            <h2 className="card-title">Custom fields</h2>
            <p>Evaluate free key value code</p>
            <ul className="timeline mb-7 ">
                {jobs.map( (j,i)=> (<Job status={props.context.jobFlow.kind} trace={j} item={i} from={jobs.length} />))}

            </ul>

            <div className="card-actions justify-end">
                <button
                    className="btn btn-primary"
                    onClick={() => {
                        props.setState(CustomFields.Updaters.Template.start(props.context.provider))}}
                >Start</button>
                <button
                    className="btn btn-warning"
                    onClick={() => props.setState(CustomFields.Updaters.Template.update(props.context.provider))}
                >Apply</button>
                <button
                    className="btn btn-info"
                    onClick={() => {}}
                >Restart</button>
            </div>
            {props.context.errors.size > 0 && <>            <p>Errors:</p>
                <ul>{props.context.errors.map(e => (<li>{e}</li>))}</ul></> }

     {
            props.context.jobFlow.kind == 'finished' 
            && props.context.jobFlow.result.kind == "value" 
            && <strong>{JSON.stringify(props.context.jobFlow.result.value.evidence)}</strong>
        }
            
        </div>
    </div>:<></>
}