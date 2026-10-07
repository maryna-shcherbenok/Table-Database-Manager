using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab1IT.Domain.Types;

public sealed class IntegerType : BaseType
{
    public override string Name => "integer";

    public override bool Validate(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return true;

        return int.TryParse(value, out _);
    }
}