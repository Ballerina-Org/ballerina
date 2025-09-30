import datetime
from decimal import Decimal

from ballerina_core.parsing.parsing_types import Json
from ballerina_core.parsing.primitives import (
    bool_from_json,
    bool_to_json,
    date_from_json,
    date_to_json,
    float32_from_json,
    float32_to_json,
    int32_from_json,
    int32_to_json,
    string_from_json,
    string_to_json,
    unit_from_json,
    unit_to_json,
)
from ballerina_core.sum import Sum
from ballerina_core.unit import unit


class TestPrimitivesSerializer:
    @staticmethod
    def test_int_to_json() -> None:
        value = 42
        assert int32_to_json(value) == {"discriminator": "int32", "value": "42"}

    @staticmethod
    def test_int_from_json() -> None:
        value = 42
        serialized: Json = {"discriminator": "int32", "value": "42"}
        assert int32_from_json(serialized) == Sum.right(value)

    @staticmethod
    def test_string_to_json() -> None:
        value = "hello"
        assert string_to_json(value) == value

    @staticmethod
    def test_string_from_json() -> None:
        serialized: Json = "hello"
        assert string_from_json(serialized) == Sum.right(serialized)

    @staticmethod
    def test_unit_to_json() -> None:
        assert unit_to_json(unit) == {"discriminator": "unit"}

    @staticmethod
    def test_unit_from_json() -> None:
        serialized: Json = {"discriminator": "unit"}
        assert unit_from_json(serialized) == Sum.right(unit)

    @staticmethod
    def test_bool_to_json() -> None:
        value = True
        assert bool_to_json(value) == {"discriminator": "bool", "value": value}

    @staticmethod
    def test_bool_from_json() -> None:
        serialized: Json = {"discriminator": "bool", "value": True}
        expected = True
        assert bool_from_json(serialized) == Sum.right(expected)

    @staticmethod
    def test_float_to_json() -> None:
        value = Decimal("3.14")
        assert float32_to_json(value) == {"discriminator": "float32", "value": "3.14"}

    @staticmethod
    def test_float_from_json() -> None:
        serialized: Json = {"discriminator": "float32", "value": "3.14"}
        assert float32_from_json(serialized) == Sum.right(Decimal("3.14"))

    @staticmethod
    def test_date_to_json() -> None:
        value = datetime.date(2021, 2, 1)
        assert date_to_json(value) == {"discriminator": "date", "value": "2021-02-01"}

    @staticmethod
    def test_date_from_json() -> None:
        serialized: Json = {"discriminator": "date", "value": "2021-02-01"}
        assert date_from_json(serialized) == Sum.right(datetime.date(2021, 2, 1))
