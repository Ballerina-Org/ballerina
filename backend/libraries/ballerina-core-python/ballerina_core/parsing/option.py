from typing import TypeVar

from ballerina_core.option import Option
from ballerina_core.parsing.parsing_types import FromJson, Json, ToJson

_Option = TypeVar("_Option")

_CASE_KEY: str = "case"
_VALUE_KEY: str = "value"
_SOME_VALUE: str = "some"
_NOTHING_VALUE: str = "nothing"


def option_to_json(some_to_json: ToJson[_Option], /) -> ToJson[Option[_Option]]:
    def to_json(value: Option[_Option]) -> Json:
        none: Json = None  # needed because dictionaries are invariant
        return value.fold(
            lambda a: {_CASE_KEY: _SOME_VALUE, _VALUE_KEY: some_to_json(a)},
            lambda: {_CASE_KEY: _NOTHING_VALUE, _VALUE_KEY: none},
        )

    return to_json


def option_from_json(some_from_json: FromJson[_Option], /) -> FromJson[Option[_Option]]:
    def from_json(value: Json) -> Option[_Option]:
        match value:
            case dict():
                if _CASE_KEY not in value:
                    raise ValueError(f"Missing case: {value}")
                match value[_CASE_KEY]:
                    case discriminator if discriminator == _SOME_VALUE:
                        if _VALUE_KEY not in value:
                            raise ValueError(f"Missing value: {value}")
                        return Option.some(some_from_json(value[_VALUE_KEY]))
                    case discriminator if discriminator == _NOTHING_VALUE:
                        return Option.nothing()
                    case _:
                        raise ValueError(f"Invalid discriminator: {value}")
            case _:
                raise ValueError(f"Not a dictionary: {value}")

    return from_json
