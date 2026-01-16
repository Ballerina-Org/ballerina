from __future__ import annotations

from collections.abc import Sequence
from functools import reduce
from typing import TypeVar

from typing_extensions import assert_never

from ballerina_core.fun import identity
from ballerina_core.parsing.keys import DISCRIMINATOR_KEY, VALUE_KEY
from ballerina_core.parsing.parsing_types import FromJson, Json, ParsingError, ToJson
from ballerina_core.parsing.primitives import unit_to_json
from ballerina_core.parsing.tuple import tuple_2_to_json
from ballerina_core.sum import Sum
from ballerina_core.unit import unit

_A = TypeVar("_A")


def list_to_json(item_to_json: ToJson[_A]) -> ToJson[Sequence[_A]]:
    def identifier_to_json(identifier: str) -> Json:
        return {DISCRIMINATOR_KEY: "lookup", VALUE_KEY: {DISCRIMINATOR_KEY: "id", "value": ("List", identifier)}}

    cons_constructor = identifier_to_json("Cons")
    nil: Json = {DISCRIMINATOR_KEY: "apply", VALUE_KEY: (identifier_to_json("Nil"), unit_to_json(unit))}

    def cons(head: _A, tail: Json) -> Json:
        return {
            DISCRIMINATOR_KEY: "apply",
            VALUE_KEY: (cons_constructor, tuple_2_to_json(item_to_json, identity)((head, tail))),
        }

    def to_json(value: Sequence[_A]) -> Json:
        as_list = list(value)
        match as_list:
            case []:
                return nil
            case [head, *tail]:
                return cons(head, to_json(tail))
        assert_never(as_list)  # type: ignore[arg-type]

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
