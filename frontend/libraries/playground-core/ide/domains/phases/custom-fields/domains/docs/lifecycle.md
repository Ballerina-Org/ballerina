
The Specification is (currently) a json definition of the desired Forms UI along with respective types definitions and api's list
that both enables filling the Form with symetric data that ultimately will end up in the backend for the ongoing processing of the just designed spec. 
With IDE, we visually leverage the Ballerina DSL (F#) and sometimes the Ballerina lang itself to design and test the Specification.








and how it display we design the ultimate backend Value<'typeExt, 'valueExt'> 
starting from the Specification, that goes into the Form Engine.

The specification is defined with a (JSON serialized) TypeExpr (v1 or v2) part of the Ballerina F# DSL,
and then is evaluated to the Value and seeded with random data before entering the Form Engine (more detail below).

In the Form Engine we can test and proceed the deltas, and send them to the underlying store.

As this lifecycle of converting the JSON int oa powerful Value that can be also enriched into to Custom Entity with a Ballerina Lang is not triviall,
lets see all the intermediate steps in details along with the accompanying glossary.

