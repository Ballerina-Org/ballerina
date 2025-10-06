from ballerina_core.parsing.keys import DISCRIMINATOR_KEY, VALUE_KEY
from ballerina_core.parsing.list import list_from_json, list_to_json
from ballerina_core.parsing.parsing_types import Json
from ballerina_core.parsing.primitives import int32_from_json, int32_to_json
from ballerina_core.sum import Sum


def test_list_to_json() -> None:
    value = [1, 2, 3]
    serializer = list_to_json(int32_to_json)
    serialized = serializer(value)
    assert serialized == {DISCRIMINATOR_KEY: "list", VALUE_KEY: [int32_to_json(1), int32_to_json(2), int32_to_json(3)]}


def test_list_from_json() -> None:
    serialized: Json = {DISCRIMINATOR_KEY: "list", VALUE_KEY: [int32_to_json(1), int32_to_json(2), int32_to_json(3)]}
    parser = list_from_json(int32_from_json)
    value = parser(serialized)
    assert value == Sum.right([1, 2, 3])


def test_should_convert_list_to_and_from_json() -> None:
    value = [1, 2, 3]
    serializer = list_to_json(int32_to_json)
    parser = list_from_json(int32_from_json)
    assert parser(serializer(value)) == Sum.right(value)
