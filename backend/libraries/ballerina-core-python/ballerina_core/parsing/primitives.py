from __future__ import annotations

from ballerina_core.parsing.parsing_types import Json


def str_serializer(value: str) -> str:
    return value


def str_deserializer(value: Json) -> str:
    match value:
        case str():
            return value
        case _:
            raise ValueError(f"Not a string: {value}")


def int_serializer(value: int) -> Json:
    return value


def int_deserializer(value: Json) -> int:
    match value:
        case int():
            return value
        case _:
            raise ValueError(f"Not an int: {value}")


def none_serializer() -> Json:
    return None


def none_deserializer(value: Json) -> None:
    match value:
        case None:
            return
        case _:
            raise ValueError(f"Not None: {value}")


def bool_serializer(value: bool) -> Json:  # noqa: FBT001
    return value


def bool_deserializer(value: Json) -> bool:
    match value:
        case bool():
            return value
        case _:
            raise ValueError(f"Not a bool: {value}")


def float_serializer(value: float) -> Json:
    return value


def float_deserializer(value: Json) -> float:
    match value:
        case float():
            return value
        case _:
            raise ValueError(f"Not a float: {value}")
