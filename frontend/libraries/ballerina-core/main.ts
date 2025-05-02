export * from "./src/api-response-handler/state";
export * from "./src/api-response-handler/coroutines/runner";
export * from "./src/apiResultStatus/state";
export * from "./src/async/state";
export * from "./src/async/domains/promise/state";
export * from "./src/async/domains/synchronized/state";
export * from "./src/async/domains/synchronized/coroutines/synchronize";
export * from "./src/async/domains/mirroring/domains/synchronization-result/state";
export * from "./src/async/domains/mirroring/domains/entity/state";
export * from "./src/async/domains/mirroring/domains/entity/domains/loaded-entity/state";
export * from "./src/async/domains/mirroring/domains/entity/domains/loaded-collection-entity/state";
export * from "./src/async/domains/mirroring/domains/entity/domains/loaded-collection/state";
export * from "./src/async/domains/mirroring/domains/entity/domains/loaded-entities/state";
export * from "./src/async/domains/mirroring/domains/singleton/state";
export * from "./src/async/domains/mirroring/domains/singleton/coroutines/synchronizers";
export * from "./src/async/domains/mirroring/domains/collection/state";
export * from "./src/async/domains/mirroring/domains/collection/coroutines/synchronizers";
export * from "./src/async/domains/mirroring/domains/synchronized-entities/state";
export * from "./src/baseEntity/domains/identifiable/state";
export * from "./src/collections/domains/immutable/domains/map/state";
export * from "./src/collections/domains/immutable/domains/list/state";
export * from "./src/collections/domains/immutable/domains/orderedMap/state";
export * from "./src/collections/domains/immutable/domains/ordereredSet/state";
export * from "./src/collections/domains/array/state";
export * from "./src/coroutines/state";
export * from "./src/coroutines/template";
export * from "./src/coroutines/builder";
export * from "./src/debounced/state";
export * from "./src/debounced/coroutines/debounce";
export * from "./src/diagnostics/domains/message-box/state";
export * from "./src/foreignMutations/state";
export * from "./src/fun/state";
export * from "./src/fun/domains/curry/state";
export * from "./src/fun/domains/id/state";
export * from "./src/fun/domains/simpleCallback/state";
export * from "./src/fun/domains/uncurry/state";
export * from "./src/fun/domains/unit/state";
export * from "./src/fun/domains/predicate/state";
export * from "./src/fun/domains/predicate/domains/bool-expr";
export * from "./src/fun/domains/updater/state";
export * from "./src/fun/domains/updater/domains/simpleUpdater/state";
export * from "./src/fun/domains/updater/domains/simpleUpdater/domains/baseSimpleUpdater/state";
export * from "./src/fun/domains/updater/domains/maybeUpdater/state";
export * from "./src/fun/domains/updater/domains/mapUpdater/state";
export * from "./src/fun/domains/updater/domains/orderedMapUpdater/state";
export * from "./src/fun/domains/updater/domains/orderedSetUpdater/state";
export * from "./src/fun/domains/updater/domains/caseUpdater/state";
export * from "./src/fun/domains/updater/domains/replaceWith/state";
export * from "./src/infinite-data-stream/state";
export * from "./src/infinite-data-stream/template";
export * from "./src/infinite-data-stream/coroutines/runner";
export * from "./src/infinite-data-stream/coroutines/infiniteLoader";
export * from "./src/infinite-data-stream/coroutines/builder";
export * from "./src/math/domains/DOMRect/state";
export * from "./src/math/domains/number/state";
export * from "./src/math/domains/rgba/state";
export * from "./src/math/domains/size2/state";
export * from "./src/math/domains/vector2/state";
export * from "./src/math/domains/rect/state";
export * from "./src/state/domains/repository/state";
export * from "./src/template/state";
export * from "./src/validation/state";
export * from "./src/visibility/state";
export * from "./src/value/state";
export * from "./src/collections/domains/maybe/state";
export * from "./src/value/domains/mutable-value/state";
export * from "./src/collections/domains/sum/state";
export * from "./src/collections/domains/product/state";
export * from "./src/collections/domains/valueOrErrors/state";
export * from "./src/collections/domains/errors/state";
export * from "./src/queue/state";
export * from "./src/forms/domains/attachments/views/attachments-view";
export * from "./src/forms/domains/singleton/state";
export * from "./src/forms/domains/singleton/template";
export * from "./src/forms/domains/singleton/domains/form-label/state";
export * from "./src/forms/domains/collection/domains/reference/state";
export * from "./src/forms/domains/collection/domains/selection/state";
export * from "./src/forms/domains/primitives/domains/string/state";
export * from "./src/forms/domains/primitives/domains/string/template";
export * from "./src/forms/domains/primitives/domains/boolean/state";
export * from "./src/forms/domains/primitives/domains/boolean/template";
export * from "./src/forms/domains/primitives/domains/number/state";
export * from "./src/forms/domains/primitives/domains/number/template";
export * from "./src/forms/domains/primitives/domains/date/state";
export * from "./src/forms/domains/primitives/domains/date/template";
export * from "./src/forms/domains/primitives/domains/enum/state";
export * from "./src/forms/domains/primitives/domains/enum/template";
export * from "./src/forms/domains/primitives/domains/enum-multiselect/state";
export * from "./src/forms/domains/primitives/domains/enum-multiselect/template";
export * from "./src/forms/domains/primitives/domains/searchable-infinite-stream/state";
export * from "./src/forms/domains/primitives/domains/searchable-infinite-stream/template";
export * from "./src/forms/domains/primitives/domains/searchable-infinite-stream-multiselect/state";
export * from "./src/forms/domains/primitives/domains/searchable-infinite-stream-multiselect/template";
export * from "./src/forms/domains/primitives/domains/list/state";
export * from "./src/forms/domains/primitives/domains/list/template";
export * from "./src/forms/domains/primitives/domains/base-64-file/state";
export * from "./src/forms/domains/primitives/domains/base-64-file/template";
export * from "./src/forms/domains/primitives/domains/secret/state";
export * from "./src/forms/domains/primitives/domains/secret/template";
export * from "./src/forms/domains/primitives/domains/map/state";
export * from "./src/forms/domains/primitives/domains/map/template";
export * from "./src/forms/domains/primitives/domains/sum/state";
export * from "./src/forms/domains/primitives/domains/sum/template";
export * from "./src/forms/domains/primitives/domains/unit/state";
export * from "./src/forms/domains/primitives/domains/unit/template";
export * from "./src/forms/domains/parser/state";
export * from "./src/forms/domains/parser/template";
export * from "./src/forms/domains/parser/coroutines/runner";
export * from "./src/forms/domains/parser/domains/validator/state";
export * from "./src/forms/domains/parser/domains/built-ins/state";
export * from "./src/forms/domains/parser/domains/types/state";
export * from "./src/forms/domains/launcher/domains/edit/state";
export * from "./src/forms/domains/launcher/domains/edit/template";
export * from "./src/forms/domains/launcher/domains/edit/coroutines/runner";
export * from "./src/forms/domains/launcher/domains/create/state";
export * from "./src/forms/domains/launcher/domains/create/template";
export * from "./src/forms/domains/launcher/domains/create/coroutines/runner";
export * from "./src/forms/domains/launcher/domains/merger/state";
export * from "./src/forms/domains/launcher/coroutines/runner";
export * from "./src/forms/domains/launcher/state";
export * from "./src/forms/domains/launcher/template";
export * from "./src/forms/domains/parser/domains/injectables/state";
export * from "./src/forms/domains/parser/domains/predicates/state";
export * from "./src/forms/domains/parser/domains/predicates/domains/extractor/state";
export * from "./src/forms/domains/launcher/domains/passthrough/state";
export * from "./src/forms/domains/launcher/domains/passthrough/template";
export * from "./src/forms/domains/primitives/domains/tuple/state";
export * from "./src/forms/domains/primitives/domains/tuple/template";
export * from "./src/forms/domains/primitives/domains/unit/state";
export * from "./src/forms/domains/primitives/domains/unit/template";
export * from "./src/forms/domains/parser/domains/deltas/state";
export * from "./src/forms/domains/parser/domains/layout/state";
export * from "./src/value-infinite-data-stream/state";
export * from "./src/value-infinite-data-stream/template";
export * from "./src/value-infinite-data-stream/coroutines/runner";
export * from "./src/value-infinite-data-stream/coroutines/builder";

