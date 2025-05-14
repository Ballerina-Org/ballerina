from __future__ import annotations

from ballerina_core.parsing.parsing_types import Json


def str_to_json(value: str) -> str:
    return value


def str_from_json(value: Json) -> str:
    match value:
        case str():
            return value
        case _:
            raise ValueError(f"Not a string: {value}")


def int_to_json(value: int) -> Json:
    return value


def int_from_json(value: Json) -> int:
    match value:
        case int():
            return value
        case _:
            raise ValueError(f"Not an int: {value}")


def none_to_json() -> Json:
    return None


def none_from_json(value: Json) -> None:
    match value:
        case None:
            return
        case _:
            raise ValueError(f"Not None: {value}")


def bool_to_json(value: bool) -> Json:  # noqa: FBT001
    return value


def bool_from_json(value: Json) -> bool:
    match value:
        case bool():
            return value
        case _:
            raise ValueError(f"Not a bool: {value}")


def float_to_json(value: float) -> Json:
    return value


def float_from_json(value: Json) -> float:
    match value:
        case float():
            return value
        case _:
            raise ValueError(f"Not a float: {value}")
