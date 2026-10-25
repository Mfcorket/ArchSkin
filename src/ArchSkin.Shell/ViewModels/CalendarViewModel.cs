using System.Collections.ObjectModel;
using ArchSkin.Shell.Mvvm;

namespace ArchSkin.Shell.ViewGroup;

public sealed class CalendarViewModel : ObservableObject
{
    public ObservableCollection<CalendarDay> Days { get; } = new();

    private DateTime _currentMonth;

    public DateTime CurrentMonth
    {
        get => _currentMonth;
        private set => SetProperty(ref _currentMonth, value);
    }

    public string MonthLabel => _currentMonth.ToString("MMMM yyyy");

    public CalendarViewModel()
    {
        CurrentMonth = DateTime.Now;
        BuildMonth();
    }

    public void PreviousMonth()
    {
        CurrentMonth = CurrentMonth.AddMonths(-1);
        BuildMonth();
    }

    public void NextMonth()
    {
        CurrentMonth = CurrentMonth.AddMonths(1);
        BuildMonth();
    }

    public void GoToToday()
    {
        CurrentMonth = DateTime.Now;
        BuildMonth();
    }

    private void BuildMonth()
    {
        Days.Clear();

        var firstDay = new DateTime(CurrentMonth.Year, CurrentMonth.Month, 1);
        var daysInMonth = DateTime.DaysInMonth(CurrentMonth.Year, CurrentMonth.Month);
        var startOffset = (int)firstDay.DayOfWeek;

        for (var i = 0; i < startOffset; i++)
        {
            Days.Add(new CalendarDay(DateTime.MinValue, false));
        }

        var today = DateTime.Now.Date;
        for (var day = 1; day <= daysInMonth; day++)
        {
            var date = new DateTime(CurrentMonth.Year, CurrentMonth.Month, day);
            Days.Add(new CalendarDay(date, date == today));
        }
    }
}

public sealed record CalendarDay(DateTime Date, bool IsToday)
{
    public bool IsEmpty => Date == DateTime.MinValue;
    public int DayNumber => IsEmpty ? 0 : Date.Day;
}
