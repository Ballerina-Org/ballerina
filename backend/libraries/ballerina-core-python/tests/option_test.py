from ballerina_core.option import Option


class TestOption:
    @staticmethod
    def test_some() -> None:
        value: int = 42
        option: Option[int] = Option.some(value)
        result = option.fold(lambda x: x * 2, lambda: 0)
        assert result == value * 2

    @staticmethod
    def test_nothing() -> None:
        option: Option[int] = Option.nothing()
        result = option.fold(lambda x: x * 2, lambda: 0)
        assert result == 0

    @staticmethod
    def test_fold_with_none() -> None:
        option: Option[int] = Option.nothing()
        result = option.fold(lambda x: x * 2, lambda: None)
        assert result is None

    @staticmethod
    def test_fold_with_some() -> None:
        value: int = 42
        option: Option[int] = Option.some(value)
        result = option.fold(lambda x: x * 2, lambda: None)
        assert result == value * 2

    @staticmethod
    def test_map_with_some() -> None:
        value: int = 42
        option: Option[int] = Option.some(value)
        result = option.map(lambda x: x * 2)
        assert result == Option.some(value * 2)

    @staticmethod
    def test_map_with_none() -> None:
        option: Option[int] = Option.nothing()
        result = option.map(lambda x: x * 2)
        assert result == Option.nothing()

    @staticmethod
    def test_flat_map_with_some() -> None:
        value: int = 42
        option: Option[int] = Option.some(value)
        result = option.flat_map(lambda x: Option.some(x * 2))
        assert result == Option.some(value * 2)

    @staticmethod
    def test_flat_map_with_none() -> None:
        option: Option[int] = Option.nothing()
        result = option.flat_map(lambda x: Option.some(x * 2))
        assert result == Option.nothing()
