from decimal import Decimal

from typing_extensions import assert_never

from ballerina_core.parsing.keys import DISCRIMINATOR_KEY, VALUE_KEY
from ballerina_core.parsing.parsing_types import FromJson, Json
from ballerina_core.parsing.primitives import (
    decimal_from_json,
    decimal_to_json,
    int32_from_json,
    int32_to_json,
    string_from_json,
    string_to_json,
    unit_from_json,
    unit_to_json,
)
from ballerina_core.parsing.sum import (
    sum_2_from_json,
    sum_2_to_json,
    sum_3_from_json,
    sum_3_to_json,
    sum_4_from_json,
    sum_4_to_json,
)
from ballerina_core.sum import Sum, Sum3, Sum4
from ballerina_core.unit import Unit, unit


def test_sum_to_json_left() -> None:
    value = Sum[int, str].left(42)
    serializer = sum_2_to_json(int32_to_json, string_to_json)
    serialized = serializer(value)
    assert serialized == {DISCRIMINATOR_KEY: "sum", VALUE_KEY: [1, int32_to_json(42)]}


def test_sum_to_json_right() -> None:
    value = Sum[int, str].right("42")
    serializer = sum_2_to_json(int32_to_json, string_to_json)
    serialized = serializer(value)
    assert serialized == {DISCRIMINATOR_KEY: "sum", VALUE_KEY: [2, string_to_json("42")]}


def test_sum_from_json_left() -> None:
    serialized: Json = {DISCRIMINATOR_KEY: "sum", VALUE_KEY: [1, int32_to_json(42)]}
    parser: FromJson[Sum[int, str]] = sum_2_from_json(int32_from_json, string_from_json)
    value = parser(serialized)
    print(value)
    assert value == Sum.right(Sum[int, str].left(42))


def test_sum_from_json_right() -> None:
    serialized: Json = {DISCRIMINATOR_KEY: "sum", VALUE_KEY: [2, string_to_json("42")]}
    parser: FromJson[Sum[int, str]] = sum_2_from_json(int32_from_json, string_from_json)
    value = parser(serialized)
    assert value == Sum.right(Sum[int, str].right("42"))


def test_sum3_to_json_1of3() -> None:
    value = Sum3[int, str, Unit].sum1of3(42)
    serializer = sum_3_to_json(int32_to_json, string_to_json, unit_to_json)
    serialized = serializer(value)
    assert serialized == {DISCRIMINATOR_KEY: "sum", VALUE_KEY: [1, int32_to_json(42)]}


def test_sum3_to_json_2of3() -> None:
    value = Sum3[int, str, Unit].sum2of3("42")
    serializer = sum_3_to_json(int32_to_json, string_to_json, unit_to_json)
    serialized = serializer(value)
    assert serialized == {DISCRIMINATOR_KEY: "sum", VALUE_KEY: [2, string_to_json("42")]}


def test_sum3_to_json_3of3() -> None:
    value = Sum3[int, str, Unit].sum3of3(unit)
    serializer = sum_3_to_json(int32_to_json, string_to_json, unit_to_json)
    serialized = serializer(value)
    assert serialized == {DISCRIMINATOR_KEY: "sum", VALUE_KEY: [3, unit_to_json(unit)]}


def test_sum3_from_json_1of3() -> None:
    serialized: Json = {DISCRIMINATOR_KEY: "sum", VALUE_KEY: [1, int32_to_json(42)]}
    parser: FromJson[Sum3[int, str, Unit]] = sum_3_from_json(int32_from_json, string_from_json, unit_from_json)
    value = parser(serialized)
    assert value == Sum.right(Sum3[int, str, Unit].sum1of3(42))


def test_sum3_from_json_2of3() -> None:
    serialized: Json = {DISCRIMINATOR_KEY: "sum", VALUE_KEY: [2, string_to_json("42")]}
    parser: FromJson[Sum3[int, str, Unit]] = sum_3_from_json(int32_from_json, string_from_json, unit_from_json)
    value = parser(serialized)
    assert value == Sum.right(Sum3[int, str, Unit].sum2of3("42"))


def test_sum3_from_json_3of3() -> None:
    serialized: Json = {DISCRIMINATOR_KEY: "sum", VALUE_KEY: [3, unit_to_json(unit)]}
    parser: FromJson[Sum3[int, str, Unit]] = sum_3_from_json(int32_from_json, string_from_json, unit_from_json)
    value = parser(serialized)
    assert value == Sum.right(Sum3[int, str, Unit].sum3of3(unit))


