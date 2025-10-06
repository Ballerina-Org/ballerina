from __future__ import annotations

from typing import TypeVar

from ballerina_core.parsing.keys import DISCRIMINATOR_KEY, VALUE_KEY
from ballerina_core.parsing.parsing_types import FromJson, Json, ParsingError, ToJson
from ballerina_core.sum import Sum

_SumL = TypeVar("_SumL")
_SumR = TypeVar("_SumR")


def _case_to_json(discriminator: int, payload: Json) -> Json:
    return [discriminator, payload]


def _case_from_json(case_payload: Json) -> Sum[ParsingError, tuple[int, Json]]:
    match case_payload:
        case [discriminator, payload]:
            match discriminator:
                case int():
                    return Sum.right((discriminator, payload))
                case _:
                    return Sum.left(ParsingError.single(f"Invalid case payload, invalid discriminator: {case_payload}"))
        case _:
            return Sum.left(ParsingError.single(f"Invalid case payload, got {case_payload}"))


def sum2_to_json(left_to_json: ToJson[_SumL], right_to_json: ToJson[_SumR], /) -> ToJson[Sum[_SumL, _SumR]]:
    def to_json(value: Sum[_SumL, _SumR]) -> Json:
        return value.fold(
            lambda a: {DISCRIMINATOR_KEY: "sum", VALUE_KEY: _case_to_json(0, left_to_json(a))},
            lambda b: {DISCRIMINATOR_KEY: "sum", VALUE_KEY: _case_to_json(1, right_to_json(b))},
        )

    return to_json


def _handle_case_tuple(
    case_tuple: tuple[int, Json], left_from_json: FromJson[_SumL], right_from_json: FromJson[_SumR]
) -> Sum[ParsingError, Sum[_SumL, _SumR]]:
    discriminator, payload = case_tuple
    match discriminator:
        case 0:
            return (
                left_from_json(payload)
                .map_left(ParsingError.with_context(f"When parsing case {discriminator}:"))
                .map_right(Sum.left)
            )
        case 1:
            return (
                right_from_json(payload)
                .map_left(ParsingError.with_context(f"When parsing case {discriminator}:"))
                .map_right(Sum.right)
            )
        case _:
            return Sum.left(ParsingError.single(f"Invalid case tuple: invalid discriminator: {case_tuple}"))


def sum2_from_json(left_from_json: FromJson[_SumL], right_from_json: FromJson[_SumR], /) -> FromJson[Sum[_SumL, _SumR]]:
    def from_json(value: Json) -> Sum[ParsingError, Sum[_SumL, _SumR]]:
        match value:
            case {"discriminator": "sum", "value": case_payload}:
                return _case_from_json(case_payload).flat_map(
                    lambda case_tuple: _handle_case_tuple(case_tuple, left_from_json, right_from_json)
                )
            case _:
                return Sum.left(ParsingError.single(f"Sum2 got invalid structure: {value}"))

    return lambda value: from_json(value).map_left(ParsingError.with_context("Parsing sum2:"))
