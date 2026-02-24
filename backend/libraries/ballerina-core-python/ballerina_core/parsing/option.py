from typing import TypeVar

from ballerina_core.option import Option
from ballerina_core.parsing.discriminated import discriminated_to_json, discriminated_value_from_json
from ballerina_core.parsing.parsing_types import FromJson, Json, ParsingError, ToJson
from ballerina_core.sum import Sum
from ballerina_core.unit import Unit, unit

_Option = TypeVar("_Option")

_OPTION_SOME_CASE: str = "some"
_OPTION_NONE_CASE: str = "none"


def option_to_json(some_to_json: ToJson[_Option], unit_to_json: ToJson[Unit], /) -> ToJson[Option[_Option]]:
    def to_json(value: Option[_Option]) -> Json:
        return discriminated_to_json(
            "union",
            value.fold(
                lambda: [_OPTION_NONE_CASE, unit_to_json(unit)], lambda a: [_OPTION_SOME_CASE, some_to_json(a)]
            ),
        )

    return to_json


def option_from_json(some_from_json: FromJson[_Option], unit_from_json: FromJson[Unit], /) -> FromJson[Option[_Option]]:
    def from_json(value: Json) -> Sum[ParsingError, Option[_Option]]:
        return discriminated_value_from_json(
            value, "union", invalid_structure_prefix="Invalid option structure"
        ).flat_map(parse_case)

    def parse_case(case_payload: Json) -> Sum[ParsingError, Option[_Option]]:
        match case_payload:
            case [case_name, case_value]:
                match case_name:
                    case "some":
                        return some_from_json(case_value).map_right(Option.some)
                    case "none":
                        return unit_from_json(case_value).map_right(lambda _: Option.none())
                    case _:
                        return Sum.left(ParsingError.single(f"Invalid option case: {case_name}"))
            case _:
                return Sum.left(ParsingError.single(f"Invalid option structure: {case_payload}"))

    return lambda value: from_json(value).map_left(ParsingError.with_context("Parsing option:"))
