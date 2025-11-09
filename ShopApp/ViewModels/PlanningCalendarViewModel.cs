// ViewModels/PlanningCalendarViewModel.cs
using Avalonia.Controls;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using QuadroApp.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace QuadroApp.ViewModels;

public partial class PlanningCalendarViewModel : ObservableObject
{
    private readonly IDbContextFactory<AppDbContext>? _factory;

    public PlanningCalendarViewModel() { /* design-time */ }
    public PlanningCalendarViewModel(IDbContextFactory<AppDbContext> factory) => _factory = factory;

    // Header
    [ObservableProperty] private string title = "Werkbezetting kalender";
    [ObservableProperty] private string monthTitle = "";
    [ObservableProperty] private int year = DateTime.Now.Year;
    [ObservableProperty] private int month = DateTime.Now.Month;

    // Tiles + weeklijst + dagagenda
    [ObservableProperty] private ObservableCollection<DayTile> monthDays = new();
    [ObservableProperty] private ObservableCollection<WeekSummary> weekSummaries = new();
    [ObservableProperty] private ObservableCollection<AgendaRow> agendaItems = new();
    [ObservableProperty] private string agendaHeader = "Vandaag";

    // Huidige selectie
    [ObservableProperty] private DateTime selectedDate = DateTime.Today;
    [ObservableProperty] private int defaultDurationMin = 60;

    public event Action<DateTime, int>? OnConfirm; // laat window code-behind dit doorgeven

    [RelayCommand] private void PrevMonth() { var d = new DateTime(Year, Month, 1).AddMonths(-1); Year = d.Year; Month = d.Month; _ = LoadAsync(); }
    [RelayCommand] private void NextMonth() { var d = new DateTime(Year, Month, 1).AddMonths(+1); Year = d.Year; Month = d.Month; _ = LoadAsync(); }
    [RelayCommand] private void Today() { var t = DateTime.Today; Year = t.Year; Month = t.Month; SelectedDate = t; _ = LoadAsync(); }
    // in PlanningCalendarViewModel
    [RelayCommand]
    private async Task SelectDateAsync(DateTime date)
    {
        SelectedDate = date;
        await LoadAgendaAsync(date);
    }

    [RelayCommand]
    private void ConfirmSelection()
    {
        // 09:00 default (kan je vrij kiezen)
        OnConfirm?.Invoke(new DateTime(SelectedDate.Year, SelectedDate.Month, SelectedDate.Day, 9, 0, 0, DateTimeKind.Local),
                          DefaultDurationMin);
    }

    public async Task LoadAsync()
    {
        MonthTitle = new DateTime(Year, Month, 1).ToString("MMMM yyyy", CultureInfo.CurrentCulture);

        // haal werk/taak-blokken van de maand op
        List<(DateTime from, int min)> perDay = new();
        if (_factory != null)
        {
            using var db = _factory.CreateDbContext();
            var first = new DateTime(Year, Month, 1);
            var last = first.AddMonths(1);

            var q = await db.WerkTaken.AsNoTracking()
                .Where(t => t.GeplandVan >= first && t.GeplandVan < last)
                .Select(t => new { t.GeplandVan, t.DuurMinuten, t.WerkBonId })
                .ToListAsync();

            perDay = q.GroupBy(x => x.GeplandVan.Date)
                      .Select(g => (g.Key, g.Sum(x => x.DuurMinuten)))
                      .ToList();
        }

        // bouw maandraster
        MonthDays.Clear();
        var firstDay = new DateTime(Year, Month, 1);
        var start = StartOfWeek(firstDay);               // maandag
        var end = start.AddDays(6 * 7);                  // 6 rijen

        var capPerDay = 8 * 60; // 8u/dag
        for (var d = start; d < end; d = d.AddDays(1))
        {
            var used = perDay.FirstOrDefault(x => x.from == d).min;
            var util = Math.Clamp((double)used / capPerDay, 0, 1);

            MonthDays.Add(new DayTile
            {
                Date = d,
                InMonth = d.Month == Month,
                DayNumber = d.Day.ToString(),
                BusyLabel = used == 0 ? " " : $"{used / 60}u {used % 60}m",
                Busy = util,
                BusyColor = util switch
                {
                    <= 0.5 => Brushes.LimeGreen,
                    <= 0.75 => Brushes.Goldenrod,
                    <= 0.9 => Brushes.OrangeRed,
                    _ => Brushes.Red
                },
                Background = d.Date == DateTime.Today ? new SolidColorBrush(Color.FromRgb(35, 45, 70)) :
                              d.Month == Month ? Brushes.Transparent : new SolidColorBrush(Color.FromRgb(30, 30, 30)),
                Border = d == SelectedDate.Date ? Brushes.DeepSkyBlue : new SolidColorBrush(Color.FromRgb(60, 60, 60)),
                TopItems = new List<string>() // wil je top items? laad korte labels hier
            });
        }

        // week-samenvattingen links
        WeekSummaries.Clear();
        var weekStart = start;
        while (weekStart < end)
        {
            var weekEnd = weekStart.AddDays(7);
            var sum = MonthDays.Where(x => x.Date >= weekStart && x.Date < weekEnd).Sum(x => (int)(x.Busy * capPerDay));
            var util = Math.Clamp((double)sum / (capPerDay * 5), 0, 1); // 5 werkdagen
            WeekSummaries.Add(new WeekSummary
            {
                Title = $"wk {ISOWeek.GetWeekOfYear(weekStart)}",
                Range = $"{weekStart:dd/MM}–{weekEnd.AddDays(-1):dd/MM}",
                TotalLabel = $"{sum / 60} u {sum % 60} m",
                Color = util switch
                {
                    <= 0.5 => Brushes.LimeGreen,
                    <= 0.75 => Brushes.Goldenrod,
                    <= 0.9 => Brushes.OrangeRed,
                    _ => Brushes.Red
                }
            });
            weekStart = weekEnd;
        }

        await LoadAgendaAsync(SelectedDate);
    }

