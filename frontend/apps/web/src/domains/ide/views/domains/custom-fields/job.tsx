import React, {useEffect} from "react";
import {VscCircle, VscCircleFilled} from "react-icons/vsc";
import {CustomEntity, Job, JobStatus} from "playground-core";

export type JobProps = {
    trace: Job,
    item: number,
    from: number,
    entity: CustomEntity
}

function isProcessingStatus(
    status: JobStatus
): status is Extract<JobStatus, { kind: "processing" }> {
    return status.kind === "processing";
}

export const Counter = (props: Job): React.ReactElement => {   

    const [counter, setCounter] = React.useState(props.status.kind == 'processing' ? props.status.processing.checkInterval/1000 : 5 );

    useEffect(() => {
        const id = setInterval(() => {
            setCounter(v => counter > 0 ? v - 1 : 0);
        }, 1000);

        return () => clearInterval(id);
    }, [counter, props.status.kind == 'processing' && props.status.processing.checkInterval]);

    useEffect(() => {
        if(isProcessingStatus(props.status))
            setCounter(props.status.processing.checkInterval/1000);

        return () => {}
    }, [isProcessingStatus(props.status) && props.status.processing.checkCount]);
    
    if(!(props.status.kind == 'processing')) return <></>
    
    const element =             
        <div className="flex flex-col items-center justify-center p-2 bg-neutral rounded-box text-neutral-content mb-3">
        <span className="countdown font-mono text-2xl">
          <span style={{"--value":counter} /* as React.CSSProperties */ } aria-live="polite" aria-label={counter}>counter</span>
        </span>
            sec
        </div>
    
    if(props.status.processing.checkCount > 0) {
        return <div className="indicator">
            <span className="indicator-item badge badge-warning">retry: {props.status.processing.checkCount}</span>
            {element} </div>
    }
    return element
    
}
export const JobElement= (props: JobProps): React.ReactElement => {

return <li className="w-48">   
    {props.item != 0 ? <hr className="bg-primary"/>: <></>}  
    <div className="timeline-start  flex flex-col items-center justify-center mb-3">
        <Counter {...props.trace} />
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
                    ? <VscCircleFilled color='red' size={25} /> :
                    <VscCircleFilled color='green' size={25} /> }
    </div>
    <div className="timeline-end timeline-box  flex flex-col items-center justify-center">            
        <div>{props.trace.status.kind}</div></div>
    {props.item != props.from - 1 ? <hr className="bg-primary"/>: <></>}
</li>

}