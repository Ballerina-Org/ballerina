import math
from decimal import Decimal

from ballerina_core.parsing.dictionary import dict_from_json, dict_to_json
from ballerina_core.parsing.list import list_from_json, list_to_json
from ballerina_core.parsing.option import option_from_json, option_to_json
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
from ballerina_core.parsing.sum import sum_from_json, sum_to_json
from ballerina_core.primitives.option import Option
from ballerina_core.primitives.sum import Sum


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
    def test_float_to_json() -> None:
        value = Decimal(math.pi)
        assert decimal_to_json(value) == value

    @staticmethod
    def test_float_from_json() -> None:
        serialized: Json = Decimal(math.pi)
        assert decimal_from_json(serialized) == serialized


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


class TestOptionSerializer:
    @staticmethod
    def test_option_to_json_some() -> None:
        value = Option.some(42)
        serializer = option_to_json(int_to_json)
        serialized = serializer(value)
        assert serialized == {"discriminator": "some", "value": 42}

    @staticmethod
    def test_option_to_json_none() -> None:
        value: Option[int] = Option.nothing()
        serializer = option_to_json(int_to_json)
        serialized = serializer(value)
        assert serialized == {"discriminator": "nothing", "value": None}

    @staticmethod
    def test_option_from_json_some() -> None:
        serialized: Json = {"discriminator": "some", "value": 42}
        deserializer = option_from_json(int_from_json)
        value = deserializer(serialized)
        assert value == Option.some(42)

    @staticmethod
    def test_option_from_json_none() -> None:
        serialized: Json = {"discriminator": "nothing", "value": None}
        deserializer = option_from_json(int_from_json)
        value = deserializer(serialized)
        assert value == Option.nothing()


class TestSumSerializer:
    @staticmethod
    def test_sum_to_json_left() -> None:
        value = Sum[int, str].left(42)
        serializer = sum_to_json(int_to_json, str_to_json)
        serialized = serializer(value)
        assert serialized == {"discriminator": "left", "value": 42}

    @staticmethod
    def test_sum_to_json_right() -> None:
        value = Sum[int, str].right("42")
        serializer = sum_to_json(int_to_json, str_to_json)
        serialized = serializer(value)
        assert serialized == {"discriminator": "right", "value": "42"}

    @staticmethod
    def test_sum_from_json_left() -> None:
        serialized: Json = {"discriminator": "left", "value": 42}
        deserializer = sum_from_json(int_from_json, str_from_json)
        value = deserializer(serialized)
        assert value == Sum[int, str].left(42)

    @staticmethod
    def test_sum_from_json_right() -> None:
        serialized: Json = {"discriminator": "right", "value": "42"}
        deserializer = sum_from_json(int_from_json, str_from_json)
        value = deserializer(serialized)
        assert value == Sum[int, str].right("42")
