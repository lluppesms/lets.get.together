namespace DadABase.Tests;

using DadABase.Data.Models;
using DadABase.Data.Services;

[ExcludeFromCodeCoverage]
public class RecurrenceService_Tests
{
    private readonly RecurrenceService _service = new();

    [Fact]
    public void ExpandEvent_OneOffEventInWindow_ReturnsSingleOccurrence()
    {
        var ev = new Event
        {
            EventId = 1,
            CircleId = 1,
            Title = "One-off Party",
            StartsUtc = new DateTime(2026, 9, 15, 18, 0, 0, DateTimeKind.Utc),
            EndsUtc = new DateTime(2026, 9, 15, 21, 0, 0, DateTimeKind.Utc),
            IsRecurring = false
        };

        var startWindow = new DateTime(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc);
        var endWindow = new DateTime(2026, 9, 30, 23, 59, 59, DateTimeKind.Utc);

        var occurrences = _service.ExpandEvent(ev, startWindow, endWindow).ToList();

        Assert.Single(occurrences);
        Assert.Equal("One-off Party", occurrences[0].Title);
        Assert.Equal(ev.StartsUtc, occurrences[0].StartsUtc);
        Assert.Equal(ev.EndsUtc, occurrences[0].EndsUtc);
        Assert.False(occurrences[0].IsOccurrence);
    }

    [Fact]
    public void ExpandEvent_OneOffEventOutsideWindow_ReturnsEmpty()
    {
        var ev = new Event
        {
            EventId = 1,
            CircleId = 1,
            Title = "Past Event",
            StartsUtc = new DateTime(2026, 8, 15, 18, 0, 0, DateTimeKind.Utc),
            IsRecurring = false
        };

        var startWindow = new DateTime(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc);
        var endWindow = new DateTime(2026, 9, 30, 23, 59, 59, DateTimeKind.Utc);

        var occurrences = _service.ExpandEvent(ev, startWindow, endWindow).ToList();

        Assert.Empty(occurrences);
    }

    [Fact]
    public void ExpandEvent_CancelledEvent_ReturnsEmpty()
    {
        var ev = new Event
        {
            EventId = 1,
            CircleId = 1,
            Title = "Cancelled Meeting",
            StartsUtc = new DateTime(2026, 9, 15, 18, 0, 0, DateTimeKind.Utc),
            CancelledUtc = DateTime.UtcNow,
            IsRecurring = false
        };

        var startWindow = new DateTime(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc);
        var endWindow = new DateTime(2026, 9, 30, 23, 59, 59, DateTimeKind.Utc);

        var occurrences = _service.ExpandEvent(ev, startWindow, endWindow).ToList();

        Assert.Empty(occurrences);
    }

    [Fact]
    public void ExpandEvent_WeeklyRecurrence_ExpandsEveryWeek()
    {
        // Tuesday Sept 1, 2026
        var ev = new Event
        {
            EventId = 10,
            CircleId = 1,
            Title = "Weekly Pickleball",
            StartsUtc = new DateTime(2026, 9, 1, 10, 0, 0, DateTimeKind.Utc),
            EndsUtc = new DateTime(2026, 9, 1, 11, 30, 0, DateTimeKind.Utc),
            IsRecurring = true,
            RecurrenceRule = "Weekly;DayOfWeek=Tuesday"
        };

        var startWindow = new DateTime(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc);
        var endWindow = new DateTime(2026, 9, 30, 23, 59, 59, DateTimeKind.Utc);

        var occurrences = _service.ExpandEvent(ev, startWindow, endWindow).ToList();

        // Sept 1, 8, 15, 22, 29 (5 Tuesdays in Sept 2026)
        Assert.Equal(5, occurrences.Count);
        Assert.All(occurrences, o => Assert.Equal(DayOfWeek.Tuesday, o.StartsUtc.DayOfWeek));
        Assert.Equal(new DateTime(2026, 9, 1, 10, 0, 0, DateTimeKind.Utc), occurrences[0].StartsUtc);
        Assert.Equal(new DateTime(2026, 9, 29, 10, 0, 0, DateTimeKind.Utc), occurrences[4].StartsUtc);
        Assert.All(occurrences, o => Assert.True(o.IsOccurrence));
    }

