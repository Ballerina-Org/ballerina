from typing import TypeVar

from ballerina_core.parsing.parsing_types import FromJson, Json, ParsingError, ToJson
from ballerina_core.sum import Sum

_A = TypeVar("_A")
_B = TypeVar("_B")
_C = TypeVar("_C")
_D = TypeVar("_D")
_E = TypeVar("_E")
_F = TypeVar("_F")
_G = TypeVar("_G")
_H = TypeVar("_H")
_I = TypeVar("_I")
_J = TypeVar("_J")


def tuple_2_from_json(a_parser: FromJson[_A], b_parser: FromJson[_B]) -> FromJson[tuple[_A, _B]]:
    def from_json(value: Json) -> Sum[ParsingError, tuple[_A, _B]]:
        length = 2
        match value:
            case {"kind": "tuple", "elements": elements}:
                match elements:
                    case list():
                        if len(elements) == length:
                            return (
                                a_parser(elements[0])
                                .map_left(ParsingError.with_context(f"parsing element 1 of {length}:"))
                                .flat_map(
                                    lambda a: b_parser(elements[1])
                                    .map_left(ParsingError.with_context(f"parsing element 2 of {length}:"))
                                    .map_right(lambda b: (a, b))
                                )
                            )
                        return Sum.left(
                            ParsingError.single(f"Expected {length} elements in tuple, got {len(elements)}")
                        )
                    case _:
                        return Sum.left(ParsingError.single(f"Not a tuple: {elements}"))
            case _:
                return Sum.left(ParsingError.single(f"Not a tuple: {value}"))

    return lambda value: from_json(value).map_left(ParsingError.with_context("Parsing tuple:"))


def tuple_2_to_json(a_to_json: ToJson[_A], b_to_json: ToJson[_B]) -> ToJson[tuple[_A, _B]]:
    def to_json(value: tuple[_A, _B]) -> Json:
        a, b = value
        return {"kind": "tuple", "elements": [a_to_json(a), b_to_json(b)]}

    return to_json


def tuple_3_from_json(
    a_parser: FromJson[_A], b_parser: FromJson[_B], c_parser: FromJson[_C]
) -> FromJson[tuple[_A, _B, _C]]:
    def from_json(value: Json) -> Sum[ParsingError, tuple[_A, _B, _C]]:
        length = 3
        match value:
            case {"kind": "tuple", "elements": elements}:
                match elements:
                    case list():
                        if len(elements) == length:
                            return (
                                a_parser(elements[0])
                                .map_left(ParsingError.with_context(f"parsing element 1 of {length}:"))
                                .flat_map(
                                    lambda a: b_parser(elements[1])
                                    .map_left(ParsingError.with_context(f"parsing element 2 of {length}:"))
                                    .flat_map(
                                        lambda b: c_parser(elements[2])
                                        .map_left(ParsingError.with_context(f"parsing element 3 of {length}:"))
                                        .map_right(lambda c: (a, b, c))
                                    )
                                )
                            )
                        return Sum.left(
                            ParsingError.single(f"Expected {length} elements in tuple, got {len(elements)}")
                        )
                    case _:
                        return Sum.left(ParsingError.single(f"Not a tuple: {elements}"))
            case _:
                return Sum.left(ParsingError.single(f"Not a tuple: {value}"))

    return lambda value: from_json(value).map_left(ParsingError.with_context("Parsing tuple:"))


def tuple_3_to_json(a_to_json: ToJson[_A], b_to_json: ToJson[_B], c_to_json: ToJson[_C]) -> ToJson[tuple[_A, _B, _C]]:
    def to_json(value: tuple[_A, _B, _C]) -> Json:
        a, b, c = value
        return {"kind": "tuple", "elements": [a_to_json(a), b_to_json(b), c_to_json(c)]}

    return to_json


