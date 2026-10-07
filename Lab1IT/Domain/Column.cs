using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Text.Json.Serialization;
using Lab1IT.Domain.Types;

namespace Lab1IT.Domain;

public class Column
{
    public string Name { get; set; } = string.Empty;

    public string TypeName { get; set; } = "string";

    public char? LowerBound { get; set; }

    public char? UpperBound { get; set; }

    [JsonIgnore]
    public BaseType ColumnType =>
        TypeFactory.Create(
            TypeName,
            LowerBound,
            UpperBound);

    [JsonIgnore]
    public string DisplayType =>
        LowerBound.HasValue && UpperBound.HasValue
            ? $"{TypeName}[{LowerBound}-{UpperBound}]"
            : TypeName;

    public Column()
    {
    }

    public Column(
        string name,
        BaseType type)
    {
        Name = name;
        TypeName = type.Name;

        if (type is CharInvlType charInvl)
        {
            LowerBound = charInvl.LowerBound;
            UpperBound = charInvl.UpperBound;
        }
        else if (type is StringCharInvlType stringInvl)
        {
            LowerBound = stringInvl.LowerBound;
            UpperBound = stringInvl.UpperBound;
        }
    }
}
