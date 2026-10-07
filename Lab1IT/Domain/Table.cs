using System.Data;
using System.Text.Json.Serialization;

namespace Lab1IT.Domain;

public class Table
{
    public string Name { get; set; } = string.Empty;

    public List<Column> Columns { get; set; } = new();

    public List<Row> Rows { get; set; } = new();

    [JsonIgnore]
    public DataTable DataTable { get; } = new();

    public Table()
    {
    }

    public Table(string name)
    {
        Name = name;
        DataTable.TableName = name;
    }

    public Column? GetColumn(string name)
    {
        return Columns.FirstOrDefault(column =>
            string.Equals(
                column.Name,
                name,
                StringComparison.OrdinalIgnoreCase));
    }

    public int GetColumnIndex(string name)
    {
        return Columns.FindIndex(column =>
            string.Equals(
                column.Name,
                name,
                StringComparison.OrdinalIgnoreCase));
    }
}