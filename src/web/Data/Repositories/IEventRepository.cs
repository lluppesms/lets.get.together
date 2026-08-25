//-----------------------------------------------------------------------
// <copyright file="IEventRepository.cs" company="Luppes Consulting, Inc.">
// Copyright 2026, Luppes Consulting, Inc. All rights reserved.
// </copyright>
// <summary>
// Event Repository Interface
// </summary>
//-----------------------------------------------------------------------
using DadABase.Data.Models;

namespace DadABase.Data.Repositories;

/// <summary>
/// Represents the repository interface for accessing and managing event data.
/// </summary>
public interface IEventRepository
{
    /// <summary>
    /// Returns upcoming events across all circles the user belongs to, ordered by start time.
    /// </summary>
    Task<IList<Event>> GetUpcomingEventsForUserAsync(int userId);

    /// <summary>
    /// Returns all events for a specific circle, ordered by start time.
    /// </summary>
    Task<IList<Event>> GetEventsForCircleAsync(int circleId, int requestingUserId);

    /// <summary>
    /// Returns a single event with its RSVP roster, or null if not found or user is not a member.
    /// </summary>
    Task<Event?> GetEventAsync(int eventId, int requestingUserId);

    /// <summary>
    /// Creates a new event in the specified circle and returns the persisted entity.
    /// </summary>
    Task<Event> CreateEventAsync(Event newEvent, int creatorUserId);

    /// <summary>
    /// Updates an existing event. Any circle member may update.
    /// </summary>
    Task UpdateEventAsync(Event updatedEvent, int requestingUserId);

    /// <summary>
    /// Marks an event as cancelled (sets CancelledUtc).
    /// </summary>
    Task CancelEventAsync(int eventId, int requestingUserId);
}
