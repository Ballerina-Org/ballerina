import operator
from collections.abc import Sequence

import pytest

from ballerina_core.sum import Sum


def test_left_constructor() -> None:
    expected = 42
    sum_left: Sum[int, str] = Sum.left(expected)
    match sum_left.value:
        case Sum.Left(value):
            assert value == expected
        case Sum.Right(_):
            pytest.fail("Expected a Left")


def test_right_constructor() -> None:
    expected = "hello"
    sum_right: Sum[int, str] = Sum.right(expected)
    match sum_right.value:
        case Sum.Right(value):
            assert value == expected
        case Sum.Left(_):
            pytest.fail("Expected a Right")


def test_fold_left() -> None:
    sum_left: Sum[int, str] = Sum.left(42)
    result = sum_left.fold(lambda x: x * 2, len)

    expected = 84
    assert result == expected


def test_fold_right() -> None:
    sum_right: Sum[int, str] = Sum.right("hello")
    result = sum_right.fold(lambda x: x * 2, len)

    expected = 5
    assert result == expected


Error = int
error_merge = operator.add


class TestSequenceAll:
    @staticmethod
    def test_on_error() -> None:
        values: Sequence[Sum[Error, str]] = [Sum.left(1), Sum.right("a"), Sum.left(2)]
        result = Sum[Error, str].sequence_all(error_merge)(values)

        expected = Sum[Error, Sequence[str]].left(3)
        assert result == expected

    @staticmethod
    def test_on_success() -> None:
        values: Sequence[Sum[Error, str]] = [Sum.right("a"), Sum.right("b"), Sum.right("c")]
        result = Sum[Error, str].sequence_all(error_merge)(values)

        expected = Sum[Error, Sequence[str]].right(("a", "b", "c"))
        assert result == expected
