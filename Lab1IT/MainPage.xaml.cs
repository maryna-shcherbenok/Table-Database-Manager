using CommunityToolkit.Maui.Storage;

using Lab1IT.Application;
using Lab1IT.Domain;

using Syncfusion.Maui.DataGrid;
using Syncfusion.Maui.DataGrid.Helper;
using Syncfusion.Maui.TabView;

using System.Data;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace Lab1IT;

public partial class MainPage : ContentPage
{
    // =========================================================
    // FIELDS
    // =========================================================

    private readonly ApplicationController _controller;

    private DataTable? _dataTable;

    private int _selectedColumnIndex = -1;
    private int _selectedRowIndex = -1;

    private string _selectedColumnName =
        string.Empty;

    private readonly CancellationTokenSource
        _cancellationTokenSource = new();

    // =========================================================
    // AVAILABLE DATA TYPES
    // =========================================================

    public List<string> DataTypes { get; } =
        new()
        {
            "integer",
            "real",
            "char",
            "string",
            "charInvl",
            "string(charInvl)"
        };

    // =========================================================
    // CURRENT DATABASE
    // =========================================================

    private Database? CurrentDatabase =>
        _controller.CurrentDatabase;

    // =========================================================
    // CURRENT TABLE
    // =========================================================

    private Table? CurrentTable
    {
        get
        {
            if (CurrentDatabase is null)
                return null;

            if (TabView.SelectedIndex < 0)
                return null;

            int index =
                (int)TabView.SelectedIndex;

            if (index < 0 ||
                index >= TabView.Items.Count)
            {
                return null;
            }

            return TabView.Items[index]
                .BindingContext as Table;
        }
    }

    // =========================================================
    // CONSTRUCTOR
    // =========================================================

    public MainPage()
    {
        InitializeComponent();

        _controller =
            new ApplicationController();

        TypeComboBox.BindingContext =
            this;

        BoundsPanel.IsVisible =
            false;
    }

    // =========================================================
    // DATABASE - CREATE
    // =========================================================

    private async void OnCreateDatabase(
        object sender,
        EventArgs e)
    {
        if (CurrentDatabase is not null)
        {
            bool answer =
                await DisplayAlert(
                    "Warning",
                    "Creating a new database will lose all unsaved changes. Continue?",
                    "Yes",
                    "No");

            if (!answer)
                return;
        }

        string? name =
            await DisplayPromptAsync(
                "Create a new database",
                "Please enter database name:",
                "Create",
                "Cancel");

        if (string.IsNullOrWhiteSpace(name))
            return;

        try
        {
            ClearInterface();

            _controller.Manager.CreateDatabase(
                name.Trim());

            DbNameLabel.Text =
                name.Trim();

            MainScroll.IsVisible =
                true;
        }
        catch (Exception ex)
        {
            await DisplayAlert(
                "Error",
                ex.Message,
                "OK");
        }
    }

    // =========================================================
    // DATABASE - LOAD
    // =========================================================

    private async void OnLoadDatabase(
        object sender,
        EventArgs e)
    {
        try
        {
            if (CurrentDatabase is not null)
            {
                bool answer =
                    await DisplayAlert(
                        "Warning",
                        "Loading another database will lose all unsaved changes. Continue?",
                        "Yes",
                        "No");

                if (!answer)
                    return;
            }

            FilePickerFileType jsonType =
                new(
                    new Dictionary<
                        DevicePlatform,
                        IEnumerable<string>>
                    {
                        {
                            DevicePlatform.WinUI,
                            new[] { ".json" }
                        },

                        {
                            DevicePlatform.Android,
                            new[]
                            {
                                "application/json",
                                "text/json"
                            }
                        },

                        {
                            DevicePlatform.iOS,
                            new[]
                            {
                                "public.json",
                                "public.text"
                            }
                        },

                        {
                            DevicePlatform.MacCatalyst,
                            new[]
                            {
                                "public.json",
                                "public.text"
                            }
                        }
                    });

            FileResult? result =
                await FilePicker.Default.PickAsync(
                    new PickOptions
                    {
                        PickerTitle =
                            "Select database JSON file",

                        FileTypes =
                            jsonType
                    });

            if (result is null)
                return;

            if (!result.FileName.EndsWith(
                    ".json",
                    StringComparison.OrdinalIgnoreCase))
            {
                await DisplayAlert(
                    "Error",
                    "The selected file is not a JSON file.",
                    "OK");

                return;
            }

            if (string.IsNullOrWhiteSpace(
                    result.FullPath))
            {
                await DisplayAlert(
                    "Error",
                    "Unable to access the selected file.",
                    "OK");

                return;
            }

            await _controller.LoadDatabaseAsync(
                result.FullPath);

            if (CurrentDatabase is null)
            {
                await DisplayAlert(
                    "Error",
                    "Unable to load database.",
                    "OK");

                return;
            }

            ShowDatabase(
                CurrentDatabase);
        }
        catch (Exception ex)
        {
            await DisplayAlert(
                "Error",
                ex.Message,
                "OK");
        }
    }

