from ballerina_core.parsing.parsing_types import Json
from ballerina_core.parsing.primitives import int_from_json, int_to_json, str_from_json, str_to_json
from ballerina_core.parsing.sum import sum_from_json, sum_to_json
from ballerina_core.sum import Sum


class TestSumSerializer:
    @staticmethod
    def test_sum_to_json_left() -> None:
        value = Sum[int, str].left(42)
        serializer = sum_to_json(int_to_json, str_to_json)
        serialized = serializer(value)
        assert serialized == {"case": "left", "value": 42}

    @staticmethod
    def test_sum_to_json_right() -> None:
        value = Sum[int, str].right("42")
        serializer = sum_to_json(int_to_json, str_to_json)
        serialized = serializer(value)
        assert serialized == {"case": "right", "value": "42"}

    @staticmethod
    def test_sum_from_json_left() -> None:
        serialized: Json = {"case": "left", "value": 42}
        deserializer = sum_from_json(int_from_json, str_from_json)
        value = deserializer(serialized)
        assert value == Sum[int, str].left(42)

    @staticmethod
    def test_sum_from_json_right() -> None:
        serialized: Json = {"case": "right", "value": "42"}
        deserializer = sum_from_json(int_from_json, str_from_json)
        value = deserializer(serialized)
        assert value == Sum[int, str].right("42")
