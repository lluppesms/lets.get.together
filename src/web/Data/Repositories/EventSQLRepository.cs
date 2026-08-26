//-----------------------------------------------------------------------
// <copyright file="EventSQLRepository.cs" company="Luppes Consulting, Inc.">
// Copyright 2026, Luppes Consulting, Inc. All rights reserved.
// </copyright>
// <summary>
// Event SQL Repository
// </summary>
//-----------------------------------------------------------------------
using DadABase.Data.Models;

namespace DadABase.Data.Repositories;

/// <summary>
/// SQL Server implementation of <see cref="IEventRepository"/> using EF Core.
/// </summary>
public class EventSQLRepository(DadABaseDbContext context) : IEventRepository
{
    private readonly DadABaseDbContext _context = context;

    /// <inheritdoc/>
    public async Task<IList<Event>> GetUpcomingEventsForUserAsync(int userId)
    {
        var circleIds = await _context.CircleMemberships!
            .Where(m => m.UserId == userId && m.LeftUtc == null)
            .Select(m => m.CircleId)
            .ToListAsync();

        return await _context.Events!
            .Where(e => circleIds.Contains(e.CircleId) && e.CancelledUtc == null && e.StartsUtc >= DateTime.UtcNow)
            .Include(e => e.Circle)
            .Include(e => e.Rsvps)
            .OrderBy(e => e.StartsUtc)
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<IList<Event>> GetEventsByCircleAsync(int circleId, int requestingUserId)
    {
        var isMember = await _context.CircleMemberships!
            .AnyAsync(m => m.CircleId == circleId && m.UserId == requestingUserId && m.LeftUtc == null);
        if (!isMember)
        {
            return [];
        }

        return await _context.Events!
            .Where(e => e.CircleId == circleId && e.CancelledUtc == null)
            .Include(e => e.Rsvps)
            .OrderBy(e => e.StartsUtc)
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<Event?> GetByIdAsync(int eventId, int requestingUserId)
    {
        var ev = await _context.Events!
            .Include(e => e.Circle)
            .Include(e => e.Rsvps).ThenInclude(r => r.User)
            .FirstOrDefaultAsync(e => e.EventId == eventId);

        if (ev == null)
        {
            return null;
        }

        var isMember = await _context.CircleMemberships!
            .AnyAsync(m => m.CircleId == ev.CircleId && m.UserId == requestingUserId && m.LeftUtc == null);

        return isMember ? ev : null;
    }

    /// <inheritdoc/>
    public async Task<Event> CreateAsync(Event newEvent, int creatorUserId)
    {
        var isMember = await _context.CircleMemberships!
            .AnyAsync(m => m.CircleId == newEvent.CircleId && m.UserId == creatorUserId && m.LeftUtc == null);
        if (!isMember)
        {
            throw new InvalidOperationException("Only circle members may create events.");
        }

        newEvent.CreatedByUserId = creatorUserId;
        newEvent.CreatedUtc = DateTime.UtcNow;
        _context.Events!.Add(newEvent);
        await _context.SaveChangesAsync();
        return newEvent;
    }

    /// <inheritdoc/>
    public async Task UpdateAsync(Event updatedEvent, int requestingUserId)
    {
        var isMember = await _context.CircleMemberships!
            .AnyAsync(m => m.CircleId == updatedEvent.CircleId && m.UserId == requestingUserId && m.LeftUtc == null);
        if (!isMember)
        {
            throw new InvalidOperationException("Only circle members may update events.");
        }

        _context.Events!.Update(updatedEvent);
        await _context.SaveChangesAsync();
    }

    /// <inheritdoc/>
    public async Task DeleteAsync(int eventId, int requestingUserId)
    {
        var ev = await _context.Events!.FirstOrDefaultAsync(e => e.EventId == eventId)
            ?? throw new InvalidOperationException("Event not found.");

        var isMember = await _context.CircleMemberships!
            .AnyAsync(m => m.CircleId == ev.CircleId && m.UserId == requestingUserId && m.LeftUtc == null);
        if (!isMember)
        {
            throw new InvalidOperationException("Only circle members may delete events.");
        }

        _context.Events!.Remove(ev);
        await _context.SaveChangesAsync();
    }

    /// <inheritdoc/>
    public Task<IList<Event>> GetEventsForCircleAsync(int circleId, int requestingUserId)
        => GetEventsByCircleAsync(circleId, requestingUserId);

    /// <inheritdoc/>
    public Task<Event?> GetEventAsync(int eventId, int requestingUserId)
        => GetByIdAsync(eventId, requestingUserId);

    /// <inheritdoc/>
    public Task<Event> CreateEventAsync(Event newEvent, int creatorUserId)
        => CreateAsync(newEvent, creatorUserId);

    /// <inheritdoc/>
    public Task UpdateEventAsync(Event updatedEvent, int requestingUserId)
        => UpdateAsync(updatedEvent, requestingUserId);

    /// <inheritdoc/>
    public async Task CancelEventAsync(int eventId, int requestingUserId)
    {
        var ev = await _context.Events!.FindAsync(eventId)
            ?? throw new InvalidOperationException("Event not found.");

        var isMember = await _context.CircleMemberships!
            .AnyAsync(m => m.CircleId == ev.CircleId && m.UserId == requestingUserId && m.LeftUtc == null);
        if (!isMember)
        {
            throw new InvalidOperationException("Only circle members may cancel events.");
        }

        ev.CancelledUtc = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }
}
