from ballerina_core.primitives import Sum
from ballerina_core.serialization.primitives import int_deserializer, int_serializer, str_deserializer, str_serializer
from ballerina_core.serialization.serializer import Json


class TestSum:
    @staticmethod
    def test_sum_serializer_left() -> None:
        value = Sum[int, str].left(42)
        serializer = Sum.serializer(int_serializer, str_serializer)
        serialized = serializer(value)
        assert serialized == {"discriminator": "left", "value": 42}

    @staticmethod
    def test_sum_serializer_right() -> None:
        value = Sum[int, str].right("42")
        serializer = Sum.serializer(int_serializer, str_serializer)
        serialized = serializer(value)
        assert serialized == {"discriminator": "right", "value": "42"}

    @staticmethod
    def test_sum_deserializer_left() -> None:
        serialized: Json = {"discriminator": "left", "value": 42}
        deserializer = Sum[int, str].deserializer(int_deserializer, str_deserializer)
        value = deserializer(serialized)
        assert value == Sum[int, str].left(42)

    @staticmethod
    def test_sum_deserializer_right() -> None:
        serialized: Json = {"discriminator": "right", "value": "42"}
        deserializer = Sum[int, str].deserializer(int_deserializer, str_deserializer)
        value = deserializer(serialized)
        assert value == Sum[int, str].right("42")
