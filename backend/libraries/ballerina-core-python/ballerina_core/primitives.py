from __future__ import annotations

from collections.abc import Callable
from dataclasses import dataclass
from typing import Generic, TypeVar, assert_never

Unit = tuple[()]


_SumL = TypeVar("_SumL")
_SumR = TypeVar("_SumR")

_L = TypeVar("_L")
_R = TypeVar("_R")


@dataclass(frozen=True)
class Sum(Generic[_SumL, _SumR]):
    @dataclass(frozen=True)
    class Left(Generic[_L]):
        value: _L

        _C = TypeVar("_C")

    @dataclass(frozen=True)
    class Right(Generic[_R]):
        value: _R

        _C = TypeVar("_C")

    value: Left[_SumL] | Right[_SumR]

    _O = TypeVar("_O")

    def fold(self, on_left: Callable[[_SumL], _O], on_right: Callable[[_SumR], _O], /) -> _O:
        match self.value:
            case Sum.Left(value):
                return on_left(value)
            case Sum.Right(value):
                return on_right(value)
        assert_never(self.value)


_T1 = TypeVar("_T1")
_T2 = TypeVar("_T2")
