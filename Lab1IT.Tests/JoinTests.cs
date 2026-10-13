using Lab1IT.Application;
using Lab1IT.Domain;

namespace Lab1IT.Tests;

public class JoinTests
{
    private Manager _manager = null!;

    [SetUp]
    public void Setup()
    {
        _manager =
            new Manager();

        _manager.CreateDatabase(
            "University");

        // =====================================
        // STUDENTS
        // =====================================

        _manager.CreateTable(
            "Students");

        _manager.CreateColumn(
            "Students",
            "id",
            "integer");

        _manager.CreateColumn(
            "Students",
            "name",
            "string");

        _manager.AddRow(
            "Students",
            new List<string>
            {
                "1",
                "Anna"
            });

        _manager.AddRow(
            "Students",
            new List<string>
            {
                "2",
                "Mark"
            });

        _manager.AddRow(
            "Students",
            new List<string>
            {
                "4",
                "Kate"
            });

        // =====================================
        // GRADES
        // =====================================

        _manager.CreateTable(
            "Grades");

        _manager.CreateColumn(
            "Grades",
            "id",
            "integer");

        _manager.CreateColumn(
            "Grades",
            "course",
            "string");

        _manager.CreateColumn(
            "Grades",
            "grade",
            "integer");

        _manager.AddRow(
            "Grades",
            new List<string>
            {
                "1",
                "Databases",
                "95"
            });

        _manager.AddRow(
            "Grades",
            new List<string>
            {
                "2",
                "Databases",
                "88"
            });

        _manager.AddRow(
            "Grades",
            new List<string>
            {
                "3",
                "Algorithms",
                "91"
            });
    }

    [Test]
    public void JoinTables_CommonField_ReturnsOnlyMatchingRows()
    {
        Table result =
            _manager.JoinTables(
                "Students",
                "Grades",
                "id");

        Assert.That(
            result.Rows.Count,
            Is.EqualTo(2));
    }

    [Test]
    public void JoinTables_CommonField_DoesNotDuplicateJoinColumn()
    {
        Table result =
            _manager.JoinTables(
                "Students",
                "Grades",
                "id");

        int idCount =
            result.Columns.Count(
                column =>
                    column.Name == "id");

        Assert.That(
            idCount,
            Is.EqualTo(1));
    }

    [Test]
    public void JoinTables_CommonField_MergesValuesCorrectly()
    {
        Table result =
            _manager.JoinTables(
                "Students",
                "Grades",
                "id");

        Assert.That(
            result.Rows[0].Values,
            Is.EqualTo(
                new[]
                {
                    "1",
                    "Anna",
                    "Databases",
                    "95"
                }));

        Assert.That(
            result.Rows[1].Values,
            Is.EqualTo(
                new[]
                {
                    "2",
                    "Mark",
                    "Databases",
                    "88"
                }));
    }

    [Test]
    public void JoinTables_IncompatibleTypes_ThrowsException()
    {
        _manager.CreateTable(
            "Other");

        _manager.CreateColumn(
            "Other",
            "id",
            "string");

        Assert.Throws<InvalidOperationException>(
            () =>
                _manager.JoinTables(
                    "Students",
                    "Other",
                    "id"));
    }

    [Test]
    public void JoinTables_ConflictingColumnNames_GeneratesUniqueNames()
    {
        _manager.CreateColumn("Students", "score", "integer");
        _manager.CreateColumn("Students", "Grades_score", "integer");
        _manager.CreateColumn("Grades", "score", "integer");

        Table result = _manager.JoinTables(
            "Students", "Grades", "id");

        string[] columnNames = result.Columns
            .Select(c => c.Name)
            .ToArray();

        Assert.That(
            columnNames.Distinct(
                StringComparer.OrdinalIgnoreCase).Count(),
            Is.EqualTo(columnNames.Length)
        );

        Assert.That(columnNames,
            Does.Contain("Grades_score_2"));

        Assert.That(result.Rows.Count,
            Is.EqualTo(2));
    }
}