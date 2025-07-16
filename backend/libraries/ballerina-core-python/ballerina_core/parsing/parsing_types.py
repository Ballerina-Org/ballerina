from __future__ import annotations

from collections.abc import Callable, Mapping, Sequence
from dataclasses import dataclass
from decimal import Decimal
from typing import TypeVar

from ballerina_core.sum import Sum

Json = Mapping[str, "Json"] | Sequence["Json"] | str | int | Decimal | bool | None


@dataclass(frozen=True)
class ParsingError:
    _context: Sequence[str]

    def message(self) -> str:
        message = "\n\t".join(self._context)
        return f"Parsing error at {message}"

    @staticmethod
    def single(context: str) -> ParsingError:
        return ParsingError([context])

    @staticmethod
    def with_context(context: str) -> Callable[[ParsingError], ParsingError]:
        def add_context(error: ParsingError, /) -> ParsingError:
            return ParsingError([context, *error._context])

        return add_context


_ToJsonType = TypeVar("_ToJsonType")
ToJson = Callable[[_ToJsonType], Json]

_FromJsonType = TypeVar("_FromJsonType")
FromJson = Callable[[Json], Sum[ParsingError, _FromJsonType]]
