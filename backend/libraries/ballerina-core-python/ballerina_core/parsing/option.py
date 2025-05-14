from typing import TypeVar

from ballerina_core.primitives.option import Option
from ballerina_core.parsing.parsing_types import Deserializer, Json, Serializer

_Option = TypeVar("_Option")

_DISCRIMINATOR_KEY: str = "discriminator"
_VALUE_KEY: str = "value"
_SOME_VALUE: str = "some"
_NOTHING_VALUE: str = "nothing"


def option_serializer(some_serializer: Serializer[_Option], /) -> Serializer[Option[_Option]]:
    def serialize(value: Option[_Option]) -> Json:
        none: Json = None  # needed because dictionaries are invariant
        return value.fold(
            lambda a: {_DISCRIMINATOR_KEY: _SOME_VALUE, _VALUE_KEY: some_serializer(a)},
            lambda: {_DISCRIMINATOR_KEY: _NOTHING_VALUE, _VALUE_KEY: none},
        )

    return serialize


def option_deserializer(some_deserializer: Deserializer[_Option], /) -> Deserializer[Option[_Option]]:
    def deserialize(value: Json) -> Option[_Option]:
        match value:
            case dict():
                if _DISCRIMINATOR_KEY not in value:
                    raise ValueError(f"Missing discriminator: {value}")
                match value[_DISCRIMINATOR_KEY]:
                    case discriminator if discriminator == _SOME_VALUE:
                        if _VALUE_KEY not in value:
                            raise ValueError(f"Missing value: {value}")
                        return Option.some(some_deserializer(value[_VALUE_KEY]))
                    case discriminator if discriminator == _NOTHING_VALUE:
                        return Option.nothing()
                    case _:
                        raise ValueError(f"Invalid discriminator: {value}")
            case _:
                raise ValueError(f"Not a dictionary: {value}")

    return deserialize
