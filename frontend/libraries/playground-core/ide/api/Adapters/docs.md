The spec v1 consists from types and forms which are heavily connected to that types to connect with launchers
and to specify the renderers with api sections and (probably) to generate the go code.
It is a full-F# pre-backend state.

While working on the F# backend, several foundations have been changed or extended.
Among them is a new, more powerful type system.

This new type system will be used for anything data-driven: database provisioning, deployment and backend code autogeneration,
plus anything DSL-driven where the Sky is the limit.
The new type system resigns from modeling the data schema with One/Many types by introducing the dedicated spec field to describe the entities, arities, lookups (with commutes)
with tailored predicates and updaters for entities, and path's for relations.

Hence we were supposed to introduce a new spec v2.
This would require some sort of migration/transpilation of the existing specs, which are still under extending (developping manys, read-only fields,...).
A tremendous amount of work and quality assurance would be required to make this happen.

Instead we switched to the notion of a "bridge" between the existing and forthcoming details,
which can be developed and verified independently and gradually.

As a result, the work that is actually required boils down to thorough compatibility testing of the bridged specs.
The perfect place for that is IDE, which has been started 2 months before the bridge decision.
During these two months, one month was spent of the initial UI development, where (simplifiyng facts), v1 spec edited in json editor was validated
and seeded with fake data. The other month was spent of the backend development for the valid API implementation of the (inmemory) store
that uses the ballerina-lang@next version and parsers.

So at that very moment, it appeared to be a good moment to change the IDE UI to fulfill v1/v2 bridge.


The bridge is a product of v1 and typesV2 + schema as v2.
But it has it own set of problems and implications.

#Adapters

As Forms engine is the source of truth and in active development, we don't want to mess with it now so,
until impossible, we don't modify the spec v1 except no-breaking changes extensions, like new fields in the launchers.

The most problematic part in the bridge are the One/Many types. They do not exist in v2 & schema so will not be seeded through types but through schema entities.
The issue are renderers , whoose data (real and seeds) are tied to the One/Many types.

A naive and first version of the adapter would be to inject seed data created from schema entities to v1 data sources.




