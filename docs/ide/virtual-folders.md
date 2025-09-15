Specifications tend to be large, and being a text file (either json or a DSL code), can be hard to manage.
Splitting them to folders can address that problem and additionally make it easier to split the responsibilities and enable parallel development.

## starting the work

We either start the work on a specification from scratch or select the existing sub-elements from files.
And as long as the form engine gets the whole, merged specification (with an entry point related to the selected launcher),
the working environment that we load into the editor is always stuck to the current specification realm, that equals to the subfolder.

Typically the subfolder consists of a model (types) and forms related to the covered application realm specified by the subfolder name.
We can upload (mirror) the existing folder structure and work on a selected subfolder, but when we start from scratch, we have to create such a structure.