import pytest

from ballerina_core.sum import Sum


class TestSum:
    @staticmethod
    def test_left_constructor() -> None:
        expected = 42
        sum_left: Sum[int, str] = Sum.left(expected)
        match sum_left.value:
            case Sum.Left(value):
                assert value == expected
            case Sum.Right(_):
                pytest.fail("Expected a Left")

    @staticmethod
    def test_right_constructor() -> None:
        expected = "hello"
        sum_right: Sum[int, str] = Sum.right(expected)
        match sum_right.value:
            case Sum.Right(value):
                assert value == expected
            case Sum.Left(_):
                pytest.fail("Expected a Right")

    @staticmethod
    def test_fold_left() -> None:
        sum_left: Sum[int, str] = Sum.left(42)
        result = sum_left.fold(lambda x: x * 2, len)

        expected = 84
        assert result == expected

    @staticmethod
    def test_fold_right() -> None:
        sum_right: Sum[int, str] = Sum.right("hello")
        result = sum_right.fold(lambda x: x * 2, len)

        expected = 5
        assert result == expected
