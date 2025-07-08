from __future__ import annotations

from typing import TypeVar

from ballerina_core.parsing.parsing_types import FromJson, Json, ParsingError, ToJson
from ballerina_core.sum import Sum

_SumL = TypeVar("_SumL")
_SumR = TypeVar("_SumR")

_CASE_KEY = "case"
_VALUE_KEY = "value"
_LEFT_VALUE = "Sum.Left"
_RIGHT_VALUE = "Sum.Right"


def sum_to_json(left_to_json: ToJson[_SumL], right_to_json: ToJson[_SumR], /) -> ToJson[Sum[_SumL, _SumR]]:
    def to_json(value: Sum[_SumL, _SumR]) -> Json:
        return value.fold(
            lambda a: {_CASE_KEY: _LEFT_VALUE, _VALUE_KEY: left_to_json(a)},
            lambda b: {_CASE_KEY: _RIGHT_VALUE, _VALUE_KEY: right_to_json(b)},
        )

    return to_json


def sum_from_json(left_from_json: FromJson[_SumL], right_from_json: FromJson[_SumR], /) -> FromJson[Sum[_SumL, _SumR]]:
    def from_json(value: Json) -> Sum[ParsingError, Sum[_SumL, _SumR]]:
        match value:
            case dict():
                if _CASE_KEY not in value:
                    return Sum.left(ParsingError.single(f"Missing {_CASE_KEY}: {value}"))
                if _VALUE_KEY not in value:
                    return Sum.left(ParsingError.single(f"Missing {_VALUE_KEY}: {value}"))
                match value[_CASE_KEY]:
                    case discriminator if discriminator == _LEFT_VALUE:
                        return left_from_json(value[_VALUE_KEY]).map_right(Sum.left)
                    case discriminator if discriminator == _RIGHT_VALUE:
                        return right_from_json(value[_VALUE_KEY]).map_right(Sum.right)
                    case _:
                        return Sum.left(ParsingError.single(f"Invalid {_CASE_KEY}: {value}"))
            case _:
                return Sum.left(ParsingError.single(f"Not a dictionary: {value}"))

    return lambda value: from_json(value).map_left(ParsingError.append("Parsing sum:"))
