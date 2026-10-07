using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Lab1IT.Domain;

namespace Lab1IT.Application;

public static class TableOperations
{
    public static void ValidateJoin(
        Table firstTable,
        Table secondTable,
        string commonField)
    {
        Column? firstColumn =
            firstTable.GetColumn(commonField);

        Column? secondColumn =
            secondTable.GetColumn(commonField);

        if (firstColumn is null)
        {
            throw new InvalidOperationException(
                $"Field '{commonField}' does not exist " +
                $"in table '{firstTable.Name}'.");
        }

        if (secondColumn is null)
        {
            throw new InvalidOperationException(
                $"Field '{commonField}' does not exist " +
                $"in table '{secondTable.Name}'.");
        }

        if (firstColumn.TypeName != secondColumn.TypeName)
        {
            throw new InvalidOperationException(
                "Common fields have incompatible data types.");
        }

        if (firstColumn.LowerBound != secondColumn.LowerBound ||
            firstColumn.UpperBound != secondColumn.UpperBound)
        {
            throw new InvalidOperationException(
                "Interval types have incompatible bounds.");
        }
    }

    public static Row MergeRows(
        Row firstRow,
        Row secondRow,
        int secondCommonFieldIndex)
    {
        List<string> values = new();

        values.AddRange(firstRow.Values);

        for (int i = 0; i < secondRow.Values.Count; i++)
        {
            if (i != secondCommonFieldIndex)
            {
                values.Add(secondRow.Values[i]);
            }
        }

        return new Row(values);
    }
}