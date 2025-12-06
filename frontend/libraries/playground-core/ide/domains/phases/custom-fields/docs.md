# Custom Entity domain

Custom Entity domain provides UI support for changing the Ballerina code into the Value<'typeValue, 'valueExt>
that is displayed in the Form Engine and send to the underlying storage.

Currently located in the IDE phases folder (IDE is split on well-defined, isolated steps called phases)
but it is well isolated and can be moved/reused elsewhere with almost no effort.

It can be described as Workspace -> Value.
Workspace is basically a collection (tree Node) of all uploaded files (.json & .bl)
but the actual workspace is the selected file with indirect info about its siblings in its folder.

As a Custom Entity editor is used to leverage the Value generation that is next displayed in the form,
the workspace consists of the Spec json files, Ballerina (.bl) files and, for now, the prompt (.txt).

Ultimately we want to work with the document we do analyze (invoice, etc),
until all the infrastructure pieces are there we use the prompt file to both fake the document and ask
question (prompt) against it. Right now it is for the sole purpose to demonstrate the capabilities, at the 
end the prompt part will disappear from UI and hidden by the automated backend task.

Going back to Workspace -> Value.
Custom Entity domain uses the workspace via collect() and prompt() in the `data-provider` domain
- collect: gathers data from Ballerina files into a single container needed by the TypeChecking job
- prompt: obtains the prompt text along with a faked document structure. The content is added as a 
prefabricated `let prompt = ...` Ballerina code and inserted before the constructor.bl part.
After the current phase, the document with the prompt will most likely be inserted by the backend
into the type constructor (as a so called `curated context`)
All of that content required to start the first job (typechecking), is put via data-provider in the
'start' function in the Template Updater of the core custom entity repo.
If Custom Entity UI is supposed to be used elsewhere and gather the required data differently (api's)
then only the new data provider with collect() should be implemented.
`CustomEntity.Default` factory remains parameterless

# Coroutine
