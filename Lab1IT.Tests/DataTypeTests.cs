using Lab1IT.Domain.Types;

namespace Lab1IT.Tests;

public class DataTypeTests
{
    [Test]
    public void CharInvl_InsideInterval_ReturnsTrue()
    {
        CharInvlType type =
            new CharInvlType('A', 'F');

        bool result =
            type.Validate("C");

        Assert.That(
            result,
            Is.True);
    }

    [Test]
    public void CharInvl_OutsideInterval_ReturnsFalse()
    {
        CharInvlType type =
            new CharInvlType('A', 'F');

        bool result =
            type.Validate("Z");

        Assert.That(
            result,
            Is.False);
    }

    [Test]
    public void CharInvl_MoreThanOneCharacter_ReturnsFalse()
    {
        CharInvlType type =
            new CharInvlType('A', 'F');

        bool result =
            type.Validate("AB");

        Assert.That(
            result,
            Is.False);
    }

    [Test]
    public void StringCharInvl_ValidString_ReturnsTrue()
    {
        StringCharInvlType type =
            new StringCharInvlType('A', 'Z');

        bool result =
            type.Validate("KNU");

        Assert.That(
            result,
            Is.True);
    }

    [Test]
    public void StringCharInvl_InvalidCharacter_ReturnsFalse()
    {
        StringCharInvlType type =
            new StringCharInvlType('A', 'Z');

        bool result =
            type.Validate("KNU123");

        Assert.That(
            result,
            Is.False);
    }
}