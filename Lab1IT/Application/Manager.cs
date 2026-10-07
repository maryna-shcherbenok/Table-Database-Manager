using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Lab1IT.Domain;
using Lab1IT.Domain.Types;

namespace Lab1IT.Application;

public class Manager
{
    public Database? CurrentDatabase { get; private set; }

    // =========================
    // DATABASE
    // =========================

    public Database CreateDatabase(string name)
    {
        if (!Validators.IsValidName(name))
        {
            throw new ArgumentException(
                "Database name cannot be empty.");
        }

        CurrentDatabase = new Database(name);

        return CurrentDatabase;
    }

    public void SetDatabase(Database database)
    {
        CurrentDatabase = database;
    }

    // =========================
    // TABLE
    // =========================

    public Table CreateTable(string name)
    {
        Database database = RequireDatabase();

        if (!Validators.IsValidName(name))
        {
            throw new ArgumentException(
                "Table name cannot be empty.");
        }

        if (!Validators.IsTableNameUnique(
                database,
                name))
        {
            throw new InvalidOperationException(
                $"Table '{name}' already exists.");
        }

        Table table = new(name);

        database.Tables.Add(table);

        return table;
    }

    public void DeleteTable(string tableName)
    {
        Database database = RequireDatabase();

        Table table = GetTable(tableName);

        database.Tables.Remove(table);
    }

    // =========================
    // COLUMN
    // =========================

    public Column CreateColumn(
        string tableName,
        string columnName,
        string typeName,
        char? lowerBound = null,
        char? upperBound = null)
    {
        Table table = GetTable(tableName);

        if (!Validators.IsValidName(columnName))
        {
            throw new ArgumentException(
                "Column name cannot be empty.");
        }

        if (!Validators.IsColumnNameUnique(
                table,
                columnName))
        {
            throw new InvalidOperationException(
                $"Column '{columnName}' already exists.");
        }

        BaseType type = TypeFactory.Create(
            typeName,
            lowerBound,
            upperBound);

        Column column = new(
            columnName,
            type);

        table.Columns.Add(column);

        // Existing rows need a new empty cell.
        foreach (Row row in table.Rows)
        {
            row.Values.Add(string.Empty);
        }

        return column;
    }

    public void DeleteColumn(
        string tableName,
        string columnName)
    {
        Table table = GetTable(tableName);

        int columnIndex =
            table.GetColumnIndex(columnName);

        if (columnIndex < 0)
        {
            throw new InvalidOperationException(
                $"Column '{columnName}' does not exist.");
        }

        table.Columns.RemoveAt(columnIndex);

        foreach (Row row in table.Rows)
        {
            if (columnIndex < row.Values.Count)
            {
                row.Values.RemoveAt(columnIndex);
            }
        }
    }

    // =========================
    // ROW
    // =========================

    public Row AddRow(
        string tableName,
        IEnumerable<string> values)
    {
        Table table = GetTable(tableName);

        List<string> valueList =
            values.ToList();

        if (valueList.Count != table.Columns.Count)
        {
            throw new ArgumentException(
                "Number of values must match " +
                "the number of columns.");
        }

        for (int i = 0;
             i < table.Columns.Count;
             i++)
        {
            Column column = table.Columns[i];

            if (!Validators.IsValueValid(
                    column,
                    valueList[i]))
            {
                throw new ArgumentException(
                    $"Value '{valueList[i]}' is invalid " +
                    $"for column '{column.Name}' " +
                    $"of type '{column.DisplayType}'.");
            }
        }

        Row row = new(valueList);

        table.Rows.Add(row);

        return row;
    }

    public void DeleteRow(
        string tableName,
        int rowIndex)
    {
        Table table = GetTable(tableName);

        if (rowIndex < 0 ||
            rowIndex >= table.Rows.Count)
        {
            throw new ArgumentOutOfRangeException(
                nameof(rowIndex),
                "Invalid row index.");
        }

        table.Rows.RemoveAt(rowIndex);
    }

