//-----------------------------------------------------------------------
// <copyright file="UserSQLRepository.cs" company="Luppes Consulting, Inc.">
// Copyright 2026, Luppes Consulting, Inc. All rights reserved.
// </copyright>
// <summary>
// User SQL Repository
// </summary>
//-----------------------------------------------------------------------
using GetTogether.Data.Models;

namespace GetTogether.Data.Repositories;

/// <summary>
/// SQL Server implementation of <see cref="IUserRepository"/> using EF Core.
/// </summary>
public class UserSQLRepository(GetTogetherDbContext context) : IUserRepository
{
    private readonly GetTogetherDbContext _context = context;

    /// <inheritdoc/>
    public async Task<User?> FindByExternalIdAsync(string externalId)
    {
        return await _context.Users!.FirstOrDefaultAsync(u => u.ExternalId == externalId);
    }

    /// <inheritdoc/>
    public async Task<User?> GetByIdAsync(int userId)
    {
        return await _context.Users!.FindAsync(userId);
    }

    /// <inheritdoc/>
    public async Task<User> CreateUserAsync(User user)
    {
        user.CreatedUtc = DateTime.UtcNow;
        user.IsActive = true;
        _context.Users!.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    /// <inheritdoc/>
    public async Task UpdateUserAsync(User user)
    {
        _context.Users!.Update(user);
        await _context.SaveChangesAsync();
    }
}
