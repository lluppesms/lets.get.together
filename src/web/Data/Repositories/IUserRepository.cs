//-----------------------------------------------------------------------
// <copyright file="IUserRepository.cs" company="Luppes Consulting, Inc.">
// Copyright 2026, Luppes Consulting, Inc. All rights reserved.
// </copyright>
// <summary>
// User Repository Interface
// </summary>
//-----------------------------------------------------------------------
using DadABase.Data.Models;

namespace DadABase.Data.Repositories;

/// <summary>
/// Represents the repository interface for resolving and managing application users.
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Finds a user by their external identity provider subject identifier, or returns null.
    /// </summary>
    Task<User?> FindByExternalIdAsync(string externalId);

    /// <summary>
    /// Finds a user by their primary key, or returns null.
    /// </summary>
    Task<User?> GetByIdAsync(int userId);

    /// <summary>
    /// Creates a new user record. Called during first-time onboarding after invite redemption.
    /// Returns the persisted user.
    /// </summary>
    Task<User> CreateUserAsync(User user);

    /// <summary>
    /// Updates display name or email address for the specified user.
    /// </summary>
    Task UpdateUserAsync(User user);
}
