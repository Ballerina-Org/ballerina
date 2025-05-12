import math

from ballerina_core.primitives.option import Option
from ballerina_core.primitives.sum import Sum
from ballerina_core.serialization.list import list_deserializer, list_serializer
from ballerina_core.serialization.option import option_deserializer, option_serializer
from ballerina_core.serialization.primitives import (
    bool_deserializer,
    bool_serializer,
    float_deserializer,
    float_serializer,
    int_deserializer,
    int_serializer,
    none_deserializer,
    none_serializer,
    str_deserializer,
    str_serializer,
)
from ballerina_core.serialization.products import (
    dict_deserializer,
    dict_serializer,
    tuple1_deserializer,
    tuple1_serializer,
    tuple2_deserializer,
    tuple2_serializer,
)
from ballerina_core.serialization.serializer import Json
from ballerina_core.serialization.sum import sum_deserializer, sum_serializer


class TestPrimitivesSerializer:
    @staticmethod
    def test_int_serializer() -> None:
        value = 42
        assert int_serializer(value) == value

    @staticmethod
    def test_int_deserializer() -> None:
        serialized: Json = 42
        assert int_deserializer(serialized) == serialized

    @staticmethod
    def test_str_serializer() -> None:
        value = "hello"
        assert str_serializer(value) == value

    @staticmethod
    def test_str_deserializer() -> None:
        serialized: Json = "hello"
        assert str_deserializer(serialized) == serialized

    @staticmethod
    def test_none_serializer() -> None:
        value = None
        assert none_serializer() == value

    @staticmethod
    def test_none_deserializer() -> None:
        serialized: Json = None
        assert none_deserializer(serialized) == serialized  # type: ignore[func-returns-value]

    @staticmethod
    def test_bool_serializer() -> None:
        value = True
        assert bool_serializer(value) == value

    @staticmethod
    def test_bool_deserializer() -> None:
        serialized: Json = True
        assert bool_deserializer(serialized) == serialized

    @staticmethod
    def test_float_serializer() -> None:
        value = math.pi
        assert float_serializer(value) == value

    @staticmethod
    def test_float_deserializer() -> None:
        serialized: Json = math.pi
        assert float_deserializer(serialized) == serialized


class TestListSerializer:
    @staticmethod
    def test_list_serializer() -> None:
        value = [1, 2, 3]
        serializer = list_serializer(int_serializer)
        serialized = serializer(value)
        assert serialized == [1, 2, 3]

    @staticmethod
    def test_list_deserializer() -> None:
        serialized: Json = [1, 2, 3]
        deserializer = list_deserializer(int_deserializer)
        value = deserializer(serialized)
        assert value == [1, 2, 3]


class TestProductsSerializer:
    @staticmethod
    def test_tuple1_serializer() -> None:
        value = (42,)
        serializer = tuple1_serializer(int_serializer)
        serialized = serializer(value)
        assert serialized == {"Item0": 42}

    @staticmethod
    def test_tuple1_deserializer() -> None:
        serialized: Json = {"Item0": 42}
        deserializer = tuple1_deserializer(int_deserializer)
        value = deserializer(serialized)
        assert value == (42,)

    @staticmethod
    def test_tuple2_serializer() -> None:
        value = (42, "hello")
        serializer = tuple2_serializer(int_serializer, str_serializer)
        serialized = serializer(value)
        assert serialized == {"Item0": 42, "Item1": "hello"}

    @staticmethod
    def test_tuple2_deserializer() -> None:
        serialized: Json = {"Item0": 42, "Item1": "hello"}
        deserializer = tuple2_deserializer(int_deserializer, str_deserializer)
        value = deserializer(serialized)
        assert value == (42, "hello")

    @staticmethod
    def test_dict_serializer() -> None:
        value = {"a": 42, "b": 43}
        serializer = dict_serializer(str_serializer, int_serializer)
        serialized = serializer(value)
        assert serialized == {"a": 42, "b": 43}

    @staticmethod
    def test_dict_deserializer() -> None:
        serialized: Json = {"a": 42, "b": 43}
        deserializer = dict_deserializer(str_deserializer, int_deserializer)
        value = deserializer(serialized)
        assert value == {"a": 42, "b": 43}


