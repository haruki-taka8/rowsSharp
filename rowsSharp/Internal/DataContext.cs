﻿using rowsSharp.DataStore;
using rowsSharp.Domain;
using rowsSharp.ViewModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;

namespace rowsSharp;

internal class DataContext : INPC
{
    // DataStore
    public OperationHistory OperationHistory { get; } = new();
    public Config Config { get; } = new();
    public Csv Csv { get; } = new();

    // ViewModel
    public Command Command { get; }
    public RecordsView RecordsView { get; }
    public Status Status { get; } = new();

    // Domain
    internal readonly Filter Filter;
    internal readonly Preview Preview;
    internal readonly Edit Edit;
    internal readonly History History;

    internal DataContext()
    {
        Config = Domain.IO.Config.Import();

        Csv = Domain.IO.Csv.Import(Config.CsvPath, Config.HasHeader);
        RecordsView = new(Csv.Records);

        Filter = new( Status, Config, Csv, RecordsView);
        Preview = new(Status, Csv, Config.PreviewPath, Config.CopyRowFormat);
        History = new(Status, OperationHistory, Csv);
        Edit = new(Status, Config, Csv, History);
        Command = new(Edit, Filter, Preview, History, Status, Config, OperationHistory);

        if (!Csv.Records.Any())
        {
            App.Logger.Warn("CSV file not found. Starting creation wizard.");
            _ = new InitWizardWindow(Config);

            Csv = Domain.IO.Csv.Import(Config.CsvPath, Config.HasHeader);
            RecordsView = new(Csv.Records);
        }
    }

    public DelegateCommand<CancelEventArgs> Exit => new(
        (e) =>
        {
            App.Logger.Info("Changes unsaved, asking for confirmation before exiting.");
            MessageBoxResult dialog = MessageBox.Show(
                "Save changes before exiting?",
                "RowsSharp",
                MessageBoxButton.YesNoCancel,
                MessageBoxImage.Question
            );

            if (dialog == MessageBoxResult.Cancel)
            {
                e.Cancel = true;
            }
            else if (dialog == MessageBoxResult.Yes)
            {
                Edit.Save();
            }
        },
        (e) => Status.IsDirtyEditor
    );
}