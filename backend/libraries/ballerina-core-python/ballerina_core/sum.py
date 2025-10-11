from __future__ import annotations

from collections.abc import Callable
from dataclasses import dataclass
from typing import Generic, TypeVar

from typing_extensions import assert_never

_A = TypeVar("_A")
_B = TypeVar("_B")
_C = TypeVar("_C")
_D = TypeVar("_D")

_InnerA = TypeVar("_InnerA")
_InnerB = TypeVar("_InnerB")
_InnerC = TypeVar("_InnerC")
_InnerD = TypeVar("_InnerD")


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


@dataclass(frozen=True)
class Sum3(Generic[_A, _B, _C]):
    @dataclass(frozen=True)
    class Sum1Of3(Generic[_InnerA]):
        value: _InnerA

    @dataclass(frozen=True)
    class Sum2Of3(Generic[_InnerB]):
        value: _InnerB

    @dataclass(frozen=True)
    class Sum3Of3(Generic[_InnerC]):
        value: _InnerC

    value: Sum1Of3[_A] | Sum2Of3[_B] | Sum3Of3[_C]

    _O = TypeVar("_O")

    @staticmethod
    def sum1of3(value: _A, /) -> Sum3[_A, _B, _C]:
        return Sum3(Sum3.Sum1Of3(value))

    @staticmethod
    def sum2of3(value: _B, /) -> Sum3[_A, _B, _C]:
        return Sum3(Sum3.Sum2Of3(value))

    @staticmethod
    def sum3of3(value: _C, /) -> Sum3[_A, _B, _C]:
        return Sum3(Sum3.Sum3Of3(value))

    def fold(self, on_1of3: Callable[[_A], _O], on_2of3: Callable[[_B], _O], on_3of3: Callable[[_C], _O], /) -> _O:
        match self.value:
            case Sum3.Sum1Of3(value):
                return on_1of3(value)
            case Sum3.Sum2Of3(value):
                return on_2of3(value)
            case Sum3.Sum3Of3(value):
                return on_3of3(value)
        assert_never(self.value)

    def map_1of3(self, on_1of3: Callable[[_A], _O], /) -> Sum3[_O, _B, _C]:
        return self.fold(lambda value: Sum3.sum1of3(on_1of3(value)), Sum3.sum2of3, Sum3.sum3of3)

    def map_2of3(self, on_2of3: Callable[[_B], _O], /) -> Sum3[_A, _O, _C]:
        return self.fold(Sum3.sum1of3, lambda value: Sum3.sum2of3(on_2of3(value)), Sum3.sum3of3)

    def map_3of3(self, on_3of3: Callable[[_C], _O], /) -> Sum3[_A, _B, _O]:
        return self.fold(Sum3.sum1of3, Sum3.sum2of3, lambda value: Sum3.sum3of3(on_3of3(value)))


@dataclass(frozen=True)
class Sum4(Generic[_A, _B, _C, _D]):
    @dataclass(frozen=True)
    class Sum1Of4(Generic[_InnerA]):
        value: _InnerA

    @dataclass(frozen=True)
    class Sum2Of4(Generic[_InnerB]):
        value: _InnerB

    @dataclass(frozen=True)
    class Sum3Of4(Generic[_InnerC]):
        value: _InnerC

    @dataclass(frozen=True)
    class Sum4Of4(Generic[_InnerD]):
        value: _InnerD

        _C = TypeVar("_C")

    value: Sum1Of4[_A] | Sum2Of4[_B] | Sum3Of4[_C] | Sum4Of4[_D]

    _O = TypeVar("_O")

    @staticmethod
    def sum1of4(value: _A, /) -> Sum4[_A, _B, _C, _D]:
        return Sum4(Sum4.Sum1Of4(value))

    @staticmethod
    def sum2of4(value: _B, /) -> Sum4[_A, _B, _C, _D]:
        return Sum4(Sum4.Sum2Of4(value))

    @staticmethod
    def sum3of4(value: _C, /) -> Sum4[_A, _B, _C, _D]:
        return Sum4(Sum4.Sum3Of4(value))

    @staticmethod
    def sum4of4(value: _D, /) -> Sum4[_A, _B, _C, _D]:
        return Sum4(Sum4.Sum4Of4(value))

    def fold(
        self,
        on_1of4: Callable[[_A], _O],
        on_2of4: Callable[[_B], _O],
        on_3of4: Callable[[_C], _O],
        on_4of4: Callable[[_D], _O],
        /,
    ) -> _O:
        match self.value:
            case Sum4.Sum1Of4(value):
                return on_1of4(value)
            case Sum4.Sum2Of4(value):
                return on_2of4(value)
            case Sum4.Sum3Of4(value):
                return on_3of4(value)
            case Sum4.Sum4Of4(value):
                return on_4of4(value)
        assert_never(self.value)

    def map_1of4(self, on_1of4: Callable[[_A], _O], /) -> Sum4[_O, _B, _C, _D]:
        return self.fold(lambda value: Sum4.sum1of4(on_1of4(value)), Sum4.sum2of4, Sum4.sum3of4, Sum4.sum4of4)

    def map_2of4(self, on_2of4: Callable[[_B], _O], /) -> Sum4[_A, _O, _C, _D]:
        return self.fold(Sum4.sum1of4, lambda value: Sum4.sum2of4(on_2of4(value)), Sum4.sum3of4, Sum4.sum4of4)

    def map_3of4(self, on_3of4: Callable[[_C], _O], /) -> Sum4[_A, _B, _O, _D]:
        return self.fold(Sum4.sum1of4, Sum4.sum2of4, lambda value: Sum4.sum3of4(on_3of4(value)), Sum4.sum4of4)

    def map_4of4(self, on_4of4: Callable[[_D], _O], /) -> Sum4[_A, _B, _C, _O]:
        return self.fold(Sum4.sum1of4, Sum4.sum2of4, Sum4.sum3of4, lambda value: Sum4.sum4of4(on_4of4(value)))
