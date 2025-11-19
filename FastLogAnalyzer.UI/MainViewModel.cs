using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FastLogAnalyzer.Core;
using FastLogAnalyzer.Logic;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;

namespace FastLogAnalyzer.UI;

public partial class MainViewModel : ObservableObject
{
    private readonly IServiceProvider _services;
    private readonly System.Timers.Timer _memoryTimer;

    [ObservableProperty] private string _filePath = "Select a file...";
    [ObservableProperty] private string _statusMessage = "Ready";
    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private double _elapsedTime;
    [ObservableProperty] private bool _useFastParser = true;
    [ObservableProperty] private long _peakMemory;
    
    public ObservableCollection<double> MemoryValues { get; } = new();
    public ISeries[] Series { get; set; }

    public MainViewModel(IServiceProvider services)
    {
        _services = services;

        Series = new ISeries[]
        {
            new LineSeries<double>
            {
                Values = MemoryValues,
                Fill = null,
                GeometrySize = 0,
                LineSmoothness = 0
            }
        };

        _memoryTimer = new System.Timers.Timer(100);
        _memoryTimer.Elapsed += (s, e) =>
        {
            double currentMem = GC.GetTotalMemory(false) / 1024.0 / 1024.0;
            if (currentMem > PeakMemory) PeakMemory = (long)currentMem;

            Application.Current.Dispatcher.Invoke(() =>
            {
                MemoryValues.Add(currentMem);
                if (MemoryValues.Count > 200) MemoryValues.RemoveAt(0);
            });
        };
    }

    [RelayCommand]
        private void SelectFile()
        {
            var dialog = new OpenFileDialog { Filter = "CSV Files|*.csv" };
            if (dialog.ShowDialog() == true)
            {
                FilePath = dialog.FileName;
            }
        }

    [RelayCommand]
    private async Task RunBenchmark()
    {
        if (!File.Exists(FilePath))
        {
            StatusMessage = "File not found";
            return;
        }

        IsBusy = true;
        StatusMessage = "Processing...";
        MemoryValues.Clear();
        PeakMemory = 0;
        _memoryTimer.Start();

        ILogParser parser = UseFastParser
            ? _services.GetRequiredService<FastLogParser>()
            : _services.GetRequiredService<LegacyLogParser>();

        var sw = Stopwatch.StartNew();

        try
        {
            LogEntry[] result = await Task.Run(async () =>
                await parser.ParseAsync(FilePath, CancellationToken.None));

            sw.Stop();
            ElapsedTime = sw.Elapsed.TotalSeconds;
            StatusMessage = $"Done! Parsed {result.Length:N0} lines";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
        finally
        {
            _memoryTimer.Stop();
            IsBusy = false;
            GC.Collect();
        }
    }
}

