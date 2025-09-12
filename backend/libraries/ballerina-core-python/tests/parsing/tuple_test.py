import pytest
from typing_extensions import assert_never

from ballerina_core.parsing.parsing_types import Json
from ballerina_core.parsing.primitives import int_from_json, int_to_json
from ballerina_core.parsing.tuple import (
    tuple_2_from_json,
    tuple_2_to_json,
    tuple_3_from_json,
    tuple_3_to_json,
    tuple_4_from_json,
    tuple_4_to_json,
    tuple_5_from_json,
    tuple_5_to_json,
    tuple_6_from_json,
    tuple_6_to_json,
    tuple_7_from_json,
    tuple_7_to_json,
    tuple_8_from_json,
    tuple_8_to_json,
    tuple_9_from_json,
    tuple_9_to_json,
    tuple_10_from_json,
    tuple_10_to_json,
)
from ballerina_core.sum import Sum


def test_tuple_2_to_json() -> None:
    value = (1, 2)
    serializer = tuple_2_to_json(int_to_json, int_to_json)
    serialized = serializer(value)
    assert serialized == {"kind": "tuple", "elements": [int_to_json(1), int_to_json(2)]}


def test_tuple_2_from_json() -> None:
    serialized: Json = {"kind": "tuple", "elements": [int_to_json(1), int_to_json(2)]}
    parser = tuple_2_from_json(int_from_json, int_from_json)
    value = parser(serialized)
    assert value == Sum.right((1, 2))


def test_should_convert_tuple_to_and_from_json() -> None:
    value = (1, 2)
    serializer = tuple_2_to_json(int_to_json, int_to_json)
    parser = tuple_2_from_json(int_from_json, int_from_json)
    assert parser(serializer(value)) == Sum.right(value)


def test_tuple_3_roundtrip() -> None:
    value = (1, 2, 3)
    serializer = tuple_3_to_json(int_to_json, int_to_json, int_to_json)
    parser = tuple_3_from_json(int_from_json, int_from_json, int_from_json)
    assert parser(serializer(value)) == Sum.right(value)


def test_tuple_4_roundtrip() -> None:
    value = (1, 2, 3, 4)
    serializer = tuple_4_to_json(int_to_json, int_to_json, int_to_json, int_to_json)
    parser = tuple_4_from_json(int_from_json, int_from_json, int_from_json, int_from_json)
    assert parser(serializer(value)) == Sum.right(value)


def test_tuple_5_roundtrip() -> None:
    value = (1, 2, 3, 4, 5)
    serializer = tuple_5_to_json(int_to_json, int_to_json, int_to_json, int_to_json, int_to_json)
    parser = tuple_5_from_json(int_from_json, int_from_json, int_from_json, int_from_json, int_from_json)
    assert parser(serializer(value)) == Sum.right(value)


def test_tuple_6_roundtrip() -> None:
    value = (1, 2, 3, 4, 5, 6)
    serializer = tuple_6_to_json(int_to_json, int_to_json, int_to_json, int_to_json, int_to_json, int_to_json)
    parser = tuple_6_from_json(int_from_json, int_from_json, int_from_json, int_from_json, int_from_json, int_from_json)
    assert parser(serializer(value)) == Sum.right(value)


def test_tuple_7_roundtrip() -> None:
    value = (1, 2, 3, 4, 5, 6, 7)
    serializer = tuple_7_to_json(
        int_to_json, int_to_json, int_to_json, int_to_json, int_to_json, int_to_json, int_to_json
    )
    parser = tuple_7_from_json(
        int_from_json, int_from_json, int_from_json, int_from_json, int_from_json, int_from_json, int_from_json
    )
    assert parser(serializer(value)) == Sum.right(value)


def test_tuple_8_roundtrip() -> None:
    value = (1, 2, 3, 4, 5, 6, 7, 8)
    serializer = tuple_8_to_json(
        int_to_json, int_to_json, int_to_json, int_to_json, int_to_json, int_to_json, int_to_json, int_to_json
    )
    parser = tuple_8_from_json(
        int_from_json,
        int_from_json,
        int_from_json,
        int_from_json,
        int_from_json,
        int_from_json,
        int_from_json,
        int_from_json,
    )
    assert parser(serializer(value)) == Sum.right(value)


def test_tuple_9_roundtrip() -> None:
    value = (1, 2, 3, 4, 5, 6, 7, 8, 9)
    serializer = tuple_9_to_json(
        int_to_json,
        int_to_json,
        int_to_json,
        int_to_json,
        int_to_json,
        int_to_json,
        int_to_json,
        int_to_json,
        int_to_json,
    )
    parser = tuple_9_from_json(
        int_from_json,
        int_from_json,
        int_from_json,
        int_from_json,
        int_from_json,
        int_from_json,
        int_from_json,
        int_from_json,
        int_from_json,
    )
    assert parser(serializer(value)) == Sum.right(value)


def test_tuple_10_roundtrip() -> None:
    value = (1, 2, 3, 4, 5, 6, 7, 8, 9, 10)
    serializer = tuple_10_to_json(
        int_to_json,
        int_to_json,
        int_to_json,
        int_to_json,
        int_to_json,
        int_to_json,
        int_to_json,
        int_to_json,
        int_to_json,
        int_to_json,
    )
    parser = tuple_10_from_json(
        int_from_json,
        int_from_json,
        int_from_json,
        int_from_json,
        int_from_json,
        int_from_json,
        int_from_json,
        int_from_json,
        int_from_json,
        int_from_json,
    )
    assert parser(serializer(value)) == Sum.right(value)


def test_tuple_3_error_context_on_second_element() -> None:
    serialized: Json = {"kind": "tuple", "elements": [int_to_json(1), {"kind": "int", "value": 123}, int_to_json(3)]}
    parser = tuple_3_from_json(int_from_json, int_from_json, int_from_json)
    result = parser(serialized)
    match result.value:
        case Sum.Left(value):
            assert value.message() == "Parsing error at Parsing tuple:\n\tparsing element 2 of 3:\n\tNot an int: 123"
        case Sum.Right(_):
            pytest.fail("Expected an error")
        case _:
            assert_never(result.value)
