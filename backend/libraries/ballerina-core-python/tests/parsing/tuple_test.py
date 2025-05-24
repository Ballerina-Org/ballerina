from ballerina_core.parsing.parsing_types import Json
from ballerina_core.parsing.primitives import int_from_json, int_to_json
from ballerina_core.parsing.tuple import tuple_2_from_json, tuple_2_to_json
from ballerina_core.sum import Sum


def test_tuple_2_to_json() -> None:
    value = (1, 2)
    serializer = tuple_2_to_json(int_to_json, int_to_json)
    serialized = serializer(value)
    assert serialized == {"kind": "tuple", "elements": [int_to_json(1), int_to_json(2)]}


def test_tuple_2_from_json() -> None:
    serialized: Json = {"kind": "tuple", "elements": [int_to_json(1), int_to_json(2)]}
    parser = tuple_2_from_json(int_from_json, int_from_json)
    value = parser(serialized)
    assert value == Sum.right((1, 2))


def test_should_convert_tuple_to_and_from_json() -> None:
    value = (1, 2)
    serializer = tuple_2_to_json(int_to_json, int_to_json)
    parser = tuple_2_from_json(int_from_json, int_from_json)
    assert parser(serializer(value)) == Sum.right(value)
