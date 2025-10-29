from __future__ import annotations

from typing import TypeVar

from ballerina_core.parsing.keys import DISCRIMINATOR_KEY, VALUE_KEY
from ballerina_core.parsing.parsing_types import FromJson, Json, ParsingError, ToJson
from ballerina_core.sum import Sum, Sum3, Sum4

_SumL = TypeVar("_SumL")
_SumR = TypeVar("_SumR")

_Sum3A = TypeVar("_Sum3A")
_Sum3B = TypeVar("_Sum3B")
_Sum3C = TypeVar("_Sum3C")

_Sum4A = TypeVar("_Sum4A")
_Sum4B = TypeVar("_Sum4B")
_Sum4C = TypeVar("_Sum4C")
_Sum4D = TypeVar("_Sum4D")


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


def sum_2_to_json(left_to_json: ToJson[_SumL], right_to_json: ToJson[_SumR], /) -> ToJson[Sum[_SumL, _SumR]]:
    def to_json(value: Sum[_SumL, _SumR]) -> Json:
        return {
            DISCRIMINATOR_KEY: "sum",
            VALUE_KEY: value.fold(
                lambda a: _case_to_json(1, left_to_json(a)), lambda b: _case_to_json(2, right_to_json(b))
            ),
        }

    return to_json


def _handle_case_tuple(
    case_tuple: tuple[int, Json], left_from_json: FromJson[_SumL], right_from_json: FromJson[_SumR]
) -> Sum[ParsingError, Sum[_SumL, _SumR]]:
    discriminator, payload = case_tuple
    match discriminator:
        case 1:
            return (
                left_from_json(payload)
                .map_left(ParsingError.with_context(f"When parsing case {discriminator}:"))
                .map_right(Sum.left)
            )
        case 2:
            return (
                right_from_json(payload)
                .map_left(ParsingError.with_context(f"When parsing case {discriminator}:"))
                .map_right(Sum.right)
            )
        case _:
            return Sum.left(ParsingError.single(f"Invalid case tuple: invalid discriminator: {case_tuple}"))


def sum_2_from_json(
    left_from_json: FromJson[_SumL], right_from_json: FromJson[_SumR], /
) -> FromJson[Sum[_SumL, _SumR]]:
    def from_json(value: Json) -> Sum[ParsingError, Sum[_SumL, _SumR]]:
        match value:
            case {"discriminator": "sum", "value": case_payload}:
                return _case_from_json(case_payload).flat_map(
                    lambda case_tuple: _handle_case_tuple(case_tuple, left_from_json, right_from_json)
                )
            case _:
                return Sum.left(ParsingError.single(f"Sum2 got invalid structure: {value}"))

    return lambda value: from_json(value).map_left(ParsingError.with_context("Parsing sum2:"))


def sum_3_to_json(
    a_to_json: ToJson[_Sum3A], b_to_json: ToJson[_Sum3B], c_to_json: ToJson[_Sum3C], /
) -> ToJson[Sum3[_Sum3A, _Sum3B, _Sum3C]]:
    def to_json(value: Sum3[_Sum3A, _Sum3B, _Sum3C]) -> Json:
        return {
            DISCRIMINATOR_KEY: "sum",
            VALUE_KEY: value.fold(
                lambda a: _case_to_json(1, a_to_json(a)),
                lambda b: _case_to_json(2, b_to_json(b)),
                lambda c: _case_to_json(3, c_to_json(c)),
            ),
        }

    return to_json


def _handle_case_tuple_3(
    case_tuple: tuple[int, Json],
    a_from_json: FromJson[_Sum3A],
    b_from_json: FromJson[_Sum3B],
    c_from_json: FromJson[_Sum3C],
) -> Sum[ParsingError, Sum3[_Sum3A, _Sum3B, _Sum3C]]:
    discriminator, payload = case_tuple
    match discriminator:
        case 1:
            return (
                a_from_json(payload)
                .map_left(ParsingError.with_context(f"When parsing case {discriminator}:"))
                .map_right(Sum3.sum1of3)
            )
        case 2:
            return (
                b_from_json(payload)
                .map_left(ParsingError.with_context(f"When parsing case {discriminator}:"))
                .map_right(Sum3.sum2of3)
            )
        case 3:
            return (
                c_from_json(payload)
                .map_left(ParsingError.with_context(f"When parsing case {discriminator}:"))
                .map_right(Sum3.sum3of3)
            )
        case _:
            return Sum.left(ParsingError.single(f"Invalid case tuple: invalid discriminator: {case_tuple}"))


