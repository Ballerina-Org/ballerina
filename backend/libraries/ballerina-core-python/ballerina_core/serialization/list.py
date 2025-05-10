from __future__ import annotations

from typing import TypeVar

from ballerina_core.serialization.serializer import Deserializer, Json, Serializer

_A = TypeVar("_A")


def list_serializer(item_serializer: Serializer[_A]) -> Serializer[list[_A]]:
    def serialize(value: list[_A]) -> Json:
        return [item_serializer(item) for item in value]

    return serialize


def list_deserializer(item_deserializer: Deserializer[_A]) -> Deserializer[list[_A]]:
    def deserialize(value: Json) -> list[_A]:
        match value:
            case list():
                return [item_deserializer(item) for item in value]
            case _:
                raise ValueError(f"Not a list: {value}")

    return deserialize
