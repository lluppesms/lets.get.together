//-----------------------------------------------------------------------
// <copyright file="IUserRepository.cs" company="Luppes Consulting, Inc.">
// Copyright 2026, Luppes Consulting, Inc. All rights reserved.
// </copyright>
// <summary>
// User Repository Interface
// </summary>
//-----------------------------------------------------------------------
using GetTogether.Data.Models;

namespace GetTogether.Data.Repositories;

/// <summary>
/// Represents the repository interface for resolving and managing application users.
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Finds a user by their provider-qualified external identity, or returns null.
    /// </summary>
    Task<User?> FindByIdentityAsync(ExternalIdentityProvider provider, string issuer, string subject);

    /// <summary>
    /// Finds a user by their primary key, or returns null.
    /// </summary>
    Task<User?> GetByIdAsync(int userId);

    /// <summary>
    /// Creates a user with their first linked provider identity and email alias.
    /// </summary>
    Task<User> CreateUserAsync(User user, UserIdentity identity, UserEmailAlias emailAlias);

    /// <summary>
    /// Links a provider identity to an existing user.
    /// </summary>
    Task<UserIdentity> AddIdentityAsync(UserIdentity identity);

    /// <summary>
    /// Adds an email alias to an existing user.
    /// </summary>
    Task<UserEmailAlias> AddEmailAliasAsync(UserEmailAlias emailAlias);

    /// <summary>
    /// Updates the user profile fields.
    /// </summary>
    Task UpdateUserAsync(User user);
}
