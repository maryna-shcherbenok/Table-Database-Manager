using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Globalization;

namespace Lab1IT.Domain.Types;

public sealed class RealType : BaseType
{
    public override string Name => "real";

    public override bool Validate(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return true;

        string normalized = value.Replace(',', '.');

        return double.TryParse(
            normalized,
            NumberStyles.Float,
            CultureInfo.InvariantCulture,
            out _);
    }
}
