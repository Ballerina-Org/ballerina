import {Co} from "./builder";
import {
    replaceWith,
    Value, Option, Updater
} from "ballerina-core";
import {Ide} from "../state";
import {listSpecs} from "../api/specs"
import {Bridge} from "../domains/bridge/state";



export const liveUpdatesCounter =

    Co.Repeat(
        Co.Seq([
            Co.While(context => context[0].liveUpdates.kind == "r" && context[0].liveUpdates.value > 0,
                Co.Seq([
                    
                    //Co.Wait(1000),
                    Co.GetState().then(context =>
                        Co.SetState(
                            Ide.Updaters.Core.liveUpdates(
                                replaceWith(
                                    context.liveUpdates.kind == "r" ?
                                        Option.Default.some(context.liveUpdates.value - 1)
                                        : Option.Default.none()
                                )
                            )
                        )
                    )
                ])
            ),

            Co.GetState().then(context =>
                Co.Await(()=> Ide.Operations.validateV1(context), x => {})
                .then( (s) => 
                    Co.SetState(s.kind == "l" ? s.value : replaceWith(context))
                   ))
            ,
            // Co.GetState().then((context: Ide) => 
            //   Co.SetState(
            //
            //          Ide.Updaters.Core.bridge(Bridge.Updaters.Template.setV1Body(
            //                context.bridge.bridge.left.left.specBody
            //            ))
            //
            //   )
            // ),
            Co.SetState(Ide.Updaters.Core.liveUpdates(
                replaceWith(Option.Default.some(0))

            ).then(Ide.Updaters.Core.bridge(Bridge.Operations.undirty()))),

        ])
    )