// import {
//   Sum,
//   Errors,
//   ValueOrErrors,
//   MapRepo,
//   PredicateValueExtractor,
//   PredicateValue,
//   DispatchParsedType,
//   Predicate,
// } from "ballerina-core";
// import { List, Map, Set } from "immutable";

// type ExprType = DispatchParsedType<any>;
// const ExprType = DispatchParsedType;

// const AType = // [A * B] where C = C1 of B1 | C2 of B2 and B1 = { F1:A; F2:A*A } and B2 = { F1:A*A; F2:A }
//   ExprType.Default.primitive("int", "in");
// const B1Type = ExprType.Default.record(
//   "B1",
//   Map<string, ExprType>([
//     ["F1", ExprType.Default.lookup("A")],
//     [
//       "F2",
//       ExprType.Default.tuple(
//         "B1_F2",
//         [ExprType.Default.lookup("A"), ExprType.Default.lookup("A")],
//         "B1_F2",
//       ),
//     ],
//   ]),
//   "B1",
//   [],
// );
// const B2Type = ExprType.Default.record(
//   "B2",
//   Map<string, ExprType>([
//     [
//       "F1",
//       ExprType.Default.tuple(
//         "B2_F2",
//         [ExprType.Default.lookup("A"), ExprType.Default.lookup("A")],
//         "B2_F2",
//       ),
//     ],
//     ["F2", ExprType.Default.lookup("A")],
//   ]),
//   "B2",
//   [],
// );
// const CType = ExprType.Default.union(
//   "C",
//   Map([
//     ["C1", B1Type],
//     ["C2", B2Type],
//   ]),
//   "C",
// );

// const T = ExprType.Default.tuple(
//   "T",
//   [ExprType.Default.lookup("A"), ExprType.Default.lookup("C")],
//   "T",
// );

// const V = PredicateValue.Default.tuple(
//   List([
//     PredicateValue.Default.varLookup("a1"),
//     PredicateValue.Default.unionCase(
//       // "C1",
//       // PredicateValue.Default.record(
//       //   Map([
//       //     ["F1", PredicateValue.Default.varLookup("a2")],
//       //     ["F2", PredicateValue.Default.tuple(List([
//       //        PredicateValue.Default.varLookup("a3"),
//       //        PredicateValue.Default.varLookup("a4")
//       //     ]))],
//       //   ])
//       // )

//       "C2",
//       PredicateValue.Default.record(
//         Map([
//           [
//             "F1",
//             PredicateValue.Default.tuple(
//               List([
//                 PredicateValue.Default.varLookup("a4"),
//                 PredicateValue.Default.varLookup("a5"),
//               ]),
//             ),
//           ],
//           ["F2", PredicateValue.Default.varLookup("a6")],
//         ]),
//       ),
//     ),
//   ]),
// );

// const res = PredicateValueExtractor.Operations.ExtractPredicateValue(
//   "A",
//   Map([
//     ["A", AType],
//     ["B1", B1Type],
//     ["B2", B2Type],
//     ["C", CType],
//   ]),
//   T,
// )(V);

// console.log(JSON.stringify(res));
