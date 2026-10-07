using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Lab1IT.Data;
using Lab1IT.Domain;

namespace Lab1IT.Application;

public class ApplicationController
{
    public Manager Manager { get; }

    public PersistenceService Persistence { get; }

    public ApplicationController()
    {
        Manager = new Manager();
        Persistence = new PersistenceService();
    }

    public Database? CurrentDatabase =>
        Manager.CurrentDatabase;

    public async Task SaveDatabaseAsync(
        string filePath)
    {
        if (CurrentDatabase is null)
        {
            throw new InvalidOperationException(
                "There is no database to save.");
        }

        await Persistence.SaveAsync(
            CurrentDatabase,
            filePath);
    }

    public async Task LoadDatabaseAsync(
        string filePath)
    {
        Database database =
            await Persistence.LoadAsync(
                filePath);

        Manager.SetDatabase(database);
    }
}