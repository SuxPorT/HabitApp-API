using HabitApp.Domain.Entities;
using HabitApp.Domain.Services.Interfaces;
using HabitApp.Infrastructure.Data.Interfaces;

namespace HabitApp.Domain.Services;

public abstract class BaseService<TEntity>(IBaseRepository<TEntity> repository) : IBaseService<TEntity> where TEntity : BaseEntity 
{
    protected readonly IBaseRepository<TEntity> _repository = repository;

    public async Task<IEnumerable<TEntity>> GetAllAsync()
        => await _repository.GetAllAsync();

    public async Task<TEntity?> GetByIdAsync(int id)
        => await _repository.GetByIdAsync(id);

    public async Task<TEntity> AddAsync(TEntity entity)
        => await _repository.AddAsync(entity);

    public async Task<TEntity> UpdateAsync(TEntity entity)
        => await _repository.UpdateAsync(entity);

    public async Task<bool> DeleteAsync(int id)
        => await _repository.DeleteAsync(id);
}
