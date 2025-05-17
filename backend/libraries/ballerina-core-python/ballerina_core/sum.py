from __future__ import annotations

from collections.abc import Callable
from dataclasses import dataclass
from typing import Generic, TypeVar, assert_never

_SumL = TypeVar("_SumL")
_SumR = TypeVar("_SumR")

_Left = TypeVar("_Left")
_Right = TypeVar("_Right")


@dataclass(frozen=True)
class Sum(Generic[_SumL, _SumR]):
    @dataclass(frozen=True)
    class Left(Generic[_Left]):
        value: _Left

        _C = TypeVar("_C")

    @dataclass(frozen=True)
    class Right(Generic[_Right]):
        value: _Right

        _C = TypeVar("_C")

    value: Left[_SumL] | Right[_SumR]

    _O = TypeVar("_O")

    @staticmethod
    def left(value: _SumL, /) -> Sum[_SumL, _SumR]:
        return Sum(Sum.Left(value))

    @staticmethod
    def right(value: _SumR, /) -> Sum[_SumL, _SumR]:
        return Sum(Sum.Right(value))

    def fold(self, on_left: Callable[[_SumL], _O], on_right: Callable[[_SumR], _O], /) -> _O:
        match self.value:
            case Sum.Left(value):
                return on_left(value)
            case Sum.Right(value):
                return on_right(value)
        assert_never(self.value)

    def map_left(self, on_left: Callable[[_SumL], _O], /) -> Sum[_O, _SumR]:
        return self.fold(lambda value: Sum.left(on_left(value)), Sum.right)

    def map_right(self, on_right: Callable[[_SumR], _O], /) -> Sum[_SumL, _O]:
        return self.fold(Sum.left, lambda value: Sum.right(on_right(value)))

    def flat_map(self, on_right: Callable[[_SumR], Sum[_SumL, _O]], /) -> Sum[_SumL, _O]:
        return self.fold(Sum.left, on_right)
