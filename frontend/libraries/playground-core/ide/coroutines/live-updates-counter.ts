import {Co} from "./builder";
import {
    replaceWith,
    Value, Option
} from "ballerina-core";
import {Ide} from "../state";
import {listSpecs} from "../api/specs"

export const liveUpdatesCounter =
    
        Co.Repeat(
            Co.Seq([
                Co.GetState().then(context =>
                 
                    Co.SetState(
                        Ide.Updaters.Core.liveUpdates(
                            replaceWith(
                                context.liveUpdates.kind == "r" ?
                                    Option.Default.some(context.liveUpdates.value - 1)
                                    : Option.Default.none()
                            )
                        )
                    )), 
                Co.Wait(1000), 
                Co.GetState().then(context =>
                    Co.Seq([
                        Co.Await(() =>
                            context.liveUpdates.kind == "r" && context.liveUpdates.value <= 0 ?   
                                Ide.Operations.validateV1(context) : Promise.resolve((context: Ide) => context)
                            , x => {} 
                        
                        ),
                        Co.SetState(Ide.Updaters.Core.liveUpdates(
                            replaceWith(
                                context.liveUpdates.kind == "r" && context.liveUpdates.value <= 0 ?
                                    Option.Default.some(7)
                                    : context.liveUpdates
                            )
                        ))
                    ])
                ),


]
            )
        )