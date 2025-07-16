from typing import TypeVar

from ballerina_core.option import Option
from ballerina_core.parsing.parsing_types import FromJson, Json, ParsingError, ToJson
from ballerina_core.sum import Sum
from ballerina_core.unit import Unit, unit

_Option = TypeVar("_Option")

_CASE_KEY: str = "case"
_VALUE_KEY: str = "value"
_SOME_VALUE: str = "some"
_NONE_VALUE: str = "none"


def option_to_json(some_to_json: ToJson[_Option], none_to_json: ToJson[Unit], /) -> ToJson[Option[_Option]]:
    def to_json(value: Option[_Option]) -> Json:
        return value.fold(
            lambda a: {_CASE_KEY: _SOME_VALUE, _VALUE_KEY: some_to_json(a)},
            lambda: {_CASE_KEY: _NONE_VALUE, _VALUE_KEY: none_to_json(unit)},
        )

    return to_json


def option_from_json(some_from_json: FromJson[_Option], none_from_json: FromJson[Unit], /) -> FromJson[Option[_Option]]:
    def from_json(value: Json) -> Sum[ParsingError, Option[_Option]]:  # noqa: PLR0911
        match value:
            case dict():
                if _CASE_KEY not in value:
                    return Sum.left(ParsingError.single(f"Missing case: {value}"))
                match value[_CASE_KEY]:
                    case discriminator if discriminator == _SOME_VALUE:
                        if _VALUE_KEY not in value:
                            return Sum.left(ParsingError.single(f"Missing value: {value}"))
                        return some_from_json(value[_VALUE_KEY]).map_right(Option.some)
                    case discriminator if discriminator == _NONE_VALUE:
                        if _VALUE_KEY not in value:
                            return Sum.left(ParsingError.single(f"Missing value: {value}"))
                        return none_from_json(value[_VALUE_KEY]).map_right(lambda _: Option.none())
                    case _:
                        return Sum.left(ParsingError.single(f"Invalid discriminator: {value}"))
            case _:
                return Sum.left(ParsingError.single(f"Not a dictionary: {value}"))

    return lambda value: from_json(value).map_left(ParsingError.with_context("Parsing option:"))
