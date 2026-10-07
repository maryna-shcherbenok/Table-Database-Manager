using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab1IT.Domain.Types;

public abstract class BaseType
{
    public abstract string Name { get; }

    public virtual bool Validate(string? value)
    {
        return string.IsNullOrEmpty(value);
    }
}