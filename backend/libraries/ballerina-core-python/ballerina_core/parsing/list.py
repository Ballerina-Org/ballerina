from __future__ import annotations

from collections.abc import Sequence
from functools import reduce
from typing import TypeVar

from ballerina_core.parsing.parsing_types import FromJson, Json, ParsingError, ToJson
from ballerina_core.sum import Sum

_A = TypeVar("_A")


_KIND_KEY: str = "kind"


def list_to_json(item_to_json: ToJson[_A]) -> ToJson[Sequence[_A]]:
    def to_json(value: Sequence[_A]) -> Json:
        return {_KIND_KEY: "list", "elements": [item_to_json(item) for item in value]}

    return to_json


def list_from_json(item_from_json: FromJson[_A]) -> FromJson[Sequence[_A]]:
    def from_json(value: Json) -> Sum[ParsingError, Sequence[_A]]:
        match value:
            case {"kind": "list", "elements": elements}:
                match elements:
                    case list():
                        return reduce(
                            lambda acc, item: acc.flat_map(
                                lambda items: item_from_json(item).map_right(lambda item: [*items, item])
                            ),
                            elements,
                            Sum.right([]),
                        )
                    case _:
                        return Sum.left(ParsingError.single(f"Not a list: {elements}"))
            case _:
                return Sum.left(ParsingError.single(f"Not a list: {value}"))

    return lambda value: from_json(value).map_left(ParsingError.append("Parsing list:"))