def test_sum3_from_json_invalid_discriminator() -> None:
    serialized: Json = {DISCRIMINATOR_KEY: "sum", VALUE_KEY: [4, int32_to_json(42)]}
    parser: FromJson[Sum3[int, str, Unit]] = sum_3_from_json(int32_from_json, string_from_json, unit_from_json)
    value = parser(serialized)
    match value.value:
        case Sum.Left(error):
            assert "Invalid case tuple: invalid discriminator" in error.message()
        case Sum.Right(_):
            raise AssertionError("Expected an error")
        case _:
            assert_never(value.value)


def test_sum4_to_json_1of4() -> None:
    value = Sum4[int, str, Unit, Decimal].sum1of4(42)
    serializer = sum_4_to_json(int32_to_json, string_to_json, unit_to_json, decimal_to_json)
    serialized = serializer(value)
    assert serialized == {DISCRIMINATOR_KEY: "sum", VALUE_KEY: [1, int32_to_json(42)]}


def test_sum4_to_json_2of4() -> None:
    value = Sum4[int, str, Unit, Decimal].sum2of4("42")
    serializer = sum_4_to_json(int32_to_json, string_to_json, unit_to_json, decimal_to_json)
    serialized = serializer(value)
    assert serialized == {DISCRIMINATOR_KEY: "sum", VALUE_KEY: [2, string_to_json("42")]}


def test_sum4_to_json_3of4() -> None:
    value = Sum4[int, str, Unit, Decimal].sum3of4(unit)
    serializer = sum_4_to_json(int32_to_json, string_to_json, unit_to_json, decimal_to_json)
    serialized = serializer(value)
    assert serialized == {DISCRIMINATOR_KEY: "sum", VALUE_KEY: [3, unit_to_json(unit)]}


def test_sum4_to_json_4of4() -> None:
    value = Sum4[int, str, Unit, Decimal].sum4of4(Decimal("3.14"))
    serializer = sum_4_to_json(int32_to_json, string_to_json, unit_to_json, decimal_to_json)
    serialized = serializer(value)
    assert serialized == {DISCRIMINATOR_KEY: "sum", VALUE_KEY: [4, {"discriminator": "decimal", "value": "3.14"}]}


def test_sum4_from_json_1of4() -> None:
    serialized: Json = {DISCRIMINATOR_KEY: "sum", VALUE_KEY: [1, int32_to_json(42)]}
    parser: FromJson[Sum4[int, str, Unit, Decimal]] = sum_4_from_json(
        int32_from_json, string_from_json, unit_from_json, decimal_from_json
    )
    value = parser(serialized)
    assert value == Sum.right(Sum4[int, str, Unit, Decimal].sum1of4(42))


def test_sum4_from_json_2of4() -> None:
    serialized: Json = {DISCRIMINATOR_KEY: "sum", VALUE_KEY: [2, string_to_json("42")]}
    parser: FromJson[Sum4[int, str, Unit, Decimal]] = sum_4_from_json(
        int32_from_json, string_from_json, unit_from_json, decimal_from_json
    )
    value = parser(serialized)
    assert value == Sum.right(Sum4[int, str, Unit, Decimal].sum2of4("42"))


def test_sum4_from_json_3of4() -> None:
    serialized: Json = {DISCRIMINATOR_KEY: "sum", VALUE_KEY: [3, unit_to_json(unit)]}
    parser: FromJson[Sum4[int, str, Unit, Decimal]] = sum_4_from_json(
        int32_from_json, string_from_json, unit_from_json, decimal_from_json
    )
    value = parser(serialized)
    assert value == Sum.right(Sum4[int, str, Unit, Decimal].sum3of4(unit))


def test_sum4_from_json_4of4() -> None:
    serialized: Json = {DISCRIMINATOR_KEY: "sum", VALUE_KEY: [4, decimal_to_json(Decimal("3.14"))]}
    parser: FromJson[Sum4[int, str, Unit, Decimal]] = sum_4_from_json(
        int32_from_json, string_from_json, unit_from_json, decimal_from_json
    )
    value = parser(serialized)
    assert value == Sum.right(Sum4[int, str, Unit, Decimal].sum4of4(Decimal("3.14")))


def test_sum4_from_json_invalid_discriminator() -> None:
    serialized: Json = {DISCRIMINATOR_KEY: "sum", VALUE_KEY: [5, int32_to_json(42)]}
    parser: FromJson[Sum4[int, str, Unit, Decimal]] = sum_4_from_json(
        int32_from_json, string_from_json, unit_from_json, decimal_from_json
    )
    value = parser(serialized)
    match value.value:
        case Sum.Left(error):
            assert "Invalid case tuple: invalid discriminator" in error.message()
        case Sum.Right(_):
            raise AssertionError("Expected an error")
        case _:
            assert_never(value.value)
