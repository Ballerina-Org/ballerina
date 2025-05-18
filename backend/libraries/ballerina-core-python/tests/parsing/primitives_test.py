from decimal import Decimal

from ballerina_core.parsing.list import list_from_json, list_to_json
from ballerina_core.parsing.parsing_types import Json
from ballerina_core.parsing.primitives import (
    bool_from_json,
    bool_to_json,
    float_from_json,
    float_to_json,
    int_from_json,
    int_to_json,
    string_from_json,
    string_to_json,
    unit_from_json,
    unit_to_json,
)
from ballerina_core.unit import unit


class TestPrimitivesSerializer:
    @staticmethod
    def test_int_to_json() -> None:
        value = 42
        assert int_to_json(value) == {"kind": "int", "value": "42"}

    @staticmethod
    def test_int_from_json() -> None:
        value = 42
        serialized: Json = {"kind": "int", "value": "42"}
        assert int_from_json(serialized) == value

    @staticmethod
    def test_string_to_json() -> None:
        value = "hello"
        assert string_to_json(value) == value

    @staticmethod
    def test_string_from_json() -> None:
        serialized: Json = "hello"
        assert string_from_json(serialized) == serialized

    @staticmethod
    def test_unit_to_json() -> None:
        assert unit_to_json(unit) == {"kind": "unit"}

    @staticmethod
    def test_unit_from_json() -> None:
        serialized: Json = {"kind": "unit"}
        assert unit_from_json(serialized) == unit

    @staticmethod
    def test_bool_to_json() -> None:
        value = True
        assert bool_to_json(value) == value

    @staticmethod
    def test_bool_from_json() -> None:
        serialized: Json = True
        assert bool_from_json(serialized) == serialized

    @staticmethod
    def test_float_to_json() -> None:
        value = Decimal("3.14")
        assert float_to_json(value) == {"kind": "float", "value": "3.14"}

    @staticmethod
    def test_float_from_json() -> None:
        serialized: Json = {"kind": "float", "value": "3.14"}
        assert float_from_json(serialized) == Decimal("3.14")


class TestListSerializer:
    @staticmethod
    def test_list_to_json() -> None:
        value = [1, 2, 3]
        serializer = list_to_json(int_to_json)
        serialized = serializer(value)
        assert serialized == {"kind": "list", "elements": [int_to_json(1), int_to_json(2), int_to_json(3)]}

    @staticmethod
    def test_list_from_json() -> None:
        serialized: Json = {"kind": "list", "elements": [int_to_json(1), int_to_json(2), int_to_json(3)]}
        parser = list_from_json(int_from_json)
        value = parser(serialized)
        assert value == [1, 2, 3]
