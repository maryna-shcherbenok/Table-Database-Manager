using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Text.Json;
using Lab1IT.Domain;

namespace Lab1IT.Data;

public class PersistenceService
{
    private readonly JsonSerializerOptions _options =
        new()
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };

    public async Task SaveAsync(
        Database database,
        string filePath)
    {
        string json =
            JsonSerializer.Serialize(
                database,
                _options);

        await File.WriteAllTextAsync(
            filePath,
            json);
    }

    public async Task<Database> LoadAsync(
        string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException(
                "Database file was not found.",
                filePath);
        }

        string json =
            await File.ReadAllTextAsync(
                filePath);

        Database? database =
            JsonSerializer.Deserialize<Database>(
                json,
                _options);

        return database
               ?? throw new InvalidOperationException(
                   "Unable to load database.");
    }
}