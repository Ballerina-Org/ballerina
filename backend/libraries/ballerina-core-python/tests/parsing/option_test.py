from ballerina_core.option import Option
from ballerina_core.parsing.option import option_from_json, option_to_json
from ballerina_core.parsing.parsing_types import Json
from ballerina_core.parsing.primitives import int32_from_json, int32_to_json, unit_from_json, unit_to_json
from ballerina_core.sum import Sum
from ballerina_core.unit import unit


def test_option_to_json_some() -> None:
    value = Option.some(42)
    serializer = option_to_json(int32_to_json, unit_to_json)
    serialized = serializer(value)
    assert serialized == {"discriminator": "union", "value": ["some", int32_to_json(42)]}


def test_option_to_json_none() -> None:
    value: Option[int] = Option.none()
    serializer = option_to_json(int32_to_json, unit_to_json)
    serialized = serializer(value)
    assert serialized == {"discriminator": "union", "value": ["none", unit_to_json(unit)]}


def test_option_from_json_some() -> None:
    serialized: Json = {"discriminator": "union", "value": ["some", int32_to_json(42)]}
    parser = option_from_json(int32_from_json, unit_from_json)
    value = parser(serialized)
    assert value == Sum.right(Option.some(42))


def test_option_from_json_none() -> None:
    serialized: Json = {"discriminator": "union", "value": ["none", unit_to_json(unit)]}
    parser = option_from_json(int32_from_json, unit_from_json)
    value = parser(serialized)
    assert value == Sum.right(Option.none())
