

// type LauncherAndEntityProps = ProgressiveAB<IdeLauncher, IdeEntity> & { setState: BasicFun<Updater<Ide>, void> };
//
// export const LauncherAndEntity = (props: LauncherAndEntityProps): React.ReactElement => {
//     switch (props.kind) {
//         case "selectA":
//             return <LauncherSelector
//                 onChange={async (value: any) => 
//                     props.setState(
//                         LockedSpec.Updaters.Step.selectLauncher(value)
//                     )
//                 }
//                 options={props.options}
//             />
//         case "selectB":
//             return <EntitiesSelector
//                 onChange={async (value: any) => 
//                     props.setState(
//                         LockedSpec.Updaters.Step.selectEntity(props.a, value)
//                     )
//                 }
//                 options={props.options}
//             />
//         default: {
//             return <div/>;
//         }
//     }
// }
