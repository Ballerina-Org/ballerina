from __future__ import annotations

from collections.abc import Sequence
from functools import reduce
from typing import TypeVar

from ballerina_core.parsing.keys import DISCRIMINATOR_KEY, VALUE_KEY
from ballerina_core.parsing.parsing_types import FromJson, Json, ParsingError, ToJson
from ballerina_core.sum import Sum

_A = TypeVar("_A")


def list_to_json(item_to_json: ToJson[_A]) -> ToJson[Sequence[_A]]:
    def to_json(value: Sequence[_A]) -> Json:
        return {DISCRIMINATOR_KEY: "list", VALUE_KEY: [item_to_json(item) for item in value]}

    return to_json


def list_from_json(item_from_json: FromJson[_A]) -> FromJson[Sequence[_A]]:
    def from_json(value: Json) -> Sum[ParsingError, Sequence[_A]]:
        match value:
            case {"discriminator": "list", "value": elements}:
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
                return Sum.left(ParsingError.single(f"Invalid structure: {value}"))

    return lambda value: from_json(value).map_left(ParsingError.with_context("Parsing list:"))
