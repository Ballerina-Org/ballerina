import {
  DispatchFieldTypeConverters,
} from "web/src/domains/dispatched-passthrough-form/apis/field-converters.ts"

describe("plain values and basic structure", () => {

  it("parses a plain int value", () => {
    expect(DispatchFieldTypeConverters.number.fromAPIRawValue(42)).toBe(42);
  });

  it("parses a structured int value", () => {
    expect(DispatchFieldTypeConverters.number.fromAPIRawValue({ kind: "int", value: "42"})).toBe(42);
  });
  
})
describe("parses structured float", () => {
  test.concurrent.each([
    ["0.0", 0.0],
    ["-0.0", -0.0],
    ["1.0", 1.0],
    ["1.5", 1.5],
    ["-1.0", -1.0],
    ["1.", 1.0],
    ["-1.", -1.0],
    //TODO: provide some F#ish strange floats
  ])('parse structure float (%s)', async (a: string, expected: number) => {
    expect(
      DispatchFieldTypeConverters.number.fromAPIRawValue({kind: "float", value: a})
    ).toBe(expected);
  });
})
