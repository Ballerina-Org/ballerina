from ballerina_core.option import Option


def test_some() -> None:
    value: int = 42
    option: Option[int] = Option.some(value)
    result = option.fold(lambda: 0, lambda x: x * 2)
    assert result == value * 2


def test_nothing() -> None:
    option: Option[int] = Option.none()
    result = option.fold(lambda: 0, lambda x: x * 2)
    assert result == 0


def test_fold_with_none() -> None:
    option: Option[int] = Option.none()
    result = option.fold(lambda: None, lambda x: x * 2)
    assert result is None


def test_fold_with_some() -> None:
    value: int = 42
    option: Option[int] = Option.some(value)
    result = option.fold(lambda: None, lambda x: x * 2)
    assert result == value * 2


def test_map_with_some() -> None:
    value: int = 42
    option: Option[int] = Option.some(value)
    result = option.map(lambda x: x * 2)
    assert result == Option.some(value * 2)


def test_map_with_none() -> None:
    option: Option[int] = Option.none()
    result = option.map(lambda x: x * 2)
    assert result == Option.none()


def test_flat_map_with_some() -> None:
    value: int = 42
    option: Option[int] = Option.some(value)
    result = option.flat_map(lambda x: Option.some(x * 2))
    assert result == Option.some(value * 2)


def test_flat_map_with_none() -> None:
    option: Option[int] = Option.none()
    result = option.flat_map(lambda x: Option.some(x * 2))
    assert result == Option.none()


def test_flatten_with_some() -> None:
    option: Option[Option[int]] = Option.some(Option.some(42))
    result = Option.flatten(option)
    assert result == Option.some(42)


def test_flatten_with_none() -> None:
    option: Option[Option[int]] = Option.none()
    result = Option.flatten(option)
    assert result == Option.none()
