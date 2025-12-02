import React, {useEffect} from "react";
import {VscCircle, VscCircleFilled} from "react-icons/vsc";
import {JobTrace} from "playground-core/ide/domains/phases/custom-fields/domains/job/state.ts";

export type JobProps = {
    trace: JobTrace,
    item: number,
    from: number,
    status: string
}

export const Job= (props: JobProps): React.ReactElement => {
    const [counter, setCounter] = React.useState(5);
    useEffect(() => {
        if (counter <= -1) return;

        const id = setTimeout(() => {
            setCounter(v => v - 1);
        }, 1000);

        return () => clearTimeout(id);
    }, [counter]);
    
    let countdown =    counter < 0 || props.status == 'finished' ? <></> :<span className="countdown font-mono text-xl">
<span style={{"--value":counter, "--digits":1}} aria-live="polite" aria-label={counter}>counter</span>
</span>
    
    return <li className="w-48">   {props.item != 0 ? <hr className="bg-primary"/>: <></>}  
        <div className="timeline-start  flex flex-col items-center justify-center">
            <div>{countdown}</div>
            <div>{props.trace.job.kind}</div>

        </div>
       
        <div className="timeline-middle">
            { props.trace.kind != 'completed' ? <VscCircle size={25} /> : <VscCircleFilled color='green' size={25} /> }
        </div>
        <div className="timeline-end timeline-box  flex flex-col items-center justify-center mt-3">            
            <div>{props.trace.kind}</div></div>
        {props.item != props.from - 1 ? <hr className="bg-primary"/>: <></>}
    </li>

}