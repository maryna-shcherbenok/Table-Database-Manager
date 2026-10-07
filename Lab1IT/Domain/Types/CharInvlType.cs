using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab1IT.Domain.Types;

public sealed class CharInvlType : BaseType
{
    public override string Name => "charInvl";

    public char LowerBound { get; }

    public char UpperBound { get; }

    public CharInvlType(char lowerBound, char upperBound)
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

        if (value.Length != 1)
            return false;

        char character = value[0];

        return character >= LowerBound
               && character <= UpperBound;
    }
}