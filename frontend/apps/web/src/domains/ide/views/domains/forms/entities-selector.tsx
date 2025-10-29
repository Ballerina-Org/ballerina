import React, { useState } from "react";
import {Option} from "ballerina-core";

// export const EntitiesSelector = (props: {
//     options: string[];
//     onChange?: (string: any) => void;
// }) => {
//     const [selected, setSelected] = useState<any>(Option.Default.none());
//
//     const handleChange = (value: string) => {
//         setSelected(Option.Default.some(value));
//         props.onChange?.(value); 
//     };
// 
//     return (
//         <div className="join">
//            {props.options.map((opt) => 
//                (<input onChange={() => 
//                    handleChange(opt)} 
//                        className="join-item btn" 
//                        type="radio" 
//                        name="entities" 
//                        aria-label={opt} 
//                        checked={selected.kind == "r" && selected.value === opt
//                } />))}
//         </div>
//     );
// };
