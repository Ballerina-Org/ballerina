from decimal import Decimal
import datetime

from ballerina_core.parsing.parsing_types import Json
from ballerina_core.parsing.primitives import (
    date_from_json,
    date_to_json,
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
from ballerina_core.sum import Sum
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
        assert int_from_json(serialized) == Sum.right(value)

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
        assert unit_to_json(unit) == {"kind": "unit"}

    @staticmethod
    def test_unit_from_json() -> None:
        serialized: Json = {"kind": "unit"}
        assert unit_from_json(serialized) == Sum.right(unit)

    @staticmethod
    def test_bool_to_json() -> None:
        value = True
        assert bool_to_json(value) == value

    @staticmethod
    def test_bool_from_json() -> None:
        serialized: Json = True
        assert bool_from_json(serialized) == Sum.right(serialized)

    @staticmethod
    def test_float_to_json() -> None:
        value = Decimal("3.14")
        assert float_to_json(value) == {"kind": "float", "value": "3.14"}

    @staticmethod
    def test_float_from_json() -> None:
        serialized: Json = {"kind": "float", "value": "3.14"}
        assert float_from_json(serialized) == Sum.right(Decimal("3.14"))

    @staticmethod
    def test_date_to_json() -> None:
        value = datetime.date(2021, 1, 1)
        assert date_to_json(value) == {"kind": "date", "value": "2021-01-01"}

    @staticmethod
    def test_date_from_json() -> None:
        serialized: Json = {"kind": "date", "value": "2021-01-01"}
        assert date_from_json(serialized) == Sum.right(datetime.date(2021, 1, 1))
