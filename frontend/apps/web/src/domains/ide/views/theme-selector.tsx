import React, {Dispatch, SetStateAction} from "react";

const themes = [
    "wireframe","fantasy","winter","lofi","dark","dracula",
    "bumblebee","emerald","halloween","retro","cyberpunk","abyss","sunset","dim","business","luxury","black","pastel","aqua","forest"
]

export const Themes = {
    dropdown: (theme: string, setTheme: Dispatch<SetStateAction<string>>) =>  
        <details>
            <summary>{theme}</summary>
            <ul className="menu dropdown-content bg-base-100 rounded-box z-1 w-52 p-2 shadow-sm">
                {
                    themes.map(theme => 
                        (
                            <li>
                                <a onClick={(_) => setTheme(theme)}>
                                    {theme}
                                </a>
                            </li>
                        )
                    )
                }
            </ul>
        </details>
}