def tuple_4_from_json(
    a_parser: FromJson[_A], b_parser: FromJson[_B], c_parser: FromJson[_C], d_parser: FromJson[_D]
) -> FromJson[tuple[_A, _B, _C, _D]]:
    def from_json(value: Json) -> Sum[ParsingError, tuple[_A, _B, _C, _D]]:
        length = 4
        match value:
            case {"kind": "tuple", "elements": elements}:
                match elements:
                    case list():
                        if len(elements) == length:
                            return (
                                a_parser(elements[0])
                                .map_left(ParsingError.with_context(f"parsing element 1 of {length}:"))
                                .flat_map(
                                    lambda a: b_parser(elements[1])
                                    .map_left(ParsingError.with_context(f"parsing element 2 of {length}:"))
                                    .flat_map(
                                        lambda b: c_parser(elements[2])
                                        .map_left(ParsingError.with_context(f"parsing element 3 of {length}:"))
                                        .flat_map(
                                            lambda c: d_parser(elements[3])
                                            .map_left(ParsingError.with_context(f"parsing element 4 of {length}:"))
                                            .map_right(lambda d: (a, b, c, d))
                                        )
                                    )
                                )
                            )
                        return Sum.left(
                            ParsingError.single(f"Expected {length} elements in tuple, got {len(elements)}")
                        )
                    case _:
                        return Sum.left(ParsingError.single(f"Not a tuple: {elements}"))
            case _:
                return Sum.left(ParsingError.single(f"Not a tuple: {value}"))

    return lambda value: from_json(value).map_left(ParsingError.with_context("Parsing tuple:"))


def tuple_4_to_json(
    a_to_json: ToJson[_A], b_to_json: ToJson[_B], c_to_json: ToJson[_C], d_to_json: ToJson[_D]
) -> ToJson[tuple[_A, _B, _C, _D]]:
    def to_json(value: tuple[_A, _B, _C, _D]) -> Json:
        a, b, c, d = value
        return {"kind": "tuple", "elements": [a_to_json(a), b_to_json(b), c_to_json(c), d_to_json(d)]}

    return to_json


def tuple_5_from_json(
    a_parser: FromJson[_A],
    b_parser: FromJson[_B],
    c_parser: FromJson[_C],
    d_parser: FromJson[_D],
    e_parser: FromJson[_E],
) -> FromJson[tuple[_A, _B, _C, _D, _E]]:
    def from_json(value: Json) -> Sum[ParsingError, tuple[_A, _B, _C, _D, _E]]:
        length = 5
        match value:
            case {"kind": "tuple", "elements": elements}:
                match elements:
                    case list():
                        if len(elements) == length:
                            return (
                                a_parser(elements[0])
                                .map_left(ParsingError.with_context(f"parsing element 1 of {length}:"))
                                .flat_map(
                                    lambda a: b_parser(elements[1])
                                    .map_left(ParsingError.with_context(f"parsing element 2 of {length}:"))
                                    .flat_map(
                                        lambda b: c_parser(elements[2])
                                        .map_left(ParsingError.with_context(f"parsing element 3 of {length}:"))
                                        .flat_map(
                                            lambda c: d_parser(elements[3])
                                            .map_left(ParsingError.with_context(f"parsing element 4 of {length}:"))
                                            .flat_map(
                                                lambda d: e_parser(elements[4])
                                                .map_left(ParsingError.with_context(f"parsing element 5 of {length}:"))
                                                .map_right(lambda e: (a, b, c, d, e))
                                            )
                                        )
                                    )
                                )
                            )
                        return Sum.left(
                            ParsingError.single(f"Expected {length} elements in tuple, got {len(elements)}")
                        )
                    case _:
                        return Sum.left(ParsingError.single(f"Not a tuple: {elements}"))
            case _:
                return Sum.left(ParsingError.single(f"Not a tuple: {value}"))

    return lambda value: from_json(value).map_left(ParsingError.with_context("Parsing tuple:"))


def tuple_5_to_json(
    a_to_json: ToJson[_A], b_to_json: ToJson[_B], c_to_json: ToJson[_C], d_to_json: ToJson[_D], e_to_json: ToJson[_E]
) -> ToJson[tuple[_A, _B, _C, _D, _E]]:
    def to_json(value: tuple[_A, _B, _C, _D, _E]) -> Json:
        a, b, c, d, e = value
        return {"kind": "tuple", "elements": [a_to_json(a), b_to_json(b), c_to_json(c), d_to_json(d), e_to_json(e)]}

    return to_json


