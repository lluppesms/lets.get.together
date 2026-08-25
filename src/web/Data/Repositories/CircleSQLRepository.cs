//-----------------------------------------------------------------------
// <copyright file="CircleSQLRepository.cs" company="Luppes Consulting, Inc.">
// Copyright 2026, Luppes Consulting, Inc. All rights reserved.
// </copyright>
// <summary>
// Circle SQL Repository
// </summary>
//-----------------------------------------------------------------------
using DadABase.Data.Models;

namespace DadABase.Data.Repositories;

/// <summary>
/// SQL Server implementation of <see cref="ICircleRepository"/> using EF Core.
/// </summary>
public class CircleSQLRepository(DadABaseDbContext context) : ICircleRepository
{
    private readonly DadABaseDbContext _context = context;

    /// <inheritdoc/>
    public async Task<IList<Circle>> GetCirclesForUserAsync(int userId)
    {
        return await _context.Circles!
            .Where(c => !c.IsArchived && c.Members.Any(m => m.UserId == userId && m.LeftUtc == null))
            .Include(c => c.Members)
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<Circle?> GetCircleAsync(int circleId, int requestingUserId)
    {
        var isMember = await _context.CircleMemberships!
            .AnyAsync(m => m.CircleId == circleId && m.UserId == requestingUserId && m.LeftUtc == null);
        if (!isMember)
        {
            return null;
        }

        return await _context.Circles!
            .Include(c => c.Members).ThenInclude(m => m.User)
            .Include(c => c.Events)
            .FirstOrDefaultAsync(c => c.CircleId == circleId && !c.IsArchived);
    }

    /// <inheritdoc/>
    public async Task<IList<CircleMembership>> GetMembersAsync(int circleId, int requestingUserId)
    {
        var isMember = await _context.CircleMemberships!
            .AnyAsync(m => m.CircleId == circleId && m.UserId == requestingUserId && m.LeftUtc == null);
        if (!isMember)
        {
            return [];
        }

        return await _context.CircleMemberships!
            .Where(m => m.CircleId == circleId && m.LeftUtc == null)
            .Include(m => m.User)
            .OrderBy(m => m.User!.DisplayName)
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<Circle> CreateCircleAsync(Circle circle, int creatorUserId)
    {
        circle.CreatedByUserId = creatorUserId;
        circle.CreatedUtc = DateTime.UtcNow;
        _context.Circles!.Add(circle);
        await _context.SaveChangesAsync();

        var membership = new CircleMembership
        {
            CircleId = circle.CircleId,
            UserId = creatorUserId,
            Role = "Member",
            JoinedUtc = DateTime.UtcNow
        };
        _context.CircleMemberships!.Add(membership);
        await _context.SaveChangesAsync();

        return circle;
    }

    /// <inheritdoc/>
    public async Task UpdateCircleAsync(Circle circle, int requestingUserId)
    {
        var isMember = await _context.CircleMemberships!
            .AnyAsync(m => m.CircleId == circle.CircleId && m.UserId == requestingUserId && m.LeftUtc == null);
        if (!isMember)
        {
            throw new InvalidOperationException("Only circle members may update a circle.");
        }

        _context.Circles!.Update(circle);
        await _context.SaveChangesAsync();
    }

    /// <inheritdoc/>
    public async Task RemoveMemberAsync(int circleId, int userId, int requestingUserId)
    {
        var isMember = await _context.CircleMemberships!
            .AnyAsync(m => m.CircleId == circleId && m.UserId == requestingUserId && m.LeftUtc == null);
        if (!isMember)
        {
            throw new InvalidOperationException("Only circle members may remove another member.");
        }

        var membership = await _context.CircleMemberships!
            .FirstOrDefaultAsync(m => m.CircleId == circleId && m.UserId == userId && m.LeftUtc == null)
            ?? throw new InvalidOperationException("Membership not found.");

        membership.LeftUtc = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }
}
