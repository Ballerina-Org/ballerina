from __future__ import annotations

from collections.abc import Callable
from typing import TypeVar

from ballerina_core.parsing.parsing_types import FromJson, Json, ToJson

_A = TypeVar("_A")
_B = TypeVar("_B")


def dict_to_json(key_to_json: Callable[[_A], str], value_to_json: ToJson[_B]) -> ToJson[dict[_A, _B]]:
    def to_json(value: dict[_A, _B]) -> Json:
        return {key_to_json(k): value_to_json(v) for k, v in value.items()}

    return to_json


def dict_from_json(key_from_json: FromJson[_A], value_from_json: FromJson[_B]) -> FromJson[dict[_A, _B]]:
    def from_json(value: Json) -> dict[_A, _B]:
        match value:
            case dict():
                return {key_from_json(k): value_from_json(v) for k, v in value.items()}
            case _:
                raise ValueError(f"Not a dictionary: {value}")

    return from_json
