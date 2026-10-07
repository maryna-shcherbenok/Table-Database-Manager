using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab1IT.Domain.Types;

public static class TypeFactory
{
    public static BaseType Create(
        string typeName,
        char? lowerBound = null,
        char? upperBound = null)
    {
        return typeName switch
        {
            "integer" =>
                new IntegerType(),

            "real" =>
                new RealType(),

            "char" =>
                new CharType(),

            "string" =>
                new StringType(),

            "charInvl" =>
                new CharInvlType(
                    lowerBound
                    ?? throw new ArgumentException(
                        "charInvl requires lower bound."),

                    upperBound
                    ?? throw new ArgumentException(
                        "charInvl requires upper bound.")),

            "string(charInvl)" =>
                new StringCharInvlType(
                    lowerBound
                    ?? throw new ArgumentException(
                        "string(charInvl) requires lower bound."),

                    upperBound
                    ?? throw new ArgumentException(
                        "string(charInvl) requires upper bound.")),

            _ => throw new ArgumentException(
                $"Unsupported type: {typeName}")
        };
    }
}
