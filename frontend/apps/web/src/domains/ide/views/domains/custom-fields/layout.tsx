import React from "react";
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

type CustomFieldsProps = { context: CustomFields, node: INode<Meta> , setState: BasicFun<Updater<Ide>, void> };

export const CustomFieldsTracker: CustomFieldsView = (props): React.ReactElement => {
    debugger
    const jobs = props.context.jobFlow.traces
    return props.context.visibility == ('fully-visible' as Visibility)
        && props.context.node.kind == "r" && CustomFields.Operations.isAvailable(props.context.node.value) ? <div className="card bg-base-100 w-full shadow-sm">
        <div className="card-body">
            <h2 className="card-title">Custom fields</h2>
            <p>Evaluate free key value code</p>
            <p>Do I have a node: {props.context.folder.kind == "r" ? 'yes':'no'}</p>
            <ul className="timeline">
                {jobs.map( j=> (
                    <li>
                
                        <div className="timeline-middle">
                            <VscCircle size={20} />
                        </div>
                        <div className="timeline-end timeline-box">{j.kind}:{j.job.kind}</div>
                        <hr className="bg-primary" />
                    </li>
                ))}
                {/*<li>*/}
                {/*    <div className="timeline-middle">*/}
                {/*        <VscCircle size={20} />*/}
                {/*    </div>*/}
                {/*    <div className="timeline-end timeline-box">Type Checking</div>*/}
                {/*    <hr className="bg-primary" />*/}
                {/*</li>*/}
                {/*<li>*/}
                {/*    <hr />*/}
                {/*    <div className="timeline-middle">*/}
                {/*        <VscCircle size={20} />*/}
                {/*    </div>*/}
                {/*    <div className="timeline-end timeline-box">iMac</div>*/}
                {/*    <hr className="bg-primary" />*/}
                {/*</li>*/}
                {/*<li>*/}
                {/*    <hr />*/}
                {/*    <div className="timeline-middle">*/}
                {/*        <VscCircleFilled size={20} />*/}
                {/*    </div>*/}
                {/*    <div className="timeline-end timeline-box">iPod</div>*/}
                {/*    <hr />*/}
                {/*</li>*/}
                {/*<li>*/}
                {/*    <hr />*/}
                {/*    <div className="timeline-middle">*/}
                {/*        <VscCircle size={20} />*/}
                {/*    </div>*/}
                {/*    <div className="timeline-end timeline-box">iPhone</div>*/}
                {/*    <hr />*/}
                {/*</li>*/}
                {/*<li>*/}
                {/*    <hr />*/}
                {/*    <div className="timeline-middle">*/}
                {/*        <VscCircle size={20} />*/}
                {/*    </div>*/}
                {/*    <div className="timeline-end timeline-box">Apple Watch</div>*/}
                {/*</li>*/}
            </ul>
            <div className="card-actions justify-end">
                <button 
                    className="btn btn-primary"
                    onClick={() => {
                        props.setState(
                            CustomFields.Updaters.Core.folder(
                                replaceWith(props.context.node)
                                
                            ).then(CustomFields.Updaters.Template.start()))}}
                >Start</button>
            </div>
            <p>status: {props.context.jobFlow.kind}</p>
            <p>Errors:</p>
            <ul>{props.context.errors.map(e => (<li>{e}</li>))}</ul>
            
        </div>
    </div>:<></>
}