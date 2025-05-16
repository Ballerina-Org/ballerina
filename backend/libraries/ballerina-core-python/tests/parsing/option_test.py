from ballerina_core.option import Option
from ballerina_core.parsing.option import option_from_json, option_to_json
from ballerina_core.parsing.parsing_types import Json
from ballerina_core.parsing.primitives import int_from_json, int_to_json


class TestOptionSerializer:
    @staticmethod
    def test_option_to_json_some() -> None:
        value = Option.some(42)
        serializer = option_to_json(int_to_json)
        serialized = serializer(value)
        assert serialized == {"case": "some", "value": 42}

    @staticmethod
    def test_option_to_json_none() -> None:
        value: Option[int] = Option.nothing()
        serializer = option_to_json(int_to_json)
        serialized = serializer(value)
        assert serialized == {"case": "nothing", "value": None}

    @staticmethod
    def test_option_from_json_some() -> None:
        serialized: Json = {"case": "some", "value": 42}
        deserializer = option_from_json(int_from_json)
        value = deserializer(serialized)
        assert value == Option.some(42)

    @staticmethod
    def test_option_from_json_none() -> None:
        serialized: Json = {"case": "nothing", "value": None}
        deserializer = option_from_json(int_from_json)
        value = deserializer(serialized)
        assert value == Option.nothing()
