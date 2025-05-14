from __future__ import annotations

from collections.abc import Callable, Mapping, Sequence
from decimal import Decimal
from typing import TypeVar

Json = Mapping[str, "Json"] | Sequence["Json"] | str | int | Decimal | bool | None

_ToJsonType = TypeVar("_ToJsonType")
ToJson = Callable[[_ToJsonType], Json]

_FromJsonType = TypeVar("_FromJsonType")
FromJson = Callable[[Json], _FromJsonType]
