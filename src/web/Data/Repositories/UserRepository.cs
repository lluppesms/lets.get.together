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
public class UserRepository(GetTogetherDbContext context) : IUserRepository
{
    private readonly GetTogetherDbContext _context = context;

    /// <inheritdoc/>
    public async Task<User?> FindByIdentityAsync(ExternalIdentityProvider provider, string issuer, string subject)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(issuer);
        ArgumentException.ThrowIfNullOrWhiteSpace(subject);

        return await _context.Users!
            .Include(user => user.EmailAliases)
            .SingleOrDefaultAsync(user => user.Identities.Any(identity =>
                identity.Provider == provider && identity.Issuer == issuer && identity.Subject == subject));
    }

    /// <inheritdoc/>
    public async Task<User?> GetByIdAsync(int userId)
    {
        return await _context.Users!
            .Include(user => user.EmailAliases)
            .SingleOrDefaultAsync(user => user.UserId == userId);
    }

    /// <inheritdoc/>
    public async Task<User> CreateUserAsync(User user, UserIdentity identity, UserEmailAlias emailAlias)
    {
        ArgumentNullException.ThrowIfNull(user);
        ArgumentNullException.ThrowIfNull(identity);
        ArgumentNullException.ThrowIfNull(emailAlias);

        user.CreatedUtc = DateTime.UtcNow;
        user.IsActive = true;
        identity.CreatedUtc = DateTime.UtcNow;
        emailAlias.CreatedUtc = DateTime.UtcNow;
        _context.Users!.Add(user);
        user.Identities.Add(identity);
        user.EmailAliases.Add(emailAlias);
        await _context.SaveChangesAsync();
        return user;
    }

    /// <inheritdoc/>
    public async Task<UserIdentity> AddIdentityAsync(UserIdentity identity)
    {
        ArgumentNullException.ThrowIfNull(identity);
        identity.CreatedUtc = DateTime.UtcNow;
        _context.UserIdentities!.Add(identity);
        await _context.SaveChangesAsync();
        return identity;
    }

    /// <inheritdoc/>
    public async Task<UserEmailAlias> AddEmailAliasAsync(UserEmailAlias emailAlias)
    {
        ArgumentNullException.ThrowIfNull(emailAlias);
        emailAlias.CreatedUtc = DateTime.UtcNow;
        _context.UserEmailAliases!.Add(emailAlias);
        await _context.SaveChangesAsync();
        return emailAlias;
    }

    /// <inheritdoc/>
    public async Task UpdateUserAsync(User user)
    {
        _context.Users!.Update(user);
        await _context.SaveChangesAsync();
    }
}
