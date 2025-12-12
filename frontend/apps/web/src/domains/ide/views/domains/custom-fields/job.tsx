import React, {useEffect} from "react";
import {VscCircle, VscCircleFilled} from "react-icons/vsc";
import {CustomEntity, Job} from "playground-core";

export type JobProps = {
    trace: Job,
    item: number,
    from: number,
    entity: CustomEntity
}

    export const Counter = (props: Job): React.ReactElement => {   
    
        const [counter, setCounter] = React.useState(props.status.kind == 'processing' ? props.status.processing.checkInterval/1000 : 5 );
        useEffect(() => {
            const id = setInterval(() => {
                setCounter(v => counter > 0 ? v - 1 : 0);
            }, 1000);
    
            return () => clearTimeout(id);
        }, [counter, props.status.kind == 'processing' && props.status.processing.checkInterval]);
        
        if(!(props.status.kind == 'processing')) return <></>
        
        if(props.status.processing.checkCount > 0) {
            return <div className="indicator">
                <span
                    className="indicator-item badge badge-secondary">retry: {props.status.processing.checkCount - 1}</span>
                <button className="btn">                <span className="countdown font-mono text-xl">
                        <span style={{"--value": counter, "--digits": 1}} aria-live="polite"
                              aria-label={counter}>counter</span>
                    </span></button>
            </div>
        }
        return <span className="countdown font-mono text-xl">
                        <span style={{"--value": counter, "--digits": 1}} aria-live="polite"
                              aria-label={counter}>counter</span>
                    </span>
        
    }
    export const JobElement= (props: JobProps): React.ReactElement => {

    return <li className="w-48">   
        {props.item != 0 ? <hr className="bg-primary"/>: <></>}  
        <div className="timeline-start  flex flex-col items-center justify-center">
            {props.entity.status.kind == 'job' && props.entity.status.job.status.kind == 'processing' && <Counter {...props.trace} />}
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
        <div className="timeline-end timeline-box  flex flex-col items-center justify-center mt-3">            
            <div>{props.trace.status.kind}</div></div>
        {props.item != props.from - 1 ? <hr className="bg-primary"/>: <></>}
    </li>

}