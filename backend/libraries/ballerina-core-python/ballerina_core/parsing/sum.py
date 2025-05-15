from __future__ import annotations

from typing import TypeVar

from ballerina_core.parsing.parsing_types import FromJson, Json, ToJson
from ballerina_core.primitives.sum import Sum

_SumL = TypeVar("_SumL")
_SumR = TypeVar("_SumR")

_DISCRIMINATOR_KEY = "discriminator"
_VALUE_KEY = "value"
_LEFT_VALUE = "left"
_RIGHT_VALUE = "right"


def sum_to_json(left_to_json: ToJson[_SumL], right_to_json: ToJson[_SumR], /) -> ToJson[Sum[_SumL, _SumR]]:
    def to_json(value: Sum[_SumL, _SumR]) -> Json:
        return value.fold(
            lambda a: {_DISCRIMINATOR_KEY: _LEFT_VALUE, _VALUE_KEY: left_to_json(a)},
            lambda b: {_DISCRIMINATOR_KEY: _RIGHT_VALUE, _VALUE_KEY: right_to_json(b)},
        )

    return to_json


def sum_from_json(left_from_json: FromJson[_SumL], right_from_json: FromJson[_SumR], /) -> FromJson[Sum[_SumL, _SumR]]:
    def from_json(value: Json) -> Sum[_SumL, _SumR]:
        match value:
            case dict():
                if _DISCRIMINATOR_KEY not in value:
                    raise ValueError(f"Missing discriminator: {value}")
                if _VALUE_KEY not in value:
                    raise ValueError(f"Missing value: {value}")
                match value[_DISCRIMINATOR_KEY]:
                    case discriminator if discriminator == _LEFT_VALUE:
                        return Sum.left(left_from_json(value[_VALUE_KEY]))
                    case discriminator if discriminator == _RIGHT_VALUE:
                        return Sum.right(right_from_json(value[_VALUE_KEY]))
                    case _:
                        raise ValueError(f"Invalid discriminator: {value}")
            case _:
                raise ValueError(f"Not a dictionary: {value}")

    return from_json
