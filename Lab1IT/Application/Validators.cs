using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Lab1IT.Domain;

namespace Lab1IT.Application;

public static class Validators
{
    public static bool IsValidName(string? name)
    {
        return !string.IsNullOrWhiteSpace(name);
    }

    public static bool IsTableNameUnique(
        Database database,
        string tableName)
    {
        return !database.Tables.Any(table =>
            string.Equals(
                table.Name,
                tableName,
                StringComparison.OrdinalIgnoreCase));
    }

    public static bool IsColumnNameUnique(
        Table table,
        string columnName)
    {
        return !table.Columns.Any(column =>
            string.Equals(
                column.Name,
                columnName,
                StringComparison.OrdinalIgnoreCase));
    }

    public static bool IsValueValid(
        Column column,
        string? value)
    {
        return column.ColumnType.Validate(value);
    }
}