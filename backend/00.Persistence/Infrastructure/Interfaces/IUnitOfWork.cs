using Microsoft.EntityFrameworkCore.Storage;
using Persistence.Repository.Base;

namespace Infrastructure.Interfaces;

/// <summary>
///     Interface for the Unit of Work pattern, providing methods to manage transactions
///     and repositories within a single database context.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    /// <summary>
    ///     Starts a database transaction asynchronously.
    /// </summary>
    Task BeginTransactionAsync();

    /// <summary>
    ///     Commits the current transaction.
    /// </summary>
    void Commit();

    /// <summary>
    ///     Commits the current transaction asynchronously.
    /// </summary>
    Task CommitAsync();

    /// <summary>
    ///     Saves all changes made in the context to the database.
    /// </summary>
    /// <returns>The number of state entries written to the database.</returns>
    Task<int> Complete();

    /// <summary>
    ///     Gets the repository for the specified entity type.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <returns>An instance of IRepository for the specified entity type.</returns>
    IRepository<TEntity>? Repository<TEntity>() where TEntity : class;

    /// <summary>
    ///     Rolls back the current transaction.
    /// </summary>
    void Rollback();

    // Consider removing this if it overlaps with Complete()
    /// <summary>
    ///     Saves changes to a specific entity type.
    /// </summary>
    /// <typeparam name="T">The type of the entity.</typeparam>
    /// <returns>True if the changes were saved successfully; otherwise, false.</returns>
    bool Save<T>() where T : class;

    IExecutionStrategy CreateExecutionStrategy();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}
