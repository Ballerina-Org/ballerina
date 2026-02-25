from __future__ import annotations

from collections.abc import Sequence
from functools import reduce
from typing import TypeVar

from ballerina_core.parsing.discriminated import discriminated_to_json, discriminated_value_from_json
from ballerina_core.parsing.parsing_types import FromJson, Json, ParsingError, ToJson
from ballerina_core.sum import Sum

_A = TypeVar("_A")


def list_to_json(item_to_json: ToJson[_A]) -> ToJson[Sequence[_A]]:
    def to_json(value: Sequence[_A]) -> Json:
        return discriminated_to_json("list", [item_to_json(item) for item in value])

    return to_json


def list_from_json(item_from_json: FromJson[_A]) -> FromJson[Sequence[_A]]:
    def from_json(value: Json) -> Sum[ParsingError, Sequence[_A]]:
        def parse_elements(elements: Json) -> Sum[ParsingError, Sequence[_A]]:
            match elements:
                case list():
                    return reduce(
                        lambda acc, item: acc.flat_map(
                            lambda items: item_from_json(item).map_right(lambda parsed_item: [*items, parsed_item])
                        ),
                        elements,
                        Sum.right([]),
                    )
                case _:
                    return Sum.left(ParsingError.single(f"Not a list: {elements}"))

        return discriminated_value_from_json(value, "list").flat_map(parse_elements)

    return lambda value: from_json(value).map_left(ParsingError.with_context("Parsing list:"))