class TestOptionSerializer:
    @staticmethod
    def test_option_serializer_some() -> None:
        value = Option.some(42)
        serializer = option_serializer(int_serializer)
        serialized = serializer(value)
        assert serialized == {"discriminator": "some", "value": 42}

    @staticmethod
    def test_option_serializer_none() -> None:
        value: Option[int] = Option.nothing()
        serializer = option_serializer(int_serializer)
        serialized = serializer(value)
        assert serialized == {"discriminator": "nothing", "value": None}

    @staticmethod
    def test_option_deserializer_some() -> None:
        serialized: Json = {"discriminator": "some", "value": 42}
        deserializer = option_deserializer(int_deserializer)
        value = deserializer(serialized)
        assert value == Option.some(42)

    @staticmethod
    def test_option_deserializer_none() -> None:
        serialized: Json = {"discriminator": "nothing", "value": None}
        deserializer = option_deserializer(int_deserializer)
        value = deserializer(serialized)
        assert value == Option.nothing()


class TestSumSerializer:
    @staticmethod
    def test_sum_serializer_left() -> None:
        value = Sum[int, str].left(42)
        serializer = sum_serializer(int_serializer, str_serializer)
        serialized = serializer(value)
        assert serialized == {"discriminator": "left", "value": 42}

    @staticmethod
    def test_sum_serializer_right() -> None:
        value = Sum[int, str].right("42")
        serializer = sum_serializer(int_serializer, str_serializer)
        serialized = serializer(value)
        assert serialized == {"discriminator": "right", "value": "42"}

    @staticmethod
    def test_sum_deserializer_left() -> None:
        serialized: Json = {"discriminator": "left", "value": 42}
        deserializer = sum_deserializer(int_deserializer, str_deserializer)
        value = deserializer(serialized)
        assert value == Sum[int, str].left(42)

    @staticmethod
    def test_sum_deserializer_right() -> None:
        serialized: Json = {"discriminator": "right", "value": "42"}
        deserializer = sum_deserializer(int_deserializer, str_deserializer)
        value = deserializer(serialized)
        assert value == Sum[int, str].right("42")


class TestComplexTypesSerializer:
    @staticmethod
    def test_should_serialize_list_of_tuples_of_int_and_option_string() -> None:
        value = [(42, Option.some("hello")), (43, Option.nothing())]
        serializer = list_serializer(tuple2_serializer(int_serializer, option_serializer(str_serializer)))
        serialized = serializer(value)
        assert serialized == [
            {"Item0": 42, "Item1": {"discriminator": "some", "value": "hello"}},
            {"Item0": 43, "Item1": {"discriminator": "nothing", "value": None}},
        ]

    @staticmethod
    def test_should_deserialize_list_of_tuples_of_int_and_option_string() -> None:
        serialized: Json = [
            {"Item0": 42, "Item1": {"discriminator": "some", "value": "hello"}},
            {"Item0": 43, "Item1": {"discriminator": "nothing", "value": None}},
        ]
        deserializer = list_deserializer(tuple2_deserializer(int_deserializer, option_deserializer(str_deserializer)))
        value = deserializer(serialized)
        assert value == [(42, Option.some("hello")), (43, Option.nothing())]

    @staticmethod
    def test_should_serialize_record_of_sum_of_string_and_option_float_when_str_is_set() -> None:
        value = {"sum": Sum[str, Option[float]].left("hello")}
        serializer = dict_serializer(
            str_serializer, sum_serializer(str_serializer, option_serializer(float_serializer))
        )
        serialized = serializer(value)
        assert serialized == {"sum": {"discriminator": "left", "value": "hello"}}

    @staticmethod
    def test_should_deserialize_record_of_sum_of_string_and_option_float_when_str_is_set() -> None:
        serialized: Json = {"sum": {"discriminator": "left", "value": "hello"}}
        deserializer = dict_deserializer(
            str_deserializer, sum_deserializer(str_deserializer, option_deserializer(float_deserializer))
        )
        value = deserializer(serialized)
        assert value == {"sum": Sum[str, Option[float]].left("hello")}

    @staticmethod
    def test_should_serialize_record_of_sum_of_string_and_option_float_when_float_is_set() -> None:
        value = {"sum": Sum[str, Option[float]].right(Option.some(math.pi))}
        serializer = dict_serializer(
            str_serializer, sum_serializer(str_serializer, option_serializer(float_serializer))
        )
        serialized = serializer(value)
        assert serialized == {"sum": {"discriminator": "right", "value": {"discriminator": "some", "value": math.pi}}}

    @staticmethod
    def test_should_deserialize_record_of_sum_of_string_and_option_float_when_float_is_set() -> None:
        serialized: Json = {"sum": {"discriminator": "right", "value": {"discriminator": "some", "value": math.pi}}}
        deserializer = dict_deserializer(
            str_deserializer, sum_deserializer(str_deserializer, option_deserializer(float_deserializer))
        )
        value = deserializer(serialized)
        assert value == {"sum": Sum[str, Option[float]].right(Option.some(math.pi))}
