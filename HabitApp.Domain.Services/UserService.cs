using HabitApp.Domain.Entities;
using HabitApp.Domain.Services.Interfaces;
using HabitApp.Infrastructure.Data.Interfaces;

namespace HabitApp.Domain.Services;

public class UserService(IUserRepository repository) : IUserService
{
    private readonly IUserRepository _repository = repository;

    public async Task<User?> AuthenticateAsync(string email, string password)
    {
        var user = await _repository.GetByEmailAsync(email);
        if (user == null || user.Password != password) return null;
        return user;
    }

    public async Task<IEnumerable<User>> GetAllAsync()
        => await _repository.GetAllAsync();

    public async Task<User?> GetByIdAsync(int id)
        => await _repository.GetByIdAsync(id);

    public async Task<User> AddAsync(User user)
        => await _repository.AddAsync(user);

    public async Task<User> UpdateAsync(User user)
        => await _repository.UpdateAsync(user);

    public async Task<bool> DeleteAsync(int id)
        => await _repository.DeleteAsync(id);
}
