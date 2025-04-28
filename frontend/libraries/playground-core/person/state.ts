import { faker } from "@faker-js/faker";
import {
  CollectionReference,
  CollectionSelection,
  Guid,
  Maybe,
} from "ballerina-core";
import { OrderedMap } from "immutable";
import { Interest } from "./apis/mocks";
import { Address, City } from "./domains/address/state";
import { v4 } from "uuid";

export type Gender = CollectionReference;
export type Department = CollectionReference;
export const Department = CollectionReference;

export type Person = {
  name: string;
  surname: string;
  birthday: Maybe<Date>;
  subscribeToNewsletter: boolean;
  gender: CollectionSelection<Gender>;
  interests: OrderedMap<Guid, Interest>;
  departments: OrderedMap<Guid, Department>;
  address: Address;
};
export const Person = {
  Default: Object.assign(
    (
      name: string,
      surname: string,
      birthday: Date,
      subscribeToNewsletter: boolean,
      gender: CollectionSelection<Gender>,
      interests: OrderedMap<Guid, Interest>,
      departments: OrderedMap<Guid, Department>,
      address: Address,
    ): Person => ({
      name,
      surname,
      birthday,
      subscribeToNewsletter,
      gender,
      interests,
      departments,
      address,
    }),
    {
      mocked: (): Person => ({
        name: faker.person.firstName(),
        surname: faker.person.lastName(),
        birthday: new Date(
          Date.now() - Math.random() * 1000 * 60 * 60 * 24 * 365 * 45,
        ),
        subscribeToNewsletter: Math.random() > 0.5,
        gender:
          CollectionSelection<CollectionReference>().Default.right(
            "no selection",
          ),
        interests: OrderedMap(),
        departments: OrderedMap(),
        address: {
          street: faker.location.street(),
          number: Math.floor(Math.random() * 500),
          city:
            Math.random() > 0.5
              ? CollectionSelection<CollectionReference>().Default.right(
                  "no selection",
                )
              : CollectionSelection<CollectionReference>().Default.left(
                  City.Default(v4(), faker.location.city()),
                ),
        },
      }),
    },
  ),
  Operations: {
    // VisibleFields: OrderedMap<keyof Person, Predicate<PersonFormPredicateContext>>([
    //   ["name", PersonFormPredicateContext.Predicates.True],
    //   ["surname", PersonFormPredicateContext.Predicates.True],
    //   ["gender", PersonFormPredicateContext.Predicates.True],
    //   ["subscribeToNewsletter", PersonFormPredicateContext.Predicates.True],
    //   ["interests", PersonFormPredicateContext.Predicates.SubscribedToNewsletter],
    //   ["departments", PersonFormPredicateContext.Predicates.True],
    //   ["address", PersonFormPredicateContext.Predicates.BC.or(PersonFormPredicateContext.Predicates.FO)],
    // ])
  },
};
// export type PersonFormState = FormStateFromEntity<Person, {
//   birthday: DateFormState;
//   gender: EnumFormState<PersonFormPredicateContext & FormLabel & BaseEnumContext<Gender>, Gender>;
//   interests: EnumFormState<PersonFormPredicateContext & FormLabel & BaseEnumContext<{Value: CollectionReference}>, {Value: CollectionReference}>;
//   departments: SearchableInfiniteStreamState<Department>;
//   address: AddressFormState
// }>;
// export const PersonFormState = {
//   Default: (): PersonFormState => ({
//     commonFormState: CommonFormState.Default(),
//     formFieldStates: {
//       name: { commonFormState: CommonFormState.Default(), customFormState: unit },
//       surname: { commonFormState: CommonFormState.Default(), customFormState: unit },
//       subscribeToNewsletter: { commonFormState: CommonFormState.Default(), customFormState: unit },
//       birthday: { commonFormState: CommonFormState.Default(), customFormState: DateFormState.Default() },
//       gender: { commonFormState: CommonFormState.Default(), customFormState: EnumFormState<FormLabel & BaseEnumContext<{Gender}>, Gender>().Default() },
//       interests: { commonFormState: CommonFormState.Default(), customFormState: EnumFormState<FormLabel & BaseEnumContext<Interest>, Interest>().Default() },
//       departments: { commonFormState: CommonFormState.Default(), customFormState: SearchableInfiniteStreamState<Department>().Default("", PersonApi.getDepartments()) },
//       address: { commonFormState: CommonFormState.Default(), customFormState:  AddressFormState.Default() }
//     }
//   })
// };
