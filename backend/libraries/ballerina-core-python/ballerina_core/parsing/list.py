from __future__ import annotations

from typing import TypeVar

from ballerina_core.parsing.parsing_types import FromJson, Json, ToJson

_A = TypeVar("_A")


_KIND_KEY: str = "kind"


def list_to_json(item_to_json: ToJson[_A]) -> ToJson[list[_A]]:
    def to_json(value: list[_A]) -> Json:
        return {_KIND_KEY: "list", "elements": [item_to_json(item) for item in value]}

    return to_json


def list_from_json(item_from_json: FromJson[_A]) -> FromJson[list[_A]]:
    def from_json(value: Json) -> list[_A]:
        match value:
            case {"kind": "list", "elements": elements}:
                match elements:
                    case list():
                        return [item_from_json(item) for item in elements]
                    case _:
                        raise ValueError(f"Not a list: {elements}")
            case _:
                raise ValueError(f"Not a list: {value}")

    return from_json
