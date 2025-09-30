/*
*  in the current IDE state we either create a spec from:
*  - uploaded partial forms that later can be merged and displayed (great for reusing a library of forms and models)
*    -> this is called 'compose' mode
*  - uploaded (zipped) full forms that can be updated and exported back to zip 
*    but only one of them can be selected and displayed (great for explorations, tests and small adjustment)
*    -> this is called 'explore' mode
*  - manually creating the spec from scratch, the spec is initiated with a set of empty expected files 
*    which can be later merged and displayed
* 
*  All that is only for now to restrict and validate the UX accordingly, it will be most likely extended/mixed
*  later which for now is YAGNI
*/

export type DataEntry =
    | 'upload-manual'         // user starts from scratch (with predefined files) and creates a spec on the fly
    | 'upload-zip'     // user uploads zipped full forms -> it put the spec in the compose mode
    | 'upload-folder'; // user upload folder of partial forms -> it puts the spec in the select mode

export type SpecMode =
    | { mode: 'compose', entry: Extract<DataEntry, 'upload-folder'> }
    | { mode: 'explore', entry: Extract<DataEntry, 'upload-zip'> }
    | { mode: 'scratch', entry: Extract<DataEntry, 'upload-manual'> }

export type SpecOrigin = { origin: 'selected' | 'creating' }

//FIXME: reuse the current launcher(s) type ?
export type IdeLauncher = any
export type IdeEntity = any