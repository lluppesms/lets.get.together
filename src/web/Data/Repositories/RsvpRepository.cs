//-----------------------------------------------------------------------
// <copyright file="RsvpSQLRepository.cs" company="Luppes Consulting, Inc.">
// Copyright 2026, Luppes Consulting, Inc. All rights reserved.
// </copyright>
// <summary>
// RSVP SQL Repository
// </summary>
//-----------------------------------------------------------------------
using GetTogether.Data.Models;

namespace GetTogether.Data.Repositories;

/// <summary>
/// SQL Server implementation of <see cref="IRsvpRepository"/> using EF Core.
/// </summary>
public class RsvpRepository(GetTogetherDbContext context) : IRsvpRepository
{
    private static readonly HashSet<string> AllowedStatuses = ["Accept", "Decline", "Maybe"];

    private readonly GetTogetherDbContext _context = context;

    /// <inheritdoc/>
    public async Task<RSVP?> GetRsvpAsync(int eventId, int userId)
    {
        var ev = await _context.Events!.FindAsync(eventId);
        if (ev == null)
        {
            return null;
        }

        var isMember = await _context.CircleMemberships!
            .AnyAsync(m => m.CircleId == ev.CircleId && m.UserId == userId && m.LeftUtc == null);
        if (!isMember)
        {
            return null;
        }

        return await _context.Rsvps!
            .FirstOrDefaultAsync(r => r.EventId == eventId && r.UserId == userId);
    }

    /// <inheritdoc/>
    public async Task<IList<RSVP>> GetRsvpsForEventAsync(int eventId, int requestingUserId)
    {
        return await GetRsvpsByEventAsync(eventId, requestingUserId);
    }

    /// <inheritdoc/>
    public async Task<IList<RSVP>> GetRsvpsByEventAsync(int eventId, int requestingUserId)
    {
        var ev = await _context.Events!.FindAsync(eventId);
        if (ev == null)
        {
            return [];
        }

        var isMember = await _context.CircleMemberships!
            .AnyAsync(m => m.CircleId == ev.CircleId && m.UserId == requestingUserId && m.LeftUtc == null);
        if (!isMember)
        {
            return [];
        }

        return await _context.Rsvps!
            .Where(r => r.EventId == eventId)
            .Include(r => r.User)
            .OrderBy(r => r.User!.DisplayName)
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<IList<RSVP>> GetRsvpsByOccurrenceAsync(int eventId, DateTime occurrenceDate, int requestingUserId)
    {
        var ev = await _context.Events!.FindAsync(eventId);
        if (ev == null)
        {
            return [];
        }

        var isMember = await _context.CircleMemberships!
            .AnyAsync(m => m.CircleId == ev.CircleId && m.UserId == requestingUserId && m.LeftUtc == null);
        if (!isMember)
        {
            return [];
        }

        var targetDate = occurrenceDate.Date;
        return await _context.Rsvps!
            .Where(r => r.EventId == eventId && (r.OccurrenceDate == null || (r.OccurrenceDate.HasValue && r.OccurrenceDate.Value.Date == targetDate)))
            .Include(r => r.User)
            .OrderBy(r => r.User!.DisplayName)
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<RSVP> UpsertRsvpAsync(int eventId, int userId, string status, string? notes = null)
    {
        return await UpsertRsvpAsync(eventId, userId, status, notes, null);
    }

    /// <inheritdoc/>
    public async Task<RSVP> UpsertRsvpAsync(int eventId, int userId, string status, string? notes, DateTime? occurrenceDate)
    {
        if (!AllowedStatuses.Contains(status))
        {
            throw new ArgumentException($"Invalid RSVP status '{status}'. Must be Accept, Decline, or Maybe.");
        }

        var ev = await _context.Events!.FindAsync(eventId)
            ?? throw new InvalidOperationException("Event not found.");

        var isMember = await _context.CircleMemberships!
            .AnyAsync(m => m.CircleId == ev.CircleId && m.UserId == userId && m.LeftUtc == null);
        if (!isMember)
        {
            throw new InvalidOperationException("Only circle members may RSVP to events.");
        }

        RSVP? existing = null;
        if (occurrenceDate.HasValue)
        {
            var targetDate = occurrenceDate.Value.Date;
            existing = await _context.Rsvps!
                .FirstOrDefaultAsync(r => r.EventId == eventId && r.UserId == userId && r.OccurrenceDate.HasValue && r.OccurrenceDate.Value.Date == targetDate);
        }

        existing ??= await _context.Rsvps!
            .FirstOrDefaultAsync(r => r.EventId == eventId && r.UserId == userId);

        if (existing != null)
        {
            existing.Status = status;
            existing.Notes = notes;
            existing.RespondedUtc = DateTime.UtcNow;
            if (occurrenceDate.HasValue)
            {
                existing.OccurrenceDate = occurrenceDate.Value.Date;
            }

            await _context.SaveChangesAsync();
            return existing;
        }

        var rsvp = new RSVP
        {
            EventId = eventId,
            CircleId = ev.CircleId,
            UserId = userId,
            Status = status,
            Notes = notes,
            OccurrenceDate = occurrenceDate?.Date,
            RespondedUtc = DateTime.UtcNow
        };
        _context.Rsvps!.Add(rsvp);
        await _context.SaveChangesAsync();
        return rsvp;
    }

    /// <inheritdoc/>
    public async Task<IList<User>> GetUnansweredMembersAsync(int eventId, int requestingUserId)
    {
        return await GetUnansweredMembersAsync(eventId, requestingUserId, null);
    }

    /// <inheritdoc/>
    public async Task<IList<User>> GetUnansweredMembersAsync(int eventId, int requestingUserId, DateTime? occurrenceDate)
    {
        var ev = await _context.Events!.FindAsync(eventId);
        if (ev == null)
        {
            return [];
        }

        var isMember = await _context.CircleMemberships!
            .AnyAsync(m => m.CircleId == ev.CircleId && m.UserId == requestingUserId && m.LeftUtc == null);
        if (!isMember)
        {
            return [];
        }

        var rsvpQuery = _context.Rsvps!.Where(r => r.EventId == eventId);
        if (occurrenceDate.HasValue)
        {
            var targetDate = occurrenceDate.Value.Date;
            rsvpQuery = rsvpQuery.Where(r => r.OccurrenceDate == null || (r.OccurrenceDate.HasValue && r.OccurrenceDate.Value.Date == targetDate));
        }

        var respondedUserIds = await rsvpQuery
            .Select(r => r.UserId)
            .Distinct()
            .ToListAsync();

        return await _context.CircleMemberships!
            .Where(m => m.CircleId == ev.CircleId && m.LeftUtc == null && !respondedUserIds.Contains(m.UserId))
            .Include(m => m.User)
            .Select(m => m.User!)
            .OrderBy(u => u.DisplayName)
            .ToListAsync();
    }
}