def tuple_6_from_json(  # noqa: PLR0917,PLR0913
    a_parser: FromJson[_A],
    b_parser: FromJson[_B],
    c_parser: FromJson[_C],
    d_parser: FromJson[_D],
    e_parser: FromJson[_E],
    f_parser: FromJson[_F],
) -> FromJson[tuple[_A, _B, _C, _D, _E, _F]]:
    def from_json(value: Json) -> Sum[ParsingError, tuple[_A, _B, _C, _D, _E, _F]]:
        length = 6
        match value:
            case {"kind": "tuple", "elements": elements}:
                match elements:
                    case list():
                        if len(elements) == length:
                            return (
                                a_parser(elements[0])
                                .map_left(ParsingError.with_context(f"parsing element 1 of {length}:"))
                                .flat_map(
                                    lambda a: b_parser(elements[1])
                                    .map_left(ParsingError.with_context(f"parsing element 2 of {length}:"))
                                    .flat_map(
                                        lambda b: c_parser(elements[2])
                                        .map_left(ParsingError.with_context(f"parsing element 3 of {length}:"))
                                        .flat_map(
                                            lambda c: d_parser(elements[3])
                                            .map_left(ParsingError.with_context(f"parsing element 4 of {length}:"))
                                            .flat_map(
                                                lambda d: e_parser(elements[4])
                                                .map_left(ParsingError.with_context(f"parsing element 5 of {length}:"))
                                                .flat_map(
                                                    lambda e: f_parser(elements[5])
                                                    .map_left(
                                                        ParsingError.with_context(f"parsing element 6 of {length}:")
                                                    )
                                                    .map_right(lambda f: (a, b, c, d, e, f))
                                                )
                                            )
                                        )
                                    )
                                )
                            )
                        return Sum.left(
                            ParsingError.single(f"Expected {length} elements in tuple, got {len(elements)}")
                        )
                    case _:
                        return Sum.left(ParsingError.single(f"Not a tuple: {elements}"))
            case _:
                return Sum.left(ParsingError.single(f"Not a tuple: {value}"))

    return lambda value: from_json(value).map_left(ParsingError.with_context("Parsing tuple:"))


def tuple_6_to_json(  # noqa: PLR0917,PLR0913
    a_to_json: ToJson[_A],
    b_to_json: ToJson[_B],
    c_to_json: ToJson[_C],
    d_to_json: ToJson[_D],
    e_to_json: ToJson[_E],
    f_to_json: ToJson[_F],
) -> ToJson[tuple[_A, _B, _C, _D, _E, _F]]:
    def to_json(value: tuple[_A, _B, _C, _D, _E, _F]) -> Json:
        a, b, c, d, e, f = value
        return {
            "kind": "tuple",
            "elements": [a_to_json(a), b_to_json(b), c_to_json(c), d_to_json(d), e_to_json(e), f_to_json(f)],
        }

    return to_json


def tuple_7_from_json(  # noqa: PLR0917,PLR0913
    a_parser: FromJson[_A],
    b_parser: FromJson[_B],
    c_parser: FromJson[_C],
    d_parser: FromJson[_D],
    e_parser: FromJson[_E],
    f_parser: FromJson[_F],
    g_parser: FromJson[_G],
) -> FromJson[tuple[_A, _B, _C, _D, _E, _F, _G]]:
    def from_json(value: Json) -> Sum[ParsingError, tuple[_A, _B, _C, _D, _E, _F, _G]]:
        length = 7
        match value:
            case {"kind": "tuple", "elements": elements}:
                match elements:
                    case list():
                        if len(elements) == length:
                            return (
                                a_parser(elements[0])
                                .map_left(ParsingError.with_context(f"parsing element 1 of {length}:"))
                                .flat_map(
                                    lambda a: b_parser(elements[1])
                                    .map_left(ParsingError.with_context(f"parsing element 2 of {length}:"))
                                    .flat_map(
                                        lambda b: c_parser(elements[2])
                                        .map_left(ParsingError.with_context(f"parsing element 3 of {length}:"))
                                        .flat_map(
                                            lambda c: d_parser(elements[3])
                                            .map_left(ParsingError.with_context(f"parsing element 4 of {length}:"))
                                            .flat_map(
                                                lambda d: e_parser(elements[4])
                                                .map_left(ParsingError.with_context(f"parsing element 5 of {length}:"))
                                                .flat_map(
                                                    lambda e: f_parser(elements[5])
                                                    .map_left(
                                                        ParsingError.with_context(f"parsing element 6 of {length}:")
                                                    )
                                                    .flat_map(
                                                        lambda f: g_parser(elements[6])
                                                        .map_left(
                                                            ParsingError.with_context(f"parsing element 7 of {length}:")
                                                        )
                                                        .map_right(lambda g: (a, b, c, d, e, f, g))
                                                    )
                                                )
                                            )
                                        )
                                    )
                                )
                            )
                        return Sum.left(
                            ParsingError.single(f"Expected {length} elements in tuple, got {len(elements)}")
                        )
                    case _:
                        return Sum.left(ParsingError.single(f"Not a tuple: {elements}"))
            case _:
                return Sum.left(ParsingError.single(f"Not a tuple: {value}"))

    return lambda value: from_json(value).map_left(ParsingError.with_context("Parsing tuple:"))


