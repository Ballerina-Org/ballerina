from __future__ import annotations

from collections.abc import Callable
from typing import TypeVar

from ballerina_core.serialization.serializer import Deserializer, Json, Serializer

_A = TypeVar("_A")
_B = TypeVar("_B")


_T = TypeVar("_T")


def sum_2_serializer(
    a_discriminator: str,
    a_serializer: Serializer[_A],
    b_discriminator: str,
    b_serializer: Serializer[_B],
    fold: Callable[[_T, Serializer[_A], Serializer[_B]], Json],
) -> Serializer[_T]:
    def serialize(value: _T) -> Json:
        return fold(
            value,
            lambda a: {"discriminator": a_discriminator, "value": a_serializer(a)},
            lambda b: {"discriminator": b_discriminator, "value": b_serializer(b)},
        )

    return serialize


def sum_2_deserializer(  # noqa: PLR0913,PLR0917
    a_discriminator: str,
    a_deserializer: Deserializer[_A],
    b_discriminator: str,
    b_deserializer: Deserializer[_B],
    from_a: Callable[[_A], _T],
    from_b: Callable[[_B], _T],
) -> Deserializer[_T]:
    def deserialize(value: Json) -> _T:
        match value:
            case dict():
                match value["discriminator"]:
                    case discriminator if discriminator == a_discriminator:
                        return from_a(a_deserializer(value["value"]))
                    case discriminator if discriminator == b_discriminator:
                        return from_b(b_deserializer(value["value"]))
                    case _:
                        raise ValueError(f"Invalid discriminator: {value['discriminator']}")
            case _:
                raise ValueError(f"Not a dictionary: {value}")

    return deserialize
