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


_Option = TypeVar("_Option")
_Some = TypeVar("_Some")
_Nothing = TypeVar("_Nothing")


@dataclass(frozen=True)
class Option(Generic[_Option]):
    @dataclass(frozen=True)
    class Some(Generic[_Some]):
        value: _Some

        _C = TypeVar("_C")

    @dataclass(frozen=True)
    class Nothing(Generic[_Nothing]):
        pass

    value: Some[_Option] | Nothing[_Option]

    _O = TypeVar("_O")

    def fold(self, on_some: Callable[[_Option], _O], on_nothing: Callable[[], _O], /) -> _O:
        match self.value:
            case Option.Some(value):
                return on_some(value)
            case Option.Nothing():
                return on_nothing()
        assert_never(self.value)