def sum_3_from_json(
    a_from_json: FromJson[_Sum3A], b_from_json: FromJson[_Sum3B], c_from_json: FromJson[_Sum3C], /
) -> FromJson[Sum3[_Sum3A, _Sum3B, _Sum3C]]:
    def from_json(value: Json) -> Sum[ParsingError, Sum3[_Sum3A, _Sum3B, _Sum3C]]:
        match value:
            case {"discriminator": "sum", "value": case_payload}:
                return _case_from_json(case_payload).flat_map(
                    lambda case_tuple: _handle_case_tuple_3(case_tuple, a_from_json, b_from_json, c_from_json)
                )
            case _:
                return Sum.left(ParsingError.single(f"Sum3 got invalid structure: {value}"))

    return lambda value: from_json(value).map_left(ParsingError.with_context("Parsing sum3:"))


def sum_4_to_json(
    a_to_json: ToJson[_Sum4A], b_to_json: ToJson[_Sum4B], c_to_json: ToJson[_Sum4C], d_to_json: ToJson[_Sum4D], /
) -> ToJson[Sum4[_Sum4A, _Sum4B, _Sum4C, _Sum4D]]:
    def to_json(value: Sum4[_Sum4A, _Sum4B, _Sum4C, _Sum4D]) -> Json:
        return {
            DISCRIMINATOR_KEY: "sum",
            VALUE_KEY: value.fold(
                lambda a: _case_to_json(1, a_to_json(a)),
                lambda b: _case_to_json(2, b_to_json(b)),
                lambda c: _case_to_json(3, c_to_json(c)),
                lambda d: _case_to_json(4, d_to_json(d)),
            ),
        }

    return to_json


def _handle_case_tuple_4(
    case_tuple: tuple[int, Json],
    a_from_json: FromJson[_Sum4A],
    b_from_json: FromJson[_Sum4B],
    c_from_json: FromJson[_Sum4C],
    d_from_json: FromJson[_Sum4D],
) -> Sum[ParsingError, Sum4[_Sum4A, _Sum4B, _Sum4C, _Sum4D]]:
    discriminator, payload = case_tuple
    match discriminator:
        case 1:
            return (
                a_from_json(payload)
                .map_left(ParsingError.with_context(f"When parsing case {discriminator}:"))
                .map_right(Sum4.sum1of4)
            )
        case 2:
            return (
                b_from_json(payload)
                .map_left(ParsingError.with_context(f"When parsing case {discriminator}:"))
                .map_right(Sum4.sum2of4)
            )
        case 3:
            return (
                c_from_json(payload)
                .map_left(ParsingError.with_context(f"When parsing case {discriminator}:"))
                .map_right(Sum4.sum3of4)
            )
        case 4:
            return (
                d_from_json(payload)
                .map_left(ParsingError.with_context(f"When parsing case {discriminator}:"))
                .map_right(Sum4.sum4of4)
            )
        case _:
            return Sum.left(ParsingError.single(f"Invalid case tuple: invalid discriminator: {case_tuple}"))


def sum_4_from_json(
    a_from_json: FromJson[_Sum4A],
    b_from_json: FromJson[_Sum4B],
    c_from_json: FromJson[_Sum4C],
    d_from_json: FromJson[_Sum4D],
    /,
) -> FromJson[Sum4[_Sum4A, _Sum4B, _Sum4C, _Sum4D]]:
    def from_json(value: Json) -> Sum[ParsingError, Sum4[_Sum4A, _Sum4B, _Sum4C, _Sum4D]]:
        match value:
            case {"discriminator": "sum", "value": case_payload}:
                return _case_from_json(case_payload).flat_map(
                    lambda case_tuple: _handle_case_tuple_4(
                        case_tuple, a_from_json, b_from_json, c_from_json, d_from_json
                    )
                )
            case _:
                return Sum.left(ParsingError.single(f"Sum4 got invalid structure: {value}"))

    return lambda value: from_json(value).map_left(ParsingError.with_context("Parsing sum4:"))
