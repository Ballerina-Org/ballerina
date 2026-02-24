from ballerina_core.parsing.keys import DISCRIMINATOR_KEY, VALUE_KEY
from ballerina_core.parsing.parsing_types import Json, ParsingError
from ballerina_core.sum import Sum


def discriminated_to_json(discriminator: str, value: Json, /) -> Json:
    return {DISCRIMINATOR_KEY: discriminator, VALUE_KEY: value}


def discriminated_value_from_json(
    value: Json,
    expected_discriminator: str,
    /,
    *,
    invalid_structure_prefix: str = "Invalid structure",
) -> Sum[ParsingError, Json]:
    match value:
        case {"discriminator": discriminator, "value": payload} if discriminator == expected_discriminator:
            return Sum.right(payload)
        case _:
            return Sum.left(ParsingError.single(f"{invalid_structure_prefix}: {value}"))
