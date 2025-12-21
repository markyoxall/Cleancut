using CleanCut.Infrastructure.Data.Entities;
using CleanCut.Infrastructure.Data.Context;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace CleanCut.WinApp.Services.Management
{
    /// <summary>
    /// Database-backed implementation of IUserPreferencesService.
    /// Demonstrates OCP: new behavior (DB storage) is added without changing ManagementLoader.
    /// </summary>
    public class DatabaseUserPreferencesService : IUserPreferencesService
    {
        private readonly CleanCutDbContext _dbContext;
        private readonly ILogger<DatabaseUserPreferencesService> _logger;

        public DatabaseUserPreferencesService(CleanCutDbContext dbContext, ILogger<DatabaseUserPreferencesService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<UserPreferences?> LoadPreferencesAsync(string moduleName, CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.UserPreferences.AsNoTracking().FirstOrDefaultAsync(u => u.ModuleName == moduleName, cancellationToken);
            if (entity == null) return null;

            try
            {
                return System.Text.Json.JsonSerializer.Deserialize<UserPreferences>(entity.PayloadJson);
            }
            catch (System.Exception ex)
            {
                _logger.LogWarning(ex, "Failed to deserialize preferences for {Module}", moduleName);
                return null;
            }
        }

        public async Task SavePreferencesAsync(string moduleName, UserPreferences preferences, string appUserName, CancellationToken cancellationToken = default)
        {
            var json = System.Text.Json.JsonSerializer.Serialize(preferences, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });

            var existing = await _dbContext.UserPreferences.FirstOrDefaultAsync(u => u.ModuleName == moduleName, cancellationToken);
            if (existing == null)
            {
                var entity = new UserPreferenceEntity
                {
                    ModuleName = moduleName,
                    PayloadJson = json,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _dbContext.UserPreferences.Add(entity);
            }
            else
            {
                existing.PayloadJson = json;
                existing.UpdatedAt = DateTime.UtcNow;
                _dbContext.UserPreferences.Update(existing);
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
