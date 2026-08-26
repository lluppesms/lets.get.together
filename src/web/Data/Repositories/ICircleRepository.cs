//-----------------------------------------------------------------------
// <copyright file="ICircleRepository.cs" company="Luppes Consulting, Inc.">
// Copyright 2026, Luppes Consulting, Inc. All rights reserved.
// </copyright>
// <summary>
// Circle Repository Interface
// </summary>
//-----------------------------------------------------------------------
using GetTogether.Data.Models;

namespace GetTogether.Data.Repositories;

/// <summary>
/// Represents the repository interface for accessing and managing circle data.
/// </summary>
public interface ICircleRepository
{
    /// <summary>
    /// Returns all circles the specified user belongs to.
    /// </summary>
    Task<IList<Circle>> GetCirclesForUserAsync(int userId);

    /// <summary>
    /// Returns the circle with the specified identifier, or null if not found.
    /// The user must be a member of the circle.
    /// </summary>
    Task<Circle?> GetCircleAsync(int circleId, int requestingUserId);

    /// <summary>
    /// Returns all active members of the specified circle.
    /// </summary>
    Task<IList<CircleMembership>> GetMembersAsync(int circleId, int requestingUserId);

    /// <summary>
    /// Creates a new circle and adds the creator as its first member.
    /// Returns the created circle.
    /// </summary>
    Task<Circle> CreateCircleAsync(Circle circle, int creatorUserId);

    /// <summary>
    /// Adds a user to the circle, or reactivates their previous membership.
    /// The requesting user must be an active member.
    /// </summary>
    Task<CircleMembership> AddMemberAsync(int circleId, int userId, int requestingUserId);

    /// <summary>
    /// Updates circle name/description. Only members may update.
    /// </summary>
    Task UpdateCircleAsync(Circle circle, int requestingUserId);

    /// <summary>
    /// Removes the specified user from the circle (sets LeftUtc).
    /// </summary>
    Task RemoveMemberAsync(int circleId, int userId, int requestingUserId);
}
