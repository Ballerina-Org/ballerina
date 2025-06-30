import { faker } from "@faker-js/faker";
import {
  SearchableInfiniteStreamState,
  PromiseRepo,
  OrderedMapRepo,
} from "ballerina-core";
import { v4 } from "uuid";
import { City } from "../state";
import { Range } from "immutable";
import {IDEApi} from "../../../../ide/apis/spec";

const data = () => ({data: OrderedMapRepo.Default.fromIdentifiables(
  Range(0, 20)
    .map((_) => City.Default(v4(), faker.location.city()))
    .toArray(),
),
  hasMoreValues: Math.random() > 0.5,
})

// export const AddressApi = {
//   getCities:
//     (): SearchableInfiniteStreamState["customFormState"]["getChunk"] =>
//     (_searchText) =>
//     (_streamPosition) =>
//       PromiseRepo.Default.mock(() =>
//       {
//         debugger
//         const d = data()
//         return d
//       }),
// };
const test = (_searchText: string): Promise<any> =>
  PromiseRepo.Default.mock(() =>
    IDEApi.searchCities(_searchText).then(data => ({
      data: OrderedMapRepo.Default.fromIdentifiables(data.payload),
      hasMoreValues: Math.random() > 0.5
    }))
  );

export const AddressApi = {
  getCities:
    (): SearchableInfiniteStreamState["customFormState"]["getChunk"] =>
      (_searchText) =>
        (_streamPosition) => {
          const data = test(_searchText);
          return data ;},
};