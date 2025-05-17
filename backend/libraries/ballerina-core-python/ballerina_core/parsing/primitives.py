from __future__ import annotations

from decimal import Decimal, InvalidOperation

from ballerina_core.parsing.parsing_types import Json, ParsingError
from ballerina_core.sum import Sum
from ballerina_core.unit import Unit, unit

_KIND_KEY: str = "kind"


def string_to_json(value: str) -> Json:
    return value


def string_from_json(value: Json) -> Sum[ParsingError, str]:
    match value:
        case str():
            return Sum.right(value)
        case _:
            return Sum.left(ParsingError.single(f"Not a string: {value}"))


def int_to_json(value: int) -> Json:
    return {_KIND_KEY: "int", "value": str(value)}


def int_from_json(value: Json) -> Sum[ParsingError, int]:
    match value:
        case {"kind": "int", "value": int_value}:
            match int_value:
                case str():
                    return Sum.right(int(int_value))
                case _:
                    return Sum.left(ParsingError.single(f"Not an int: {int_value}"))
        case _:
            return Sum.left(ParsingError.single(f"Not a dictionary: {value}"))


def unit_to_json(_: Unit) -> Json:
    return {_KIND_KEY: "unit"}


def unit_from_json(value: Json) -> Sum[ParsingError, Unit]:
    match value:
        case {"kind": "unit"}:
            return Sum.right(unit)
        case _:
            return Sum.left(ParsingError.single(f"Not a unit: {value}"))


def bool_to_json(value: bool) -> Json:  # noqa: FBT001
    return value


def bool_from_json(value: Json) -> Sum[ParsingError, bool]:
    match value:
        case bool():
            return Sum.right(value)
        case _:
            return Sum.left(ParsingError.single(f"Not a bool: {value}"))


def float_to_json(value: Decimal) -> Json:
    return {_KIND_KEY: "float", "value": str(value)}


def float_from_json(value: Json) -> Sum[ParsingError, Decimal]:
    match value:
        case {"kind": "float", "value": float_value}:
            match float_value:
                case str():
                    try:
                        return Sum.right(Decimal(float_value))
                    except InvalidOperation:
                        return Sum.left(ParsingError.single(f"Not a float: {float_value}"))
                case _:
                    return Sum.left(ParsingError.single(f"Not a string: {float_value}"))
        case _:
            return Sum.left(ParsingError.single(f"Not a float: {value}"))
