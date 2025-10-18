using Infrastructure.Data;
using Infrastructure.Interfaces;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Persistence.Repository.Base;

namespace Infrastructure.WorkUnit;

public class UnitOfWork(AppDbContext context) : IUnitOfWork
{
    private readonly Dictionary<Type, object> _repositories = new();
    private bool _disposed;
    private IDbContextTransaction _transaction;

    public IRepository<T>? Repository<T>() where T : class
    {
        if (_repositories.ContainsKey(typeof(T))) return _repositories[typeof(T)] as IRepository<T>;

        IRepository<T>? repo = new Repository<T>(context);
        _repositories.Add(typeof(T), repo);
        return repo;
    }

    public bool Save<T>() where T : class
    {
        var returnValue = true;
        try
        {
            var entries = context.ChangeTracker.Entries<T>()
                .Where(e => e.State == EntityState.Modified || e.State == EntityState.Added ||
                            e.State == EntityState.Deleted);

            foreach (var entry in entries) entry.State = entry.State;

            context.SaveChanges();
        }
        catch (Exception)
        {
            returnValue = false;
        }

        return returnValue;
    }

    public async Task<int> Complete()
    {
        return await context.SaveChangesAsync();
    }

    public async Task BeginTransactionAsync()
    {
        _transaction = await context.Database.BeginTransactionAsync();
    }

    public void Commit()
    {
        try
        {
            _transaction?.Commit();
        }
        finally
        {
            _transaction?.Dispose();
            _transaction = null;
        }
    }

    public async Task CommitAsync()
    {
        await context.SaveChangesAsync();
        if (_transaction != null)
            await _transaction.CommitAsync();
    }

    public void Rollback()
    {
        try
        {
            _transaction?.Rollback();
        }
        finally
        {
            _transaction?.Dispose();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            context.Dispose();
            _transaction?.Dispose();
        }

        _disposed = true;
    }
    public IExecutionStrategy CreateExecutionStrategy()
    {
        return context.Database.CreateExecutionStrategy();
    }
    public async Task CommitTransactionAsync()
    {
        try
        {
            await context.SaveChangesAsync();
            if (_transaction != null)
            {
                await _transaction.CommitAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }
        catch
        {
            await RollbackTransactionAsync(); // Si hay error, revierte cambios
            throw;
        }
    }
    public async Task RollbackTransactionAsync()
    {
        try
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }
        catch (Exception ex)
        {
            throw new Exception("Error al revertir la transacción.", ex);
        }
    }
}
