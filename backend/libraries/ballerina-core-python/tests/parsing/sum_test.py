from ballerina_core.parsing.parsing_types import FromJson, Json
from ballerina_core.parsing.primitives import int_from_json, int_to_json, string_from_json, string_to_json
from ballerina_core.parsing.sum import sum_from_json, sum_to_json
from ballerina_core.sum import Sum


def test_sum_to_json_left() -> None:
    value = Sum[int, str].left(42)
    serializer = sum_to_json(int_to_json, string_to_json)
    serialized = serializer(value)
    assert serialized == {"case": "Sum.Left", "value": int_to_json(42)}


def test_sum_to_json_right() -> None:
    value = Sum[int, str].right("42")
    serializer = sum_to_json(int_to_json, string_to_json)
    serialized = serializer(value)
    assert serialized == {"case": "Sum.Right", "value": string_to_json("42")}


def test_sum_from_json_left() -> None:
    serialized: Json = {"case": "Sum.Left", "value": int_to_json(42)}
    parser: FromJson[Sum[int, str]] = sum_from_json(int_from_json, string_from_json)
    value = parser(serialized)
    assert value == Sum.right(Sum[int, str].left(42))


def test_sum_from_json_right() -> None:
    serialized: Json = {"case": "Sum.Right", "value": string_to_json("42")}
    parser: FromJson[Sum[int, str]] = sum_from_json(int_from_json, string_from_json)
    value = parser(serialized)
    assert value == Sum.right(Sum[int, str].right("42"))