// Dispatched forms
export * from "./src/forms/domains/dispatched-forms/deserializer/domains/specification/state";
export * from "./src/forms/domains/dispatched-forms/deserializer/domains/specification/domains/apis/state";
export * from "./src/forms/domains/dispatched-forms/deserializer/template";
export * from "./src/forms/domains/dispatched-forms/deserializer/coroutines/runner";
export * from "./src/forms/domains/dispatched-forms/deserializer/state";
export * from "./src/forms/domains/dispatched-forms/dispatcher/domains/deltas/state";
export * from "./src/forms/domains/dispatched-forms/dispatcher/domains/nestedDispatcher/state";
export * from "./src/forms/domains/dispatched-forms/dispatcher/domains/nestedDispatcher/domains/nestedPrimitiveDispatcher/state";
export * from "./src/forms/domains/dispatched-forms/dispatcher/domains/nestedDispatcher/domains/nestedSumDispatcher/state";
export * from "./src/forms/domains/dispatched-forms/dispatcher/domains/nestedDispatcher/domains/nestedTupleDispatcher/state";
export * from "./src/forms/domains/dispatched-forms/dispatcher/domains/nestedDispatcher/domains/nestedListDispatcher/state";
export * from "./src/forms/domains/dispatched-forms/dispatcher/domains/nestedDispatcher/domains/nestedMapDispatcher/state";
export * from "./src/forms/domains/dispatched-forms/dispatcher/domains/nestedDispatcher/domains/nestedLookupDispatcher/state";
export * from "./src/forms/domains/dispatched-forms/dispatcher/domains/abstract-renderers/base-64-file/template";
export * from "./src/forms/domains/dispatched-forms/dispatcher/domains/abstract-renderers/boolean/template";
export * from "./src/forms/domains/dispatched-forms/dispatcher/domains/abstract-renderers/date/template";
export * from "./src/forms/domains/dispatched-forms/dispatcher/domains/abstract-renderers/enum/template";
export * from "./src/forms/domains/dispatched-forms/dispatcher/domains/abstract-renderers/enum-multiselect/template";
export * from "./src/forms/domains/dispatched-forms/dispatcher/domains/abstract-renderers/list/template";
export * from "./src/forms/domains/dispatched-forms/dispatcher/domains/abstract-renderers/map/template";
export * from "./src/forms/domains/dispatched-forms/dispatcher/domains/abstract-renderers/number/template";
export * from "./src/forms/domains/dispatched-forms/dispatcher/domains/abstract-renderers/searchable-infinite-stream/template";
export * from "./src/forms/domains/dispatched-forms/dispatcher/domains/abstract-renderers/searchable-infinite-stream-multiselect/template";
export * from "./src/forms/domains/dispatched-forms/dispatcher/domains/abstract-renderers/secret/template";
export * from "./src/forms/domains/dispatched-forms/dispatcher/domains/abstract-renderers/string/template";
export * from "./src/forms/domains/dispatched-forms/dispatcher/domains/abstract-renderers/sum/template";
export * from "./src/forms/domains/dispatched-forms/dispatcher/domains/abstract-renderers/tuple/template";
export * from "./src/forms/domains/dispatched-forms/dispatcher/domains/abstract-renderers/unit/template";
export * from "./src/forms/domains/dispatched-forms/dispatcher/domains/abstract-renderers/union/template";
export * from "./src/forms/domains/dispatched-forms/dispatcher/domains/runner/template";
export * from "./src/forms/domains/dispatched-forms/dispatcher/domains/runner/state";
export * from "./src/forms/domains/dispatched-forms/deserializer/domains/specification/domains/types/state";
export * from "./src/forms/domains/dispatched-forms/dispatcher/domains/abstract-renderers/record/template";
export * from "./src/forms/domains/dispatched-forms/dispatcher/domains/abstract-renderers/record/state";
export * from "./src/forms/domains/dispatched-forms/dispatcher/domains/nestedDispatcher/domains/nestedSingleSelectionDispatcher/state";
export * from "./src/forms/domains/dispatched-forms/dispatcher/domains/nestedDispatcher/domains/nestedMultiSelectionDispatcher/state";
export * from "./src/forms/domains/dispatched-forms/dispatcher/domains/recordFormDispatcher/state";
export * from "./src/forms/domains/dispatched-forms/dispatcher/domains/abstract-renderers/record/state";
export * from "./src/forms/domains/dispatched-forms/dispatcher/domains/abstract-renderers/record/template";
export * from "./src/forms/domains/dispatched-forms/dispatcher/domains/abstract-renderers/base-64-file/template";
export * from "./src/forms/domains/dispatched-forms/dispatcher/domains/abstract-renderers/base-64-file/state";
export * from "./src/forms/domains/dispatched-forms/dispatcher/domains/abstract-renderers/boolean/template";
export * from "./src/forms/domains/dispatched-forms/dispatcher/domains/abstract-renderers/boolean/state";
export * from "./src/forms/domains/dispatched-forms/dispatcher/domains/abstract-renderers/date/template";
export * from "./src/forms/domains/dispatched-forms/dispatcher/domains/abstract-renderers/date/state";
export * from "./src/forms/domains/dispatched-forms/dispatcher/domains/abstract-renderers/enum/template";
export * from "./src/forms/domains/dispatched-forms/dispatcher/domains/abstract-renderers/enum/state";
export * from "./src/forms/domains/dispatched-forms/dispatcher/domains/abstract-renderers/enum-multiselect/template";
export * from "./src/forms/domains/dispatched-forms/dispatcher/domains/abstract-renderers/enum-multiselect/state";
export * from "./src/forms/domains/dispatched-forms/dispatcher/domains/abstract-renderers/list/template";
export * from "./src/forms/domains/dispatched-forms/dispatcher/domains/abstract-renderers/list/state";
export * from "./src/forms/domains/dispatched-forms/dispatcher/domains/abstract-renderers/map/template";
export * from "./src/forms/domains/dispatched-forms/dispatcher/domains/abstract-renderers/map/state";
export * from "./src/forms/domains/dispatched-forms/dispatcher/domains/abstract-renderers/number/template";
export * from "./src/forms/domains/dispatched-forms/dispatcher/domains/abstract-renderers/number/state";
export * from "./src/forms/domains/dispatched-forms/dispatcher/domains/abstract-renderers/searchable-infinite-stream/template";
export * from "./src/forms/domains/dispatched-forms/dispatcher/domains/abstract-renderers/searchable-infinite-stream/state";
export * from "./src/forms/domains/dispatched-forms/dispatcher/domains/abstract-renderers/searchable-infinite-stream-multiselect/template";
export * from "./src/forms/domains/dispatched-forms/dispatcher/domains/abstract-renderers/searchable-infinite-stream-multiselect/state";
export * from "./src/forms/domains/dispatched-forms/dispatcher/domains/abstract-renderers/secret/template";
export * from "./src/forms/domains/dispatched-forms/dispatcher/domains/abstract-renderers/secret/state";
export * from "./src/forms/domains/dispatched-forms/dispatcher/domains/abstract-renderers/string/template";
export * from "./src/forms/domains/dispatched-forms/dispatcher/domains/abstract-renderers/string/state";
export * from "./src/forms/domains/dispatched-forms/dispatcher/domains/abstract-renderers/sum/template";
export * from "./src/forms/domains/dispatched-forms/dispatcher/domains/abstract-renderers/sum/state";
export * from "./src/forms/domains/dispatched-forms/dispatcher/domains/abstract-renderers/tuple/template";
export * from "./src/forms/domains/dispatched-forms/dispatcher/domains/abstract-renderers/tuple/state";
export * from "./src/forms/domains/dispatched-forms/dispatcher/domains/abstract-renderers/unit/template";
export * from "./src/forms/domains/dispatched-forms/dispatcher/domains/abstract-renderers/unit/state";
export * from "./src/forms/domains/dispatched-forms/dispatcher/domains/abstract-renderers/union/template";
export * from "./src/forms/domains/dispatched-forms/dispatcher/domains/abstract-renderers/union/state";
export * from "./src/forms/domains/dispatched-forms/dispatcher/domains/abstract-renderers/record/template";
export * from "./src/forms/domains/dispatched-forms/dispatcher/domains/abstract-renderers/record/state";
export * from "./src/forms/domains/dispatched-forms/built-ins/state";
export * from "./src/forms/domains/dispatched-forms/deserializer/domains/specification/domains/form/domains/renderers/domains/tableFormRenderer/state";
export * from "./src/forms/domains/dispatched-forms/dispatcher/domains/tableFormDispatcher/state";
export * from "./src/forms/domains/dispatched-forms/dispatcher/domains/abstract-renderers/table/state";
export * from "./src/forms/domains/dispatched-forms/dispatcher/domains/abstract-renderers/table/template";
export * from "./src/forms/domains/dispatched-forms/deserializer/domains/specification/domains/form/domains/renderers/domains/tableFormRenderer/state";
export * from "./src/forms/domains/dispatched-forms/dispatcher/domains/nestedDispatcher/domains/nestedTableDisaptcher/state";
export * from "./src/forms/domains/dispatched-forms/dispatcher/domains/abstract-renderers/state";
// import { simpleUpdater, simpleUpdaterWithChildren } from "./src/fun/domains/updater/domains/simpleUpdater/state"
// import { Updater } from "./src/fun/domains/updater/state"