    // =========================
    // CELL
    // =========================

    public void EditCell(
        string tableName,
        int rowIndex,
        string columnName,
        string newValue)
    {
        Table table = GetTable(tableName);

        if (rowIndex < 0 ||
            rowIndex >= table.Rows.Count)
        {
            throw new ArgumentOutOfRangeException(
                nameof(rowIndex),
                "Invalid row index.");
        }

        int columnIndex =
            table.GetColumnIndex(columnName);

        if (columnIndex < 0)
        {
            throw new InvalidOperationException(
                $"Column '{columnName}' does not exist.");
        }

        Column column =
            table.Columns[columnIndex];

        if (!Validators.IsValueValid(
                column,
                newValue))
        {
            throw new ArgumentException(
                $"Value '{newValue}' is invalid " +
                $"for type '{column.DisplayType}'.");
        }

        table.Rows[rowIndex]
            .Values[columnIndex] = newValue;
    }

    // =========================
    // JOIN - VARIANT 8
    // =========================

    public Table JoinTables(
        string firstTableName,
        string secondTableName,
        string commonField)
    {
        Table firstTable =
            GetTable(firstTableName);

        Table secondTable =
            GetTable(secondTableName);

        if (ReferenceEquals(
                firstTable,
                secondTable))
        {
            throw new InvalidOperationException(
                "Select two different tables.");
        }

        TableOperations.ValidateJoin(
            firstTable,
            secondTable,
            commonField);

        int firstCommonIndex =
            firstTable.GetColumnIndex(
                commonField);

        int secondCommonIndex =
            secondTable.GetColumnIndex(
                commonField);

        Table resultTable =
            new($"{firstTable.Name}_{secondTable.Name}_Join");

        // Columns from the first table
        foreach (Column column
                 in firstTable.Columns)
        {
            resultTable.Columns.Add(
                CloneColumn(column));
        }

        // Columns from the second table,
        // except the repeated common field
        for (int i = 0;
             i < secondTable.Columns.Count;
             i++)
        {
            if (i == secondCommonIndex)
                continue;

            Column sourceColumn =
                secondTable.Columns[i];

            string resultName =
                sourceColumn.Name;

            // Avoid duplicate column names.
            if (resultTable.GetColumn(
                    resultName) is not null)
            {
                resultName =
                    $"{secondTable.Name}_{resultName}";
            }

            Column resultColumn =
                CloneColumn(sourceColumn);

            resultColumn.Name =
                resultName;

            resultTable.Columns.Add(
                resultColumn);
        }

        // INNER JOIN
        foreach (Row firstRow
                 in firstTable.Rows)
        {
            string firstValue =
                firstRow.Values[
                    firstCommonIndex];

            foreach (Row secondRow
                     in secondTable.Rows)
            {
                string secondValue =
                    secondRow.Values[
                        secondCommonIndex];

                if (firstValue == secondValue)
                {
                    Row mergedRow =
                        TableOperations.MergeRows(
                            firstRow,
                            secondRow,
                            secondCommonIndex);

                    resultTable.Rows.Add(
                        mergedRow);
                }
            }
        }

        return resultTable;
    }

    // =========================
    // HELPERS
    // =========================

    public Table GetTable(string tableName)
    {
        Database database =
            RequireDatabase();

        return database.GetTable(tableName)
               ?? throw new InvalidOperationException(
                   $"Table '{tableName}' does not exist.");
    }

    private Database RequireDatabase()
    {
        return CurrentDatabase
               ?? throw new InvalidOperationException(
                   "Database has not been created or loaded.");
    }

    private static Column CloneColumn(
        Column source)
    {
        return new Column
        {
            Name = source.Name,
            TypeName = source.TypeName,
            LowerBound = source.LowerBound,
            UpperBound = source.UpperBound
        };
    }
}