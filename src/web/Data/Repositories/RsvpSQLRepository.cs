//-----------------------------------------------------------------------
// <copyright file="RsvpSQLRepository.cs" company="Luppes Consulting, Inc.">
// Copyright 2026, Luppes Consulting, Inc. All rights reserved.
// </copyright>
// <summary>
// RSVP SQL Repository
// </summary>
//-----------------------------------------------------------------------
using DadABase.Data.Models;

namespace DadABase.Data.Repositories;

/// <summary>
/// SQL Server implementation of <see cref="IRsvpRepository"/> using EF Core.
/// </summary>
public class RsvpSQLRepository(DadABaseDbContext context) : IRsvpRepository
{
    private static readonly HashSet<string> AllowedStatuses = ["Accept", "Decline", "Maybe"];

    private readonly DadABaseDbContext _context = context;

    /// <inheritdoc/>
    public async Task<RSVP?> GetRsvpAsync(int eventId, int userId)
    {
        return await _context.Rsvps!
            .FirstOrDefaultAsync(r => r.EventId == eventId && r.UserId == userId);
    }

    /// <inheritdoc/>
    public async Task<IList<RSVP>> GetRsvpsForEventAsync(int eventId, int requestingUserId)
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
    public async Task<RSVP> UpsertRsvpAsync(int eventId, int userId, string status, string? notes = null)
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

        var existing = await _context.Rsvps!
            .FirstOrDefaultAsync(r => r.EventId == eventId && r.UserId == userId);

        if (existing != null)
        {
            existing.Status = status;
            existing.Notes = notes;
            existing.RespondedUtc = DateTime.UtcNow;
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
            RespondedUtc = DateTime.UtcNow
        };
        _context.Rsvps!.Add(rsvp);
        await _context.SaveChangesAsync();
        return rsvp;
    }
}