// // little testing playground and microsample: please do not remove
// type City = { name: string, population: number }
// const City = {
//   Default: (): City => ({
//     name: "Mirano", population: 25000
//   }),
//   Updaters: {
//     Core: {
//       ...simpleUpdater<City>()("name"),
//       ...simpleUpdater<City>()("population"),
//     }
//   }
// }
// type Address = { street: string, number: number, city: City }
// const Address = {
//   Default: (): Address => ({
//     street: "Don Minzoni", number: 20, city: City.Default()
//   }),
//   Updaters: {
//     Core: {
//       ...simpleUpdater<Address>()("street"),
//       ...simpleUpdater<Address>()("number"),
//       ...simpleUpdaterWithChildren<Address>()(City.Updaters)("city"),
//     },
//   }
// }
// type Person = { name: string, surname: string, address: Address }
// const Person = {
//   Default: (): Person => ({
//     name: "John", surname: "Doe", address: Address.Default()
//   }),
//   Updaters: {
//     Core: {
//       ...simpleUpdater<Person>()("name"),
//       ...simpleUpdater<Person>()("surname"),
//       ...simpleUpdaterWithChildren<Person>()(Address.Updaters)("address"),
//     }
//   }
// }

// const personUpdater:Updater<Person> =
//   Person.Updaters.Core.address.children.Core.city.children.Core.name(_ =>"The Great " + _)
//   .then(Person.Updaters.Core.address(Address.Updaters.Core.street(_ => _ + " str.")))
//   .then(Person.Updaters.Core.address.children.Core.number(_ => _ + 1))
// console.log(personUpdater(Person.Default()))

// const addressUpdater:Updater<Address> = Address.Updaters.Core.city.children.name(_ =>"The Great " + _)
// console.log(addressUpdater(Address.Default()))
