from abc import ABC, abstractmethod


class BaseType(ABC):
    name = "base"

    @abstractmethod
    def validate(self, value: str) -> bool:
        pass


class IntegerType(BaseType):
    name = "integer"

    def validate(self, value: str) -> bool:
        if value is None or value == "":
            return False

        try:
            int(value)
            return True
        except ValueError:
            return False


class RealType(BaseType):
    name = "real"

    def validate(self, value: str) -> bool:
        if value is None or value == "":
            return False

        try:
            float(value.replace(",", "."))
            return True
        except ValueError:
            return False


class CharType(BaseType):
    name = "char"

    def validate(self, value: str) -> bool:
        return value is not None and len(value) == 1


class StringType(BaseType):
    name = "string"

    def validate(self, value: str) -> bool:
        return value is not None


class CharInvlType(BaseType):
    name = "charInvl"

    def __init__(self, lower_bound: str, upper_bound: str):
        if (
            lower_bound is None
            or upper_bound is None
            or len(lower_bound) != 1
            or len(upper_bound) != 1
        ):
            raise ValueError(
                "Bounds for charInvl must contain exactly one character."
            )

        if lower_bound > upper_bound:
            raise ValueError(
                "Lower bound cannot be greater than upper bound."
            )

        self.lower_bound = lower_bound
        self.upper_bound = upper_bound

    def validate(self, value: str) -> bool:
        if value is None or len(value) != 1:
            return False

        return self.lower_bound <= value <= self.upper_bound


class StringCharInvlType(BaseType):
    name = "string(charInvl)"

    def __init__(self, lower_bound: str, upper_bound: str):
        if (
            lower_bound is None
            or upper_bound is None
            or len(lower_bound) != 1
            or len(upper_bound) != 1
        ):
            raise ValueError(
                "Bounds for string(charInvl) must contain exactly one character."
            )

        if lower_bound > upper_bound:
            raise ValueError(
                "Lower bound cannot be greater than upper bound."
            )

        self.lower_bound = lower_bound
        self.upper_bound = upper_bound

    def validate(self, value: str) -> bool:
        if value is None or value == "":
            return False

        return all(
            self.lower_bound <= symbol <= self.upper_bound
            for symbol in value
        )


def create_type(
    type_name: str,
    lower_bound: str | None = None,
    upper_bound: str | None = None
) -> BaseType:

    if type_name == "integer":
        return IntegerType()

    if type_name == "real":
        return RealType()

    if type_name == "char":
        return CharType()

    if type_name == "string":
        return StringType()

    if type_name == "charInvl":
        return CharInvlType(
            lower_bound,
            upper_bound
        )

    if type_name == "string(charInvl)":
        return StringCharInvlType(
            lower_bound,
            upper_bound
        )

    raise ValueError(f"Unknown data type: {type_name}")
