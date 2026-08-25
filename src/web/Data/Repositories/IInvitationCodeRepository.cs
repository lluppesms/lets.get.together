//-----------------------------------------------------------------------
// <copyright file="IInvitationCodeRepository.cs" company="Luppes Consulting, Inc.">
// Copyright 2026, Luppes Consulting, Inc. All rights reserved.
// </copyright>
// <summary>
// Invitation Code Repository Interface
// </summary>
//-----------------------------------------------------------------------
using DadABase.Data.Models;

namespace DadABase.Data.Repositories;

/// <summary>
/// Represents the repository interface for managing invitation codes.
/// </summary>
public interface IInvitationCodeRepository
{
    /// <summary>
    /// Creates a single-use invitation code for the specified circle.
    /// The code is generated internally and returned on the result entity.
    /// </summary>
    Task<InvitationCode> CreateCodeAsync(int circleId, int createdByUserId, DateTime? expiresUtc = null);

    /// <summary>
    /// Returns all invitation codes created by the specified user for the specified circle.
    /// </summary>
    Task<IList<InvitationCode>> GetCodesForCircleAsync(int circleId, int requestingUserId);

    /// <summary>
    /// Looks up an unused code string and returns the matching entity, or null if not found/expired/revoked.
    /// </summary>
    Task<InvitationCode?> FindValidCodeAsync(string code);

    /// <summary>
    /// Redeems the code for the specified user, adds them to the associated circle, and returns the membership.
    /// Throws InvalidOperationException if the code is already used, expired, or revoked.
    /// </summary>
    Task<CircleMembership> RedeemCodeAsync(string code, int newUserId);

    /// <summary>
    /// Revokes an unused code. Only the creator or any circle member may revoke.
    /// </summary>
    Task RevokeCodeAsync(int invitationCodeId, int requestingUserId);
}
