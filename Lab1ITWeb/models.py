from dataclasses import dataclass, field
from typing import Optional

from data_types import BaseType, create_type


@dataclass
class Column:
    name: str
    type_name: str = "string"
    lower_bound: Optional[str] = None
    upper_bound: Optional[str] = None

    @property
    def column_type(self) -> BaseType:
        return create_type(
            self.type_name,
            self.lower_bound,
            self.upper_bound
        )

    @property
    def display_type(self) -> str:
        if (
            self.lower_bound is not None
            and self.upper_bound is not None
        ):
            return (
                f"{self.type_name}"
                f"[{self.lower_bound}-{self.upper_bound}]"
            )

        return self.type_name

    def to_dict(self) -> dict:
        return {
            "Name": self.name,
            "TypeName": self.type_name,
            "LowerBound": self.lower_bound,
            "UpperBound": self.upper_bound
        }

    @classmethod
    def from_dict(cls, data: dict):
        return cls(
            name=data.get(
                "Name",
                data.get("name", "")
            ),
            type_name=data.get(
                "TypeName",
                data.get("type_name", "string")
            ),
            lower_bound=data.get(
                "LowerBound",
                data.get("lower_bound")
            ),
            upper_bound=data.get(
                "UpperBound",
                data.get("upper_bound")
            )
        )


@dataclass
class Row:
    values: list[str] = field(
        default_factory=list
    )

    def to_dict(self) -> dict:
        return {
            "Values": self.values
        }

    @classmethod
    def from_dict(cls, data: dict):
        values = data.get(
            "Values",
            data.get("values", [])
        )

        return cls(
            values=[
                str(value)
                for value in values
            ]
        )


@dataclass
class Table:
    name: str
    columns: list[Column] = field(
        default_factory=list
    )
    rows: list[Row] = field(
        default_factory=list
    )

    def get_column(
        self,
        name: str
    ) -> Optional[Column]:

        for column in self.columns:
            if (
                column.name.lower()
                == name.lower()
            ):
                return column

        return None

    def get_column_index(
        self,
        name: str
    ) -> int:

        for index, column in enumerate(
            self.columns
        ):
            if (
                column.name.lower()
                == name.lower()
            ):
                return index

        return -1

    def to_dict(self) -> dict:
        return {
            "Name": self.name,
            "Columns": [
                column.to_dict()
                for column in self.columns
            ],
            "Rows": [
                row.to_dict()
                for row in self.rows
            ]
        }

    @classmethod
    def from_dict(cls, data: dict):
        columns_data = data.get(
            "Columns",
            data.get("columns", [])
        )

        rows_data = data.get(
            "Rows",
            data.get("rows", [])
        )

        return cls(
            name=data.get(
                "Name",
                data.get("name", "")
            ),
            columns=[
                Column.from_dict(column)
                for column in columns_data
            ],
            rows=[
                Row.from_dict(row)
                for row in rows_data
            ]
        )


@dataclass
class Database:
    name: str
    tables: list[Table] = field(
        default_factory=list
    )

    def get_table(
        self,
        name: str
    ) -> Optional[Table]:

        for table in self.tables:
            if (
                table.name.lower()
                == name.lower()
            ):
                return table

        return None

    def to_dict(self) -> dict:
        return {
            "Name": self.name,
            "Tables": [
                table.to_dict()
                for table in self.tables
            ]
        }

    @classmethod
    def from_dict(cls, data: dict):
        name = data.get(
            "Name",
            data.get("name", "")
        )

        if not name or not str(name).strip():
            raise ValueError(
                "Database name is missing."
            )

        tables_data = data.get(
            "Tables",
            data.get("tables", [])
        )

        return cls(
            name=str(name).strip(),
            tables=[
                Table.from_dict(table)
                for table in tables_data
            ]
        )
