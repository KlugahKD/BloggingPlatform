using System.Linq.Expressions;
using BloggingPlatform.Data.Entities;
using BloggingPlatform.Data.Repositories.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BloggingPlatform.Data.Repositories.Providers;

public class GenericRepository<TEntity>(BloggingPlatformDbContext dbDbContext, ILogger<GenericRepository<TEntity>> logger)
    : IGenericRepository<TEntity>
    where TEntity : BaseEntity
{
    private readonly DbSet<TEntity> _dbSet = dbDbContext.Set<TEntity>();

    public  IQueryable<TEntity> AsQueryable()
    {
        try
        {
            logger.LogInformation("Fetching entities as queryable.");
        
            return _dbSet.AsQueryable();

        }
        catch (Exception e)
        {
            logger.LogError(e, "Error occurred while fetching entities as queryable.");

            return Enumerable.Empty<TEntity>().AsQueryable();
        }
        
    }

    public async Task<List<TEntity>> AsQueryableAsync()
    {
        try
        {
            logger.LogInformation("Fetching entities as queryable.");

            return await _dbSet.ToListAsync();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error occurred while fetching entities as queryable.");

            return new List<TEntity>();
        }
    }

    public async Task<TEntity?> GetByIdAsync(string id)
    {
        try
        {
            logger.LogInformation("Fetching entity by id: {Id}", id);
            
            return await _dbSet.FindAsync(id);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error occurred while fetching entity by id: {Id}", id);

            return default;
        }   
    }
    
    
    public async Task<bool> AddAsync(TEntity entity)
    {
        try
        {
            logger.LogInformation("Adding entity to database.");
            
            await _dbSet.AddAsync(entity);

            return await dbDbContext.SaveChangesAsync() > 0;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error occurred while adding entity to database.");

            return false;
        }
    }

    public async Task<bool> UpdateAsync(TEntity entity)
    {
        try
        {
            logger.LogInformation("Updating entity in database.");
            
            dbDbContext.Entry(entity).State = EntityState.Modified;

            return await dbDbContext.SaveChangesAsync() > 0;

        }
        catch (Exception e)
        {
            logger.LogError(e, "Error occurred while updating entity in database.");

            return false;
        }
    }

    public async Task<bool> SoftDeleteAsync(string id)
    {
        try
        {
            logger.LogInformation("Soft deleting entity by id: {Id}", id);
            
            var entity = await GetByIdAsync(id);

            if (entity != null)
            {
                entity.IsActive = false;
                entity.IsDeleted = true;
                entity.DeletedDate = DateTime.UtcNow;
                entity.UpdatedAt = DateTime.Now;

                _dbSet.Update(entity);

                return await dbDbContext.SaveChangesAsync() > 0;
            }

            logger.LogDebug("Entity not found with id: {Id}", id);
            return false;

        }
        catch (Exception e)
        {
            logger.LogError(e, "Error occurred while soft deleting entity by id: {Id}", id);

            return false;
        }
    }

    public async Task<bool> HardDeleteAsync(string id)
    {
        try
        {
            logger.LogInformation("Hard deleting entity by id: {Id}", id);
            
            var entity = await GetByIdAsync(id);

            if (entity != null)
            {
                _dbSet.Remove(entity);
                return await dbDbContext.SaveChangesAsync() > 0;
            }

            logger.LogDebug("Entity not found with id: {Id}", id);
            return false;

        }
        catch (Exception e)
        {
            logger.LogError(e, "Error occurred while hard deleting entity by id: {Id}", id);

            return false;
        }
    }
    

    public async Task<bool> BulkInsertAsync(List<TEntity> entities)
    {
        try
        {
            logger.LogInformation("Bulk inserting entities to database.");
            
            await _dbSet.AddRangeAsync(entities);

            return await dbDbContext.SaveChangesAsync() > 0;

        }
        catch (Exception e)
        {
            logger.LogError(e, "Error occurred while bulk inserting entities to database.");

            return false;
        }
       
    }

    public async Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate)
    {
        try
        {
            logger.LogInformation("Checking if entity exists in database.");

            return await _dbSet
                .AsNoTracking()
                .AnyAsync(predicate);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error occurred while checking if entity exists in database.");

            return false;

        }
    }

    public Task<TEntity?> FindAsync(Expression<Func<TEntity, bool>> predicate)
    {
        try
        {
            logger.LogInformation("Finding entity in database.");

            return _dbSet
                .AsNoTracking()
                .FirstOrDefaultAsync(predicate);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error occurred while finding entity in database.");

            return Task.FromResult(default(TEntity));
        }
    }
}