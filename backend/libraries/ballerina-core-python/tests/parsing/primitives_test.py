import datetime
from decimal import Decimal

from ballerina_core.parsing.keys import DISCRIMINATOR_KEY, VALUE_KEY
from ballerina_core.parsing.parsing_types import Json
from ballerina_core.parsing.primitives import (
    bool_from_json,
    bool_to_json,
    date_from_json,
    date_to_json,
    decimal_from_json,
    decimal_to_json,
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
        assert int32_to_json(value) == {DISCRIMINATOR_KEY: "int32", VALUE_KEY: "42"}

    @staticmethod
    def test_int_from_json() -> None:
        value = 42
        serialized: Json = {DISCRIMINATOR_KEY: "int32", VALUE_KEY: "42"}
        assert int32_from_json(serialized) == Sum.right(value)

    @staticmethod
    def test_string_to_json() -> None:
        value = "hello"
        assert string_to_json(value) == {DISCRIMINATOR_KEY: "string", VALUE_KEY: value}

    @staticmethod
    def test_string_from_json() -> None:
        serialized: Json = {DISCRIMINATOR_KEY: "string", VALUE_KEY: "hello"}
        assert string_from_json(serialized) == Sum.right("hello")

    @staticmethod
    def test_unit_to_json() -> None:
        assert unit_to_json(unit) == {DISCRIMINATOR_KEY: "unit", VALUE_KEY: "()"}

    @staticmethod
    def test_unit_from_json() -> None:
        serialized: Json = {DISCRIMINATOR_KEY: "unit", VALUE_KEY: "()"}
        assert unit_from_json(serialized) == Sum.right(unit)

    @staticmethod
    def test_bool_to_json() -> None:
        value = True
        assert bool_to_json(value) == {DISCRIMINATOR_KEY: "bool", VALUE_KEY: value}

    @staticmethod
    def test_bool_from_json() -> None:
        serialized: Json = {DISCRIMINATOR_KEY: "bool", VALUE_KEY: True}
        expected = True
        assert bool_from_json(serialized) == Sum.right(expected)

    @staticmethod
    def test_decimal_to_json() -> None:
        value = Decimal("3.14")
        assert decimal_to_json(value) == {DISCRIMINATOR_KEY: "decimal", VALUE_KEY: "3.14"}

    @staticmethod
    def test_decimal_from_json() -> None:
        serialized: Json = {DISCRIMINATOR_KEY: "decimal", VALUE_KEY: "3.14"}
        assert decimal_from_json(serialized) == Sum.right(Decimal("3.14"))

    @staticmethod
    def test_date_to_json() -> None:
        value = datetime.date(2021, 2, 1)
        assert date_to_json(value) == {DISCRIMINATOR_KEY: "date", VALUE_KEY: "2021-02-01"}

    @staticmethod
    def test_date_from_json() -> None:
        serialized: Json = {DISCRIMINATOR_KEY: "date", VALUE_KEY: "2021-02-01"}
        assert date_from_json(serialized) == Sum.right(datetime.date(2021, 2, 1))
