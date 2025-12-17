using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CleanCut.BlazorSPA.Pages.State;

/// <summary>
/// Generic CRUD state service interface for entities with a Guid key.
/// </summary>
public interface IStateService<T>
{
    Task<List<T>> GetAllAsync();
    Task<T?> GetByIdAsync(Guid id);
    Task CreateAsync(T entity);
    Task UpdateAsync(T entity);
    Task<bool> DeleteAsync(Guid id);
}
