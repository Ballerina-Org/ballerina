import React from 'react'
import { Ide} from "playground-core";
import {List} from "immutable";

type Props = Ide;

export const ErrorsPanel: React.FC<Props> = (props: Props) => {

    const errors: List<string> =
        props.phase.kind === 'locked' ? props.phase.locked.errors :
            props.phase.kind === 'hero' ? props.phase.hero.errors :
                props.phase.kind === 'selection' ? props.phase.selection.errors :
                    props.phase.kind === 'bootstrap' ? props.phase.bootstrap.errors :
                        List();
    
    return (<div className="no-radius w-full mx-auto">
        <div className="inset-0 top-0 z-20 m-0 p-0">
            <div className="space-y-2  w-full">
                {
                    errors.map(error =>
                        (<pre
                            data-prefix="6"
                            className="m-0 pl-3 bg-rose-300 text-warning-content">
                                                        {error}
                                                    </pre>))
                }
            </div>
        </div>
        <div className="relative w-120 mx-auto rounded-lg overflow-hidden">
            <img style={{opacity: errors.size == 0? 0.8 : 0.2}}
                 src="https://framerusercontent.com/images/umluhwUKaIcQzUGEWAe9SRafnc4.png?width=1024&height=1024"
                 alt="Descriptive alt"
                 className="w-full object-cover rounded-lg"/>
            {errors.size == 0 && <div className="absolute inset-0 grid place-items-center">
                <span className="px-3 py-1 rounded-full bg-black/60 text-white text-sm">No issues so far</span>
            </div>}
        </div>
    </div>)
}

