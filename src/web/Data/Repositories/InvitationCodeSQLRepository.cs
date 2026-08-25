//-----------------------------------------------------------------------
// <copyright file="InvitationCodeSQLRepository.cs" company="Luppes Consulting, Inc.">
// Copyright 2026, Luppes Consulting, Inc. All rights reserved.
// </copyright>
// <summary>
// Invitation Code SQL Repository
// </summary>
//-----------------------------------------------------------------------
using DadABase.Data.Models;

namespace DadABase.Data.Repositories;

/// <summary>
/// SQL Server implementation of <see cref="IInvitationCodeRepository"/> using EF Core.
/// </summary>
public class InvitationCodeSQLRepository(DadABaseDbContext context) : IInvitationCodeRepository
{
    private readonly DadABaseDbContext _context = context;

    /// <inheritdoc/>
    public async Task<InvitationCode> CreateCodeAsync(int circleId, int createdByUserId, DateTime? expiresUtc = null)
    {
        var isMember = await _context.CircleMemberships!
            .AnyAsync(m => m.CircleId == circleId && m.UserId == createdByUserId && m.LeftUtc == null);
        if (!isMember)
        {
            throw new InvalidOperationException("Only circle members may generate invitation codes.");
        }

        var code = new InvitationCode
        {
            CircleId = circleId,
            CreatedByUserId = createdByUserId,
            Code = Guid.NewGuid().ToString("N"),
            CreatedUtc = DateTime.UtcNow,
            ExpiresUtc = expiresUtc
        };

        _context.InvitationCodes!.Add(code);
        await _context.SaveChangesAsync();
        return code;
    }

    /// <inheritdoc/>
    public async Task<IList<InvitationCode>> GetCodesForCircleAsync(int circleId, int requestingUserId)
    {
        var isMember = await _context.CircleMemberships!
            .AnyAsync(m => m.CircleId == circleId && m.UserId == requestingUserId && m.LeftUtc == null);
        if (!isMember)
        {
            return [];
        }

        return await _context.InvitationCodes!
            .Where(ic => ic.CircleId == circleId)
            .Include(ic => ic.CreatedByUser)
            .Include(ic => ic.RedeemedByUser)
            .OrderByDescending(ic => ic.CreatedUtc)
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<InvitationCode?> FindValidCodeAsync(string code)
    {
        return await _context.InvitationCodes!
            .Include(ic => ic.Circle)
            .FirstOrDefaultAsync(ic =>
                ic.Code == code &&
                ic.RedeemedByUserId == null &&
                ic.RevokedUtc == null &&
                (ic.ExpiresUtc == null || ic.ExpiresUtc > DateTime.UtcNow));
    }

    /// <inheritdoc/>
    public async Task<CircleMembership> RedeemCodeAsync(string code, int newUserId)
    {
        var invitation = await FindValidCodeAsync(code)
            ?? throw new InvalidOperationException("Invitation code is invalid, already used, or expired.");

        var isAlreadyMember = await _context.CircleMemberships!
            .AnyAsync(m => m.CircleId == invitation.CircleId && m.UserId == newUserId && m.LeftUtc == null);
        if (isAlreadyMember)
        {
            throw new InvalidOperationException("The user is already an active member of this circle.");
        }

        invitation.RedeemedByUserId = newUserId;
        invitation.RedeemedUtc = DateTime.UtcNow;

        var membership = new CircleMembership
        {
            CircleId = invitation.CircleId,
            UserId = newUserId,
            Role = "Member",
            JoinedUtc = DateTime.UtcNow
        };
        _context.CircleMemberships!.Add(membership);
        await _context.SaveChangesAsync();
        return membership;
    }

    /// <inheritdoc/>
    public async Task RevokeCodeAsync(int invitationCodeId, int requestingUserId)
    {
        var invitation = await _context.InvitationCodes!.FindAsync(invitationCodeId)
            ?? throw new InvalidOperationException("Invitation code not found.");

        var isMember = await _context.CircleMemberships!
            .AnyAsync(m => m.CircleId == invitation.CircleId && m.UserId == requestingUserId && m.LeftUtc == null);
        if (!isMember)
        {
            throw new InvalidOperationException("Only circle members may revoke invitation codes.");
        }

        if (invitation.RedeemedByUserId != null)
        {
            throw new InvalidOperationException("Cannot revoke an already-redeemed invitation code.");
        }

        invitation.RevokedUtc = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }
}
