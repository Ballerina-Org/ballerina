import React, {useEffect} from "react";
import {VscCircle, VscCircleFilled} from "react-icons/vsc";
import {CustomEntity, Job} from "playground-core";

export type JobProps = {
    trace: Job,
    item: number,
    from: number,
    entity: CustomEntity
}

export const JobElement= (props: JobProps): React.ReactElement => {
    const [counter, setCounter] = React.useState(5);
    useEffect(() => {
        const id = setInterval(() => {
            setCounter(v => counter > 0 ? v - 1 : 0);
        }, 1000);

        return () => clearTimeout(id);
    }, [counter]);
    
    let countdown =    
        counter < 0 || props.entity.status.kind != 'job' || props.entity.status.job.status.kind != 'processing' 
            ? <></> 
            :
                <span className="countdown font-mono text-xl">
                    <span style={{"--value":counter, "--digits":1}} aria-live="polite" aria-label={counter}>counter</span>
                </span>
    
    return <li className="w-48">   
        {props.item != 0 ? <hr className="bg-primary"/>: <></>}  
        <div className="timeline-start  flex flex-col items-center justify-center">
            {props.trace.status.kind === 'processing' 
                ? props.trace.status.processing.checkCount > 1 
                    ?<div className="indicator">
                        <span className="indicator-item badge badge-secondary">retry: {props.trace.status.processing.checkCount - 1}</span>
                        <button className="btn">{countdown}</button>
                    </div>
                    :<div>{countdown}</div>: <></>}
            <div>{props.trace.kind}</div>

        </div>
       
        <div className="timeline-middle">
            { 
                props.trace.status.kind == 'starting' 
                ? <VscCircle color='black' size={25} />
                    :
                    props.trace.status.kind != 'completed' 
                    ? <VscCircleFilled color="orange" size={25} /> 
                    : props.entity.status.kind == 'result' && props.entity.status.value.kind == 'errors' && (props.item == props.from - 1)
                        ?<VscCircleFilled color='red' size={25} /> :
                        <VscCircleFilled color='green' size={25} /> }
        </div>
        <div className="timeline-end timeline-box  flex flex-col items-center justify-center mt-3">            
            <div>{props.trace.status.kind}</div></div>
        {props.item != props.from - 1 ? <hr className="bg-primary"/>: <></>}
    </li>

}