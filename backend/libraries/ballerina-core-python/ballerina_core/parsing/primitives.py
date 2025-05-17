from __future__ import annotations

from decimal import Decimal

from ballerina_core.parsing.parsing_types import Json
from ballerina_core.unit import Unit, unit

_KIND_KEY: str = "kind"


def string_to_json(value: str) -> Json:
    return value


def string_from_json(value: Json) -> str:
    match value:
        case str():
            return value
        case _:
            raise ValueError(f"Not a string: {value}")


def int_to_json(value: int) -> Json:
    return {_KIND_KEY: "int", "value": str(value)}


def int_from_json(value: Json) -> int:
    match value:
        case {"kind": "int", "value": int_value}:
            match int_value:
                case str():
                    return int(int_value)
                case _:
                    raise ValueError(f"Not an int: {int_value}")
        case _:
            raise ValueError(f"Not an int: {value}")


def unit_to_json(_: Unit) -> Json:
    return {_KIND_KEY: "unit"}


def unit_from_json(value: Json) -> Unit:
    match value:
        case {"kind": "unit"}:
            return unit
        case _:
            raise ValueError(f"Not a unit: {value}")


def bool_to_json(value: bool) -> Json:  # noqa: FBT001
    return value


def bool_from_json(value: Json) -> bool:
    match value:
        case bool():
            return value
        case _:
            raise ValueError(f"Not a bool: {value}")


def float_to_json(value: Decimal) -> Json:
    return {_KIND_KEY: "float", "value": str(value)}


def float_from_json(value: Json) -> Decimal:
    match value:
        case {"kind": "float", "value": float_value}:
            match float_value:
                case str():
                    return Decimal(float_value)
                case _:
                    raise ValueError(f"Not a float: {float_value}")
        case _:
            raise ValueError(f"Not a float: {value}")
