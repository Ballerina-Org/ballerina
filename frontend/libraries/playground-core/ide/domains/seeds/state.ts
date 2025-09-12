
/*
* entity going from backend for the Forms will be converter to the forms format
* but requested directly from the store will have a full format
*/

export type Seeds = {
    entities: any[],
    lookups: any[]
}

export type FormsSeedEntity = {
    id: string,
    value: any
}