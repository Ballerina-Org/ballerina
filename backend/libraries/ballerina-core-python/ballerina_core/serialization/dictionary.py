from __future__ import annotations

from collections.abc import Callable
from typing import TypeVar

from ballerina_core.serialization.serializer import Deserializer, Json, Serializer

_A = TypeVar("_A")
_B = TypeVar("_B")


def dict_serializer(key_serializer: Callable[[_A], str], value_serializer: Serializer[_B]) -> Serializer[dict[_A, _B]]:
    def serialize(value: dict[_A, _B]) -> Json:
        return {key_serializer(k): value_serializer(v) for k, v in value.items()}

    return serialize


def dict_deserializer(
    key_deserializer: Deserializer[_A], value_deserializer: Deserializer[_B]
) -> Deserializer[dict[_A, _B]]:
    def deserialize(value: Json) -> dict[_A, _B]:
        match value:
            case dict():
                return {key_deserializer(k): value_deserializer(v) for k, v in value.items()}
            case _:
                raise ValueError(f"Not a dictionary: {value}")

    return deserialize
