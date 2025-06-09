from typing import TypeVar

_T = TypeVar("_T")


def identity(value: _T, /) -> _T:
    return value
