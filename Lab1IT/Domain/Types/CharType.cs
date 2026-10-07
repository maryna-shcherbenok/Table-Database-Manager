using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab1IT.Domain.Types;

public sealed class CharType : BaseType
{
    public override string Name => "char";

    public override bool Validate(string? value)
    {
        return string.IsNullOrEmpty(value)
               || value.Length == 1;
    }
}