def tuple_7_to_json(  # noqa: PLR0917,PLR0913
    a_to_json: ToJson[_A],
    b_to_json: ToJson[_B],
    c_to_json: ToJson[_C],
    d_to_json: ToJson[_D],
    e_to_json: ToJson[_E],
    f_to_json: ToJson[_F],
    g_to_json: ToJson[_G],
) -> ToJson[tuple[_A, _B, _C, _D, _E, _F, _G]]:
    def to_json(value: tuple[_A, _B, _C, _D, _E, _F, _G]) -> Json:
        a, b, c, d, e, f, g = value
        return {
            "kind": "tuple",
            "elements": [
                a_to_json(a),
                b_to_json(b),
                c_to_json(c),
                d_to_json(d),
                e_to_json(e),
                f_to_json(f),
                g_to_json(g),
            ],
        }

    return to_json


def tuple_8_from_json(  # noqa: PLR0917,PLR0913
    a_parser: FromJson[_A],
    b_parser: FromJson[_B],
    c_parser: FromJson[_C],
    d_parser: FromJson[_D],
    e_parser: FromJson[_E],
    f_parser: FromJson[_F],
    g_parser: FromJson[_G],
    h_parser: FromJson[_H],
) -> FromJson[tuple[_A, _B, _C, _D, _E, _F, _G, _H]]:
    def from_json(value: Json) -> Sum[ParsingError, tuple[_A, _B, _C, _D, _E, _F, _G, _H]]:
        length = 8
        match value:
            case {"kind": "tuple", "elements": elements}:
                match elements:
                    case list():
                        if len(elements) == length:
                            return (
                                a_parser(elements[0])
                                .map_left(ParsingError.with_context(f"parsing element 1 of {length}:"))
                                .flat_map(
                                    lambda a: b_parser(elements[1])
                                    .map_left(ParsingError.with_context(f"parsing element 2 of {length}:"))
                                    .flat_map(
                                        lambda b: c_parser(elements[2])
                                        .map_left(ParsingError.with_context(f"parsing element 3 of {length}:"))
                                        .flat_map(
                                            lambda c: d_parser(elements[3])
                                            .map_left(ParsingError.with_context(f"parsing element 4 of {length}:"))
                                            .flat_map(
                                                lambda d: e_parser(elements[4])
                                                .map_left(ParsingError.with_context(f"parsing element 5 of {length}:"))
                                                .flat_map(
                                                    lambda e: f_parser(elements[5])
                                                    .map_left(
                                                        ParsingError.with_context(f"parsing element 6 of {length}:")
                                                    )
                                                    .flat_map(
                                                        lambda f: g_parser(elements[6])
                                                        .map_left(
                                                            ParsingError.with_context(f"parsing element 7 of {length}:")
                                                        )
                                                        .flat_map(
                                                            lambda g: h_parser(elements[7])
                                                            .map_left(
                                                                ParsingError.with_context(
                                                                    f"parsing element 8 of {length}:"
                                                                )
                                                            )
                                                            .map_right(lambda h: (a, b, c, d, e, f, g, h))
                                                        )
                                                    )
                                                )
                                            )
                                        )
                                    )
                                )
                            )
                        return Sum.left(
                            ParsingError.single(f"Expected {length} elements in tuple, got {len(elements)}")
                        )
                    case _:
                        return Sum.left(ParsingError.single(f"Not a tuple: {elements}"))
            case _:
                return Sum.left(ParsingError.single(f"Not a tuple: {value}"))

    return lambda value: from_json(value).map_left(ParsingError.with_context("Parsing tuple:"))


def tuple_8_to_json(  # noqa: PLR0917,PLR0913
    a_to_json: ToJson[_A],
    b_to_json: ToJson[_B],
    c_to_json: ToJson[_C],
    d_to_json: ToJson[_D],
    e_to_json: ToJson[_E],
    f_to_json: ToJson[_F],
    g_to_json: ToJson[_G],
    h_to_json: ToJson[_H],
) -> ToJson[tuple[_A, _B, _C, _D, _E, _F, _G, _H]]:
    def to_json(value: tuple[_A, _B, _C, _D, _E, _F, _G, _H]) -> Json:
        a, b, c, d, e, f, g, h = value
        return {
            "kind": "tuple",
            "elements": [
                a_to_json(a),
                b_to_json(b),
                c_to_json(c),
                d_to_json(d),
                e_to_json(e),
                f_to_json(f),
                g_to_json(g),
                h_to_json(h),
            ],
        }

    return to_json


