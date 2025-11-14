
import React, { useState } from 'react';
import styled from '@emotion/styled';
import {Ide, postCodegen, postVfs} from "playground-core";
import {BasicFun, SimpleCallback, Unit, Updater} from "ballerina-core";

type Props = { heroVisible: boolean, onSelection1: SimpleCallback<void>, onSelection2: BasicFun<Unit,void>};

export const Hero: React.FC<Props> = (props: Props) => {

    if(!props.heroVisible) return <></>;
    return (<div className="hero bg-base-200 min-h-screen">
        <div className="hero-content flex-col lg:flex-row">
            <img
                src="https://framerusercontent.com/images/d2NXyeDEEobl0VGSBndK8lteLU.png?scale-down-to=1024&width=2000&height=2040"
                className="max-w-sm rounded-lg shadow-2xl"
            />
            <div className="ml-12">
                <h1 className="flex items-center justify-between text-5xl font-bold text-green-950">
                    Ballerina IDE
                    <img
                        className="w-40 ml-4"
                        src="https://github.com/Ballerina-Org/ballerina/raw/main/docs/pics/Ballerina_logo-04.svg"
                        alt="Ballerina Logo"
                    />
                </h1>
                <div className="mt-2 flex w-full">
                    <p className="py-6">
                        Experiment with the spec to explore how it shapes your forms — all within a playground environment that validates your spec and provides a real-time, interactive preview.
                    </p>
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
                                        onClick={() => props.onSelection1() }>select</button>
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