    [Fact]
    public void ExpandEvent_BiweeklyRecurrence_ExpandsEveryTwoWeeks()
    {
        // Wednesday Sept 2, 2026
        var ev = new Event
        {
            EventId = 11,
            CircleId = 1,
            Title = "Biweekly Sync",
            StartsUtc = new DateTime(2026, 9, 2, 14, 0, 0, DateTimeKind.Utc),
            IsRecurring = true,
            RecurrenceRule = "Biweekly;DayOfWeek=Wednesday"
        };

        var startWindow = new DateTime(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc);
        var endWindow = new DateTime(2026, 10, 31, 23, 59, 59, DateTimeKind.Utc);

        var occurrences = _service.ExpandEvent(ev, startWindow, endWindow).ToList();

        // Sept 2, Sept 16, Sept 30, Oct 14, Oct 28
        Assert.Equal(5, occurrences.Count);
        Assert.Equal(new DateTime(2026, 9, 2, 14, 0, 0, DateTimeKind.Utc), occurrences[0].StartsUtc);
        Assert.Equal(new DateTime(2026, 9, 16, 14, 0, 0, DateTimeKind.Utc), occurrences[1].StartsUtc);
        Assert.Equal(new DateTime(2026, 9, 30, 14, 0, 0, DateTimeKind.Utc), occurrences[2].StartsUtc);
        Assert.Equal(new DateTime(2026, 10, 14, 14, 0, 0, DateTimeKind.Utc), occurrences[3].StartsUtc);
        Assert.Equal(new DateTime(2026, 10, 28, 14, 0, 0, DateTimeKind.Utc), occurrences[4].StartsUtc);
    }

    [Fact]
    public void ExpandEvent_MonthlyRecurrence_ExpandsOnDayOfMonth()
    {
        // 15th of the month starting Sept 15, 2026
        var ev = new Event
        {
            EventId = 12,
            CircleId = 1,
            Title = "Monthly Board Game Night",
            StartsUtc = new DateTime(2026, 9, 15, 19, 0, 0, DateTimeKind.Utc),
            IsRecurring = true,
            RecurrenceRule = "Monthly;DayOfMonth=15"
        };

        var startWindow = new DateTime(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc);
        var endWindow = new DateTime(2026, 12, 31, 23, 59, 59, DateTimeKind.Utc);

        var occurrences = _service.ExpandEvent(ev, startWindow, endWindow).ToList();

        // Sept 15, Oct 15, Nov 15, Dec 15 (4 occurrences)
        Assert.Equal(4, occurrences.Count);
        Assert.Equal(new DateTime(2026, 9, 15, 19, 0, 0, DateTimeKind.Utc), occurrences[0].StartsUtc);
        Assert.Equal(new DateTime(2026, 10, 15, 19, 0, 0, DateTimeKind.Utc), occurrences[1].StartsUtc);
        Assert.Equal(new DateTime(2026, 11, 15, 19, 0, 0, DateTimeKind.Utc), occurrences[2].StartsUtc);
        Assert.Equal(new DateTime(2026, 12, 15, 19, 0, 0, DateTimeKind.Utc), occurrences[3].StartsUtc);
    }

    [Fact]
    public void ExpandEvents_MultipleEvents_ReturnsOrderedOccurrences()
    {
        var ev1 = new Event
        {
            EventId = 1,
            Title = "Event 1",
            StartsUtc = new DateTime(2026, 9, 20, 10, 0, 0, DateTimeKind.Utc),
            IsRecurring = false
        };

        var ev2 = new Event
        {
            EventId = 2,
            Title = "Event 2",
            StartsUtc = new DateTime(2026, 9, 10, 10, 0, 0, DateTimeKind.Utc),
            IsRecurring = false
        };

        var startWindow = new DateTime(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc);
        var endWindow = new DateTime(2026, 9, 30, 23, 59, 59, DateTimeKind.Utc);

        var occurrences = _service.ExpandEvents([ev1, ev2], startWindow, endWindow).ToList();

        Assert.Equal(2, occurrences.Count);
        Assert.Equal("Event 2", occurrences[0].Title);
        Assert.Equal("Event 1", occurrences[1].Title);
    }

    [Fact]
    public void ExpandEvent_InvalidWindow_ThrowsArgumentException()
    {
        var ev = new Event { Title = "Test", StartsUtc = DateTime.UtcNow };
        var start = DateTime.UtcNow;
        var end = start.AddDays(-1);

        Assert.Throws<ArgumentException>(() => _service.ExpandEvent(ev, start, end).ToList());
    }

    [Fact]
    public void ExpandEvent_NullEvent_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _service.ExpandEvent(null!, DateTime.UtcNow, DateTime.UtcNow.AddDays(1)).ToList());
    }
}
