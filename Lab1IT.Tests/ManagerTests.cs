using Lab1IT.Application;
using Lab1IT.Domain;

namespace Lab1IT.Tests;

public class ManagerTests
{
    private Manager _manager = null!;

    [SetUp]
    public void Setup()
    {
        _manager =
            new Manager();

        _manager.CreateDatabase(
            "TestDatabase");
    }

    [Test]
    public void CreateTable_AddsTableToDatabase()
    {
        _manager.CreateTable(
            "Students");

        Assert.That(
            _manager.CurrentDatabase!.Tables.Count,
            Is.EqualTo(1));

        Assert.That(
            _manager.CurrentDatabase.Tables[0].Name,
            Is.EqualTo("Students"));
    }

    [Test]
    public void CreateDuplicateTable_ThrowsException()
    {
        _manager.CreateTable(
            "Students");

        Assert.Throws<InvalidOperationException>(
            () =>
                _manager.CreateTable(
                    "Students"));
    }

    [Test]
    public void CreateColumn_AddsEmptyCellToExistingRows()
    {
        Table table =
            _manager.CreateTable(
                "Students");

        _manager.CreateColumn(
            "Students",
            "id",
            "integer");

        _manager.AddRow(
            "Students",
            new List<string>
            {
                "1"
            });

        _manager.CreateColumn(
            "Students",
            "name",
            "string");

        Assert.That(
            table.Columns.Count,
            Is.EqualTo(2));

        Assert.That(
            table.Rows[0].Values.Count,
            Is.EqualTo(2));

        Assert.That(
            table.Rows[0].Values[1],
            Is.EqualTo(string.Empty));
    }

    [Test]
    public void EditCell_InvalidInteger_ThrowsException()
    {
        Table table =
            _manager.CreateTable("Students");

        _manager.CreateColumn(
            "Students",
            "id",
            "integer");

        _manager.AddRow(
            "Students",
            new List<string>
            {
            "1"
            });

        Assert.Throws<ArgumentException>(
            () =>
                _manager.EditCell(
                    "Students",
                    0,
                    "id",
                    "abc"));

        Assert.That(
            table.Rows[0].Values[0],
            Is.EqualTo("1"));
    }
}