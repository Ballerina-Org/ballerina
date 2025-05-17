from __future__ import annotations

from collections.abc import Callable
from dataclasses import dataclass
from typing import Generic, TypeVar, assert_never

_Option = TypeVar("_Option")
_Some = TypeVar("_Some")


@dataclass(frozen=True)
class Option(Generic[_Option]):
    @dataclass(frozen=True)
    class Some(Generic[_Some]):
        value: _Some

        _C = TypeVar("_C")

    @dataclass(frozen=True)
    class Nothing:
        pass

    value: Some[_Option] | Nothing

    _O = TypeVar("_O")

    @staticmethod
    def some(value: _Option, /) -> Option[_Option]:
        return Option(Option.Some(value))

    @staticmethod
    def none() -> Option[_Option]:
        return Option(Option.Nothing())

    def fold(self, on_some: Callable[[_Option], _O], on_nothing: Callable[[], _O], /) -> _O:
        match self.value:
            case Option.Some(value):
                return on_some(value)
            case Option.Nothing():
                return on_nothing()
        assert_never(self.value)

    def map(self, on_some: Callable[[_Option], _O], /) -> Option[_O]:
        return self.fold(lambda value: Option.some(on_some(value)), Option.none)

    def flat_map(self, on_some: Callable[[_Option], Option[_O]], /) -> Option[_O]:
        return self.fold(on_some, Option.none)
