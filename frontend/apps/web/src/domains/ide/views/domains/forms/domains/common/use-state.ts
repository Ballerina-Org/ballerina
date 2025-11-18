import React from "react";

export function useStateWithEffectAndCondition<T>(
    init: T,
    condition: boolean
): [T, React.Dispatch<React.SetStateAction<T>>] {
    const [value, setValue] = React.useState(init);
    React.useEffect(() => {
        if (condition) setValue(init);
    }, [init, condition]);
    return [value, setValue];
}
