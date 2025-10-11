from __future__ import annotations

from collections.abc import Callable
from dataclasses import dataclass
from typing import Generic, TypeVar

from typing_extensions import assert_never

_A = TypeVar("_A")
_B = TypeVar("_B")

_InnerA = TypeVar("_InnerA")
_InnerB = TypeVar("_InnerB")


@dataclass(frozen=True)
class Sum(Generic[_A, _B]):
    @dataclass(frozen=True)
    class Left(Generic[_InnerA]):
        value: _InnerA

    @dataclass(frozen=True)
    class Right(Generic[_InnerB]):
        value: _InnerB

    value: Left[_A] | Right[_B]

    _O = TypeVar("_O")

    @staticmethod
    def left(value: _A, /) -> Sum[_A, _B]:
        return Sum(Sum.Left(value))

    @staticmethod
    def right(value: _B, /) -> Sum[_A, _B]:
        return Sum(Sum.Right(value))

    def fold(self, on_left: Callable[[_A], _O], on_right: Callable[[_B], _O], /) -> _O:
        match self.value:
            case Sum.Left(value):
                return on_left(value)
            case Sum.Right(value):
                return on_right(value)
        assert_never(self.value)

    def map_left(self, on_left: Callable[[_A], _O], /) -> Sum[_O, _B]:
        return self.fold(lambda value: Sum.left(on_left(value)), Sum.right)

    def map_right(self, on_right: Callable[[_B], _O], /) -> Sum[_A, _O]:
        return self.fold(Sum.left, lambda value: Sum.right(on_right(value)))

    def flat_map(self, on_right: Callable[[_B], Sum[_A, _O]], /) -> Sum[_A, _O]:
        return self.fold(Sum.left, on_right)
