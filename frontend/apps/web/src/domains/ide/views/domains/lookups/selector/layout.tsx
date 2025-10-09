import React from "react";
import { Set } from "immutable";
import {BasicFun, Option} from "ballerina-core";

type Chosen = {
    sourceEntityName: string,
    targetLookupNames: Set<string> 
}

type Available = {
    entityNames: Set<string>,
    lookups: { name: string, fromEntity: string, toEntity: string }[]
}

//TODO: maybe ProgressiveAB approach instead?
export const SelectEntityAndLookups = (props: Available & { apply: BasicFun<Chosen, void>}): React.ReactElement => {
    const [chosen, setChosen] = React.useState<Option<Chosen>>(Option.Default.none())
    const [available, setAvailable] = React.useState<Available>(props)
    const availableLookups = 
        chosen.kind == "r" ? 
            <form>
            {available.lookups
                .filter(l => 
                    l.fromEntity == chosen.value.sourceEntityName)
                .map(l =>
                <input 
                    className="btn" 
                    type="checkbox" 
                    name="available-lookups" 
                    aria-label={l.name}
                    checked={available.lookups.map(lookup => lookup.name).includes(l.name)}
                    onChange={(e) => {
                        if(chosen.kind !== "r") return;
                        const next = chosen.value.targetLookupNames.has(l.name) ? chosen.value.targetLookupNames.delete(l.name) : chosen.value.targetLookupNames.add(l.name);
                        setChosen(Option.Default.some({...chosen.value, targetLookupNames: next}))
                    }}
                />
            )}
            <input className="btn btn-square" type="reset" value="×" onClick={(_) => setAvailable(props)}/>
            </form> : <></>
    const choseEntity = <div className="filter">
        <input className="btn filter-reset" type="radio" name="select-entity" aria-label="All"/>
        {available.entityNames.map(entity => (
            <input 
                className="btn" 
                type="radio" 
                name="select-entity" 
                aria-label={entity}
                checked={chosen.kind == "r" && chosen.value.sourceEntityName == entity }
                onChange={(e) => {
                    const next = 
                        chosen.kind == "l" ? 
                            { sourceEntityName: entity, targetLookupNames: Set([]) } 
                            : {...chosen.value, sourceEntityName: e.target.value};
                    setChosen(Option.Default.some(next))
                }}
            />
        ))}
    </div>
    return <fieldset className="fieldset bg-base-200 border-base-300 rounded-box w-xs border p-4">
            <legend className="fieldset-legend">Select entity & lookups</legend>
    
            <label className="label">Entity</label>
            {choseEntity}
    
            <label className="label">Lookups</label>
            {availableLookups}
    
            <button 
                disabled={chosen.kind == 'l' || chosen.value.targetLookupNames.size == 0}
                className="btn btn-neutral mt-4" 
                onClick={() => 
                    chosen.kind == 'r' 
                    && chosen.value.targetLookupNames.size > 0 
                    && props.apply(chosen.value)}>Apply</button>
        </fieldset>
}