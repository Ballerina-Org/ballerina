from __future__ import annotations

import datetime
from decimal import Decimal, InvalidOperation

from ballerina_core.parsing.discriminated import discriminated_to_json, discriminated_value_from_json
from ballerina_core.parsing.parsing_types import Json, ParsingError
from ballerina_core.sum import Sum
from ballerina_core.unit import Unit, unit

UNIT_VALUE = "()"


def string_to_json(value: str) -> Json:
    return discriminated_to_json("string", value)


def string_from_json(value: Json) -> Sum[ParsingError, str]:
    return discriminated_value_from_json(value, "string").flat_map(
        lambda string_value: (
            Sum.right(string_value)
            if isinstance(string_value, str)
            else Sum.left(ParsingError.single(f"Not a string: {string_value}"))
        )
    )


def int32_to_json(value: int) -> Json:
    return discriminated_to_json("int32", str(value))


def int32_from_json(value: Json) -> Sum[ParsingError, int]:
    return discriminated_value_from_json(value, "int32").flat_map(
        lambda int_value: (
            Sum.right(int(int_value))
            if isinstance(int_value, str)
            else Sum.left(ParsingError.single(f"Not an int: {int_value}"))
        )
    )


def unit_to_json(_: Unit) -> Json:
    return discriminated_to_json("unit", UNIT_VALUE)


def unit_from_json(value: Json) -> Sum[ParsingError, Unit]:
    return discriminated_value_from_json(value, "unit").flat_map(
        lambda unit_value: (
            Sum.right(unit)
            if unit_value == UNIT_VALUE
            else Sum.left(ParsingError.single(f"Invalid structure: {value}"))
        )
    )


def bool_to_json(value: bool) -> Json:  # noqa: FBT001
    return discriminated_to_json("bool", value)


def bool_from_json(value: Json) -> Sum[ParsingError, bool]:
    return discriminated_value_from_json(value, "bool").flat_map(
        lambda bool_value: (
            Sum.right(bool_value)
            if isinstance(bool_value, bool)
            else Sum.left(ParsingError.single(f"Not a bool: {bool_value}"))
        )
    )


def decimal_to_json(value: Decimal) -> Json:
    return discriminated_to_json("decimal", str(value))


def decimal_from_json(value: Json) -> Sum[ParsingError, Decimal]:
    def parse_decimal(float_value: Json) -> Sum[ParsingError, Decimal]:
        match float_value:
            case str():
                try:
                    return Sum.right(Decimal(float_value))
                except InvalidOperation:
                    return Sum.left(ParsingError.single(f"Not a float: {float_value}"))
            case _:
                return Sum.left(ParsingError.single(f"Not a string: {float_value}"))

    return discriminated_value_from_json(value, "decimal").flat_map(parse_decimal)


def date_to_json(value: datetime.date) -> Json:
    return discriminated_to_json("date", value.isoformat())


def date_from_json(value: Json) -> Sum[ParsingError, datetime.date]:
    def parse_date(date_value: Json) -> Sum[ParsingError, datetime.date]:
        match date_value:
            case str():
                try:
                    return Sum.right(datetime.date.fromisoformat(date_value))
                except ValueError as e:
                    return Sum.left(ParsingError.single(f"Invalid date: {date_value} ({e})"))
            case _:
                return Sum.left(ParsingError.single(f"Date is not a string: {date_value}"))

    return discriminated_value_from_json(value, "date").flat_map(parse_date)
