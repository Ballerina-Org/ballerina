from typing import TypeVar

from ballerina_core.parsing.parsing_types import FromJson, Json, ParsingError, ToJson
from ballerina_core.sum import Sum

_A = TypeVar("_A")
_B = TypeVar("_B")


def tuple_2_from_json(a_parser: FromJson[_A], b_parser: FromJson[_B]) -> FromJson[tuple[_A, _B]]:
    def from_json(value: Json) -> Sum[ParsingError, tuple[_A, _B]]:
        length = 2
        match value:
            case {"kind": "tuple", "elements": elements}:
                match elements:
                    case list():
                        if len(elements) == length:
                            return a_parser(elements[0]).flat_map(
                                lambda a: b_parser(elements[1]).map_right(lambda b: (a, b))
                            )
                        return Sum.left(
                            ParsingError.single(f"Expected {length} elements in tuple, got {len(elements)}")
                        )
                    case _:
                        return Sum.left(ParsingError.single(f"Not a tuple: {elements}"))
            case _:
                return Sum.left(ParsingError.single(f"Not a tuple: {value}"))

    return lambda value: from_json(value).map_left(ParsingError.with_context("Parsing tuple:"))


def tuple_2_to_json(a_to_json: ToJson[_A], b_to_json: ToJson[_B]) -> ToJson[tuple[_A, _B]]:
    def to_json(value: tuple[_A, _B]) -> Json:
        a, b = value
        return {"kind": "tuple", "elements": [a_to_json(a), b_to_json(b)]}

    return to_json
