namespace GetTogether.Data.Models;

/// <summary>
/// Determines how RSVPs apply to a recurring event.
/// </summary>
public enum RsvpMode
{
    /// <summary>
    /// An RSVP applies to one occurrence of the event.
    /// </summary>
    PerOccurrence,

    /// <summary>
    /// An RSVP applies to the entire event series.
    /// </summary>
    Series
}