def tuple_9_from_json(  # noqa: PLR0917,PLR0913
    a_parser: FromJson[_A],
    b_parser: FromJson[_B],
    c_parser: FromJson[_C],
    d_parser: FromJson[_D],
    e_parser: FromJson[_E],
    f_parser: FromJson[_F],
    g_parser: FromJson[_G],
    h_parser: FromJson[_H],
    i_parser: FromJson[_I],
) -> FromJson[tuple[_A, _B, _C, _D, _E, _F, _G, _H, _I]]:
    def from_json(value: Json) -> Sum[ParsingError, tuple[_A, _B, _C, _D, _E, _F, _G, _H, _I]]:
        length = 9
        match value:
            case {"kind": "tuple", "elements": elements}:
                match elements:
                    case list():
                        if len(elements) == length:
                            return (
                                a_parser(elements[0])
                                .map_left(ParsingError.with_context(f"parsing element 1 of {length}:"))
                                .flat_map(
                                    lambda a: b_parser(elements[1])
                                    .map_left(ParsingError.with_context(f"parsing element 2 of {length}:"))
                                    .flat_map(
                                        lambda b: c_parser(elements[2])
                                        .map_left(ParsingError.with_context(f"parsing element 3 of {length}:"))
                                        .flat_map(
                                            lambda c: d_parser(elements[3])
                                            .map_left(ParsingError.with_context(f"parsing element 4 of {length}:"))
                                            .flat_map(
                                                lambda d: e_parser(elements[4])
                                                .map_left(ParsingError.with_context(f"parsing element 5 of {length}:"))
                                                .flat_map(
                                                    lambda e: f_parser(elements[5])
                                                    .map_left(
                                                        ParsingError.with_context(f"parsing element 6 of {length}:")
                                                    )
                                                    .flat_map(
                                                        lambda f: g_parser(elements[6])
                                                        .map_left(
                                                            ParsingError.with_context(f"parsing element 7 of {length}:")
                                                        )
                                                        .flat_map(
                                                            lambda g: h_parser(elements[7])
                                                            .map_left(
                                                                ParsingError.with_context(
                                                                    f"parsing element 8 of {length}:"
                                                                )
                                                            )
                                                            .flat_map(
                                                                lambda h: i_parser(elements[8])
                                                                .map_left(
                                                                    ParsingError.with_context(
                                                                        f"parsing element 9 of {length}:"
                                                                    )
                                                                )
                                                                .map_right(lambda i: (a, b, c, d, e, f, g, h, i))
                                                            )
                                                        )
                                                    )
                                                )
                                            )
                                        )
                                    )
                                )
                            )
                        return Sum.left(
                            ParsingError.single(f"Expected {length} elements in tuple, got {len(elements)}")
                        )
                    case _:
                        return Sum.left(ParsingError.single(f"Not a tuple: {elements}"))
            case _:
                return Sum.left(ParsingError.single(f"Not a tuple: {value}"))

    return lambda value: from_json(value).map_left(ParsingError.with_context("Parsing tuple:"))


def tuple_9_to_json(  # noqa: PLR0917,PLR0913
    a_to_json: ToJson[_A],
    b_to_json: ToJson[_B],
    c_to_json: ToJson[_C],
    d_to_json: ToJson[_D],
    e_to_json: ToJson[_E],
    f_to_json: ToJson[_F],
    g_to_json: ToJson[_G],
    h_to_json: ToJson[_H],
    i_to_json: ToJson[_I],
) -> ToJson[tuple[_A, _B, _C, _D, _E, _F, _G, _H, _I]]:
    def to_json(value: tuple[_A, _B, _C, _D, _E, _F, _G, _H, _I]) -> Json:
        a, b, c, d, e, f, g, h, i = value
        return {
            "kind": "tuple",
            "elements": [
                a_to_json(a),
                b_to_json(b),
                c_to_json(c),
                d_to_json(d),
                e_to_json(e),
                f_to_json(f),
                g_to_json(g),
                h_to_json(h),
                i_to_json(i),
            ],
        }

    return to_json


