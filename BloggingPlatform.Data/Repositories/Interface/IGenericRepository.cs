using BloggingPlatform.Data.Entities;

namespace BloggingPlatform.Data.Repositories.Interface;

public interface IGenericRepository<TEntity> where TEntity : BaseEntity
{
    IQueryable<TEntity> AsQueryable();
    Task<TEntity?> GetByIdAsync(string id);
    Task<bool> AddAsync(TEntity entity);
    Task<bool> UpdateAsync(TEntity entity);
    Task<bool> SoftDeleteAsync(string id, string? userId);
    Task<bool> HardDeleteAsync(string id);
    Task<bool> BulkInsertAsync(List<TEntity> entities);
    Task<List<TEntity>> AsQueryableAsync();
}