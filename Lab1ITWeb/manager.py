from models import Database, Table, Column, Row
from data_types import create_type


class Manager:
    def __init__(self):
        self.current_database: Database | None = None

    def create_database(self, name: str) -> Database:
        name = self._validate_name(name, "Database")
        self.current_database = Database(name=name)
        return self.current_database

    def set_database(self, database: Database):
        if database is None:
            raise ValueError("Database cannot be null.")

        if not database.name.strip():
            raise ValueError("Database name cannot be empty.")

        self.current_database = database

    def create_table(self, name: str) -> Table:
        database = self._require_database()

        name = self._validate_name(name, "Table")

        if database.get_table(name) is not None:
            raise ValueError(
                f"Table '{name}' already exists."
            )

        table = Table(name=name)
        database.tables.append(table)

        return table

    def delete_table(self, name: str):
        database = self._require_database()
        table = self._require_table(name)

        database.tables.remove(table)

    def create_column(
        self,
        table_name: str,
        column_name: str,
        type_name: str,
        lower_bound: str | None = None,
        upper_bound: str | None = None
    ) -> Column:

        table = self._require_table(table_name)

        column_name = self._validate_name(
            column_name,
            "Column"
        )

        if table.get_column(column_name) is not None:
            raise ValueError(
                f"Column '{column_name}' already exists."
            )

        # Перевірка типу та bounds
        create_type(
            type_name,
            lower_bound,
            upper_bound
        )

        column = Column(
            name=column_name,
            type_name=type_name,
            lower_bound=lower_bound,
            upper_bound=upper_bound
        )

        table.columns.append(column)

        # Якщо рядки вже були,
        # додаємо нову порожню клітинку
        for row in table.rows:
            row.values.append("")

        return column

    def delete_column(
        self,
        table_name: str,
        column_name: str
    ):
        table = self._require_table(table_name)

        column_index = table.get_column_index(
            column_name
        )

        if column_index == -1:
            raise ValueError(
                f"Column '{column_name}' does not exist."
            )

        table.columns.pop(column_index)

        for row in table.rows:
            if column_index < len(row.values):
                row.values.pop(column_index)

    def add_row(self, table_name: str) -> Row:
        table = self._require_table(table_name)

        if not table.columns:
            raise ValueError(
                "Add at least one column before adding rows."
            )

        row = Row(
            values=[
                ""
                for _ in table.columns
            ]
        )

        table.rows.append(row)

        return row

    def delete_row(
        self,
        table_name: str,
        row_index: int
    ):
        table = self._require_table(table_name)

        if (
            row_index < 0
            or row_index >= len(table.rows)
        ):
            raise ValueError(
                "Invalid row index."
            )

        table.rows.pop(row_index)

    def edit_cell(
        self,
        table_name: str,
        row_index: int,
        column_name: str,
        value: str
    ):
        table = self._require_table(table_name)

        if (
            row_index < 0
            or row_index >= len(table.rows)
        ):
            raise ValueError(
                "Invalid row index."
            )

        column_index = table.get_column_index(
            column_name
        )

        if column_index == -1:
            raise ValueError(
                f"Column '{column_name}' does not exist."
            )

        column = table.columns[column_index]

        value = "" if value is None else str(value)

        # Порожню клітинку дозволяємо,
        # щоб рядок можна було заповнювати поступово
        if (
            value != ""
            and not column.column_type.validate(value)
        ):
            raise ValueError(
                f"Value '{value}' is invalid "
                f"for type {column.display_type}."
            )

        row = table.rows[row_index]

        while len(row.values) < len(table.columns):
            row.values.append("")

        row.values[column_index] = value

    def validate_join(
        self,
        first_table: Table,
        second_table: Table,
        common_field: str
    ) -> tuple[int, int]:

        if first_table is second_table:
            raise ValueError(
                "Choose two different tables."
            )

        first_index = first_table.get_column_index(
            common_field
        )

        second_index = second_table.get_column_index(
            common_field
        )

        if first_index == -1:
            raise ValueError(
                f"Column '{common_field}' does not exist "
                f"in '{first_table.name}'."
            )

        if second_index == -1:
            raise ValueError(
                f"Column '{common_field}' does not exist "
                f"in '{second_table.name}'."
            )

        first_column = first_table.columns[first_index]
        second_column = second_table.columns[second_index]

        if (
            first_column.type_name
            != second_column.type_name
        ):
            raise ValueError(
                "Common fields have incompatible types."
            )

        if (
            first_column.lower_bound
            != second_column.lower_bound
            or first_column.upper_bound
            != second_column.upper_bound
        ):
            raise ValueError(
                "Common fields have incompatible interval bounds."
            )

        return first_index, second_index

    def join_tables(
        self,
        first_table_name: str,
        second_table_name: str,
        common_field: str
    ) -> Table:

        first_table = self._require_table(
            first_table_name
        )

        second_table = self._require_table(
            second_table_name
        )

        first_index, second_index = (
            self.validate_join(
                first_table,
                second_table,
                common_field
            )
        )

        result = Table(
            name=(
                f"{first_table.name}_"
                f"{second_table.name}_Join"
            )
        )

        # Копіюємо всі колонки першої таблиці
        for column in first_table.columns:
            result.columns.append(
                self._clone_column(column)
            )

        existing_names = {
            column.name.lower()
            for column in result.columns
        }

        # Додаємо колонки другої таблиці,
        # крім common field
        for index, column in enumerate(
            second_table.columns
        ):
            if index == second_index:
                continue

            new_name = column.name

            if new_name.lower() in existing_names:
                new_name = (
                    f"{second_table.name}_{new_name}"
                )

            base_name = new_name
            suffix = 2

            while new_name.lower() in existing_names:
                new_name = (
                    f"{base_name}_{suffix}"
                )
                suffix += 1

            cloned = self._clone_column(
                column
            )

            cloned.name = new_name

            result.columns.append(cloned)

            existing_names.add(
                new_name.lower()
            )

        # INNER JOIN
        for first_row in first_table.rows:
            first_value = self._get_value(
                first_row,
                first_index
            )

            for second_row in second_table.rows:
                second_value = self._get_value(
                    second_row,
                    second_index
                )

                if first_value != second_value:
                    continue

                merged_values = list(
                    first_row.values
                )

                while (
                    len(merged_values)
                    < len(first_table.columns)
                ):
                    merged_values.append("")

                for index in range(
                    len(second_table.columns)
                ):
                    if index == second_index:
                        continue

                    merged_values.append(
                        self._get_value(
                            second_row,
                            index
                        )
                    )

                result.rows.append(
                    Row(values=merged_values)
                )

        return result

    def add_join_result(
        self,
        result: Table
    ) -> Table:

        database = self._require_database()

        base_name = result.name
        current_name = base_name
        suffix = 2

        while (
            database.get_table(current_name)
            is not None
        ):
            current_name = (
                f"{base_name}_{suffix}"
            )
            suffix += 1

        result.name = current_name
        database.tables.append(result)

        return result

    def _require_database(self) -> Database:
        if self.current_database is None:
            raise ValueError(
                "Create or open a database first."
            )

        return self.current_database

    def _require_table(
        self,
        name: str
    ) -> Table:

        database = self._require_database()

        table = database.get_table(name)

        if table is None:
            raise ValueError(
                f"Table '{name}' does not exist."
            )

        return table

    @staticmethod
    def _clone_column(
        column: Column
    ) -> Column:

        return Column(
            name=column.name,
            type_name=column.type_name,
            lower_bound=column.lower_bound,
            upper_bound=column.upper_bound
        )

    @staticmethod
    def _get_value(
        row: Row,
        index: int
    ) -> str:

        if index >= len(row.values):
            return ""

        return row.values[index]

    @staticmethod
    def _validate_name(
        name: str,
        entity_name: str
    ) -> str:

        if name is None:
            raise ValueError(
                f"{entity_name} name cannot be empty."
            )

        name = str(name).strip()

        if not name:
            raise ValueError(
                f"{entity_name} name cannot be empty."
            )

        return name
