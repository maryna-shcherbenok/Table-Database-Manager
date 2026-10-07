using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab1IT.Domain;

public class Database
{
    public string Name { get; set; } = string.Empty;

    public List<Table> Tables { get; set; } = new();

    public Database()
    {
    }

    public Database(string name)
    {
        Name = name;
    }

    public Table? GetTable(string name)
    {
        return Tables.FirstOrDefault(table =>
            string.Equals(
                table.Name,
                name,
                StringComparison.OrdinalIgnoreCase));
    }
}
