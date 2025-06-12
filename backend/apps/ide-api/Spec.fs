namespace IDEApi

open System

type Spec =
    { Name: string }

    member this.length =
        this.Name.Length
                
// type SpecRequest = { specBody: string }
// type SpecValidationResult<'T> = { isValid: bool } //; Payload: 'T }