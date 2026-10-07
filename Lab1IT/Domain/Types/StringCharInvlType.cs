using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab1IT.Domain.Types;

public sealed class StringCharInvlType : BaseType
{
    public override string Name => "string(charInvl)";

    public char LowerBound { get; }

    public char UpperBound { get; }

    public StringCharInvlType(
        char lowerBound,
        char upperBound)
    {
        if (lowerBound > upperBound)
        {
            throw new ArgumentException(
                "Lower bound must not be greater than upper bound.");
        }

        LowerBound = lowerBound;
        UpperBound = upperBound;
    }

    public override bool Validate(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return true;

        return value.All(
            character =>
                character >= LowerBound
                && character <= UpperBound);
    }
}
