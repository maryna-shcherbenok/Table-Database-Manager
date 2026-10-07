using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab1IT.Domain.Types;

public sealed class StringType : BaseType
{
    public override string Name => "string";

    public override bool Validate(string? value)
    {
        return value is not null;
    }
}