    public async Task LoadAgendaAsync(DateTime date)
    {
        SelectedDate = date;
        AgendaHeader = date.ToString("dddd dd MMMM", CultureInfo.CurrentCulture);
        AgendaItems.Clear();

        if (_factory == null) return;
        using var db = _factory.CreateDbContext();

        var start = StartOfDay(date);
        var end = start.AddDays(1);

        var q = await db.WerkTaken.AsNoTracking()
            .Where(t => t.GeplandVan >= start && t.GeplandVan < end)
            .OrderBy(t => t.GeplandVan)
            .Select(t => new AgendaRow
            {
                From = t.GeplandVan.TimeOfDay,
                To = t.GeplandTot.TimeOfDay,
                DurLabel = $"{t.DuurMinuten / 60}u {t.DuurMinuten % 60}m",
                WerkBonId = t.WerkBonId,
                Title = "Werktaak"
            })
            .ToListAsync();

        foreach (var r in q) AgendaItems.Add(r);
    }




    // aanroepen via XAML EventSetter
    public void SelectDateFromTile(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (sender is Border b && b.DataContext is DayTile d) _ = LoadAgendaAsync(d.Date);
    }
    public void DoubleTapDateFromTile(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (sender is Border b && b.DataContext is DayTile d)
            OnConfirm?.Invoke(StartOfDay(d.Date).AddHours(9), DefaultDurationMin);
    }

    private static DateTime StartOfWeek(DateTime dt)
    {
        int diff = (7 + (int)dt.DayOfWeek - (int)DayOfWeek.Monday) % 7;
        return dt.AddDays(-diff).Date;
    }
    private static DateTime StartOfDay(DateTime d) => new(d.Year, d.Month, d.Day, 0, 0, 0, DateTimeKind.Local);
}

public class DayTile : ObservableObject
{
    public DateTime Date { get; set; }
    public string DayNumber { get; set; } = "";
    public string BusyLabel { get; set; } = "";
    public double Busy { get; set; }
    public IBrush BusyColor { get; set; } = Brushes.LimeGreen;
    public double BusyBarWidth => Math.Clamp(Busy, 0, 1) * 120; // 120px bar
    public bool InMonth { get; set; }
    public IBrush Background { get; set; } = Brushes.Transparent;
    public IBrush Border { get; set; } = Brushes.Gray;
    public List<string> TopItems { get; set; } = new();
}

public class WeekSummary
{
    public string Title { get; set; } = "";
    public string Range { get; set; } = "";
    public string TotalLabel { get; set; } = "";
    public IBrush Color { get; set; } = Brushes.Gray;
}

public class AgendaRow
{
    public TimeSpan From { get; set; }
    public TimeSpan To { get; set; }
    public string DurLabel { get; set; } = "";
    public int WerkBonId { get; set; }
    public string Title { get; set; } = "";
}
