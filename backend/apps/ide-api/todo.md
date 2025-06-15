# Todo (✅/❌/🟡)

## backend

🟡 consider using minimal API (currently I don't see a template for F#, there is only C# version)

❌ configure CORS properly

## frontend

❌ move styles to a dedicated file (.styled.ts), they span too many lines

❌ continue with own raw json editor (react json-editor should be only tmp)

## milestones
### [15/6]

🟢 [13/6] as a user, I can edit a spec as raw json with basic syntax highlighting 

    💬 a very simple raw editor with syntax highlighting has been implemented, but it only good for now to display very small json's, 
        making it more usable will require a lot of work

    📌 json-edit-react is used temporarily to proceed with further development

✅ [13/6] as a user, I can run a spec - this sets it as active

    💬 currently we don't have a repository of existing specs, so the active spec is the only one used selected from SampleSpecs

✅ [13/6] specs are validated as the user types/saves and eventual errors are shown

    💬 tested by removing a field from spec and seeing backend errors

✅ [13/6] when a valid spec is run, random values are seeded for the models

    💬 similarly to DispatcherFormsApp.tsx

✍️ DispatcherFormsApp.tsx is build on onetime coroutines, so it will not be executed on a second and further forms runs (after editing),
   will be addressed in the next milestone
    