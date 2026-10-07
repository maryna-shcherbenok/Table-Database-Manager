using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab1IT.Domain;

public class Row
{
    public List<string> Values { get; set; } = new();

    public Row()
    {
    }

    public Row(IEnumerable<string> values)
    {
        Values = values.ToList();
    }
}