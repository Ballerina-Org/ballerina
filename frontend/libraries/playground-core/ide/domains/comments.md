📌 I expect this file is removed after some several POC's validations and code review's,
and is intended to keep the initial decisions explained to the peers

[1 💬]: layout domain is loosely coupled:
    
    - spec-editor and spec-runner are siblings to each other but their respective UI elements may be present also in a layout (yet another sibling)
        
        - like spec-runner action button may somehow affect the appearance of spec-editor and vice-versa), if it wasn't a sibling but nested in respective subdomains (children), then it would be possible to communicate between them