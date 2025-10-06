from ballerina_core.parsing.keys import DISCRIMINATOR_KEY, VALUE_KEY
from ballerina_core.parsing.parsing_types import FromJson, Json
from ballerina_core.parsing.primitives import int32_from_json, int32_to_json, string_from_json, string_to_json
from ballerina_core.parsing.sum import sum2_from_json, sum2_to_json
from ballerina_core.sum import Sum


def test_sum_to_json_left() -> None:
    value = Sum[int, str].left(42)
    serializer = sum2_to_json(int32_to_json, string_to_json)
    serialized = serializer(value)
    assert serialized == {DISCRIMINATOR_KEY: "sum", VALUE_KEY: [0, int32_to_json(42)]}


def test_sum_to_json_right() -> None:
    value = Sum[int, str].right("42")
    serializer = sum2_to_json(int32_to_json, string_to_json)
    serialized = serializer(value)
    assert serialized == {DISCRIMINATOR_KEY: "sum", VALUE_KEY: [1, string_to_json("42")]}


def test_sum_from_json_left() -> None:
    serialized: Json = {DISCRIMINATOR_KEY: "sum", VALUE_KEY: [0, int32_to_json(42)]}
    parser: FromJson[Sum[int, str]] = sum2_from_json(int32_from_json, string_from_json)
    value = parser(serialized)
    print(value)
    assert value == Sum.right(Sum[int, str].left(42))


def test_sum_from_json_right() -> None:
    serialized: Json = {DISCRIMINATOR_KEY: "sum", VALUE_KEY: [1, string_to_json("42")]}
    parser: FromJson[Sum[int, str]] = sum2_from_json(int32_from_json, string_from_json)
    value = parser(serialized)
    assert value == Sum.right(Sum[int, str].right("42"))
