from typing import TypeVar

from ballerina_core.option import Option
from ballerina_core.parsing.keys import DISCRIMINATOR_KEY, VALUE_KEY
from ballerina_core.parsing.parsing_types import FromJson, Json, ParsingError, ToJson
from ballerina_core.sum import Sum
from ballerina_core.unit import Unit, unit

_Option = TypeVar("_Option")

_OPTION_SOME_CASE: str = "some"
_OPTION_NONE_CASE: str = "none"


def option_to_json(some_to_json: ToJson[_Option], unit_to_json: ToJson[Unit], /) -> ToJson[Option[_Option]]:
    def to_json(value: Option[_Option]) -> Json:
        return {
            DISCRIMINATOR_KEY: "union",
            VALUE_KEY: value.fold(
                lambda: [_OPTION_NONE_CASE, unit_to_json(unit)], lambda a: [_OPTION_SOME_CASE, some_to_json(a)]
            ),
        }

    return to_json


def option_from_json(some_from_json: FromJson[_Option], unit_from_json: FromJson[Unit], /) -> FromJson[Option[_Option]]:
    def from_json(value: Json) -> Sum[ParsingError, Option[_Option]]:
        match value:
            case {"discriminator": "union", "value": [case_name, case_value]}:
                match case_name:
                    case "some":
                        return some_from_json(case_value).map_right(Option.some)
                    case "none":
                        return unit_from_json(case_value).map_right(lambda _: Option.none())
                    case _:
                        return Sum.left(ParsingError.single(f"Invalid option case: {case_name}"))
            case _:
                return Sum.left(ParsingError.single(f"Invalid option structure: {value}"))

    return lambda value: from_json(value).map_left(ParsingError.with_context("Parsing option:"))
