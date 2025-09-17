from __future__ import annotations

from collections.abc import Callable, Sequence
from dataclasses import dataclass
from functools import reduce
from typing import Generic, TypeVar

from typing_extensions import assert_never

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

    @staticmethod
    def sequence_all(
        merge: Callable[[_SumL, _SumL], _SumL],
    ) -> Callable[[Sequence[Sum[_SumL, _SumR]]], Sum[_SumL, Sequence[_SumR]]]:
        """
        Sequence `Sequence[Sum[L, R]]` into a single `Sum[L, Sequence[R]]`.

        Behavior:
        - If **all** inputs are `Right`, returns `Right` of a tuple containing the values
          in order: `Right((_r0, _r1, ..., _rn))`.
        - If **any** input is `Left`, returns a `Left` that merges **all** encountered
          `Left` values using the provided `merge(left1, left2) -> L` function.
            * Example: given inputs `[Left(e1), Right(a), Left(e2)]` the result is
              `Left(merge(e1, e2))`.

        This is useful when you want to:
        - Accumulate all successes (rights) while
        - Accumulating/combining all failures (lefts) with an associative merge (e.g.,
          concatenating error messages, unioning error sets, summing counts, etc.)

        Args:
            merge: Function to combine two `L` values into one. It will be used to
                   fold together multiple `Left` values encountered during sequencing.
                   This is the addition operation of a semigroup.

        Returns:
            A function that, given a `Sequence[Sum[_SumL, _SumR]]`, produces
            `Sum[_SumL, Sequence[_SumR]]` as described above.
        """

        def sequence(values: Sequence[Sum[_SumL, _SumR]]) -> Sum[_SumL, Sequence[_SumR]]:
            def step(acc: Sum[_SumL, Sequence[_SumR]], new_value: Sum[_SumL, _SumR]) -> Sum[_SumL, Sequence[_SumR]]:
                return acc.fold(
                    lambda acc_left: new_value.fold(
                        lambda new_left: Sum.left(merge(acc_left, new_left)), lambda _: Sum.left(acc_left)
                    ),
                    lambda collected: new_value.fold(Sum.left, lambda new_right: Sum.right((*collected, new_right))),
                )

            return reduce(step, values, Sum[_SumL, Sequence[_SumR]].right(()))

        return sequence
