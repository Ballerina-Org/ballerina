
import React, { useState } from 'react';
import styled from '@emotion/styled';
import {Ide, postCodegen, postVfs} from "playground-core";
import {BasicFun, Updater} from "ballerina-core";

type Props = Ide & { setState: BasicFun<Updater<Ide>, void> };

export const Hero: React.FC<Props> = (props: Props) => {

    if(!props.heroVisible) return <></>;
    return (<div className="hero bg-base-200 min-h-screen">
        <div className="hero-content flex-col lg:flex-row">
            <img
                src="https://framerusercontent.com/images/d2NXyeDEEobl0VGSBndK8lteLU.png?scale-down-to=1024&width=2000&height=2040"
                className="max-w-sm rounded-lg shadow-2xl"
            />
            <div className="ml-12">
                <h1 className="text-5xl font-bold text-green-950">Ballerina Ide</h1>
                <div className="mt-5 flex w-full">
                    <p className="py-6">
                        Experiment with the domain by writing a spec and seeing it in action in a playground environment that offers a realtime interactive preview.
                    </p>
                    <img className="w-40" src="https://github.com/Ballerina-Org/ballerina/raw/main/docs/pics/Ballerina_logo-04.svg" />
                </div>
                <div className="w-full flex flex-col gap-4">
                    {/* top two cards side by side on lg, stacked on sm */}
                    <div className="flex flex-col lg:flex-row gap-4">
                        <div className="card bg-base-100 w-full lg:w-1/2 shadow-sm">
                            <div className="card-body">
                                <h2 className="card-title">Project Room</h2>
                                <p>Explore, modify and test existing specifications in an interactive way</p>
                                <div className="card-actions justify-end">
                                    <button 
                                        className="btn bg-green-950 text-white hover:bg-green-600"
                                        onClick={() => props.setState(
                                            Ide.Updaters.Phases.hero.toBootstrap()
                                                .then(Ide.Updaters.CommonUI.toggleHero()))}>select</button>
                                </div>
                            </div>
                        </div>

                        <div className="card bg-base-100 w-full lg:w-1/2 shadow-sm">
                            <div className="card-body">
                                <h2 className="card-title">Data Driven</h2>
                                <p>Compose a new spec from the standard library of predefined specification chunks</p>
                                <div className="card-actions justify-end">
                                    <button className="btn btn-primary" disabled={true}>select</button>
                                </div>
                            </div>
                        </div>
                    </div>
                    
                    {/*<div className="flex justify-end">*/}
                    {/*    <button*/}
                    {/*        className="btn btn-neutral btn-outline"*/}
                    {/*        onClick={() => props.setState(*/}
                    {/*            Ide.Updaters.Phases.hero.toBootstrap()*/}
                    {/*                .then(Ide.Updaters.CommonUI.toggleHero()))}*/}
                    {/*    >*/}
                    {/*        or start from scratch*/}
                    {/*    </button>*/}
                    {/*</div>*/}
                </div>




            </div>
        </div>
    </div>)
}

