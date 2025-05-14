from __future__ import annotations

from typing import TypeVar

from ballerina_core.parsing.parsing_types import FromJson, Json, ToJson

_A = TypeVar("_A")
_B = TypeVar("_B")


def tuple1_to_json(a_to_json: ToJson[_A]) -> ToJson[tuple[_A]]:
    def to_json(value: tuple[_A]) -> Json:
        return {"Item0": a_to_json(value[0])}

    return to_json


def tuple1_from_json(a_from_json: FromJson[_A]) -> FromJson[tuple[_A]]:
    def from_json(value: Json) -> tuple[_A]:
        match value:
            case dict():
                if "Item0" not in value:
                    raise ValueError(f"Missing item key: {value}")
                return (a_from_json(value["Item0"]),)
            case _:
                raise ValueError(f"Not a dictionary: {value}")

    return from_json


def tuple2_to_json(a_to_json: ToJson[_A], b_to_json: ToJson[_B]) -> ToJson[tuple[_A, _B]]:
    def to_json(value: tuple[_A, _B]) -> Json:
        return {"Item0": a_to_json(value[0]), "Item1": b_to_json(value[1])}

    return to_json


def tuple2_from_json(a_from_json: FromJson[_A], b_from_json: FromJson[_B]) -> FromJson[tuple[_A, _B]]:
    def from_json(value: Json) -> tuple[_A, _B]:
        match value:
            case dict():
                if "Item0" not in value or "Item1" not in value:
                    raise ValueError(f"Missing item key: {value}")
                return (a_from_json(value["Item0"]), b_from_json(value["Item1"]))
            case _:
                raise ValueError(f"Not a dictionary: {value}")

    return from_json