    // =========================================================
    // DATABASE - SAVE
    // =========================================================

    private async void OnSaveDatabase(
        object sender,
        EventArgs e)
    {
        if (CurrentDatabase is null)
        {
            await DisplayAlert(
                "Error",
                "There is no database to save.",
                "OK");

            return;
        }

        try
        {
            using MemoryStream stream =
                new();

            JsonSerializerOptions options =
                new()
                {
                    WriteIndented = true,

                    Encoder =
                        JavaScriptEncoder
                            .UnsafeRelaxedJsonEscaping
                };

            await JsonSerializer.SerializeAsync(
                stream,
                CurrentDatabase,
                options);

            stream.Position = 0;

            string defaultName =
                $"{CurrentDatabase.Name}.json";

            FileSaverResult result =
                await FileSaver.Default.SaveAsync(
                    defaultName,
                    stream,
                    _cancellationTokenSource.Token);

            if (result.IsSuccessful)
            {
                await DisplayAlert(
                    "Saved",
                    $"Database was saved successfully.\n{result.FilePath}",
                    "OK");
            }
            else
            {
                await DisplayAlert(
                    "Error",
                    result.Exception?.Message ??
                    "Database was not saved.",
                    "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert(
                "Error",
                ex.Message,
                "OK");
        }
    }

    // =========================================================
    // TABLE - ADD
    // =========================================================

    private async void OnAddTable(
        object sender,
        EventArgs e)
    {
        if (CurrentDatabase is null)
        {
            await DisplayAlert(
                "Error",
                "Create or load a database first.",
                "OK");

            return;
        }

        string tableName =
            TableNameEntry.Text?.Trim()
            ?? string.Empty;

        if (string.IsNullOrWhiteSpace(
                tableName))
        {
            await DisplayAlert(
                "Can't create a table",
                "Enter a table name.",
                "OK");

            return;
        }

        try
        {
            Table table =
                _controller.Manager.CreateTable(
                    tableName);

            AddTableTab(
                table);

            TabView.SelectedIndex =
                TabView.Items.Count - 1;

            _dataTable =
                table.DataTable;

            DataGrid.ItemsSource =
                _dataTable.DefaultView;

            VisualizeGrid(
                table);

            TableNameEntry.Text =
                string.Empty;
        }
        catch (Exception ex)
        {
            await DisplayAlert(
                "Can't create a table",
                ex.Message,
                "OK");
        }
    }

    // =========================================================
    // TABLE - DELETE
    // =========================================================

    private async void OnDeleteTable(
        object sender,
        EventArgs e)
    {
        Table? table =
            CurrentTable;

        if (table is null)
        {
            await DisplayAlert(
                "Nothing selected",
                "Select a table tab first.",
                "OK");

            return;
        }

        bool answer =
            await DisplayAlert(
                "Delete table",
                $"Delete table '{table.Name}'?",
                "Delete",
                "Cancel");

        if (!answer)
            return;

        try
        {
            int currentIndex =
                (int)TabView.SelectedIndex;

            _controller.Manager.DeleteTable(
                table.Name);

            SfTabItem selectedTab =
                TabView.Items[currentIndex];

            TabView.Items.Remove(
                selectedTab);

            ResetCellSelection();

            if (TabView.Items.Count > 0)
            {
                int newIndex =
                    Math.Min(
                        currentIndex,
                        TabView.Items.Count - 1);

                TabView.SelectedIndex =
                    newIndex;

                Table? newTable =
                    CurrentTable;

                if (newTable is not null)
                {
                    VisualizeGrid(
                        newTable);
                }
            }
            else
            {
                DataGrid.ItemsSource =
                    null;

                DataGrid.Columns.Clear();

                _dataTable =
                    null;
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert(
                "Error",
                ex.Message,
                "OK");
        }
    }

    // =========================================================
    // TYPE SELECTION
    // =========================================================

    private void OnTypeSelectionChanged(
        object sender,
        Syncfusion.Maui.Inputs.SelectionChangedEventArgs e)
    {
        string typeName =
            TypeComboBox.SelectedItem?.ToString()
            ?? TypeComboBox.Text
            ?? string.Empty;

        BoundsPanel.IsVisible =
            typeName == "charInvl"
            ||
            typeName == "string(charInvl)";
    }

    // =========================================================
    // COLUMN - ADD
    // =========================================================

    private async void OnAddColumn(
        object sender,
        EventArgs e)
    {
        Table? table =
            CurrentTable;

        if (table is null)
        {
            await DisplayAlert(
                "Nothing selected",
                "Select a table first.",
                "OK");

            return;
        }

        string columnName =
            ColumnNameEntry.Text?.Trim()
            ?? string.Empty;

        string typeName =
            TypeComboBox.SelectedItem?.ToString()
            ?? TypeComboBox.Text
            ?? string.Empty;

        if (string.IsNullOrWhiteSpace(
                columnName))
        {
            await DisplayAlert(
                "Can't create a column",
                "Enter a column name.",
                "OK");

            return;
        }

        if (string.IsNullOrWhiteSpace(
                typeName))
        {
            await DisplayAlert(
                "Can't create a column",
                "Select a data type.",
                "OK");

            return;
        }

        char? lowerBound = null;
        char? upperBound = null;

        if (typeName == "charInvl"
            ||
            typeName == "string(charInvl)")
        {
            string lower =
                LowerBoundEntry.Text
                ?? string.Empty;

            string upper =
                UpperBoundEntry.Text
                ?? string.Empty;

            if (lower.Length != 1
                ||
                upper.Length != 1)
            {
                await DisplayAlert(
                    "Invalid interval",
                    "Enter exactly one character for each interval bound.",
                    "OK");

                return;
            }

            lowerBound =
                lower[0];

            upperBound =
                upper[0];

            if (lowerBound > upperBound)
            {
                await DisplayAlert(
                    "Invalid interval",
                    "Lower bound cannot be greater than upper bound.",
                    "OK");

                return;
            }
        }

        try
        {
            _controller.Manager.CreateColumn(
                table.Name,
                columnName,
                typeName,
                lowerBound,
                upperBound);

            ResetCellSelection();

            VisualizeGrid(
                table);

            ColumnNameEntry.Text =
                string.Empty;

            TypeComboBox.SelectedItem =
                null;

            TypeComboBox.Text =
                string.Empty;

            LowerBoundEntry.Text =
                string.Empty;

            UpperBoundEntry.Text =
                string.Empty;

            BoundsPanel.IsVisible =
                false;
        }
        catch (Exception ex)
        {
            await DisplayAlert(
                "Can't create a column",
                ex.Message,
                "OK");
        }
    }

    // =========================================================
    // COLUMN - DELETE
    // =========================================================

    private async void OnDeleteColumn(
        object sender,
        EventArgs e)
    {
        Table? table =
            CurrentTable;

        if (table is null)
        {
            await DisplayAlert(
                "Nothing selected",
                "Select a table first.",
                "OK");

            return;
        }

        if (_selectedColumnIndex < 0
            ||
            _selectedColumnIndex >= table.Columns.Count)
        {
            await DisplayAlert(
                "Nothing selected",
                "Single-click a cell in the column you want to delete.",
                "OK");

            return;
        }

        Column column =
            table.Columns[
                _selectedColumnIndex];

        bool answer =
            await DisplayAlert(
                "Delete column",
                $"Delete column '{column.Name}'?",
                "Delete",
                "Cancel");

        if (!answer)
            return;

        try
        {
            _controller.Manager.DeleteColumn(
                table.Name,
                column.Name);

            ResetCellSelection();

            VisualizeGrid(
                table);
        }
        catch (Exception ex)
        {
            await DisplayAlert(
                "Error",
                ex.Message,
                "OK");
        }
    }

    // =========================================================
    // ROW - ADD
    // =========================================================

    private async void OnAddRow(
        object sender,
        EventArgs e)
    {
        Table? table =
            CurrentTable;

        if (table is null)
        {
            await DisplayAlert(
                "Nothing selected",
                "Select a table first.",
                "OK");

            return;
        }

        if (table.Columns.Count == 0)
        {
            await DisplayAlert(
                "Can't add row",
                "Create at least one column first.",
                "OK");

            return;
        }

        List<string> values =
            Enumerable
                .Repeat(
                    string.Empty,
                    table.Columns.Count)
                .ToList();

        table.Rows.Add(
            new Row(values));

        ResetCellSelection();

        VisualizeGrid(
            table);
    }

    // =========================================================
    // ROW - DELETE
    // =========================================================

    private async void OnDeleteRow(
        object sender,
        EventArgs e)
    {
        Table? table =
            CurrentTable;

        if (table is null)
        {
            await DisplayAlert(
                "Nothing selected",
                "Select a table first.",
                "OK");

            return;
        }

        if (_selectedRowIndex < 0
            ||
            _selectedRowIndex >= table.Rows.Count)
        {
            await DisplayAlert(
                "Nothing selected",
                "Single-click a cell in the row you want to delete.",
                "OK");

            return;
        }

        bool answer =
            await DisplayAlert(
                "Delete row",
                $"Delete row {_selectedRowIndex + 1}?",
                "Delete",
                "Cancel");

        if (!answer)
            return;

        table.Rows.RemoveAt(
            _selectedRowIndex);

        ResetCellSelection();

        VisualizeGrid(
            table);
    }

    // =========================================================
    // TAB SELECTION
    // =========================================================

    private void OnTabSelectionChanged(
        object sender,
        TabSelectionChangedEventArgs e)
    {
        Table? table =
            CurrentTable;

        if (table is null)
            return;

        ResetCellSelection();

        VisualizeGrid(
            table);
    }

    // =========================================================
    // CELL ACTIVATED
    //
    // IMPORTANT:
    // This event happens when the user single-clicks a cell.
    // It allows Delete Row / Delete Column to work without
    // entering edit mode first.
    // =========================================================

    private void OnCurrentCellActivated(
    object sender,
    DataGridCurrentCellActivatedEventArgs e)
    {
        Table? table = CurrentTable;

        if (table is null)
            return;

        // ---------------- ROW ----------------

        int recordIndex =
            DataGrid.ResolveToRecordIndex(
                e.NewRowColumnIndex.RowIndex);

        if (recordIndex >= 0 &&
            recordIndex < table.Rows.Count)
        {
            _selectedRowIndex =
                recordIndex;
        }

        // ---------------- COLUMN ----------------

        int visibleColumnIndex =
            e.NewRowColumnIndex.ColumnIndex;

        int gridColumnIndex =
            DataGrid.ResolveToGridVisibleColumnIndex(
                visibleColumnIndex);

        if (gridColumnIndex >= 0 &&
            gridColumnIndex < DataGrid.Columns.Count)
        {
            string mappingName =
                DataGrid.Columns[gridColumnIndex]
                    .MappingName
                ?? string.Empty;

            int modelColumnIndex =
                table.Columns.FindIndex(
                    column =>
                        string.Equals(
                            column.Name,
                            mappingName,
                            StringComparison.OrdinalIgnoreCase));

            if (modelColumnIndex >= 0)
            {
                _selectedColumnIndex =
                    modelColumnIndex;

                _selectedColumnName =
                    table.Columns[
                        modelColumnIndex].Name;
            }
        }
    }

    // =========================================================
    // GRID - BEGIN EDIT
    // =========================================================

    private void OnCurrentCellBeginEdit(
        object sender,
        DataGridCurrentCellBeginEditEventArgs e)
    {
        Table? table =
            CurrentTable;

        if (table is null
            ||
            e.Column is null)
        {
            return;
        }

        _selectedColumnName =
            e.Column.MappingName
            ?? string.Empty;

        _selectedColumnIndex =
            table.Columns.FindIndex(
                column =>
                    string.Equals(
                        column.Name,
                        _selectedColumnName,
                        StringComparison.OrdinalIgnoreCase));

        int recordIndex =
            DataGrid.ResolveToRecordIndex(
                e.RowColumnIndex.RowIndex);

        if (recordIndex >= 0
            &&
            recordIndex < table.Rows.Count)
        {
            _selectedRowIndex =
                recordIndex;
        }
    }

    // =========================================================
    // GRID - END EDIT / VALUE VALIDATION
    // =========================================================

    private void OnCurrentCellEndEdit(
        object sender,
        DataGridCurrentCellEndEditEventArgs e)
    {
        Table? table =
            CurrentTable;

        if (table is null)
            return;

        string columnName =
            _selectedColumnName;

        int columnIndex =
            table.Columns.FindIndex(
                column =>
                    string.Equals(
                        column.Name,
                        columnName,
                        StringComparison.OrdinalIgnoreCase));

        if (columnIndex < 0)
            return;

        int recordIndex =
            DataGrid.ResolveToRecordIndex(
                e.RowColumnIndex.RowIndex);

        if (recordIndex < 0
            ||
            recordIndex >= table.Rows.Count)
        {
            return;
        }

        string newValue =
            e.NewValue?.ToString()
            ?? string.Empty;

        Column column =
            table.Columns[
                columnIndex];

        // Empty cells are allowed while filling the row.
        // Non-empty values must satisfy the data type.

        if (!string.IsNullOrEmpty(newValue)
            &&
            !column.ColumnType.Validate(
                newValue))
        {
            e.Cancel =
                true;

            DisplayAlert(
                "Invalid value",
                $"Value '{newValue}' is not valid for " +
                $"column '{column.Name}' of type " +
                $"'{column.DisplayType}'.",
                "OK");

            return;
        }

        table.Rows[recordIndex]
            .Values[columnIndex] =
            newValue;

        _selectedRowIndex =
            recordIndex;

        _selectedColumnIndex =
            columnIndex;
    }

    // =========================================================
    // JOIN - VARIANT 8
    // =========================================================

    private async void OnJoinTables(
        object sender,
        EventArgs e)
    {
        if (CurrentDatabase is null)
        {
            await DisplayAlert(
                "Error",
                "Create or load a database first.",
                "OK");

            return;
        }

        if (CurrentDatabase.Tables.Count < 2)
        {
            await DisplayAlert(
                "JOIN",
                "At least two tables are required.",
                "OK");

            return;
        }

        string[] tableNames =
            CurrentDatabase.Tables
                .Select(
                    table =>
                        table.Name)
                .ToArray();

        // ---------------- FIRST TABLE ----------------

        string? firstTable =
            await DisplayActionSheet(
                "First table",
                "Cancel",
                null,
                tableNames);

        if (string.IsNullOrWhiteSpace(
                firstTable)
            ||
            firstTable == "Cancel")
        {
            return;
        }

        // ---------------- SECOND TABLE ----------------

        string[] secondTableNames =
            tableNames
                .Where(
                    name =>
                        name != firstTable)
                .ToArray();

        string? secondTable =
            await DisplayActionSheet(
                "Second table",
                "Cancel",
                null,
                secondTableNames);

        if (string.IsNullOrWhiteSpace(
                secondTable)
            ||
            secondTable == "Cancel")
        {
            return;
        }

        Table first =
            _controller.Manager.GetTable(
                firstTable);

        Table second =
            _controller.Manager.GetTable(
                secondTable);

        // ---------------- COMMON FIELDS ----------------

        string[] commonFields =
            first.Columns
                .Select(
                    column =>
                        column.Name)
                .Where(
                    name =>
                        second.GetColumn(name)
                        is not null)
                .ToArray();

        if (commonFields.Length == 0)
        {
            await DisplayAlert(
                "JOIN",
                "The selected tables have no common fields.",
                "OK");

            return;
        }

        string? commonField =
            await DisplayActionSheet(
                "Common field",
                "Cancel",
                null,
                commonFields);

        if (string.IsNullOrWhiteSpace(
                commonField)
            ||
            commonField == "Cancel")
        {
            return;
        }

        try
        {
            Table result =
                _controller.Manager.JoinTables(
                    firstTable,
                    secondTable,
                    commonField);

            /*
             * If Manager only returns the resulting table
             * without adding it to the database,
             * the UI adds it here.
             */

            if (!CurrentDatabase.Tables.Contains(
                    result))
            {
                string baseName =
                    result.Name;

                string newName =
                    baseName;

                int number = 2;

                while (
                    CurrentDatabase.Tables.Any(
                        table =>
                            string.Equals(
                                table.Name,
                                newName,
                                StringComparison.OrdinalIgnoreCase)))
                {
                    newName =
                        $"{baseName}_{number}";

                    number++;
                }

                result.Name =
                    newName;

                CurrentDatabase.Tables.Add(
                    result);
            }

            AddTableTab(
                result);

            TabView.SelectedIndex =
                TabView.Items.Count - 1;

            ResetCellSelection();

            VisualizeGrid(
                result);

            await DisplayAlert(
                "JOIN completed",
                $"Result table '{result.Name}' was created.\n" +
                $"Rows: {result.Rows.Count}",
                "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert(
                "JOIN error",
                ex.Message,
                "OK");
        }
    }

    // =========================================================
    // ADD TABLE TAB
    // =========================================================

    private void AddTableTab(
        Table table)
    {
        SfTabItem tab =
            new()
            {
                Header =
                    table.Name,

                BindingContext =
                    table
            };

        TabView.Items.Add(
            tab);
    }

    // =========================================================
    // VISUALIZE TABLE
    // =========================================================

    private void VisualizeGrid(
        Table table)
    {
        DataGrid.ItemsSource =
            null;

        DataGrid.Columns.Clear();

        DataTable dataTable =
            table.DataTable;

        dataTable.Columns.Clear();
        dataTable.Rows.Clear();

        // ---------------- DATA TABLE COLUMNS ----------------

        foreach (Column column
                 in table.Columns)
        {
            dataTable.Columns.Add(
                column.Name);
        }

        // ---------------- VISUAL GRID COLUMNS ----------------

        foreach (Column column
                 in table.Columns)
        {
            DataGrid.Columns.Add(
                new DataGridTextColumn
                {
                    MappingName =
                        column.Name,

                    HeaderText =
                        $"{column.Name}\n" +
                        $"{column.DisplayType}"
                });
        }

        // ---------------- ROWS ----------------

        foreach (Row row
                 in table.Rows)
        {
            System.Data.DataRow dataRow =
                dataTable.NewRow();

            for (int i = 0;
                 i < table.Columns.Count;
                 i++)
            {
                dataRow[i] =
                    i < row.Values.Count
                        ? row.Values[i]
                        : string.Empty;
            }

            dataTable.Rows.Add(
                dataRow);
        }

        _dataTable =
            dataTable;

        DataGrid.ItemsSource =
            dataTable.DefaultView;
    }

    // =========================================================
    // SHOW LOADED DATABASE
    // =========================================================

    private void ShowDatabase(
        Database database)
    {
        ClearInterface();

        DbNameLabel.Text =
            database.Name;

        foreach (Table table
                 in database.Tables)
        {
            AddTableTab(
                table);
        }

        MainScroll.IsVisible =
            true;

        if (TabView.Items.Count > 0)
        {
            TabView.SelectedIndex =
                0;

            Table? table =
                CurrentTable;

            if (table is not null)
            {
                VisualizeGrid(
                    table);
            }
        }
    }

    // =========================================================
    // RESET CELL SELECTION STATE
    // =========================================================

    private void ResetCellSelection()
    {
        _selectedColumnIndex =
            -1;

        _selectedRowIndex =
            -1;

        _selectedColumnName =
            string.Empty;
    }

    // =========================================================
    // CLEAR GUI
    // =========================================================

    private void ClearInterface()
    {
        TabView.Items.Clear();

        DataGrid.ItemsSource =
            null;

        DataGrid.Columns.Clear();

        _dataTable =
            null;

        ResetCellSelection();

        TableNameEntry.Text =
            string.Empty;

        ColumnNameEntry.Text =
            string.Empty;

        TypeComboBox.SelectedItem =
            null;

        TypeComboBox.Text =
            string.Empty;

        LowerBoundEntry.Text =
            string.Empty;

        UpperBoundEntry.Text =
            string.Empty;

        BoundsPanel.IsVisible =
            false;
    }
}