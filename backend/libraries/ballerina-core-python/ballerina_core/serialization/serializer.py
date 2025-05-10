from __future__ import annotations

from collections.abc import Callable, Mapping, Sequence
from typing import TypeVar

Json = Mapping[str, "Json"] | Sequence["Json"] | str | int | float | bool | None

_SerializerType = TypeVar("_SerializerType")
Serializer = Callable[[_SerializerType], Json]

_DeserializerType = TypeVar("_DeserializerType")
Deserializer = Callable[[Json], _DeserializerType]
