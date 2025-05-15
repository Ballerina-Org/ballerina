import math
from decimal import Decimal

from ballerina_core.parsing.dictionary import dict_from_json, dict_to_json
from ballerina_core.parsing.list import list_from_json, list_to_json
from ballerina_core.parsing.parsing_types import Json
from ballerina_core.parsing.primitives import (
    bool_from_json,
    bool_to_json,
    decimal_from_json,
    decimal_to_json,
    int_from_json,
    int_to_json,
    none_from_json,
    none_to_json,
    str_from_json,
    str_to_json,
)
from ballerina_core.parsing.products import tuple1_from_json, tuple1_to_json, tuple2_from_json, tuple2_to_json


class TestPrimitivesSerializer:
    @staticmethod
    def test_int_to_json() -> None:
        value = 42
        assert int_to_json(value) == value

    @staticmethod
    def test_int_from_json() -> None:
        serialized: Json = 42
        assert int_from_json(serialized) == serialized

    @staticmethod
    def test_str_to_json() -> None:
        value = "hello"
        assert str_to_json(value) == value

    @staticmethod
    def test_str_from_json() -> None:
        serialized: Json = "hello"
        assert str_from_json(serialized) == serialized

    @staticmethod
    def test_none_to_json() -> None:
        value = None
        assert none_to_json() == value

    @staticmethod
    def test_none_from_json() -> None:
        serialized: Json = None
        assert none_from_json(serialized) == serialized  # type: ignore[func-returns-value]

    @staticmethod
    def test_bool_to_json() -> None:
        value = True
        assert bool_to_json(value) == value

    @staticmethod
    def test_bool_from_json() -> None:
        serialized: Json = True
        assert bool_from_json(serialized) == serialized

    @staticmethod
    def test_decimal_to_json() -> None:
        value = Decimal("3.14")
        assert decimal_to_json(value) == "3.14"

    @staticmethod
    def test_decimal_from_json() -> None:
        serialized: Json = "3.14"
        assert decimal_from_json(serialized) == Decimal("3.14")


class TestListSerializer:
    @staticmethod
    def test_list_to_json() -> None:
        value = [1, 2, 3]
        serializer = list_to_json(int_to_json)
        serialized = serializer(value)
        assert serialized == [1, 2, 3]

    @staticmethod
    def test_list_from_json() -> None:
        serialized: Json = [1, 2, 3]
        deserializer = list_from_json(int_from_json)
        value = deserializer(serialized)
        assert value == [1, 2, 3]


class TestProductsSerializer:
    @staticmethod
    def test_tuple1_to_json() -> None:
        value = (42,)
        serializer = tuple1_to_json(int_to_json)
        serialized = serializer(value)
        assert serialized == {"Item0": 42}

    @staticmethod
    def test_tuple1_from_json() -> None:
        serialized: Json = {"Item0": 42}
        deserializer = tuple1_from_json(int_from_json)
        value = deserializer(serialized)
        assert value == (42,)

    @staticmethod
    def test_tuple2_to_json() -> None:
        value = (42, "hello")
        serializer = tuple2_to_json(int_to_json, str_to_json)
        serialized = serializer(value)
        assert serialized == {"Item0": 42, "Item1": "hello"}

    @staticmethod
    def test_tuple2_from_json() -> None:
        serialized: Json = {"Item0": 42, "Item1": "hello"}
        deserializer = tuple2_from_json(int_from_json, str_from_json)
        value = deserializer(serialized)
        assert value == (42, "hello")


class TestDictionarySerializer:
    @staticmethod
    def test_dict_to_json() -> None:
        value = {"a": 42, "b": 43}
        serializer = dict_to_json(str_to_json, int_to_json)
        serialized = serializer(value)
        assert serialized == {"a": 42, "b": 43}

    @staticmethod
    def test_dict_from_json() -> None:
        serialized: Json = {"a": 42, "b": 43}
        deserializer = dict_from_json(str_from_json, int_from_json)
        value = deserializer(serialized)
        assert value == {"a": 42, "b": 43}
