from __future__ import annotations

from typing import TypeVar

from ballerina_core.primitives.sum import Sum
from ballerina_core.serialization.serializer import Deserializer, Json, Serializer

_SumL = TypeVar("_SumL")
_SumR = TypeVar("_SumR")

_DISCRIMINATOR_KEY = "discriminator"
_VALUE_KEY = "value"
_LEFT_VALUE = "left"
_RIGHT_VALUE = "right"


def sum_serializer(
    left_serializer: Serializer[_SumL], right_serializer: Serializer[_SumR], /
) -> Serializer[Sum[_SumL, _SumR]]:
    def serialize(value: Sum[_SumL, _SumR]) -> Json:
        return value.fold(
            lambda a: {_DISCRIMINATOR_KEY: _LEFT_VALUE, _VALUE_KEY: left_serializer(a)},
            lambda b: {_DISCRIMINATOR_KEY: _RIGHT_VALUE, _VALUE_KEY: right_serializer(b)},
        )

    return serialize


def sum_deserializer(
    left_deserializer: Deserializer[_SumL], right_deserializer: Deserializer[_SumR], /
) -> Deserializer[Sum[_SumL, _SumR]]:
    def deserialize(value: Json) -> Sum[_SumL, _SumR]:
        match value:
            case dict():
                if _DISCRIMINATOR_KEY not in value:
                    raise ValueError(f"Missing discriminator: {value}")
                if _VALUE_KEY not in value:
                    raise ValueError(f"Missing value: {value}")
                match value[_DISCRIMINATOR_KEY]:
                    case discriminator if discriminator == _LEFT_VALUE:
                        return Sum.left(left_deserializer(value[_VALUE_KEY]))
                    case discriminator if discriminator == _RIGHT_VALUE:
                        return Sum.right(right_deserializer(value[_VALUE_KEY]))
                    case _:
                        raise ValueError(f"Invalid discriminator: {value}")
            case _:
                raise ValueError(f"Not a dictionary: {value}")

    return deserialize