def tuple_10_from_json(  # noqa: PLR0917,PLR0913
    a_parser: FromJson[_A],
    b_parser: FromJson[_B],
    c_parser: FromJson[_C],
    d_parser: FromJson[_D],
    e_parser: FromJson[_E],
    f_parser: FromJson[_F],
    g_parser: FromJson[_G],
    h_parser: FromJson[_H],
    i_parser: FromJson[_I],
    j_parser: FromJson[_J],
) -> FromJson[tuple[_A, _B, _C, _D, _E, _F, _G, _H, _I, _J]]:
    def from_json(value: Json) -> Sum[ParsingError, tuple[_A, _B, _C, _D, _E, _F, _G, _H, _I, _J]]:
        length = 10
        match value:
            case {"kind": "tuple", "elements": elements}:
                match elements:
                    case list():
                        if len(elements) == length:
                            return (
                                a_parser(elements[0])
                                .map_left(ParsingError.with_context(f"parsing element 1 of {length}:"))
                                .flat_map(
                                    lambda a: b_parser(elements[1])
                                    .map_left(ParsingError.with_context(f"parsing element 2 of {length}:"))
                                    .flat_map(
                                        lambda b: c_parser(elements[2])
                                        .map_left(ParsingError.with_context(f"parsing element 3 of {length}:"))
                                        .flat_map(
                                            lambda c: d_parser(elements[3])
                                            .map_left(ParsingError.with_context(f"parsing element 4 of {length}:"))
                                            .flat_map(
                                                lambda d: e_parser(elements[4])
                                                .map_left(ParsingError.with_context(f"parsing element 5 of {length}:"))
                                                .flat_map(
                                                    lambda e: f_parser(elements[5])
                                                    .map_left(
                                                        ParsingError.with_context(f"parsing element 6 of {length}:")
                                                    )
                                                    .flat_map(
                                                        lambda f: g_parser(elements[6])
                                                        .map_left(
                                                            ParsingError.with_context(f"parsing element 7 of {length}:")
                                                        )
                                                        .flat_map(
                                                            lambda g: h_parser(elements[7])
                                                            .map_left(
                                                                ParsingError.with_context(
                                                                    f"parsing element 8 of {length}:"
                                                                )
                                                            )
                                                            .flat_map(
                                                                lambda h: i_parser(elements[8])
                                                                .map_left(
                                                                    ParsingError.with_context(
                                                                        f"parsing element 9 of {length}:"
                                                                    )
                                                                )
                                                                .flat_map(
                                                                    lambda i: j_parser(elements[9])
                                                                    .map_left(
                                                                        ParsingError.with_context(
                                                                            f"parsing element 10 of {length}:"
                                                                        )
                                                                    )
                                                                    .map_right(lambda j: (a, b, c, d, e, f, g, h, i, j))
                                                                )
                                                            )
                                                        )
                                                    )
                                                )
                                            )
                                        )
                                    )
                                )
                            )
                        return Sum.left(
                            ParsingError.single(f"Expected {length} elements in tuple, got {len(elements)}")
                        )
                    case _:
                        return Sum.left(ParsingError.single(f"Not a tuple: {elements}"))
            case _:
                return Sum.left(ParsingError.single(f"Not a tuple: {value}"))

    return lambda value: from_json(value).map_left(ParsingError.with_context("Parsing tuple:"))


def tuple_10_to_json(  # noqa: PLR0917,PLR0913
    a_to_json: ToJson[_A],
    b_to_json: ToJson[_B],
    c_to_json: ToJson[_C],
    d_to_json: ToJson[_D],
    e_to_json: ToJson[_E],
    f_to_json: ToJson[_F],
    g_to_json: ToJson[_G],
    h_to_json: ToJson[_H],
    i_to_json: ToJson[_I],
    j_to_json: ToJson[_J],
) -> ToJson[tuple[_A, _B, _C, _D, _E, _F, _G, _H, _I, _J]]:
    def to_json(value: tuple[_A, _B, _C, _D, _E, _F, _G, _H, _I, _J]) -> Json:
        a, b, c, d, e, f, g, h, i, j = value
        return {
            "kind": "tuple",
            "elements": [
                a_to_json(a),
                b_to_json(b),
                c_to_json(c),
                d_to_json(d),
                e_to_json(e),
                f_to_json(f),
                g_to_json(g),
                h_to_json(h),
                i_to_json(i),
                j_to_json(j),
            ],
        }

    return to_json
