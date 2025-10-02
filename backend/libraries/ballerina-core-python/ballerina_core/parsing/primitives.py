from __future__ import annotations

import datetime
from decimal import Decimal, InvalidOperation

from ballerina_core.parsing.keys import DISCRIMINATOR_KEY, VALUE_KEY
from ballerina_core.parsing.parsing_types import Json, ParsingError
from ballerina_core.sum import Sum
from ballerina_core.unit import Unit, unit


def string_to_json(value: str) -> Json:
    return {DISCRIMINATOR_KEY: "string", VALUE_KEY: value}


def string_from_json(value: Json) -> Sum[ParsingError, str]:
    match value:
        case {"discriminator": "string", "value": string_value}:
            match string_value:
                case str():
                    return Sum.right(string_value)
                case _:
                    return Sum.left(ParsingError.single(f"Not a string: {string_value}"))
        case _:
            return Sum.left(ParsingError.single(f"Invalid structure: {value}"))


def int32_to_json(value: int) -> Json:
    return {DISCRIMINATOR_KEY: "int32", VALUE_KEY: str(value)}


def int32_from_json(value: Json) -> Sum[ParsingError, int]:
    match value:
        case {"discriminator": "int32", "value": int_value}:
            match int_value:
                case str():
                    return Sum.right(int(int_value))
                case _:
                    return Sum.left(ParsingError.single(f"Not an int: {int_value}"))
        case _:
            return Sum.left(ParsingError.single(f"Invalid structure: {value}"))


def unit_to_json(_: Unit) -> Json:
    return {DISCRIMINATOR_KEY: "unit"}


def unit_from_json(value: Json) -> Sum[ParsingError, Unit]:
    match value:
        case {"discriminator": "unit"}:
            return Sum.right(unit)
        case _:
            return Sum.left(ParsingError.single(f"Invalid structure: {value}"))


def bool_to_json(value: bool) -> Json:  # noqa: FBT001
    return {DISCRIMINATOR_KEY: "bool", VALUE_KEY: value}


def bool_from_json(value: Json) -> Sum[ParsingError, bool]:
    match value:
        case {"discriminator": "bool", "value": bool_value}:
            match bool_value:
                case bool():
                    return Sum.right(bool_value)
                case _:
                    return Sum.left(ParsingError.single(f"Not a bool: {bool_value}"))
        case _:
            return Sum.left(ParsingError.single(f"Invalid structure: {value}"))


def float32_to_json(value: Decimal) -> Json:
    return {DISCRIMINATOR_KEY: "float32", VALUE_KEY: str(value)}


def float32_from_json(value: Json) -> Sum[ParsingError, Decimal]:
    match value:
        case {"discriminator": "float32", "value": float_value}:
            match float_value:
                case str():
                    try:
                        return Sum.right(Decimal(float_value))
                    except InvalidOperation:
                        return Sum.left(ParsingError.single(f"Not a float: {float_value}"))
                case _:
                    return Sum.left(ParsingError.single(f"Not a string: {float_value}"))
        case _:
            return Sum.left(ParsingError.single(f"Invalid structure: {value}"))


def date_to_json(value: datetime.date) -> Json:
    return {DISCRIMINATOR_KEY: "date", VALUE_KEY: value.isoformat()}


def date_from_json(value: Json) -> Sum[ParsingError, datetime.date]:
    match value:
        case {"discriminator": "date", "value": date_value}:
            match date_value:
                case str():
                    try:
                        return Sum.right(datetime.date.fromisoformat(date_value))
                    except ValueError as e:
                        return Sum.left(ParsingError.single(f"Invalid date: {date_value} ({e})"))
                case _:
                    return Sum.left(ParsingError.single(f"Date is not a string: {date_value}"))
        case _:
            return Sum.left(ParsingError.single(f"Invalid structure: {value}"))
